using Contracts.FeatureFlags;
using ExampleService.Domain.FeatureFlags;
using ExampleService.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExampleService.Api.FeatureFlags;

[ApiController]
[Route("api/feature-flags")]
public class FeatureFlagsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<FeatureFlagsController> _logger;

    public FeatureFlagsController(AppDbContext dbContext, ILogger<FeatureFlagsController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<FeatureFlagResponse>> CreateFeatureFlag(
        [FromBody] CreateFeatureFlagRequest request,
        CancellationToken cancellationToken = default)
    {
        // Check for duplicate key
        if (await _dbContext.FeatureFlags.AnyAsync(f => f.Key == request.Key, cancellationToken))
        {
            _logger.LogWarning("Attempted to create feature flag with duplicate key: {Key}", request.Key);
            return Conflict();
        }

        var featureFlag = new FeatureFlag
        {
            Id = Guid.NewGuid(),
            Key = request.Key,
            Description = request.Description,
            Enabled = request.Enabled,
            RolloutPercentage = request.RolloutPercentage
        };

        _dbContext.FeatureFlags.Add(featureFlag);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created feature flag with ID: {Id}, Key: {Key}", featureFlag.Id, featureFlag.Key);

        var response = MapToResponse(featureFlag);
        return CreatedAtAction(
            nameof(GetFeatureFlagById),
            new { id = featureFlag.Id },
            response);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FeatureFlagResponse>>> GetAllFeatureFlags(
        CancellationToken cancellationToken = default)
    {
        var featureFlags = await _dbContext.FeatureFlags
            .Select(f => MapToResponse(f))
            .ToListAsync(cancellationToken);

        return Ok(featureFlags);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FeatureFlagResponse>> GetFeatureFlagById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var featureFlag = await _dbContext.FeatureFlags.FindAsync([id], cancellationToken);
        if (featureFlag == null)
        {
            _logger.LogWarning("Feature flag not found with ID: {Id}", id);
            return NotFound();
        }

        return Ok(MapToResponse(featureFlag));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<FeatureFlagResponse>> UpdateFeatureFlag(
        Guid id,
        [FromBody] UpdateFeatureFlagRequest request,
        CancellationToken cancellationToken = default)
    {
        var featureFlag = await _dbContext.FeatureFlags.FindAsync([id], cancellationToken);
        if (featureFlag == null)
        {
            _logger.LogWarning("Attempted to update non-existent feature flag with ID: {Id}", id);
            return NotFound();
        }

        // Check for duplicate key if key is being changed
        if (featureFlag.Key != request.Key &&
            await _dbContext.FeatureFlags.AnyAsync(f => f.Key == request.Key && f.Id != id, cancellationToken))
        {
            _logger.LogWarning("Attempted to update feature flag {Id} with duplicate key: {Key}", id, request.Key);
            return Conflict();
        }

        featureFlag.Key = request.Key;
        featureFlag.Description = request.Description;
        featureFlag.Enabled = request.Enabled;
        featureFlag.RolloutPercentage = request.RolloutPercentage;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated feature flag with ID: {Id}", id);

        return Ok(MapToResponse(featureFlag));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteFeatureFlag(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var featureFlag = await _dbContext.FeatureFlags.FindAsync([id], cancellationToken);
        if (featureFlag == null)
        {
            _logger.LogWarning("Attempted to delete non-existent feature flag with ID: {Id}", id);
            return NotFound();
        }

        _dbContext.FeatureFlags.Remove(featureFlag);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted feature flag with ID: {Id}, Key: {Key}", id, featureFlag.Key);

        return NoContent();
    }

    private static FeatureFlagResponse MapToResponse(FeatureFlag featureFlag)
    {
        return new FeatureFlagResponse
        {
            Id = featureFlag.Id,
            Key = featureFlag.Key,
            Description = featureFlag.Description,
            Enabled = featureFlag.Enabled,
            RolloutPercentage = featureFlag.RolloutPercentage
        };
    }
}

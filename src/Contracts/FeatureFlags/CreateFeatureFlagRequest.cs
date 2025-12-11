using System.ComponentModel.DataAnnotations;

namespace Contracts.FeatureFlags;

public class CreateFeatureFlagRequest
{
    [Required]
    [MaxLength(100)]
    public required string Key { get; set; }

    public string? Description { get; set; }

    public bool Enabled { get; set; }

    [Range(0, 100)]
    public int? RolloutPercentage { get; set; }
}


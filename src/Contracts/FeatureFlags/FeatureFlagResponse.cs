namespace Contracts.FeatureFlags;

public class FeatureFlagResponse
{
    public Guid Id { get; set; }

    public required string Key { get; set; }

    public string? Description { get; set; }

    public bool Enabled { get; set; }

    public int? RolloutPercentage { get; set; }
}


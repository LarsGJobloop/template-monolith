using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ExampleService.Domain.FeatureFlags;

[Index(nameof(Key), IsUnique = true)]
public class FeatureFlag
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Key { get; set; } = null!;

    public string? Description { get; set; }

    public bool Enabled { get; set; }

    [Range(0, 100)]
    public int? RolloutPercentage { get; set; }
}

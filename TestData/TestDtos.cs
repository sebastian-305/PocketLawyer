namespace PocketLawyer.TestData.Dtos;

/// <summary>
/// Base DTO for testing inheritance
/// </summary>
public class BaseDto
{
    /// <summary>
    /// Base identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Optional description
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Derived DTO for testing property inheritance
/// </summary>
public class DerivedDto : BaseDto
{
    /// <summary>
    /// Derived name property
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional email
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Required status
    /// </summary>
    public bool IsActive { get; set; }
}

/// <summary>
/// Multi-level inheritance DTO
/// </summary>
public class MultiLevelDto : DerivedDto
{
    /// <summary>
    /// Additional property at third level
    /// </summary>
    public int Score { get; set; }
}

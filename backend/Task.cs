using System.ComponentModel.DataAnnotations;

namespace Backend;

public class TaskItem
{
    [Key]
    public string Id { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public bool Completed { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class MetaEntry
{
    [Key]
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

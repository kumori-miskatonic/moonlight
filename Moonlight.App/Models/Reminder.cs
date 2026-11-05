using SQLite;

namespace Moonlight.App.Models;
public sealed class Reminder
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    [MaxLength(300)]
    public required string Text { get; set; } = string.Empty;
    public DateTime Due { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsImportant { get; set; }

}
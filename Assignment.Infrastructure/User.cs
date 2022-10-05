namespace Assignment.Infrastructure;

public class User
{
    public int Id { get; set; }

    [StringLength(100)]
    public string? Name { get; set; }
    [EmailAddress, StringLength(100)]
    public string? Email { get; set; }

    public IList<WorkItem>? WorkItems { get; set; }
}

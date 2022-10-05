namespace Assignment.Infrastructure;
using Assignment.Core;

public class WorkItem
{

    public int Id { get; set; }
    [StringLength(100)]
    public string? Title { get; set; }


    public User? AssignedTo { get; set; }
    public string? Description { get; set; }

    public State State { get; set; }


    public ICollection<Tag> Tags { get; set; } = new List<Tag>();


}

namespace Assignment.Infrastructure;

public class WorkItem
{
    public virtual int Id {get; set;}
    
    [Required]
    [StringLength(100)]
    public virtual string Title {get; set;}

    public virtual User? AssignedTo {get; set;}

    public virtual string? Description {get; set;}
    [Required]
    public virtual State State {get; set;}

    public virtual ICollection<Tag> Tags {get; set;}

    public virtual DateTime Created {get; set;}

    public virtual DateTime StateUpdated {get; set;}

    public WorkItem(string title)
    {
        Title = title;
        Tags = new HashSet<Tag>();
    }
}

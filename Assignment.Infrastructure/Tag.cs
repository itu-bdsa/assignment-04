namespace Assignment.Infrastructure;

public class Tag
{
    public virtual int Id {get; set;}
    [Required]
    [StringLength(50)]
    public virtual string Name{get; set;}//Should be unique, how do we enforce that
    public virtual ICollection<WorkItem> WorkItems{get; set;}

    public Tag(String name)
    {
        WorkItems = new HashSet<WorkItem>();
        Name = name;
    }
}

using System.Collections.ObjectModel;

namespace Assignment.Infrastructure;

public class WorkItemRepository : IWorkItemRepository
{
    private readonly KanbanContext _context;

    public (Response Response, int ItemId) Create(WorkItemCreateDTO workItem)
    {
        Response response;
        var entity = new WorkItem(workItem.Title);
        entity.Created = DateTime.UtcNow;
        entity.StateUpdated = DateTime.UtcNow;

        var entry = _context.Items.Add(entity);
        _context.SaveChanges();

        response = Created;        

        return (response, entry.Entity.Id);
    }
    public Response Delete(int ItemId)
    {
        var entity = _context.Items.FirstOrDefault(workItem => workItem.Id == ItemId);
        if(entity == null) return NotFound;
        Response response;
        switch(entity.State){
            case New:
                response = Deleted;
                _context.Items.Remove(entity);
                break;
            case Active:
                entity.State = Removed;
                entity.StateUpdated = DateTime.UtcNow;
                response = Deleted;
                break;
            default:
                response = Conflict;
            break;
        }
        _context.SaveChanges();
        return response;
    }

    public WorkItemDetailsDTO Find(int itemId)
    {
        var entity = _context.Items.FirstOrDefault(workItem => workItem.Id == itemId);
        if(entity == null) return null!;
        var tags = new ReadOnlyCollection<string>(entity.Tags.Select(t => t.Name).ToList());
        return new WorkItemDetailsDTO(itemId, entity.Title, entity.Description!, entity.Created, entity.AssignedTo?.Name!, tags, entity.State, entity.StateUpdated);
    }

    public IReadOnlyCollection<WorkItemDTO> Read() => new ReadOnlyCollection<WorkItemDTO>(_context.Items.Select(t => new WorkItemDTO(t.Id, t.Title, t.AssignedTo!.Name, new ReadOnlyCollection<string>(t.Tags.Select(ta => ta.Name).ToList()),t.State)).ToList());

    public IReadOnlyCollection<WorkItemDTO> ReadByState(State state) => new ReadOnlyCollection<WorkItemDTO>(_context.Items.Where(t => t.State == state).Select(t => new WorkItemDTO(t.Id, t.Title, t.AssignedTo!.Name, new ReadOnlyCollection<string>(t.Tags.Select(ta => ta.Name).ToList()),t.State)).ToList());

    public IReadOnlyCollection<WorkItemDTO> ReadByTag(string tag) => new ReadOnlyCollection<WorkItemDTO>(_context.Items.Where(t => t.Tags.Select(tag => tag.Name).Contains(tag)).Select(t => new WorkItemDTO(t.Id, t.Title, t.AssignedTo!.Name, new ReadOnlyCollection<string>(t.Tags.Select(ta => ta.Name).ToList()),t.State)).ToList());

    public IReadOnlyCollection<WorkItemDTO> ReadByUser(int userId) => new ReadOnlyCollection<WorkItemDTO>(_context.Items.Where(t => t.AssignedTo!.Id == userId).Select(t => new WorkItemDTO(t.Id, t.Title, t.AssignedTo!.Name, new ReadOnlyCollection<string>(t.Tags.Select(ta => ta.Name).ToList()),t.State)).ToList());

    public IReadOnlyCollection<WorkItemDTO> ReadRemoved() => new ReadOnlyCollection<WorkItemDTO>(_context.Items.Where(t => t.State == Removed).Select(t => new WorkItemDTO(t.Id, t.Title, t.AssignedTo!.Name, new ReadOnlyCollection<string>(t.Tags.Select(ta => ta.Name).ToList()),t.State)).ToList());

    public Response Update(WorkItemUpdateDTO workItem)
    {
        var entity = _context.Items.FirstOrDefault(t => t.Id == workItem.Id);
        if(entity == null) return NotFound;
        var assignedToUser = _context.Users.FirstOrDefault(user => user.Id == workItem.AssignedToId);
        if(workItem.AssignedToId != null && assignedToUser == null) return BadRequest;

        entity.AssignedTo = assignedToUser;
        entity.Description = workItem.Description;
        entity.Title = workItem.Title;
        entity.Tags = _context.Tags.Where(tag => workItem.Tags.Contains(tag.Name)).ToList();
        if(entity.State != workItem.State) {
            entity.State = workItem.State;
            entity.StateUpdated = DateTime.UtcNow;
        }

        _context.SaveChanges();

        return Updated;
    }

        public WorkItemRepository(KanbanContext context)
    {
        _context = context;
    }
}

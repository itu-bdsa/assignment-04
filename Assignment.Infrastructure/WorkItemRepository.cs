namespace Assignment.Infrastructure;

public class WorkItemRepository : IWorkItemRepository
{
    private readonly KanbanContext _context;

    public WorkItemRepository(KanbanContext context)
    {
        _context = context;
    }
    
    public (Response Response, int ItemId) Create(WorkItemCreateDTO item)
    {
        Response response;
        var entity = new WorkItem(item.Title);
        var assignedUser = _context.Users.FirstOrDefault(u => u.Id == item.AssignedToId);

        if(assignedUser == null && item.AssignedToId != null) return (Response.BadRequest, 0);

        var tagsDB = _context.Tags.ToList();
        ICollection<Tag> tags = item.Tags.Count != 0 ? tagsDB
            .Where(tag => item.Tags.Contains(tag.Name))
            .ToList() 
            : new List<Tag>();
        
        entity.Description = item.Description!;
        entity.State = State.New;
        entity.AssignedTo = assignedUser;
        entity.Tags = tags;

        if(assignedUser != null) entity.State = State.Active;

        var taskExists = _context.Items.FirstOrDefault(t => t.Id == entity.Id) != null;
        
        if(taskExists)
        {
            return (Response.Conflict, 0);
        }

        entity.Created = DateTime.UtcNow;
        entity.StateUpdated = DateTime.UtcNow;

        _context.Items.Add(entity);
        _context.SaveChanges();

        response = Response.Created;
        

        return (response, entity.Id);
    }

    public IReadOnlyCollection<WorkItemDTO> ReadRemoved()
    {
        return ReadByState(State.Removed);
    }

    public IReadOnlyCollection<WorkItemDTO> ReadByTag(string tag)
    {
        var tagQuery = from t in Read()
            where t.Tags.Contains(tag)
            select t ;

        return tagQuery.Any() ? tagQuery.ToList() : null!;
    }

    public IReadOnlyCollection<WorkItemDTO> ReadByUser(int userId)
    {
        var userQuery = from t in Read() where t.Id == userId select t;
        
        return userQuery.Any() ? userQuery.ToList() : null!;
    }

    public IReadOnlyCollection<WorkItemDTO> ReadByState(State state)
    {
        var stateQuery = from t in Read() where t.State == state select t;
        
        return stateQuery.Any() ? stateQuery.ToList() : null!;
    }
    
    public IReadOnlyCollection<WorkItemDTO> Read()
    {
        if(!_context.Items.Any())
        {
            return null!;
        }


        var tasks = from t in _context.Items
            select new WorkItemDTO(t.Id, t.Title, t.AssignedTo!.Name, t.Tags.Select(x => x.Name).ToList(), t.State);

        return tasks.ToList();
    }

    public Response Update(WorkItemUpdateDTO workitem)
    {
        
        var entity = _context.Items.Find(workitem.Id);
        
        if(entity == null) return Response.NotFound;
        
        var assignedUser = _context.Users.FirstOrDefault(u => u.Id == workitem.AssignedToId);

        if(assignedUser == null) return Response.BadRequest;

        entity.Title = workitem.Title;
        entity.Description = workitem.Description!;
        entity.AssignedTo = assignedUser;
        entity.Tags = workitem.Tags.Select(name => new Tag(name))
            .ToList();
        if(entity.State != workitem.State) entity.StateUpdated = DateTime.UtcNow;
        
        entity.State = workitem.State;
        
        _context.SaveChanges();
        
        return Response.Updated;
    }

    public Response Delete(int workItemId)
    {
        var workitem = _context.Items.FirstOrDefault(u => u.Id == workItemId);
        
        if(workitem == null)
        {
            return Response.NotFound;
        }
        
        if(workitem.State == State.New) 
        {
            _context.Items.Remove(workitem);
        }
        else if (workitem.State == State.Active) 
        {
            workitem.State = State.Removed;
            workitem.StateUpdated = DateTime.UtcNow;
        }
        else return Response.Conflict;
        _context.SaveChanges();
        return Response.Deleted;
    }

    public WorkItemDetailsDTO? Find(int workItemId)
    {
        var taskNotExists = _context.Items.FirstOrDefault(t => t.Id == workItemId) == null;

        if (taskNotExists)
        {
            return null;
        }
        
        var workitem = from t in _context.Items where t.Id == workItemId 
        select new WorkItemDetailsDTO(t.Id, t.Title, t.Description, t.Created, t.AssignedTo!.Name, t.Tags.Select(x => x.Name).ToList(), t.State, t.StateUpdated);
        return workitem.First();
    }
}
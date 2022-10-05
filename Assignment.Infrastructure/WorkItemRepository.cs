namespace Assignment.Infrastructure;

public class WorkItemRepository : IWorkItemRepository
{
  private readonly KanbanContext _context;

  public WorkItemRepository(KanbanContext context)
  {
    _context = context;
  }
  public (Response Response, int WorkItemId) Create(WorkItemCreateDTO workItem)
  {
    var entity = new WorkItem
    {
      Title = workItem.Title,
      Description = workItem.Description,
      State = State.New,
      Tags = _context.Tags.Select(t => t).Where(t => workItem.Tags.Contains(t.Name!)).ToList()
    };

    _context.WorkItems.Add(entity);
    _context.SaveChanges();
    var response = Response.Created;
    return (response, entity.Id);
  }

  public Response Delete(int workItemId)
  {
    var workItem = _context.WorkItems.FirstOrDefault(w => w.Id == workItemId);
    Response response;

    if (workItem is null)
    {
      response = Response.NotFound;
      return response;
    }

    switch (workItem.State)
    {
      case State.New:
        _context.WorkItems.Remove(workItem);
        response = Response.Deleted;
        break;
      case State.Active:
        workItem.State = State.Removed;
        response = Response.Updated;
        break;
      case State.Resolved:
      case State.Closed:
      case State.Removed:
        response = Response.Conflict;
        break;
      default:
        response = Response.BadRequest;
        break;
    }
    _context.SaveChanges();
    return response;
  }

  public WorkItemDetailsDTO Find(int workItemId)
  {
    var workItem = _context.WorkItems.FirstOrDefault(w => w.Id == workItemId);

    if (workItem is null)
    {
      return null!;
    }
    else
    {
      return new WorkItemDetailsDTO(workItemId, workItem.Title!, workItem.Description!, DateTime.Today, workItem.AssignedTo!.Name!, workItem.Tags.Select(t => t.Name).ToArray()!, workItem.State, DateTime.Now);
    }

  }

  public IReadOnlyCollection<WorkItemDTO> Read()
  {
    var workItems = from w in _context.WorkItems
                    orderby w.State
                    select new WorkItemDTO(w.Id, w.Title!, w.AssignedTo!.Name!, w.Tags.Select(t => t.Name).ToArray()!, w.State);

    return workItems.ToArray();
  }

  public IReadOnlyCollection<WorkItemDTO> ReadByState(State state)
  {
    var workItems = from w in _context.WorkItems
                    where w.State == state
                    select new WorkItemDTO(w.Id, w.Title!, w.AssignedTo!.Name!, w.Tags.Select(t => t.Name).ToArray()!, w.State);

    return workItems.ToArray();
  }

  public IReadOnlyCollection<WorkItemDTO> ReadByTag(string tag)
  {
    var workItems = from w in _context.WorkItems
                    where w.Tags.Select(t => t.Name).Contains(tag)
                    orderby w.State
                    select new WorkItemDTO(w.Id, w.Title!, w.AssignedTo!.Name!, w.Tags.Select(t => t.Name).ToArray()!, w.State);

    return workItems.ToArray();
  }

  public IReadOnlyCollection<WorkItemDTO> ReadByUser(int userId)
  {
    var workItems = from w in _context.WorkItems
                    where w.AssignedTo!.Id == userId
                    orderby w.State
                    select new WorkItemDTO(w.Id, w.Title!, w.AssignedTo!.Name!, w.Tags.Select(t => t.Name).ToArray()!, w.State);

    return workItems.ToArray();
  }

  public IReadOnlyCollection<WorkItemDTO> ReadRemoved()
  {
    var workItems = from w in _context.WorkItems
                    where w.State == State.Removed
                    select new WorkItemDTO(w.Id, w.Title!, w.AssignedTo!.Name!, w.Tags.Select(t => t.Name).ToArray()!, w.State);

    return workItems.ToArray();
  }

  public Response Update(WorkItemUpdateDTO workItem)
  {
    Response response;
    var entity = _context.WorkItems.Find(workItem.Id);

    if (entity is null)
    {
      response = Response.NotFound;
      return response;
    }
    else
    {
      entity.Title = workItem.Title;
      entity.Description = workItem.Description ?? entity.Description;
      entity.Tags = _context.Tags.Select(t => t).Where(t => workItem.Tags.Contains(t.Name!)).ToList() ?? entity.Tags;
      if (workItem is not null)
      {
        entity.State = workItem.State;
      }
      response = Response.Updated;
      _context.SaveChanges();
    }
    return response;
  }
}

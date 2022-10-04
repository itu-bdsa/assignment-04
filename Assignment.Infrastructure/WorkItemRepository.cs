using Assignment.Core;
using Microsoft.EntityFrameworkCore;
namespace Assignment.Infrastructure;


public sealed class WorkItemRepository : IWorkItemRepository
{   
     private readonly KanbanContext _context;

    public WorkItemRepository(KanbanContext context)
    {
        _context = context;
    }
    public (Response Response, int ItemId) Create(WorkItemCreateDTO item) {
        
        Response response;

        
          var entity = new WorkItem(item.Title);
            entity.Created = DateTime.UtcNow;
            entity.Title = item.Title;
           
            entity.State = State.New;
            entity.StateUpdated = DateTime.UtcNow;
            var tagList = new List<Tag>();
            foreach (string s in item.Tags){
                if((from t in _context.Tags where t.Name== s select t).FirstOrDefault()!= null){
                    tagList.Add((from t in _context.Tags where t.Name== s select t).FirstOrDefault());
                }
            }
            entity.Tags = tagList;



            if((from u in _context.Users where u.Id == item.AssignedToId select u).FirstOrDefault()==null){
                response = Response.BadRequest;
            }
            else {
                entity.AssignedTo = (from u in _context.Users where u.Id == item.AssignedToId select u).FirstOrDefault()!;
                entity.AssignedToId = item.AssignedToId;
                 response = Response.Created;
            }

            
            _context.Items.Add(entity);
            _context.SaveChanges();
    
        var created = new WorkItemDTO(entity.Id, entity.Title, entity.AssignedTo.Name, entity.Tags.Select(t=>t.Name).ToList().AsReadOnly(), entity.State);
        
        return (response, created.Id);
    }
    public IReadOnlyCollection<WorkItemDTO> Read() {
         var tasks = from t in _context.Items
                    orderby t.Title
                    select new WorkItemDTO(t.Id, t.Title, t.AssignedTo.Name, t.Tags.Select(t=>t.Name).ToList().AsReadOnly(), t.State);

        if (tasks.Any()){
            return tasks.ToArray()!;
        }
        else {
            return null!;
        }     

    }
    public IReadOnlyCollection<WorkItemDTO> ReadRemoved() {
        var tasks = from t in _context.Items
                    orderby t.Title
                    where t.State == State.Removed
                    select new WorkItemDTO(t.Id, t.Title, t.AssignedTo.Name, t.Tags.Select(t=>t.Name).ToList().AsReadOnly(), t.State);

        if (tasks.Any()){
            return tasks.ToArray()!;
        }
        else {
            return null!;
        }    

    }
    public IReadOnlyCollection<WorkItemDTO> ReadByTag(string tag) {
        var tasks = from t in _context.Items
                    orderby t.Title
                    where t.Tags.Select(t=>t.Name).ToList().AsReadOnly().Contains(tag)
                    select new WorkItemDTO(t.Id, t.Title, t.AssignedTo.Name, t.Tags.Select(t=>t.Name).ToList().AsReadOnly(), t.State);

         if (tasks.Any()){
            return tasks.ToArray()!;
        }
        else {
            return null!;
        }    

    }
    public IReadOnlyCollection<WorkItemDTO> ReadByUser(int userId) {
        var tasks = from t in _context.Items
                    orderby t.Title
                    where t.AssignedTo.Id == userId 
                    select new WorkItemDTO(t.Id, t.Title, t.AssignedTo.Name, t.Tags.Select(t=>t.Name).ToList().AsReadOnly(), t.State);

         if (tasks.Any()){
            return tasks.ToArray()!;
        }
        else {
            return null!;
        }  

    }
    public IReadOnlyCollection<WorkItemDTO> ReadByState(State state) {
        var tasks = from t in _context.Items
                    orderby t.Title
                    where t.State == state 
                    select new WorkItemDTO(t.Id, t.Title, t.AssignedTo.Name, t.Tags.Select(t=>t.Name).ToList().AsReadOnly(), t.State);

         if (tasks.Any()){
            return tasks.ToArray()!;
        }
        else {
            return null!;
        }  

    }
    public WorkItemDetailsDTO Find(int taskId) {
        var tasks = from t in _context.Items
                    where t.Id == taskId
                    select new WorkItemDetailsDTO(t.Id, t.Title, t.Created, t.AssignedTo.Name, t.Tags.Select(t=>t.Name).ToList().AsReadOnly(), t.State, t.StateUpdated);
         if (tasks.Any()){
            return tasks.FirstOrDefault()!;
        }
        else {
            return null!;
        }
    }
    public Response Update(WorkItemUpdateDTO item) {
         var entity = _context.Items.Find(item.Id);
        Response response;

        if (entity is null)
        {
            response = Response.NotFound;
        }
        else if (_context.Items.FirstOrDefault(c => c.Id != item.Id) != null)
        {
            response = Response.Conflict;
        }
        else
        {   entity.Id = item.Id;
            entity.Title = item.Title;
            if(entity.State != item.State){
                entity.State = item.State;
                entity.StateUpdated = DateTime.UtcNow;
            }
        
            var tagList = new List<Tag>();
            foreach (string s in item.Tags){
                if((from t in _context.Tags where t.Name== s select t).FirstOrDefault()!= null){
                    tagList.Add((from t in _context.Tags where t.Name== s select t).FirstOrDefault()!);
                }
            }
            entity.Tags = tagList;
            if((from u in _context.Users where u.Id == item.AssignedToId select u).FirstOrDefault()==null){
                response = Response.BadRequest;
            }
            else {
                entity.AssignedTo = (from u in _context.Users where u.Id == item.AssignedToId select u).FirstOrDefault()!;
                entity.AssignedToId = item.AssignedToId;
                  response = Response.Updated;
            }

            _context.SaveChanges();
        }

        return response;

    }
    public Response Delete(int itemId) {
        var workItem = _context.Items.FirstOrDefault(t => t.Id == itemId);
        Response response;

        if (workItem is null){
            response = Response.NotFound;
        }

        else if (workItem.State == State.Active){
            workItem.State = State.Removed;
            response = Response.Updated;
        }
        else if (workItem.State == State.Resolved ||workItem.State == State.Closed || workItem.State == State.Removed){
            response = Response.Conflict;
        }
        else {
            _context.Items.Remove(workItem);
            _context.SaveChanges();
            response = Response.Deleted;
        }
        return response;

    }

}
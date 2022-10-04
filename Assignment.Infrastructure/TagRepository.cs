using Assignment.Core;
namespace Assignment.Infrastructure;

public class TagRepository : ITagRepository
{

    private readonly KanbanContext _context;

    public TagRepository(KanbanContext context)
    {
        _context = context;
    }


    public (Response Response, int TagId) Create(TagCreateDTO tag) {
        var entity = _context.Tags.FirstOrDefault(t => t.Name == tag.Name);
        Response response;

        if (entity is null){
            entity = new Tag(tag.Name);

            _context.Tags.Add(entity);
            _context.SaveChanges();

            response = Response.Created;
        }
        else {
            response = Response.Conflict;
        }

        var created = new TagDTO(entity.Id, entity.Name);
        
        return (response, created.Id);
    }

    public IReadOnlyCollection<TagDTO> Read(){
        var tags = from t in _context.Tags
                    orderby t.Name
                    select new TagDTO(t.Id, t.Name);

        if (tags.Any()){
            return tags.ToArray()!;
        }
        else {
            return null!;
        }           
    }

    public TagDTO Find(int tagId){
        var tags = from t in _context.Tags
                    where t.Id == tagId
                    select new TagDTO(t.Id, t.Name);
        if (tags.Any()){
            return tags.FirstOrDefault()!;
        }
        else {
            return null!;
        }
    }
    public Response Update(TagUpdateDTO tag){
        var entity = _context.Tags.Find(tag.Id);
        Response response;

        if (entity is null)
        {
            response = Response.NotFound;
        }
        else if (_context.Tags.FirstOrDefault(c => c.Id != tag.Id && c.Name == tag.Name) != null)
        {
            response = Response.Conflict;
        }
        else
        {
            entity.Name = tag.Name;
            _context.SaveChanges();
            response = Response.Updated;
        }

        return response;
    }
    public Response Delete(int tagId, bool force = false){
        var tag = _context.Tags.FirstOrDefault(t => t.Id == tagId);
        Response response;

        if (tag is null){
            response = Response.NotFound;
        }
        else if (tag.WorkItems.Any() && force == false){
            response = Response.Conflict;
        }
        else {
            _context.Tags.Remove(tag);
            _context.SaveChanges();
            response = Response.Deleted;
        }
        return response;
    }
}
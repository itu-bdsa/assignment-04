using System.Collections.ObjectModel;
namespace Assignment.Infrastructure;

public class TagRepository : ITagRepository
{
    private readonly KanbanContext _context;
    public (Response Response, int TagId) Create(TagCreateDTO tag)
    {
        var entity = _context.Tags.FirstOrDefault(t => t.Name == tag.Name);
        if(entity != null) return (Conflict, entity.Id);

        var entry = _context.Tags.Add(new Tag(tag.Name));
        _context.SaveChanges();
  
        return (Created, entry.Entity.Id);
    }
    public IReadOnlyCollection<TagDTO> Read() => new ReadOnlyCollection<TagDTO>(_context.Tags.Select(t => new TagDTO(t.Id, t.Name)).ToList());
    public TagDTO Find(int tagId)
    {
        var entity = _context.Tags.FirstOrDefault(t => t.Id == tagId);
        if(entity == null) return null;
        return new TagDTO(tagId, entity.Name);
    }
    public Response Update(TagUpdateDTO tag)
    {
        if(_context.Tags.FirstOrDefault(t => t.Name == tag.Name) != null) return Conflict;
        var entity = _context.Tags.FirstOrDefault(t => t.Id == tag.Id);
        if(entity == null) return NotFound;
        entity.Name = tag.Name;
        _context.SaveChanges();
        return Updated;
    }
    public Response Delete(int tagId, bool force = false)
    {
        var entity = _context.Tags.FirstOrDefault(t => t.Id == tagId);
        if(entity == null) return NotFound;
        if(force)
        {
            _context.Tags.Remove(entity);
            return Deleted;
        }
        if(_context.Items.Where(t => t.Tags.Contains(entity)).Any()) return Conflict;
        _context.Tags.Remove(entity);
        return Deleted;
    }

    public TagRepository(KanbanContext context)
    {
        _context = context;
    }
}

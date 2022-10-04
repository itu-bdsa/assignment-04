namespace Assignment.Infrastructure;

using System.Collections.Generic;
using Assignment.Core;

public class TagRepository : ITagRepository
{
  private readonly KanbanContext _context;

  public TagRepository(KanbanContext context) {
    _context = context;
  }

  public (Response Response, int TagId) Create(TagCreateDTO tag)
  {
    if(_context.Tags.Where(t => t.Name == tag.Name).FirstOrDefault() != null) {
        return (Response.Conflict, -1); 
    }
    var t = new Tag(tag.Name);
    _context.Tags.Add(t);
    _context.SaveChanges();
    return (Response.Created, t.Id);
  }

  public Response Delete(int tagId, bool force = false)
  {
    var tagToBeDeleted = _context.Tags.Find(tagId);
    if(tagToBeDeleted != null){
      if(tagToBeDeleted.WorkItems.Count == 0 || force){
        _context.Tags.Remove(tagToBeDeleted);
        return Response.Deleted;
      } else {
        return Response.Conflict;
      }
    } else {
      return Response.NotFound;
    }
  }

  public TagDTO? Find(int tagId)
  {
    var tag = _context.Tags.Find(tagId);
    if(tag != null){
      return new TagDTO(tag.Id, tag.Name);
    }
    return null;
  }

  public IReadOnlyCollection<TagDTO> Read()
  {
    return _context.Tags.Select(tag => new TagDTO(tag.Id, tag.Name)).ToList();
  }

  public TagDTO Read(int tagId)
  {
    return Find(tagId)!;
  }

  public IReadOnlyCollection<TagDTO> ReadAll()
  {
      return Read();
  }

  public Response Update(TagUpdateDTO tag)
  {
      var _tag = _context.Tags.Find(tag.Id);
      if(_tag != null){
        _tag.Name = tag.Name;
        _context.Tags.Update(_tag);
        return Response.Updated;
      }
      return Response.NotFound;
  }
}
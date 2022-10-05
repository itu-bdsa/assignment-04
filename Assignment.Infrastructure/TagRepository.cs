namespace Assignment.Infrastructure;

public class TagRepository : ITagRepository
{

  private readonly KanbanContext _kanban;

  public TagRepository(KanbanContext kanban)
  {
    _kanban = kanban;
  }


  public (Response Response, int TagId) Create(TagCreateDTO tag)
  {
    var entity = _kanban.Tags.FirstOrDefault(t => t.Name == tag.Name);
    Response response;

    if (entity is null)
    {
      entity = new Tag { Name = tag.Name };
      _kanban.Add(entity);
      _kanban.SaveChanges();
      response = Response.Created;
    }
    else
    {
      response = Response.Conflict;
    }

    return (response, entity.Id);
  }

  public Response Delete(int tagId, bool force = false)
  {
    var tag = _kanban.Tags.FirstOrDefault(t => t.Id == tagId);

    Response response;

    if (tag is null)
    {
      response = Response.NotFound;
    }
    else if (tag.WorkItems.Any() && !force)
    {
      response = Response.Conflict;
    }
    else if (!force)
    {
      response = Response.Conflict;
    }
    else
    {
      response = Response.Deleted;
    }

    return response;
  }

  public TagDTO Find(int tagId)
  {
    var tags = from t in _kanban.Tags
               where t.Id == tagId
               select new TagDTO(t.Id, t.Name!);

    return tags.FirstOrDefault()!;
  }

  public IReadOnlyCollection<TagDTO> Read()
  {
    var tags = from t in _kanban.Tags
               orderby t.Name
               select new TagDTO(t.Id, t.Name!);

    return tags.ToArray();
  }

  public Response Update(TagUpdateDTO tag)
  {
    var entity = _kanban.Users.Find(tag.Id);
    Response response;

    if (entity is null)
    {
      response = Response.NotFound;
    }
    else if (_kanban.Users.FirstOrDefault(t => t.Id != tag.Id && t.Name == tag.Name) != null)
    {
      response = Response.Conflict;
    }
    else
    {
      entity.Name = tag.Name;
      _kanban.SaveChanges();
      response = Response.Updated;
    }

    return response;
  }
}

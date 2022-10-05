namespace Assignment.Infrastructure;

public class UserRepository : IUserRepository
{
    private readonly KanbanContext _kanban;

    public UserRepository(KanbanContext kanban)
    {
        _kanban = kanban;
    }

    public (Response Response, int UserId) Create(UserCreateDTO user)
    {
        var entity = _kanban.Users.FirstOrDefault(k => k.Email == user.Email);

        Response resp;

      
        if (entity is null)
        {
            entity = new User { Name = user.Name, Email = user.Email };

            _kanban.Add(entity);
            _kanban.SaveChanges();
            resp = Response.Created;

        }
        else
        {
            resp = Response.Conflict;
        }

        return (resp, entity.Id);

    }

    public Response Delete(int userId, bool force = false)
    {
        var user = _kanban.Users.FirstOrDefault(u => u.Id == userId);

        Response response;
        
         if (user is null)
        {
            response = Response.Conflict;
        }
        else if (user.WorkItems!.Any() && !force)
        {
            response = Response.NotFound;
        }
        else if(!force)
        {
            response = Response.Conflict;
        }
         else
        {
            response = Response.Deleted;
        }

        return response;
    }

    public UserDTO Find(int userId)
    {
        var users = from u in _kanban.Users
                    where u.Id == userId
                    select new UserDTO(u.Id, u.Name, u.Email);

        return users.FirstOrDefault();
    }

    public IReadOnlyCollection<UserDTO> Read()
    {
        var users = from u in _kanban.Users
                    orderby u.Name
                    select new UserDTO(u.Id, u.Name, u.Email);
        return users.ToArray();
    }

    public Response Update(UserUpdateDTO user)
    {
        var entity = _kanban.Users.Find(user.Id);
        Response response;

        if (entity is null)
        {
            response = Response.NotFound;
        }
        else if (_kanban.Users.FirstOrDefault(u => u.Id != user.Id && u.Email == user.Email) != null)
        {
            response = Response.Conflict;
        }
        else
        {
            entity.Name = user.Name;
            entity.Email = user.Email;
            _kanban.SaveChanges();
            response = Response.Updated;
        }

        return response;
    }
}

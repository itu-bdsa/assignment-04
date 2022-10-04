namespace Assignment.Infrastructure;
using Assignment.Core;

public sealed class UserRepository : IUserRepository
{
    private readonly KanbanContext _context;

    public UserRepository(KanbanContext context)
    {
        _context = context;
    }

    public (Response Response, int UserId) Create(UserCreateDTO user) {
        var entity = _context.Users.FirstOrDefault(u => u.Email == user.Email);
        Response response;

        if (entity is null){
            entity = new User(user.Name, user.Email);

            _context.Users.Add(entity);
            _context.SaveChanges();

            response = Response.Created;
        }
        else {
            response = Response.Conflict;
        }

        var created = new UserDTO(entity.Id, entity.Name, entity.Email);
        
        return (response, created.Id);
    }

    public IReadOnlyCollection<UserDTO> Read(){
        var users = from u in _context.Users
                    orderby u.Name
                    select new UserDTO(u.Id, u.Name, u.Email);

        if (users.Any()){
            return users.ToArray()!;
        }
        else {
            return null!;
        }          
    }

    public UserDTO Find(int userId){
        var users = from u in _context.Users
                    where u.Id == userId
                    select new UserDTO(u.Id, u.Name, u.Email);
        if (users.Any()){
            return users.FirstOrDefault()!;
        }
        else {
            return null!;
        }
    }
    
    public Response Update(UserUpdateDTO user){
        var entity = _context.Users.Find(user.Id);
        Response response;

        if (entity is null)
        {
            response = Response.NotFound;
        }
        else if (_context.Users.FirstOrDefault(c => c.Id != user.Id && c.Email == user.Email) != null)
        {
            response = Response.Conflict;
        }
        else
        {
            entity.Name = user.Name;
            entity.Email = user.Email;
            _context.SaveChanges();
            response = Response.Updated;
        }

        return response;
    }
    public Response Delete(int userId, bool force = false){
        var user = _context.Users.FirstOrDefault(u => u.Id == userId);
        Response response;

        if (user is null){
            response = Response.NotFound;
        }
        else if (user.Items.Any() && force == false){
            response = Response.Conflict;
        }
        else {
            _context.Users.Remove(user);
            _context.SaveChanges();
            response = Response.Deleted;
        }
        return response;
    }
}

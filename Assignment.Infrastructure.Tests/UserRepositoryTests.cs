namespace Assignment.Infrastructure.Tests;

public class UserRepositoryTests : IDisposable
{
    private readonly KanbanContext _context;
    private readonly UserRepository _repository;
     private readonly WorkItemRepository _workItemRepository;

    public UserRepositoryTests()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var builder = new DbContextOptionsBuilder<KanbanContext>();
        builder.UseSqlite(connection);
        var context = new KanbanContext(builder.Options);
        context.Database.EnsureCreated();

        var user = new User("Poul Poulsen", "poul@thepoul.dk") {
            Id = 1
        };
        var user2 = new User("Jens Jensen", "jens@thejensen.dk") {
            Id = 2,
            
        };
        context.Add(user);
        context.Add(user2);
        context.SaveChanges();

        _context = context;
        _repository = new UserRepository(_context);
        _workItemRepository = new WorkItemRepository(_context);
    }

    [Fact]
    public void Delete_user_assigned_to_WorkItem_without_force_returns_conflict(){
        var (userRes, userId) = _repository.Create(new UserCreateDTO("navn1", "email1"));
        var workItemDTO = new WorkItemCreateDTO("UI Layout", userId, "Redo design of ui layout", new List<String>{"tag-navn"});
        var (workItemRes, workItemId) = _workItemRepository.Create(workItemDTO);

        var deletedResponse = _repository.Delete(userId, false);
        Assert.Equal(Response.Conflict, deletedResponse);
    }

    [Fact]
    public void Delete_user_assigned_to_WorkItem_with_force_returns_deleted(){
        var (userRes, userId) = _repository.Create(new UserCreateDTO("navn1", "email1"));
        var workItemDTO = new WorkItemCreateDTO("UI Layout", userId, "Redo design of ui layout", new List<String>{"tag-navn"});
        var (workItemRes, workItemId) = _workItemRepository.Create(workItemDTO);

        var deletedResponse = _repository.Delete(userId, true);
        Assert.Equal(Response.Deleted, deletedResponse);
    }

    [Fact]
    public void Create_returns_userID_and_created_response() {
        //Arrange
        var user = new UserCreateDTO("Jens", "jens@jensen.dk");

        //act
        var actual = _repository.Create(user);
        //assert
        actual.Should().Be((Response.Created, 3));
    }

    [Fact]
    public void Create_returns_conflict_response_given_same_email() {
        //Arrange
        var user = new UserCreateDTO("Henrik", "poul@thepoul.dk");

        //act
        var actual = _repository.Create(user);
        //assert
        actual.Should().Be((Response.Conflict, 0));
    }

    [Fact]
    public void Delete_returns_deleted_response_given_userId() {
        

        var actual = _repository.Delete(1);

        actual.Should().Be(Response.Deleted);
    }

    [Fact]
    public void Read_returns_UserDTO_given_userId() {

        var actual = _repository.Read();

        var expected = new UserDTO(1, "Poul Poulsen", "poul@thepoul.dk");

        actual.First().Should().Be(expected);
    }

    [Fact]
    public void Read_returns_all_users() {
        var actual = _repository.Read();
        actual.Should().BeEquivalentTo(new[] {
            new UserDTO(1,"Poul Poulsen", "poul@thepoul.dk"), new UserDTO(2, "Jens Jensen", "jens@thejensen.dk")
        });
    }

    [Fact]
    public void Update_returns_response_given_user_update_dto() {
        var newUser = new UserUpdateDTO(1, "Poul Poulsen", "poulcool@thepoul.dk");
        var actual = _repository.Update(newUser);

        actual.Should().Be(Response.Updated);
    }

    [Fact]
    public void Delete_notfound_null_given_MaxValue_userid() {
        
        var actual = _repository.Delete(int.MaxValue);

        actual.Should().Be(Response.NotFound);
    }

    [Fact]
    public void Update_returns_notfound_given_maxvalue_userID() {
        var newUser = new UserUpdateDTO(int.MaxValue, "Poul Poulsen", "poulcool@thepoul.dk");
        var actual = _repository.Update(newUser);

        actual.Should().Be(Response.NotFound);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
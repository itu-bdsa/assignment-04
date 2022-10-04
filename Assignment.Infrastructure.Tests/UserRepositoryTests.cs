using Xunit;
using Microsoft.EntityFrameworkCore;

namespace Assignment.Infrastructure.Tests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Assignment.Core;
public sealed class UserRepositoryTests : IDisposable
{
    private readonly KanbanContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var builder = new DbContextOptionsBuilder<KanbanContext>();
        builder.UseSqlite(connection);
        var context = new KanbanContext(builder.Options);
        context.Database.EnsureCreated();
        var user1 = new User("Oliver", "OllesEmail.dk") { Id = 1 };
        context.Users.AddRange(user1, new User("EMy-Chunnn", "coolKidzz@mail.ru") { Id = 2 });
        context.Items.Add(new WorkItem("Do stuff") {AssignedTo = user1, Title = "Do stuff", State = State.Active});
        context.SaveChanges();

        _context = context;
        _repository = new UserRepository(_context);
    }

    [Fact]
    public void Create_given_User_returns_Created_with_User()
    {
        var (response, id) = _repository.Create(new UserCreateDTO("Emy", "emys@email.com"));

        response.Should().Be(Response.Created);

        id.Should().Be(3);
    }

    [Fact]
    public void Create_given_existing_User_returns_Conflict_with_existing_User()
    {
        var (response, id) = _repository.Create(new UserCreateDTO("Oliver", "OllesEmail.dk"));

        response.Should().Be(Response.Conflict);

        id.Should().Be(1);
    }

    [Fact]
    public void Find_given_non_existing_id_returns_null() => _repository.Find(42).Should().BeNull();

    [Fact]
    public void Read_given_existing_id_returns_User() => _repository.Find(1).Should().Be(new UserDTO(1, "Oliver", "OllesEmail.dk"));

    [Fact]
    public void ReadAll_returns_all_users() => _repository.Read().Should().BeEquivalentTo(new[] { new UserDTO(1, "Oliver", "OllesEmail.dk"), new UserDTO(2, "EMy-Chunnn", "coolKidzz@mail.ru") });

    [Fact]
    public void Update_given_non_existing_User_returns_NotFound() => _repository.Update(new UserUpdateDTO(42, "Andyboii", "lafu@4life")).Should().Be(Response.NotFound);

    [Fact]
    public void Update_given_existing_Email_returns_Conflict_and_does_not_update()
    {
        var response = _repository.Update(new UserUpdateDTO(2, "EMy-Chunnn", "OllesEmail.dk"));

        response.Should().Be(Response.Conflict);

        var entity = _context.Users.Find(2)!;

        entity.Email.Should().Be("coolKidzz@mail.ru");
    }
    
    [Fact]
    public void Update_updates_Name_and_returns_Updated()
    {
        var response = _repository.Update(new UserUpdateDTO(2, "EMy-Chun", "coolKidzz@mail.ru"));

        response.Should().Be(Response.Updated);

        var entity = _context.Users.Find(2)!;

        entity.Name.Should().Be("EMy-Chun");
    }

    [Fact]
    public void Update_updates_Email_and_returns_Updated()
    {
        var response = _repository.Update(new UserUpdateDTO(2, "EMy-Chunnn", "coolKidzz@gmail.com"));

        response.Should().Be(Response.Updated);

        var entity = _context.Users.Find(2)!;

        entity.Email.Should().Be("coolKidzz@gmail.com");
    }

    [Fact]
    public void Delete_given_non_existing_Id_returns_NotFound() => _repository.Delete(42).Should().Be(Response.NotFound);

    [Fact]
    public void Delete_deletes_and_returns_Deleted()
    {
        var response = _repository.Delete(2);

        response.Should().Be(Response.Deleted);

        var entity = _context.Users.Find(2);

        entity.Should().BeNull();
    }

    [Fact]
    public void Delete_given_existing_User_with_Tasks_returns_Conflict_and_does_not_delete()
    {
        var response = _repository.Delete(1);

        response.Should().Be(Response.Conflict);

        _context.Users.Find(1).Should().NotBeNull();
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
}

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace Assignment.Infrastructure.Tests;

public class UserRepositoryTests : IDisposable
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

    context.Users.AddRange(
      new User() {Id = 6, Name = "Ron Weasly", Email = "ronweasly@gmail.com", WorkItems = new List<WorkItem>()},
      new User() {Id = 7, Name = "Harry Potter", Email = "harryp@hotmail.dk", WorkItems = new List<WorkItem>()});

    context.SaveChanges();

      _context = context;
      _repository = new UserRepository(_context);

  }
   [Fact]
  public void Test_user_can_delete_force() => _repository.Delete(6, true).Should().Be(Response.Deleted);


    [Fact]
  public void Test_user_cannot_delete_return_Conflict() => _repository.Delete(7, false).Should().Be(Response.Conflict);


  [Fact]
  public void Test_try_create_user_already_exsist_return_Conflict()
  {
    //Arrange
    var (response, id) = _repository.Create(new UserCreateDTO("Harry Potter", "harryp@hotmail.dk"));
    response.Should().Be(Response.Conflict);

  }


  public void Dispose()
  {
     _context.Dispose();
  }
}

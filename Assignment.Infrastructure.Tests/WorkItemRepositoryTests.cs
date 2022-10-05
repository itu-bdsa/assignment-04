using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace Assignment.Infrastructure.Tests;

public class WorkItemRepositoryTests : IDisposable
{
  private readonly KanbanContext _context;
  private readonly WorkItemRepository _repository;
  public WorkItemRepositoryTests()
  {
    var connection = new SqliteConnection("Filename=:memory:");
    connection.Open();
    var builder = new DbContextOptionsBuilder<KanbanContext>();
    builder.UseSqlite(connection);
    var context = new KanbanContext(builder.Options);
    context.Database.EnsureCreated();

    context.WorkItems.AddRange(new WorkItem() { Id = 100, Title = "pink post it", State = State.Active },
                                new WorkItem() { Id = 11, Title = "blue post it", State = Core.State.New },
                                new WorkItem() { Id = 12, Title = "green post it", State = Core.State.Resolved },
                                new WorkItem() { Id = 13, Title = "red post it", State = Core.State.Closed },
                                new WorkItem() { Id = 14, Title = "purple post it", State = Core.State.Removed });

    //context.Users.Add(new User() { Id = 5, Name = "Clara" });

    context.SaveChanges();

    _context = context;
    _repository = new WorkItemRepository(context);
    context.SaveChanges();

  }
  [Fact]
  public void Test_only_new_can_be_deleted()
  {
    //Arrange
    var response = _repository.Delete(11);
    response.Should().Be(Response.Deleted);

    //Act
    var entity = _context.WorkItems.Find(11);

    //Assert
    entity.Should().BeNull();

  }

  [Fact]
  public void Test_delete_active_state_should_be_removed()
  {
    var response = _repository.Delete(100);

    var entity = _context.WorkItems.Find(100);

    entity!.State.Should().Be(State.Removed);

  }

  [Fact]
  public void Test_delete_resolved_return_conflict() => _repository.Delete(12).Should().Be(Response.Conflict);


  [Fact]
  public void Test_delete_closed_return_conflict() => _repository.Delete(13).Should().Be(Response.Conflict);

  [Fact]
  public void Test_delete_removed_return_conflict() => _repository.Delete(14).Should().Be(Response.Conflict);

  [Fact]
  public void create_should_return_new_and_created_current_time()
  {
    var (response, workitemId) = _repository.Create(new WorkItemCreateDTO("black post it", 5, "Clara", new List<string>()));
    response.Should().Be(Response.Created);
    _context.WorkItems.Find(workitemId)!.State.Should().Be(State.New);
  }


  public void Dispose()
  {
    _context.Dispose();
  }


}

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace Assignment.Infrastructure.Tests;

public class TagRepositoryTests : IDisposable
{
  private readonly KanbanContext _context;
  private readonly TagRepository _repository;

  public TagRepositoryTests()
  {
    var connection = new SqliteConnection("Filename=:memory:");
    connection.Open();
    var build = new DbContextOptionsBuilder<KanbanContext>();
    build.UseSqlite(connection);
    var context = new KanbanContext(build.Options);
    context.Database.EnsureCreated();

    context.Tags.AddRange(
      new Tag(){ Id = 8, Name = "Frontend"},
      new Tag(){ Id = 9, Name = "backend"},
      new Tag(){ Id = 10, Name = "any end"}
    );
    
    context.SaveChanges();  
    _context = context;
    _repository = new TagRepository(_context);
  }
  [Fact]
  public void Test_tags_can_delete_force() => _repository.Delete(8, true).Should().Be(Response.Deleted);


  [Fact]
  public void Test_tags_cannot_delete_return_Conflict() => _repository.Delete(9, false).Should().Be(Response.Conflict);


  [Fact]
  public void Test_creat_tag_already_exist_return_Conflict()
  {
    var (response, id) = _repository.Create(new TagCreateDTO("any end"));
    response.Should().Be(Response.Conflict);
  }

  public void Dispose()
  {
    _context.Dispose();
  }

}



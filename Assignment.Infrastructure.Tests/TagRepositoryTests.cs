using Xunit;
using Microsoft.EntityFrameworkCore;

namespace Assignment.Infrastructure.Tests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Assignment.Core;
public sealed class TagRepositoryTests : IDisposable
{
    private readonly KanbanContext _context;
    private readonly TagRepository _repository;

    public TagRepositoryTests()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var builder = new DbContextOptionsBuilder<KanbanContext>();
        builder.UseSqlite(connection);
        var context = new KanbanContext(builder.Options);
        context.Database.EnsureCreated();
        var user1 = new User("Oliver", "OllesEmail.dk") { Id = 1 };
        var item1 = new WorkItem("Do stuff"){AssignedTo = user1, Title = "Do stuff", State = State.Active};
        var tag1 = new Tag("Winable") { Id = 1, WorkItems = new HashSet<WorkItem>{item1}};
        context.Tags.AddRange(tag1, new Tag("Lose") { Id = 2 });
        context.Items.Add(new WorkItem("Do stuff") {AssignedTo = user1, Title = "Do stuff", State = State.Active});
        context.Users.Add(user1);
        context.SaveChanges();

        _context = context;
        _repository = new TagRepository(_context);
    }

    [Fact]
    public void Create_given_Tag_returns_Created_with_Tag()
    {
        var (response, id) = _repository.Create(new TagCreateDTO("Win"));

        response.Should().Be(Response.Created);

        id.Should().Be(3);
    }

    [Fact]
    public void Create_given_existing_User_returns_Conflict_with_existing_User()
    {
        var (response, id) = _repository.Create(new TagCreateDTO("Winable"));

        response.Should().Be(Response.Conflict);

        id.Should().Be(1);
    }

    [Fact]
    public void Find_given_non_existing_id_returns_null() => _repository.Find(42).Should().BeNull();

    [Fact]
    public void Find_given_existing_id_returns_User() => _repository.Find(1).Should().Be(new TagDTO(1, "Winable"));

    [Fact]
    public void Read_returns_all_tags() => _repository.Read().Should().BeEquivalentTo(new[] { new TagDTO(1, "Winable"), new TagDTO(2, "Lose") });

    [Fact]
    public void Update_given_non_existing_Tag_returns_NotFound() => _repository.Update(new TagUpdateDTO(42, "GoNext")).Should().Be(Response.NotFound);

    [Fact]
    public void Update_given_existing_Name_returns_Conflict_and_does_not_update()
    {
        var response = _repository.Update(new TagUpdateDTO(2, "Winable"));

        response.Should().Be(Response.Conflict);

        var entity = _context.Tags.Find(2)!;

        entity.Name.Should().Be("Lose");
    }
    
    [Fact]
    public void Update_updates_Name_and_returns_Updated()
    {
        var response = _repository.Update(new TagUpdateDTO(2, "NoLose"));

        response.Should().Be(Response.Updated);

        var entity = _context.Tags.Find(2)!;

        entity.Name.Should().Be("NoLose");
    }


    [Fact]
    public void Delete_given_non_existing_Id_returns_NotFound() => _repository.Delete(42).Should().Be(Response.NotFound);

    [Fact]
    public void Delete_deletes_and_returns_Deleted()
    {
        var response = _repository.Delete(2);

        response.Should().Be(Response.Deleted);

        var entity = _context.Tags.Find(2);

        entity.Should().BeNull();
    }

    [Fact]
    public void Delete_given_existing_Tag_in_use_returns_Conflict_and_does_not_delete()
    {
        var response = _repository.Delete(1);

        response.Should().Be(Response.Conflict);

        _context.Tags.Find(1).Should().NotBeNull();
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
}

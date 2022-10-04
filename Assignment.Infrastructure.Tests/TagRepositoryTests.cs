using Assignment.Core;
namespace Assignment.Infrastructure.Tests;

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
        context.Tags.AddRange(new Tag("High priority") { Id = 1 }, new Tag("Low priority") { Id = 2 });
        context.Items.Add(new WorkItem("Baking cake") {Id = 1});
        context.Items.Find(1)!.Tags = new HashSet<Tag>(){context.Tags.Find(1)!};
        context.SaveChanges();

        _context = context;
        _repository = new TagRepository(_context);
    }

    [Fact]
    public void Delete_given_non_existing_Id_should_return_NotFound() => _repository.Delete(100).Should().Be(NotFound);

    [Fact]
    public void Update_given_non_existing_Id_should_return_NotFound() => _repository.Update(new TagUpdateDTO(100, "Critical")).Should().Be(NotFound);

    [Fact]
    public void Find_given_non_exisiting_Id_should_return_null() => _context.Tags.Find(3).Should().Be(null);

    [Fact]
    public void Delete_given_tag_in_use_returns_Conflict() => _repository.Delete(1).Should().Be(Conflict);


    [Fact]
    public void Delete_given_tag_in_use_with_force_flag_returns_Deleted() =>  _repository.Delete(1, true).Should().Be(Deleted);

    [Fact]
    public void Create_given_already_exisiting_tag_returns_Conflict() => _repository.Create(new TagCreateDTO("High priority")).Should().Be((Conflict, 1));

    public void Dispose()
    {
        _context.Dispose();
    }
}

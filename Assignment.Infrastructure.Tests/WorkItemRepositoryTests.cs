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
        context.Items.AddRange(new WorkItem("Clean dishes") { Id = 1 }, new WorkItem("do BDSA assignment") { Id = 2 });
        context.Users.Add(new User("Sven","sven@svensker.se") {Id = 1});
        context.SaveChanges();

        _context = context;
        _repository = new WorkItemRepository(_context);
    }

    [Fact]
    public void Delete_given_non_existing_Id_should_return_NotFound() => _repository.Delete(100).Should().Be(NotFound);

    [Fact]
    public void Update_given_non_existing_Id_should_return_NotFound() => _repository.Update(new WorkItemUpdateDTO(100, "Do work", 1, "Work should be done", new HashSet<string>(){"High priority"}, Active)).Should().Be(NotFound);

    [Fact]
    public void Find_given_non_exisiting_Id_should_return_null() => _context.Tags.Find(3).Should().Be(null);

    [Fact]
    public void Delete_given_new_workitem_should_remove_from_database()
    {
        var obj = _context.Items.Find(1)!;
        obj.State = New;

        var response = _repository.Delete(1);

       response.Should().Be(Deleted);
    }

    [Fact]
    public void Delete_given_active_workitem_should_change_state_to_removed()
    {
        var obj = _context.Items.Find(1)!;
        obj.State = Active;

        _repository.Delete(1);

        obj.State.Should().Be(Removed);
    }

    [Fact]
    public void Delete_given_removed_workitem_should_return_Conflict()
    {
        var obj = _context.Items.Find(1)!;
        obj.State = Removed;

        var response = _repository.Delete(1);

        response.Should().Be(Conflict);
    }

    [Fact]
    public void Create_sets_State_of_new_workitem_to_new()
    {
        var (response, newItemId) = _repository.Create(new WorkItemCreateDTO("Test kode", null, null, new HashSet<string>()));
        var obj = _context.Items.Find(newItemId)!;

        obj.State.Should().Be(New);
        obj.Created.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        obj.StateUpdated.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Update_when_updating_state_sets_new_StateUpdated_datetime()
    {
        var obj = _context.Items.Find(1)!;
        obj.StateUpdated = DateTime.UtcNow.AddSeconds(-10);
        
        var response = _repository.Update(new WorkItemUpdateDTO(1, "Clean dishes", null, null, new HashSet<string>(), Closed));

        response.Should().Be(Updated);
        obj.StateUpdated.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

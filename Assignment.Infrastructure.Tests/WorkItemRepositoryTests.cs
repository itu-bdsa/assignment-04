
namespace Assignment.Entities.Tests;

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

        var user = new User("Poul Poulsen", "poul@thepoul.dk") {
            Id = 1
        };
        context.Add(user);
        context.SaveChanges();

        _context = context;
        _repository = new WorkItemRepository(_context);
    }

    [Fact]
    public void Create_WorkItem_Upholds_Rules()
    {
        //Arrange
        var workItemDTO = new WorkItemCreateDTO("UI Layout", null, "Redo design of ui layout", new List<string>() { "something", "somethign else"});

        //Act
        var (response, workItemid) = _repository.Create(workItemDTO);
        var workItem = _context.Items.Find(workItemid);

        //Assert
        Assert.Equal(Response.Created, response);
        Assert.Equal(State.New, workItem.State);
        Assert.Equal(DateTime.UtcNow, workItem.Created, precision: TimeSpan.FromSeconds(10));
        Assert.Equal(DateTime.UtcNow, workItem.StateUpdated, precision: TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void Delete_New()
    {
        //Arrange
        var workItem = new WorkItemCreateDTO("UI Layout", null, "Redo design of ui layout", new List<string>() { "something", "somethign else"});
        var (reponse, newId) = _repository.Create(workItem);

        //Act
        var response = _repository.Delete(newId);
        var found = _context.Items.Find(newId);

        //Assert
        Assert.Equal(Response.Deleted, response);
        Assert.Null(found);
    }

    [Fact]
    public void Delete_Active_HaveState_Removed()
    {
        //Arrange
        var workItem = new WorkItem("Database Structure") {
            AssignedToId = null, 
            Description = "Setup database with suituble data structures", 
            Tags = null};
        workItem.State = State.Active;
        _context.Items.Add(workItem);
        _context.SaveChanges();

        //Act
        var response = _repository.Delete(workItem.Id);

        //Assert
        Assert.Equal(Response.Deleted, response);
        Assert.Equal(State.Removed, workItem.State);
    }

    [Fact]
    public void Delete_Return_Conflict()
    {
        //Arrange
        var workItem = new WorkItem("Database Structure"){
            AssignedToId = null, 
            Description = "Setup database with suituble data structures", 
            Tags = null};
        workItem.State = State.Closed;
        _context.Items.Add(workItem);
        _context.SaveChanges();

        //Act
        var response = _repository.Delete(workItem.Id);

        //Assert
        Assert.Equal(Response.Conflict, response);
    }

    [Fact]
    public void Create_WorkItem_With_NonExsistingUser_Return_BadRequest()
    {
        //Arrange
        var workItemDTO = new WorkItemCreateDTO("UI Layout", 7, "Redo design of ui layout", null);

        //Act
        var (response, workItemId) = _repository.Create(workItemDTO);

        //Assert
        Assert.Equal(Response.BadRequest, response);
    }

    [Fact]
    public void Changing_State_Updates_StateUpdated()
    {
        //Arrange
        var workItem = new WorkItemCreateDTO("Database Structure", 1, "Description", new List<string>());
        var result = _repository.Create(workItem);

        var updateDTO = new WorkItemUpdateDTO(result.ItemId, workItem.Title, workItem.AssignedToId, workItem.Description, new List<string>(), State.Resolved);

        //Act
        var response = _repository.Update(updateDTO);
        var actual = _repository.Find(result.ItemId);

        //Assert
        Assert.Equal(Response.Updated, response);
        Assert.Equal(State.Resolved, actual.State);
        Assert.Equal(DateTime.UtcNow, actual.StateUpdated, precision: TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Link_Tags_To_WorkItem()
    {
        //Arrange
        var tag = new Tag("Tag1");
        _context.Tags.Add(tag);
        _context.SaveChanges();

        var workItemDTO = new WorkItemCreateDTO("UI Layout", null, "Redo design of ui layout", new List<string>() { "Tag1" });

        //Act
        var (response, workItemId) = _repository.Create(workItemDTO);
        var workItem = _context.Items.Find(workItemId);

        //Assert
        Assert.Equal(Response.Created, response);
        Assert.True(workItem.Tags.FirstOrDefault().Id == tag.Id);
    }

    [Fact]
    public void Update_NonExsistingWorkItem_Return_NotFound()
    {
        //Arrange
        var updateDTO = new WorkItemUpdateDTO(7, "UI Layout", null, "Redo design of ui layout", null, State.Resolved);

        //Act
        var response = _repository.Update(updateDTO);

        //Assert
        Assert.Equal(Response.NotFound, response);
    }

    [Fact]
    public void Read_NonTexsistingWorkItem_Return_Null()
    {
        //Arrange

        //Act
        var workItem = _repository.Find(7);

        //Assert
        Assert.Null(workItem);
    }

    public void Dispose() {
        _context.Dispose();
    }
}
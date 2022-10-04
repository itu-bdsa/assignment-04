namespace Assignment3.Entities.Tests;
using Assignment.Entities;
using Assignment.Core;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public class TagRepositoryTests : IDisposable
{
  private readonly KanbanContext _context;
  private readonly TagRepository _repository;
  private readonly WorkItemRepository _workItemRepository;

  public TagRepositoryTests() {
    var connection = new SqliteConnection("Filename=:memory:");
    connection.Open();
    var builder = new DbContextOptionsBuilder<KanbanContext>();
    builder.UseSqlite(connection);
    var context = new KanbanContext(builder.Options);
    context.Database.EnsureCreated();

    _context = context;
    _repository = new TagRepository(_context);
    _workItemRepository = new WorkItemRepository(_context);
  }

  [Fact]
  public void create_tag_and_adds_to_db() {
    var (res, id) = _repository.Create(new TagCreateDTO("tag-navn-1"));
    Assert.Equal(Response.Created, res);
    Assert.NotNull(_context.Tags.Find(id));
  }

  [Fact]
  public void create_duplicate_tag_return_conflict() {
    var (res1, id1) = _repository.Create(new TagCreateDTO("tag-navn-1"));
    var (res2, id2) = _repository.Create(new TagCreateDTO("tag-navn-1"));

    Assert.Equal(Response.Created, res1);
    Assert.Equal(1, id1);

    Assert.Equal(Response.Conflict, res2);
    Assert.Equal(-1, id2);
  }

  [Fact]
  public void delete_tag_not_in_use_without_force_returns_Deleted(){
    var (res, id) = _repository.Create(new TagCreateDTO("tag-navn-1"));
    var deleteResponse = _repository.Delete(id, false);
    Assert.Equal(Response.Deleted, deleteResponse);
  }

  [Fact]
  public void delete_tag_not_in_use_with_force_returns_Deleted(){
    var (res, id) = _repository.Create(new TagCreateDTO("tag-navn-1"));
    var deleteResponse = _repository.Delete(id, true);
    Assert.Equal(Response.Deleted, deleteResponse);
  }

  [Fact]
  public void delete_tag_in_use_with_force_returns_Deleted(){
    var (tagRes, tagId) = _repository.Create(new TagCreateDTO("tag-navn-1"));
    var workItemDTO = new WorkItemCreateDTO("UI Layout", null, "Redo design of ui layout", new List<String>{"tag-navn-1"});
    var (workItemRes, workItemId) = _workItemRepository.Create(workItemDTO);

    var insertedTag = _context.Tags.Find(tagId);
    var insertedWorkItem = _context.Items.Find(workItemId);
    bool insertedTagContainsInsertedWorkItem = insertedTag!.WorkItems.Contains(insertedWorkItem!);

    Assert.NotNull(insertedTag);
    Assert.NotNull(insertedWorkItem);
    Assert.Equal(true, insertedTagContainsInsertedWorkItem);

    var deleteResponse = _repository.Delete(tagId, true);

    Assert.Equal(Response.Deleted, deleteResponse);
  }

  [Fact]
  public void delete_tag_in_use_without_force_returns_Conflict(){
    var (tagRes, tagId) = _repository.Create(new TagCreateDTO("tag-navn-1"));
    var workItemDTO = new WorkItemCreateDTO("UI Layout", null, "Redo design of ui layout", new List<String>{"tag-navn-1"});
    var (workItemRes, workItemId) = _workItemRepository.Create(workItemDTO);

    var insertedTag = _context.Tags.Find(tagId);
    var insertedWorkItem = _context.Items.Find(workItemId);
    bool insertedTagContainsInsertedWorkItem = insertedTag!.WorkItems.Contains(insertedWorkItem!);

    Assert.NotNull(insertedTag);
    Assert.NotNull(insertedWorkItem);
    Assert.Equal(true, insertedTagContainsInsertedWorkItem);

    var deleteResponse = _repository.Delete(tagId, false);

    Assert.Equal(Response.Conflict, deleteResponse);
  }

  [Fact]
  public void find_existing_tag_returns_tagdto() {
    var (tagRes, tagId) = _repository.Create(new TagCreateDTO("tag-navn-1"));
    var result = _repository.Find(tagId);
    Assert.NotNull(result);
    Assert.Equal("tag-navn-1", result.Name);
  }

  [Fact]
  public void find_non_existing_tag_returns_null() {
    var result = _repository.Find(1);
    Assert.Null(result);
  }

  [Fact]
  public void update_existing_tag_returns_Updated(){
    var (res, id) = _repository.Create(new TagCreateDTO("tag-navn-1"));
    var updateResponse = _repository.Update(new TagUpdateDTO(id, "nyt-tag-navn-1"));

    var updatedName = _context.Tags.Find(id)!.Name;
    Assert.Equal("nyt-tag-navn-1", updatedName);
    Assert.Equal(Response.Updated, updateResponse);
  }

  [Fact]
  public void update_nonexisting_tag_returns_Updated(){
    var updateResponse = _repository.Update(new TagUpdateDTO(1, "nyt-tag-navn-1"));

    Assert.Equal(Response.NotFound, updateResponse);
  }

  public void Dispose() {
    _context.Dispose();
  }

}
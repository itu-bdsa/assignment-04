using Assignment.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Assignment;

internal class kanbanFactory : IDesignTimeDbContextFactory<KanbanContext>
{
  public KanbanContext CreateDbContext(string[] args)
  {
    var configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
    var connectionString = configuration.GetConnectionString("kanban");

    var optionsBuilder = new DbContextOptionsBuilder<KanbanContext>();
    optionsBuilder.UseNpgsql(connectionString);

    return new KanbanContext(optionsBuilder.Options);
  }
}

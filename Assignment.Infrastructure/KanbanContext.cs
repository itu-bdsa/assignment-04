using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Assignment.Infrastructure
{
  public partial class KanbanContext : DbContext
  {
    public KanbanContext()
    {
    }

    public KanbanContext(DbContextOptions<KanbanContext> options)
        : base(options)
    {
    }

    public virtual DbSet<WorkItem> WorkItems => Set<WorkItem>();
    public virtual DbSet<Tag> Tags => Set<Tag>();
    public virtual DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder
            .Entity<WorkItem>()
            .Property(e => e.State)
            .HasConversion(new EnumToStringConverter<State>());
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
  }
}

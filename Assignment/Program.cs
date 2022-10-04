using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Assignment.Infrastructure;
using Microsoft.EntityFrameworkCore;

string _conStr = @"
    Server=0.0.0.0:1433;
    Database=musing-keldysh;
    User Id=SA;
    Password=<YourStrong@Passw0rd>;";

var optionsBuilder = new DbContextOptionsBuilder<KanbanContext>();
optionsBuilder.UseSqlServer(_conStr);

var context = new KanbanContext(optionsBuilder.Options);
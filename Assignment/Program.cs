using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var connectionString = configuration.GetConnectionString("kanban");

using var con = new NpgsqlConnection(connectionString);
con.Open();

var sql = "SELECT version()";

using var cmd = new NpgsqlCommand(sql, con);

var version = cmd.ExecuteScalar()!.ToString();
Console.WriteLine($"PostgreSQL version: {version}");

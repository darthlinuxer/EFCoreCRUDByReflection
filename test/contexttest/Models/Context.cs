using System;
using Microsoft.EntityFrameworkCore;

namespace Models;

public class Context : DbContext
{
    public string DbName;
    public Context(DbContextOptions<Context> options) : base(options)
    {
    }
    public Context(string dbName)
    {
        this.DbName = dbName;
    }

    public DbSet<Person> Persons { get; set; }
    public DbSet<PersonWithoutKey> PersonsWithoutKey { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase(this.DbName);
        optionsBuilder.LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name });
    }

}
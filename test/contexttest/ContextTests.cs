using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;
using Newtonsoft.Json;
using Universal.Context;

namespace contexthandler;

[TestClass]
public partial class ContextTests
{
    private Context _context;
    private UniversalContext _service;
    static int dbNumber;
    private readonly ILogger log;
    private readonly ILoggerFactory logFactory;
    public ContextTests()
    {
        logFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        log = this.logFactory.CreateLogger<ContextTests>();
    }

    [TestInitialize]
    public void TestInitialization()
    {
        _context = new Context($"db{dbNumber}");
        _service = new UniversalContext(_context);
        _context.Persons.AddRange(new Person[]
        {
            new Person(){Name="Anakin",Surname="SkyWalker"},
            new Person(){Name="Luke", Surname="SkyWalker"}
        });
        _context.PersonsWithoutKey.AddRange(new PersonWithoutKey[]
       {
            new PersonWithoutKey(){Name="Anakin",Surname="SkyWalker"},
            new PersonWithoutKey(){Name="Luke", Surname="SkyWalker"}
       });
        _context.SaveChanges();
        log.LogInformation("Init called on database db{a}", dbNumber);
        dbNumber++;
    }

    [TestMethod]
    public void RawSqlQuery()
    {
        var query = "select * from Persons";
        var results = _service.RawSqlQuery(
            query,
            x =>
            {
                var obj = new ExpandoObject() as IDictionary<string, object>;
                var columns = x.GetColumnSchemaAsync().GetAwaiter().GetResult();
                var i = 0;
                while (i < columns.Count())
                {
                    obj.Add(x.GetName(i), x.GetValue(i));
                    i++;
                }
                return obj;
            },
            System.Data.CommandType.Text);
        Assert.IsNotNull(results);
    }

    [TestMethod]
    public void Add()
    {
        var leia = new Person() { Name = "Leia", Surname = "Skywalker" };
        Person addedPerson = _service.Add<Person>(leia) as Person;
        _service.Save();
        var persons = _service.GetAll<Person>("Name", 1, 10, false)!.ToList();
        Assert.IsTrue(persons.Count == 3);
        Assert.IsTrue(persons.Contains(leia));
        Assert.IsTrue(addedPerson.Name == "Leia");
    }
    [TestMethod]
    public void Add2()
    {
        var leia = new Person() { Name = "Leia", Surname = "Skywalker" };
        Person addedPerson = _service.Add("Persons", leia) as Person;
        _service.Save();
        var persons = _service.GetAll("Persons", "Name", 1, 10, false)!.ToList();
        Assert.IsTrue(persons.Count == 3);
        Assert.IsTrue(persons.Contains(leia));
        Assert.IsTrue(addedPerson.Name == "Leia");
    }

    [TestMethod]
    public void RemoveWithExpression()
    {
        _service.Remove<Person>(p => p.Name == "Anakin");
        _service.Save();
        var persons = _service.GetAll<Person>("Name", 1, 10, false)!.ToList();
        Assert.IsTrue(persons.Count == 1);
    }

    [TestMethod]
    public void RemoveWithExpression2()
    {
        _service.Remove("Persons", "Surname == \"SkyWalker\"");
        _service.Save();
        var persons = _service.GetAll("Persons").ToList();
        Assert.IsTrue(persons.Count == 0);
    }

    [TestMethod]
    public void RemoveWithObject()
    {
        var luke = _service.Get<Person>(p => p.Name == "Luke");
        _service.Remove<Person>(luke!);
        _service.Save();
        var persons = _service.GetAll<Person>("Name", 1, 10, false)!.ToList();
        Assert.IsTrue(persons.Count == 1);
    }

    [TestMethod]
    public void RemoveWithObject2()
    {
        var luke = _service.Get("Persons", "Name == \"Luke\"");
        _service.Remove("Persons", luke!);
        _service.Save();
        var persons = _service.GetAll("Persons", "Name", 1, 10, false)!.ToList();
        Assert.IsTrue(persons.Count == 1);
    }

    [TestMethod]
    public void Update()
    {
        var anakin = _service.Get<Person>(p => p.Name == "Anakin");
        anakin!.Name = "Darth";
        anakin!.Surname = null;
        var updatedPerson = _service.Update<Person>(anakin);
        _service.Save();
        var vader = _service.Get<Person>(p => p.Name == "Darth");
        Assert.IsTrue(vader is not null);
        Assert.IsTrue(updatedPerson.Name == "Darth");
    }

    [TestMethod]
    public void Update1()
    {
        Person anakin = _service.Get("Persons", "Name == \"Anakin\"") as Person;
        anakin!.Surname = "Vader";
        var updatedPerson = _service.Update("Persons", anakin!) as Person;
        _service.Save();
        var vader = _service.Get("Persons", "Surname == \"Vader\"");
        Assert.IsTrue(vader is not null);
        Assert.IsTrue(updatedPerson!.Name == "Anakin");
    }
    [TestMethod]
    public void Update2()
    {
        Person anakin = _service.Get("Persons", "Name == \"Anakin\"") as Person;
        Person vader = new() { id = anakin!.id, Surname = "Vader" };
        var updatedPerson = _service.Update("Persons", vader) as Person;
        _service.Save();
        var vaderInDb = _service.Get("Persons", "Surname == \"Vader\"");
        Assert.IsTrue(vaderInDb is not null);
        Assert.IsTrue(updatedPerson!.Name is null);
    }

    [TestMethod]
    public void Update3()
    {
        Person anakin = _service.Get("Persons", "Name == \"Anakin\"") as Person;
        Person vader = new() { Surname = "Vader" };
        var updatedPerson = _service.Update("Persons", anakin, vader) as Person;
        _service.Save();
        var vaderInDb = _service.Get("Persons", "Surname == \"Vader\"");
        Assert.IsTrue(vaderInDb is not null);
        Assert.IsTrue(updatedPerson!.Name is null);
    }

    //Anonymous object
    [TestMethod]
    public void Update4()
    {
        PersonWithoutKey anakin = _service.Get("PersonsWithoutKey", "Name == \"Anakin\"") as PersonWithoutKey;
        //vader is an anonymous object and have no KeyAttributes
        var vader = new { Id = anakin!.id, SurName = "Vader" };
        var updatedPerson = _service.Update("PersonsWithoutKey", vader, new[] { "id" }) as PersonWithoutKey;
        _service.Save();
        var vaderInDb = _service.Get("PersonsWithoutKey", "Surname == \"Vader\"") as PersonWithoutKey;
        Assert.IsTrue(vaderInDb is not null);
        Assert.IsTrue(vaderInDb!.Name == "Anakin");
        Assert.IsTrue(vaderInDb!.Surname == "Vader");
        Assert.IsTrue(updatedPerson!.Name == "Anakin");
        Assert.IsTrue(updatedPerson!.Surname == "Vader");
    }

    //Anonymous object with null values
    [TestMethod]
    public void Update5()
    {
        PersonWithoutKey anakin = _service.Get("PersonsWithoutKey", "Name == \"Anakin\"") as PersonWithoutKey;
        //vader is an anonymous object and have no KeyAttributes
        //Beware that passing null properties they are updated also
        var vader = new PersonWithoutKey { id = anakin!.id, Name = null, Surname = "Vader" };
        var updatedPerson = _service.Update("PersonsWithoutKey", vader, new[] { "id" }) as PersonWithoutKey;
        _service.Save();
        var vaderInDb = _service.Get("PersonsWithoutKey", "Surname == \"Vader\"") as PersonWithoutKey;
        Assert.IsTrue(vaderInDb is not null);
        Assert.IsTrue(vaderInDb!.Name == null);
        Assert.IsTrue(vaderInDb!.Surname == "Vader");
        Assert.IsTrue(updatedPerson!.Name == null);
        Assert.IsTrue(updatedPerson!.Surname == "Vader");
    }

    //Anonymous object with null values
    [TestMethod]
    public void Update6()
    {
        PersonWithoutKey anakin = _service.Get("PersonsWithoutKey", "Name == \"Anakin\"") as PersonWithoutKey;
        //vader is an anonymous object and have no KeyAttributes
        //Beware that passing null properties they are updated also
        dynamic vader = new ExpandoObject();
        vader.id = anakin!.id;
        vader.Surname = "Vader";
        var updatedPerson = _service.Update("PersonsWithoutKey", vader, new[] { "id" }) as PersonWithoutKey;
        _service.Save();
        var vaderInDb = _service.Get("PersonsWithoutKey", "Surname == \"Vader\"") as PersonWithoutKey;
        Assert.IsTrue(vaderInDb is not null);
        Assert.IsTrue(vaderInDb!.Name == "Anakin");
        Assert.IsTrue(vaderInDb!.Surname == "Vader");
        Assert.IsTrue(updatedPerson!.Name == "Anakin");
        Assert.IsTrue(updatedPerson!.Surname == "Vader");
    }

    [TestMethod]
    public void Update7()
    {
        var anakin = new { id = 1, Surname = "Vader" };
        var anakinJson = JsonConvert.SerializeObject(anakin);
        using JsonDocument document = JsonDocument.Parse(anakinJson);
        var anakinElement = document.RootElement;
        var updatedPerson = _service.Update("PersonsWithoutKey", anakinElement, new[] { "id" }) as PersonWithoutKey;
        _service.Save();
        var vaderInDb = _service.Get("PersonsWithoutKey", "Surname == \"Vader\"") as PersonWithoutKey;
        Assert.IsTrue(vaderInDb is not null);
        Assert.IsTrue(vaderInDb!.Name == "Anakin");
        Assert.IsTrue(vaderInDb!.Surname == "Vader");
        Assert.IsTrue(updatedPerson!.Name == "Anakin");
        Assert.IsTrue(updatedPerson!.Surname == "Vader");
    }


    [TestMethod]
    public void Query()
    {
        var lukeQuery = _service.Query<Person>("Name == \"Luke\"");
        Assert.IsTrue(lukeQuery!.Count() == 1);
    }

    [TestMethod]
    public void Query2()
    {
        var lukeQuery = _service.Query("Persons", "Name == \"Luke\"");
        Assert.IsTrue(lukeQuery!.Count() == 1);
    }

    [TestMethod]
    public void TestDbSetType()
    {
        var dbSetType = _service.DbSetUnderlyingType("Persons");
        Assert.IsTrue(dbSetType == typeof(Person));
    }

    [TestMethod]
    public void GetAll()
    {
        var collection = _service.GetAll("Persons");
        Assert.IsTrue(collection?.Count() == 2);
    }

    [TestMethod]
    public void Clone()
    {
        object luke = new Person() { Name = "Luke" };
        object skywalker = new Person() { Surname = "Skywalker" };

        UniversalContext.Copy(ref luke, skywalker);
        Assert.IsTrue(((Person)luke).Surname == "Skywalker");

        UniversalContext.Copy(ref luke, skywalker);
        Assert.IsTrue(((Person)luke).Name is null);
    }

}
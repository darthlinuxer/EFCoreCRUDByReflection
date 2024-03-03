using System.Collections.Generic;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;

namespace contexthandler;

[TestClass]
public partial class ContextTests
{

    //TEST WON´T WORK WITH INMEMORY COLLECTIONS
    //UNCOMMENT AND RUN TEST WITH A REAL DATABASE
    // [TestMethod]
    // public async Task RawSqlQueryAsync()
    // {
    //     var query = "select * from Persons";
    //     var results = await _service.RawSqlQueryAsync(
    //         query,
    //         x =>
    //         {
    //             var obj = new ExpandoObject() as IDictionary<string, object>;
    //             var columns = x.GetColumnSchemaAsync().GetAwaiter().GetResult();
    //             var i = 0;
    //             while (i < columns.Count())
    //             {
    //                 obj.Add(x.GetName(i), x.GetValue(i));
    //                 i++;
    //             }
    //             return obj;
    //         },
    //         CancellationToken.None, System.Data.CommandType.Text);
    //     Assert.IsNotNull(results);
    // }

    [TestMethod]
    public async Task AddAsyncTyped()
    {
        var leia = new Person() { Name = "Leia", Surname = "Skywalker" };
        Person addedPerson = (Person)await _service.AddAsync<Person>(leia, CancellationToken.None);
        _service.Save();
        var persons = _service.GetAllAsync<Person>(orderby: "Name", page: 1, count: 10, descending: false, asNoTracking: false);

        bool thereIsALeiaPerson = false;
        bool theNameOfTheAddedPersonIsLeia = false;
        int personCount = 0;
        await foreach (var person in persons)
        {
            personCount++;
            if (person == addedPerson) thereIsALeiaPerson = true;
            if (person.Name == "Leia") theNameOfTheAddedPersonIsLeia = true;
        }
        Assert.IsTrue(personCount == 3);
        Assert.IsTrue(thereIsALeiaPerson);
        Assert.IsTrue(theNameOfTheAddedPersonIsLeia);
    }

    [TestMethod]
    public async Task AddAsync()
    {
        var leia = new Person() { Name = "Leia", Surname = "Skywalker" };
        Person addedPerson = (Person)await _service.AddAsync(dbSetName: "Persons", leia, CancellationToken.None);
        await _service.SaveAsync();
        var persons = _service.GetAllAsync(dbSetName: "Persons", orderby: "Name", page: 1, count: 10, descending: false, asNoTracking: false);

        bool thereIsALeiaPerson = false;
        bool theNameOfTheAddedPersonIsLeia = false;
        int personCount = 0;
        await foreach (var person in persons)
        {
            personCount++;
            if ((Person)person == addedPerson) thereIsALeiaPerson = true;
            if (((Person)person).Name == "Leia") theNameOfTheAddedPersonIsLeia = true;
        }
        Assert.IsTrue(personCount == 3);
        Assert.IsTrue(thereIsALeiaPerson);
        Assert.IsTrue(theNameOfTheAddedPersonIsLeia);
    }

    [TestMethod]
    public async Task AddBulkAsync_WithKnownTypes()
    {
        var leia = new Person() { Name = "Leia", Surname = "Skywalker" };
        var hanSolo = new Person { Name = "Han", Surname = "Solo"};
        var itemsToAdd = new List<object>(){
            leia, hanSolo
        };

        await _service.AddBulkAsync<Person>(itemsToAdd, CancellationToken.None);
        await _service.SaveAsync();
        
        var persons = _service.GetAllAsync(dbSetName: "Persons", orderby: "Name", page: 1, count: 10, descending: false, asNoTracking: false);

        bool thereIsALeiaPerson = false;
        bool theNameOfTheAddedPersonIsLeia = false;
        bool thereIsAHanSoloPerson = false;
        int personCount = 0;
        await foreach (var person in persons)
        {
            personCount++;
            if ((Person)person == leia) thereIsALeiaPerson = true;
            if (((Person)person).Name == "Leia") theNameOfTheAddedPersonIsLeia = true;
            if (person == hanSolo) thereIsAHanSoloPerson = true;
        }
        Assert.IsTrue(personCount == 4);
        Assert.IsTrue(thereIsALeiaPerson);
        Assert.IsTrue(theNameOfTheAddedPersonIsLeia);
        Assert.IsTrue(thereIsAHanSoloPerson);
    }

     [TestMethod]
    public async Task AddBulkAsync_WithTypesAsString()
    {
        var leia = new Person() { Name = "Leia", Surname = "Skywalker" };
        var hanSolo = new Person { Name = "Han", Surname = "Solo"};
        var itemsToAdd = new List<object>(){
            leia, hanSolo
        };

        await _service.AddBulkAsync("Persons", itemsToAdd, CancellationToken.None);
        await _service.SaveAsync();
        
        var persons = _service.GetAllAsync(dbSetName: "Persons", orderby: "Name", page: 1, count: 10, descending: false, asNoTracking: false);

        bool thereIsALeiaPerson = false;
        bool theNameOfTheAddedPersonIsLeia = false;
        bool thereIsAHanSoloPerson = false;
        int personCount = 0;
        await foreach (var person in persons)
        {
            personCount++;
            if ((Person)person == leia) thereIsALeiaPerson = true;
            if (((Person)person).Name == "Leia") theNameOfTheAddedPersonIsLeia = true;
            if (person == hanSolo) thereIsAHanSoloPerson = true;
        }
        Assert.IsTrue(personCount == 4);
        Assert.IsTrue(thereIsALeiaPerson);
        Assert.IsTrue(theNameOfTheAddedPersonIsLeia);
        Assert.IsTrue(thereIsAHanSoloPerson);
    }

    [TestMethod]
    public async Task GetAllAsyncTyped()
    {
        var collection = _service.GetAllAsync<Person>("id", 1, 10, true);
        var counter = 0;
        await foreach (var person in collection)
        {
            counter++;
        }
        Assert.IsTrue(counter == 2);
    }

    [TestMethod]
    public async Task GetAllAsyncTypedWithIncludes()
    {
        var collection = _service.GetAllAsync<Person>("id", 1, 10, true, true, "Books");
        var counter = 0;
        bool thereIsAnAnakinPerson = false;
        bool whoReadThreeBooks = false;
        await foreach (var person in collection)
        {
            if (person.Name == "Anakin") thereIsAnAnakinPerson = true;
            if (person.Books?.Count == 3) whoReadThreeBooks = true;
            counter++;
        }
        Assert.IsTrue(counter == 2);
        Assert.IsTrue(thereIsAnAnakinPerson);
        Assert.IsTrue(whoReadThreeBooks);
    }

    [TestMethod]
    public async Task GetAsyncTyped()
    {
        var luke = await _service.GetAsync<Person>(c => c.Name == "Luke", CancellationToken.None);
        Assert.IsTrue(luke.Name == "Luke");
    }

    [TestMethod]
    public async Task GetAsyncTypedWithIncludes()
    {
        var anakin = await _service.GetAsync<Person>(c => c.Name == "Anakin", CancellationToken.None, true, "Books", "Address");
        Assert.IsTrue(anakin.Name == "Anakin");
        Assert.IsTrue(anakin.Books?.Count == 3);
        Assert.IsTrue(anakin.Address is not null);
    }

    [TestMethod]
    public async Task GetAsync()
    {
        object? luke = await _service.GetAsync("Persons", "Name==\"Luke\"", CancellationToken.None);
        Assert.IsTrue((luke as Person)?.Name == "Luke");
    }

    [TestMethod]
    public async Task GetAsyncWithIncludes()
    {
        object? anakin = await _service.GetAsync("Persons", "Name==\"Anakin\"", CancellationToken.None, asNoTracking: true, "Books", "Address");
        Assert.IsTrue((anakin as Person)?.Name == "Anakin");
        Assert.IsTrue((anakin as Person)?.Address?.City == "Capitol");
    }

    [TestMethod]
    public async Task GetFilteredTypeAsync()
    {
        var personCollectionAsyncEnumerable = _service.GetAllFilteredAsync<Person>(where: "Name==\"Luke\"", orderby: "Name", page: 1, count: 10, descending: true);
        await foreach (var person in personCollectionAsyncEnumerable)
        {
            Assert.IsTrue(person.Name == "Luke");
        }
    }

    [TestMethod]
    public async Task GetFilteredTypeAsyncWithIncludes()
    {
        var personCollectionAsyncEnumerable = _service.GetAllFilteredAsync<Person>(where: "Name==\"Anakin\"", orderby: "Name", page: 1, count: 10, descending: true, asNoTracking: true, includeNavigationNames: "Books");
        await foreach (var person in personCollectionAsyncEnumerable)
        {
            Assert.IsTrue(person.Books?.Count == 3);
            Assert.IsTrue(person.Name == "Anakin");
        }
    }

    [TestMethod]
    public async Task GetFilteredType()
    {
        var personCollectionAsyncEnumerable = _service.GetAllFilteredAsync(dbSetName: "Persons", where: "Name==\"Luke\"", orderby: "Name", page: 1, count: 10, descending: true);
        await foreach (var person in personCollectionAsyncEnumerable)
        {
            Assert.IsTrue((person as Person).Name == "Luke");
        }
    }

    [TestMethod]
    public async Task GetFilteredTypeWithIncludes()
    {
        var personCollectionAsyncEnumerable = _service.GetAllFilteredAsync(dbSetName: "Persons", where: "Name==\"Anakin\"", orderby: "Name", page: 1, count: 10, descending: true, asNoTracking: true, includeNavigationNames: "Books");
        await foreach (var person in personCollectionAsyncEnumerable)
        {
            Assert.IsTrue((person as Person).Books?.Count == 3);
            Assert.IsTrue((person as Person).Name == "Anakin");
        }
    }

    [TestMethod]
    public async Task GetAllFilteredCancellableTypedAsyncWithIncludes()
    {
        var ct = new CancellationToken();
        var personCollectionAsync = _service.GetAllFilteredCancellableAsync<Person>(
            where: "Id > 0",
            orderby: "Name",
            page: 1,
            count: 10,
            descending: true,
            ct,
            asNoTracking: true,
            includeNavigationNames: "Books");

        var personList = new List<Person>();
        await foreach (var person in personCollectionAsync.WithCancellation(ct))
        {
            if(ct.IsCancellationRequested) break;
            personList.Add(person!);            
        }
        Assert.IsTrue(personList.Count==2);
    }


    //Anonymous object with null values
    [TestMethod]
    public async Task UpdateAsyncExpandoObject()
    {
        PersonWithoutKey? anakin = await _service.GetAsync("PersonsWithoutKey", "Name == \"Anakin\"", CancellationToken.None) as PersonWithoutKey;
        //vader is an anonymous object and have no KeyAttributes
        //Beware that passing null properties they are updated also
        dynamic vader = new ExpandoObject();
        vader.id = anakin!.id;
        vader.Surname = "Vader";
        var updatedPerson = await _service.UpdateAsync("PersonsWithoutKey", vader, CancellationToken.None, new[] { "id" }) as PersonWithoutKey;
        await _service.SaveAsync();
        var vaderInDb = await _service.GetAsync("PersonsWithoutKey", "Surname == \"Vader\"", CancellationToken.None) as PersonWithoutKey;
        Assert.IsTrue(vaderInDb is not null);
        Assert.IsTrue(vaderInDb!.Name == "Anakin");
        Assert.IsTrue(vaderInDb!.Surname == "Vader");
        Assert.IsTrue(updatedPerson!.Name == "Anakin");
        Assert.IsTrue(updatedPerson!.Surname == "Vader");
    }

}
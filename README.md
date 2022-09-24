# EFCoreCRUDByReflection

```
using Universal.Context;

namespace MyControllers;
public class BodyModel
{
    public string? Where { get; set; }
    public string? OrderBy { get; set; }
    public bool? Descending { get; set; }
    public int? Page { get; set; }
    public int? ItemsPerPage { get; set; }
}

[ApiController]
[Route("[controller]")]
public class MyController : ControllerBase
{
    private readonly UniversalContext _service;

    public MyController(
        MyEFContext context)
    {
        _service = new UniversalContext(context);
    }

    [HttpGet]
    [Route("{dbSetName}/[action]")]
    public IEnumerable<object>? GetAllPaginated([FromRoute] string dbSetName, [FromBody] BodyModel body)
    {
        IEnumerable<object>? elements = _service.GetAll(dbSetName: dbSetName, orderby: body?.OrderBy ?? "id", page: body?.Page ?? 1, count: body?.ItemsPerPage ?? 10, descending: body?.Descending ?? false)?.ToList();
        return elements;
    }

      [HttpGet]
    [Route("{dbSetName}/[action]")]
    public IEnumerable<object>? GetAll([FromRoute] string dbSetName)
    {
        IQueryable<object>? elements = _service.GetAll(dbSetName);
        return elements;
    }

    [HttpGet]
    [Route("{dbSetName}/[action]")]
    public IEnumerable<object>? GetAllFiltered([FromRoute] string dbSetName, [FromQuery] string where)
    {
        var elements = _service.GetAll(dbSetName: dbSetName, where: where, orderby: "id", page: 1, count: 10, descending: false)?.ToList();
        return elements;
    }

    [HttpGet]
    [Route("{dbSetName}/[action]")]
    public object? Get([FromRoute] string dbSetName, [FromQuery] string where)
    {
        var element = _service.Get(dbSetName: dbSetName, where: where);
        return element;
    }

    [HttpPost]
    [Route("{dbSetName}/[action]")]
    public object? Add([FromRoute] string dbSetName, [FromBody] object body)
    {
        var dbSetType = _service.DbSetUnderlyingType(dbSetName: dbSetName);
        var obj = UniversalContext.ConvertTo(t: dbSetType, item: (JsonElement)body);
        var addedElement = _service.Add(dbSetName: dbSetName, obj: obj);
        _service.Save();
        return addedElement;
    }

     [HttpPost]
    [Route("{dbSetName}/[action]")]
    public object? Update([FromRoute] string dbSetName, [FromBody] JsonElement body, [FromQuery] params string[] key)
    {
        var updatedElement = _service.Update(dbSetName, body, key);
        _service.Save();
        return updatedElement;
    }

    [HttpPost]
    [Route("{dbSetName}/[action]")]
    public IEnumerable<object> Remove([FromRoute] string dbSetName, [FromQuery] string where)
    {
        var removedElement = _service.Remove(dbSetName: dbSetName, where: where);
        _service.Save();
        return removedElement;
    }

}
```
This library enables the user to work with any Context.  
**Where** clauses are the same as <https://dynamic-linq.net/>  
The advantage is to enable the possibility to perform CRUDs on any DbSet by calling it with a string name.  
Fell free to contribute!

## Postman Calls

| CRUD | Body | Query |
|------|------|-------|
| Update | {"id":1, "field":"xxx" } | key = id |
  
Other CRUD Methods can be seen in the insomnia example file added to this project!  
**Enjoy!**
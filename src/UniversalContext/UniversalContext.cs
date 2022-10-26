namespace Universal.Context;
public partial class UniversalContext
{
    public DbContext Context { get; init; }
    public UniversalContext(DbContext context)
    {
        this.Context = context;
    }

    //USAGE 
    //var result = Helper.RawSqlQuery(
    //     "SELECT TOP 10 Name, COUNT(*) FROM Users U"
    //     + " INNER JOIN Signups S ON U.UserId = S.UserId"
    //     + " GROUP BY U.Name ORDER BY COUNT(*) DESC",
    //     x => new TopUser { Name = (string)x[0], Count = (int)x[1] });
    public List<T> RawSqlQuery<T>(string query, Func<DbDataReader, T> map, CommandType commandType = CommandType.Text)
    {
        using var context = Context;
        using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = query;
        command.CommandType = commandType;
        context.Database.OpenConnection();
        using var result = command.ExecuteReader();
        var entities = new List<T>();

        while (result.Read()) { entities.Add(map(result)); }
        return entities;
    }

    public DbDataReader RawSqlQuery(string query, CommandType commandType = CommandType.Text)
    {
        using var context = Context;
        using var command = Context.Database.GetDbConnection().CreateCommand();
        command.CommandText = query;
        command.CommandType = commandType;
        context.Database.OpenConnection();
        using var result = command.ExecuteReader();
        return result;
    }

    public object Add<T>(object obj) where T : class
    {
        var convertedObj = (T)obj;
        Context.Add(convertedObj);
        return convertedObj;
    }

    public object Add(string dbSetName, object obj)
    {
        var dbSetType = DbSetUnderlyingType(dbSetName);
        if (dbSetType is null) throw new Exception("DbSet type cannot be null!");
        var convertedObj = Convert.ChangeType(obj, dbSetType);
        Context.Add(convertedObj);
        return convertedObj;
    }
    public async Task<object> AddAsync<T>(object obj) where T : class
    {
        var convertedObj = (T)obj;
        await Context.AddAsync(convertedObj);
        return convertedObj;
    }

    public async Task<object> AddAsync(string dbSetName, object obj)
    {
        var dbSetType = DbSetUnderlyingType(dbSetName);
        if (dbSetType is null) throw new Exception("DbSet type cannot be null!");
        var convertedObj = Convert.ChangeType(obj, dbSetType);
        await Context.AddAsync(convertedObj);
        return convertedObj;
    }
    public object Remove<T>(object obj) where T : class
    {
        var convertedObj = (T)obj;
        Context.Set<T>().Remove(convertedObj);
        return convertedObj;
    }

    public object Remove(string dbSetName, object obj)
    {
        var dbSetType = DbSetUnderlyingType(dbSetName);
        if (dbSetType is null) throw new Exception("DbSet type cannot be null!");
        var convertedObj = Convert.ChangeType(obj, dbSetType);
        Context.Remove(convertedObj);
        return convertedObj;
    }
    public IEnumerable<object> Remove<T>(Expression<Func<T, bool>> p) where T : class
    {
        try
        {
            var pred = p.Compile();
            var objs = Context.Set<T>().Where(p);
            Context.Set<T>().RemoveRange(objs);
            return objs;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public IEnumerable<object> Remove(string dbSetName, string where)
    {
        try
        {
            var objs = Query(dbSetName, where);
            var dbSetType = DbSetUnderlyingType(dbSetName);
            if (dbSetType is null) throw new Exception("dbSetType cannot be null!");
            List<object> convertedObjects = new();
            foreach (var obj in objs?.AsEnumerable<object>())
            {
                convertedObjects.Add(Convert.ChangeType(obj, dbSetType));
            }
            Context.RemoveRange(convertedObjects);
            return convertedObjects;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public T? Get<T>(Expression<Func<T, bool>> p) where T : class
    {
        try
        {
            var pred = p.Compile();
            return Context.Set<T>().Where(p).AsEnumerable<T>().SingleOrDefault();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public object? Get(string dbSetName, string where)
    {
        try
        {
            var collection = Query(dbSetName, where);
            return collection?.SingleOrDefault();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public object? Find(Type entityType, params object?[] keys) => Context.Find(entityType, keys);
    public T Update<T>(object obj) where T : class
    {
        var convertedObj = (T)obj;
        Context.Set<T>().Update(convertedObj);
        return convertedObj;
    }

    private object Update(Type dbSetType, object target, object source, IEnumerable<string>? keyNames = null)
    {
        if (target.GetType() != dbSetType)
        {
            target = Convert.ChangeType(target, dbSetType);
            if (target is null) throw new Exception("Update error! Target cannot be null");
        }
        Context.Entry(target).State = EntityState.Modified;
        Copy(ref target, source, keyNames);
        return target;
    }

    private object Update(Type dbSetType, object target, ExpandoObject source, IEnumerable<string>? keyNames = null)
    {
        Context.Entry(target).State = EntityState.Modified;
        foreach (var item in source as IDictionary<string, object>)
        {
            if (keyNames?.Contains(item.Key) ?? false) continue;
            var targetProp = target.GetType().GetProperty(item.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase);
            if (targetProp is null) throw new Exception("Keynames do not belong to target object");
            Type t = Nullable.GetUnderlyingType(targetProp!.PropertyType) ?? targetProp!.PropertyType;
            var convertedValue = Convert.ChangeType(item.Value, t);
            targetProp.SetValue(target, convertedValue);
        }
        return target;
    }

    public object Update(string dbSetName, object target, object source, IEnumerable<string>? keyNames = null)
    {
        var dbSetType = DbSetUnderlyingType(dbSetName);
        if (dbSetType is null) throw new Exception("dbSetType cannot be null!");
        return Update(dbSetType, target, source, keyNames);
    }

    public object Update(string dbSetName, object sourceWithId, IEnumerable<string>? keyNames = null)
    {
        try
        {
            var dbSetType = DbSetUnderlyingType(dbSetName);
            if (dbSetType is null) throw new Exception("dbSetType cannot be null!");
            var keys = GetKeysValues(sourceWithId, keyNames);
            var targetInDb = Find(dbSetType, keys?.ToArray()!)!;
            if (targetInDb is null) throw new Exception("Body object with provided Keys not found in Database");
            return Update(dbSetType, targetInDb, sourceWithId, keyNames);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public object Update(string dbSetName, ExpandoObject sourceWithId, IEnumerable<string>? keyNames = null)
    {
        try
        {
            var dbSetType = DbSetUnderlyingType(dbSetName);
            if (dbSetType is null) throw new Exception("dbSetType cannot be null!");
            var keys = GetKeysValues(sourceWithId, keyNames);
            var targetInDb = Find(dbSetType, keys?.ToArray()!)!;
            if (targetInDb is null) throw new Exception("body object with provided Keys not found in Database");
            return Update(dbSetType, targetInDb, sourceWithId, keyNames);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public object Update(string dbSetName, JsonElement sourceWithId, IEnumerable<string> keyNames)
    {
        try
        {
            var dbSetType = DbSetUnderlyingType(dbSetName);
            var sourceWithIdAsJson = sourceWithId.GetRawText();
            var expandoElement = JsonConvert.DeserializeObject<ExpandoObject>(sourceWithIdAsJson);
            var keyValues = GetKeysValues(expandoElement, keyNames);
            var whereClauses = keyNames.Zip(keyValues, (x, y) => $"{x} == {y}");
            var where = whereClauses.Aggregate((x, y) =>
            {
                if (y is not null) return $"{x} && {y}";
                return x;
            });
            var targetInDb = Query(dbSetName, where).Single();
            if (targetInDb is null) throw new Exception("body object with provided Keys not found in Database");
            return Update(dbSetType, targetInDb, expandoElement, keyNames);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }


    public IEnumerable<T>? GetAll<T>(string orderby, int page, int count, bool descending) where T : class
    {
        try
        {
            string direction = descending ? "desc" : "asc";
            return Context.Set<T>().OrderBy($"{orderby} {direction}").Skip((page - 1) * count).Take(count).AsEnumerable<T>();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public IQueryable<object>? GetAll(string dbSetName, string orderby, int page, int count, bool descending)
    {
        try
        {
            var prop = GetProperty(dbSetName);
            IQueryable<object>? collection = prop?.GetGetMethod()?.Invoke(Context, null) as IQueryable<object>;
            string direction = descending ? "desc" : "asc";
            return collection?.OrderBy($"{orderby} {direction}").Skip((page - 1) * count).Take(count);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public IEnumerable<T>? GetAll<T>(string where, string orderby, int page, int count, bool descending) where T : class
    {
        try
        {
            string direction = descending ? "desc" : "asc";
            return Context.Set<T>().Where(where).OrderBy($"{orderby} {direction}").Skip((page - 1) * count).Take(count).AsEnumerable<T>();
        }
        catch (Exception)
        {
            throw;
        }
    }
    public IEnumerable<object>? GetAll(string dbSetName, string where, string orderby, int page, int count, bool descending)
    {
        try
        {
            string direction = descending ? "desc" : "asc";
            return Query(dbSetName, where)?.OrderBy($"{orderby} {direction}").Skip((page - 1) * count).Take(count).AsEnumerable<object>();
        }
        catch (Exception)
        {
            throw;
        }
    }
    public IQueryable<object>? GetAll(string dbSetName)
    {
        var prop = GetProperty(dbSetName);
        return prop?.GetGetMethod()?.Invoke(Context, null) as IQueryable<object>;
    }
    public IQueryable<object>? Query<T>(string where) where T : class => Context.Set<T>().Where(where);
    public IQueryable<object>? Query(string dbSetName, string where)
    {
        var prop = GetProperty(dbSetName);
        IQueryable<object>? collection = prop?.GetGetMethod()?.Invoke(Context, null) as IQueryable<object>;
        return collection?.Where(where);
    }

}
namespace Universal.Context;
public partial class UniversalContext
{
    public DbContext Context { get; init; }
    private Logger _log;
    public UniversalContext(DbContext context)
    {
        this.Context = context;
    }

    public UniversalContext(DbContext context, Logger log)
    {
        this.Context = context;
        this._log = log;
        _log.Verbose("Constructor: Universal Context created with DbContext: {@a}", context);
    }

    public object? Find(Type entityType, params object?[] keys)
    {
        _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
        var result = Context.Find(entityType, keys);
        _log?.Debug("return: {@a}", result);
        return result;
    }

    public IQueryable<T> Query<T>() where T : class
    {
        _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
        var result = Context.Set<T>().AsQueryable<T>();
        _log?.Debug("return: {@a}", result);
        return result;
    }
    public IQueryable<object>? Query(string dbSetName)
    {
        try
        {
            _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
            var prop = GetProperty(dbSetName);
            IQueryable<object>? collection = prop?.GetGetMethod()?.Invoke(Context, null) as IQueryable<object>;
            _log?.Debug("return: {@a}", collection);
            return collection;
        }
        catch (Exception ex)
        {
            _log?.Error("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, ex);
            throw;
        }
    }
    public IQueryable<object>? QueryFiltered<T>(string where) where T : class
    {
        _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
        var result = Context.Set<T>().Where(where);
        _log?.Debug("return: {@a}", result);
        return result;
    }
    public IQueryable<object>? QueryFiltered(string dbSetName, string where)
    {
        try
        {
            _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
            var prop = GetProperty(dbSetName);
            IQueryable<object>? collection = prop?.GetGetMethod()?.Invoke(Context, null) as IQueryable<object>;
            var result = collection?.Where(where);
            _log?.Debug("return: {@a}", result);
            return result;
        }
        catch (Exception ex)
        {
            _log?.Error("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, ex);
            throw;
        }
    }

    public List<T> RawSqlQuery<T>(string query, Func<DbDataReader, T> map, CommandType commandType = CommandType.Text)
    {
        try
        {
            _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
            Context.Database.OpenConnection();
            using var command = Context.Database.GetDbConnection().CreateCommand();
            command.CommandText = query;
            command.CommandType = commandType;
            using var result = command.ExecuteReader();
            var entities = new List<T>();
            while (result.Read()) { entities.Add(map(result)); }
            Context.Database.CloseConnection();
            _log?.Debug("return: {@a}", entities);
            return entities;
        }
        catch (Exception ex)
        {
            _log?.Error("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, ex);
            throw;
        }
    }

    public DbDataReader RawSqlQuery(string query, CommandType commandType = CommandType.Text)
    {
        try
        {
            _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
            using var context = Context;
            context.Database.OpenConnection();
            using var command = Context.Database.GetDbConnection().CreateCommand();
            command.CommandText = query;
            command.CommandType = commandType;
            using var result = command.ExecuteReader();
            context.Database.CloseConnection();
            _log?.Debug("return: {@a}", result);
            return result;
        }
        catch (Exception ex)
        {
            _log?.Error("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, ex);
            throw;
        }
    }

    public object Add<T>(object obj) where T : class
    {
        try
        {
            _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
            var convertedObj = (T)obj;
            Context.Add(convertedObj);
            _log?.Debug("return: {@a}", convertedObj);
            return convertedObj;
        }
        catch (Exception ex)
        {
            _log?.Error("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, ex);
            throw;
        }

    }

    public object Add(string dbSetName, object obj)
    {
        try
        {
            _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
            var dbSetType = DbSetUnderlyingType(dbSetName);
            if (dbSetType is null) throw new Exception("DbSet type cannot be null!");
            var convertedObj = Convert.ChangeType(obj, dbSetType);
            Context.Add(convertedObj);
            _log?.Debug("return: {@a}", convertedObj);
            return convertedObj;
        }
        catch (Exception ex)
        {
            _log?.Error("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, ex);
            throw;
        }
    }

     public object Addulk(IList<object> objs)
    {
        try
        {
            _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
            Context.AddRange(objs);
            return objs;
        }
        catch (Exception ex)
        {
            _log?.Error("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, ex);
            throw;
        }
    }
    public object Remove<T>(object obj) where T : class
    {
        try
        {
            _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
            var convertedObj = (T)obj;
            Context.Set<T>().Remove(convertedObj);
            _log?.Debug("return: {@a}", convertedObj);
            return convertedObj;
        }
        catch (Exception ex)
        {
            _log?.Error("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, ex);
            throw;
        }
    }

    public object Remove(string dbSetName, object obj)
    {
        try
        {
            _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
            var dbSetType = DbSetUnderlyingType(dbSetName);
            if (dbSetType is null) throw new Exception("DbSet type cannot be null!");
            var convertedObj = Convert.ChangeType(obj, dbSetType);
            Context.Remove(convertedObj);
            _log?.Debug("return: {@a}", convertedObj);
            return convertedObj;
        }
        catch (Exception ex)
        {
            _log?.Error("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, ex);
            throw;
        }
    }
    public IEnumerable<object> Remove<T>(Expression<Func<T, bool>> p) where T : class
    {
        _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
        try
        {
            var pred = p.Compile();
            var objs = Context.Set<T>().Where(p);
            Context.Set<T>().RemoveRange(objs);
            _log?.Debug("return: {@a}", objs);
            return objs;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public IEnumerable<object> Remove(string dbSetName, string where)
    {
        _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
        try
        {
            var objs = QueryFiltered(dbSetName, where);
            var dbSetType = DbSetUnderlyingType(dbSetName);
            if (dbSetType is null) throw new Exception("dbSetType cannot be null!");
            List<object> convertedObjects = new();
            foreach (var obj in objs?.AsEnumerable<object>())
            {
                convertedObjects.Add(Convert.ChangeType(obj, dbSetType));
            }
            Context.RemoveRange(convertedObjects);
            _log?.Debug("return: {@a}", convertedObjects);
            return convertedObjects;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public T? Get<T>(Expression<Func<T, bool>> p) where T : class
    {
        _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
        try
        {
            var pred = p.Compile();
            var result = Context.Set<T>().Where(p).AsEnumerable<T>().SingleOrDefault();
            _log?.Debug("return: {@a}", result);
            return result;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public object? Get(string dbSetName, string where)
    {
        _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
        try
        {
            var collection = QueryFiltered(dbSetName, where);
            var result = collection?.SingleOrDefault();
            _log?.Debug("return: {@a}", result);
            return result;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public IEnumerable<T>? GetAll<T>(string orderby, int page, int count, bool descending) where T : class
    {
        _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
        try
        {
            string direction = descending ? "desc" : "asc";
            var result = Context.Set<T>().OrderBy($"{orderby} {direction}").Skip((page - 1) * count).Take(count).AsEnumerable<T>();
            _log?.Debug("return: {@a}", result);
            return result;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public IQueryable<object>? GetAll(string dbSetName, string orderby, int page, int count, bool descending)
    {
        _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
        try
        {
            var prop = GetProperty(dbSetName);
            IQueryable<object>? collection = prop?.GetGetMethod()?.Invoke(Context, null) as IQueryable<object>;
            string direction = descending ? "desc" : "asc";
            var result = collection?.OrderBy($"{orderby} {direction}").Skip((page - 1) * count).Take(count);
            _log?.Debug("return: {@a}", result);
            return result;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public IEnumerable<T>? GetAll<T>(string where, string orderby, int page, int count, bool descending) where T : class
    {
        _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
        try
        {
            string direction = descending ? "desc" : "asc";
            var result = Context.Set<T>().Where(where).OrderBy($"{orderby} {direction}").Skip((page - 1) * count).Take(count).AsEnumerable<T>();
            _log?.Debug("return: {@a}", result);
            return result;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public IEnumerable<object>? GetAll(string dbSetName, string where, string orderby, int page, int count, bool descending)
    {
        _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
        try
        {
            string direction = descending ? "desc" : "asc";
            var result = QueryFiltered(dbSetName, where)?.OrderBy($"{orderby} {direction}").Skip((page - 1) * count).Take(count).AsEnumerable<object>();
            _log?.Debug("return: {@a}", result);
            return result;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public IQueryable<object>? GetAll(string dbSetName)
    {
        try
        {
            _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
            var prop = GetProperty(dbSetName);
            var result = prop?.GetGetMethod()?.Invoke(Context, null) as IQueryable<object>;
            _log?.Debug("return: {@a}", result);
            return result;
        }
        catch (Exception ex)
        {
            _log?.Error("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, ex);
            throw;
        }
    }

    public T Update<T>(object obj) where T : class
    {
        try
        {
            _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
            var convertedObj = (T)obj;
            Context.Set<T>().Update(convertedObj);
            _log?.Debug("return: {@a}", convertedObj);
            return convertedObj;
        }
        catch (Exception ex)
        {
            _log?.Error("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, ex);
            throw;
        }
    }

    private object Update(Type dbSetType, object target, object source, IEnumerable<string>? keyNames = null)
    {
        _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
        if (target.GetType() != dbSetType)
        {
            target = Convert.ChangeType(target, dbSetType);
            if (target is null) throw new Exception("Update error! Target cannot be null");
        }
        Context.Entry(target).State = EntityState.Modified;
        Copy(ref target, source, keyNames);
        _log?.Debug("return: {@a}", target);
        return target;
    }

    private object Update(object target, ExpandoObject source, IEnumerable<string>? keyNames = null)
    {
        try
        {
            _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
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
            _log?.Debug("return: {@a}", target);
            return target;
        }
        catch (Exception ex)
        {
            _log?.Error("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, ex);
            throw;
        }
    }

    public object Update(string dbSetName, object target, object source, IEnumerable<string>? keyNames = null)
    {
        try
        {
            _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
            var dbSetType = DbSetUnderlyingType(dbSetName);
            if (dbSetType is null) throw new Exception("dbSetType cannot be null!");
            var result = Update(dbSetType, target, source, keyNames);
            _log?.Debug("return: {@a}", result);
            return result;
        }
        catch (Exception ex)
        {
            _log?.Error("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, ex);
            throw;
        }
    }

    public object Update(string dbSetName, object sourceWithId, IEnumerable<string>? keyNames = null)
    {
        _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
        try
        {
            var dbSetType = DbSetUnderlyingType(dbSetName);
            if (dbSetType is null) throw new Exception("dbSetType cannot be null!");
            var keys = GetKeysValues(sourceWithId, keyNames);
            var targetInDb = Find(dbSetType, keys?.ToArray()!)!;
            if (targetInDb is null) throw new Exception("Body object with provided Keys not found in Database");
            var result = Update(dbSetType, targetInDb, sourceWithId, keyNames);
            _log?.Debug("return: {@a}", result);
            return result;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public object Update(string dbSetName, ExpandoObject sourceWithId, IEnumerable<string>? keyNames = null)
    {
        _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
        try
        {
            var dbSetType = DbSetUnderlyingType(dbSetName);
            if (dbSetType is null) throw new Exception("dbSetType cannot be null!");
            var keys = GetKeysValues(sourceWithId, keyNames);
            var targetInDb = Find(dbSetType, keys?.ToArray()!)!;
            if (targetInDb is null) throw new Exception("body object with provided Keys not found in Database");
            var result = Update(targetInDb, sourceWithId, keyNames);
            _log?.Debug("return: {@a}", result);
            return result;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public object Update(string dbSetName, JsonElement sourceWithId, IEnumerable<string> keyNames)
    {
        _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
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
            var targetInDb = QueryFiltered(dbSetName, where)?.Single();
            if (targetInDb is null) throw new Exception("body object with provided Keys not found in Database");
            var result = Update(targetInDb, expandoElement, keyNames);
            _log?.Debug("return: {@a}", result);
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }



}
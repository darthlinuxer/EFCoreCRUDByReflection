namespace Universal.Context;
public partial class UniversalContext
{
    public async Task<object>? FindAsync(Type entityType, object[] keys, CancellationToken ct)
    {
        _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
        var result = await Context.FindAsync(entityType, keys, ct);
        _log?.Debug("return: {@a}", result);
        return result;
    }
    public async Task<List<T>> RawSqlQueryAsync<T>(string query, Func<DbDataReader, T> map, CancellationToken ct, CommandType commandType = CommandType.Text)
    {
        try
        {
            _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
            using var context = Context;
            context.Database.OpenConnection();
            using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = query;
            command.CommandType = commandType;
            using var result = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, ct);
            var entities = new List<T>();
            while (result.Read()) { entities.Add(map(result)); }
            context.Database.CloseConnection();
            _log?.Debug("return: {@a}", entities);
            return entities;
        }
        catch (Exception ex)
        {
            _log?.Error("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, ex);
            throw;
        }
    }

    public async Task<DbDataReader> RawSqlQueryAsync(string query, CancellationToken ct, CommandType commandType = CommandType.Text)
    {
        try
        {
            _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
            using var context = Context;
            context.Database.OpenConnection();
            using var command = Context.Database.GetDbConnection().CreateCommand();
            command.CommandText = query;
            command.CommandType = commandType;
            using var result = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, ct);
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

    public async Task<object> AddAsync<T>(object obj, CancellationToken ct) where T : class
    {
        try
        {
            _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
            var convertedObj = (T)obj;
            await Context.AddAsync(convertedObj, ct);
            var result = convertedObj;
            _log?.Debug("return: {@a}", result);
            return result;
        }
        catch (Exception ex)
        {
            _log?.Error("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, ex);
            throw;
        }
    }
    public async Task<object> AddAsync(string dbSetName, object obj, CancellationToken ct)
    {
        try
        {
            _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
            var dbSetType = DbSetUnderlyingType(dbSetName);
            if (dbSetType is null) throw new Exception("DbSet type cannot be null!");
            var convertedObj = Convert.ChangeType(obj, dbSetType);
            await Context.AddAsync(convertedObj, ct);
            var result = convertedObj;
            _log?.Debug("return: {@a}", result);
            return result;
        }
        catch (Exception ex)
        {
            _log?.Error("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, ex);
            throw;
        }
    }

    public async Task<T> GetAsync<T>(Expression<Func<T, bool>> p, CancellationToken ct, bool asNoTracking = false, params string[] includeNavigationNames) where T : class
    {
        _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
        try
        {
            var pred = p.Compile();
            var mainQuery = Context.Set<T>().Where(p);
            if (asNoTracking) mainQuery = mainQuery.AsNoTracking();
            if (includeNavigationNames.Length > 0) foreach(var navigation in includeNavigationNames) mainQuery = mainQuery.Include(navigation);
            var result = await mainQuery.SingleOrDefaultAsync(ct);
            _log?.Debug("return: {@a}", result);
            return result;
        }
        catch (Exception ex)
        {
            _log?.Error("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, ex);
            throw;
        }
    }

    public async Task<object?> GetAsync(string dbSetName, string where, CancellationToken ct, bool asNoTracking = false, params string[] includeNavigationNames)
    {
        _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
        try
        {
            var mainQuery = QueryFiltered(dbSetName, where);
            if (asNoTracking) mainQuery = mainQuery.AsNoTracking<object>();
            if (includeNavigationNames.Length > 0) foreach(var navigation in includeNavigationNames) mainQuery = mainQuery.Include(navigation);
            var result = await mainQuery?.SingleOrDefaultAsync(ct);
            _log?.Debug("return: {@a}", result);
            return result;
        }
        catch (Exception ex)
        {
            _log?.Error("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, ex);
            throw;
        }
    }

    public IAsyncEnumerable<object>? GetAllAsync(string dbSetName, string orderby, int page, int count, bool descending, bool asNoTracking = true, params string[] includeNavigationNames)
    {
        _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
        try
        {
            string direction = descending ? "desc" : "asc";
            var mainQuery = Query(dbSetName)?.OrderBy($"{orderby} {direction}").Skip((page - 1) * count).Take(count);
            if (asNoTracking) mainQuery = mainQuery?.AsNoTracking();
            if (includeNavigationNames.Length > 0) foreach(var navigation in includeNavigationNames) mainQuery = mainQuery.Include(navigation);
            var result = mainQuery?.AsAsyncEnumerable();
            _log?.Debug("return: {@a}", result);
            return result;
        }
        catch (Exception ex)
        {
            _log?.Error("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, ex);
            throw;
        }
    }

    public IAsyncEnumerable<T>? GetAllAsync<T>(string orderby, int page, int count, bool descending, bool asNoTracking = true, params string[] includeNavigationNames) where T : class
    {
        _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
        try
        {
            string direction = descending ? "desc" : "asc";
            var mainQuery = Context.Set<T>().OrderBy($"{orderby} {direction}").Skip((page - 1) * count).Take(count);
            if (asNoTracking) mainQuery = mainQuery?.AsNoTracking();
            if (includeNavigationNames.Length > 0) foreach(var navigation in includeNavigationNames) mainQuery = mainQuery.Include(navigation);
            var result = mainQuery?.AsAsyncEnumerable();
            _log?.Debug("return: {@a}", result);
            return result;
        }
        catch (Exception ex)
        {
            _log?.Error("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, ex);
            throw;
        }
    }

    public IAsyncEnumerable<T>? GetAllFilteredAsync<T>(string where, string orderby, int page, int count, bool descending, bool asNoTracking = true, params string[] includeNavigationNames) where T : class
    {
        _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
        try
        {
            string direction = descending ? "desc" : "asc";
            var mainQuery = Context.Set<T>().Where(where).OrderBy($"{orderby} {direction}").Skip((page - 1) * count).Take(count);
            if (asNoTracking) mainQuery = mainQuery?.AsNoTracking();
            if (includeNavigationNames.Length > 0) foreach(var navigation in includeNavigationNames) mainQuery = mainQuery.Include(navigation);
            var result = mainQuery?.AsAsyncEnumerable();
            _log?.Debug("return: {@a}", result);
            return result;
        }
        catch (Exception ex)
        {
            _log?.Error("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, ex);
            throw;
        }
    }


    public IAsyncEnumerable<object>? GetAllFilteredAsync(string dbSetName, string where, string orderby, int page, int count, bool descending, bool asNoTracking = true, params string[] includeNavigationNames)
    {
        _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
        try
        {
            string direction = descending ? "desc" : "asc";
            var mainQuery = QueryFiltered(dbSetName, where)?.OrderBy($"{orderby} {direction}").Skip((page - 1) * count).Take(count);
            if (asNoTracking) mainQuery = mainQuery?.AsNoTracking();
            if (includeNavigationNames.Length > 0) foreach(var navigation in includeNavigationNames) mainQuery = mainQuery.Include(navigation);
            var result = mainQuery?.AsAsyncEnumerable();
            _log?.Debug("return: {@a}", result);
            return result;
        }
        catch (Exception ex)
        {
            _log?.Error("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, ex);
            throw;
        }
    }

    public async Task<object> UpdateAsync(string dbSetName, object sourceWithId, CancellationToken ct, IEnumerable<string>? keyNames = null)
    {
        _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
        try
        {
            var dbSetType = DbSetUnderlyingType(dbSetName);
            if (dbSetType is null) throw new Exception("dbSetType cannot be null!");
            var keys = GetKeysValues(sourceWithId, keyNames);
            var targetInDb = await FindAsync(dbSetType, keys?.ToArray(), ct)!;
            if (targetInDb is null) throw new Exception("Body object with provided Keys not found in Database");
            var result = Update(dbSetType, targetInDb, sourceWithId, keyNames);
            _log?.Debug("return: {@a}", result);
            return result;
        }
        catch (Exception ex)
        {
            _log?.Error("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, ex);
            throw;
        }
    }

    public async Task<object> UpdateAsync(string dbSetName, ExpandoObject sourceWithId, CancellationToken ct, IEnumerable<string>? keyNames = null)
    {
        _log?.Information("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, MethodBase.GetCurrentMethod()?.GetCustomAttributes());
        try
        {
            var dbSetType = DbSetUnderlyingType(dbSetName);
            if (dbSetType is null) throw new Exception("dbSetType cannot be null!");
            var keys = GetKeysValues(sourceWithId, keyNames);
            var targetInDb = await FindAsync(dbSetType, keys?.ToArray()!, ct)!;
            if (targetInDb is null) throw new Exception("body object with provided Keys not found in Database");
            var result = Update(targetInDb, sourceWithId, keyNames);
            _log?.Debug("return: {@a}", result);
            return result;
        }
        catch (Exception ex)
        {
            _log?.Error("{a}:{b} {@c}", this, MethodBase.GetCurrentMethod()?.Name, ex);
            throw;
        }
    }

}
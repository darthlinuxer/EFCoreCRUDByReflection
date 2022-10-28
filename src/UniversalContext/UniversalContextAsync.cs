namespace Universal.Context;
public partial class UniversalContext
{
      public async Task<List<T>> RawSqlQueryAsync<T>(string query, Func<DbDataReader, T> map, CancellationToken ct, CommandType commandType = CommandType.Text)
    {
        using var context = Context;
        context.Database.OpenConnection();
        using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = query;
        command.CommandType = commandType;
        using var result = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, ct);
        var entities = new List<T>();
        while (result.Read()) { entities.Add(map(result)); }
        context.Database.CloseConnection();
        return entities;
    }

    public async Task<DbDataReader> RawSqlQueryAsync(string query, CancellationToken ct, CommandType commandType = CommandType.Text)
    {
        using var context = Context;
        context.Database.OpenConnection();
        using var command = Context.Database.GetDbConnection().CreateCommand();
        command.CommandText = query;
        command.CommandType = commandType;
        using var result = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, ct);
        context.Database.CloseConnection();
        return result;
    }

    public async Task<object> AddAsync<T>(object obj, CancellationToken ct) where T : class
    {
        var convertedObj = (T)obj;
        await Context.AddAsync(convertedObj, ct);
        return convertedObj;
    }
    public async Task<object> AddAsync(string dbSetName, object obj, CancellationToken ct)
    {
        var dbSetType = DbSetUnderlyingType(dbSetName);
        if (dbSetType is null) throw new Exception("DbSet type cannot be null!");
        var convertedObj = Convert.ChangeType(obj, dbSetType);
        await Context.AddAsync(convertedObj, ct);
        return convertedObj;
    }

    public async Task<IEnumerable<object>> RemoveAsync(string dbSetName, string where, CancellationToken ct)
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


}
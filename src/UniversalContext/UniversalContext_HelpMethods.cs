using System.Dynamic;

namespace Universal.Context;
public partial class UniversalContext
{

    public DbSet<T> DbSet<T>() where T : class => Context.Set<T>();
    public int Save() => Context.SaveChanges();
    public async Task<int> SaveAsync() => await Context.SaveChangesAsync();
    private PropertyInfo? GetProperty(string dbSetName) => Context.GetType().GetProperty(dbSetName, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
    public static object ConvertTo<T>(JsonElement item) where T : class => item.ToObject<T>(new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true
    });
    public static object ConvertTo(Type t, JsonElement item) => item.ToObject(t, new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true
    });
    public Type DbSetUnderlyingType(string dbSetName)
    {
        var prop = Context.GetType()
                           .GetProperty(dbSetName, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (prop is null) throw new Exception("Dbset not found!");
        return prop.PropertyType
                   .GenericTypeArguments.Single().UnderlyingSystemType;
    }
    public static void Copy(ref object target, object source, IEnumerable<string>? keys = null)
    {
        try
        {
            foreach (var sourceProp in source.GetType().GetProperties(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.IgnoreCase))
            {
                var targetProp = target.GetType().GetProperty(sourceProp.Name, BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (targetProp is null) continue;
                //can´t update key attributes
                if (targetProp.CustomAttributes.Any(c => c.AttributeType == typeof(System.ComponentModel.DataAnnotations.KeyAttribute))) continue;
                if (keys is not null && keys.Select(c => c.ToLower()).Contains(targetProp.Name.ToLower())) continue;
                //If it reached here then it´s not a key attribute and value can be updated on target 
                targetProp?.SetValue(target, sourceProp.GetValue(source));
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    private static IEnumerable<PropertyInfo>? GetKeys(object target)
    {
        return target.GetType().GetProperties(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.IgnoreCase)
        .Where(c => c.CustomAttributes.Any(c => c.AttributeType == typeof(System.ComponentModel.DataAnnotations.KeyAttribute)));
    }

    private static IEnumerable<PropertyInfo>? GetKeys(object target, IEnumerable<string> keyNames)
    {
        return target.GetType().GetProperties(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.IgnoreCase)
        .Where(c => keyNames.Select(d => d.ToLower()).Contains(c.Name.ToLower()));
    }
    private static IEnumerable<object?>? GetKeysValues(object target, IEnumerable<string>? keyNames = null)
    {
        IEnumerable<PropertyInfo>? keyProperties = null;
        if (keyNames is not null)
        {
            keyProperties = GetKeys(target, keyNames);
        }
        else
        {
            //Target better have some DataAnnotation Key Attributes then
            keyProperties = GetKeys(target);
        }
        return keyProperties!.Select(c => c.GetValue(target));
    }

    private static IEnumerable<object?>? GetKeysValues(ExpandoObject target, IEnumerable<string>? keyNames = null)
    {
        var dict = target as IDictionary<string, object>;
        var keyValues = new List<object>();
        if (keyNames is not null)
        {
            foreach (var keyName in keyNames)
            {
                _ = dict.TryGetValue(keyName, out var value);
                if (value is not null) keyValues.Add(value);
            }
            return keyValues;
        }
        return dict.Values;
    }
}
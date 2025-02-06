using System.Reflection;


namespace Tests.Source._Utils;

public static class ReflectionUtils
{
    private static BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    public static T GetPrivateFieldByType<T>(this object instance)
    {
        var instanceType = instance.GetType();
        return (T)instanceType.GetFields(bindingFlags).Single(f => f.FieldType.IsAssignableTo(typeof(T)))
            .GetValue(instance);
    }
    
    public static Array GetPrivateArray(this object instance)
    {
        var instanceType = instance.GetType();
        return (Array)instanceType.GetFields(bindingFlags).Single(f => f.FieldType.IsArray)
            .GetValue(instance);
    }
    
    public static T GetPrivateArray<T>(this object instance)
    {
        var instanceType = instance.GetType();
        return (T)instanceType.GetFields(bindingFlags).Single(f => f.FieldType.IsArray)
            .GetValue(instance);
    }
    
    public static T InvokePrivateMethod<T>(this object obj, string methodName, params object[] parameters)
    {
        Type type = obj.GetType();
        MethodInfo method = type.GetMethod(methodName, bindingFlags);
        
        if (method == null) throw new MissingMethodException($"Method '{methodName}' not found in type '{type.FullName}'");
        
        return (T)(method.Invoke(obj, parameters));
    }
}
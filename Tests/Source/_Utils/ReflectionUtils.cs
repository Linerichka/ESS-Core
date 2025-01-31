using System.Reflection;


namespace Tests.Source._Utils;

public static class ReflectionUtils
{
    public static T GetPrivateFieldByType<T>(this object instance)
    {
        var instanceType = instance.GetType();
        return (T)instanceType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Single(f => f.FieldType.IsAssignableTo(typeof(T)))
            .GetValue(instance);
    }
    
    public static Array GetPrivateArray(this object instance)
    {
        var instanceType = instance.GetType();
        return (Array)instanceType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Single(f => f.FieldType.IsArray)
            .GetValue(instance);
    }
    
    public static T GetPrivateArray<T>(this object instance)
    {
        var instanceType = instance.GetType();
        return (T)instanceType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Single(f => f.FieldType.IsArray)
            .GetValue(instance);
    }
}
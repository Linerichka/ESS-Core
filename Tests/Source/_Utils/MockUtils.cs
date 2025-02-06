using System.Linq.Expressions;
using System.Reflection;
using Moq;
using Moq.Language.Flow;


namespace Tests.Source._Utils;

public static class MockUtils
{
    public static ISetup<T> SetupDefaultArgs<T>(this Mock<T> mock, string methodName) where T : class
        => mock.Setup(GenerateExpression<T>(methodName));

    public static void VerifyDefaultArgs<T>(this Mock<T> mock, string methodName, Times times) where T : class
        => mock.Verify(GenerateExpression<T>(methodName), times);
    public static void VerifyDefaultArgs<T>(this Mock<T> mock, Action method, Times times) where T : class
        => mock.Verify(GenerateExpression<T>(method.Method.Name), times);
    

    public static void VerifyDefaultArgs<T>(this Mock<T> mock, string[] methodsName, Times times) where T : class
    {
        foreach (var methodName in methodsName)
        {
            mock.VerifyDefaultArgs(methodName, times);
        }
    }

    public static void VerifyDefaultArgs<T>(this Mock<T> mock, Times times, params Action[] methods) where T : class
    {
        foreach (var method in methods)
        {
            mock.VerifyDefaultArgs(method, times);
        }
    }
    public static void VerifyDefaultArgs<T>(this Mock<T> mock, params (Action, Times)[] methodsTimes) where T : class
    {
        foreach (var methodTime in methodsTimes)
        {
            mock.VerifyDefaultArgs(methodTime.Item1, methodTime.Item2);
        }
    }
    public static void VerifyDefaultArgs<T>(this Mock<T> mock, params Action[] methods) where T : class
    {
        foreach (var method in methods)
        {
            mock.VerifyDefaultArgs(method, Times.Never());
        }
    }

    private static Expression<Action<T>> GenerateExpression<T>(string methodName)
    {
        var method = typeof(T).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.Name == methodName).First();

        var instance = Expression.Parameter(typeof(T), "m");
        var parameters = method.GetParameters().Select(p => GenerateItIsAny(p.ParameterType));
        var callExp = Expression.Call(instance, method, parameters);
        return Expression.Lambda<Action<T>>(callExp, instance);
    }

    private static MethodCallExpression GenerateItIsAny(Type T)
    {
        var ItIsAnyT = typeof(It)
            .GetMethod("IsAny")
            .MakeGenericMethod(T);
        return Expression.Call(ItIsAnyT);
    }
}
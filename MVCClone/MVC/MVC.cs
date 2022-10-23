using MVCClone.Errors;
using MVCClone.MVC;
using System.Collections.Specialized;
using System.Reflection;
using System.Web;

public class MVCContainer {
    private List<Type> cachedControllers = new List<Type>();

    public MVCContainer()
    {
        var controllerType = typeof(Controller);
        cachedControllers = controllerType
            .Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && controllerType.IsAssignableFrom(type))
            .ToList();
    }

    public object Resolve(Uri uri) {
        var controller = getController(uri);

        if (controller == null)
            throw new BadRequestException();

        var action = getAction(controller, uri);

        if (action == null)
            throw new BadRequestException();

        var parameters = getParameters(action, uri);

#pragma warning disable CS8603 // Possible null reference return.
        return action.Invoke(controller, parameters);
#pragma warning restore CS8603 // Possible null reference return.
    }

    private object[] getParameters(MethodInfo methodInfo, Uri uri)
    {
        var parameterInfos = methodInfo.GetParameters().ToList();
        if (parameterInfos.Count == 0)
        {
            return null;
        }
        var results = new object[parameterInfos.Count];

        var query = HttpUtility.ParseQueryString(uri.Query);

        for (int i = 0; i < parameterInfos.Count; i++)
        {
            var info = parameterInfos[i];
            var type = parameterInfos[i].ParameterType;
            ModelBinding(results, query, i, info, type);
        }

        return results;
    }

    private static void ModelBinding(object[] results, NameValueCollection query, int i, ParameterInfo info, Type type)
    {
        var value = query[info.Name];
        if (type == typeof(String))
        {
            results[i] = value;
        }
        else if (type == typeof(Int32))
        {
            results[i] = Int32.Parse(value);
        }
    }

    private MethodInfo getAction(Controller controller, Uri uri) {

        var action = uri.AbsolutePath.Split("/").Last();
        return controller
            .GetType()
            .GetMethods()
            .First(x => x.Name.Equals(action, StringComparison.InvariantCultureIgnoreCase));
    }

    private Controller getController(Uri uri)
    {
        var controllerType = cachedControllers
            .FirstOrDefault(x => uri.AbsolutePath
                .StartsWith($"/{x.Name.Replace("Controller", "")}", StringComparison.InvariantCultureIgnoreCase));

        if (controllerType == null)
            throw new Exception("");

        return (Controller) Activator.CreateInstance(controllerType, null)!;
    }
}

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fibrous.Proxy
{
    public class AsyncFiberProxy<T> : DispatchProxyAsync
    {
        private IAsyncFiber _fiber;
        private T _decorated;

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            if (targetMethod == null) throw new ArgumentException(nameof(targetMethod));

            var targetMethodName = targetMethod.Name;
            if (targetMethodName == "Dispose")
            {
                Dispose();
                return targetMethod.Invoke(_decorated, args);

            }
            
            if (targetMethod.ReturnType != typeof(void) ||
                (targetMethodName.StartsWith("add_", StringComparison.Ordinal) ||
                 targetMethodName.StartsWith("remove_", StringComparison.Ordinal)))
                return targetMethod.Invoke(_decorated, args);

            _fiber.Enqueue(() => (Task)targetMethod.Invoke(_decorated, args));
            return null;
        }

        protected override Task InvokeAsync(MethodInfo method, object[] args)
        {
            _fiber.Enqueue(async () => await (Task)method.Invoke(_decorated, args));
            return Task.CompletedTask;
        }

        protected override Task<T1> InvokeAsyncT<T1>(MethodInfo method, object[] args)
        {
            throw new NotImplementedException();
        }

        private void Dispose()
        {
            _fiber.Dispose();
        }

        private void Init(T decorated, Action<Exception> callback = null)
        {
            if (decorated == null)
            {
                throw new ArgumentNullException(nameof(decorated));
            }
            _decorated = decorated;
            IAsyncExecutor executor = callback != null ? new AsyncExceptionHandlingExecutor(callback) : (IAsyncExecutor)new AsyncExecutor();
            _fiber = new AsyncFiber(executor);
        }

        public static T Create(T decorated, Action<Exception> callback = null)
        {
            //Check that it has IDisposable. no properties, 
            //just void methods with any parameters
            Type type = typeof(T);

            bool disposable = decorated is IDisposable;
            if (!disposable)
                throw new ArgumentException("Interface must inherit IDisposable");

            var propertyInfos = type.GetProperties();
            bool hasProperties = propertyInfos.Length > 0;
            if (hasProperties)
                throw new ArgumentException("Interface must not have properties");

            bool badMethods = type.GetMethods().Count(x => x.ReturnType != typeof(Task) && (x.ReturnType == typeof(void) && !CheckName(x))) > 0;
            if (badMethods)
                throw new ArgumentException("All interface methods must return Task except IDisposable");

            object proxy = Create<T, AsyncFiberProxy<T>>();
            var fiberProxy = (AsyncFiberProxy<T>)proxy;
            fiberProxy.Init(decorated, callback);
            return (T)proxy;
        }

        private static bool CheckName(MethodInfo x)
        {
            return new[] {"add_", "remove_", "Dispose"}.Any(y => x.Name.Contains(y));
        }
    }
}
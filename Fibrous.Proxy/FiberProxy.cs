using System;
using System.Linq;
using System.Reflection;

namespace Fibrous.Proxy
{
    public class FiberProxy<T> : DispatchProxy
    {
        private T _decorated;
        private IFiber _fiber;

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            if (targetMethod == null) throw new ArgumentException(nameof(targetMethod));

            var targetMethodName = targetMethod.Name;
            if (targetMethodName == "Dispose")
            {
                Dispose();
                return targetMethod.Invoke(_decorated, args);
            }

            //Handle event attach.detach
            if (targetMethod.ReturnType != typeof(void) ||
                (targetMethodName.StartsWith("add_", StringComparison.Ordinal) ||
                 targetMethodName.StartsWith("remove_", StringComparison.Ordinal)))
                return targetMethod.Invoke(_decorated, args);

            _fiber.Enqueue(() => targetMethod.Invoke(_decorated, args));
            return null;

        }

        private void Dispose()
        {
            _fiber.Dispose();
        }

        private void Init(T decorated, Action<Exception> callback = null)
        {
            if (decorated == null) throw new ArgumentNullException(nameof(decorated));

            _decorated = decorated;
            var executor = callback != null ? new ExceptionHandlingExecutor(callback) : (IExecutor) new Executor();
            _fiber = new Fiber(executor);
        }

        public static T Create(T decorated, Action<Exception> callback = null)
        {
            //Check that it has IDisposable. no properties, 
            //just void methods with any parameters
            var type = typeof(T);

            var disposable = decorated is IDisposable;
            if (!disposable)
                throw new ArgumentException("Interface must inherit IDisposable");

            var propertyInfos = type.GetProperties();
            var hasProperties = propertyInfos.Length > 0;
            if (hasProperties)
                throw new ArgumentException("Interface must not have properties");

            var badMethods = type.GetMethods().Count(x => x.ReturnType != typeof(void)) > 0;
            if (badMethods)
                throw new ArgumentException("All interface methods must return void");

            object proxy = Create<T, FiberProxy<T>>();
            var fiberProxy = (FiberProxy<T>) proxy;
            fiberProxy.Init(decorated, callback);
            return (T) proxy;
        }
    }
}
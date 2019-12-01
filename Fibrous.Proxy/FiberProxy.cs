using System;
using System.Reflection;
using System.Linq;

namespace Fibrous.Proxy
{
    
    public class FiberProxy<T>:DispatchProxy
    {
        private IFiber _fiber;
        private T _decorated;

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            if (targetMethod != null)
            {
                var targetMethodName = targetMethod.Name;
                if (targetMethodName == "Dispose")
                {
                    Dispose();
                    return targetMethod.Invoke(_decorated, args);

                }

                if (targetMethod.ReturnType == typeof(void) && !(targetMethodName.StartsWith("add_", StringComparison.Ordinal) || targetMethodName.StartsWith("remove_", StringComparison.Ordinal)))
                {
                    _fiber.Enqueue(() => targetMethod.Invoke(_decorated, args));
                    return null;
                }
                else
                {
                    return targetMethod.Invoke(_decorated, args);
                }
            }

            throw new ArgumentException(nameof(targetMethod));
        }
        
        private void Dispose()
        {
            _fiber.Dispose();
        }

        private void Init( T decorated, Action<Exception> callback = null)
        {
            if (decorated == null)
            {
                throw new ArgumentNullException(nameof(decorated));
            }
            _decorated = decorated;
            IExecutor executor = callback != null ? new ExceptionHandlingExecutor(callback) : (IExecutor)new Executor();
            _fiber = Fiber.StartNew(executor);
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

            bool badMethods = type.GetMethods().Count(x => x.ReturnType != typeof(void)) > 0;
            if (badMethods)
                throw new ArgumentException("All interface methods must return void");

            object proxy = Create<T, FiberProxy<T>>();
            var fiberProxy = (FiberProxy<T>)proxy;
            fiberProxy.Init(decorated, callback);
            return (T)proxy;
        }
    }
}

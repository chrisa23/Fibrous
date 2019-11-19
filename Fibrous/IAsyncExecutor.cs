using System;
using System.Threading.Tasks;

namespace Fibrous
{
    public interface IAsyncExecutor
    {
        Task Execute(Func<Task> toExecute);
    }
}
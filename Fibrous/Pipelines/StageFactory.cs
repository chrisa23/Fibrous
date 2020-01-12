//using System;
//using System.Threading.Tasks;

//namespace Fibrous.Pipelines
//{
//    public interface IStageFactory : IDisposable
//    {
//        Stage<TIn, TOut> Stage<TIn, TOut>(Func<TIn, TOut> f, Action<Exception> errorCallback = null);
//        Tee<T> Tee<T>(Action<T> f, Action<Exception> errorCallback = null);
//        AsyncStage<TIn, TOut> AsyncStage<TIn, TOut>(Func<TIn, Task<TOut>> f, Action<Exception> errorCallback = null);
//        AsyncTee<T> AsyncTee<T>(Func<T, Task> f, Action<Exception> errorCallback = null);
//    }

//    public class StageFactory: Disposables, IStageFactory
//    {
//        private readonly Action<Exception> _errorCallback;

//        public StageFactory(Action<Exception> errorCallback = null)
//        {
//            _errorCallback = errorCallback;
//        }

//        public Stage<TIn, TOut> Stage<TIn, TOut>(Func<TIn, TOut> f, Action<Exception> errorCallback = null)
//        {
//            var stage = new Stage<TIn, TOut>(f, errorCallback ?? _errorCallback);
//            Add(stage);
//            return stage;
//        }

//        public Tee<T> Tee<T>(Action<T> f, Action<Exception> errorCallback = null)
//        {
//            var stage = new Tee<T>(f, errorCallback ?? _errorCallback);
//            Add(stage);
//            return stage;
//        }

//        public AsyncStage<TIn, TOut> AsyncStage<TIn, TOut>(Func<TIn, Task<TOut>> f, Action<Exception> errorCallback = null)
//        {
//            var stage = new AsyncStage<TIn, TOut>(f, errorCallback ?? _errorCallback);
//            Add(stage);
//            return stage;
//        }

//        public AsyncTee<T> AsyncTee<T>(Func<T, Task> f, Action<Exception> errorCallback = null)
//        {
//            var stage = new AsyncTee<T>(f, errorCallback ?? _errorCallback);
//            Add(stage);
//            return stage;
//        }
//    }

//    public class PipelineBuilder //: IDisposable
//    {
//        private IStageFactory _stageFactory;

//        public PipelineBuilder(Action<Exception> errorCallback = null)
//        {
//            _stageFactory = new StageFactory(errorCallback);
//        }

//        public void AddStage<TIn, TOut>(Func<TIn, TOut> f)
//        {

//        }

//        public IStage<TIn, TOut> Build<TIn, TOut>()
//        {

//        }
//    }
//}

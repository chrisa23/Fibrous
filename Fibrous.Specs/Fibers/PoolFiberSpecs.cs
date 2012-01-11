using Machine.Specifications;

namespace Fibrous.Specs.Fibers
{
    [Subject("PoolFiberSpecs")]
    public class When : PoolFiberSpecs
    {
        private Because of =
            () => { };

        private It should = () => { };
    }

    public abstract class PoolFiberSpecs
    {
        private Establish context =
            () => { };

        protected Cleanup cleanup =
            () => { };
    }
}
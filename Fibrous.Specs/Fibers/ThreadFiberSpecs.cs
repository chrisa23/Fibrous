using Machine.Specifications;

[Subject("ThreadFiberSpecs")]
public class When : ThreadFiberSpecs
{
    private Because of =
        () => { };

    private It should = () => { };
}

public abstract class ThreadFiberSpecs
{
    private Establish context =
        () => { };

    protected Cleanup cleanup =
        () => { };
}
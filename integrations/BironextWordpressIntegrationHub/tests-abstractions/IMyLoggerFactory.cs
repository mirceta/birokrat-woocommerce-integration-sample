using si.birokrat.next.common.logging;

namespace tests.composition.root_builder
{
    public interface IMyLoggerFactory
    {
        public IMyLogger Create();
    }
}

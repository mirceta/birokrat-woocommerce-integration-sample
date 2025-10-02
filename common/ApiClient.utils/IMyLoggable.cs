using si.birokrat.next.common.logging;

namespace BirokratNext.api_clientv2
{
    public interface IMyLoggable {
        void SetLogger(IMyLogger logger);
    }
}
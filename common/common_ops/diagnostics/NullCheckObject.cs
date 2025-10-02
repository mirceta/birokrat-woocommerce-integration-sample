using System.Threading.Tasks;

namespace common_ops.diagnostics
{
    internal class NullCheckObject : ICheck
    {
        private bool _outcome;
        public NullCheckObject(bool outcome)
        {
            _outcome = outcome;
        }

        public async Task<ResultRecord> Run()
        {
            return new ResultRecord(_outcome, GetType().Name, string.Empty);
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;

namespace biro_to_woo.logic.change_trackers.exhaustive
{
    public interface IBatchCreator<T>
    {
        List<Task> CreateBatch(T accumulator);
    }


}

using gui_generator;
using System.Linq;

namespace gui_gen
{
    public class IntegrationConfigTools {
        public static CurrentValue NullImplementationOptions(CurrentValue val)
        {

            if (val == null)
                return null;

            val.addinfo = null;
            val.implementationOptions = null;

            if (val.value == null)
            {
                val.value = "";
            }

            val.dependencies?.ToList().ForEach(x => NullImplementationOptions(x));
            val.elements?.ToList().ForEach(x => NullImplementationOptions(x));
            return val;
        }
    }
}

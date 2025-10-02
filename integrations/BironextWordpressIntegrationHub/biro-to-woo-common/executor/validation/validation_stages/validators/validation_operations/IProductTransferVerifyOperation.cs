using core.logic.common_birokrat;
using core.structs;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace biro_to_woo_common.executor.validation_stages.validators.validation_operations
{

    public interface IProductTransferVerifyOperation
    {
        void Verify(string sifra, BiroOutComparisonContext context);
    }
}

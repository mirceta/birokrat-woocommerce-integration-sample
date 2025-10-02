using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace gui_inferable
{
    public interface ISetupable
    {
        Task Setup(); // THIS IS NEEDED IF AN OBJECT IS NOT PREPARED FOR USE IN THE CONSTRUCTOR BECAUSE IT REQUIRES ASYNC OPERATIONS.
        // FOR EXAMPLE ICOUNTRYMAPPER ON LOAD FETCHES BIROKRAT SIFRANT - SO THUS WE NEED TO ON LOAD CALL THE SETUP() METHOD!
    }
}

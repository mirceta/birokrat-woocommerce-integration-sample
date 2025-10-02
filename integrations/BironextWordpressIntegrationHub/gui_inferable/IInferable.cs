using System;
using System.Collections.Generic;

namespace gui_inferable
{
    public interface IInferable
    {
        /*
         WHY DO WE NEED IInferable??
         
         We need it for the purposes of testing for example - ValidationComponents are inferred instead of declared directly,
         This means that when we use an IZalogaRetriever for example, we will extract it from the object hierarchy 
         of the integration object instead of declaring and defining it.

         Downside: This may clash with variables. If we have declared a variable it needs to take precedence over an inferred variable.
         Why? for example - IZalogeRetriever implementations implement Infer() -> which returns *this*. So this means that there may be 
        a variable which is zalogaRetriever = new RetryingZalogaRetriever(new PerPartesZalogaRetriever), but inferred will be
         PerPartesZaloga retriever because it was deeper in the hierarchy.


        When is it appropriate?
        For example for extracting IZalogaRetriever this is not suitable, because variables are more suitable for this!
        But for example for inferring the statuses of order flows that are used, you can check OrderCondition's implementation of
        IInferable to see why it can be useful!
         */
        Dictionary<string, object> Infer(Dictionary<string, object> state);
    }
}

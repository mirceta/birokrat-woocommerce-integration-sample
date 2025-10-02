using System.Collections.Generic;

namespace ConsoleApp1
{
    public interface IDocumentParameterCommandValue
    {
        string Get(object order, Dictionary<string, object> data);
    }
}

using System.Collections.Generic;

namespace ConsoleApp1
{
    public interface IDocumentParameterCommandCondition
    {
        bool Is(object order, Dictionary<string, object> data);
    }
}

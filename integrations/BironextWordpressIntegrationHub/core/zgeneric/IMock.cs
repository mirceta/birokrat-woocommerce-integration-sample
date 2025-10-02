using System;
using System.Collections.Generic;
using System.Text;

namespace core.zgeneric
{
    public interface IMock<T>
    {
        T Get();
    }

    public interface IMockWithInject<T> {
        T Get();
        void Inject(Dictionary<string, object> injections);

        void SetFieldProperty(string fieldName, string propertyName, object value);

        Dictionary<string, object> GetFields();

        string GetSignature();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Data;

internal class CustomAttributeProvider : ICustomAttributeProvider
{
    private List<Attribute> _attributes = new();

    public CustomAttributeProvider(Attribute attribute)
    {
        _attributes.Add(attribute);
    }

    public CustomAttributeProvider(IEnumerable<Attribute> attributes)
    {
        _attributes.AddRange(attributes);
    }

    public object[] GetCustomAttributes(bool inherit)
    {
        return _attributes.ToArray();
    }

    public object[] GetCustomAttributes(Type attributeType, bool inherit)
    {
        return _attributes.Where(a => a.GetType() == attributeType).ToArray();
    }

    public bool IsDefined(Type attributeType, bool inherit)
    {
        return _attributes.Any(a => a.GetType() == attributeType);
    }
}

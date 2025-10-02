using System;

namespace gui_attributes
{
    [AttributeUsage(AttributeTargets.Constructor, Inherited = false)]
    public sealed class GuiConstructorAttribute : Attribute
    {
        public GuiConstructorAttribute()
        {
        }
    }
}

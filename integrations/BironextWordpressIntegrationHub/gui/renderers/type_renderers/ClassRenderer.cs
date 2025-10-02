using gui_generator;
using Newtonsoft.Json.Schema;
using System;

namespace gui_gen {

    public class ClassRenderer : ComplexElementsRenderer {

        public ClassRenderer(int width, CurrentValue value,
            Func<int> rerender,
            int height_displacement,
            int depth,
            IDememoizer dememo) :
            base(width, value, rerender, height_displacement, depth, (x) => value.dependencies = x, (x) => x.dependencies, dememo, false)
        {}
        
    }

    public enum CurrentValueListField { 
        ELEMENTS,
        DEPENDENCIES
    }
}

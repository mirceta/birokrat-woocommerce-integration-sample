using gui_generator;
using System;

namespace gui_gen {
    public class ListRenderer : ComplexElementsRenderer {

        public ListRenderer(int width, CurrentValue value,
            Func<int> rerender,
            int height_displacement,
            int depth,
            IDememoizer dememo) : base(width, value, rerender, height_displacement, depth, (x) => value.elements = x, (x) => x.elements, dememo, true) { }

    }
}

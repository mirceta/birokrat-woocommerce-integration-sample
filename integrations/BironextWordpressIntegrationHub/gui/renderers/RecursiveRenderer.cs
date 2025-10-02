using gui_generator;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace gui_gen {
    public class RecursiveRenderer {

        int HEIGHT_DISPLACEMENT = 0;
        Func<int> rerender;
        int depth = 0;
        IDememoizer dememo;
        int elementWidth;
        public RecursiveRenderer(int elementWidth, int HEIGHT_DISPLACEMENT, Func<int> rerender, int depth, IDememoizer dememo) {
            this.elementWidth = elementWidth;
            this.HEIGHT_DISPLACEMENT = HEIGHT_DISPLACEMENT;
            this.rerender = rerender;
            this.depth = depth;
            this.dememo = dememo;
        }

        public CurrentValueControl recurse(CurrentValue val) {
            depth++;

            if (val.typeCategory == "dictionary") {
                Console.WriteLine();
            }

             Dictionary<string, Func<CurrentValueControl>> map = new Dictionary<string, Func<CurrentValueControl>>() {
                { "class", () =>             new VisibilityControllerRenderer(val, HEIGHT_DISPLACEMENT, rerender, 
                                                new ClassRenderer(elementWidth, val, rerender, HEIGHT_DISPLACEMENT, depth, dememo), dememo) },
                { "primitive", () =>         new PrimitiveRenderer(elementWidth, val, depth) },
                { "list", () =>              new VisibilityControllerRenderer(val, HEIGHT_DISPLACEMENT, rerender, 
                                                new ListRenderer(elementWidth, val, rerender, HEIGHT_DISPLACEMENT, depth, dememo), dememo) },
                { "dictionary", () =>        new VisibilityControllerRenderer(val, HEIGHT_DISPLACEMENT, rerender, 
                                                new DictionaryRenderer(elementWidth, val, rerender, HEIGHT_DISPLACEMENT, depth, dememo), dememo) },
                { "enum", () =>              new EnumRenderer(val, depth, dememo) }
            };



            if (val == null)
            {
                return new NopRenderer(depth);
            }
            else if ((val.value != null && val.value.StartsWith("@@")))
            {
                return new VariableRenderer(val);
            }
            else if (val.typeCategory == null)
            {
                return new NopRenderer(depth);
            }
            else if (val.typeCategory == "variable") {
                return new NopRenderer(depth);
            }
            else
            {
                return map[val.typeCategory]();
            }
        }
    }
}

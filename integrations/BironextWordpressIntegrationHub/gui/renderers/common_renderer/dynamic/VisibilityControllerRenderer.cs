using gui_generator;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace gui_gen {
    public class VisibilityControllerRenderer : CurrentValueControl {

        ComplexElementsRenderer bassRenderer;
        CurrentValue value;
        int height_displacement;
        Func<int> rerender;
        IDememoizer dememo;

        bool visibility_change_triggered = false; // ONLY USE WHEN USING CurrentValue property of this class!
        public VisibilityControllerRenderer(CurrentValue value, int height_displacement, 
            Func<int> rerender, ComplexElementsRenderer renderer, IDememoizer dememo) {
            this.value = value;
            this.bassRenderer = renderer;
            this.height_displacement = height_displacement;
            this.rerender = rerender;
            this.dememo = dememo;
            Create();
        }

        public override CurrentValue CurrentValue {
            get {

                /*
                if ((visibility_change_triggered && value.addinfo == "visible")  // the control has just become visible, and was previously hidden! T
                                                                                 // Thus we don't collect the current state, because it was hidden until now!
                    ||
                    (!visibility_change_triggered && string.IsNullOrEmpty(value.addinfo))) // the constrol has remained hidden, so we don't collect the state!!!
                {
                    // from the GUI!
                    visibility_change_triggered = false;
                    return value;
                }
                */

                return bassRenderer.CurrentValue;
            }
        }

        private void Create() {

            var btn = ControlVisibilityButton();
            this.Controls.Add(btn);
            bassRenderer.Location = new Point(25, 0);
            this.Controls.Add(bassRenderer);
            this.Size = new Size(bassRenderer.Size.Width, bassRenderer.Size.Height + height_displacement);
        }

        private Control ControlVisibilityButton() {

            Control ctr = new Control();
            ctr.Size = new System.Drawing.Size(25, 2 * height_displacement);

            Button showButton = new Button();
            showButton.Size = new System.Drawing.Size(25, height_displacement);
            showButton.Text = value.addinfo == "visible" ? "-" : "+";

            showButton.Click += (evnt, e) => {
                if (value.addinfo == "visible")
                    value.addinfo = "";
                else
                    value.addinfo = "visible";

                visibility_change_triggered = true;
                rerender();
            };
            showButton.BackColor = Color.White;

            Button showAllButton = new Button();
            showAllButton.Size = new System.Drawing.Size(25, height_displacement);
            showAllButton.Location = new System.Drawing.Point(0, height_displacement);
            showAllButton.Text = "A";
            showAllButton.Click += (evnt, e) => {
                setAllToVisible(value);
                rerender();
            };
            showAllButton.BackColor = Color.White;

            ctr.Controls.Add(showButton);
            ctr.Controls.Add(showAllButton);

            return ctr;
        }

        private void setAllToVisible(CurrentValue value) {

            if (string.IsNullOrEmpty(value.addinfo)) {
                value.addinfo = "visible";
                visibility_change_triggered = true;
            }

            value.dependencies?.ToList().ForEach(x => { setAllToVisible(x); });
            value.elements?.ToList().ForEach(x => { setAllToVisible(x); });
            
        }

    }
}

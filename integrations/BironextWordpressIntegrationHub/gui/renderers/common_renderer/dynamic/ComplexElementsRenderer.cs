using gui_generator;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace gui_gen
{

    public class ComplexElementsRenderer : CurrentValueControl {

        CurrentValue mainCurrentValue;
        List<CurrentValueControl> nextLevelControls;

        // these functions are just for switching between using dependencies or elements member of the current value.
        Action<CurrentValue[]> assignIterable;
        Func<CurrentValue, CurrentValue[]> iterableFromCurrentValue;

        int height_displacement;
        Func<int> rerender;
        int depth;
        IDememoizer dememo;
        bool addDelete;
        int elementWidth;
        public ComplexElementsRenderer(int width, CurrentValue value, 
        Func<int> rerender, 
        int height_displacement, 
        int depth,
        Action<CurrentValue[]> assignIterable,
        Func<CurrentValue, CurrentValue[]> iterableFromCurrentValue,
        IDememoizer dememo,
        bool addDelete) {
            this.elementWidth = width;
            this.mainCurrentValue = value;
            this.height_displacement = height_displacement;
            this.rerender = rerender;
            this.depth = depth;
            nextLevelControls = new List<CurrentValueControl>();
            this.assignIterable = assignIterable;
            this.iterableFromCurrentValue = iterableFromCurrentValue;
            this.dememo = dememo;
            this.addDelete = addDelete;
            CreatControl(width);
        }

        public override CurrentValue CurrentValue {
            get {

                if (this is ClassRenderer)
                {
                    /*
                    Sadly when this was developed I wasn't smart enough to put an elaborate
                    comment over here of why the below if (...) return value; is needed, but
                    I am sure that no expansions of class elements (ClassRenderer) will work if it is removed.
                    This is hinted at by the old comment below - "during implementation option change".

                    However now we enter this part IFF $this is of type ClassRenderer, because if
                    $this is a ListRenderer or DictionaryRenderer, then in the case of deleting the
                    last element - nextLevelControls will become null, and because the value will
                    be returned over here, the below assignIterable call will never happen, therefore
                    the root CurrentValue will never be updated - hence it then becomes impossible to
                    delete the last element in a list or dictionary. Thus in ListRenderer or DictionaryRenderer
                    we NEED to call assignIterable EVERY TIME!
                     */
                    if (nextLevelControls == null || nextLevelControls.Count == 0)
                        return mainCurrentValue; // during implementation option change!
                }
                assignIterable(nextLevelControls.Select(x => x.CurrentValue).ToArray());

                //value.implementationOptions = null;
                return mainCurrentValue;
            }
        }

        void onImplementationOptionChosen(CurrentValue chosen) {
            mainCurrentValue = chosen;
            nextLevelControls = null;
            rerender();
        }

        Control showButton;
        private void CreatControl(int width) {

            if (mainCurrentValue.variable == "simpleProductSyncer") {
                
            }

            if (mainCurrentValue.addinfo == "visible")
            {
                mainCurrentValue = dememo.Expand(mainCurrentValue);
            }
            else {
                mainCurrentValue = dememo.Explode(mainCurrentValue);
            }

            var dock = new ComplexElementsDockRenderer((int)(0.5 * width), mainCurrentValue, depth, 
                (x) => onImplementationOptionChosen(x))
                .RenderDock(height_displacement);

            this.Controls.Add(dock);
            int currhdisplace = height_displacement;
            if (mainCurrentValue.addinfo == "visible") {
                currhdisplace = RenderNextLevel(currhdisplace);


                if (addDelete) {
                    Button some = new Button();
                    some.Text = "ADD";
                    some.Size = new Size(50, 30);
                    some.BackColor = Color.White;
                    some.ForeColor = Color.Black;
                    some.Location = new System.Drawing.Point(25, currhdisplace);
                    some.Click += (x,e) => onAdd();
                    this.Controls.Add(some);
                    currhdisplace += some.Height + 5;
                }
                
            }
            this.Size = new System.Drawing.Size(width, currhdisplace);
            this.BackColor = ComplexElementsRendererHelper.DetermineControlBackcolor(mainCurrentValue, depth);
        }

        void onDelete(int index) {
            
            DialogResult result = MessageBox.Show("Ste prepričani da želite izbrisati element?",
                "Brisanje elementa", 
                MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {
                nextLevelControls.RemoveAt(index);
                rerender();
            }
        }

        void onAdd() {


            CurrentValue next = null;
            if (nextLevelControls.Count != 0) {
                next = JsonConvert.DeserializeObject<CurrentValue>(JsonConvert.SerializeObject(nextLevelControls[0].CurrentValue));
                var tmp = JsonConvert.SerializeObject(next);
            } else {

                if (mainCurrentValue.typeCategory == "list")
                {
                    string type = mainCurrentValue.type;
                    next = createElement(type);
                }
                else if (mainCurrentValue.typeCategory == "dictionary") {
                    string[] typePair = mainCurrentValue.type.Split(',');
                    var key = createElement(typePair[0]);
                    var val = createElement(typePair[1]);

                    key.addinfo = "visible";
                    val.addinfo = "visible";

                    next = new CurrentValue
                    {
                        typeCategory = "list",
                        type = "Object",
                        addinfo = "visible", // absolutely crucial - because RecursiveRenderer.recurse(next) will only add nextLevel if the node is visible. And nextLevel will get collected into the CurrentValue of the control.
                        elements = new CurrentValue[] { key, val }
                    };


                }    
            }
            if (next != null) {
                CurrentValueControl curr = new RecursiveRenderer(elementWidth, height_displacement, rerender, depth, dememo).recurse(next);
                nextLevelControls.Add(curr);
                if (mainCurrentValue.elements == null)
                    mainCurrentValue.elements = new CurrentValue[] { };
                var some = mainCurrentValue.elements.ToList();
                some.Add(next);
                mainCurrentValue.elements = some.ToArray();
            }
            rerender();

        }

        private CurrentValue createElement(string type)
        {
            CurrentValue next;
            CurrentValue next1 = null;
            if (!isObject(type) && !isPrimitive(type))
            {
                var memo = dememo.Find(type);
                next1 = dememo.Expand(memo);
            }
            else
            {
                if (type == "Boolean" || type == "@#@#Boolean")
                {
                    next1 = new CurrentValue { typeCategory = "primitive", type = "Boolean", value = "True" };
                }
                else if (type == "Int32" || type == "@#@#Int32")
                {
                    next1 = new CurrentValue { typeCategory = "primitive", type = "Int32", value = "0" };
                }
                else if (type == "String" || type == "@#@#String")
                {
                    next1 = new CurrentValue { typeCategory = "primitive", type = "String", value = "" };
                }
                else
                {
                    throw new Exception("Add operation is not supported for type " + type);
                }
            }
            next = next1;
            return next;
        }

        bool isObject(string currentValueType) {
            return currentValueType == "Object";
        }

        bool isPrimitive(string currentValueType) {
            return currentValueType == "Boolean" || currentValueType == "@#@#Boolean" || currentValueType == "Int32" || currentValueType == "@#@#Int32"
                || currentValueType == "String" || currentValueType == "@#@#String";
        }



        private int RenderNextLevel(int currhdisplace) {
            CurrentValue[] toIterate = null;
            toIterate = iterableFromCurrentValue(mainCurrentValue);
            if (toIterate == null) {
                toIterate = new CurrentValue[] { };
            }
            for (int i = 0; i < toIterate.Length; i++)
            {
                CurrentValueControl curr = new RecursiveRenderer(elementWidth, height_displacement, rerender, depth, dememo).recurse(toIterate[i]);
                nextLevelControls.Add(curr);

                Control tmp = curr;
                if (!isVariableControl(curr))
                {
                    if (addDelete)
                    {
                        tmp = new DeleteWrapperControl(curr, i, (x) => onDelete(x));
                    }
                    tmp.Location = new System.Drawing.Point(5, currhdisplace);

                    currhdisplace += tmp.Size.Height + 10;
                }
                this.Controls.Add(tmp);
                    
            }
            return currhdisplace;
        }

        private static bool isVariableControl(CurrentValueControl curr)
        {
            return curr.CurrentValue.value != null && curr.CurrentValue.value.StartsWith("$");
        }
    }







    public class DeleteWrapperControl : Control
    {
        private Button leftButton;
        private Control rightControl;
        Action<int> onDeletePressed;
        int idx;
        public DeleteWrapperControl(CurrentValueControl rightControl, int idx, Action<int> onDeletePressed)
        {
            this.onDeletePressed = onDeletePressed;
            this.idx = idx;
            InitializeComponents(rightControl);
        }

        private void InitializeComponents(CurrentValueControl x)
        {
            // Create the button
            leftButton = new Button();
            leftButton.Text = "X";
            leftButton.BackColor = Color.Red;
            leftButton.Width = 30;  // Set the width of the button
            leftButton.Click += (z,e) => onDeletePressed(idx);

            // Create the right control using the RecursiveRenderer
            rightControl = x;
            rightControl.Location = new Point(35, 0);

            // Add both controls to the CompositeControl
            this.Controls.Add(leftButton);
            this.Controls.Add(rightControl);


            this.Size = new Size(rightControl.Size.Width + leftButton.Size.Width, rightControl.Size.Height);
        }
    }



    class ComplexElementsRendererHelper {
        public static Color DetermineControlBackcolor(CurrentValue value, int depth)
        {
            string tp = value.type != null ? value.type : "hl";
            string vari = value.variable != null ? value.variable : "var";

            int r = tp.Select(x => (char)x).Aggregate(0, (x, y) => x + y) * 17;
            r = (80 + (depth * 20)) % 256;

            int g = vari.Select(x => (char)x).Aggregate(0, (x, y) => x + y);
            g = (80 + (depth * 20)) % 256;

            int b = vari.Select(x => (char)x).Aggregate(0, (x, y) => x + y) * 21;
            b = (100 + (depth * 20)) % 256;

            return Color.FromArgb(r, g, b);
        }
    }
    class ComplexElementsDockRenderer {

        CurrentValue value;
        int depth;
        Action<CurrentValue> onChoiceSelected;

        int width;
        public ComplexElementsDockRenderer(int width, CurrentValue value, int depth, Action<CurrentValue> onChoiceSelected) {
            this.width = width;
            this.value = value;
            this.depth = depth;
            this.onChoiceSelected = onChoiceSelected;
        }
          
        public Control RenderDock(int height_displacement)
        {
            if (value.implementationOptions == null || value.implementationOptions.Length == 1 || value.implementationOptions.All(x => x.type == null))
            {
                var tmp = GenericControls.CreateTextfield(width, value.variable, value.type, depth);
                tmp.Enabled = false;
                return tmp;
            }
            else
            {
                return renderComboBox(height_displacement);
            }
        }


        private Control renderComboBox(int height_displacement)
        {
            // Create a Panel to contain the Label and ComboBox
            var panel = new Panel();
            panel.Size = new System.Drawing.Size(width, height_displacement);

            // Create the Label
            var label = new Label();
            label.Text = value.variable; // Assuming value has a property labelContent
            label.Size = new System.Drawing.Size(width / 4, height_displacement);
            label.Location = new System.Drawing.Point(0, 0);
            label.BorderStyle = BorderStyle.FixedSingle;
            panel.Controls.Add(label);

            // Create the custom ComboBox
            var combo = new NoMouseWheelComboBox();
            var items = value.implementationOptions.Select(x => x.type).ToList();
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
            combo.Items.AddRange(items.ToArray());
            combo.Size = new System.Drawing.Size(3 * width / 4, height_displacement);
            combo.Location = new System.Drawing.Point(width / 4, 0);

            // Set the initial selected item
            int tm = items.IndexOf(value.type);
            if (tm >= 0)  // Ensure the index is valid
            {
                combo.SelectedIndex = tm;
            }

            // Event handler for when the selected item changes
            combo.SelectedIndexChanged += (x, y) =>
            {
                Console.WriteLine("");
                string selected = (string)((ComboBox)x).SelectedItem;
                var chosen = value.implementationOptions.Where(z => z.type == selected).Single();
                onChoiceSelected(chosen);
            };

            // Add the ComboBox to the Panel
            panel.Controls.Add(combo);

            // Optional: Adjust the colors based on depth if needed
            panel.ForeColor = GenericControls.GetFrontcolor(depth);
            panel.BackColor = GenericControls.GetBackcolor(depth);

            return panel;
        }



    }

    public class NoMouseWheelComboBox : ComboBox
    {
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            // Do nothing to ignore the mouse wheel event
            ((HandledMouseEventArgs)e).Handled = true;
        }
    }


}

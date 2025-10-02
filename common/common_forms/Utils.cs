using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace common_forms
{
    public static class Utils
    {
        /// <summary>
        /// This will prevent form duplication. If form of the same type is active it will bring it to the forefront.
        /// Otherwise it will create and display a new form.
        /// <para>Example: Utils.StartForm(() => new ExampleForm(arg1, arg2));</para>
        /// </summary>
        public static T StartForm<T>(Func<T> formFactory) where T : Form
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form.GetType() == typeof(T)) // or form is T
                {
                    form.BringToFront();
                    return (T)form;
                }
            }
            var newForm = formFactory();
            newForm.Show();
            return newForm;
        }

        public static T GetForm<T>(Func<T> formFactory) where T : Form
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form.GetType() == typeof(T)) // or form is T
                {
                    form.BringToFront();
                    return (T)form;
                }
            }
            var newForm = formFactory();
            return newForm;
        }

        public static Control[] GetAllControlsInForm(Control parent)
        {
            var allControls = GetAllControlsRecursive(parent);
            return allControls.ToArray();
        }

        private static IEnumerable<Control> GetAllControlsRecursive(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                yield return control;

                if (control.HasChildren)
                {
                    foreach (Control childControl in GetAllControlsRecursive(control))
                        yield return childControl;
                }
            }
        }

        public static void ColorFormAndControlsToDark(Form form)
        {
            form.BackColor = CustomColors.BACKGROUND_COLOR;

            var controls = GetAllControlsInForm(form);
            foreach (Control c in controls)
            {
                if (c is Label)
                {
                    c.BackColor = CustomColors.BACKGROUND_COLOR;
                    c.ForeColor = Color.White;
                }

                if (c is CheckBox)
                {
                    c.BackColor = CustomColors.BACKGROUND_COLOR;
                    c.ForeColor = Color.White;
                }

                if (c is Panel)
                {
                    c.BackColor = CustomColors.BACKGROUND_COLOR;
                }
            }
        }

        public static bool DoesControllHaveDefaultBackColor(Control control)
        {
            Color defaultBackColor = SystemColors.Control;
            return control.BackColor == defaultBackColor;
        }
    }
}

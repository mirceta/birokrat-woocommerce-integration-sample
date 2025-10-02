using common_forms.Controls;
using common_forms.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace common_forms.Forms
{
    /// <summary>
    /// Represents a form that displays a list of items in a ListView control. The form allows users to select items
    /// with a double click and is customizable with respect to the displayed columns and initial width.
    /// <para>Example of arguments:</para>
    /// <code>
    /// List&lt;string&gt; categories = new List&lt;string&gt; { "Name", "Release Date", "Version" };
    /// 
    /// List&lt;List&lt;string&gt;&gt; items = new List&lt;List&lt;string&gt;&gt;
    /// {
    ///     new List&lt;string&gt; { "Visual Studio", "2019-04-02", "16.0" },
    ///     new List&lt;string&gt; { "Notepad", "2020-12-10", "22.1" }
    /// };
    /// </code>
    /// </summary>
    public class DisplayListViewForm : Form
    {
        private readonly RichTextBoxTextFormatter _textFormatter;
        private DarkRichTextBox textBox;
        private ListView listView;
        private readonly int _width;
        private readonly int MARGIN = 15;

        private static List<string> _selectedItem;

        /// <summary>
        /// <inheritdoc cref="DisplayListViewForm"/>
        /// </summary>
        /// <param name="items">The collection of item collections to be displayed, each inner collection represents one row.</param>
        /// <param name="categories">The collection of category names to be used as column headers in the ListView.</param>
        /// <param name="label">The label text for the form's title bar. Defaults to "Select with double Click".</param>
        /// <param name="width">The initial width of the form. Defaults to 600 pixels.</param>
        public DisplayListViewForm(IEnumerable<IEnumerable<string>> items, IEnumerable<string> categories, string label = "Select with double Click", int width = 700)
        {
            this.Width = width;
            this.Height = width;

            listView = new DarkListView
            {
                Left = MARGIN,
                Top = MARGIN,  
                Width = this.ClientSize.Width - 2 * MARGIN,
                Height = this.ClientSize.Height - MARGIN * 2,
                View = View.Details,
                FullRowSelect = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top,
            };

            // Define columns in ListView based on categories
            foreach (var category in categories)
                listView.Columns.Add(category, -2, HorizontalAlignment.Left);

            // Add items to the ListView
            foreach (var item in items)
            {
                var listViewItem = new ListViewItem(item.ToArray());
                listView.Items.Add(listViewItem);
            }

            Controls.Add(textBox);
            Controls.Add(listView);

            Text = string.IsNullOrEmpty(label) ? "Info" : label;
            BackColor = CustomColors.BACKGROUND_COLOR;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;

            listView.DoubleClick += ListView_DoubleClick;

            _textFormatter = new RichTextBoxTextFormatter(textBox);
            _width = width;
        }

        private void ListView_DoubleClick(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
            {
                _selectedItem = new List<string>();

                for (int i = 0; i < listView.SelectedItems.Count; i++)
                {
                    foreach (ListViewItem.ListViewSubItem item in listView.SelectedItems[i].SubItems)
                        _selectedItem.Add(item.Text);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        /// <summary>
        /// <inheritdoc cref="DisplayListViewForm"/>
        /// </summary>
        /// <param name="items">The collection of item collections to be displayed, each inner collection represents one row.</param>
        /// <param name="categories">The collection of category names to be used as column headers in the ListView.</param>
        /// <param name="label">The label text for the form's title bar. Defaults to "Select with double Click".</param>
        /// <param name="width">The initial width of the form. Defaults to 600 pixels.</param>
        public static List<string> ShowForm(IEnumerable<IEnumerable<string>> items, IEnumerable<string> categories, string label = "", int width = 600)
        {
            using (var form = Utils.GetForm(() => new DisplayListViewForm(items, categories, label, width)))
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    return _selectedItem;
                }
                return default;
            }
        }
    }
}

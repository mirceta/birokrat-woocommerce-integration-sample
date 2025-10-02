using System;
using System.Windows.Forms;

namespace common_forms.Forms
{
    /// <summary>
    /// Represents a form with persistent Yes/No prompts that ask for confirmation multiple times.
    /// </summary>
    /// <remarks>
    /// This form repeatedly prompts the user with "Yes" or "No" options until the user confirms their choice by clicking "Yes" three times.
    /// </remarks>
    public class AnnoyingYesNoPrompt : Form
    {
        private int yesCount = 0;
        private Label promptLabel;
        private Button yesButton;
        private Button noButton;

        /// <summary>
        /// <inheritdoc cref="AnnoyingYesNoPrompt"/>
        /// </summary>
        ///  /// <param name="text">The text to display as the prompt message. If empty, a default message is displayed.</param>
        public AnnoyingYesNoPrompt(string text = "")
        {
            // Form settings
            Text = "Are You Sure?";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new System.Drawing.Size(400, 225);

            int buttonWidth = Width / 4;
            int controlHeight = 50;
            int margin = 15;

            // Label
            promptLabel = new Label
            {
                Left = margin,
                Top = margin,
                Text = !string.IsNullOrEmpty(text) ? text : "Are you absolutely sure\nyou want to proceed?",
                Width = Width - 2 * margin,
                Height = controlHeight,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Arial", 10),
            };

            // Yes Button
            yesButton = new Button
            {
                Text = "Yes",
                Left = Width / 3 - buttonWidth / 2,
                Width = buttonWidth,
                Height = controlHeight,
                Top = promptLabel.Top + margin * 2 + controlHeight,
                DialogResult = DialogResult.Cancel
            };
            yesButton.Click += YesButton_Click;

            // No Button
            noButton = new Button
            {
                Text = "No",
                Left = Width / 3 * 2 - buttonWidth / 2,
                Width = buttonWidth,
                Height = controlHeight,
                Top = promptLabel.Top + margin * 2 + controlHeight,
                DialogResult = DialogResult.Cancel
            };
            noButton.Click += NoButton_Click;

            // Add controls to the form
            Controls.Add(promptLabel);
            Controls.Add(yesButton);
            Controls.Add(noButton);
        }

        private void YesButton_Click(object sender, EventArgs e)
        {
            yesCount++;
            if (yesCount >= 3)
            {
                DialogResult = DialogResult.Yes;
                Close();
            }
            else
            {
                switch (yesCount)
                {
                    case 1:
                        promptLabel.Text = "Really?";
                        break;
                    case 2:
                        promptLabel.Text = "Really really?";
                        break;
                    default:
                        break;
                }
                DialogResult = DialogResult.None;
            }
        }

        private void NoButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
            Close();
        }

        /// <summary>
        /// Displays the form as a modal dialog and returns the result of user interaction.
        /// </summary>
        /// <param name="text">The text to display in the prompt.</param>
        /// <returns>True if the user ultimately chooses "Yes"; otherwise, false.</returns>
        public static bool ShowForm(string text = "")
        {
            using (var window = new AnnoyingYesNoPrompt(text))
            {
                DialogResult result = window.ShowDialog();
                return result == DialogResult.Yes;
            }
        }
    }
}

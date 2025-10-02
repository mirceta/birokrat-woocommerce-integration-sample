using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace common_forms.Forms
{
    /// <summary>
    /// Represents a semi-transparent overlay that can cover a specified form to indicate processing or waiting. Implements IDisposable
    /// so it can simply be used as: using var overlay = OverlayForm.ShowForm(this);
    /// </summary>
    public class OverlayForm : Form, IDisposable
    {
        private bool isActive;
        private Form _formToCoverRef;

        private Label lblProgress;
        private Panel pnlProgress;

        /// <summary>
        /// <inheritdoc cref="OverlayForm"/>
        /// </summary>
        /// <param name="formToCover">The form that the overlay will cover.</param>
        /// <param name="opacity">The opacity level of the overlay.</param>
        public OverlayForm(Form formToCover, float opacity = .5f)
        {
            InitializeComponent();
            _formToCoverRef = formToCover;

            this.BackColor = Color.Black;
            this.Opacity = opacity;
            this.FormBorderStyle = FormBorderStyle.None;
            this.ControlBox = false;
            this.ShowInTaskbar = false;
            this.Owner = formToCover;
            this.StartPosition = FormStartPosition.Manual;
            this.AutoScaleMode = AutoScaleMode.None;
            this.Location = _formToCoverRef.PointToScreen(Point.Empty);
            this.ClientSize = _formToCoverRef.ClientSize;
            this.FormClosing += OverlayForm_FormClosing;

            var size = this.Width / 9;
            var posY = this.Height / 2 - size;

            pnlProgress.Width = size;
            pnlProgress.Height = size;
            pnlProgress.Location = new Point(size, posY);

            var size2 = size * 1f;
            lblProgress.Text = "Working...";
            lblProgress.ForeColor = Color.White;
            lblProgress.Location = new Point(this.Width / 2 - lblProgress.Width / 2, pnlProgress.Location.Y + (int)size2);

            isActive = true;
            var counter = 0;

            _ = Task.Run(async () =>
            {
                while (isActive)
                {
                    try
                    {
                        switch (counter)
                        {
                            case 0:
                                SetPanelPosition(pnlProgress, size, posY);
                                break;
                            case 1:
                                SetPanelPosition(pnlProgress, size * 3, posY);
                                break;
                            case 2:
                                SetPanelPosition(pnlProgress, size * 5, posY);
                                break;
                            default:
                                SetPanelPosition(pnlProgress, size * 7, posY);
                                break;
                        }
                        counter = counter < 3 ? counter + 1 : 0;
                    }
                    catch (Exception ex)
                    {
                        await Console.Out.WriteLineAsync(ex.Message);
                    }
                    finally
                    {
                        await Task.Delay(150);
                    }
                }
            });

            _formToCoverRef.Move += WhenFormToCoverMoves;
            _formToCoverRef.Resize += WhenFormToCoverResizes;

            Show();
        }

        private void InitializeComponent()
        {
            lblProgress = new Label
            {
                AutoSize = true,
                Font = new Font("Arial", 15.2F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = SystemColors.InfoText,
                Location = new Point(358, 320), // Default position
                Size = new Size(87, 30),
                Text = "Working...",
                Name = "lblProgress"
            };

            pnlProgress = new Panel
            {
                BackColor = SystemColors.HighlightText,
                Location = new Point(263, 115), // Default position
                Size = new Size(250, 125),
                Name = "pnlProgress"
            };

            this.Controls.Add(pnlProgress);
            this.Controls.Add(lblProgress);
        }

        private void WhenFormToCoverResizes(object sender, EventArgs e)
        {
            this.Location = _formToCoverRef.PointToScreen(Point.Empty);
            this.ClientSize = _formToCoverRef.ClientSize;
        }

        private void WhenFormToCoverMoves(Object sender, EventArgs e)
        {
            this.Location = _formToCoverRef.PointToScreen(Point.Empty);
        }

        private void OverlayForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            isActive = false;
            _formToCoverRef.Move -= WhenFormToCoverMoves;
            _formToCoverRef.Move -= WhenFormToCoverResizes;
        }

        private void SetPanelPosition(Panel panel, int posX, int posY)
        {
            if (panel.InvokeRequired)
            {
                panel.BeginInvoke((MethodInvoker)delegate ()
                {
                    panel.Location = new Point(posX, posY);
                });
            }
            else
            {
                panel.Location = new Point(posX, posY);
            }
        }

        public void SetText(string text)
        {
            if (lblProgress.InvokeRequired)
            {
                lblProgress.BeginInvoke((MethodInvoker)delegate ()
                {
                    lblProgress.Text = text;
                });
            }
            else
            {
                lblProgress.Text = text;
            }
        }

        public static OverlayForm ShowForm(Form formToCover, float opacity = .5f)
        {
            return Utils.StartForm(() => new OverlayForm(formToCover, opacity));
        }
    }
}

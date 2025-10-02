
namespace gui_gen {
    partial class ResultCompare {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.rtbLeft = new System.Windows.Forms.RichTextBox();
            this.rtbRight = new System.Windows.Forms.RichTextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // rtbLeft
            // 
            this.rtbLeft.Location = new System.Drawing.Point(10, 94);
            this.rtbLeft.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.rtbLeft.Name = "rtbLeft";
            this.rtbLeft.Size = new System.Drawing.Size(633, 632);
            this.rtbLeft.TabIndex = 0;
            this.rtbLeft.Text = "";
            // 
            // rtbRight
            // 
            this.rtbRight.Location = new System.Drawing.Point(648, 94);
            this.rtbRight.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.rtbRight.Name = "rtbRight";
            this.rtbRight.Size = new System.Drawing.Size(634, 633);
            this.rtbRight.TabIndex = 1;
            this.rtbRight.Text = "";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(10, 44);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(222, 45);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "NADALJUJ";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(238, 44);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(222, 45);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "PREKINI";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // ResultCompare
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1294, 738);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.rtbRight);
            this.Controls.Add(this.rtbLeft);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "ResultCompare";
            this.Text = "ResultCompare";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbLeft;
        private System.Windows.Forms.RichTextBox rtbRight;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}
namespace config_packer_test
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tbTest1 = new System.Windows.Forms.TextBox();
            tbTest2 = new System.Windows.Forms.TextBox();
            SuspendLayout();
            // 
            // tbTest1
            // 
            tbTest1.Location = new System.Drawing.Point(72, 36);
            tbTest1.Name = "tbTest1";
            tbTest1.Size = new System.Drawing.Size(125, 27);
            tbTest1.TabIndex = 0;
            // 
            // tbTest2
            // 
            tbTest2.Location = new System.Drawing.Point(72, 69);
            tbTest2.Name = "tbTest2";
            tbTest2.Size = new System.Drawing.Size(125, 27);
            tbTest2.TabIndex = 1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(tbTest2);
            Controls.Add(tbTest1);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox tbTest1;
        private System.Windows.Forms.TextBox tbTest2;
    }
}

namespace playground_forms
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
            richTextBox1 = new System.Windows.Forms.RichTextBox();
            tbInput = new System.Windows.Forms.TextBox();
            btnGo = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // richTextBox1
            // 
            richTextBox1.Location = new System.Drawing.Point(29, 112);
            richTextBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new System.Drawing.Size(812, 732);
            richTextBox1.TabIndex = 0;
            richTextBox1.Text = "";
            // 
            // tbInput
            // 
            tbInput.Location = new System.Drawing.Point(38, 39);
            tbInput.Name = "tbInput";
            tbInput.Size = new System.Drawing.Size(312, 23);
            tbInput.TabIndex = 1;
            // 
            // btnGo
            // 
            btnGo.Location = new System.Drawing.Point(365, 40);
            btnGo.Name = "btnGo";
            btnGo.Size = new System.Drawing.Size(75, 23);
            btnGo.TabIndex = 2;
            btnGo.Text = "GO";
            btnGo.UseVisualStyleBackColor = true;
            btnGo.Click += btnGo_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(865, 851);
            Controls.Add(btnGo);
            Controls.Add(tbInput);
            Controls.Add(richTextBox1);
            Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.TextBox tbInput;
        private System.Windows.Forms.Button btnGo;
    }
}

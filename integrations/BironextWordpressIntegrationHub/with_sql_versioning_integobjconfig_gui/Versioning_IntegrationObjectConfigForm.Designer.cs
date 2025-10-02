namespace with_sql_versioning_integobjconfig_gui
{
    partial class Versioning_IntegrationObjectConfigForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.Button btnViewChangesSinceLastVersion;
            pnlVersionData = new System.Windows.Forms.Panel();
            pnlActions = new System.Windows.Forms.Panel();
            pnlConfig = new System.Windows.Forms.Panel();
            pnlHistory = new System.Windows.Forms.Panel();
            btnResetChanges = new System.Windows.Forms.Button();
            btnViewChangesSinceLastVersion = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // pnlVersionData
            // 
            pnlVersionData.Location = new System.Drawing.Point(1117, 2);
            pnlVersionData.Name = "pnlVersionData";
            pnlVersionData.Size = new System.Drawing.Size(662, 109);
            pnlVersionData.TabIndex = 0;
            // 
            // pnlActions
            // 
            pnlActions.Location = new System.Drawing.Point(1117, 117);
            pnlActions.Name = "pnlActions";
            pnlActions.Size = new System.Drawing.Size(662, 138);
            pnlActions.TabIndex = 1;
            // 
            // pnlConfig
            // 
            pnlConfig.Location = new System.Drawing.Point(3, 2);
            pnlConfig.Name = "pnlConfig";
            pnlConfig.Size = new System.Drawing.Size(1097, 948);
            pnlConfig.TabIndex = 2;
            // 
            // pnlHistory
            // 
            pnlHistory.Location = new System.Drawing.Point(1117, 304);
            pnlHistory.Name = "pnlHistory";
            pnlHistory.Size = new System.Drawing.Size(653, 637);
            pnlHistory.TabIndex = 3;
            // 
            // btnResetChanges
            // 
            btnResetChanges.Location = new System.Drawing.Point(1120, 262);
            btnResetChanges.Name = "btnResetChanges";
            btnResetChanges.Size = new System.Drawing.Size(156, 36);
            btnResetChanges.TabIndex = 4;
            btnResetChanges.Text = "RESET CHANGES";
            btnResetChanges.UseVisualStyleBackColor = true;
            btnResetChanges.Click += btnResetChanges_Click;
            // 
            // btnViewChangesSinceLastVersion
            // 
            btnViewChangesSinceLastVersion.Location = new System.Drawing.Point(1286, 264);
            btnViewChangesSinceLastVersion.Name = "btnViewChangesSinceLastVersion";
            btnViewChangesSinceLastVersion.Size = new System.Drawing.Size(350, 34);
            btnViewChangesSinceLastVersion.TabIndex = 5;
            btnViewChangesSinceLastVersion.Text = "VIEW CHANGES SINCE LAST VERSION";
            btnViewChangesSinceLastVersion.UseVisualStyleBackColor = true;
            btnViewChangesSinceLastVersion.Click += btnViewChangesSinceLastVersion_Click;
            // 
            // Versioning_IntegrationObjectConfigForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1782, 953);
            Controls.Add(btnViewChangesSinceLastVersion);
            Controls.Add(btnResetChanges);
            Controls.Add(pnlHistory);
            Controls.Add(pnlConfig);
            Controls.Add(pnlActions);
            Controls.Add(pnlVersionData);
            Name = "Versioning_IntegrationObjectConfigForm";
            Text = "Versioning_IntegrationObjectConfigForm";
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlVersionData;
        private System.Windows.Forms.Panel pnlActions;
        private System.Windows.Forms.Panel pnlConfig;
        private System.Windows.Forms.Panel pnlHistory;
        private System.Windows.Forms.Button btnResetChanges;
    }
}
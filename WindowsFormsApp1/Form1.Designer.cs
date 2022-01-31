
namespace WindowsFormsApp1
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            this.lblDirectory = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtDirectory = new System.Windows.Forms.TextBox();
            this.btnProcess = new System.Windows.Forms.Button();
            this.btnProcessXml = new System.Windows.Forms.Button();
            this.txtTables = new System.Windows.Forms.TextBox();
            this.btnCheckTables = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.label2 = new System.Windows.Forms.Label();
            this.txtDirectoryToSave = new System.Windows.Forms.TextBox();
            this.btnBrowseLocationToSaveFiles = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblDirectory
            // 
            this.lblDirectory.AutoSize = true;
            this.lblDirectory.Location = new System.Drawing.Point(40, 44);
            this.lblDirectory.Name = "lblDirectory";
            this.lblDirectory.Size = new System.Drawing.Size(207, 13);
            this.lblDirectory.TabIndex = 3;
            this.lblDirectory.Text = "Directory where the SSIS packages reside";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(645, 38);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(32, 23);
            this.btnBrowse.TabIndex = 6;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtDirectory
            // 
            this.txtDirectory.Location = new System.Drawing.Point(253, 41);
            this.txtDirectory.Name = "txtDirectory";
            this.txtDirectory.Size = new System.Drawing.Size(386, 20);
            this.txtDirectory.TabIndex = 5;
            this.txtDirectory.Text = "C:\\temp\\BarcSafe\\";
            // 
            // btnProcess
            // 
            this.btnProcess.Location = new System.Drawing.Point(683, 38);
            this.btnProcess.Name = "btnProcess";
            this.btnProcess.Size = new System.Drawing.Size(200, 23);
            this.btnProcess.TabIndex = 4;
            this.btnProcess.Text = "Create documentation in text file";
            this.toolTip1.SetToolTip(this.btnProcess, "The Data Flow Task, Execute SQL tasks will be analysed on embedded SQL and the SQ" +
        "L code will be put into text files...");
            this.btnProcess.UseVisualStyleBackColor = true;
            this.btnProcess.Click += new System.EventHandler(this.btnProcess_Click);
            // 
            // btnProcessXml
            // 
            this.btnProcessXml.Location = new System.Drawing.Point(683, 65);
            this.btnProcessXml.Name = "btnProcess";
            this.btnProcessXml.Size = new System.Drawing.Size(200, 23);
            this.btnProcessXml.TabIndex = 4;
            this.btnProcessXml.Text = "Create documentation in XML file";
            this.toolTip1.SetToolTip(this.btnProcessXml, "The Data Flow Task, Execute SQL tasks will be analysed on embedded SQL and the SQL code will be put into text files...");
            this.btnProcessXml.UseVisualStyleBackColor = true;
            this.btnProcessXml.Click += new System.EventHandler(this.btnProcessXml_Click);
            // 
            // txtTables
            // 
            this.txtTables.Location = new System.Drawing.Point(43, 96);
            this.txtTables.Multiline = true;
            this.txtTables.Name = "txtTables";
            this.txtTables.Size = new System.Drawing.Size(634, 228);
            this.txtTables.TabIndex = 7;
            // 
            // btnCheckTables
            // 
            this.btnCheckTables.Location = new System.Drawing.Point(683, 301);
            this.btnCheckTables.Name = "btnCheckTables";
            this.btnCheckTables.Size = new System.Drawing.Size(150, 23);
            this.btnCheckTables.TabIndex = 8;
            this.btnCheckTables.Text = "Check tables";
            this.toolTip1.SetToolTip(this.btnCheckTables, "Checks if the table exists inside the SSIS package (either in embedded SQL or ins" +
        "ide the controls)");
            this.btnCheckTables.UseVisualStyleBackColor = true;
            this.btnCheckTables.Click += new System.EventHandler(this.btnCheckTables_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(40, 80);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(326, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Paste a list of tables you want to check versus your SSIS packages";
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 5000;
            this.toolTip1.InitialDelay = 1000;
            this.toolTip1.ReshowDelay = 500;
            this.toolTip1.ShowAlways = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(40, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(146, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Directory to save the text files";
            // 
            // txtDirectoryToSave
            // 
            this.txtDirectoryToSave.Location = new System.Drawing.Point(253, 11);
            this.txtDirectoryToSave.Name = "txtDirectoryToSave";
            this.txtDirectoryToSave.Size = new System.Drawing.Size(386, 20);
            this.txtDirectoryToSave.TabIndex = 11;
            this.txtDirectoryToSave.Text = "C:\\temp\\BarcSafe\\txtfiles\\";
            // 
            // btnBrowseLocationToSaveFiles
            // 
            this.btnBrowseLocationToSaveFiles.Location = new System.Drawing.Point(645, 8);
            this.btnBrowseLocationToSaveFiles.Name = "btnBrowseLocationToSaveFiles";
            this.btnBrowseLocationToSaveFiles.Size = new System.Drawing.Size(32, 23);
            this.btnBrowseLocationToSaveFiles.TabIndex = 12;
            this.btnBrowseLocationToSaveFiles.Text = "...";
            this.btnBrowseLocationToSaveFiles.UseVisualStyleBackColor = true;
            this.btnBrowseLocationToSaveFiles.Click += new System.EventHandler(this.btnBrowseLocationToSaveFiles_Click);
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(897, 360);
            this.Controls.Add(this.btnBrowseLocationToSaveFiles);
            this.Controls.Add(this.txtDirectoryToSave);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCheckTables);
            this.Controls.Add(this.txtTables);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtDirectory);
            this.Controls.Add(this.btnProcess);
            this.Controls.Add(this.btnProcessXml);
            this.Controls.Add(this.lblDirectory);
            this.Name = "Form1";
            this.Text = "SSIS Package analyzer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
   
        private System.Windows.Forms.Label lblDirectory;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtDirectory;
        private System.Windows.Forms.Button btnProcess;
        private System.Windows.Forms.Button btnProcessXml;
        private System.Windows.Forms.TextBox txtTables;
        private System.Windows.Forms.Button btnCheckTables;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtDirectoryToSave;
        private System.Windows.Forms.Button btnBrowseLocationToSaveFiles;
    }
}


namespace LegendsViewer.Controls.Tabs
{
    partial class SitesTab
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tcSites = new System.Windows.Forms.TabControl();
            this.tpSiteSearch = new System.Windows.Forms.TabPage();
            this.btnSiteListReset = new System.Windows.Forms.Button();
            this.lblSiteList = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.radSiteBeastAttacks = new System.Windows.Forms.RadioButton();
            this.radSiteSortDeaths = new System.Windows.Forms.RadioButton();
            this.radSortConnections = new System.Windows.Forms.RadioButton();
            this.cmbSitePopulation = new System.Windows.Forms.ComboBox();
            this.radSiteSortPopulation = new System.Windows.Forms.RadioButton();
            this.radSiteSortWarfare = new System.Windows.Forms.RadioButton();
            this.radSiteSortFiltered = new System.Windows.Forms.RadioButton();
            this.radSiteOwners = new System.Windows.Forms.RadioButton();
            this.radSiteNone = new System.Windows.Forms.RadioButton();
            this.radSiteSortEvents = new System.Windows.Forms.RadioButton();
            this.label26 = new System.Windows.Forms.Label();
            this.cmbSiteType = new System.Windows.Forms.ComboBox();
            this.txtSiteSearch = new System.Windows.Forms.TextBox();
            this.listSiteSearch = new System.Windows.Forms.ListBox();
            this.btnSiteSearch = new System.Windows.Forms.Button();
            this.tpSiteEvents = new System.Windows.Forms.TabPage();
            this.tcSites.SuspendLayout();
            this.tpSiteSearch.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.SuspendLayout();
            // 
            // tcSites
            // 
            this.tcSites.Controls.Add(this.tpSiteSearch);
            this.tcSites.Controls.Add(this.tpSiteEvents);
            this.tcSites.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcSites.Location = new System.Drawing.Point(0, 0);
            this.tcSites.Name = "tcSites";
            this.tcSites.SelectedIndex = 0;
            this.tcSites.Size = new System.Drawing.Size(269, 518);
            this.tcSites.TabIndex = 1;
            // 
            // tpSiteSearch
            // 
            this.tpSiteSearch.Controls.Add(this.btnSiteListReset);
            this.tpSiteSearch.Controls.Add(this.lblSiteList);
            this.tpSiteSearch.Controls.Add(this.groupBox1);
            this.tpSiteSearch.Controls.Add(this.txtSiteSearch);
            this.tpSiteSearch.Controls.Add(this.listSiteSearch);
            this.tpSiteSearch.Controls.Add(this.btnSiteSearch);
            this.tpSiteSearch.Location = new System.Drawing.Point(4, 22);
            this.tpSiteSearch.Name = "tpSiteSearch";
            this.tpSiteSearch.Padding = new System.Windows.Forms.Padding(3);
            this.tpSiteSearch.Size = new System.Drawing.Size(261, 492);
            this.tpSiteSearch.TabIndex = 0;
            this.tpSiteSearch.Text = "Search";
            this.tpSiteSearch.UseVisualStyleBackColor = true;
            // 
            // btnSiteListReset
            // 
            this.btnSiteListReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSiteListReset.Location = new System.Drawing.Point(6, 241);
            this.btnSiteListReset.Name = "btnSiteListReset";
            this.btnSiteListReset.Size = new System.Drawing.Size(50, 20);
            this.btnSiteListReset.TabIndex = 43;
            this.btnSiteListReset.Text = "Reset";
            this.btnSiteListReset.UseVisualStyleBackColor = true;
            this.btnSiteListReset.Click += new System.EventHandler(this.ResetSiteBaseList);
            // 
            // lblSiteList
            // 
            this.lblSiteList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSiteList.Location = new System.Drawing.Point(61, 245);
            this.lblSiteList.Name = "lblSiteList";
            this.lblSiteList.Size = new System.Drawing.Size(198, 36);
            this.lblSiteList.TabIndex = 42;
            this.lblSiteList.Text = "All";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.groupBox6);
            this.groupBox1.Controls.Add(this.label26);
            this.groupBox1.Controls.Add(this.cmbSiteType);
            this.groupBox1.Location = new System.Drawing.Point(3, 284);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(254, 202);
            this.groupBox1.TabIndex = 34;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Filter / Sort";
            // 
            // groupBox6
            // 
            this.groupBox6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox6.Controls.Add(this.radSiteBeastAttacks);
            this.groupBox6.Controls.Add(this.radSiteSortDeaths);
            this.groupBox6.Controls.Add(this.radSortConnections);
            this.groupBox6.Controls.Add(this.cmbSitePopulation);
            this.groupBox6.Controls.Add(this.radSiteSortPopulation);
            this.groupBox6.Controls.Add(this.radSiteSortWarfare);
            this.groupBox6.Controls.Add(this.radSiteSortFiltered);
            this.groupBox6.Controls.Add(this.radSiteOwners);
            this.groupBox6.Controls.Add(this.radSiteNone);
            this.groupBox6.Controls.Add(this.radSiteSortEvents);
            this.groupBox6.Location = new System.Drawing.Point(6, 59);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(240, 137);
            this.groupBox6.TabIndex = 14;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Sort By";
            // 
            // radSiteBeastAttacks
            // 
            this.radSiteBeastAttacks.AutoSize = true;
            this.radSiteBeastAttacks.Location = new System.Drawing.Point(113, 87);
            this.radSiteBeastAttacks.Name = "radSiteBeastAttacks";
            this.radSiteBeastAttacks.Size = new System.Drawing.Size(91, 17);
            this.radSiteBeastAttacks.TabIndex = 23;
            this.radSiteBeastAttacks.TabStop = true;
            this.radSiteBeastAttacks.Text = "Beast Attacks";
            this.radSiteBeastAttacks.UseVisualStyleBackColor = true;
            this.radSiteBeastAttacks.CheckedChanged += new System.EventHandler(this.searchSiteList);
            // 
            // radSiteSortDeaths
            // 
            this.radSiteSortDeaths.AutoSize = true;
            this.radSiteSortDeaths.Location = new System.Drawing.Point(6, 110);
            this.radSiteSortDeaths.Name = "radSiteSortDeaths";
            this.radSiteSortDeaths.Size = new System.Drawing.Size(59, 17);
            this.radSiteSortDeaths.TabIndex = 22;
            this.radSiteSortDeaths.TabStop = true;
            this.radSiteSortDeaths.Text = "Deaths";
            this.radSiteSortDeaths.UseVisualStyleBackColor = true;
            this.radSiteSortDeaths.CheckedChanged += new System.EventHandler(this.searchSiteList);
            // 
            // radSortConnections
            // 
            this.radSortConnections.AutoSize = true;
            this.radSortConnections.Location = new System.Drawing.Point(113, 64);
            this.radSortConnections.Name = "radSortConnections";
            this.radSortConnections.Size = new System.Drawing.Size(84, 17);
            this.radSortConnections.TabIndex = 21;
            this.radSortConnections.TabStop = true;
            this.radSortConnections.Text = "Connections";
            this.radSortConnections.UseVisualStyleBackColor = true;
            this.radSortConnections.CheckedChanged += new System.EventHandler(this.searchSiteList);
            // 
            // cmbSitePopulation
            // 
            this.cmbSitePopulation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSitePopulation.FormattingEnabled = true;
            this.cmbSitePopulation.Location = new System.Drawing.Point(113, 41);
            this.cmbSitePopulation.Name = "cmbSitePopulation";
            this.cmbSitePopulation.Size = new System.Drawing.Size(121, 21);
            this.cmbSitePopulation.TabIndex = 20;
            this.cmbSitePopulation.SelectedIndexChanged += new System.EventHandler(this.searchSiteList);
            // 
            // radSiteSortPopulation
            // 
            this.radSiteSortPopulation.AutoSize = true;
            this.radSiteSortPopulation.Location = new System.Drawing.Point(113, 19);
            this.radSiteSortPopulation.Name = "radSiteSortPopulation";
            this.radSiteSortPopulation.Size = new System.Drawing.Size(75, 17);
            this.radSiteSortPopulation.TabIndex = 19;
            this.radSiteSortPopulation.TabStop = true;
            this.radSiteSortPopulation.Text = "Population";
            this.radSiteSortPopulation.UseVisualStyleBackColor = true;
            this.radSiteSortPopulation.CheckedChanged += new System.EventHandler(this.searchSiteList);
            // 
            // radSiteSortWarfare
            // 
            this.radSiteSortWarfare.AutoSize = true;
            this.radSiteSortWarfare.Location = new System.Drawing.Point(6, 87);
            this.radSiteSortWarfare.Name = "radSiteSortWarfare";
            this.radSiteSortWarfare.Size = new System.Drawing.Size(57, 17);
            this.radSiteSortWarfare.TabIndex = 18;
            this.radSiteSortWarfare.TabStop = true;
            this.radSiteSortWarfare.Text = "Battles";
            this.radSiteSortWarfare.UseVisualStyleBackColor = true;
            this.radSiteSortWarfare.CheckedChanged += new System.EventHandler(this.searchSiteList);
            // 
            // radSiteSortFiltered
            // 
            this.radSiteSortFiltered.AutoSize = true;
            this.radSiteSortFiltered.Location = new System.Drawing.Point(6, 42);
            this.radSiteSortFiltered.Name = "radSiteSortFiltered";
            this.radSiteSortFiltered.Size = new System.Drawing.Size(95, 17);
            this.radSiteSortFiltered.TabIndex = 17;
            this.radSiteSortFiltered.TabStop = true;
            this.radSiteSortFiltered.Text = "Filtered Events";
            this.radSiteSortFiltered.UseVisualStyleBackColor = true;
            this.radSiteSortFiltered.CheckedChanged += new System.EventHandler(this.searchSiteList);
            // 
            // radSiteOwners
            // 
            this.radSiteOwners.AutoSize = true;
            this.radSiteOwners.Location = new System.Drawing.Point(6, 65);
            this.radSiteOwners.Name = "radSiteOwners";
            this.radSiteOwners.Size = new System.Drawing.Size(91, 17);
            this.radSiteOwners.TabIndex = 16;
            this.radSiteOwners.TabStop = true;
            this.radSiteOwners.Text = "Owner History";
            this.radSiteOwners.UseVisualStyleBackColor = true;
            this.radSiteOwners.CheckedChanged += new System.EventHandler(this.searchSiteList);
            // 
            // radSiteNone
            // 
            this.radSiteNone.AutoSize = true;
            this.radSiteNone.Checked = true;
            this.radSiteNone.Location = new System.Drawing.Point(113, 110);
            this.radSiteNone.Name = "radSiteNone";
            this.radSiteNone.Size = new System.Drawing.Size(51, 17);
            this.radSiteNone.TabIndex = 15;
            this.radSiteNone.TabStop = true;
            this.radSiteNone.Text = "None";
            this.radSiteNone.UseVisualStyleBackColor = true;
            this.radSiteNone.CheckedChanged += new System.EventHandler(this.searchSiteList);
            // 
            // radSiteSortEvents
            // 
            this.radSiteSortEvents.AutoSize = true;
            this.radSiteSortEvents.Location = new System.Drawing.Point(6, 19);
            this.radSiteSortEvents.Name = "radSiteSortEvents";
            this.radSiteSortEvents.Size = new System.Drawing.Size(58, 17);
            this.radSiteSortEvents.TabIndex = 14;
            this.radSiteSortEvents.Text = "Events";
            this.radSiteSortEvents.UseVisualStyleBackColor = true;
            this.radSiteSortEvents.CheckedChanged += new System.EventHandler(this.searchSiteList);
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(6, 16);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(31, 13);
            this.label26.TabIndex = 12;
            this.label26.Text = "Type";
            // 
            // cmbSiteType
            // 
            this.cmbSiteType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSiteType.FormattingEnabled = true;
            this.cmbSiteType.Location = new System.Drawing.Point(6, 32);
            this.cmbSiteType.Name = "cmbSiteType";
            this.cmbSiteType.Size = new System.Drawing.Size(121, 21);
            this.cmbSiteType.TabIndex = 0;
            this.cmbSiteType.SelectedIndexChanged += new System.EventHandler(this.searchSiteList);
            // 
            // txtSiteSearch
            // 
            this.txtSiteSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSiteSearch.Location = new System.Drawing.Point(81, 5);
            this.txtSiteSearch.Name = "txtSiteSearch";
            this.txtSiteSearch.Size = new System.Drawing.Size(177, 20);
            this.txtSiteSearch.TabIndex = 33;
            this.txtSiteSearch.TextChanged += new System.EventHandler(this.searchSiteList);
            // 
            // listSiteSearch
            // 
            this.listSiteSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listSiteSearch.FormattingEnabled = true;
            this.listSiteSearch.Location = new System.Drawing.Point(3, 31);
            this.listSiteSearch.Name = "listSiteSearch";
            this.listSiteSearch.Size = new System.Drawing.Size(256, 186);
            this.listSiteSearch.TabIndex = 32;
            this.listSiteSearch.SelectedIndexChanged += new System.EventHandler(this.listSiteSearch_SelectedIndexChanged);
            // 
            // btnSiteSearch
            // 
            this.btnSiteSearch.Location = new System.Drawing.Point(3, 3);
            this.btnSiteSearch.Name = "btnSiteSearch";
            this.btnSiteSearch.Size = new System.Drawing.Size(75, 23);
            this.btnSiteSearch.TabIndex = 31;
            this.btnSiteSearch.Text = "Search";
            this.btnSiteSearch.UseVisualStyleBackColor = true;
            this.btnSiteSearch.Click += new System.EventHandler(this.searchSiteList);
            // 
            // tpSiteEvents
            // 
            this.tpSiteEvents.AutoScroll = true;
            this.tpSiteEvents.Location = new System.Drawing.Point(4, 22);
            this.tpSiteEvents.Name = "tpSiteEvents";
            this.tpSiteEvents.Padding = new System.Windows.Forms.Padding(3);
            this.tpSiteEvents.Size = new System.Drawing.Size(261, 492);
            this.tpSiteEvents.TabIndex = 1;
            this.tpSiteEvents.Text = "Events";
            this.tpSiteEvents.UseVisualStyleBackColor = true;
            // 
            // SitesTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tcSites);
            this.Name = "SitesTab";
            this.tcSites.ResumeLayout(false);
            this.tpSiteSearch.ResumeLayout(false);
            this.tpSiteSearch.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tcSites;
        private System.Windows.Forms.TabPage tpSiteSearch;
        private System.Windows.Forms.Button btnSiteListReset;
        private System.Windows.Forms.Label lblSiteList;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.RadioButton radSiteBeastAttacks;
        private System.Windows.Forms.RadioButton radSiteSortDeaths;
        private System.Windows.Forms.RadioButton radSortConnections;
        private System.Windows.Forms.ComboBox cmbSitePopulation;
        private System.Windows.Forms.RadioButton radSiteSortPopulation;
        private System.Windows.Forms.RadioButton radSiteSortWarfare;
        private System.Windows.Forms.RadioButton radSiteSortFiltered;
        private System.Windows.Forms.RadioButton radSiteOwners;
        private System.Windows.Forms.RadioButton radSiteNone;
        private System.Windows.Forms.RadioButton radSiteSortEvents;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.ComboBox cmbSiteType;
        private System.Windows.Forms.TextBox txtSiteSearch;
        private System.Windows.Forms.ListBox listSiteSearch;
        private System.Windows.Forms.Button btnSiteSearch;
        private System.Windows.Forms.TabPage tpSiteEvents;
    }
}

namespace DuplicateFinderMulti
{
    partial class DuplicateFinderMultiRibbon : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public DuplicateFinderMultiRibbon()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

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
      this.tabDuplicateFinderMulti = this.Factory.CreateRibbonTab();
      this.group1 = this.Factory.CreateRibbonGroup();
      this.btnShowHidePane = this.Factory.CreateRibbonButton();
      this.btnRegister = this.Factory.CreateRibbonButton();
      this.btnAbout = this.Factory.CreateRibbonButton();
      this.tabDuplicateFinderMulti.SuspendLayout();
      this.group1.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabDuplicateFinderMulti
      // 
      this.tabDuplicateFinderMulti.Groups.Add(this.group1);
      this.tabDuplicateFinderMulti.Label = "DuplicateFinderMulti";
      this.tabDuplicateFinderMulti.Name = "tabDuplicateFinderMulti";
      // 
      // group1
      // 
      this.group1.Items.Add(this.btnShowHidePane);
      this.group1.Items.Add(this.btnRegister);
      this.group1.Items.Add(this.btnAbout);
      this.group1.Label = "General";
      this.group1.Name = "group1";
      // 
      // btnShowHidePane
      // 
      this.btnShowHidePane.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
      this.btnShowHidePane.Label = "Show/Hide Pane";
      this.btnShowHidePane.Name = "btnShowHidePane";
      this.btnShowHidePane.OfficeImageId = "PageOrientationPortrait";
      this.btnShowHidePane.ShowImage = true;
      this.btnShowHidePane.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnShowHidePane_Click);
      // 
      // btnRegister
      // 
      this.btnRegister.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
      this.btnRegister.Label = "Register";
      this.btnRegister.Name = "btnRegister";
      this.btnRegister.OfficeImageId = "AdpPrimaryKey";
      this.btnRegister.ShowImage = true;
      this.btnRegister.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnRegister_Click);
      // 
      // btnAbout
      // 
      this.btnAbout.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
      this.btnAbout.Label = "About";
      this.btnAbout.Name = "btnAbout";
      this.btnAbout.OfficeImageId = "AccessTableContacts";
      this.btnAbout.ShowImage = true;
      this.btnAbout.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnAbout_Click);
      // 
      // DuplicateFinderMultiRibbon
      // 
      this.Name = "DuplicateFinderMultiRibbon";
      this.RibbonType = "Microsoft.Word.Document";
      this.Tabs.Add(this.tabDuplicateFinderMulti);
      this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.DuplicateFinderMultiRibbon_Load);
      this.tabDuplicateFinderMulti.ResumeLayout(false);
      this.tabDuplicateFinderMulti.PerformLayout();
      this.group1.ResumeLayout(false);
      this.group1.PerformLayout();
      this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab tabDuplicateFinderMulti;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup group1;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnShowHidePane;
    internal Microsoft.Office.Tools.Ribbon.RibbonButton btnRegister;
    internal Microsoft.Office.Tools.Ribbon.RibbonButton btnAbout;
  }

  partial class ThisRibbonCollection
    {
        internal DuplicateFinderMultiRibbon DuplicateFinderMultiRibbon
        {
            get { return this.GetRibbon<DuplicateFinderMultiRibbon>(); }
        }
    }
}

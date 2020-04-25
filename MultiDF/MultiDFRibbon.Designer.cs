namespace MultiDF
{
    partial class MultiDFRibbon : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public MultiDFRibbon()
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
      this.tabMultiDF = this.Factory.CreateRibbonTab();
      this.grpPane = this.Factory.CreateRibbonGroup();
      this.btnShowHidePane = this.Factory.CreateRibbonButton();
      this.grpUser = this.Factory.CreateRibbonGroup();
      this.btnLogin = this.Factory.CreateRibbonButton();
      this.btnLogout = this.Factory.CreateRibbonButton();
      this.grpProject = this.Factory.CreateRibbonGroup();
      this.btnNewProject = this.Factory.CreateRibbonButton();
      this.btnOpenProject = this.Factory.CreateRibbonMenu();
      this.btnSaveProject = this.Factory.CreateRibbonButton();
      this.separator3 = this.Factory.CreateRibbonSeparator();
      this.btnExport = this.Factory.CreateRibbonButton();
      this.btnMergeAsDOCX = this.Factory.CreateRibbonButton();
      this.btnMergeAsPDF = this.Factory.CreateRibbonButton();
      this.btnUpload = this.Factory.CreateRibbonButton();
      this.grpSourceDocs = this.Factory.CreateRibbonGroup();
      this.btnAddSourceDoc = this.Factory.CreateRibbonButton();
      this.btnRemoveSourceDoc = this.Factory.CreateRibbonButton();
      this.separator2 = this.Factory.CreateRibbonSeparator();
      this.btnUpdateQAs = this.Factory.CreateRibbonButton();
      this.btnCheckSyncWithSource = this.Factory.CreateRibbonButton();
      this.grpAnalysis = this.Factory.CreateRibbonGroup();
      this.btnProcess = this.Factory.CreateRibbonButton();
      this.btnStopProcess = this.Factory.CreateRibbonButton();
      this.separator1 = this.Factory.CreateRibbonSeparator();
      this.btnOpenResultsWindow = this.Factory.CreateRibbonButton();
      this.btnExportResults = this.Factory.CreateRibbonButton();
      this.grpAbout = this.Factory.CreateRibbonGroup();
      this.btnRegister = this.Factory.CreateRibbonButton();
      this.btnAbout = this.Factory.CreateRibbonButton();
      this.btnFixNumbering = this.Factory.CreateRibbonButton();
      this.tabMultiDF.SuspendLayout();
      this.grpPane.SuspendLayout();
      this.grpUser.SuspendLayout();
      this.grpProject.SuspendLayout();
      this.grpSourceDocs.SuspendLayout();
      this.grpAnalysis.SuspendLayout();
      this.grpAbout.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabMultiDF
      // 
      this.tabMultiDF.Groups.Add(this.grpPane);
      this.tabMultiDF.Groups.Add(this.grpUser);
      this.tabMultiDF.Groups.Add(this.grpProject);
      this.tabMultiDF.Groups.Add(this.grpSourceDocs);
      this.tabMultiDF.Groups.Add(this.grpAnalysis);
      this.tabMultiDF.Groups.Add(this.grpAbout);
      this.tabMultiDF.Label = "MultiDF";
      this.tabMultiDF.Name = "tabMultiDF";
      // 
      // grpPane
      // 
      this.grpPane.Items.Add(this.btnShowHidePane);
      this.grpPane.Label = "Pane";
      this.grpPane.Name = "grpPane";
      // 
      // btnShowHidePane
      // 
      this.btnShowHidePane.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
      this.btnShowHidePane.Label = "Show/Hide Pane";
      this.btnShowHidePane.Name = "btnShowHidePane";
      this.btnShowHidePane.OfficeImageId = "PageOrientationPortrait";
      this.btnShowHidePane.ShowImage = true;
      this.btnShowHidePane.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.ShowHidePaneButton_Click);
      // 
      // grpUser
      // 
      this.grpUser.Items.Add(this.btnLogin);
      this.grpUser.Items.Add(this.btnLogout);
      this.grpUser.Label = "User";
      this.grpUser.Name = "grpUser";
      // 
      // btnLogin
      // 
      this.btnLogin.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
      this.btnLogin.Label = "Login";
      this.btnLogin.Name = "btnLogin";
      this.btnLogin.OfficeImageId = "CheckNames";
      this.btnLogin.ShowImage = true;
      this.btnLogin.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnLogin_Click);
      // 
      // btnLogout
      // 
      this.btnLogout.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
      this.btnLogout.Label = "Logout";
      this.btnLogout.Name = "btnLogout";
      this.btnLogout.OfficeImageId = "Lock";
      this.btnLogout.ShowImage = true;
      this.btnLogout.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnLogout_Click);
      // 
      // grpProject
      // 
      this.grpProject.Items.Add(this.btnNewProject);
      this.grpProject.Items.Add(this.btnOpenProject);
      this.grpProject.Items.Add(this.btnSaveProject);
      this.grpProject.Items.Add(this.separator3);
      this.grpProject.Items.Add(this.btnExport);
      this.grpProject.Items.Add(this.btnMergeAsDOCX);
      this.grpProject.Items.Add(this.btnMergeAsPDF);
      this.grpProject.Items.Add(this.btnUpload);
      this.grpProject.Label = "Project";
      this.grpProject.Name = "grpProject";
      // 
      // btnNewProject
      // 
      this.btnNewProject.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
      this.btnNewProject.Label = "New Project";
      this.btnNewProject.Name = "btnNewProject";
      this.btnNewProject.OfficeImageId = "CheckNames";
      this.btnNewProject.ShowImage = true;
      this.btnNewProject.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnNewProject_Click);
      // 
      // btnOpenProject
      // 
      this.btnOpenProject.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
      this.btnOpenProject.Dynamic = true;
      this.btnOpenProject.Label = "Open Project";
      this.btnOpenProject.Name = "btnOpenProject";
      this.btnOpenProject.OfficeImageId = "FileOpen";
      this.btnOpenProject.ShowImage = true;
      // 
      // btnSaveProject
      // 
      this.btnSaveProject.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
      this.btnSaveProject.Label = "Save Project";
      this.btnSaveProject.Name = "btnSaveProject";
      this.btnSaveProject.OfficeImageId = "FileSave";
      this.btnSaveProject.ShowImage = true;
      this.btnSaveProject.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnSaveProject_Click);
      // 
      // separator3
      // 
      this.separator3.Name = "separator3";
      // 
      // btnExport
      // 
      this.btnExport.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
      this.btnExport.Label = "Export";
      this.btnExport.Name = "btnExport";
      this.btnExport.OfficeImageId = "WindowSaveWorkspace";
      this.btnExport.ShowImage = true;
      this.btnExport.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnExport_Click);
      // 
      // btnMergeAsDOCX
      // 
      this.btnMergeAsDOCX.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
      this.btnMergeAsDOCX.Label = "Merge as DOCX";
      this.btnMergeAsDOCX.Name = "btnMergeAsDOCX";
      this.btnMergeAsDOCX.OfficeImageId = "ExportWord";
      this.btnMergeAsDOCX.ShowImage = true;
      this.btnMergeAsDOCX.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnMergeAsDOCX_Click);
      // 
      // btnMergeAsPDF
      // 
      this.btnMergeAsPDF.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
      this.btnMergeAsPDF.Label = "Merge as PDF";
      this.btnMergeAsPDF.Name = "btnMergeAsPDF";
      this.btnMergeAsPDF.OfficeImageId = "SaveSelectionToTableOfContentsGallery";
      this.btnMergeAsPDF.ShowImage = true;
      this.btnMergeAsPDF.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnMergeAsPDF_Click);
      // 
      // btnUpload
      // 
      this.btnUpload.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
      this.btnUpload.Label = "Upload Master File";
      this.btnUpload.Name = "btnUpload";
      this.btnUpload.OfficeImageId = "OutlookGlobe";
      this.btnUpload.ShowImage = true;
      this.btnUpload.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnUpload_Click);
      // 
      // grpSourceDocs
      // 
      this.grpSourceDocs.Items.Add(this.btnAddSourceDoc);
      this.grpSourceDocs.Items.Add(this.btnRemoveSourceDoc);
      this.grpSourceDocs.Items.Add(this.separator2);
      this.grpSourceDocs.Items.Add(this.btnUpdateQAs);
      this.grpSourceDocs.Items.Add(this.btnCheckSyncWithSource);
      this.grpSourceDocs.Items.Add(this.btnFixNumbering);
      this.grpSourceDocs.Label = "Source Documents";
      this.grpSourceDocs.Name = "grpSourceDocs";
      // 
      // btnAddSourceDoc
      // 
      this.btnAddSourceDoc.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
      this.btnAddSourceDoc.Label = "Add";
      this.btnAddSourceDoc.Name = "btnAddSourceDoc";
      this.btnAddSourceDoc.OfficeImageId = "SourceControlAddObjects";
      this.btnAddSourceDoc.ShowImage = true;
      this.btnAddSourceDoc.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnAddSourceDoc_Click);
      // 
      // btnRemoveSourceDoc
      // 
      this.btnRemoveSourceDoc.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
      this.btnRemoveSourceDoc.Label = "Remove";
      this.btnRemoveSourceDoc.Name = "btnRemoveSourceDoc";
      this.btnRemoveSourceDoc.OfficeImageId = "FilePermissionRestrictMenu";
      this.btnRemoveSourceDoc.ShowImage = true;
      this.btnRemoveSourceDoc.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnRemoveSourceDoc_Click);
      // 
      // separator2
      // 
      this.separator2.Name = "separator2";
      // 
      // btnUpdateQAs
      // 
      this.btnUpdateQAs.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
      this.btnUpdateQAs.Label = "Update QAs";
      this.btnUpdateQAs.Name = "btnUpdateQAs";
      this.btnUpdateQAs.OfficeImageId = "Refresh";
      this.btnUpdateQAs.ShowImage = true;
      this.btnUpdateQAs.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnUpdateQAs_Click);
      // 
      // btnCheckSyncWithSource
      // 
      this.btnCheckSyncWithSource.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
      this.btnCheckSyncWithSource.Label = "Check with Source";
      this.btnCheckSyncWithSource.Name = "btnCheckSyncWithSource";
      this.btnCheckSyncWithSource.OfficeImageId = "DataValidation";
      this.btnCheckSyncWithSource.ShowImage = true;
      this.btnCheckSyncWithSource.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnCheckSyncWithSource_Click);
      // 
      // grpAnalysis
      // 
      this.grpAnalysis.Items.Add(this.btnProcess);
      this.grpAnalysis.Items.Add(this.btnStopProcess);
      this.grpAnalysis.Items.Add(this.separator1);
      this.grpAnalysis.Items.Add(this.btnOpenResultsWindow);
      this.grpAnalysis.Items.Add(this.btnExportResults);
      this.grpAnalysis.Label = "Duplicates";
      this.grpAnalysis.Name = "grpAnalysis";
      // 
      // btnProcess
      // 
      this.btnProcess.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
      this.btnProcess.Label = "Start Processing";
      this.btnProcess.Name = "btnProcess";
      this.btnProcess.OfficeImageId = "MacroNames";
      this.btnProcess.ShowImage = true;
      this.btnProcess.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnProcess_Click);
      // 
      // btnStopProcess
      // 
      this.btnStopProcess.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
      this.btnStopProcess.Label = "Stop Processing";
      this.btnStopProcess.Name = "btnStopProcess";
      this.btnStopProcess.OfficeImageId = "ReviewRejectChange";
      this.btnStopProcess.ShowImage = true;
      this.btnStopProcess.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnStopProcess_Click);
      // 
      // separator1
      // 
      this.separator1.Name = "separator1";
      // 
      // btnOpenResultsWindow
      // 
      this.btnOpenResultsWindow.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
      this.btnOpenResultsWindow.Label = "Open Results Window";
      this.btnOpenResultsWindow.Name = "btnOpenResultsWindow";
      this.btnOpenResultsWindow.OfficeImageId = "WindowSwitchWindowsMenuExcel";
      this.btnOpenResultsWindow.ShowImage = true;
      this.btnOpenResultsWindow.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnOpenResultsWindow_Click);
      // 
      // btnExportResults
      // 
      this.btnExportResults.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
      this.btnExportResults.Label = "Export Results";
      this.btnExportResults.Name = "btnExportResults";
      this.btnExportResults.OfficeImageId = "AccessNavigationOptions";
      this.btnExportResults.ShowImage = true;
      this.btnExportResults.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnExportResults_Click);
      // 
      // grpAbout
      // 
      this.grpAbout.Items.Add(this.btnRegister);
      this.grpAbout.Items.Add(this.btnAbout);
      this.grpAbout.Label = "About";
      this.grpAbout.Name = "grpAbout";
      // 
      // btnRegister
      // 
      this.btnRegister.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
      this.btnRegister.Label = "Register";
      this.btnRegister.Name = "btnRegister";
      this.btnRegister.OfficeImageId = "AdpPrimaryKey";
      this.btnRegister.ShowImage = true;
      this.btnRegister.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.RegisterButton_Click);
      // 
      // btnAbout
      // 
      this.btnAbout.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
      this.btnAbout.Label = "About";
      this.btnAbout.Name = "btnAbout";
      this.btnAbout.OfficeImageId = "AccessTableContacts";
      this.btnAbout.ShowImage = true;
      this.btnAbout.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.AboutButton_Click);
      // 
      // btnFixNumbering
      // 
      this.btnFixNumbering.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
      this.btnFixNumbering.Label = "Fix Numbering";
      this.btnFixNumbering.Name = "btnFixNumbering";
      this.btnFixNumbering.OfficeImageId = "FormattingUnique";
      this.btnFixNumbering.ShowImage = true;
      this.btnFixNumbering.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnFixNumbering_Click);
      // 
      // MultiDFRibbon
      // 
      this.Name = "MultiDFRibbon";
      this.RibbonType = "Microsoft.Word.Document";
      this.Tabs.Add(this.tabMultiDF);
      this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.MultiDFRibbon_Load);
      this.tabMultiDF.ResumeLayout(false);
      this.tabMultiDF.PerformLayout();
      this.grpPane.ResumeLayout(false);
      this.grpPane.PerformLayout();
      this.grpUser.ResumeLayout(false);
      this.grpUser.PerformLayout();
      this.grpProject.ResumeLayout(false);
      this.grpProject.PerformLayout();
      this.grpSourceDocs.ResumeLayout(false);
      this.grpSourceDocs.PerformLayout();
      this.grpAnalysis.ResumeLayout(false);
      this.grpAnalysis.PerformLayout();
      this.grpAbout.ResumeLayout(false);
      this.grpAbout.PerformLayout();
      this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab tabMultiDF;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup grpPane;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnShowHidePane;
    internal Microsoft.Office.Tools.Ribbon.RibbonButton btnRegister;
    internal Microsoft.Office.Tools.Ribbon.RibbonButton btnAbout;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup grpUser;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnLogin;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnLogout;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup grpProject;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnNewProject;
        internal Microsoft.Office.Tools.Ribbon.RibbonMenu btnOpenProject;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnSaveProject;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnExport;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnMergeAsPDF;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnUpload;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup grpSourceDocs;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnAddSourceDoc;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnRemoveSourceDoc;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnUpdateQAs;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnCheckSyncWithSource;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup grpAnalysis;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnProcess;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnStopProcess;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnOpenResultsWindow;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnExportResults;
    internal Microsoft.Office.Tools.Ribbon.RibbonSeparator separator3;
    internal Microsoft.Office.Tools.Ribbon.RibbonSeparator separator2;
    internal Microsoft.Office.Tools.Ribbon.RibbonSeparator separator1;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup grpAbout;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnMergeAsDOCX;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnFixNumbering;
    }

    partial class ThisRibbonCollection
    {
        internal MultiDFRibbon MultiDFRibbon
        {
            get { return this.GetRibbon<MultiDFRibbon>(); }
        }
    }
}

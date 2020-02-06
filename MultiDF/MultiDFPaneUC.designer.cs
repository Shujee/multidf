namespace MultiDF
{
  partial class MultiDFPaneUC
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
      this.EH = new System.Windows.Forms.Integration.ElementHost();
      this.SuspendLayout();
      // 
      // EH
      // 
      this.EH.Dock = System.Windows.Forms.DockStyle.Fill;
      this.EH.Location = new System.Drawing.Point(0, 0);
      this.EH.Name = "EH";
      this.EH.Size = new System.Drawing.Size(286, 582);
      this.EH.TabIndex = 0;
      this.EH.Child = null;
      // 
      // MultiDFPaneUC
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.EH);
      this.Name = "MultiDFPaneUC";
      this.Size = new System.Drawing.Size(286, 582);
      this.ResumeLayout(false);

    }

    #endregion

    internal System.Windows.Forms.Integration.ElementHost EH;
  }
}

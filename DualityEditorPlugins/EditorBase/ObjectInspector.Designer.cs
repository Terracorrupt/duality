﻿namespace EditorBase
{
	partial class ObjectInspector
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ObjectInspector));
			this.nodeStateIcon = new Aga.Controls.Tree.NodeControls.NodeStateIcon();
			this.nodeTextBoxName = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.timerSelectSched = new System.Windows.Forms.Timer(this.components);
			this.toolStrip = new System.Windows.Forms.ToolStrip();
			this.buttonAutoRefresh = new System.Windows.Forms.ToolStripButton();
			this.buttonClone = new System.Windows.Forms.ToolStripButton();
			this.propertyGrid = new DualityEditor.Controls.DualitorPropertyGrid();
			this.toolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// nodeStateIcon
			// 
			this.nodeStateIcon.DataPropertyName = "Image";
			this.nodeStateIcon.LeftMargin = 1;
			this.nodeStateIcon.ParentColumn = null;
			this.nodeStateIcon.ScaleMode = Aga.Controls.Tree.ImageScaleMode.Clip;
			// 
			// nodeTextBoxName
			// 
			this.nodeTextBoxName.DataPropertyName = "Text";
			this.nodeTextBoxName.EditEnabled = true;
			this.nodeTextBoxName.IncrementalSearchEnabled = true;
			this.nodeTextBoxName.LeftMargin = 3;
			this.nodeTextBoxName.ParentColumn = null;
			// 
			// timerSelectSched
			// 
			this.timerSelectSched.Interval = 50;
			this.timerSelectSched.Tick += new System.EventHandler(this.timerSelectSched_Tick);
			// 
			// toolStrip
			// 
			this.toolStrip.BackColor = System.Drawing.SystemColors.InactiveCaption;
			this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonAutoRefresh,
            this.buttonClone});
			this.toolStrip.Location = new System.Drawing.Point(0, 0);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Size = new System.Drawing.Size(231, 25);
			this.toolStrip.TabIndex = 1;
			this.toolStrip.Text = "toolStrip";
			// 
			// buttonAutoRefresh
			// 
			this.buttonAutoRefresh.CheckOnClick = true;
			this.buttonAutoRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonAutoRefresh.Image = global::EditorBase.Properties.Resources.arrow_refresh;
			this.buttonAutoRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonAutoRefresh.Name = "buttonAutoRefresh";
			this.buttonAutoRefresh.Size = new System.Drawing.Size(23, 22);
			this.buttonAutoRefresh.Text = "Auto-Refresh in Sandbox";
			this.buttonAutoRefresh.CheckedChanged += new System.EventHandler(this.buttonAutoRefresh_CheckedChanged);
			// 
			// buttonClone
			// 
			this.buttonClone.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonClone.Enabled = false;
			this.buttonClone.Image = global::EditorBase.Properties.Resources.page_copy;
			this.buttonClone.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonClone.Name = "buttonClone";
			this.buttonClone.Size = new System.Drawing.Size(23, 22);
			this.buttonClone.Text = "Clone View";
			this.buttonClone.Click += new System.EventHandler(this.buttonClone_Click);
			// 
			// propertyGrid
			// 
			this.propertyGrid.AllowDrop = true;
			this.propertyGrid.AutoScroll = true;
			this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.propertyGrid.Location = new System.Drawing.Point(0, 25);
			this.propertyGrid.Margin = new System.Windows.Forms.Padding(0);
			this.propertyGrid.Name = "propertyGrid";
			this.propertyGrid.ReadOnly = false;
			this.propertyGrid.Size = new System.Drawing.Size(231, 407);
			this.propertyGrid.TabIndex = 0;
			// 
			// ObjectInspector
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(231, 432);
			this.Controls.Add(this.propertyGrid);
			this.Controls.Add(this.toolStrip);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "ObjectInspector";
			this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockRight;
			this.Text = "Object Inspector";
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Aga.Controls.Tree.NodeControls.NodeTextBox nodeTextBoxName;
		private Aga.Controls.Tree.NodeControls.NodeStateIcon nodeStateIcon;
		private DualityEditor.Controls.DualitorPropertyGrid propertyGrid;
		private System.Windows.Forms.Timer timerSelectSched;
		private System.Windows.Forms.ToolStrip toolStrip;
		private System.Windows.Forms.ToolStripButton buttonAutoRefresh;
		private System.Windows.Forms.ToolStripButton buttonClone;
	}
}
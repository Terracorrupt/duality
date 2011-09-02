﻿namespace DualityEditor.Forms
{
	partial class MainForm
	{
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Verwendete Ressourcen bereinigen.
		/// </summary>
		/// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Vom Windows Form-Designer generierter Code

		/// <summary>
		/// Erforderliche Methode für die Designerunterstützung.
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
			WeifenLuo.WinFormsUI.Docking.DockPanelSkin dockPanelSkin1 = new WeifenLuo.WinFormsUI.Docking.DockPanelSkin();
			WeifenLuo.WinFormsUI.Docking.AutoHideStripSkin autoHideStripSkin1 = new WeifenLuo.WinFormsUI.Docking.AutoHideStripSkin();
			WeifenLuo.WinFormsUI.Docking.DockPanelGradient dockPanelGradient1 = new WeifenLuo.WinFormsUI.Docking.DockPanelGradient();
			WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient1 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
			WeifenLuo.WinFormsUI.Docking.DockPaneStripSkin dockPaneStripSkin1 = new WeifenLuo.WinFormsUI.Docking.DockPaneStripSkin();
			WeifenLuo.WinFormsUI.Docking.DockPaneStripGradient dockPaneStripGradient1 = new WeifenLuo.WinFormsUI.Docking.DockPaneStripGradient();
			WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient2 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
			WeifenLuo.WinFormsUI.Docking.DockPanelGradient dockPanelGradient2 = new WeifenLuo.WinFormsUI.Docking.DockPanelGradient();
			WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient3 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
			WeifenLuo.WinFormsUI.Docking.DockPaneStripToolWindowGradient dockPaneStripToolWindowGradient1 = new WeifenLuo.WinFormsUI.Docking.DockPaneStripToolWindowGradient();
			WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient4 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
			WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient5 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
			WeifenLuo.WinFormsUI.Docking.DockPanelGradient dockPanelGradient3 = new WeifenLuo.WinFormsUI.Docking.DockPanelGradient();
			WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient6 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
			WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient7 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.dockPanel = new WeifenLuo.WinFormsUI.Docking.DockPanel();
			this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
			this.BottomToolStripPanel = new System.Windows.Forms.ToolStripPanel();
			this.TopToolStripPanel = new System.Windows.Forms.ToolStripPanel();
			this.RightToolStripPanel = new System.Windows.Forms.ToolStripPanel();
			this.LeftToolStripPanel = new System.Windows.Forms.ToolStripPanel();
			this.ContentPanel = new System.Windows.Forms.ToolStripContentPanel();
			this.pluginWatcher = new System.IO.FileSystemWatcher();
			this.dataDirWatcher = new System.IO.FileSystemWatcher();
			this.sourceDirWatcher = new System.IO.FileSystemWatcher();
			this.mainToolStrip = new System.Windows.Forms.ToolStrip();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.actionSaveAll = new System.Windows.Forms.ToolStripButton();
			this.actionOpenCode = new System.Windows.Forms.ToolStripButton();
			this.actionRunApp = new System.Windows.Forms.ToolStripButton();
			this.actionDebugApp = new System.Windows.Forms.ToolStripButton();
			this.actionRunSandbox = new System.Windows.Forms.ToolStripButton();
			this.actionPauseSandbox = new System.Windows.Forms.ToolStripButton();
			this.actionStopSandbox = new System.Windows.Forms.ToolStripButton();
			((System.ComponentModel.ISupportInitialize)(this.pluginWatcher)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataDirWatcher)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sourceDirWatcher)).BeginInit();
			this.mainToolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// dockPanel
			// 
			this.dockPanel.ActiveAutoHideContent = null;
			this.dockPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dockPanel.DockBackColor = System.Drawing.SystemColors.Control;
			this.dockPanel.Location = new System.Drawing.Point(0, 49);
			this.dockPanel.Name = "dockPanel";
			this.dockPanel.ShowDocumentIcon = true;
			this.dockPanel.Size = new System.Drawing.Size(916, 639);
			dockPanelGradient1.EndColor = System.Drawing.SystemColors.ControlLight;
			dockPanelGradient1.StartColor = System.Drawing.SystemColors.ControlLight;
			autoHideStripSkin1.DockStripGradient = dockPanelGradient1;
			tabGradient1.EndColor = System.Drawing.SystemColors.Control;
			tabGradient1.StartColor = System.Drawing.SystemColors.Control;
			tabGradient1.TextColor = System.Drawing.SystemColors.ControlDarkDark;
			autoHideStripSkin1.TabGradient = tabGradient1;
			autoHideStripSkin1.TextFont = new System.Drawing.Font("Segoe UI", 9F);
			dockPanelSkin1.AutoHideStripSkin = autoHideStripSkin1;
			tabGradient2.EndColor = System.Drawing.SystemColors.ControlLightLight;
			tabGradient2.StartColor = System.Drawing.SystemColors.ControlLightLight;
			tabGradient2.TextColor = System.Drawing.SystemColors.ControlText;
			dockPaneStripGradient1.ActiveTabGradient = tabGradient2;
			dockPanelGradient2.EndColor = System.Drawing.SystemColors.Control;
			dockPanelGradient2.StartColor = System.Drawing.SystemColors.Control;
			dockPaneStripGradient1.DockStripGradient = dockPanelGradient2;
			tabGradient3.EndColor = System.Drawing.SystemColors.ControlLight;
			tabGradient3.StartColor = System.Drawing.SystemColors.ControlLight;
			tabGradient3.TextColor = System.Drawing.SystemColors.ControlText;
			dockPaneStripGradient1.InactiveTabGradient = tabGradient3;
			dockPaneStripSkin1.DocumentGradient = dockPaneStripGradient1;
			dockPaneStripSkin1.TextFont = new System.Drawing.Font("Segoe UI", 9F);
			tabGradient4.EndColor = System.Drawing.SystemColors.ActiveCaption;
			tabGradient4.LinearGradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
			tabGradient4.StartColor = System.Drawing.SystemColors.GradientActiveCaption;
			tabGradient4.TextColor = System.Drawing.SystemColors.ActiveCaptionText;
			dockPaneStripToolWindowGradient1.ActiveCaptionGradient = tabGradient4;
			tabGradient5.EndColor = System.Drawing.SystemColors.Control;
			tabGradient5.StartColor = System.Drawing.SystemColors.Control;
			tabGradient5.TextColor = System.Drawing.SystemColors.ControlText;
			dockPaneStripToolWindowGradient1.ActiveTabGradient = tabGradient5;
			dockPanelGradient3.EndColor = System.Drawing.SystemColors.ControlLight;
			dockPanelGradient3.StartColor = System.Drawing.SystemColors.ControlLight;
			dockPaneStripToolWindowGradient1.DockStripGradient = dockPanelGradient3;
			tabGradient6.EndColor = System.Drawing.SystemColors.InactiveCaption;
			tabGradient6.LinearGradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
			tabGradient6.StartColor = System.Drawing.SystemColors.GradientInactiveCaption;
			tabGradient6.TextColor = System.Drawing.SystemColors.InactiveCaptionText;
			dockPaneStripToolWindowGradient1.InactiveCaptionGradient = tabGradient6;
			tabGradient7.EndColor = System.Drawing.Color.Transparent;
			tabGradient7.StartColor = System.Drawing.Color.Transparent;
			tabGradient7.TextColor = System.Drawing.SystemColors.ControlDarkDark;
			dockPaneStripToolWindowGradient1.InactiveTabGradient = tabGradient7;
			dockPaneStripSkin1.ToolWindowGradient = dockPaneStripToolWindowGradient1;
			dockPanelSkin1.DockPaneStripSkin = dockPaneStripSkin1;
			this.dockPanel.Skin = dockPanelSkin1;
			this.dockPanel.TabIndex = 0;
			// 
			// mainMenuStrip
			// 
			this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
			this.mainMenuStrip.Name = "mainMenuStrip";
			this.mainMenuStrip.Size = new System.Drawing.Size(916, 24);
			this.mainMenuStrip.TabIndex = 2;
			this.mainMenuStrip.Text = "menuStrip1";
			// 
			// BottomToolStripPanel
			// 
			this.BottomToolStripPanel.Location = new System.Drawing.Point(0, 0);
			this.BottomToolStripPanel.Name = "BottomToolStripPanel";
			this.BottomToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.BottomToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.BottomToolStripPanel.Size = new System.Drawing.Size(0, 0);
			// 
			// TopToolStripPanel
			// 
			this.TopToolStripPanel.Location = new System.Drawing.Point(0, 0);
			this.TopToolStripPanel.Name = "TopToolStripPanel";
			this.TopToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.TopToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.TopToolStripPanel.Size = new System.Drawing.Size(0, 0);
			// 
			// RightToolStripPanel
			// 
			this.RightToolStripPanel.Location = new System.Drawing.Point(0, 0);
			this.RightToolStripPanel.Name = "RightToolStripPanel";
			this.RightToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.RightToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.RightToolStripPanel.Size = new System.Drawing.Size(0, 0);
			// 
			// LeftToolStripPanel
			// 
			this.LeftToolStripPanel.Location = new System.Drawing.Point(0, 0);
			this.LeftToolStripPanel.Name = "LeftToolStripPanel";
			this.LeftToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.LeftToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.LeftToolStripPanel.Size = new System.Drawing.Size(0, 0);
			// 
			// ContentPanel
			// 
			this.ContentPanel.Size = new System.Drawing.Size(916, 639);
			// 
			// pluginWatcher
			// 
			this.pluginWatcher.EnableRaisingEvents = true;
			this.pluginWatcher.Filter = "*.dll";
			this.pluginWatcher.IncludeSubdirectories = true;
			this.pluginWatcher.NotifyFilter = ((System.IO.NotifyFilters)((System.IO.NotifyFilters.LastWrite | System.IO.NotifyFilters.CreationTime)));
			this.pluginWatcher.Path = "Plugins";
			this.pluginWatcher.SynchronizingObject = this;
			this.pluginWatcher.Changed += new System.IO.FileSystemEventHandler(this.corePluginWatcher_Changed);
			this.pluginWatcher.Created += new System.IO.FileSystemEventHandler(this.corePluginWatcher_Changed);
			// 
			// dataDirWatcher
			// 
			this.dataDirWatcher.EnableRaisingEvents = true;
			this.dataDirWatcher.IncludeSubdirectories = true;
			this.dataDirWatcher.SynchronizingObject = this;
			this.dataDirWatcher.Changed += new System.IO.FileSystemEventHandler(this.dataDirWatcher_Changed);
			this.dataDirWatcher.Created += new System.IO.FileSystemEventHandler(this.dataDirWatcher_Created);
			this.dataDirWatcher.Deleted += new System.IO.FileSystemEventHandler(this.dataDirWatcher_Deleted);
			this.dataDirWatcher.Renamed += new System.IO.RenamedEventHandler(this.dataDirWatcher_Renamed);
			// 
			// sourceDirWatcher
			// 
			this.sourceDirWatcher.EnableRaisingEvents = true;
			this.sourceDirWatcher.IncludeSubdirectories = true;
			this.sourceDirWatcher.SynchronizingObject = this;
			this.sourceDirWatcher.Changed += new System.IO.FileSystemEventHandler(this.sourceDirWatcher_Changed);
			this.sourceDirWatcher.Created += new System.IO.FileSystemEventHandler(this.sourceDirWatcher_Created);
			this.sourceDirWatcher.Deleted += new System.IO.FileSystemEventHandler(this.sourceDirWatcher_Deleted);
			this.sourceDirWatcher.Renamed += new System.IO.RenamedEventHandler(this.sourceDirWatcher_Renamed);
			// 
			// mainToolStrip
			// 
			this.mainToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.mainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.actionSaveAll,
            this.toolStripSeparator1,
            this.actionOpenCode,
            this.toolStripSeparator2,
            this.actionRunApp,
            this.actionDebugApp,
            this.toolStripSeparator3,
            this.actionRunSandbox,
            this.actionPauseSandbox,
            this.actionStopSandbox});
			this.mainToolStrip.Location = new System.Drawing.Point(0, 24);
			this.mainToolStrip.Name = "mainToolStrip";
			this.mainToolStrip.Size = new System.Drawing.Size(916, 25);
			this.mainToolStrip.TabIndex = 4;
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
			// 
			// actionSaveAll
			// 
			this.actionSaveAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.actionSaveAll.Image = global::DualityEditor.Properties.Resources.disk_multiple;
			this.actionSaveAll.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.actionSaveAll.Name = "actionSaveAll";
			this.actionSaveAll.Size = new System.Drawing.Size(23, 22);
			this.actionSaveAll.Text = "Save All Project Data";
			this.actionSaveAll.Click += new System.EventHandler(this.actionSaveAll_Click);
			// 
			// actionOpenCode
			// 
			this.actionOpenCode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.actionOpenCode.Image = global::DualityEditor.Properties.Resources.page_white_csharp;
			this.actionOpenCode.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.actionOpenCode.Name = "actionOpenCode";
			this.actionOpenCode.Size = new System.Drawing.Size(23, 22);
			this.actionOpenCode.Text = "Open Project Sourcecode";
			this.actionOpenCode.Click += new System.EventHandler(this.actionOpenCode_Click);
			// 
			// actionRunApp
			// 
			this.actionRunApp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.actionRunApp.Image = global::DualityEditor.Properties.Resources.application_go;
			this.actionRunApp.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.actionRunApp.Name = "actionRunApp";
			this.actionRunApp.Size = new System.Drawing.Size(23, 22);
			this.actionRunApp.Text = "Run Application";
			this.actionRunApp.Click += new System.EventHandler(this.actionRunApp_Click);
			// 
			// actionDebugApp
			// 
			this.actionDebugApp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.actionDebugApp.Image = global::DualityEditor.Properties.Resources.application_bug;
			this.actionDebugApp.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.actionDebugApp.Name = "actionDebugApp";
			this.actionDebugApp.Size = new System.Drawing.Size(23, 22);
			this.actionDebugApp.Text = "Debug Application";
			this.actionDebugApp.Click += new System.EventHandler(this.actionDebugApp_Click);
			// 
			// actionRunSandbox
			// 
			this.actionRunSandbox.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.actionRunSandbox.Image = global::DualityEditor.Properties.Resources.control_play_blue;
			this.actionRunSandbox.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.actionRunSandbox.Name = "actionRunSandbox";
			this.actionRunSandbox.Size = new System.Drawing.Size(23, 22);
			this.actionRunSandbox.Text = "Enter Sandbox";
			this.actionRunSandbox.Click += new System.EventHandler(this.actionRunSandbox_Click);
			// 
			// actionPauseSandbox
			// 
			this.actionPauseSandbox.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.actionPauseSandbox.Image = global::DualityEditor.Properties.Resources.control_pause_blue;
			this.actionPauseSandbox.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.actionPauseSandbox.Name = "actionPauseSandbox";
			this.actionPauseSandbox.Size = new System.Drawing.Size(23, 22);
			this.actionPauseSandbox.Text = "Pause Sandbox";
			this.actionPauseSandbox.Click += new System.EventHandler(this.actionPauseSandbox_Click);
			// 
			// actionStopSandbox
			// 
			this.actionStopSandbox.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.actionStopSandbox.Image = global::DualityEditor.Properties.Resources.control_stop_blue;
			this.actionStopSandbox.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.actionStopSandbox.Name = "actionStopSandbox";
			this.actionStopSandbox.Size = new System.Drawing.Size(23, 22);
			this.actionStopSandbox.Text = "Leave Sandbox";
			this.actionStopSandbox.Click += new System.EventHandler(this.actionStopSandbox_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(916, 688);
			this.Controls.Add(this.dockPanel);
			this.Controls.Add(this.mainToolStrip);
			this.Controls.Add(this.mainMenuStrip);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.IsMdiContainer = true;
			this.MainMenuStrip = this.mainMenuStrip;
			this.Name = "MainForm";
			this.Text = "Dualitor";
			((System.ComponentModel.ISupportInitialize)(this.pluginWatcher)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataDirWatcher)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sourceDirWatcher)).EndInit();
			this.mainToolStrip.ResumeLayout(false);
			this.mainToolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private WeifenLuo.WinFormsUI.Docking.DockPanel dockPanel;
		private System.Windows.Forms.MenuStrip mainMenuStrip;
		private System.Windows.Forms.ToolStripPanel BottomToolStripPanel;
		private System.Windows.Forms.ToolStripPanel TopToolStripPanel;
		private System.Windows.Forms.ToolStripPanel RightToolStripPanel;
		private System.Windows.Forms.ToolStripPanel LeftToolStripPanel;
		private System.Windows.Forms.ToolStripContentPanel ContentPanel;
		private System.IO.FileSystemWatcher pluginWatcher;
		private System.IO.FileSystemWatcher dataDirWatcher;
		private System.IO.FileSystemWatcher sourceDirWatcher;
		private System.Windows.Forms.ToolStrip mainToolStrip;
		private System.Windows.Forms.ToolStripButton actionRunApp;
		private System.Windows.Forms.ToolStripButton actionDebugApp;
		private System.Windows.Forms.ToolStripButton actionSaveAll;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton actionOpenCode;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripButton actionRunSandbox;
		private System.Windows.Forms.ToolStripButton actionPauseSandbox;
		private System.Windows.Forms.ToolStripButton actionStopSandbox;
	}
}


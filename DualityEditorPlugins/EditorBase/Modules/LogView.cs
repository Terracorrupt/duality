﻿using System;
using System.Drawing;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using WeifenLuo.WinFormsUI.Docking;

using Duality;
using DualityEditor;

namespace EditorBase
{
	public partial class LogView : DockContent
	{
		private	int unseenWarnings	= 0;
		private	int	unseenErrors	= 0;


		public LogView()
		{
			this.InitializeComponent();

			this.SetStyle(ControlStyles.Opaque, true);
			this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

			this.toolStrip.Renderer = new DualityEditor.Controls.ToolStrip.DualitorToolStripProfessionalRenderer();

			Log.LogData.NewEntry += this.LogData_NewEntry;
		}
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			this.logEntryList.BindToOutput(Log.LogData);
			this.logEntryList.ScrollToEnd();
			this.MarkAsRead();

			this.DockPanel.ActiveAutoHideContentChanged += this.DockPanel_ActiveAutoHideContentChanged;
			Sandbox.Entering += this.Sandbox_Entering;
		}
		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			this.logEntryList.BindToOutput(null);

			this.DockPanel.ActiveAutoHideContentChanged -= this.DockPanel_ActiveAutoHideContentChanged;
			Sandbox.Entering -= this.Sandbox_Entering;
			Log.LogData.NewEntry -= this.LogData_NewEntry;
		}
		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			this.MarkAsRead();
			this.logEntryList.Focus();
		}
		private void DockPanel_ActiveAutoHideContentChanged(object sender, EventArgs e)
		{
			if (this.DockPanel.ActiveAutoHideContent == this)
			{
				this.MarkAsRead();
			}
		}
		
		internal void SaveUserData(System.Xml.XmlElement node)
		{
			node.SetAttribute("showMessages", this.buttonMessages.Checked.ToString(CultureInfo.InvariantCulture));
			node.SetAttribute("showWarnings", this.buttonWarnings.Checked.ToString(CultureInfo.InvariantCulture));
			node.SetAttribute("showErrors", this.buttonErrors.Checked.ToString(CultureInfo.InvariantCulture));
			node.SetAttribute("showCore", this.buttonCore.Checked.ToString(CultureInfo.InvariantCulture));
			node.SetAttribute("showEditor", this.buttonEditor.Checked.ToString(CultureInfo.InvariantCulture));
			node.SetAttribute("showGame", this.buttonGame.Checked.ToString(CultureInfo.InvariantCulture));
			node.SetAttribute("autoClear", this.checkAutoClear.Checked.ToString(CultureInfo.InvariantCulture));
		}
		internal void LoadUserData(System.Xml.XmlElement node)
		{
			bool tryParseBool;

			if (bool.TryParse(node.GetAttribute("showMessages"), out tryParseBool))
				this.buttonMessages.Checked = tryParseBool;
			if (bool.TryParse(node.GetAttribute("showWarnings"), out tryParseBool))
				this.buttonWarnings.Checked = tryParseBool;
			if (bool.TryParse(node.GetAttribute("showErrors"), out tryParseBool))
				this.buttonErrors.Checked = tryParseBool;
			if (bool.TryParse(node.GetAttribute("showCore"), out tryParseBool))
				this.buttonCore.Checked = tryParseBool;
			if (bool.TryParse(node.GetAttribute("showEditor"), out tryParseBool))
				this.buttonEditor.Checked = tryParseBool;
			if (bool.TryParse(node.GetAttribute("showGame"), out tryParseBool))
				this.buttonGame.Checked = tryParseBool;
			if (bool.TryParse(node.GetAttribute("autoClear"), out tryParseBool))
				this.checkAutoClear.Checked = tryParseBool;
		}

		private void MarkAsRead()
		{
			this.unseenErrors = 0;
			this.unseenWarnings = 0;
			this.UpdateTabText();
		}
		private void UpdateTabText()
		{
			if (this.unseenErrors > 0 && this.unseenWarnings > 0)
			{
				this.DockHandler.TabText = this.Text + string.Format(" ({0} {2}, {1} {3})", 
					this.unseenErrors, 
					this.unseenWarnings,
					PluginRes.EditorBaseRes.LogView_Errors,
					PluginRes.EditorBaseRes.LogView_Warnings);
			}
			else if (this.unseenErrors > 0)
			{
				this.DockHandler.TabText = this.Text + string.Format(" ({0} {1})", 
					this.unseenErrors,
					PluginRes.EditorBaseRes.LogView_Errors);
			}
			else if (this.unseenWarnings > 0)
			{
				this.DockHandler.TabText = this.Text + string.Format(" ({0} {1})", 
					this.unseenWarnings,
					PluginRes.EditorBaseRes.LogView_Warnings);
			}
			else
			{
				this.DockHandler.TabText = this.Text;
			}
		}

		private void buttonCore_CheckedChanged(object sender, EventArgs e)
		{
			if (!this.Visible) return;
			this.logEntryList.SetFilterFlag(LogEntryList.MessageFilter.SourceCore, this.buttonCore.Checked);
		}
		private void buttonEditor_CheckedChanged(object sender, EventArgs e)
		{
			if (!this.Visible) return;
			this.logEntryList.SetFilterFlag(LogEntryList.MessageFilter.SourceEditor, this.buttonEditor.Checked);
		}
		private void buttonGame_CheckedChanged(object sender, EventArgs e)
		{
			if (!this.Visible) return;
			this.logEntryList.SetFilterFlag(LogEntryList.MessageFilter.SourceGame, this.buttonGame.Checked);
		}
		private void buttonMessages_CheckedChanged(object sender, EventArgs e)
		{
			if (!this.Visible) return;
			this.logEntryList.SetFilterFlag(LogEntryList.MessageFilter.TypeMessage, this.buttonMessages.Checked);
		}
		private void buttonWarnings_CheckedChanged(object sender, EventArgs e)
		{
			if (!this.Visible) return;
			this.logEntryList.SetFilterFlag(LogEntryList.MessageFilter.TypeWarning, this.buttonWarnings.Checked);
		}
		private void buttonErrors_CheckedChanged(object sender, EventArgs e)
		{
			if (!this.Visible) return;
			this.logEntryList.SetFilterFlag(LogEntryList.MessageFilter.TypeError, this.buttonErrors.Checked);
		}
		private void actionClear_ButtonClick(object sender, EventArgs e)
		{
			this.logEntryList.DisplayMinTime = DateTime.Now;
		}

		private void Sandbox_Entering(object sender, EventArgs e)
		{
			if (this.checkAutoClear.Checked) this.actionClear_ButtonClick(sender, e);
		}
		private void LogData_NewEntry(object sender, DataLogOutput.LogEntryEventArgs e)
		{
			if (e.Entry.Type == LogMessageType.Warning)
			{
				this.unseenWarnings++;
			}
			else if (e.Entry.Type == LogMessageType.Error)
			{
				if (this.unseenErrors == 0) System.Media.SystemSounds.Hand.Play();
				this.unseenErrors++;
			}

			this.UpdateTabText();
		}
	}
}

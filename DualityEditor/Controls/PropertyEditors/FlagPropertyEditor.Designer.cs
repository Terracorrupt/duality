﻿namespace DualityEditor.Controls.PropertyEditors
{
	partial class FlagPropertyEditor
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

		#region Vom Komponenten-Designer generierter Code

		/// <summary> 
		/// Erforderliche Methode für die Designerunterstützung. 
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
			this.nameLabel = new System.Windows.Forms.Label();
			this.valueEditor = new DualityEditor.Controls.CustomFlagCheckedListBox();
			this.SuspendLayout();
			// 
			// nameLabel
			// 
			this.nameLabel.AutoEllipsis = true;
			this.nameLabel.Dock = System.Windows.Forms.DockStyle.Left;
			this.nameLabel.Location = new System.Drawing.Point(0, 0);
			this.nameLabel.Name = "nameLabel";
			this.nameLabel.Padding = new System.Windows.Forms.Padding(3);
			this.nameLabel.Size = new System.Drawing.Size(50, 82);
			this.nameLabel.TabIndex = 1;
			this.nameLabel.Text = "label1";
			this.nameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// valueEditor
			// 
			this.valueEditor.CheckOnClick = true;
			this.valueEditor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.valueEditor.FormattingEnabled = true;
			this.valueEditor.Location = new System.Drawing.Point(50, 0);
			this.valueEditor.Name = "valueEditor";
			this.valueEditor.Size = new System.Drawing.Size(153, 82);
			this.valueEditor.TabIndex = 2;
			this.valueEditor.FlagValueChanged += new System.EventHandler(this.valueEditor_FlagValueChanged);
			// 
			// FlagEnumPropertyEditor
			// 
			this.Controls.Add(this.valueEditor);
			this.Controls.Add(this.nameLabel);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "FlagEnumPropertyEditor";
			this.Size = new System.Drawing.Size(203, 82);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label nameLabel;
		private CustomFlagCheckedListBox valueEditor;
	}
}
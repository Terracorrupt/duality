﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;
using System.Reflection;

using Duality;

using DualityEditor;
using DualityEditor.Controls;
using PropertyGrid = DualityEditor.Controls.PropertyGrid;

namespace EditorBase.PropertyEditors
{
	public class DualityAppDataPropertyEditor : MemberwisePropertyEditor
	{
		public DualityAppDataPropertyEditor(PropertyEditor parentEditor, PropertyGrid parentGrid) : base(parentEditor, parentGrid, MemberFlags.Default)
		{
		}
		protected override void OnPropertySet(PropertyInfo property, IEnumerable<object> targets)
		{
			EditorBasePlugin.Instance.EditorForm.NotifyObjPropChanged(this, new ObjectSelection(targets), property);
		}
	}
	public class DualityUserDataPropertyEditor : MemberwisePropertyEditor
	{
		public DualityUserDataPropertyEditor(PropertyEditor parentEditor, PropertyGrid parentGrid) : base(parentEditor, parentGrid, MemberFlags.Default)
		{
		}
		protected override void OnPropertySet(PropertyInfo property, IEnumerable<object> targets)
		{
			EditorBasePlugin.Instance.EditorForm.NotifyObjPropChanged(this, new ObjectSelection(targets), property);
		}
	}
}
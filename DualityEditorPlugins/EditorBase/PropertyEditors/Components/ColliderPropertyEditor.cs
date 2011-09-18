﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Duality;
using Duality.Components;

using DualityEditor;
using DualityEditor.Controls;
using DualityEditor.Controls.PropertyEditors;

namespace EditorBase.PropertyEditors
{
	public class ColliderPropertyEditor : ComponentPropertyEditor
	{
		public ColliderPropertyEditor(PropertyEditor parentEditor, PropertyGrid parentGrid) : base(parentEditor, parentGrid)
		{

		}

		protected override PropertyEditor MemberEditor(MemberInfo info)
		{
			if (ReflectionHelper.MemberInfoEquals(info, ReflectionInfo.Property_Collider_Friction) ||
				ReflectionHelper.MemberInfoEquals(info, ReflectionInfo.Property_Collider_Restitution))
			{
				PropertyEditor e = this.ParentGrid.PropertyEditorProvider.CreateEditor((info as PropertyInfo).PropertyType, this, this.ParentGrid);
				NumericPropertyEditor numEdit = e as NumericPropertyEditor;
				if (numEdit != null)
				{
					numEdit.Editor.Minimum = 0.0m;
					numEdit.Editor.Maximum = 1.0m;
					numEdit.Editor.Increment = 0.05m;
				}
				return e;
			}
			else if (ReflectionHelper.MemberInfoEquals(info, ReflectionInfo.Property_Collider_LinearDamping) ||
				ReflectionHelper.MemberInfoEquals(info, ReflectionInfo.Property_Collider_AngularDamping))
			{
				PropertyEditor e = this.ParentGrid.PropertyEditorProvider.CreateEditor((info as PropertyInfo).PropertyType, this, this.ParentGrid);
				NumericPropertyEditor numEdit = e as NumericPropertyEditor;
				if (numEdit != null)
				{
					numEdit.Editor.Minimum = 0.0m;
					numEdit.Editor.Maximum = 10.0m;
					numEdit.Editor.Increment = 0.1m;
				}
				return e;
			}
			return base.MemberEditor(info);
		}
		protected override void OnPropertySet(PropertyInfo property, IEnumerable<object> targets)
		{
			base.OnPropertySet(property, targets);
			foreach (Collider c in targets.OfType<Collider>())
				c.AwakeBody();
		}
	}
}

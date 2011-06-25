﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Duality;
using DualityEditor;
using DualityEditor.Controls;

using DualityEditor.Controls.PropertyEditors;

namespace EditorBase.PropertyEditors
{
	public class CameraPropertyEditor : ComponentPropertyEditor
	{
		public CameraPropertyEditor(PropertyEditor parentEditor, PropertyGrid parentGrid) : base(parentEditor, parentGrid)
		{

		}

		protected override bool MemberPredicate(System.Reflection.MemberInfo info)
		{
			if (ReflectionHelper.MemberInfoEquals(info, ReflectionHelper.Property_Camera_OrthoAbs)) return false;
			if (ReflectionHelper.MemberInfoEquals(info, ReflectionHelper.Property_Camera_ViewportAbs)) return false;
			if (ReflectionHelper.MemberInfoEquals(info, ReflectionHelper.Property_Camera_TargetSize)) return false;
			if (ReflectionHelper.MemberInfoEquals(info, ReflectionHelper.Property_Camera_DrawDevice)) return false;
			return base.MemberPredicate(info);
		}
		protected override PropertyEditor MemberEditor(System.Reflection.MemberInfo info)
		{
			if (ReflectionHelper.MemberInfoEquals(info, ReflectionHelper.Property_Camera_VisibilityMask))
			{
				FlagPropertyEditor e = new FlagPropertyEditor(this, this.ParentGrid);
				e.EditedType = (info as System.Reflection.PropertyInfo).PropertyType;
				// ToDo: Use actual user-definable visibility groups
				e.AddFlag("None", 0);
				for (int i = 0; i < 32; ++i) e.AddFlag("Group " + i, 1UL << i);
				e.AddFlag("All", (1UL << 32) - 1);
				return e;
			}
			return base.MemberEditor(info);
		}
	}
}
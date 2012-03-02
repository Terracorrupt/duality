﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;

namespace CustomPropertyGrid
{
	public class MemberwisePropertyEditor : GroupedPropertyEditor
	{
		private	object[]						curObjects	= null;
		private	Predicate<MemberInfo>			memberPredicate		= null;
		private	Predicate<MemberInfo>			memberAffectsOthers	= null;
		private	Func<MemberInfo,PropertyEditor>	memberEditorCreator	= null;

		public override object DisplayedValue
		{
			get { return this.curObjects; }
		}
		public Predicate<MemberInfo> MemberPredicate
		{
			get { return this.memberPredicate; }
			set
			{
				if (value == null) value = this.DefaultMemberPredicate;
				if (this.memberPredicate != value)
				{
					this.memberPredicate = value;
					if (this.ContentInitialized) this.InitContent();
				}
			}
		}
		public Predicate<MemberInfo> MemberAffectsOthers
		{
			get { return this.memberAffectsOthers; }
			set
			{
				if (value == null) value = this.DefaultMemberAffectsOthers;
				if (this.memberAffectsOthers != value)
				{
					this.memberAffectsOthers = value;
					if (this.ContentInitialized) this.InitContent();
				}
			}
		}
		public Func<MemberInfo,PropertyEditor> MemberEditorCreator
		{
			get { return this.memberEditorCreator; }
			set
			{
				if (value == null) value = this.DefaultMemberEditorCreator;
				if (this.memberEditorCreator != value)
				{
					this.memberEditorCreator = value;
					if (this.ContentInitialized) this.InitContent();
				}
			}
		}


		public MemberwisePropertyEditor()
		{
			this.memberEditorCreator = this.DefaultMemberEditorCreator;
			this.memberAffectsOthers = this.DefaultMemberAffectsOthers;
			this.memberPredicate = this.DefaultMemberPredicate;
		}

		public override void InitContent()
		{
			this.ClearContent();

			base.InitContent();
			if (this.EditedType != null)
			{
				// Generate and add property editors for the current type
				this.BeginUpdate();
				// Properties
				{
					PropertyInfo[] propArr = this.EditedType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
					var propQuery = 
						from p in propArr
						where p.CanRead && p.GetIndexParameters().Length == 0 && this.memberPredicate(p)
						orderby GetTypeHierarchyLevel(p.DeclaringType) ascending, p.Name
						select p;
					foreach (PropertyInfo prop in propQuery)
					{
						this.AddEditorForProperty(prop);
					}
				}
				// Fields
				{
					FieldInfo[] fieldArr = this.EditedType.GetFields(BindingFlags.Instance | BindingFlags.Public);
					var fieldQuery =
						from f in fieldArr
						where this.memberPredicate(f)
						orderby GetTypeHierarchyLevel(f.DeclaringType) ascending, f.Name
						select f;
					foreach (FieldInfo field in fieldQuery)
					{
						this.AddEditorForField(field);
					}
				}
				this.EndUpdate();
				this.PerformGetValue();
			}
		}

		public PropertyEditor AddEditorForProperty(PropertyInfo prop)
		{
			PropertyEditor e = this.memberEditorCreator(prop);
			if (e == null) e = this.ParentGrid.CreateEditor(prop.PropertyType);
			if (e == null) return null;
			e.Getter = this.CreatePropertyValueGetter(prop);
			e.Setter = prop.CanWrite ? this.CreatePropertyValueSetter(prop) : null;
			e.PropertyName = prop.Name;
			e.EditedMember = prop;
			this.ParentGrid.ConfigureEditor(e);
			this.AddPropertyEditor(e);
			return e;
		}
		public PropertyEditor AddEditorForField(FieldInfo field)
		{
			PropertyEditor e = this.memberEditorCreator(field);
			if (e == null) e = this.ParentGrid.CreateEditor(field.FieldType);
			if (e == null) return null;
			e.Getter = this.CreateFieldValueGetter(field);
			e.Setter = this.CreateFieldValueSetter(field);
			e.PropertyName = field.Name;
			e.EditedMember = field;
			this.ParentGrid.ConfigureEditor(e);
			this.AddPropertyEditor(e);
			return e;
		}

		public override void PerformGetValue()
		{
			base.PerformGetValue();
			this.curObjects = this.GetValue().ToArray();

			if (this.curObjects == null)
			{
				return;
			}

			this.OnUpdateFromObjects(this.curObjects);

			foreach (PropertyEditor e in this.Children)
				e.PerformGetValue();
		}
		public override void PerformSetValue()
		{
			base.PerformSetValue();
			if (!this.Children.Any()) return;

			foreach (PropertyEditor e in this.Children)
				e.PerformSetValue();
		}
		protected virtual void OnUpdateFromObjects(object[] values)
		{
			string valString = null;

			if (!values.Any() || values.All(o => o == null))
			{
				this.ClearContent();

				this.Expanded = false;
					
				valString = "null";
			}
			else
			{
				valString = values.Count() == 1 ? 
					values.First().ToString() :
					string.Format(CustomPropertyGrid.Properties.Resources.PropertyGrid_N_Objects, values.Count());
			}
		}

		protected Func<IEnumerable<object>> CreatePropertyValueGetter(PropertyInfo property)
		{
			return () => this.curObjects.Select(o => o != null ? property.GetValue(o, null) : null);
		}
		protected Func<IEnumerable<object>> CreateFieldValueGetter(FieldInfo field)
		{
			return () => this.curObjects.Select(o => o != null ? field.GetValue(o) : null);
		}
		protected Action<IEnumerable<object>> CreatePropertyValueSetter(PropertyInfo property)
		{
			bool affectsOthers = this.memberAffectsOthers(property);
			return delegate(IEnumerable<object> values)
			{
				IEnumerator<object> valuesEnum = values.GetEnumerator();
				object[] targetArray = this.curObjects;

				object curValue = null;
				if (valuesEnum.MoveNext()) curValue = valuesEnum.Current;
				foreach (object target in targetArray)
				{
					if (target != null) property.SetValue(target, curValue, null);
					if (valuesEnum.MoveNext()) curValue = valuesEnum.Current;
				}
				this.OnPropertySet(property, targetArray);
				if (affectsOthers) this.PerformGetValue();

				// Fixup struct values by assigning the modified struct copy to its original member
				if (this.EditedType.IsValueType || this.ForceWriteBack) this.SetValue((IEnumerable<object>)targetArray);
			};
		}
		protected Action<IEnumerable<object>> CreateFieldValueSetter(FieldInfo field)
		{
			bool affectsOthers = this.memberAffectsOthers(field);
			return delegate(IEnumerable<object> values)
			{
				IEnumerator<object> valuesEnum = values.GetEnumerator();
				object[] targetArray = this.curObjects;

				object curValue = null;
				if (valuesEnum.MoveNext()) curValue = valuesEnum.Current;
				foreach (object target in targetArray)
				{
					if (target != null) field.SetValue(target, curValue);
					if (valuesEnum.MoveNext()) curValue = valuesEnum.Current;
				}
				this.OnFieldSet(field, targetArray);
				if (affectsOthers) this.PerformGetValue();

				// Fixup struct values by assigning the modified struct copy to its original member
				if (this.EditedType.IsValueType || this.ForceWriteBack) this.SetValue((IEnumerable<object>)targetArray);
			};
		}

		protected virtual void OnPropertySet(PropertyInfo property, IEnumerable<object> targets)
		{

		}
		protected virtual void OnFieldSet(FieldInfo property, IEnumerable<object> targets)
		{

		}

		private bool DefaultMemberPredicate(MemberInfo info)
		{
			return true;
		}
		private bool DefaultMemberAffectsOthers(MemberInfo info)
		{
			return false;
		}
		private	PropertyEditor DefaultMemberEditorCreator(MemberInfo info)
		{
			return null;
		}

		private static int GetTypeHierarchyLevel(Type t)
		{
			int level = 0;
			while (t.BaseType != null) { t = t.BaseType; level++; }
			return level;
		}
	}
}
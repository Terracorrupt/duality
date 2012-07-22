﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Duality.Cloning.Surrogates;

namespace Duality.Cloning
{
	public class CloneProvider
	{
		private Dictionary<object, object>	objToClone		= new Dictionary<object,object>();
		private	List<ISurrogate>			surrogates		= new List<ISurrogate>();
		private	Type[]						explicitUnwrap	= null;
		

		/// <summary>
		/// [GET] Enumerates registered <see cref="Duality.Cloning.ISurrogate">Cloning Surrogates</see>. If any of them
		/// matches the <see cref="System.Type"/> of an object that is to be cloned, instead of letting it
		/// clone itsself, the <see cref="Duality.Cloning.ISurrogate"/> with the highest <see cref="Duality.Cloning.ISurrogate.Priority"/>
		/// is used instead.
		/// </summary>
		public IEnumerable<ISurrogate> Surrogates
		{
			get { return this.surrogates; }
		}
		

		public CloneProvider()
		{
			this.AddSurrogate(new DelegateSurrogate());
			this.AddSurrogate(new DictionarySurrogate());
			this.AddSurrogate(new BitmapSurrogate());
		}

		/// <summary>
		/// Unregisters all <see cref="Duality.Cloning.ISurrogate">Surrogates</see>.
		/// </summary>
		public void ClearSurrogates()
		{
			this.surrogates.Clear();
		}
		/// <summary>
		/// Registers a new <see cref="Duality.Cloning.ISurrogate">Surrogate</see>.
		/// </summary>
		/// <param name="surrogate"></param>
		public void AddSurrogate(ISurrogate surrogate)
		{
			if (this.surrogates.Contains(surrogate)) return;
			this.surrogates.Add(surrogate);
			this.surrogates.StableSort((s1, s2) => s1.Priority - s2.Priority);
		}
		/// <summary>
		/// Unregisters an existing <see cref="Duality.Cloning.ISurrogate">Surrogate</see>.
		/// </summary>
		/// <param name="surrogate"></param>
		public void RemoveSurrogate(ISurrogate surrogate)
		{
			this.surrogates.Remove(surrogate);
		}
		/// <summary>
		/// Retrieves a matching <see cref="Duality.Cloning.ISurrogate"/> for the specified <see cref="System.Type"/>.
		/// </summary>
		/// <param name="t">The <see cref="System.Type"/> to retrieve a <see cref="Duality.Cloning.ISurrogate"/> for.</param>
		/// <returns></returns>
		public ISurrogate GetSurrogateFor(Type t)
		{
			return this.surrogates.FirstOrDefault(s => s.MatchesType(t));
		}

		public void SetExplicitUnwrap(params Type[] unwrapTypes)
		{
			if (unwrapTypes != null && unwrapTypes.Any(t => t == null)) throw new ArgumentException("Cannot unwrap null Type.", "unwrapTypes");
			this.explicitUnwrap = unwrapTypes;
		}

		/// <summary>
		/// Clears all existing object mappings.
		/// </summary>
		public void ClearObjectMap()
		{
			this.objToClone.Clear();
		}
		/// <summary>
		/// Requests the clone(d) object mapped to the specified base object.
		/// </summary>
		/// <param name="baseObj"></param>
		/// <returns></returns>
		public T RequestObjectClone<T>(T baseObj)
		{
			if (baseObj == null) return default(T);

			object clone;
			if (this.objToClone.TryGetValue(baseObj, out clone)) return (T)clone;

			return (T)this.CloneObject(baseObj);
		}
		/// <summary>
		/// Returns an already registered clone object, if existing.
		/// </summary>
		/// <param name="baseObj"></param>
		/// <returns></returns>
		public T GetRegisteredObjectClone<T>(T baseObj)
		{
			if (baseObj == null) return default(T);

			object clone;
			if (this.objToClone.TryGetValue(baseObj, out clone)) return (T)clone;

			return default(T);
		}
		/// <summary>
		/// Returns whether the specified object is a registered original / base object.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public bool IsOriginalObject(object obj)
		{
			return obj != null ? this.objToClone.ContainsKey(obj) : false;
		}
		/// <summary>
		/// Returns whether the specified object is a registered clone object.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public bool IsCloneObject(object obj)
		{
			return obj != null ? this.objToClone.ContainsValue(obj) : false;
		}
		/// <summary>
		/// Copies the base objects data to the specified target object.
		/// </summary>
		/// <param name="baseObj"></param>
		/// <param name="targetObj"></param>
		/// <param name="fields"></param>
		public void CopyObjectTo<T>(T baseObj, T targetObj, FieldInfo[] fields = null)
		{
			if (fields == null)
			{
				Type objType = baseObj.GetType();
				if (!this.DoesUnwrapType(objType)) return;

				// IClonables
				if (baseObj is ICloneable)
				{
					(baseObj as ICloneable).CopyDataTo(targetObj, this);
					return;
				}

				// ISurrogate
				ISurrogate surrogate = this.GetSurrogateFor(objType);
				if (surrogate != null)
				{
					surrogate.RealObject = baseObj;
					surrogate.SurrogateObject.CopyDataTo(targetObj, this);
					return;
				}

				fields = objType.GetAllFields(ReflectionHelper.BindInstanceAll);
			}
			foreach (FieldInfo f in fields)
				f.SetValue(targetObj, this.RequestObjectClone(f.GetValue(baseObj)));
		}
		/// <summary>
		/// Registers a new base-clone mapping.
		/// </summary>
		/// <param name="baseObj"></param>
		/// <param name="clone"></param>
		public void RegisterObjectClone<T>(T baseObj, T clone)
		{
			if (baseObj == null) throw new ArgumentNullException("baseObj");
			if (clone == null) throw new ArgumentNullException("clone");
			this.objToClone[baseObj] = clone;
		}

		private bool DoesUnwrapType(Type type)
		{
			bool unwrap = !type.IsShallowType();
			if (this.explicitUnwrap != null)
			{
				unwrap = unwrap && type.IsValueType;
				if (!unwrap) unwrap = this.explicitUnwrap.Any(t => t.IsAssignableFrom(type));
			}
			return unwrap;
		}
		private object CloneObject(object baseObj)
		{
			Type objType = baseObj.GetType();
			if (!this.DoesUnwrapType(objType)) return baseObj;

			// IClonables
			if (baseObj is ICloneable)
			{
				object copy = (baseObj as ICloneable).CreateTargetObject(this);
				if (objType.IsClass) this.RegisterObjectClone(baseObj, copy);
				(baseObj as ICloneable).CopyDataTo(copy, this);
				return copy;
			}

			// ISurrogate
			ISurrogate surrogate = this.GetSurrogateFor(objType);
			if (surrogate != null)
			{
				surrogate.RealObject = baseObj;
				object copy = surrogate.SurrogateObject.CreateTargetObject(this);
				if (objType.IsClass) this.RegisterObjectClone(baseObj, copy);
				surrogate.SurrogateObject.CopyDataTo(copy, this);
				return copy;
			}

			// Shallow types, cloned by assignment
			if (objType.IsShallowType())
			{
				return baseObj;
			}
			// Arrays
			else if (objType.IsArray)
			{
				Array baseArray = (Array)baseObj;
				Type elemType = objType.GetElementType();
				int length = baseArray.Length;
				Array copy = Array.CreateInstance(elemType, length);
				this.RegisterObjectClone(baseObj, copy);

				bool unwrap = this.DoesUnwrapType(elemType);
				if (unwrap)
				{
					for (int i = 0; i < length; ++i)
						copy.SetValue(this.RequestObjectClone(baseArray.GetValue(i)), i);
				}
				else
				{
					baseArray.CopyTo(copy, 0);
				}

				return copy;
			}
			// Reference types / complex objects
			else
			{
				object copy = objType.CreateInstanceOf() ?? objType.CreateInstanceOf(true);
				if (objType.IsClass) this.RegisterObjectClone(baseObj, copy);

				this.CopyObjectTo(baseObj, copy, objType.GetAllFields(ReflectionHelper.BindInstanceAll));

				return copy;
			}
		}

		public static T DeepClone<T>(T baseObj)
		{
			CloneProvider provider = new CloneProvider();
			return (T)provider.RequestObjectClone(baseObj);
		}
		public static void DeepCopyTo<T>(T baseObj, T targetObj)
		{
			CloneProvider provider = new CloneProvider();
			provider.CopyObjectTo(baseObj, targetObj);
		}
	}
}

﻿using System;

using OpenTK;

using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Dynamics.Joints;

using Duality.EditorHints;
using Duality.Resources;

namespace Duality.Components
{
	public partial class Collider
	{
		/// <summary>
		/// Describes a <see cref="Collider"/> joint. Joints limit a Colliders degree of freedom 
		/// by connecting it to fixed world coordinates or other Colliders.
		/// </summary>
		[Serializable]
		public abstract class JointInfo : Duality.Cloning.ICloneable
		{
			[NonSerialized]	
			protected	Joint		joint		= null;
			private		Collider	colA		= null;
			private		Collider	colB		= null;
			private		bool		collide		= false;
			private		bool		enabled		= true;
			private		float		breakPoint	= -1.0f;
			
			[EditorHintFlags(MemberFlags.Invisible)]
			public bool IsInitialized
			{
				get { return this.joint != null; }
			}
			[EditorHintFlags(MemberFlags.Invisible)]
			public Collider ColliderA
			{
				get { return this.colA; }
				internal set { this.colA = value; }
			}
			[EditorHintFlags(MemberFlags.Invisible)]
			public Collider ColliderB
			{
				get { return this.colB; }
				internal set { this.colB = value; }
			}
			public bool CollideConnected
			{
				get { return this.collide; }
				set { this.collide = value; this.UpdateJoint(); }
			}
			public bool Enabled
			{
				get { return this.enabled; }
				set { this.enabled = value; this.UpdateJoint(); }
			}
			[EditorHintRange(-1.0f, float.MaxValue)]
			public float BreakPoint
			{
				get { return this.breakPoint; }
				set { this.breakPoint = value; this.UpdateJoint(); }
			}


			internal void DestroyJoint()
			{
				if (this.joint == null) return;
				Scene.PhysicsWorld.RemoveJoint(this.joint);
				this.joint = null;
			}
			protected abstract Joint CreateJoint(Body bodyA, Body bodyB);
			internal virtual void UpdateJoint()
			{
				if (this.joint == null)
				{
					if (this.colA != null && this.colA.body != null && (this.colB == null || this.colB.body != null))
					{
						this.joint = this.CreateJoint(this.colA.body, this.colB != null ? this.colB.body : null);
						this.joint.UserData = this;
					}
					else return;
				}

				this.joint.CollideConnected = this.collide;
				this.joint.Enabled = this.enabled;
				this.joint.Breakpoint = this.breakPoint <= 0.0f ? float.MaxValue : this.breakPoint;
			}

			/// <summary>
			/// Copies this JointInfos data to another one. It is assumed that both are of the same type.
			/// </summary>
			/// <param name="target"></param>
			protected virtual void CopyTo(JointInfo target)
			{
				// Don't copy the parents!
				target.collide = this.collide;
				target.enabled = this.enabled;
				target.breakPoint = this.breakPoint;
			}
			/// <summary>
			/// Clones the JointInfo.
			/// </summary>
			/// <returns></returns>
			public JointInfo Clone()
			{
				JointInfo newObj = this.GetType().CreateInstanceOf() as JointInfo;
				this.CopyTo(newObj);
				return newObj;
			}

			object Cloning.ICloneable.CreateTargetObject(Cloning.CloneProvider provider)
			{
				return this.GetType().CreateInstanceOf() ?? this.GetType().CreateInstanceOf(true);
			}
			void Cloning.ICloneable.CopyDataTo(object targetObj, Cloning.CloneProvider provider)
			{
				JointInfo targetJoint = targetObj as JointInfo;
				this.CopyTo(targetJoint);
			}

			protected static Vector2 GetFarseerPoint(Collider c, Vector2 dualityPoint)
			{
				if (c == null) return PhysicsConvert.ToPhysicalUnit(dualityPoint);

				Vector2 scale = (c.GameObj != null && c.GameObj.Transform != null) ? c.GameObj.Transform.Scale.Xy : Vector2.One;
				return PhysicsConvert.ToPhysicalUnit(dualityPoint * scale);
			}
			protected static Vector2 GetDualityPoint(Collider c, Vector2 farseerPoint)
			{
				if (c == null) return PhysicsConvert.ToDualityUnit(farseerPoint);

				Vector2 scale = (c.GameObj != null && c.GameObj.Transform != null) ? c.GameObj.Transform.Scale.Xy : Vector2.One;
				return PhysicsConvert.ToDualityUnit(farseerPoint / scale);
			}
		}

		[Serializable]
		public sealed class FixedAngleJointInfo : JointInfo
		{
			private	float	angle	= 0.0f;

			public float TargetAngle
			{
				get { return this.angle; }
				set { this.angle = value; this.UpdateJoint(); }
			}

			public FixedAngleJointInfo() : this(0.0f) {}
			public FixedAngleJointInfo(float angle)
			{
				this.angle = angle;
			}

			protected override Joint CreateJoint(Body bodyA, Body bodyB)
			{
				return JointFactory.CreateFixedAngleJoint(Scene.PhysicsWorld, bodyA);
			}
			internal override void UpdateJoint()
			{
				base.UpdateJoint();
				if (this.joint == null) return;

				FixedAngleJoint j = this.joint as FixedAngleJoint;
				j.TargetAngle = this.angle;
			}

			protected override void CopyTo(JointInfo target)
			{
				base.CopyTo(target);
				FixedAngleJointInfo c = target as FixedAngleJointInfo;
				c.angle = this.angle;
			}
		}

		[Serializable]
		public sealed class WeldJointInfo : JointInfo
		{
			private Vector2 localPointA	= Vector2.Zero;
			private	Vector2	localPointB	= Vector2.Zero;
			private	float	refAngle	= 0.0f;

			public Vector2 LocalPointA
			{
				get { return this.localPointA; }
				set { this.localPointA = value; this.UpdateJoint(); }
			}
			public Vector2 LocalPointB
			{
				get { return this.localPointB; }
				set { this.localPointB = value; this.UpdateJoint(); }
			}
			public float RefAngle
			{
				get { return this.refAngle; }
				set { this.refAngle = value; this.UpdateJoint(); }
			}

			protected override Joint CreateJoint(Body bodyA, Body bodyB)
			{
				return JointFactory.CreateWeldJoint(Scene.PhysicsWorld, bodyA, bodyB, Vector2.Zero);
			}
			internal override void UpdateJoint()
			{
				base.UpdateJoint();
				if (this.joint == null) return;

				WeldJoint j = this.joint as WeldJoint;
				j.LocalAnchorA = GetFarseerPoint(this.ColliderA, this.localPointA);
				j.LocalAnchorB = GetFarseerPoint(this.ColliderB, this.localPointB);
				j.ReferenceAngle = this.refAngle;
			}

			protected override void CopyTo(JointInfo target)
			{
				base.CopyTo(target);
				WeldJointInfo c = target as WeldJointInfo;
				c.localPointA = this.localPointA;
				c.localPointB = this.localPointB;
				c.refAngle = this.refAngle;
			}
		}
	}
}

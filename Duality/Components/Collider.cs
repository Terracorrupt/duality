﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision.Shapes;

using Duality;
using Duality.Resources;

namespace Duality.Components
{
	/// <summary>
	/// Represents a body instance for physical simulation, collision detection and response.
	/// </summary>
	[Serializable]
	[RequiredComponent(typeof(Transform))]
	public class Collider : Component, ICmpInitializable, ICmpUpdatable, ITransformUpdater
	{
		/// <summary>
		/// The type of a <see cref="Collider">Colliders</see> physical body.
		/// </summary>
		public enum BodyType
		{
			/// <summary>
			/// A static body. It will never move due to physical forces.
			/// </summary>
			Static,
			/// <summary>
			/// A dynamic body. Its movement is determined by physical effects.
			/// </summary>
			Dynamic
		}

		/// <summary>
		/// Describes a <see cref="Collider">Colliders</see> primitive shape. A Colliders overall shape may be combined of any number of primitive shapes.
		/// </summary>
		[Serializable]
		public abstract class ShapeInfo
		{
			[NonSerialized]	
			protected	Fixture		fixture		= null;
			private		Collider	parent		= null;
			private		float		density		= 1.0f;
			private		float		friction	= 0.3f;
			private		float		restitution	= 0.3f;
			private		bool		sensor		= false;
			
			/// <summary>
			/// [GET] The shape's parent <see cref="Collider"/>.
			/// </summary>
			public Collider Parent
			{
				get { return this.parent; }
				set 
				{ 
					if (this.parent != value)
					{
						if (this.parent != null) this.parent.RemoveShape(this);
						this.parent = value;
						if (this.parent != null) this.parent.AddShape(this);
					}
				}
			}
			/// <summary>
			/// [GET / SET] The shapes density.
			/// </summary>
			public float Density
			{
				get { return this.density; }
				set 
				{
					this.density = value;
					if (this.parent != null) // Full update to recalculate mass
						this.parent.UpdateBodyShape();
					else
						this.UpdateFixture();
				}
			}
			/// <summary>
			/// [GET / SET] Whether or not the shape acts as sensor i.e. is not part of a rigid body.
			/// </summary>
			public bool IsSensor
			{
				get { return this.sensor; }
				set { this.sensor = value; this.UpdateFixture(); }
			}
			/// <summary>
			/// [GET / SET] The shapes friction value.
			/// </summary>
			public float Friction
			{
				get { return this.friction; }
				set { this.friction = value; this.UpdateFixture(); }
			}
			/// <summary>
			/// [GET / SET] The shapes restitution value.
			/// </summary>
			public float Restitution
			{
				get { return this.restitution; }
				set { this.restitution = value; this.UpdateFixture(); }
			}
			/// <summary>
			/// [GET] Returns the Shapes axis-aligned bounding box
			/// </summary>
			public abstract Rect AABB { get; }

			protected ShapeInfo()
			{
			}
			protected ShapeInfo(float density)
			{
				this.density = density;
			}

			internal void DestroyFixture(Body body)
			{
				if (this.fixture == null) return;
				body.DestroyFixture(this.fixture);
			}
			internal abstract void CreateFixture(Body body);
			internal virtual void UpdateFixture()
			{
				this.fixture.Shape.Density = this.density;
				this.fixture.IsSensor = this.sensor;
				this.fixture.Restitution = this.restitution;
				this.fixture.Friction = this.friction;
			}

			/// <summary>
			/// Copies this ShapeInfos data to another one. It is assumed that both are of the same type.
			/// </summary>
			/// <param name="target"></param>
			protected virtual void CopyTo(ShapeInfo target)
			{
				// Don't copy the parent!
				target.density = this.density;
				target.sensor = this.sensor;
				target.friction = this.friction;
				target.restitution = this.restitution;
			}
			/// <summary>
			/// Clones the ShapeInfo.
			/// </summary>
			/// <returns></returns>
			public ShapeInfo Clone()
			{
				ShapeInfo newObj = ReflectionHelper.CreateInstanceOf(this.GetType()) as ShapeInfo;
				this.CopyTo(newObj);
				return newObj;
			}
		}
		/// <summary>
		/// Describes a <see cref="Collider">Colliders</see> circle shape.
		/// </summary>
		[Serializable]
		public sealed class CircleShapeInfo : ShapeInfo
		{
			private	float	radius;
			private	Vector2	position;

			/// <summary>
			/// [GET / SET] The circles radius.
			/// </summary>
			public float Radius
			{
				get { return this.radius; }
				set { this.radius = value; this.UpdateFixture(); }
			}
			/// <summary>
			/// [GET / SET] The circles position.
			/// </summary>
			public Vector2 Position
			{
				get { return this.position; }
				set { this.position = value; this.UpdateFixture(); }
			}
			public override Rect AABB
			{
				get { return Rect.AlignCenter(position.X, position.Y, radius * 2, radius * 2); }
			}

			public CircleShapeInfo() {}
			public CircleShapeInfo(float radius, Vector2 position, float density) : base(density)
			{
				this.radius = radius;
				this.position = position;
			}

			internal override void CreateFixture(Body body)
			{
				this.fixture = body.CreateFixture(new CircleShape(1.0f, 1.0f), this);
			}
			internal override void UpdateFixture()
			{
				base.UpdateFixture();

				if (this.Parent == null) return;
				Vector2 scale = Vector2.One;
				if (this.Parent != null && this.Parent.GameObj != null && this.Parent.GameObj.Transform != null)
					scale = this.Parent.GameObj.Transform.Scale.Xy;
				float uniformScale = scale.Length / MathF.Sqrt(2.0f);

				CircleShape circle = this.fixture.Shape as CircleShape;
				circle.Radius = this.radius * uniformScale * 0.01f;
				circle.Position = new Vector2(this.position.X * scale.X, this.position.Y * scale.Y) * 0.01f;
			}

			protected override void CopyTo(ShapeInfo target)
			{
				base.CopyTo(target);
				CircleShapeInfo c = target as CircleShapeInfo;
				c.radius = this.radius;
				c.position = this.position;
			}
		}
		/// <summary>
		/// Describes a <see cref="Collider">Colliders</see> polygon shape.
		/// </summary>
		[Serializable]
		public sealed class PolyShapeInfo : ShapeInfo
		{
			private	Vector2[]	vertices;

			/// <summary>
			/// [GET / SET] The polygons vertices.
			/// </summary>
			public Vector2[] Vertices
			{
				get { return this.vertices; }
				set { this.vertices = value; this.UpdateFixture(); }
			}
			public override Rect AABB
			{
				get 
				{
					float minX = float.MaxValue;
					float minY = float.MaxValue;
					float maxX = float.MinValue;
					float maxY = float.MinValue;
					for (int i = 0; i < this.vertices.Length; i++)
					{
						minX = MathF.Min(minX, this.vertices[i].X);
						minY = MathF.Min(minY, this.vertices[i].Y);
						maxX = MathF.Max(maxX, this.vertices[i].X);
						maxY = MathF.Max(maxY, this.vertices[i].Y);
					}
					return new Rect(minX, minY, maxX - minX, maxY - minY);
				}
			}
			
			public PolyShapeInfo() {}
			public PolyShapeInfo(IEnumerable<Vector2> vertices, float density) : base(density)
			{
				this.vertices = vertices.ToArray();
			}

			internal override void CreateFixture(Body body)
			{
				FarseerPhysics.Common.Vertices dummy = this.CreateVertices(Vector2.One);
				this.fixture = body.CreateFixture(new PolygonShape(dummy, 1.0f), this);
			}
			internal override void UpdateFixture()
			{
				base.UpdateFixture();
				
				Vector2 scale = Vector2.One;
				if (this.Parent != null && this.Parent.GameObj != null && this.Parent.GameObj.Transform != null)
					scale = this.Parent.GameObj.Transform.Scale.Xy;

				PolygonShape poly = this.fixture.Shape as PolygonShape;
				poly.Set(this.CreateVertices(scale));
			}
			private FarseerPhysics.Common.Vertices CreateVertices(Vector2 scale)
			{
				// Sort vertices clockwise before submitting them to Farseer
				Vector2[] sortedVertices = this.vertices.ToArray();
				Vector2 centroid = Vector2.Zero;
				for (int i = 0; i < sortedVertices.Length; i++)
					centroid += sortedVertices[i];
				centroid /= sortedVertices.Length;
				sortedVertices.StableSort(delegate(Vector2 first, Vector2 second)
				{
					return MathF.RoundToInt(
						1000000.0f * MathF.Angle(centroid.X, centroid.Y, first.X, first.Y) - 
						1000000.0f * MathF.Angle(centroid.X, centroid.Y, second.X, second.Y));
				});

				// Submit vertices
				FarseerPhysics.Common.Vertices v = new FarseerPhysics.Common.Vertices(sortedVertices.Length);
				for (int i = 0; i < sortedVertices.Length; i++)
				{
					v.Add(new Vector2(
						sortedVertices[i].X * scale.X * 0.01f, 
						sortedVertices[i].Y * scale.Y * 0.01f));
				}
				return v;
			}

			protected override void CopyTo(ShapeInfo target)
			{
				base.CopyTo(target);
				PolyShapeInfo c = target as PolyShapeInfo;
				c.vertices = this.vertices != null ? (Vector2[])this.vertices.Clone() : null;
			}
		}

		private struct ColEvent
		{
			public enum EventType
			{
				Collision,
				Separation,
				PostSolve
			}

			public	EventType		Type;
			public	Fixture			FixtureA;
			public	Fixture			FixtureB;
			public	CollisionData	Data;

			public ColEvent(EventType type, Fixture fxA, Fixture fxB, CollisionData data)
			{
				this.Type = type;
				this.FixtureA = fxA;
				this.FixtureB = fxB;
				this.Data = data;
			}
		}

		private	BodyType	bodyType		= BodyType.Dynamic;
		private	float		linearDamp		= 0.0f;
		private	float		angularDamp		= 0.0f;
		private	bool		fixedAngle		= false;
		private	bool		ignoreGravity	= false;
		private	Category	colCat			= Category.Cat1;
		private	Category	colWith			= Category.All;
		private	List<ShapeInfo>	shapes		= new List<ShapeInfo>();
		[NonSerialized]	private	Body			body		= null;
		[NonSerialized]	private	List<ColEvent>	eventBuffer	= new List<ColEvent>();

		/// <summary>
		/// [GET / SET] The type of the physical body.
		/// </summary>
		public BodyType PhysicsBodyType
		{
			get { return this.bodyType; }
			set 
			{
				if (this.body != null) this.body.BodyType = (value == BodyType.Static ? FarseerPhysics.Dynamics.BodyType.Static : FarseerPhysics.Dynamics.BodyType.Dynamic);
				this.bodyType = value;
			}
		}
		/// <summary>
		/// [GET / SET] The damping that is applied to the bodies velocity.
		/// </summary>
		public float LinearDamping
		{
			get { return this.linearDamp; }
			set 
			{
				if (this.body != null) this.body.LinearDamping = value;
				this.linearDamp = value;
			}
		}
		/// <summary>
		/// [GET / SET] The damping that is applied to the bodies angular velocity.
		/// </summary>
		public float AngularDamping
		{
			get { return this.angularDamp; }
			set 
			{
				if (this.body != null) this.body.AngularDamping = value;
				this.angularDamp = value;
			}
		}
		/// <summary>
		/// [GET / SET] Whether the bodies rotation is fixed.
		/// </summary>
		public bool FixedAngle
		{
			get { return this.fixedAngle; }
			set 
			{
				if (this.body != null) this.body.FixedRotation = value;
				this.fixedAngle = value;
			}
		}
		/// <summary>
		/// [GET / SET] Whether the body ignores gravity.
		/// </summary>
		public bool IgnoreGravity
		{
			get { return this.ignoreGravity; }
			set 
			{
				if (this.body != null) this.body.IgnoreGravity = value;
				this.ignoreGravity = value;
			}
		}
		/// <summary>
		/// [GET / SET] A bitmask that specifies the collision categories to which this Collider belongs.
		/// </summary>
		public Category CollisionCategory
		{
			get { return this.colCat; }
			set
			{
				this.colCat = value;
				if (this.body != null) this.body.CollisionCategories = value;
			}
		}
		/// <summary>
		/// [GET / SET] A bitmask that specifies which collision categories this Collider interacts with.
		/// </summary>
		public Category CollidesWith
		{
			get { return this.colWith; }
			set
			{
				this.colWith = value;
				if (this.body != null) this.body.CollidesWith = value;
			}
		}
		/// <summary>
		/// [GET / SET] Enumerates all <see cref="ShapeInfo">primitive shapes</see> which this body consists of.
		/// If you modify any of the returned ShapeInfos, be sure to call <see cref="UpdateBodyShape"/> afterwards.
		/// </summary>
		public IEnumerable<ShapeInfo> Shapes
		{
			get { return this.shapes; }
			set
			{
				this.SetShapes(value);
			}
		}
		/// <summary>
		/// [GET] The physical bodys bounding radius.
		/// </summary>
		public float BoundRadius
		{
			get
			{
				if (this.shapes == null || this.shapes.Count == 0) return 0.0f;

				Rect boundRect = this.shapes[0].AABB;
				foreach (ShapeInfo info in this.shapes.Skip(1))
					boundRect = boundRect.ExpandToContain(info.AABB);

				Vector2 scale = this.GameObj.Transform.Scale.Xy;
				return boundRect.Transform(scale).BoundingRadius;
			}
		}

		public Collider()
		{
			// Default shape
			this.AddShape(new CircleShapeInfo(64.0f, Vector2.Zero, 1.0f));
		}

		/// <summary>
		/// Adds a new shape to the Collider
		/// </summary>
		/// <param name="shape"></param>
		public void AddShape(ShapeInfo shape)
		{
			if (shape == null) throw new ArgumentNullException("shapes");
			if (this.shapes != null && this.shapes.Contains(shape)) return;

			if (this.shapes == null) this.shapes = new List<ShapeInfo>();
			this.shapes.Add(shape);
			shape.Parent = this;

			if (this.body != null)
			{
				bool wasEnabled = this.body.Enabled;
				if (wasEnabled) this.body.Enabled = false;

				shape.CreateFixture(this.body);
				this.UpdateBodyShape();

				if (wasEnabled) this.body.Enabled = true;
			}
		}
		/// <summary>
		/// Removes an existing shape from the Collider.
		/// </summary>
		/// <param name="shape"></param>
		public void RemoveShape(ShapeInfo shape)
		{
			if (shape == null) throw new ArgumentNullException("shapes");
			if (this.shapes == null || !this.shapes.Contains(shape)) return;

			this.shapes.Remove(shape);
			shape.Parent = null;

			if (this.body != null)
			{
				shape.DestroyFixture(this.body);
				this.UpdateBodyShape();
			}
		}
		/// <summary>
		/// Removes all existing shapes from the Collider.
		/// </summary>
		public void ClearShapes()
		{
			if (this.shapes == null) return;

			var oldShapes = this.shapes.ToArray();
			this.shapes.Clear();
			foreach (ShapeInfo shape in oldShapes)
			{
				if (this.body != null) shape.DestroyFixture(this.body);
				shape.Parent = null;
			}
			this.UpdateBodyShape();
		}
		/// <summary>
		/// Sets the Colliders shape.
		/// </summary>
		/// <param name="shapes"></param>
		public void SetShapes(IEnumerable<ShapeInfo> shapes)
		{
			if (shapes == null) throw new ArgumentNullException("shapes");

			// Clone shape collection
			ShapeInfo[] cloned = shapes.ToArray();
			for (int i = 0; i < cloned.Length; i++)
				cloned[i] = cloned[i].Clone();
			shapes = cloned;

			if (this.shapes != null)
			{
				var oldShapes = this.shapes.ToArray();
				this.shapes.Clear();
				foreach (ShapeInfo shape in oldShapes)
				{
					if (this.body != null) shape.DestroyFixture(this.body);
					shape.Parent = null;
				}
			}

			bool wasEnabled = this.body != null && this.body.Enabled;
			if (wasEnabled) this.body.Enabled = false;

			if (this.shapes == null) this.shapes = new List<ShapeInfo>();
			foreach (ShapeInfo shape in shapes)
			{
				if (shape == null) continue;

				this.shapes.Add(shape);
				shape.Parent = this;

				if (this.body != null) shape.CreateFixture(this.body);
			}

			this.UpdateBodyShape();
			if (wasEnabled) this.body.Enabled = true;
		}

		/// <summary>
		/// Applies a Transform-local angular impulse to the object.
		/// </summary>
		/// <param name="angularImpulse"></param>
		public void ApplyLocalImpulse(float angularImpulse)
		{
			if (this.body == null) return;
			this.body.ApplyAngularImpulse(angularImpulse * 0.01f / Time.SPFMult);
		}
		/// <summary>
		/// Applies a Transform-local impulse to the objects mass center.
		/// </summary>
		/// <param name="impulse"></param>
		public void ApplyLocalImpulse(Vector2 impulse)
		{
			this.ApplyWorldImpulse(this.gameobj.Transform.GetWorldVector(new Vector3(impulse)).Xy);
		}
		/// <summary>
		/// Applies a Transform-local impulse to the specified point.
		/// </summary>
		/// <param name="impulse"></param>
		/// <param name="applyAt"></param>
		public void ApplyLocalImpulse(Vector2 impulse, Vector2 applyAt)
		{
			this.ApplyWorldImpulse(
				this.gameobj.Transform.GetWorldVector(new Vector3(impulse)).Xy,
				this.gameobj.Transform.GetWorldPoint(new Vector3(applyAt)).Xy);
		}
		/// <summary>
		/// Applies a world impulse to the objects mass center.
		/// </summary>
		/// <param name="impulse"></param>
		public void ApplyWorldImpulse(Vector2 impulse)
		{
			if (this.body == null) return;
			this.body.ApplyLinearImpulse(impulse * 0.01f / Time.SPFMult);
		}
		/// <summary>
		/// Applies a world impulse to the specified point.
		/// </summary>
		/// <param name="impulse"></param>
		/// <param name="applyAt"></param>
		public void ApplyWorldImpulse(Vector2 impulse, Vector2 applyAt)
		{
			if (this.body == null) return;
			this.body.ApplyLinearImpulse(impulse * 0.01f / Time.SPFMult, applyAt * 0.01f);
		}
		
		/// <summary>
		/// Applies a Transform-local angular force to the object.
		/// </summary>
		/// <param name="angularForce"></param>
		public void ApplyLocalForce(float angularForce)
		{
			if (this.body == null) return;
			this.body.ApplyTorque(angularForce * 0.01f / Time.SPFMult);
		}
		/// <summary>
		/// Applies a Transform-local force to the objects mass center.
		/// </summary>
		/// <param name="force"></param>
		public void ApplyLocalForce(Vector2 force)
		{
			this.ApplyWorldForce(this.gameobj.Transform.GetWorldVector(new Vector3(force)).Xy);
		}
		/// <summary>
		/// Applies a Transform-local force to the specified point.
		/// </summary>
		/// <param name="force"></param>
		/// <param name="applyAt"></param>
		public void ApplyLocalForce(Vector2 force, Vector2 applyAt)
		{
			this.ApplyWorldForce(
				this.gameobj.Transform.GetWorldVector(new Vector3(force)).Xy,
				this.gameobj.Transform.GetWorldPoint(new Vector3(applyAt)).Xy);
		}
		/// <summary>
		/// Applies a world force to the objects mass center.
		/// </summary>
		/// <param name="force"></param>
		public void ApplyWorldForce(Vector2 force)
		{
			if (this.body == null) return;
			this.body.ApplyForce(force * 0.01f / Time.SPFMult);
		}
		/// <summary>
		/// Applies a world force to the specified point.
		/// </summary>
		/// <param name="force"></param>
		/// <param name="applyAt"></param>
		public void ApplyWorldForce(Vector2 force, Vector2 applyAt)
		{
			if (this.body == null) return;
			this.body.ApplyForce(force * 0.01f / Time.SPFMult, applyAt * 0.01f);
		}

		/// <summary>
		/// Updates the Colliders internal body shape based on its set of <see cref="ShapeInfo"/> objects.
		/// </summary>
		public void UpdateBodyShape()
		{
			if (this.body == null) return;

			foreach (ShapeInfo info in this.shapes) info.UpdateFixture();
			this.body.CollisionCategories = this.colCat;
			this.body.CollidesWith = this.colWith;
			this.body.ResetMassData();

			this.AwakeBody();
		}
		private void CleanupBody()
		{
			if (this.body == null) return;

			this.body.Collision -= this.body_OnCollision;
			this.body.Separation -= this.body_OnSeparation;
			this.body.PostSolve -= this.body_PostSolve;

			this.body.Dispose();
			this.body = null;
		}
		private Body CreateBody()
		{
			Body b = new Body(Scene.CurrentPhysics, this);
			foreach (ShapeInfo s in this.shapes) s.CreateFixture(b);
			return b;
		}
		private void InitBody()
		{
			if (this.body != null) this.CleanupBody();
			Transform t = this.GameObj != null ? this.GameObj.Transform : null;

			this.body = this.CreateBody();
			this.UpdateBodyShape();

			this.body.BodyType = (this.bodyType == BodyType.Static ? FarseerPhysics.Dynamics.BodyType.Static : FarseerPhysics.Dynamics.BodyType.Dynamic);
			this.body.LinearDamping = this.linearDamp;
			this.body.AngularDamping = this.angularDamp;
			this.body.FixedRotation = this.fixedAngle;
			this.body.IgnoreGravity = this.ignoreGravity;
			this.body.CollisionCategories = this.colCat;
			this.body.CollidesWith = this.colWith;

			if (t != null)
			{
				this.body.SetTransform(t.Pos.Xy * 0.01f, t.Angle);
				this.body.LinearVelocity = t.Vel.Xy * 0.01f / Time.SPFMult;
				this.body.AngularVelocity = t.AngleVel / Time.SPFMult;
			}

			this.body.Collision += this.body_OnCollision;
			this.body.Separation += this.body_OnSeparation;
			this.body.PostSolve += this.body_PostSolve;
		}

		/// <summary>
		/// Awakes the body if it has been in a resting state that is now being left, such as
		/// when changing physical properties at runtime. You usually don't need to call this.
		/// </summary>
		public void AwakeBody()
		{
			if (this.body != null) this.body.Awake = true;
		}

		/// <summary>
		/// Performs a physical picking operation and returns the <see cref="ShapeInfo">shape</see> in which
		/// the specified world coordinate is located in.
		/// </summary>
		/// <param name="worldCoord"></param>
		/// <returns></returns>
		public ShapeInfo PickShape(Vector2 worldCoord)
		{
			if (this.body == null) return null;
			Vector2 fsWorldCoord = worldCoord * 0.01f;

			for (int i = 0; i < this.shapes.Count; i++)
			{
				Fixture f = this.body.FixtureList[i];
				if (f.TestPoint(ref fsWorldCoord)) return this.shapes[i];
			}
			return null;
		}
		/// <summary>
		/// Performs a physical picking operation and returns the <see cref="ShapeInfo">shapes</see> that
		/// intersect the specified world coordinate.
		/// </summary>
		/// <param name="worldCoord"></param>
		/// <returns></returns>
		public List<ShapeInfo> PickShapes(Vector2 worldCoord)
		{
			if (this.body == null) return new List<ShapeInfo>();

			List<ShapeInfo> picked = new List<ShapeInfo>();
			Vector2 fsWorldCoord = worldCoord * 0.01f;

			for (int i = 0; i < this.shapes.Count; i++)
			{
				Fixture f = this.body.FixtureList[i];
				if (f.TestPoint(ref fsWorldCoord)) picked.Add(this.shapes[i]);
			}

			return picked;
		}
		/// <summary>
		/// Performs a physical picking operation and returns the <see cref="ShapeInfo">shapes</see> that
		/// intersect the specified world coordinate area.
		/// </summary>
		/// <param name="worldCoord"></param>
		/// <param name="size"></param>
		/// <returns></returns>
		public List<ShapeInfo> PickShapes(Vector2 worldCoord, Vector2 size)
		{
			if (this.body == null) return new List<ShapeInfo>();
			Vector2 fsTemp;
			Vector2 fsWorldCoordStep;
			Vector2 fsWorldCoord = worldCoord * 0.01f;
			FarseerPhysics.Collision.AABB fsWorldAABB = new FarseerPhysics.Collision.AABB(fsWorldCoord, (worldCoord + size) * 0.01f);

			List<ShapeInfo> picked = new List<ShapeInfo>();
			for (int i = 0; i < this.shapes.Count; i++)
			{
				Fixture f = this.body.FixtureList[i];

				FarseerPhysics.Collision.AABB fAABB;
				FarseerPhysics.Common.Transform transform;
				this.body.GetTransform(out transform);
				f.Shape.ComputeAABB(out fAABB, ref transform, 0);
				
				if (fsWorldAABB.Contains(ref fAABB))
				{
					picked.Add(this.shapes[i]);
					continue;
				}
				else if (!FarseerPhysics.Collision.AABB.TestOverlap(ref fsWorldAABB, ref fAABB))
					continue;

				FarseerPhysics.Collision.AABB fAABBIntersect;
				fAABBIntersect.LowerBound = Vector2.ComponentMax(fAABB.LowerBound, fsWorldAABB.LowerBound);
				fAABBIntersect.UpperBound = Vector2.ComponentMin(fAABB.UpperBound, fsWorldAABB.UpperBound);

				fsWorldCoordStep = new Vector2(MathF.Max(this.shapes[i].AABB.w, 1.0f), MathF.Max(this.shapes[i].AABB.h, 1.0f)) * 0.05f * 0.01f;
				fsTemp = fAABBIntersect.LowerBound;
				do
				{
					if (f.TestPoint(ref fsTemp))
					{
						picked.Add(this.shapes[i]);
						break;
					}

					fsTemp.X += fsWorldCoordStep.X;
					if (fsTemp.X > fAABBIntersect.UpperBound.X)
					{
						fsTemp.X = fAABBIntersect.LowerBound.X;
						fsTemp.Y += fsWorldCoordStep.Y;
					}
					if (fsTemp.Y > fAABBIntersect.UpperBound.Y) break;
				} while (true);
			}

			return picked;
		}
		
		private bool body_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
		{
			this.eventBuffer.Add(new ColEvent(ColEvent.EventType.Collision, fixtureA, fixtureB, null));
			return true;
		}
		private void body_OnSeparation(Fixture fixtureA, Fixture fixtureB)
		{
			this.eventBuffer.Add(new ColEvent(ColEvent.EventType.Separation, fixtureA, fixtureB, null));
		}
        private void body_PostSolve(Contact contact, ContactConstraint impulse)
        {
            int count = contact.Manifold.PointCount;
            for (int i = 0; i < count; ++i)
            {
				if (impulse.Points[i].NormalImpulse > 0.0f || impulse.Points[i].TangentImpulse > 0.0f)
				{
					CollisionData colData = new CollisionData(this.body, impulse, i);
					if (contact.FixtureA.Body == this.body)
						this.eventBuffer.Add(new ColEvent(ColEvent.EventType.PostSolve, contact.FixtureA, contact.FixtureB, colData));
					else
						this.eventBuffer.Add(new ColEvent(ColEvent.EventType.PostSolve, contact.FixtureB, contact.FixtureA, colData));
				}
            }
        }
		
		void ICmpUpdatable.OnUpdate()
		{
			for (int i = 0; i < this.eventBuffer.Count; i++)
			{
				ColEvent e = this.eventBuffer[i];
				ColliderCollisionEventArgs args = new ColliderCollisionEventArgs(
					(e.FixtureB.Body.UserData as Collider).GameObj,
 					e.Data,
					e.FixtureA.UserData as ShapeInfo,
					e.FixtureB.UserData as ShapeInfo);

				if (e.Type == ColEvent.EventType.Collision)
					this.gameobj.NotifyCollisionBegin(this, args);
				else if (e.Type == ColEvent.EventType.Separation)
					this.gameobj.NotifyCollisionEnd(this, args);
				else if (e.Type == ColEvent.EventType.PostSolve)
					this.gameobj.NotifyCollisionSolve(this, args);
			}
			this.eventBuffer.Clear();
		}

		bool ITransformUpdater.IgnoreParent
		{
			get { return this.bodyType == BodyType.Dynamic; }
		}
		void ITransformUpdater.UpdateTransform(Transform t)
		{
			if (this.bodyType == BodyType.Dynamic)
			{
				t.SetTransform(
					new Vector3(this.body.Position.X * 100.0f, this.body.Position.Y * 100.0f, t.Pos.Z + t.Vel.Z * Time.TimeMult),
					new Vector3(this.body.LinearVelocity.X * 100.0f * Time.SPFMult, this.body.LinearVelocity.Y * 100.0f * Time.SPFMult, t.Vel.Z),
					t.Scale,
					this.body.Rotation,
					this.body.AngularVelocity * Time.SPFMult);
			}
			else
			{
				if (DualityApp.ExecContext == DualityApp.ExecutionContext.Game && (t.RelativeVel != Vector3.Zero || t.RelativeAngleVel != 0.0f))
				{
				    t.SetRelativeTransform(
				        t.RelativePos + t.RelativeVel * Time.TimeMult,
				        t.RelativeVel,
				        t.RelativeScale,
				        MathF.NormalizeAngle(t.RelativeAngle + t.RelativeAngleVel * Time.TimeMult),
				        t.RelativeAngleVel);
				    (this as ITransformUpdater).OnTransformChanged(t, Transform.DirtyFlags.Pos | Transform.DirtyFlags.Angle);
				}
			}
		}
		void ITransformUpdater.OnTransformChanged(Transform t, Transform.DirtyFlags changes)
		{
			if ((changes & Transform.DirtyFlags.Pos) != Transform.DirtyFlags.None)
				this.body.Position = t.Pos.Xy * 0.01f;
			if ((changes & Transform.DirtyFlags.Vel) != Transform.DirtyFlags.None)
				this.body.LinearVelocity = t.Vel.Xy * 0.01f / Time.SPFMult;
			if ((changes & Transform.DirtyFlags.Angle) != Transform.DirtyFlags.None)
				this.body.Rotation = t.Angle;
			if ((changes & Transform.DirtyFlags.AngleVel) != Transform.DirtyFlags.None)
				this.body.AngularVelocity = t.AngleVel / Time.SPFMult;
			if ((changes & Transform.DirtyFlags.Scale) != Transform.DirtyFlags.None)
				this.UpdateBodyShape();

			if (changes != Transform.DirtyFlags.None) this.body.Awake = true;
		}
		void ICmpInitializable.OnInit(Component.InitContext context)
		{
			if (context == InitContext.Activate)
			{
				this.InitBody();
				this.GameObj.Transform.RegisterExternalUpdater(this);
			}
		}
		void ICmpInitializable.OnShutdown(Component.ShutdownContext context)
		{
			if (context == ShutdownContext.Deactivate)
			{
				this.CleanupBody();
				this.GameObj.Transform.UnregisterExternalUpdater(this);
			}
		}

		internal override void CopyToInternal(Component target)
		{
			base.CopyToInternal(target);
			Collider c = target as Collider;

			bool wasInitialized = c.body != null;
			if (wasInitialized) c.CleanupBody();

			c.bodyType = this.bodyType;
			c.linearDamp = this.linearDamp;
			c.angularDamp = this.angularDamp;
			c.fixedAngle = this.fixedAngle;
			c.ignoreGravity = this.ignoreGravity;
			c.colCat = this.colCat;

			// Discard old shape list.
			c.shapes = null;
			if (this.shapes != null) c.SetShapes(this.shapes);

			if (wasInitialized) c.InitBody();
		}
		
		/// <summary>
		/// Performs a global physical picking operation and returns the <see cref="ShapeInfo">shape</see> in which
		/// the specified world coordinate is located in.
		/// </summary>
		/// <param name="worldCoord"></param>
		/// <returns></returns>
		public static ShapeInfo PickShapeGlobal(Vector2 worldCoord)
		{
			Vector2 fsWorldCoord = worldCoord * 0.01f;
			Fixture f = Scene.CurrentPhysics.TestPoint(fsWorldCoord);

			return f != null && f.UserData is ShapeInfo ? (f.UserData as ShapeInfo) : null;
		}
		/// <summary>
		/// Performs a global physical picking operation and returns the <see cref="ShapeInfo">shapes</see> that
		/// intersect the specified world coordinate.
		/// </summary>
		/// <param name="worldCoord"></param>
		/// <returns></returns>
		public static List<ShapeInfo> PickShapesGlobal(Vector2 worldCoord)
		{
			Vector2 fsWorldCoord = worldCoord * 0.01f;
			List<Fixture> fixtureList = Scene.CurrentPhysics.TestPointAll(fsWorldCoord);
			return new List<ShapeInfo>(fixtureList.Where(f => f != null && f.UserData is ShapeInfo).Select(f => f.UserData as ShapeInfo));
		}
		/// <summary>
		/// Performs a global physical picking operation and returns the <see cref="ShapeInfo">shapes</see> that
		/// intersect the specified world coordinate area.
		/// </summary>
		/// <param name="worldCoord"></param>
		/// <param name="size"></param>
		/// <returns></returns>
		public static List<ShapeInfo> PickShapesGlobal(Vector2 worldCoord, Vector2 size)
		{
			List<ShapeInfo> picked = new List<ShapeInfo>();

			Collider[] colliderArray = Scene.Current.ActiveObjects.GetComponents<Collider>().ToArray();
			foreach (Collider c in colliderArray)
				picked.AddRange(c.PickShapes(worldCoord, size));

			return picked;
		}
	}

	public class ColliderCollisionEventArgs : CollisionEventArgs
	{
		private	Collider.ShapeInfo	colShapeA;
		private	Collider.ShapeInfo	colShapeB;

		public Collider.ShapeInfo MyCollideShape
		{
			get { return this.colShapeA; }
		}
		public Collider.ShapeInfo OtherCollideShape
		{
			get { return this.colShapeB; }
		}

		public ColliderCollisionEventArgs(GameObject obj, CollisionData data, Collider.ShapeInfo shapeA, Collider.ShapeInfo shapeB) : base(obj, data)
		{
			this.colShapeA = shapeA;
			this.colShapeB = shapeB;
		}
	}
}

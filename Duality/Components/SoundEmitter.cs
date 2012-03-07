﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Graphics.OpenGL;
using OpenTK;

using Duality;
using Duality.EditorHints;
using Duality.VertexFormat;
using Duality.ColorFormat;
using Duality.Components;
using Duality.Resources;

namespace Duality.Components
{
	/// <summary>
	/// Provides functionality to emit sound.
	/// </summary>
	[Serializable]
	[RequiredComponent(typeof(Transform))]
	public sealed class SoundEmitter : Component, ICmpUpdatable, ICmpInitializable, ICmpEditorUpdatable
	{
		/// <summary>
		/// A single sound source.
		/// </summary>
		[Serializable]
		public class Source
		{
			private	bool				disposed	= false;
			private	ContentRef<Sound>	sound		= ContentRef<Sound>.Null;
			private	bool				looped		= true;
			private	bool				paused		= true;
			private	float				volume		= 1.0f;
			private	float				pitch		= 1.0f;
			private	Vector3				offset		= Vector3.Zero;
			[NonSerializedResource]	private	bool			hasBeenPlayed	= false;
			[NonSerialized]			private	SoundInstance	instance		= null;

			/// <summary>
			/// [GET] Returns whether this sound source has been disposed. Disposed objects are not to be used again.
			/// Treat them as null or similar.
			/// </summary>
			[EditorHintFlags(MemberFlags.Invisible)]
			public bool Disposed
			{
				get { return this.disposed; }
			}
			/// <summary>
			/// [GET] The <see cref="SoundInstance"/> that is currently allocated to emit
			/// this sources sound.
			/// </summary>
			[EditorHintFlags(MemberFlags.Invisible)]
			public SoundInstance Instance
			{
				get { return this.instance; }
			}
			/// <summary>
			/// [GET / SET] The <see cref="Duality.Resources.Sound"/> that is to be played by this source.
			/// </summary>
			public ContentRef<Sound> Sound
			{
				get { return this.sound; }
				set { this.sound = value; }
			}
			/// <summary>
			/// [GET / SET] Whether this source is looped.
			/// </summary>
			public bool Looped
			{
				get { return this.looped; }
				set 
				{ 
					if (this.instance != null) this.instance.Looped = value;
					this.looped = value;
				}
			}
			/// <summary>
			/// [GET / SET] Whether this source is paused.
			/// </summary>
			public bool Paused
			{
				get { return this.paused; }
				set 
				{ 
					if (this.instance != null) this.instance.Paused = value;
					this.paused = value;
				}
			}
			/// <summary>
			/// [GET / SET] The volume of this source.
			/// </summary>
			[EditorHintIncrement(0.1f)]
			[EditorHintRange(0.0f, 2.0f)]
			public float Volume
			{
				get { return this.volume; }
				set 
				{ 
					if (this.instance != null) this.instance.Volume = value;
					this.volume = value;
				}
			}
			/// <summary>
			/// [GET / SET] The sources pitch factor.
			/// </summary>
			[EditorHintIncrement(0.1f)]
			[EditorHintRange(0.0f, 10.0f)]
			public float Pitch
			{
				get { return this.pitch; }
				set 
				{ 
					if (this.instance != null) this.instance.Pitch = value;
					this.pitch = value;
				}
			}
			/// <summary>
			/// [GET / SET] The 3d offset of the emitted sound relative to the GameObject.
			/// </summary>
			public Vector3 Offset
			{
				get { return this.offset; }
				set
				{
					if (this.instance != null) this.instance.Pos = value;
					this.offset = value;
				}
			}

			public Source() {}
			public Source(ContentRef<Sound> snd, bool looped = true) : this(snd, looped, Vector3.Zero) {}
			public Source(ContentRef<Sound> snd, bool looped, Vector3 offset)
			{
				this.sound = snd;
				this.looped = looped;
				this.offset = offset;
			}

			/// <summary>
			/// Updates the sound source.
			/// </summary>
			/// <param name="emitter">The sources parent <see cref="SoundEmitter"/>.</param>
			/// <returns>True, if the source is still active. False, if it requests to be removed.</returns>
			public bool Update(SoundEmitter emitter)
			{
				// If the SoundInstance has been disposed, set to null
				if (this.instance != null && this.instance.Disposed) this.instance = null;

				// If there is a SoundInstance playing, but it's the wrong one, stop it
				if (this.instance != null && this.instance.SoundRef != this.sound)
				{
					this.instance.Stop();
					this.instance = null;
				}

				if (this.instance == null)
				{
					// If this Source isn't looped and it HAS been played already, remove it
					if (!this.looped && this.hasBeenPlayed) return false;

					// Play the sound
					this.instance = DualityApp.Sound.PlaySound3D(this.sound, emitter.GameObj);
					this.instance.Pos = this.offset;
					this.instance.Looped = this.looped;
					this.instance.Volume = this.volume;
					this.instance.Paused = this.paused;
					this.hasBeenPlayed = true;
				}

				return true;
			}

			/// <summary>
			/// Creates a deep copy of the sound source.
			/// </summary>
			/// <returns></returns>
			public Source Clone()
			{
				Source newSrc = new Source();
				newSrc.sound			= this.sound;
				newSrc.looped			= this.looped;
				newSrc.paused			= this.paused;
				newSrc.volume			= this.volume;
				newSrc.pitch			= this.pitch;
				newSrc.offset			= this.offset;
				newSrc.hasBeenPlayed	= this.hasBeenPlayed;
				return newSrc;
			}
		}

		private	List<Source>	sources	= new List<Source>();

		/// <summary>
		/// [GET / SET] A list of sound sources this SoundEmitter maintains. Is never null.
		/// </summary>
		public List<Source> Sources
		{
			get { return this.sources; }
			set { this.sources = value; if (this.sources == null) this.sources = new List<Source>(); }
		}

		public SoundEmitter()
		{
		}
		internal override void CopyToInternal(Component target)
		{
			base.CopyToInternal(target);
			SoundEmitter c = target as SoundEmitter;
			c.sources = this.sources == null ? null : new List<Source>(this.sources.Select(s => s.Clone()));
		}

		void ICmpUpdatable.OnUpdate()
		{
			for (int i = this.sources.Count - 1; i >= 0; i--)
				if (this.sources[i] != null && !this.sources[i].Update(this)) this.sources.RemoveAt(i);
		}
		void ICmpEditorUpdatable.OnUpdate()
		{
			if (DualityApp.ExecContext != DualityApp.ExecutionContext.Game)
			{
				for (int i = this.sources.Count - 1; i >= 0; i--)
					if (this.sources[i].Instance != null) this.sources[i].Instance.Stop();
			}
		}
		void ICmpInitializable.OnInit(Component.InitContext context)
		{
		}
		void ICmpInitializable.OnShutdown(Component.ShutdownContext context)
		{
			if (context == ShutdownContext.Deactivate || context == ShutdownContext.RemovingFromGameObject)
			{
				for (int i = this.sources.Count - 1; i >= 0; i--)
					if (this.sources[i].Instance != null) this.sources[i].Instance.Stop();
			}
		}
	}
}

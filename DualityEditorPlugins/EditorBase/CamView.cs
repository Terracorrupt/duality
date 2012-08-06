﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using BitArray = System.Collections.BitArray;

using WeifenLuo.WinFormsUI.Docking;

using Duality;
using Duality.Components;
using Duality.ColorFormat;
using Duality.Resources;

using DualityEditor;
using DualityEditor.Forms;

using EditorBase.CamViewStates;
using EditorBase.CamViewLayers;

using OpenTK;
using Key = OpenTK.Input.Key;
using MouseButton = OpenTK.Input.MouseButton;
using MouseButtonEventArgs = OpenTK.Input.MouseButtonEventArgs;
using KeyboardKeyEventArgs = OpenTK.Input.KeyboardKeyEventArgs;
using MouseMoveEventArgs = OpenTK.Input.MouseMoveEventArgs;
using MouseWheelEventArgs = OpenTK.Input.MouseWheelEventArgs;

namespace EditorBase
{
	public partial class CamView : DockContent, IHelpProvider, IMouseInput, IKeyboardInput
	{
		public class CameraChangedEventArgs : EventArgs
		{
			private	Camera	prevCam	= null;
			private	Camera	nextCam	= null;

			public Camera PreviousCamera
			{
				get { return this.prevCam; }
			}
			public Camera NextCamera
			{
				get { return this.nextCam; }
			}

			public CameraChangedEventArgs(Camera prev, Camera next)
			{
				this.prevCam = prev;
				this.nextCam = next;
			}
		}
		private class StateEntry
		{
			private Type stateType;
			private CamViewState state;

			public Type StateType
			{
				get { return this.stateType; }
			}
			public string StateName
			{
				get { return this.state.StateName; }
			}

			public StateEntry(Type stateType, CamViewState state)
			{
				this.stateType = stateType;
				this.state = state;
			}

			public override string ToString()
			{
				return this.StateName;
			}
		}
		private class LayerEntry
		{
			private Type layerType;
			private CamViewLayer layer;

			public Type LayerType
			{
				get { return this.layerType; }
			}
			public string LayerName
			{
				get { return this.layer.LayerName; }
			}
			public string LayerDesc
			{
				get { return this.layer.LayerDesc; }
			}

			public LayerEntry(Type stateType, CamViewLayer layer)
			{
				this.layerType = stateType;
				this.layer = layer;
			}

			public override string ToString()
			{
				return this.LayerName;
			}
		}

		public const float DefaultDisplayBoundRadius = 25.0f;

		private	int					runtimeId		= 0;
		private	GLControl			glControl		= null;
		private	GameObject			camObj			= null;
		private	Camera				camComp			= null;
		private	bool				camInternal		= false;
		private	CamViewState		activeState		= null;
		private	List<CamViewLayer>	activeLayers	= new List<CamViewLayer>();
		private	List<Type>			lockedLayers	= new List<Type>();
		private	ColorPickerDialog	bgColorDialog	= new ColorPickerDialog();
		private	GameObject			nativeCamObj	= null;
		private	string				loadTempState	= null;

		private	Dictionary<Type,CamViewLayer>	availLayers	= new Dictionary<Type,CamViewLayer>();
		private	Dictionary<Type,CamViewState>	availStates	= new Dictionary<Type,CamViewState>();

		private	int		inputMouseX			= 0;
		private	int		inputMouseY			= 0;
		private	int		inputMouseWheel		= 0;
		private	int		inputMouseButtons	= 0;
		private	event	EventHandler<MouseButtonEventArgs>	inputMouseDown			= null;
		private	event	EventHandler<MouseButtonEventArgs>	inputMouseUp			= null;
		private	event	EventHandler<MouseMoveEventArgs>	inputMouseMove			= null;
		private	event	EventHandler<MouseWheelEventArgs>	inputMouseWheelChanged	= null;

		private	bool		inputKeyRepeat	= false;
		private	BitArray	inputKeyPressed	= new BitArray((int)Key.LastKey + 1, false);
		private	event		EventHandler<KeyboardKeyEventArgs>	inputKeyDown	= null;
		private	event		EventHandler<KeyboardKeyEventArgs>	inputKeyUp		= null;

		public event EventHandler ParallaxRefDistChanged	= null;
		public event EventHandler<CameraChangedEventArgs> CurrentCameraChanged	= null;

		public ColorRgba BgColor
		{
			get { return this.camComp.ClearColor; }
			set { this.camComp.ClearColor = value; }
		}
		public ColorRgba FgColor
		{
			get { return this.BgColor.GetLuminance() < 0.5f ? ColorRgba.White : ColorRgba.Black; }
		}
		public float NearZ
		{
			get { return this.camComp.NearZ; }
			set { this.camComp.NearZ = value; }
		}
		public float FarZ
		{
			get { return this.camComp.FarZ; }
			set { this.camComp.FarZ = value; }
		}
		public float ParallaxRefDist
		{
			get { return (float)this.parallaxRefDist.Value; }
			set { this.parallaxRefDist.Value = Math.Min(Math.Max((decimal)value, this.parallaxRefDist.Minimum), this.parallaxRefDist.Maximum); }
		}
		public float ParallaxRefDistIncrement
		{
			get { return (float)this.parallaxRefDist.Increment; }
		}
		public bool ParallaxActive
		{
			get { return this.toggleParallaxity.Checked; }
			set { this.toggleParallaxity.Checked = value; }
		}
		public Camera CameraComponent
		{
			get { return this.camComp; }
		}
		public GameObject CameraObj
		{
			get { return this.camObj; }
		}
		public GLControl LocalGLControl
		{
			get { return this.glControl; }
		}
		public GLControl MainContextControl
		{
			get { return MainForm.Instance.MainContextControl; }
		}
		public ToolStrip ToolbarCamera
		{
			get { return this.toolbarCamera; }
		}
		public CamViewState ViewState
		{
			get { return this.activeState; }
		}
		public IEnumerable<CamViewLayer> ActiveViewLayers
		{
			get { return this.activeLayers; }
		}

		public CamView(int runtimeId)
		{
			this.InitializeComponent();
			this.bgColorDialog.OldColor = Color.FromArgb(64, 64, 64);
			this.bgColorDialog.SelectedColor = this.bgColorDialog.OldColor;
			this.bgColorDialog.AlphaEnabled = false;
			this.Text = PluginRes.EditorBaseRes.MenuItemName_CamView + " #" + runtimeId;
			this.runtimeId = runtimeId;
			this.toolbarCamera.Renderer = new DualityEditor.Controls.ToolStrip.DualitorToolStripProfessionalRenderer();
			
			var camViewStateTypeQuery = 
				from t in MainForm.Instance.GetAvailDualityEditorTypes(typeof(CamViewState))
				where !t.IsAbstract
				select t;
			foreach (Type t in camViewStateTypeQuery)
			{
				CamViewState state = t.CreateInstanceOf() as CamViewState;
				state.View = this;
				this.availStates.Add(t, state);
			}

			var camViewLayerTypeQuery = 
				from t in MainForm.Instance.GetAvailDualityEditorTypes(typeof(CamViewLayer))
				where !t.IsAbstract
				select t;
			foreach (Type t in camViewLayerTypeQuery)
			{
				CamViewLayer layer = t.CreateInstanceOf() as CamViewLayer;
				layer.View = this;
				this.availLayers.Add(t, layer);
			}
		}
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			this.InitGLControl();
			this.InitNativeCamera();
			this.InitCameraSelector();
			this.InitStateSelector();
			this.InitLayerSelector();
			this.SetCurrentCamera(null);

			// Initialize state
			Type stateType = ReflectionHelper.ResolveType(this.loadTempState, false) ?? typeof(SceneEditorCamViewState);
			this.SetCurrentState(stateType);

			// Register DualityApp updater for camera steering behaviour
			MainForm.Instance.ResourceModified += this.EditorForm_ResourceModified;
			MainForm.Instance.ObjectPropertyChanged += this.EditorForm_ObjectPropertyChanged;
			Scene.Leaving += this.Scene_Leaving;
			Scene.GameObjectUnregistered += this.Scene_GameObjectUnregistered;
			Scene.RegisteredObjectComponentRemoved += this.Scene_RegisteredObjectComponentRemoved;

			// Update Camera values according to GUI (which carries loaded or default settings)
			this.parallaxRefDist_ValueChanged(this.parallaxRefDist, null);
			this.toggleParallaxity_CheckStateChanged(this.toggleParallaxity, null);
			this.bgColorDialog_ValueChanged(this.bgColorDialog, null);

			// Update camera transform properties & GUI
			this.OnCamTransformChanged();
		}
		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			if (this.camObj != null && !this.camInternal) MainForm.Instance.EditorObjects.UnregisterObj(this.camObj);
			if (this.nativeCamObj != null) this.nativeCamObj.Dispose();

			MainForm.Instance.ResourceModified -= this.EditorForm_ResourceModified;
			MainForm.Instance.ObjectPropertyChanged -= this.EditorForm_ObjectPropertyChanged;
			Scene.Leaving -= this.Scene_Leaving;
			Scene.GameObjectUnregistered -= this.Scene_GameObjectUnregistered;
			Scene.RegisteredObjectComponentRemoved -= this.Scene_RegisteredObjectComponentRemoved;

			this.SetCurrentState((CamViewState)null);
		}
		
		private void InitGLControl()
		{
			this.SuspendLayout();

			// Get rid of a possibly existing old glControl
			if (this.glControl != null)
			{
				this.glControl.Dispose();
				this.Controls.Remove(this.glControl);
			}

			// Create a new glControl
			this.glControl = new GLControl(this.MainContextControl.GraphicsMode);
			this.glControl.BackColor = Color.Black;
			this.glControl.Dock = DockStyle.Fill;
			this.glControl.Name = "glControl";
			this.glControl.VSync = false;
			this.glControl.AllowDrop = true;
			this.glControl.MouseDown += this.glControl_MouseDown;
			this.glControl.MouseUp += this.glControl_MouseUp;
			this.glControl.MouseWheel += this.glControl_MouseWheel;
			this.glControl.MouseMove += this.glControl_MouseMove;
			this.glControl.GotFocus += this.glControl_GotFocus;
			this.glControl.PreviewKeyDown += glControl_PreviewKeyDown;
			this.glControl.KeyDown += this.glControl_KeyDown;
			this.glControl.KeyUp += this.glControl_KeyUp;
			this.glControl.Resize += this.glControl_Resize;
			this.Controls.Add(this.glControl);
			this.Controls.SetChildIndex(this.glControl, 0);

			this.ResumeLayout(true);
		}
		private void InitStateSelector()
		{
			this.stateSelector.Items.Clear();

			foreach (var pair in this.availStates)
				this.stateSelector.Items.Add(new StateEntry(pair.Key, pair.Value));
		}
		private void InitLayerSelector()
		{
			this.layerSelector.DropDown.Closing -= this.layerSelector_Closing;
			this.layerSelector.DropDownItems.Clear();

			IEnumerable<Type> camViewStateTypeQuery = 
				from t in MainForm.Instance.GetAvailDualityEditorTypes(typeof(CamViewLayer))
				where !t.IsAbstract
				select t;

			foreach (var pair in this.availLayers)
			{
				LayerEntry layerEntry = new LayerEntry(pair.Key, pair.Value);
				ToolStripMenuItem layerItem = new ToolStripMenuItem(layerEntry.LayerName);
				layerItem.Tag = layerEntry;
				layerItem.ToolTipText = layerEntry.LayerDesc;
				layerItem.Checked = this.activeLayers.Any(l => l.GetType() == layerEntry.LayerType);
				layerItem.Enabled = !this.lockedLayers.Contains(layerEntry.LayerType);
				this.layerSelector.DropDownItems.Add(layerItem);
			}
			this.layerSelector.DropDown.Closing += this.layerSelector_Closing;
		}
		private void InitCameraSelector()
		{
			this.camSelector.Items.Clear();
			this.camSelector.Items.Add(this.nativeCamObj.Camera);

			foreach (Camera c in Scene.Current.AllObjects.GetComponents<Camera>().OrderBy(c => c.GameObj.FullName))
				this.camSelector.Items.Add(c);
		}
		private void InitNativeCamera()
		{
			// Create internal Camera object
			this.nativeCamObj = new GameObject();
			this.nativeCamObj.Name = "CamView Camera " + this.runtimeId;
			this.nativeCamObj.AddComponent<Transform>();
			this.nativeCamObj.AddComponent<SoundListener>().MakeCurrent();

			Camera c = this.nativeCamObj.AddComponent<Camera>();
			c.ClearColor = ColorRgba.DarkGrey;
			c.FarZ = 100000.0f;

			this.nativeCamObj.Transform.Pos = new Vector3(0.0f, 0.0f, -c.ParallaxRefDist);
			MainForm.Instance.EditorObjects.RegisterObj(this.nativeCamObj);
		}

		public void SetCurrentCamera(Camera c)
		{
			if (c == null) c = this.nativeCamObj.Camera;
			if (c == this.camComp) return;

			Camera prev = this.camComp;
			if (this.camObj != null && !this.camInternal)
				MainForm.Instance.EditorObjects.UnregisterObj(this.camObj);

			if (c.GameObj == this.nativeCamObj)
			{
				this.camInternal = true;
				this.camObj = this.nativeCamObj;
				this.camComp = this.camObj.Camera;
				this.camSelector.SelectedIndex = 0;
			}
			else
			{
				this.camInternal = false;
				this.camObj = c.GameObj;
				this.camComp = c;
				MainForm.Instance.EditorObjects.RegisterObj(this.camObj);
				this.camSelector.SelectedIndex = this.camSelector.Items.IndexOf(c);
			}

			this.OnCurrentCameraChanged(prev, this.camComp);
			this.glControl.Invalidate();
		}
		public void SetCurrentState(Type stateType)
		{
			if (!typeof(CamViewState).IsAssignableFrom(stateType)) return;
			if (this.activeState != null && this.activeState.GetType() == stateType) return;

			this.SetCurrentState(this.availStates[stateType]);
		}
		public void SetCurrentState(CamViewState state)
		{
			if (this.activeState == state) return;
			if (this.activeState != null) this.activeState.OnLeaveState();

			this.activeState = state;
			if (this.activeState != null)
				this.stateSelector.SelectedIndex = this.stateSelector.Items.IndexOf(this.stateSelector.Items.Cast<StateEntry>().FirstOrDefault(e => e.StateType == this.activeState.GetType()));
			else
				this.stateSelector.SelectedIndex = -1;

			if (this.activeState != null) this.activeState.OnEnterState();
			this.glControl.Invalidate();
		}

		public void SetActiveLayers(params Type[] layerTypes)
		{
			this.SetActiveLayers((IEnumerable<Type>)layerTypes);
		}
		public void SetActiveLayers(IEnumerable<Type> layerTypes)
		{
			// Deactivate unused layers
			for (int i = this.activeLayers.Count - 1; i >= 0; i--)
			{
				Type layerType = this.activeLayers[i].GetType();
				if (!layerTypes.Contains(layerType)) this.DeactivateLayer(this.activeLayers[i]);
			}

			// Activate not-yet-active layers
			foreach (Type layerType in layerTypes)
				this.ActivateLayer(layerType);
		}
		public void ActivateLayer(CamViewLayer layer)
		{
			if (activeLayers == null) return;
			if (this.activeLayers.Contains(layer)) return;
			if (this.activeLayers.Any(l => l.GetType() == layer.GetType())) return;
			if (this.lockedLayers.Contains(layer.GetType())) return;

			this.activeLayers.Add(layer);
			layer.View = this;
			layer.OnActivateLayer();
			this.glControl.Invalidate();
		}
		public void ActivateLayer(Type layerType)
		{
			this.ActivateLayer(this.availLayers[layerType]);
		}
		public void DeactivateLayer(CamViewLayer layer)
		{
			if (activeLayers == null) return;
			if (!this.activeLayers.Contains(layer)) return;
			if (this.lockedLayers.Contains(layer.GetType())) return;

			layer.OnDeactivateLayer();
			layer.View = null;
			this.activeLayers.Remove(layer);
			this.glControl.Invalidate();
		}
		public void DeactivateLayer(Type layerType)
		{
			this.DeactivateLayer(this.activeLayers.FirstOrDefault(l => l.GetType() == layerType));
		}
		public void LockLayer(CamViewLayer layer)
		{
			this.LockLayer(layer.GetType());
		}
		public void LockLayer(Type layerType)
		{
			if (this.lockedLayers.Contains(layerType)) return;
			this.lockedLayers.Add(layerType);
		}
		public void UnlockLayer(CamViewLayer layer)
		{
			this.UnlockLayer(layer.GetType());
		}
		public void UnlockLayer(Type layerType)
		{
			this.lockedLayers.Remove(layerType);
		}

		internal void SaveUserData(XmlElement node)
		{
			node.SetAttribute("toggleParallaxity", this.toggleParallaxity.Checked.ToString(CultureInfo.InvariantCulture));
			node.SetAttribute("parallaxRefDist", this.nativeCamObj.Camera.ParallaxRefDist.ToString(CultureInfo.InvariantCulture));
			node.SetAttribute("bgColorArgb", this.nativeCamObj.Camera.ClearColor.ToIntArgb().ToString(CultureInfo.InvariantCulture));

			if (this.activeState != null) 
				node.SetAttribute("activeState", this.activeState.GetType().GetTypeId());

			var stateListNode = node.OwnerDocument.CreateElement("states");
			foreach (var pair in this.availStates)
			{
				var stateNode = node.OwnerDocument.CreateElement(pair.Key.GetTypeId());
				pair.Value.SaveUserData(stateNode);
				stateListNode.AppendChild(stateNode);
			}
			node.AppendChild(stateListNode);

			var layerListNode = node.OwnerDocument.CreateElement("layers");
			foreach (var pair in this.availLayers)
			{
				var layerNode = node.OwnerDocument.CreateElement(pair.Key.GetTypeId());
				pair.Value.SaveUserData(layerNode);
				layerListNode.AppendChild(layerNode);
			}
			node.AppendChild(layerListNode);
		}
		internal void LoadUserData(XmlElement node)
		{
			bool tryParseBool;
			decimal tryParseDecimal;
			int tryParseInt;

			if (bool.TryParse(node.GetAttribute("toggleParallaxity"), out tryParseBool))
				this.toggleParallaxity.Checked = tryParseBool;
			if (decimal.TryParse(node.GetAttribute("parallaxRefDist"), out tryParseDecimal))
				this.parallaxRefDist.Value = Math.Abs(tryParseDecimal);
			if (int.TryParse(node.GetAttribute("bgColorArgb"), out tryParseInt))
			{
				this.bgColorDialog.OldColor = Color.FromArgb(tryParseInt);
				this.bgColorDialog.SelectedColor = this.bgColorDialog.OldColor;
			}

			this.loadTempState = node.GetAttribute("activeState");

			var stateListNode = node.ChildNodes.OfType<XmlElement>().FirstOrDefault(e => e.Name == "states");
			if (stateListNode != null)
			{
				foreach (var pair in this.availStates)
				{
					var stateNode = stateListNode.ChildNodes.OfType<XmlElement>().FirstOrDefault(e => e.Name == pair.Key.GetTypeId());
					if (stateNode == null) continue;
					pair.Value.LoadUserData(stateNode);
				}
			}

			var layerListNode = node.ChildNodes.OfType<XmlElement>().FirstOrDefault(e => e.Name == "layers");
			if (layerListNode != null)
			{
				foreach (var pair in this.availLayers)
				{
					var layerNode = layerListNode.ChildNodes.OfType<XmlElement>().FirstOrDefault(e => e.Name == pair.Key.GetTypeId());
					if (layerNode == null) continue;
					pair.Value.LoadUserData(layerNode);
				}
			}
		}

		public void OnCamTransformChanged()
		{
			//if (this.camInternal) return;
			MainForm.Instance.NotifyObjPropChanged(
				this, new ObjectSelection(this.camObj.Transform),
				ReflectionInfo.Property_Transform_RelativeVel,
				ReflectionInfo.Property_Transform_RelativeAngleVel,
				ReflectionInfo.Property_Transform_RelativeAngle,
				ReflectionInfo.Property_Transform_RelativePos);
		}
		public void SetToolbarCamSettingsEnabled(bool value)
		{
			this.toggleParallaxity.Enabled = value;
			this.parallaxRefDist.Enabled = value;
			this.camSelector.Enabled = value;
			this.showBgColorDialog.Enabled = value;
			this.layerSelector.Enabled = value;
		}

		public void FocusOnObject(GameObject obj)
		{
			if (obj == null || obj.Transform == null) return;
			if (!this.activeState.CameraActionAllowed) return;
			Vector3 targetPos = obj.Transform.Pos - Vector3.UnitZ * this.camComp.ParallaxRefDist;
			targetPos.Z = MathF.Min(this.camObj.Transform.Pos.Z, targetPos.Z);
			this.camObj.Transform.Pos = targetPos;
			this.OnCamTransformChanged();
			this.LocalGLControl.Invalidate();
		}

		public void MakeDualityTarget()
		{
			DualityApp.TargetMode = this.MainContextControl.Context.GraphicsMode;
			DualityApp.TargetResolution = new OpenTK.Vector2(this.glControl.Width, this.glControl.Height);
			if (this.ContainsFocus) MainForm.Instance.SetCurrentDualityAppInput(this, this);
		}
		public ICmpRenderer PickRendererAt(int x, int y)
		{
			x = MathF.Clamp(x, 0, this.glControl.Width - 1);
			y = MathF.Clamp(y, 0, this.glControl.Height - 1);

			this.MainContextControl.Context.MakeCurrent(this.glControl.WindowInfo);
			this.MakeDualityTarget();
			return this.camComp.PickRendererAt(x, y);
		}
		public HashSet<ICmpRenderer> PickRenderersIn(int x, int y, int w, int h)
		{
			x = MathF.Clamp(x, 0, this.glControl.Width - 1);
			y = MathF.Clamp(y, 0, this.glControl.Height - 1);
			w = MathF.Clamp(w, 1, this.glControl.Width - x);
			h = MathF.Clamp(h, 1, this.glControl.Height - y);

			this.MainContextControl.Context.MakeCurrent(this.glControl.WindowInfo);
			this.MakeDualityTarget();
			return this.camComp.PickRenderersIn(x, y, w, h);
		}
		public float GetScaleAtZ(float z)
		{
			this.MakeDualityTarget();
			return this.camComp.GetScaleAtZ(z);
		}
		public Vector3 GetSpaceCoord(Vector3 screenCoord)
		{
			this.MakeDualityTarget();
			return this.camComp.GetSpaceCoord(screenCoord);
		}
		public Vector3 GetSpaceCoord(Vector2 screenCoord)
		{
			this.MakeDualityTarget();
			return this.camComp.GetSpaceCoord(screenCoord);
		}
		public Vector3 GetScreenCoord(Vector3 spaceCoord)
		{
			this.MakeDualityTarget();
			return this.camComp.GetScreenCoord(spaceCoord);
		}

		private void OnParallaxRefDistChanged()
		{
			if (!this.camInternal)
			{
				MainForm.Instance.NotifyObjPropChanged(
					this, new ObjectSelection(this.camComp),
					ReflectionInfo.Property_Camera_ParallaxRefDist);
			}
			this.glControl.Invalidate();

			if (this.ParallaxRefDistChanged != null)
				this.ParallaxRefDistChanged(this, EventArgs.Empty);
		}
		private void OnCurrentCameraChanged(Camera prev, Camera next)
		{
			if (this.CurrentCameraChanged != null)
				this.CurrentCameraChanged(this, new CameraChangedEventArgs(prev, next));
		}

		private void glControl_MouseDown(object sender, MouseEventArgs e)
		{
			if (DualityApp.ExecContext == DualityApp.ExecutionContext.Game)
			{
				MouseButton inputButton = e.Button.ToOpenTKSingle();
				this.inputMouseButtons |= e.Button.ToOpenTK();
				if (this.inputMouseDown != null) this.inputMouseDown(this, new MouseButtonEventArgs(e.X, e.Y, inputButton, true));
			}
		}
		private void glControl_MouseUp(object sender, MouseEventArgs e)
		{
			if (DualityApp.ExecContext == DualityApp.ExecutionContext.Game)
			{
				MouseButton inputButton = e.Button.ToOpenTKSingle();
				this.inputMouseButtons &= ~e.Button.ToOpenTK();
				if (this.inputMouseUp != null) this.inputMouseUp(this, new MouseButtonEventArgs(e.X, e.Y, inputButton, false));
			}
		}
		private void glControl_MouseWheel(object sender, MouseEventArgs e)
		{
			if (DualityApp.ExecContext == DualityApp.ExecutionContext.Game)
			{
				this.inputMouseWheel += e.Delta;
				if (this.inputMouseWheelChanged != null) this.inputMouseWheelChanged(this, new MouseWheelEventArgs(e.X, e.Y, this.inputMouseWheel, e.Delta));
			}
		}
		private void glControl_MouseMove(object sender, MouseEventArgs e)
		{
			if (DualityApp.ExecContext == DualityApp.ExecutionContext.Game)
			{
				int lastX = this.inputMouseX;
				int lastY = this.inputMouseY;
				this.inputMouseX = e.X;
				this.inputMouseY = e.Y;
				if (this.inputMouseMove != null) this.inputMouseMove(this, new MouseMoveEventArgs(e.X, e.Y, e.X - lastX, e.Y - lastY));
			}
		}
		private void glControl_GotFocus(object sender, EventArgs e)
		{
			if (DualityApp.ExecContext == DualityApp.ExecutionContext.Terminated) return;

			if (this.camObj.GetComponent<SoundListener>() != null)
				this.camObj.GetComponent<SoundListener>().MakeCurrent();

			this.activeState.SelectObjects(this.activeState.SelectedObjects);
		}
		private void glControl_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			if (DualityApp.ExecContext == DualityApp.ExecutionContext.Game) 
				e.IsInputKey = true;
		}
		private void glControl_KeyDown(object sender, KeyEventArgs e)
		{
			if (DualityApp.ExecContext == DualityApp.ExecutionContext.Game)
			{
				Key inputKey = e.KeyCode.ToOpenTKSingle();
				bool wasPressed = this.inputKeyPressed[(int)inputKey];
				this.inputKeyPressed = this.inputKeyPressed.Or(e.KeyCode.ToOpenTK());
				if (this.inputKeyDown != null)
				{
					if (this.inputKeyRepeat || !wasPressed)
						this.inputKeyDown(this, inputKey.ToEventArgs());
				}
			}
		}
		private void glControl_KeyUp(object sender, KeyEventArgs e)
		{
			if (DualityApp.ExecContext == DualityApp.ExecutionContext.Game)
			{
				Key inputKey = e.KeyCode.ToOpenTKSingle();
				this.inputKeyPressed = this.inputKeyPressed.And(e.KeyCode.ToOpenTK().Not());
				if (this.inputKeyUp != null) this.inputKeyUp(this, inputKey.ToEventArgs());
			}
		}
		private void glControl_Resize(object sender, EventArgs e)
		{
			this.glControl.Invalidate();
		}

		private void toggleParallaxity_CheckStateChanged(object sender, EventArgs e)
		{
			if (this.camComp == null) return;

			this.camComp.ParallaxRefDist = this.toggleParallaxity.Checked ? (float)this.parallaxRefDist.Value : -(float)this.parallaxRefDist.Value;
			this.OnParallaxRefDistChanged();
		}
		private void parallaxRefDist_ValueChanged(object sender, EventArgs e)
		{
			if (this.camComp == null) return;

			if (this.parallaxRefDist.Value < 30m)
				this.parallaxRefDist.Increment = 1m;
			else if (this.parallaxRefDist.Value < 150m)
				this.parallaxRefDist.Increment = 5m;
			else if (this.parallaxRefDist.Value < 300m)
				this.parallaxRefDist.Increment = 10m;
			else if (this.parallaxRefDist.Value < 1500m)
				this.parallaxRefDist.Increment = 50m;
			else if (this.parallaxRefDist.Value < 3000m)
				this.parallaxRefDist.Increment = 100m;
			else if (this.parallaxRefDist.Value < 15000m)
				this.parallaxRefDist.Increment = 500m;
			else if (this.parallaxRefDist.Value < 30000m)
				this.parallaxRefDist.Increment = 1000m;
			else if (this.parallaxRefDist.Value < 150000m)
				this.parallaxRefDist.Increment = 5000m;
			else
				this.parallaxRefDist.Increment = 10000m;

			this.camComp.ParallaxRefDist = this.toggleParallaxity.Checked ? (float)this.parallaxRefDist.Value : -(float)this.parallaxRefDist.Value;
			this.OnParallaxRefDistChanged();
		}
		private void showBgColorDialog_Click(object sender, EventArgs e)
		{
			this.bgColorDialog.OldColor = Color.FromArgb(
				255,
				this.camComp.ClearColor.R,
				this.camComp.ClearColor.G,
				this.camComp.ClearColor.B);
			this.bgColorDialog.PrimaryAttribute = ColorPickerDialog.PrimaryAttrib.Hue;
			if (this.bgColorDialog.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
				this.bgColorDialog_ValueChanged(this.bgColorDialog, null);
		}
		private void bgColorDialog_ValueChanged(object sender, EventArgs e)
		{
			this.camComp.ClearColor = new ColorRgba(
				this.bgColorDialog.SelectedColor.R,
				this.bgColorDialog.SelectedColor.G,
				this.bgColorDialog.SelectedColor.B,
				0);
			if (!this.camInternal)
			{
				MainForm.Instance.NotifyObjPropChanged(
					this, new ObjectSelection(this.camComp),
					ReflectionInfo.Property_Camera_ClearColor);
			}
			this.glControl.Invalidate();
		}
		
		private void EditorForm_ResourceModified(object sender, ResourceEventArgs e)
		{
			if (!e.IsResource) return;
			this.glControl.Invalidate();
		}
		private void EditorForm_ObjectPropertyChanged(object sender, ObjectPropertyChangedEventArgs e)
		{
			this.glControl.Invalidate();
		}

		private void Scene_Leaving(object sender, EventArgs e)
		{
			if (!this.camInternal) this.SetCurrentCamera(null);
			this.glControl.Invalidate();
		}
		private void Scene_RegisteredObjectComponentRemoved(object sender, ComponentEventArgs e)
		{
			if (this.camComp == e.Component) this.SetCurrentCamera(null);
		}
		private void Scene_GameObjectUnregistered(object sender, ObjectManagerEventArgs<GameObject> e)
		{
			if (this.camObj == e.Object) this.SetCurrentCamera(null);
		}

		private void camSelector_DropDown(object sender, EventArgs e)
		{
			this.InitCameraSelector();
		}
		private void camSelector_DropDownClosed(object sender, EventArgs e)
		{
			if (this.camSelector.SelectedIndex == -1)
			{
				this.camSelector.SelectedIndex = this.camSelector.Items.IndexOf(this.camComp);
				return;
			}
		}
		private void camSelector_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.SetCurrentCamera(this.camSelector.SelectedItem as Camera);
		}
		private void stateSelector_DropDown(object sender, EventArgs e)
		{
			this.InitStateSelector();
		}
		private void stateSelector_DropDownClosed(object sender, EventArgs e)
		{
			if (this.stateSelector.SelectedIndex == -1)
			{
				this.stateSelector.SelectedIndex = this.activeState != null ? this.stateSelector.Items.IndexOf(this.stateSelector.Items.Cast<StateEntry>().FirstOrDefault(sce => sce.StateType == this.activeState.GetType())) : -1;
				return;
			}
		}
		private void stateSelector_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.stateSelector.SelectedItem == null) return;
			this.SetCurrentState(((StateEntry)this.stateSelector.SelectedItem).StateType);
		}
		private void layerSelector_DropDownOpening(object sender, EventArgs e)
		{
			this.InitLayerSelector();
		}
		private void layerSelector_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			LayerEntry layerEntry = e.ClickedItem.Tag as LayerEntry;
			ToolStripMenuItem layerItem = e.ClickedItem as ToolStripMenuItem;

			if (layerItem.Checked)
				this.DeactivateLayer(layerEntry.LayerType);
			else
				this.ActivateLayer(layerEntry.LayerType);

			layerItem.Checked = this.activeLayers.Any(l => l.GetType() == layerEntry.LayerType);
		}
		private void layerSelector_Closing(object sender, ToolStripDropDownClosingEventArgs e)
		{
			if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked) e.Cancel = true;
		}

		HelpInfo IHelpProvider.ProvideHoverHelp(Point localPos, ref bool captured)
		{
			HelpInfo result = null;
			Point globalPos = this.PointToScreen(localPos);

			Point glLocalPos = this.glControl.PointToClient(globalPos);
			if (this.glControl.ClientRectangle.Contains(glLocalPos))
			{
				if (this.activeState is IHelpProvider)
				{
					IHelpProvider stateHelper = this.activeState as IHelpProvider;
					result = stateHelper.ProvideHoverHelp(glLocalPos, ref captured);
				}
			}

			return result;
		}
		bool IHelpProvider.PerformHelpAction(HelpInfo info)
		{
			if (this.activeState is IHelpProvider)
			{
				IHelpProvider stateHelper = this.activeState as IHelpProvider;
				return stateHelper.PerformHelpAction(info);
			}
			else
			{
				return this.DefaultPerformHelpAction(info);
			}
		}

		int IMouseInput.X
		{
			get { return this.inputMouseX; }
		}
		int IMouseInput.Y
		{
			get { return this.inputMouseY; }
		}
		int IMouseInput.Wheel
		{
			get { return this.inputMouseWheel; }
		}
		bool IMouseInput.this[MouseButton btn]
		{
			get { return (this.inputMouseButtons & (1 << (int)btn)) != 0; }
		}
		event EventHandler<MouseButtonEventArgs> IMouseInput.ButtonUp
		{
			add { this.inputMouseUp += value; }
			remove { this.inputMouseUp -= value; }
		}
		event EventHandler<MouseButtonEventArgs> IMouseInput.ButtonDown
		{
			add { this.inputMouseDown += value; }
			remove { this.inputMouseDown -= value; }
		}
		event EventHandler<MouseMoveEventArgs> IMouseInput.Move
		{
			add { this.inputMouseMove += value; }
			remove { this.inputMouseMove -= value; }
		}
		event EventHandler<MouseWheelEventArgs> IMouseInput.WheelChanged
		{
			add { this.inputMouseWheelChanged += value; }
			remove { this.inputMouseWheelChanged -= value; }
		}

		bool IKeyboardInput.KeyRepeat
		{
			get { return this.inputKeyRepeat; }
			set { this.inputKeyRepeat = value; }
		}
		bool IKeyboardInput.this[Key key]
		{
			get { return this.inputKeyPressed[(int)key]; }
		}
		event EventHandler<KeyboardKeyEventArgs> IKeyboardInput.KeyUp
		{
			add { this.inputKeyUp += value; }
			remove { this.inputKeyUp -= value; }
		}
		event EventHandler<KeyboardKeyEventArgs> IKeyboardInput.KeyDown
		{
			add { this.inputKeyDown += value; }
			remove { this.inputKeyDown -= value; }
		}
	}
}

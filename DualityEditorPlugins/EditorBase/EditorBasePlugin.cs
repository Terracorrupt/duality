﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using WeifenLuo.WinFormsUI.Docking;

using Duality;
using Duality.Components;
using Duality.Components.Renderers;
using Duality.Components.Physics;
using Duality.Resources;
using TextRenderer = Duality.Components.Renderers.TextRenderer;

using DualityEditor;
using DualityEditor.Forms;
using DualityEditor.EditorRes;
using DualityEditor.CorePluginInterface;

using EditorBase.PluginRes;


namespace EditorBase
{
	public class EditorBasePlugin : EditorPlugin
	{
		private	static	EditorBasePlugin	instance	= null;
		internal static EditorBasePlugin Instance
		{
			get { return instance; }
		}


		private	ProjectFolderView		projectView		= null;
		private	SceneView				sceneView		= null;
		private	LogView					logView			= null;
		private	List<ObjectInspector>	objViews		= new List<ObjectInspector>();
		private	List<CamView>			camViews		= new List<CamView>();
		private	bool					isLoading		= false;

		private	ToolStripMenuItem	menuItemProjectView	= null;
		private	ToolStripMenuItem	menuItemSceneView	= null;
		private	ToolStripMenuItem	menuItemObjView		= null;
		private	ToolStripMenuItem	menuItemCamView		= null;
		private	ToolStripMenuItem	menuItemLogView		= null;
		private	ToolStripMenuItem	menuItemAppData		= null;
		private	ToolStripMenuItem	menuItemUserData	= null;


		public override string Id
		{
			get { return "EditorBase"; }
		}
		public IEnumerable<ObjectInspector>	ObjViews
		{
			get { return this.objViews; }
		}


		public EditorBasePlugin()
		{
			instance = this;
		}
		protected override IDockContent DeserializeDockContent(Type dockContentType)
		{
			this.isLoading = true;
			IDockContent result;
			if (dockContentType == typeof(CamView))
				result = this.RequestCamView();
			else if (dockContentType == typeof(ProjectFolderView))
				result = this.RequestProjectView();
			else if (dockContentType == typeof(SceneView))
				result = this.RequestSceneView();
			else if (dockContentType == typeof(ObjectInspector))
				result = this.RequestObjView();
			else if (dockContentType == typeof(LogView))
				result = this.RequestLogView();
			else
				result = base.DeserializeDockContent(dockContentType);
			this.isLoading = false;
			return result;
		}

		protected override void SaveUserData(System.Xml.XmlElement node)
		{
			System.Xml.XmlDocument doc = node.OwnerDocument;
			for (int i = 0; i < this.camViews.Count; i++)
			{
				System.Xml.XmlElement camViewElem = doc.CreateElement("CamView_" + i);
				node.AppendChild(camViewElem);
				this.camViews[i].SaveUserData(camViewElem);
			}
			for (int i = 0; i < this.objViews.Count; i++)
			{
				System.Xml.XmlElement objViewElem = doc.CreateElement("ObjInspector_" + i);
				node.AppendChild(objViewElem);
				this.objViews[i].SaveUserData(objViewElem);
			}
			if (this.logView != null)
			{
				System.Xml.XmlElement logViewElem = doc.CreateElement("LogView_0");
				node.AppendChild(logViewElem);
				this.logView.SaveUserData(logViewElem);
			}
		}
		protected override void LoadUserData(System.Xml.XmlElement node)
		{
			this.isLoading = true;
			for (int i = 0; i < this.camViews.Count; i++)
			{
				System.Xml.XmlNodeList camViewElemQuery = node.GetElementsByTagName("CamView_" + i);
				if (camViewElemQuery.Count == 0) continue;

				System.Xml.XmlElement camViewElem = camViewElemQuery[0] as System.Xml.XmlElement;
				this.camViews[i].LoadUserData(camViewElem);
			}
			for (int i = 0; i < this.objViews.Count; i++)
			{
				System.Xml.XmlNodeList objViewElemQuery = node.GetElementsByTagName("ObjInspector_" + i);
				if (objViewElemQuery.Count == 0) continue;

				System.Xml.XmlElement objViewElem = objViewElemQuery[0] as System.Xml.XmlElement;
				this.objViews[i].LoadUserData(objViewElem);
			}
			if (this.logView != null)
			{
				System.Xml.XmlNodeList logViewElemQuery = node.GetElementsByTagName("LogView_0");
				if (logViewElemQuery.Count > 0)
				{
					System.Xml.XmlElement logViewElem = logViewElemQuery[0] as System.Xml.XmlElement;
					this.logView.LoadUserData(logViewElem);
				}
			}
			this.isLoading = false;
		}

		protected override void LoadPlugin()
		{
			base.LoadPlugin();

			// Register core resource lookups
			CorePluginRegistry.RegisterTypeImage(typeof(DrawTechnique), EditorBaseRes.IconResDrawTechnique, CorePluginRegistry.ImageContext_Icon);
			CorePluginRegistry.RegisterTypeImage(typeof(FragmentShader), EditorBaseRes.IconResFragmentShader, CorePluginRegistry.ImageContext_Icon);
			CorePluginRegistry.RegisterTypeImage(typeof(Material), EditorBaseRes.IconResMaterial, CorePluginRegistry.ImageContext_Icon);
			CorePluginRegistry.RegisterTypeImage(typeof(Pixmap), EditorBaseRes.IconResPixmap, CorePluginRegistry.ImageContext_Icon);
			CorePluginRegistry.RegisterTypeImage(typeof(Prefab), EditorBaseRes.IconResPrefabFull, CorePluginRegistry.ImageContext_Icon + "_Full");
			CorePluginRegistry.RegisterTypeImage(typeof(Prefab), EditorBaseRes.IconResPrefabEmpty, CorePluginRegistry.ImageContext_Icon);
			CorePluginRegistry.RegisterTypeImage(typeof(RenderTarget), EditorBaseRes.IconResRenderTarget, CorePluginRegistry.ImageContext_Icon);
			CorePluginRegistry.RegisterTypeImage(typeof(ShaderProgram), EditorBaseRes.IconResShaderProgram, CorePluginRegistry.ImageContext_Icon);
			CorePluginRegistry.RegisterTypeImage(typeof(Texture), EditorBaseRes.IconResTexture, CorePluginRegistry.ImageContext_Icon);
			CorePluginRegistry.RegisterTypeImage(typeof(VertexShader), EditorBaseRes.IconResVertexShader, CorePluginRegistry.ImageContext_Icon);
			CorePluginRegistry.RegisterTypeImage(typeof(Scene), EditorBaseRes.IconResScene, CorePluginRegistry.ImageContext_Icon);
			CorePluginRegistry.RegisterTypeImage(typeof(AudioData), EditorBaseRes.IconResAudioData, CorePluginRegistry.ImageContext_Icon);
			CorePluginRegistry.RegisterTypeImage(typeof(Sound), EditorBaseRes.IconResSound, CorePluginRegistry.ImageContext_Icon);
			CorePluginRegistry.RegisterTypeImage(typeof(Font), EditorBaseRes.IconResFont, CorePluginRegistry.ImageContext_Icon);

			CorePluginRegistry.RegisterTypeImage(typeof(GameObject), EditorBaseRes.IconGameObj, CorePluginRegistry.ImageContext_Icon);
			CorePluginRegistry.RegisterTypeImage(typeof(GameObject), EditorBaseRes.IconGameObjLink, CorePluginRegistry.ImageContext_Icon + "_Link");
			CorePluginRegistry.RegisterTypeImage(typeof(GameObject), EditorBaseRes.IconGameObjLinkBroken, CorePluginRegistry.ImageContext_Icon + "_Link_Broken");
			CorePluginRegistry.RegisterTypeImage(typeof(Component), EditorBaseRes.IconCmpUnknown, CorePluginRegistry.ImageContext_Icon);
			CorePluginRegistry.RegisterTypeImage(typeof(SpriteRenderer), EditorBaseRes.IconCmpSpriteRenderer, CorePluginRegistry.ImageContext_Icon);
			CorePluginRegistry.RegisterTypeImage(typeof(AnimSpriteRenderer), EditorBaseRes.IconCmpSpriteRenderer, CorePluginRegistry.ImageContext_Icon);
			CorePluginRegistry.RegisterTypeImage(typeof(TextRenderer), EditorBaseRes.IconResFont, CorePluginRegistry.ImageContext_Icon);
			CorePluginRegistry.RegisterTypeImage(typeof(Transform), EditorBaseRes.IconCmpTransform, CorePluginRegistry.ImageContext_Icon);
			CorePluginRegistry.RegisterTypeImage(typeof(Camera), EditorBaseRes.IconCmpCamera, CorePluginRegistry.ImageContext_Icon);
			CorePluginRegistry.RegisterTypeImage(typeof(SoundEmitter), EditorBaseRes.IconResSound, CorePluginRegistry.ImageContext_Icon);
			CorePluginRegistry.RegisterTypeImage(typeof(SoundListener), EditorBaseRes.IconCmpSoundListener, CorePluginRegistry.ImageContext_Icon);
			CorePluginRegistry.RegisterTypeImage(typeof(RigidBody), EditorBaseRes.IconCmpRectCollider, CorePluginRegistry.ImageContext_Icon);

			CorePluginRegistry.RegisterTypeCategory(typeof(Transform), "", CorePluginRegistry.CategoryContext_General);
			CorePluginRegistry.RegisterTypeCategory(typeof(SpriteRenderer), GeneralRes.Category_Graphics, CorePluginRegistry.CategoryContext_General);
			CorePluginRegistry.RegisterTypeCategory(typeof(AnimSpriteRenderer), GeneralRes.Category_Graphics, CorePluginRegistry.CategoryContext_General);
			CorePluginRegistry.RegisterTypeCategory(typeof(TextRenderer), GeneralRes.Category_Graphics, CorePluginRegistry.CategoryContext_General);
			CorePluginRegistry.RegisterTypeCategory(typeof(Camera), GeneralRes.Category_Graphics, CorePluginRegistry.CategoryContext_General);
			CorePluginRegistry.RegisterTypeCategory(typeof(SoundEmitter), GeneralRes.Category_Sound, CorePluginRegistry.CategoryContext_General);
			CorePluginRegistry.RegisterTypeCategory(typeof(SoundListener), GeneralRes.Category_Sound, CorePluginRegistry.CategoryContext_General);
			CorePluginRegistry.RegisterTypeCategory(typeof(RigidBody), GeneralRes.Category_Physics, CorePluginRegistry.CategoryContext_General);

			CorePluginRegistry.RegisterTypeCategory(typeof(Scene), "", CorePluginRegistry.CategoryContext_General);
			CorePluginRegistry.RegisterTypeCategory(typeof(Prefab), "", CorePluginRegistry.CategoryContext_General);
			CorePluginRegistry.RegisterTypeCategory(typeof(Pixmap), GeneralRes.Category_Graphics, CorePluginRegistry.CategoryContext_General);
			CorePluginRegistry.RegisterTypeCategory(typeof(Texture), GeneralRes.Category_Graphics, CorePluginRegistry.CategoryContext_General);
			CorePluginRegistry.RegisterTypeCategory(typeof(Material), GeneralRes.Category_Graphics, CorePluginRegistry.CategoryContext_General);
			CorePluginRegistry.RegisterTypeCategory(typeof(Font), GeneralRes.Category_Graphics, CorePluginRegistry.CategoryContext_General);
			CorePluginRegistry.RegisterTypeCategory(typeof(RenderTarget), GeneralRes.Category_Graphics, CorePluginRegistry.CategoryContext_General);
			CorePluginRegistry.RegisterTypeCategory(typeof(DrawTechnique), GeneralRes.Category_Graphics, CorePluginRegistry.CategoryContext_General);
			CorePluginRegistry.RegisterTypeCategory(typeof(ShaderProgram), GeneralRes.Category_Graphics, CorePluginRegistry.CategoryContext_General);
			CorePluginRegistry.RegisterTypeCategory(typeof(VertexShader), GeneralRes.Category_Graphics, CorePluginRegistry.CategoryContext_General);
			CorePluginRegistry.RegisterTypeCategory(typeof(FragmentShader), GeneralRes.Category_Graphics, CorePluginRegistry.CategoryContext_General);
			CorePluginRegistry.RegisterTypeCategory(typeof(AudioData), GeneralRes.Category_Sound, CorePluginRegistry.CategoryContext_General);
			CorePluginRegistry.RegisterTypeCategory(typeof(Sound), GeneralRes.Category_Sound, CorePluginRegistry.CategoryContext_General);

			// Register conversion actions
			CorePluginRegistry.RegisterEditorAction(new EditorAction<Pixmap>				(EditorBaseRes.ActionName_CreateTexture,		EditorBaseRes.IconResTexture,		p => Texture.CreateFromPixmap(p),	EditorBaseRes.ActionDesc_CreateTexture),		CorePluginRegistry.ActionContext_ContextMenu);
			CorePluginRegistry.RegisterEditorAction(new EditorAction<Texture>				(EditorBaseRes.ActionName_CreateMaterial,		EditorBaseRes.IconResMaterial,		t => Material.CreateFromTexture(t), EditorBaseRes.ActionDesc_CreateMaterial),		CorePluginRegistry.ActionContext_ContextMenu);
			CorePluginRegistry.RegisterEditorAction(new EditorAction<AudioData>			(EditorBaseRes.ActionName_CreateSound,			EditorBaseRes.IconResSound,			a => Sound.CreateFromAudioData(a),	EditorBaseRes.ActionDesc_CreateSound),			CorePluginRegistry.ActionContext_ContextMenu);
			CorePluginRegistry.RegisterEditorAction(new EditorGroupAction<AbstractShader>	(EditorBaseRes.ActionName_CreateShaderProgram,	EditorBaseRes.IconResShaderProgram, this.ActionShaderCreateProgram,		EditorBaseRes.ActionDesc_CreateShaderProgram),	CorePluginRegistry.ActionContext_ContextMenu);

			// Register open actions
			CorePluginRegistry.RegisterEditorAction(new EditorAction<Pixmap>			(null, null, this.ActionPixmapOpenRes,			EditorBaseRes.ActionDesc_OpenResourceExternal), CorePluginRegistry.ActionContext_OpenRes);
			CorePluginRegistry.RegisterEditorAction(new EditorAction<AudioData>		(null, null, this.ActionAudioDataOpenRes,		EditorBaseRes.ActionDesc_OpenResourceExternal), CorePluginRegistry.ActionContext_OpenRes);
			CorePluginRegistry.RegisterEditorAction(new EditorAction<AbstractShader>	(null, null, this.ActionAbstractShaderOpenRes,	EditorBaseRes.ActionDesc_OpenResourceExternal), CorePluginRegistry.ActionContext_OpenRes);
			CorePluginRegistry.RegisterEditorAction(new EditorAction<Prefab>			(null, null, this.ActionPrefabOpenRes,			EditorBaseRes.ActionDesc_InstantiatePrefab),	CorePluginRegistry.ActionContext_OpenRes);
			CorePluginRegistry.RegisterEditorAction(new EditorAction<Scene>			(null, null, this.ActionSceneOpenRes,			EditorBaseRes.ActionDesc_OpenScene),			CorePluginRegistry.ActionContext_OpenRes);
			CorePluginRegistry.RegisterEditorAction(new EditorAction<GameObject>		(null, null, this.ActionGameObjectOpenRes,		EditorBaseRes.ActionDesc_FocusGameObject,		g => g.Transform != null),			CorePluginRegistry.ActionContext_OpenRes);
			CorePluginRegistry.RegisterEditorAction(new EditorAction<Component>		(null, null, this.ActionComponentOpenRes,		EditorBaseRes.ActionDesc_FocusGameObject,		c => c.GameObj.Transform != null),	CorePluginRegistry.ActionContext_OpenRes);

			// Register data converters
			CorePluginRegistry.RegisterDataConverter<GameObject>(new DataConverters.GameObjFromPrefab());
			CorePluginRegistry.RegisterDataConverter<GameObject>(new DataConverters.GameObjFromComponents());
			CorePluginRegistry.RegisterDataConverter<Component>(new DataConverters.ComponentFromSound());
			CorePluginRegistry.RegisterDataConverter<Component>(new DataConverters.ComponentFromMaterial());
			CorePluginRegistry.RegisterDataConverter<Component>(new DataConverters.ComponentFromFont());
			CorePluginRegistry.RegisterDataConverter<BatchInfo>(new DataConverters.BatchInfoFromMaterial());
			CorePluginRegistry.RegisterDataConverter<Material>(new DataConverters.MaterialFromBatchInfo());
			CorePluginRegistry.RegisterDataConverter<Material>(new DataConverters.MaterialFromTexture());
			CorePluginRegistry.RegisterDataConverter<Texture>(new DataConverters.TextureFromMaterial());
			CorePluginRegistry.RegisterDataConverter<Texture>(new DataConverters.TextureFromPixmap());
			CorePluginRegistry.RegisterDataConverter<Pixmap>(new DataConverters.PixmapFromTexture());
			CorePluginRegistry.RegisterDataConverter<Sound>(new DataConverters.SoundFromAudioData());
			CorePluginRegistry.RegisterDataConverter<AudioData>(new DataConverters.AudioDataFromSound());
			CorePluginRegistry.RegisterDataConverter<Prefab>(new DataConverters.PrefabFromGameObject());

			// Register preview generators
			CorePluginRegistry.RegisterPreviewGenerator(new PreviewGenerators.PixmapPreviewGenerator());
			CorePluginRegistry.RegisterPreviewGenerator(new PreviewGenerators.AudioDataPreviewGenerator());
			CorePluginRegistry.RegisterPreviewGenerator(new PreviewGenerators.FontPreviewGenerator());

			// Register file importers
			CorePluginRegistry.RegisterFileImporter(new PixmapFileImporter());
			CorePluginRegistry.RegisterFileImporter(new AudioDataFileImporter());
			CorePluginRegistry.RegisterFileImporter(new ShaderFileImporter());
			CorePluginRegistry.RegisterFileImporter(new FontFileImporter());

			// Register PropertyEditor provider
			CorePluginRegistry.RegisterPropertyEditorProvider(new PropertyEditors.PropertyEditorProvider());
		}
		protected override void InitPlugin(MainForm main)
		{
			base.InitPlugin(main);

			// Request menus
			this.menuItemProjectView = main.RequestMenu(Path.Combine(GeneralRes.MenuName_View, EditorBaseRes.MenuItemName_ProjectView));
			this.menuItemSceneView = main.RequestMenu(Path.Combine(GeneralRes.MenuName_View, EditorBaseRes.MenuItemName_SceneView));
			this.menuItemObjView = main.RequestMenu(Path.Combine(GeneralRes.MenuName_View, EditorBaseRes.MenuItemName_ObjView));
			this.menuItemCamView = main.RequestMenu(Path.Combine(GeneralRes.MenuName_View, EditorBaseRes.MenuItemName_CamView));
			this.menuItemLogView = main.RequestMenu(Path.Combine(GeneralRes.MenuName_View, EditorBaseRes.MenuItemName_LogView));
			this.menuItemAppData = main.RequestMenu(Path.Combine(GeneralRes.MenuName_Settings, EditorBaseRes.MenuItemName_AppData));
			this.menuItemUserData = main.RequestMenu(Path.Combine(GeneralRes.MenuName_Settings, EditorBaseRes.MenuItemName_UserData));

			// Configure menus
			this.menuItemProjectView.Image = EditorBaseRes.IconProjectView.ToBitmap();
			this.menuItemSceneView.Image = EditorBaseRes.IconSceneView.ToBitmap();
			this.menuItemObjView.Image = EditorBaseRes.IconObjView.ToBitmap();
			this.menuItemCamView.Image = EditorBaseRes.IconEye.ToBitmap();
			this.menuItemLogView.Image = EditorBaseRes.IconLogView.ToBitmap();

			this.menuItemProjectView.Click += this.menuItemProjectView_Click;
			this.menuItemSceneView.Click += this.menuItemSceneView_Click;
			this.menuItemObjView.Click += this.menuItemObjView_Click;
			this.menuItemCamView.Click += this.menuItemCamView_Click;
			this.menuItemLogView.Click += this.menuItemLogView_Click;
			this.menuItemAppData.Click += this.menuItemAppData_Click;
			this.menuItemUserData.Click += this.menuItemUserData_Click;

			main.ResourceModified += this.main_ResourceModified;
			main.ObjectPropertyChanged += this.main_ObjectPropertyChanged;
		}
		
		public ProjectFolderView RequestProjectView()
		{
			if (this.projectView == null || this.projectView.IsDisposed)
			{
				this.projectView = new ProjectFolderView();
				this.projectView.FormClosed += delegate(object sender, FormClosedEventArgs e) { this.projectView = null; };
			}

			if (!this.isLoading)
			{
				this.projectView.Show(MainForm.Instance.MainDockPanel);
				if (this.projectView.Pane != null)
				{
					this.projectView.Pane.Activate();
					this.projectView.Focus();
				}
			}

			return this.projectView;
		}
		public SceneView RequestSceneView()
		{
			if (this.sceneView == null || this.sceneView.IsDisposed)
			{
				this.sceneView = new SceneView();
				this.sceneView.FormClosed += delegate(object sender, FormClosedEventArgs e) { this.sceneView = null; };
			}

			if (!this.isLoading)
			{
				this.sceneView.Show(MainForm.Instance.MainDockPanel);
				if (this.sceneView.Pane != null)
				{
					this.sceneView.Pane.Activate();
					this.sceneView.Focus();
				}
			}

			return this.sceneView;
		}
		public LogView RequestLogView()
		{
			if (this.logView == null || this.logView.IsDisposed)
			{
				this.logView = new LogView();
				this.logView.FormClosed += delegate(object sender, FormClosedEventArgs e) { this.logView = null; };
			}

			if (!this.isLoading)
			{
				this.logView.Show(MainForm.Instance.MainDockPanel);
				if (this.logView.Pane != null)
				{
					this.logView.Pane.Activate();
					this.logView.Focus();
				}
			}

			return this.logView;
		}
		public ObjectInspector RequestObjView(bool dontShow = false)
		{
			ObjectInspector objView = new ObjectInspector(this.objViews.Count);
			this.objViews.Add(objView);
			objView.FormClosed += delegate(object sender, FormClosedEventArgs e) { this.objViews.Remove(sender as ObjectInspector); };

			if (!this.isLoading && !dontShow)
			{
				objView.Show(MainForm.Instance.MainDockPanel);
				if (objView.Pane != null)
				{
					objView.Pane.Activate();
					objView.Focus();
				}
			}
			return objView;
		}
		public CamView RequestCamView()
		{
			CamView cam = new CamView(this.camViews.Count);
			this.camViews.Add(cam);
			cam.FormClosed += delegate(object sender, FormClosedEventArgs e) { this.camViews.Remove(sender as CamView); };

			if (!this.isLoading)
			{
				cam.Show(MainForm.Instance.MainDockPanel);
				if (cam.Pane != null)
				{
					cam.Pane.Activate();
					cam.Focus();
				}
			}
			return cam;
		}

		private void menuItemProjectView_Click(object sender, EventArgs e)
		{
			this.RequestProjectView();
		}
		private void menuItemSceneView_Click(object sender, EventArgs e)
		{
			this.RequestSceneView();
		}
		private void menuItemObjView_Click(object sender, EventArgs e)
		{
			ObjectInspector objView = this.RequestObjView();
		}
		private void menuItemCamView_Click(object sender, EventArgs e)
		{
			this.RequestCamView();
		}
		private void menuItemLogView_Click(object sender, EventArgs e)
		{
			this.RequestLogView();
		}
		private void menuItemAppData_Click(object sender, EventArgs e)
		{
			MainForm.Instance.Select(this, new ObjectSelection(DualityApp.AppData));
		}
		private void menuItemUserData_Click(object sender, EventArgs e)
		{
			MainForm.Instance.Select(this, new ObjectSelection(DualityApp.UserData));
		}

		private void ActionShaderCreateProgram(IEnumerable<AbstractShader> shaderEnum)
		{
			List<VertexShader> vertexShaders = shaderEnum.OfType<VertexShader>().ToList();
			List<FragmentShader> fragmentShaders = shaderEnum.OfType<FragmentShader>().ToList();

			if (vertexShaders.Count == 1 && fragmentShaders.Count >= 1)
				foreach (FragmentShader frag in fragmentShaders) this.ActionShaderCreateProgram_Create(frag, vertexShaders[0]);
			else if (fragmentShaders.Count == 1 && vertexShaders.Count >= 1)
				foreach (VertexShader vert in vertexShaders) this.ActionShaderCreateProgram_Create(fragmentShaders[0], vert);
			else
			{
				for (int i = 0; i < MathF.Max(vertexShaders.Count, fragmentShaders.Count); i++)
				{
					this.ActionShaderCreateProgram_Create(
						i < fragmentShaders.Count ? fragmentShaders[i] : null, 
						i < vertexShaders.Count ? vertexShaders[i] : null);
				}
			}
		}
		private void ActionShaderCreateProgram_Create(FragmentShader frag, VertexShader vert)
		{
			AbstractShader refShader = (vert != null) ? (AbstractShader)vert : (AbstractShader)frag;

			string nameTemp = refShader.Name;
			string dirTemp = Path.GetDirectoryName(refShader.Path);
			if (nameTemp.Contains("Shader"))
				nameTemp = nameTemp.Replace("Shader", "Program");
			else if (nameTemp.Contains("Shader"))
				nameTemp = nameTemp.Replace("shader", "program");
			else
				nameTemp += "Program";

			string programPath = PathHelper.GetFreePath(Path.Combine(dirTemp, nameTemp), ShaderProgram.FileExt);
			ShaderProgram program = new ShaderProgram(vert, frag);
			program.Save(programPath);
		}

		private void ActionPixmapOpenRes(Pixmap pixmap)
		{
			if (pixmap == null) return;
			EditorHelper.OpenResourceSrcFile(pixmap, ".png", pixmap.SavePixelData);
		}
		private void ActionAudioDataOpenRes(AudioData audio)
		{
			if (audio == null) return;
			EditorHelper.OpenResourceSrcFile(audio, ".ogg", audio.SaveOggVorbisData);
		}
		private void ActionAbstractShaderOpenRes(AbstractShader shader)
		{
			if (shader == null) return;
			EditorHelper.OpenResourceSrcFile(shader, shader is FragmentShader ? ".frag" : ".vert", shader.SaveSource);
		}
		private void ActionPrefabOpenRes(Prefab prefab)
		{
			try
			{
				GameObject newObj = prefab.Instantiate();
				Duality.Resources.Scene.Current.RegisterObj(newObj);
				MainForm.Instance.Select(this, new ObjectSelection(newObj));
			}
			catch (Exception exception)
			{
				Log.Editor.WriteError("An error occured instanciating Prefab {1}: {0}", 
					Log.Exception(exception),
					prefab != null ? prefab.Path : "null");
			}
		}
		private void ActionSceneOpenRes(Scene scene)
		{
			string lastPath = Scene.CurrentPath;
			try
			{
				Scene.Current = scene;
			}
			catch (Exception exception)
			{
				Log.Editor.WriteError("An error occured while switching from Scene {1} to Scene {2}: {0}", 
					Log.Exception(exception),
					lastPath,
					scene != null ? scene.Path : "null");
			}
		}
		private void ActionGameObjectOpenRes(GameObject obj)
		{
			if (obj.Transform == null) return;
			foreach (CamView view in this.camViews)
				view.FocusOnObject(obj);
		}
		private void ActionComponentOpenRes(Component cmp)
		{
			GameObject obj = cmp.GameObj;
			if (obj == null) return;
			this.ActionGameObjectOpenRes(obj);
		}

		private void main_ResourceModified(object sender, ResourceEventArgs e)
		{
			if (e.IsResource) this.OnResourceModified(e.Content);
		}
		private void main_ObjectPropertyChanged(object sender, ObjectPropertyChangedEventArgs e)
		{
			if (e.Objects.ResourceCount > 0)
			{
				foreach (var r in e.Objects.Resources)
					this.OnResourceModified(r);
			}
		}
		private void OnResourceModified(ContentRef<Resource> resRef)
		{
			// If a font has been modified, update all TextRenderers
			if (resRef.Is<Font>())
			{
				foreach (Duality.Components.Renderers.TextRenderer r in Scene.Current.AllObjects.GetComponents<Duality.Components.Renderers.TextRenderer>())
				{
					r.Text.ApplySource();
					r.UpdateMetrics();
				}
			}
			// If its a Pixmap, reload all associated Textures
			else if (resRef.Is<Pixmap>())
			{
				foreach (ContentRef<Texture> tex in ContentProvider.GetAvailContent<Texture>())
				{
					if (!tex.IsAvailable) continue;
					if (tex.Res.BasePixmap.Res == resRef.Res)
					{
						tex.Res.ReloadData();
					}
				}
			}
			// If its a Texture, update all associated RenderTargets
			else if (resRef.Is<Texture>())
			{
				foreach (ContentRef<RenderTarget> rt in ContentProvider.GetAvailContent<RenderTarget>())
				{
					if (!rt.IsAvailable) continue;
					if (rt.Res.Targets.Any(target => target.Res == resRef.Res as Texture))
					{
						rt.Res.SetupOpenGLRes();
					}
				}
			}
			// If its some kind of shader, update all associated ShaderPrograms
			else if (resRef.Is<AbstractShader>())
			{
				foreach (ContentRef<ShaderProgram> sp in ContentProvider.GetAvailContent<ShaderProgram>())
				{
					if (!sp.IsAvailable) continue;
					if (sp.Res.Fragment.Res == resRef.Res as FragmentShader ||
						sp.Res.Vertex.Res == resRef.Res as VertexShader)
					{
						bool wasCompiled = sp.Res.Compiled;
						sp.Res.AttachShaders();
						if (wasCompiled) sp.Res.Compile();
					}
				}
			}
		}

		public static void SortToolStripTypeItems(ToolStripItemCollection items)
		{
			var menuSubItems = items.Cast<ToolStripItem>().ToArray();
			SortToolStripTypeItems(menuSubItems);
			items.Clear();
			items.AddRange(menuSubItems);
		}
		public static void SortToolStripTypeItems(IList<ToolStripItem> items)
		{
			items.StableSort(delegate(ToolStripItem item1, ToolStripItem item2)
			{
				ToolStripMenuItem menuItem1 = item1 as ToolStripMenuItem;
				ToolStripMenuItem menuItem2 = item2 as ToolStripMenuItem;

				System.Reflection.Assembly assembly1 = item1.Tag is Type ? (item1.Tag as Type).Assembly : item1.Tag as System.Reflection.Assembly;
				System.Reflection.Assembly assembly2 = item2.Tag is Type ? (item2.Tag as Type).Assembly : item2.Tag as System.Reflection.Assembly;
				int score1 = assembly1 == typeof(DualityApp).Assembly ? 1 : 0;
				int score2 = assembly2 == typeof(DualityApp).Assembly ? 1 : 0;
				int result = score2 - score1;
				if (result != 0) return result;

				result = 
					(menuItem1 != null ? Math.Sign(menuItem1.DropDownItems.Count) : 0) - 
					(menuItem2 != null ? Math.Sign(menuItem2.DropDownItems.Count) : 0);
				if (result != 0) return result;

				result = System.String.CompareOrdinal(item1.Text, item2.Text);
				return result;
			});

			foreach (ToolStripItem item in items)
			{
				ToolStripMenuItem menuItem = item as ToolStripMenuItem;
				if (menuItem != null && menuItem.HasDropDownItems) SortToolStripTypeItems(menuItem.DropDownItems);
			}
		}
	}
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

using OpenTK;
using WeifenLuo.WinFormsUI.Docking;

using Duality;
using Duality.Components;
using Duality.Components.Renderers;
using Duality.ColorFormat;
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

		protected override void SaveUserData(System.Xml.XmlDocument doc, System.Xml.XmlElement node)
		{
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
			CorePluginHelper.RegisterTypeImage(typeof(DrawTechnique), EditorBaseRes.IconResDrawTechnique, CorePluginHelper.ImageContext_Icon);
			CorePluginHelper.RegisterTypeImage(typeof(FragmentShader), EditorBaseRes.IconResFragmentShader, CorePluginHelper.ImageContext_Icon);
			CorePluginHelper.RegisterTypeImage(typeof(Material), EditorBaseRes.IconResMaterial, CorePluginHelper.ImageContext_Icon);
			CorePluginHelper.RegisterTypeImage(typeof(Pixmap), EditorBaseRes.IconResPixmap, CorePluginHelper.ImageContext_Icon);
			CorePluginHelper.RegisterTypeImage(typeof(Prefab), EditorBaseRes.IconResPrefabFull, CorePluginHelper.ImageContext_Icon + "_Full");
			CorePluginHelper.RegisterTypeImage(typeof(Prefab), EditorBaseRes.IconResPrefabEmpty, CorePluginHelper.ImageContext_Icon);
			CorePluginHelper.RegisterTypeImage(typeof(RenderTarget), EditorBaseRes.IconResRenderTarget, CorePluginHelper.ImageContext_Icon);
			CorePluginHelper.RegisterTypeImage(typeof(ShaderProgram), EditorBaseRes.IconResShaderProgram, CorePluginHelper.ImageContext_Icon);
			CorePluginHelper.RegisterTypeImage(typeof(Texture), EditorBaseRes.IconResTexture, CorePluginHelper.ImageContext_Icon);
			CorePluginHelper.RegisterTypeImage(typeof(VertexShader), EditorBaseRes.IconResVertexShader, CorePluginHelper.ImageContext_Icon);
			CorePluginHelper.RegisterTypeImage(typeof(Scene), EditorBaseRes.IconResScene, CorePluginHelper.ImageContext_Icon);
			CorePluginHelper.RegisterTypeImage(typeof(AudioData), EditorBaseRes.IconResAudioData, CorePluginHelper.ImageContext_Icon);
			CorePluginHelper.RegisterTypeImage(typeof(Sound), EditorBaseRes.IconResSound, CorePluginHelper.ImageContext_Icon);
			CorePluginHelper.RegisterTypeImage(typeof(Font), EditorBaseRes.IconResFont, CorePluginHelper.ImageContext_Icon);

			CorePluginHelper.RegisterTypeImage(typeof(GameObject), EditorBaseRes.IconGameObj, CorePluginHelper.ImageContext_Icon);
			CorePluginHelper.RegisterTypeImage(typeof(GameObject), EditorBaseRes.IconGameObjLink, CorePluginHelper.ImageContext_Icon + "_Link");
			CorePluginHelper.RegisterTypeImage(typeof(GameObject), EditorBaseRes.IconGameObjLinkBroken, CorePluginHelper.ImageContext_Icon + "_Link_Broken");
			CorePluginHelper.RegisterTypeImage(typeof(Component), EditorBaseRes.IconCmpUnknown, CorePluginHelper.ImageContext_Icon);
			CorePluginHelper.RegisterTypeImage(typeof(SpriteRenderer), EditorBaseRes.IconCmpSpriteRenderer, CorePluginHelper.ImageContext_Icon);
			CorePluginHelper.RegisterTypeImage(typeof(AnimSpriteRenderer), EditorBaseRes.IconCmpSpriteRenderer, CorePluginHelper.ImageContext_Icon);
			CorePluginHelper.RegisterTypeImage(typeof(TextRenderer), EditorBaseRes.IconResFont, CorePluginHelper.ImageContext_Icon);
			CorePluginHelper.RegisterTypeImage(typeof(Transform), EditorBaseRes.IconCmpTransform, CorePluginHelper.ImageContext_Icon);
			CorePluginHelper.RegisterTypeImage(typeof(Camera), EditorBaseRes.IconCmpCamera, CorePluginHelper.ImageContext_Icon);
			CorePluginHelper.RegisterTypeImage(typeof(SoundEmitter), EditorBaseRes.IconResSound, CorePluginHelper.ImageContext_Icon);
			CorePluginHelper.RegisterTypeImage(typeof(SoundListener), EditorBaseRes.IconCmpSoundListener, CorePluginHelper.ImageContext_Icon);
			CorePluginHelper.RegisterTypeImage(typeof(Collider), EditorBaseRes.IconCmpRectCollider, CorePluginHelper.ImageContext_Icon);

			CorePluginHelper.RegisterTypeCategory(typeof(Transform), "", CorePluginHelper.CategoryContext_General);
			CorePluginHelper.RegisterTypeCategory(typeof(SpriteRenderer), GeneralRes.Category_Graphics, CorePluginHelper.CategoryContext_General);
			CorePluginHelper.RegisterTypeCategory(typeof(AnimSpriteRenderer), GeneralRes.Category_Graphics, CorePluginHelper.CategoryContext_General);
			CorePluginHelper.RegisterTypeCategory(typeof(TextRenderer), GeneralRes.Category_Graphics, CorePluginHelper.CategoryContext_General);
			CorePluginHelper.RegisterTypeCategory(typeof(Camera), GeneralRes.Category_Graphics, CorePluginHelper.CategoryContext_General);
			CorePluginHelper.RegisterTypeCategory(typeof(SoundEmitter), GeneralRes.Category_Sound, CorePluginHelper.CategoryContext_General);
			CorePluginHelper.RegisterTypeCategory(typeof(SoundListener), GeneralRes.Category_Sound, CorePluginHelper.CategoryContext_General);
			CorePluginHelper.RegisterTypeCategory(typeof(Collider), GeneralRes.Category_Physics, CorePluginHelper.CategoryContext_General);

			CorePluginHelper.RegisterTypeCategory(typeof(Scene), "", CorePluginHelper.CategoryContext_General);
			CorePluginHelper.RegisterTypeCategory(typeof(Prefab), "", CorePluginHelper.CategoryContext_General);
			CorePluginHelper.RegisterTypeCategory(typeof(Pixmap), GeneralRes.Category_Graphics, CorePluginHelper.CategoryContext_General);
			CorePluginHelper.RegisterTypeCategory(typeof(Texture), GeneralRes.Category_Graphics, CorePluginHelper.CategoryContext_General);
			CorePluginHelper.RegisterTypeCategory(typeof(Material), GeneralRes.Category_Graphics, CorePluginHelper.CategoryContext_General);
			CorePluginHelper.RegisterTypeCategory(typeof(Font), GeneralRes.Category_Graphics, CorePluginHelper.CategoryContext_General);
			CorePluginHelper.RegisterTypeCategory(typeof(RenderTarget), GeneralRes.Category_Graphics, CorePluginHelper.CategoryContext_General);
			CorePluginHelper.RegisterTypeCategory(typeof(DrawTechnique), GeneralRes.Category_Graphics, CorePluginHelper.CategoryContext_General);
			CorePluginHelper.RegisterTypeCategory(typeof(ShaderProgram), GeneralRes.Category_Graphics, CorePluginHelper.CategoryContext_General);
			CorePluginHelper.RegisterTypeCategory(typeof(VertexShader), GeneralRes.Category_Graphics, CorePluginHelper.CategoryContext_General);
			CorePluginHelper.RegisterTypeCategory(typeof(FragmentShader), GeneralRes.Category_Graphics, CorePluginHelper.CategoryContext_General);
			CorePluginHelper.RegisterTypeCategory(typeof(AudioData), GeneralRes.Category_Sound, CorePluginHelper.CategoryContext_General);
			CorePluginHelper.RegisterTypeCategory(typeof(Sound), GeneralRes.Category_Sound, CorePluginHelper.CategoryContext_General);

			// Register conversion actions
			CorePluginHelper.RegisterEditorAction(new EditorAction<Pixmap>				(EditorBaseRes.ActionName_CreateTexture,		EditorBaseRes.IconResTexture,		p => Texture.CreateFromPixmap(p),	EditorBaseRes.ActionDesc_CreateTexture),		CorePluginHelper.ActionContext_ContextMenu);
			CorePluginHelper.RegisterEditorAction(new EditorAction<Texture>				(EditorBaseRes.ActionName_CreateMaterial,		EditorBaseRes.IconResMaterial,		t => Material.CreateFromTexture(t), EditorBaseRes.ActionDesc_CreateMaterial),		CorePluginHelper.ActionContext_ContextMenu);
			CorePluginHelper.RegisterEditorAction(new EditorAction<AudioData>			(EditorBaseRes.ActionName_CreateSound,			EditorBaseRes.IconResSound,			a => Sound.CreateFromAudioData(a),	EditorBaseRes.ActionDesc_CreateSound),			CorePluginHelper.ActionContext_ContextMenu);
			CorePluginHelper.RegisterEditorAction(new EditorGroupAction<AbstractShader>	(EditorBaseRes.ActionName_CreateShaderProgram,	EditorBaseRes.IconResShaderProgram, this.ActionShaderCreateProgram,		EditorBaseRes.ActionDesc_CreateShaderProgram),	CorePluginHelper.ActionContext_ContextMenu);

			// Register open actions
			CorePluginHelper.RegisterEditorAction(new EditorAction<Pixmap>			(null, null, this.ActionPixmapOpenRes,			EditorBaseRes.ActionDesc_OpenResourceExternal), CorePluginHelper.ActionContext_OpenRes);
			CorePluginHelper.RegisterEditorAction(new EditorAction<AudioData>		(null, null, this.ActionAudioDataOpenRes,		EditorBaseRes.ActionDesc_OpenResourceExternal), CorePluginHelper.ActionContext_OpenRes);
			CorePluginHelper.RegisterEditorAction(new EditorAction<AbstractShader>	(null, null, this.ActionAbstractShaderOpenRes,	EditorBaseRes.ActionDesc_OpenResourceExternal), CorePluginHelper.ActionContext_OpenRes);
			CorePluginHelper.RegisterEditorAction(new EditorAction<Prefab>			(null, null, this.ActionPrefabOpenRes,			EditorBaseRes.ActionDesc_InstantiatePrefab),	CorePluginHelper.ActionContext_OpenRes);
			CorePluginHelper.RegisterEditorAction(new EditorAction<Scene>			(null, null, this.ActionSceneOpenRes,			EditorBaseRes.ActionDesc_OpenScene),			CorePluginHelper.ActionContext_OpenRes);
			CorePluginHelper.RegisterEditorAction(new EditorAction<GameObject>		(null, null, this.ActionGameObjectOpenRes,		EditorBaseRes.ActionDesc_FocusGameObject,		g => g.Transform != null),			CorePluginHelper.ActionContext_OpenRes);
			CorePluginHelper.RegisterEditorAction(new EditorAction<Component>		(null, null, this.ActionComponentOpenRes,		EditorBaseRes.ActionDesc_FocusGameObject,		c => c.GameObj.Transform != null),	CorePluginHelper.ActionContext_OpenRes);

			// Register data converters
			CorePluginHelper.RegisterDataConverter<GameObject>(new DataConverters.GameObjFromPrefab());
			CorePluginHelper.RegisterDataConverter<GameObject>(new DataConverters.GameObjFromComponents());
			CorePluginHelper.RegisterDataConverter<Component>(new DataConverters.ComponentFromSound());
			CorePluginHelper.RegisterDataConverter<Component>(new DataConverters.ComponentFromMaterial());
			CorePluginHelper.RegisterDataConverter<Component>(new DataConverters.ComponentFromFont());
			CorePluginHelper.RegisterDataConverter<BatchInfo>(new DataConverters.BatchInfoFromMaterial());
			CorePluginHelper.RegisterDataConverter<Material>(new DataConverters.MaterialFromBatchInfo());
			CorePluginHelper.RegisterDataConverter<Material>(new DataConverters.MaterialFromTexture());
			CorePluginHelper.RegisterDataConverter<Texture>(new DataConverters.TextureFromMaterial());
			CorePluginHelper.RegisterDataConverter<Texture>(new DataConverters.TextureFromPixmap());
			CorePluginHelper.RegisterDataConverter<Pixmap>(new DataConverters.PixmapFromTexture());
			CorePluginHelper.RegisterDataConverter<Sound>(new DataConverters.SoundFromAudioData());
			CorePluginHelper.RegisterDataConverter<AudioData>(new DataConverters.AudioDataFromSound());
			CorePluginHelper.RegisterDataConverter<Prefab>(new DataConverters.PrefabFromGameObject());

			// Register preview generators
			CorePluginHelper.RegisterPreviewGenerator(new PreviewGenerators.PixmapPreviewGenerator());
			CorePluginHelper.RegisterPreviewGenerator(new PreviewGenerators.AudioDataPreviewGenerator());
			CorePluginHelper.RegisterPreviewGenerator(new PreviewGenerators.FontPreviewGenerator());

			// Register PropertyEditor provider
			CorePluginHelper.RegisterPropertyEditorProvider(new PropertyEditors.PropertyEditorProvider());
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

			this.menuItemProjectView.Click += new EventHandler(this.menuItemProjectView_Click);
			this.menuItemSceneView.Click += new EventHandler(this.menuItemSceneView_Click);
			this.menuItemObjView.Click += new EventHandler(this.menuItemObjView_Click);
			this.menuItemCamView.Click += new EventHandler(this.menuItemCamView_Click);
			this.menuItemLogView.Click += new EventHandler(this.menuItemLogView_Click);
			this.menuItemAppData.Click += new EventHandler(this.menuItemAppData_Click);
			this.menuItemUserData.Click += new EventHandler(this.menuItemUserData_Click);

			// Register file importers
			main.RegisterFileImporter(new PixmapFileImporter());
			main.RegisterFileImporter(new AudioDataFileImporter());
			main.RegisterFileImporter(new ShaderFileImporter());
			main.RegisterFileImporter(new FontFileImporter());

			main.ResourceModified += this.main_ResourceModified;
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
				this.projectView.Show(this.EditorForm.MainDockPanel);
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
				this.sceneView.Show(this.EditorForm.MainDockPanel);
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
				this.logView.Show(this.EditorForm.MainDockPanel);
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
				objView.Show(this.EditorForm.MainDockPanel);
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
				cam.Show(this.EditorForm.MainDockPanel);
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
			this.EditorForm.Select(this, new ObjectSelection(DualityApp.AppData));
		}
		private void menuItemUserData_Click(object sender, EventArgs e)
		{
			this.EditorForm.Select(this, new ObjectSelection(DualityApp.UserData));
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
				EditorBasePlugin.Instance.EditorForm.Select(this, new ObjectSelection(newObj));
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
			// If a font has been modified, update all TextRenderers
			if (typeof(Font).IsAssignableFrom(e.ContentType))
			{
				foreach (Duality.Components.Renderers.TextRenderer r in Scene.Current.AllObjects.GetComponents<Duality.Components.Renderers.TextRenderer>())
				{
					r.Text.ApplySource();
					r.UpdateMetrics();
				}
			}
			// If its a Pixmap, reload all associated Textures
			else if (typeof(Pixmap).IsAssignableFrom(e.ContentType))
			{
				foreach (ContentRef<Texture> tex in ContentProvider.GetAvailContent<Texture>())
				{
					if (!tex.IsAvailable) continue;
					if (tex.Res.BasePixmap.Res == e.Content.Res)
					{
						tex.Res.ReloadData();
					}
				}
			}
			// If its a Texture, update all associated RenderTargets
			else if (typeof(Texture).IsAssignableFrom(e.ContentType))
			{
				foreach (ContentRef<RenderTarget> rt in ContentProvider.GetAvailContent<RenderTarget>())
				{
					if (!rt.IsAvailable) continue;
					if (rt.Res.Targets.Any(target => target.Res == e.Content.Res as Texture))
					{
						rt.Res.SetupOpenGLRes();
					}
				}
			}
			// If its some kind of shader, update all associated ShaderPrograms
			else if (typeof(AbstractShader).IsAssignableFrom(e.ContentType))
			{
				foreach (ContentRef<ShaderProgram> sp in ContentProvider.GetAvailContent<ShaderProgram>())
				{
					if (!sp.IsAvailable) continue;
					if (sp.Res.Fragment.Res == e.Content.Res as FragmentShader ||
						sp.Res.Vertex.Res == e.Content.Res as VertexShader)
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
				int result;
				ToolStripMenuItem menuItem1 = item1 as ToolStripMenuItem;
				ToolStripMenuItem menuItem2 = item2 as ToolStripMenuItem;

				System.Reflection.Assembly assembly1 = item1.Tag is Type ? (item1.Tag as Type).Assembly : item1.Tag as System.Reflection.Assembly;
				System.Reflection.Assembly assembly2 = item2.Tag is Type ? (item2.Tag as Type).Assembly : item2.Tag as System.Reflection.Assembly;
				int score1 = assembly1 == typeof(DualityApp).Assembly ? 1 : 0;
				int score2 = assembly2 == typeof(DualityApp).Assembly ? 1 : 0;
				result = score2 - score1;
				if (result != 0) return result;

				result = 
					(menuItem1 != null ? Math.Sign(menuItem1.DropDownItems.Count) : 0) - 
					(menuItem2 != null ? Math.Sign(menuItem2.DropDownItems.Count) : 0);
				if (result != 0) return result;

				result = item1.Text.CompareTo(item2.Text);
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

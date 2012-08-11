﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Drawing;
using System.Xml;
using System.Text.RegularExpressions;

using Duality;
using Duality.Resources;
using Duality.ObjectManagers;

using DualityEditor.Forms;
using DualityEditor.CorePluginInterface;

using OpenTK;

using Ionic.Zip;
using WeifenLuo.WinFormsUI.Docking;

namespace DualityEditor
{
	public enum SelectMode
	{
		Set,
		Append,
		Toggle
	}

	public static class DualityEditorApp
	{
		private	const	string	UserDataFile			= "editoruserdata.xml";
		private	const	string	UserDataDockSeparator	= "<!-- DockPanel Data -->";
		
		private	static MainForm						mainForm			= null;
		private	static GLControl					mainContextControl	= null;
		private	static List<EditorPlugin>			plugins				= new List<EditorPlugin>();
		private	static Dictionary<Type,List<Type>>	availTypeDict	= new Dictionary<Type,List<Type>>();
		private	static ReloadCorePluginDialog		corePluginReloader	= null;
		private	static bool							needsRecovery		= false;
		private	static GameObjectManager			editorObjects		= new GameObjectManager();
		private	static bool							dualityAppSuspended	= true;
		private	static List<Resource>				unsavedResources	= new List<Resource>();
		private	static ObjectSelection				selectionCurrent	= ObjectSelection.Null;
		private	static ObjectSelection				selectionPrevious	= ObjectSelection.Null;
		private	static bool							selectionChanging	= false;


		public	static	event	EventHandler	Terminating			= null;
		public	static	event	EventHandler	Idling				= null;
		public	static	event	EventHandler	UpdatingEngine		= null;
		public	static	event	EventHandler	SaveAllTriggered	= null;
		public	static	event	EventHandler<SelectionChangedEventArgs>			SelectionChanged		= null;
		public	static	event	EventHandler<ObjectPropertyChangedEventArgs>	ObjectPropertyChanged	= null;
		

		public static MainForm MainForm
		{
			get { return mainForm; }
		}
		public static GameObjectManager EditorObjects
		{
			get { return editorObjects; }
		}
		public static ObjectSelection Selection
		{
			get { return selectionCurrent; }
		}
		public static bool IsSelectionChanging
		{
			get { return selectionChanging; }
		}
		public static GLControl MainContextControl
		{
			get { return mainContextControl; }
		}
		public static IEnumerable<EditorPlugin> Plugins
		{
			get { return plugins; }
		}
		public static IEnumerable<Resource> UnsavedResources
		{
			get { return unsavedResources.Where(r => !r.Disposed && !r.IsDefaultContent && !string.IsNullOrEmpty(r.Path)); }
		}
		private static bool AppStillIdle
		{
			 get
			{
				NativeMethods.Message msg;
				return !NativeMethods.PeekMessage(out msg, IntPtr.Zero, 0, 0, 0);
			 }
		}
		
		
		public static void Init(MainForm mainForm, bool recover)
		{
			DualityEditorApp.needsRecovery = recover;
			DualityEditorApp.mainForm = mainForm;

			// Create working directories, if not existing yet.
			if (!Directory.Exists(EditorHelper.DataDirectory))
			{
				Directory.CreateDirectory(EditorHelper.DataDirectory);
				using (FileStream s = File.OpenWrite(Path.Combine(EditorHelper.DataDirectory, "WorkingFolderIcon.ico")))
				{
					EditorRes.GeneralRes.IconWorkingFolder.Save(s);
				}
				using (StreamWriter w = new StreamWriter(Path.Combine(EditorHelper.DataDirectory, "desktop.ini")))
				{
					w.WriteLine("[.ShellClassInfo]");
					w.WriteLine("ConfirmFileOp=0");
					w.WriteLine("NoSharing=0");
					w.WriteLine("IconFile=WorkingFolderIcon.ico");
					w.WriteLine("IconIndex=0");
					w.WriteLine("InfoTip=This is DualityEditors working folder");
				}

				DirectoryInfo dirInfo = new DirectoryInfo(EditorHelper.DataDirectory);
				dirInfo.Attributes |= FileAttributes.System;

				FileInfo fileInfoDesktop = new FileInfo(Path.Combine(EditorHelper.DataDirectory, "desktop.ini"));
				fileInfoDesktop.Attributes |= FileAttributes.Hidden;

				FileInfo fileInfoIcon = new FileInfo(Path.Combine(EditorHelper.DataDirectory, "WorkingFolderIcon.ico"));
				fileInfoIcon.Attributes |= FileAttributes.Hidden;
			}
			if (!Directory.Exists(EditorHelper.SourceDirectory)) Directory.CreateDirectory(EditorHelper.SourceDirectory);
			if (!Directory.Exists(EditorHelper.SourceMediaDirectory)) Directory.CreateDirectory(EditorHelper.SourceMediaDirectory);
			if (!Directory.Exists(EditorHelper.SourceCodeDirectory)) Directory.CreateDirectory(EditorHelper.SourceCodeDirectory);

			// Initialize Duality
			DualityApp.Init(DualityApp.ExecutionEnvironment.Editor, DualityApp.ExecutionContext.Editor, new[] {"logfile", "logfile_editor"});
			InitMainGLContext();
			ContentProvider.InitDefaultContent();
			LoadPlugins();
			LoadUserData();
			InitPlugins();

			// Set up core plugin reloader
			corePluginReloader = new ReloadCorePluginDialog(mainForm);
			
			// Register events
			mainForm.Activated += mainForm_Activated;
			mainForm.Deactivate += mainForm_Deactivate;
			Scene.Leaving += Scene_Leaving;
			Scene.Entered += Scene_Entered;
			Application.Idle += Application_Idle;
			Resource.ResourceSaved += Resource_ResourceSaved;
			FileEventManager.PluginChanged += FileEventManager_PluginChanged;

			// Initialize secondary editor components
			Sandbox.Init();
			HelpSystem.Init();
			FileEventManager.Init();

			// Enter an empty Scene and allow the engine to run
			Scene.Current = new Scene();
			dualityAppSuspended = false;
		}
		public static bool Terminate(bool byUser)
		{
			bool cancel = false;

			// Display safety message boxes if the close operation is triggered by the user.
			if (byUser)
			{
				var unsavedResTemp = DualityEditorApp.UnsavedResources.ToArray();
				if (unsavedResTemp.Any())
				{
					string unsavedResText = unsavedResTemp.Take(5).ToString(r => r.GetType().GetTypeCSCodeName(true) + ":\t" + r.FullName, "\n");
					if (unsavedResTemp.Count() > 5) 
						unsavedResText += "\n" + string.Format(EditorRes.GeneralRes.Msg_ConfirmQuitUnsaved_Desc_More, unsavedResTemp.Count() - 5);
					DialogResult result = MessageBox.Show(
						string.Format(EditorRes.GeneralRes.Msg_ConfirmQuitUnsaved_Desc, "\n\n" + unsavedResText + "\n\n"), 
						EditorRes.GeneralRes.Msg_ConfirmQuitUnsaved_Caption, 
						MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
					if (result == DialogResult.Yes)
					{
						Sandbox.Stop();
						DualityEditorApp.SaveAllProjectData();
					}
					else if (result == DialogResult.Cancel)
						cancel = true;
				}
				else
				{
					DialogResult result = MessageBox.Show(
						EditorRes.GeneralRes.Msg_ConfirmQuit_Desc, 
						EditorRes.GeneralRes.Msg_ConfirmQuit_Caption, 
						MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					if (result == DialogResult.No)
						cancel = true;
				}
			}

			// Not cancelling? Then actually start terminating.
			if (!cancel)
			{
				if (Terminating != null) Terminating(null, EventArgs.Empty);

				// Initialize secondary editor components
				FileEventManager.Terminate();
				HelpSystem.Terminate();
				Sandbox.Terminate();

				// Terminate Duality
				DualityEditorApp.SaveUserData();
				DualityApp.Terminate();
			}

			return !cancel;
		}

		private static void LoadPlugins()
		{
			CorePluginRegistry.RegisterPropertyEditorProvider(new Controls.PropertyEditors.DualityPropertyEditorProvider());

			Log.Editor.Write("Scanning for editor plugins...");
			Log.Editor.PushIndent();

			if (Directory.Exists("Plugins"))
			{
				string[] pluginDllPaths = Directory.GetFiles("Plugins", "*.editor.dll", SearchOption.AllDirectories);
				foreach (string dllPath in pluginDllPaths)
				{
					Log.Editor.Write("Loading '{0}'...", dllPath);
					Log.Editor.PushIndent();
					Assembly pluginAssembly = Assembly.Load(File.ReadAllBytes(dllPath));
					Type[] exportedTypes = pluginAssembly.GetExportedTypes();
					try
					{
						// Initialize plugin objects
						for (int j = 0; j < exportedTypes.Length; j++)
						{
							if (typeof(EditorPlugin).IsAssignableFrom(exportedTypes[j]))
							{
								Log.Editor.Write("Instantiating class '{0}'...", exportedTypes[j].Name);
								EditorPlugin plugin = (EditorPlugin)exportedTypes[j].CreateInstanceOf();
								plugin.LoadPlugin();
								plugins.Add(plugin);
							}
						}
					}
					catch (Exception e)
					{
						Log.Editor.WriteError("Error loading plugin '{0}'. Exception: {1}", dllPath, Log.Exception(e));
					}
					Log.Editor.PopIndent();
				}
			}

			Log.Editor.PopIndent();
		}
		private static void InitPlugins()
		{
			Log.Editor.Write("Initializing editor plugins...");
			Log.Editor.PushIndent();
			foreach (EditorPlugin plugin in plugins)
			{
				Log.Editor.Write("'{0}'...", plugin.Id);
				plugin.InitPlugin(mainForm);
			}
			Log.Editor.PopIndent();
		}
		
		public static IEnumerable<Assembly> GetDualityEditorAssemblies()
		{
			yield return typeof(MainForm).Assembly;
			foreach (Assembly a in plugins.Select(ep => ep.GetType().Assembly)) yield return a;
		}
		public static IEnumerable<Type> GetAvailDualityEditorTypes(Type baseType)
		{
			List<Type> availTypes;
			if (availTypeDict.TryGetValue(baseType, out availTypes)) return availTypes;

			availTypes = new List<Type>();
			IEnumerable<Assembly> asmQuery = GetDualityEditorAssemblies();
			foreach (Assembly asm in asmQuery)
			{
				availTypes.AddRange(
					from t in asm.GetExportedTypes()
					where baseType.IsAssignableFrom(t)
					orderby t.Name
					select t);
			}
			availTypeDict[baseType] = availTypes;

			return availTypes;
		}

		private static void SaveUserData()
		{
			Log.Editor.Write("Saving user data...");
			Log.Editor.PushIndent();

			using (FileStream str = File.Create(UserDataFile))
			{
				StreamWriter writer = new StreamWriter(str);
				// --- Save custom user data here ---
				XmlDocument xmlDoc = new XmlDocument();
				XmlElement rootElement = xmlDoc.CreateElement("PluginUserData");
				xmlDoc.AppendChild(rootElement);
				foreach (EditorPlugin plugin in plugins)
				{
					XmlElement pluginXmlElement = xmlDoc.CreateElement("Plugin_" + plugin.Id);
					rootElement.AppendChild(pluginXmlElement);
					plugin.SaveUserData(pluginXmlElement);
				}
				xmlDoc.Save(writer.BaseStream);
				// ----------------------------------
				writer.WriteLine();
				writer.WriteLine(UserDataDockSeparator);
				writer.Flush();
				mainForm.MainDockPanel.SaveAsXml(str, writer.Encoding);
			}

			Log.Editor.PopIndent();
		}
		private static void LoadUserData()
		{
			if (!File.Exists(UserDataFile)) return;

			Log.Editor.Write("Loading user data...");
			Log.Editor.PushIndent();

			using (StreamReader reader = new StreamReader(UserDataFile))
			{
				string line;
				// Retrieve pre-DockPanel section
				StringBuilder editorData = new StringBuilder();
				while ((line = reader.ReadLine()) != null && line.Trim() != UserDataDockSeparator) 
					editorData.AppendLine(line);
				// Retrieve DockPanel section
				StringBuilder dockPanelData = new StringBuilder();
				while ((line = reader.ReadLine()) != null) 
					dockPanelData.AppendLine(line);

				// Load DockPanel Data
				Log.Editor.Write("Loading DockPanel data...");
				Log.Editor.PushIndent();
				MemoryStream dockPanelDataStream = new MemoryStream(reader.CurrentEncoding.GetBytes(dockPanelData.ToString()));
				try
				{
					mainForm.MainDockPanel.LoadFromXml(dockPanelDataStream, DeserializeDockContent);
				}
				catch (XmlException e)
				{
					Log.Editor.WriteError("Cannot load DockPanel data due to malformed or non-existent Xml: {0}", Log.Exception(e));
				}
				Log.Editor.PopIndent();

				// --- Read custom user data from StringBuilder here ---
				Log.Editor.Write("Loading plugin user data...");
				Log.Editor.PushIndent();
				XmlDocument xmlDoc = new XmlDocument();
				try
				{
					xmlDoc.LoadXml(editorData.ToString());
					foreach (XmlElement child in xmlDoc.DocumentElement)
					{
						if (child.Name.StartsWith("Plugin_"))
						{
							string pluginName = child.Name.Substring(7, child.Name.Length - 7);
							foreach (EditorPlugin plugin in plugins)
							{
								if (plugin.Id == pluginName)
								{
									plugin.LoadUserData(child);
									break;
								}
							}
						}
					}
				}
				catch (XmlException e)
				{
					Log.Editor.WriteError("Cannot load plugin user data due to malformed or non-existent Xml: {0}", Log.Exception(e));
				}
				Log.Editor.PopIndent();
				// -----------------------------------------------------
			}

			Log.Editor.PopIndent();
		}
		private static IDockContent DeserializeDockContent(string persistName)
		{
			Log.Editor.Write("Deserializing layout: '" + persistName + "'");

			Type dockContentType = null;
			Assembly dockContentAssembly = null;
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly a in assemblies)
			{
				if ((dockContentType = a.GetType(persistName)) != null)
				{
					dockContentAssembly = a;
					break;
				}
			}
			
			if (dockContentType == null) 
				return null;
			else
			{
				// First ask plugins from the dock contents assembly for existing instances
				IDockContent deserializeDockContent = null;
				foreach (EditorPlugin plugin in plugins)
				{
					if (plugin.GetType().Assembly == dockContentAssembly)
					{
						deserializeDockContent = plugin.DeserializeDockContent(dockContentType);
						if (deserializeDockContent != null) break;
					}
				}

				// If none exists, create one
				return deserializeDockContent ?? (dockContentType.CreateInstanceOf() as IDockContent);
			}
		}

		public static void InitMainGLContext()
		{
			if (mainContextControl != null) return;
			mainContextControl = new GLControl(DualityApp.DefaultMode);
			mainContextControl.MakeCurrent();
			DualityApp.TargetMode = mainContextControl.Context.GraphicsMode;
		}

		public static void Select(object sender, ObjectSelection sel, SelectMode mode = SelectMode.Set)
		{
			selectionPrevious = selectionCurrent;
			if (mode == SelectMode.Set)
				selectionCurrent = selectionCurrent.Transform(sel);
			else if (mode == SelectMode.Append)
				selectionCurrent = selectionCurrent.Append(sel);
			else if (mode == SelectMode.Toggle)
				selectionCurrent = selectionCurrent.Toggle(sel);
			OnSelectionChanged(sender, sel.Categories);
		}
		public static void Deselect(object sender, ObjectSelection sel)
		{
			selectionPrevious = selectionCurrent;
			selectionCurrent = selectionCurrent.Remove(sel);
			OnSelectionChanged(sender, ObjectSelection.Category.None);
		}
		public static void Deselect(object sender, ObjectSelection.Category category)
		{
			selectionPrevious = selectionCurrent;
			selectionCurrent = selectionCurrent.Clear(category);
			OnSelectionChanged(sender, ObjectSelection.Category.None);
		}
		public static void Deselect(object sender, Predicate<object> predicate)
		{
			selectionPrevious = selectionCurrent;
			selectionCurrent = selectionCurrent.Clear(predicate);
			OnSelectionChanged(sender, ObjectSelection.Category.None);
		}

		public static void SaveCurrentScene(bool skipYetUnsaved = true)
		{
			if (!String.IsNullOrEmpty(Scene.Current.Path))
				Scene.Current.Save();
			else if (!skipYetUnsaved)
			{
				string basePath = Path.Combine(EditorHelper.DataDirectory, "Scene");
				string path = PathHelper.GetFreePath(basePath, Scene.FileExt);
				Scene.Current.Save(path);
			}
		}
		public static void SaveResources()
		{
			foreach (Resource res in UnsavedResources.ToArray()) // The Property does some safety checks
			{
				if (res == Scene.Current && Sandbox.IsActive) continue;
				res.Save();
			}
			unsavedResources.Clear();
		}
		public static void FlagResourceUnsaved(IEnumerable<Resource> res)
		{
			foreach (Resource r in res)
				FlagResourceUnsaved(r);
		}
		public static void FlagResourceUnsaved(Resource res)
		{
			if (unsavedResources.Contains(res)) return;
			unsavedResources.Add(res);
		}
		public static void FlagResourceSaved(IEnumerable<Resource> res)
		{
			foreach (Resource r in res)
				FlagResourceSaved(r);
		}
		public static void FlagResourceSaved(Resource res)
		{
			unsavedResources.Remove(res);
		}
		public static bool IsResourceUnsaved(Resource res)
		{
			return UnsavedResources.Contains(res);
		}
		public static bool IsResourceUnsaved(IContentRef res)
		{
			return res.ResWeak != null ? IsResourceUnsaved(res.ResWeak) : IsResourceUnsaved(res.Path);
		}
		public static bool IsResourceUnsaved(string resPath)
		{
			return UnsavedResources.Any(r => Path.GetFullPath(r.Path) == Path.GetFullPath(resPath));
		}
		public static void SaveAllProjectData()
		{
			if (!IsResourceUnsaved(Scene.Current) && !Sandbox.IsActive) SaveCurrentScene();
			SaveResources();

			if (SaveAllTriggered != null)
				SaveAllTriggered(null, EventArgs.Empty);
		}
		
		public static void UpdatePluginSourceCode()
		{
			// Initially generate source code, if not existing yet
			if (!File.Exists(EditorHelper.SourceCodeSolutionFile)) InitPluginSourceCode();
			
			// Replace exec path in user files, since VS doesn't support relative paths there..
			{
				XmlDocument userDoc;
				const string userFileCore = EditorHelper.SourceCodeProjectCorePluginFile + ".user";
				const string userFileEditor = EditorHelper.SourceCodeProjectEditorPluginFile + ".user";

				if (File.Exists(userFileCore))
				{
					userDoc = new XmlDocument();
					userDoc.Load(userFileCore);
					foreach (XmlElement element in userDoc.GetElementsByTagName("StartProgram").OfType<XmlElement>())
						element.InnerText = Path.GetFullPath("DualityLauncher.exe");
					foreach (XmlElement element in userDoc.GetElementsByTagName("StartWorkingDirectory").OfType<XmlElement>())
						element.InnerText = Path.GetFullPath(".");
					userDoc.Save(userFileCore);
				}
				
				if (File.Exists(userFileEditor))
				{
					userDoc = new XmlDocument();
					userDoc.Load(userFileEditor);
					foreach (XmlElement element in userDoc.GetElementsByTagName("StartProgram").OfType<XmlElement>())
						element.InnerText = Path.GetFullPath("DualityEditor.exe");
					foreach (XmlElement element in userDoc.GetElementsByTagName("StartWorkingDirectory").OfType<XmlElement>())
						element.InnerText = Path.GetFullPath(".");
					userDoc.Save(userFileEditor);
				}
			}

			// Keep auto-generated files up-to-date
			File.WriteAllText(EditorHelper.SourceCodeGameResFile, EditorHelper.GenerateGameResSrcFile());
		}
		public static void ReadPluginSourceCodeContentData(out string rootNamespace, out string desiredRootNamespace)
		{
			rootNamespace = null;
			desiredRootNamespace = EditorHelper.GenerateClassNameFromPath(EditorHelper.CurrentProjectName);

			// Read root namespaces
			if (File.Exists(EditorHelper.SourceCodeProjectCorePluginFile))
			{
				XmlDocument projXml = new XmlDocument();
				projXml.Load(EditorHelper.SourceCodeProjectCorePluginFile);
				foreach (XmlElement element in projXml.GetElementsByTagName("RootNamespace").OfType<XmlElement>())
				{
					if (rootNamespace == null) rootNamespace = element.InnerText;
				}
			}
		}
		public static void InitPluginSourceCode()
		{
			// Create solution file if not existing yet
			if (!File.Exists(EditorHelper.SourceCodeSolutionFile))
			{
				using (ZipFile gamePluginZip = ZipFile.Read(ReflectionHelper.GetEmbeddedResourceStream(typeof(MainForm).Assembly,  @"Resources\GamePluginTemplate.zip")))
				{
					gamePluginZip.ExtractAll(EditorHelper.SourceCodeDirectory, ExtractExistingFileAction.DoNotOverwrite);
				}
			}

			// If Visual Studio is available, don't use the express version
			if (File.Exists(EditorHelper.SourceCodeSolutionFile) && EditorHelper.IsJITDebuggerAvailable())
			{
				string solution = File.ReadAllText(EditorHelper.SourceCodeSolutionFile);
				File.WriteAllText(EditorHelper.SourceCodeSolutionFile, solution.Replace("# Visual C# Express 2010", "# Visual Studio 2010"), Encoding.UTF8);
			}
			
			string projectClassName = EditorHelper.GenerateClassNameFromPath(EditorHelper.CurrentProjectName);
			string newRootNamespaceCore = projectClassName;
			string newRootNamespaceEditor = newRootNamespaceCore + ".Editor";
			string pluginNameCore = projectClassName + "CorePlugin";
			string pluginNameEditor = projectClassName + "EditorPlugin";
			string oldRootNamespaceCore = null;
			string oldRootNamespaceEditor = null;

			// Update root namespaces
			if (File.Exists(EditorHelper.SourceCodeProjectCorePluginFile))
			{
				XmlDocument projXml = new XmlDocument();
				projXml.Load(EditorHelper.SourceCodeProjectCorePluginFile);
				foreach (XmlElement element in projXml.GetElementsByTagName("RootNamespace").OfType<XmlElement>())
				{
					if (oldRootNamespaceCore == null) oldRootNamespaceCore = element.InnerText;
					element.InnerText = newRootNamespaceCore;
				}
				projXml.Save(EditorHelper.SourceCodeProjectCorePluginFile);
			}

			if (File.Exists(EditorHelper.SourceCodeProjectEditorPluginFile))
			{
				XmlDocument projXml = new XmlDocument();
				projXml.Load(EditorHelper.SourceCodeProjectEditorPluginFile);
				foreach (XmlElement element in projXml.GetElementsByTagName("RootNamespace").OfType<XmlElement>())
				{
					if (oldRootNamespaceEditor == null) oldRootNamespaceEditor = element.InnerText;
					element.InnerText = newRootNamespaceEditor;
				}
				projXml.Save(EditorHelper.SourceCodeProjectEditorPluginFile);
			}

			// Guess old plugin class names
			string oldPluginNameCore = oldRootNamespaceCore + "CorePlugin";
			string oldPluginNameEditor = oldRootNamespaceCore + "EditorPlugin";
			string regExpr;
			string regExprReplace;

			// Replace namespace names: Core
			if (Directory.Exists(EditorHelper.SourceCodeProjectCorePluginDir))
			{
				regExpr = @"^(\s*namespace\s*)(.*)(" + oldRootNamespaceCore + @")(.*)(\s*{)";
				regExprReplace = @"$1$2" + newRootNamespaceCore + @"$4$5";
				foreach (string filePath in Directory.GetFiles(EditorHelper.SourceCodeProjectCorePluginDir, "*.cs", SearchOption.AllDirectories))
				{
					string fileContent = File.ReadAllText(filePath);
					fileContent = Regex.Replace(fileContent, regExpr, regExprReplace, RegexOptions.Multiline);
					File.WriteAllText(filePath, fileContent, Encoding.UTF8);
				}
			}

			// Replace namespace names: Editor
			if (Directory.Exists(EditorHelper.SourceCodeProjectEditorPluginDir))
			{
				regExpr = @"^(\s*namespace\s*)(.*)(" + oldRootNamespaceEditor + @")(.*)(\s*{)";
				regExprReplace = @"$1$2" + newRootNamespaceEditor + @"$4$5";
				foreach (string filePath in Directory.GetFiles(EditorHelper.SourceCodeProjectEditorPluginDir, "*.cs", SearchOption.AllDirectories))
				{
					string fileContent = File.ReadAllText(filePath);
					fileContent = Regex.Replace(fileContent, regExpr, regExprReplace, RegexOptions.Multiline);
					File.WriteAllText(filePath, fileContent, Encoding.UTF8);
				}
			}

			// Replace class names: Core
			if (File.Exists(EditorHelper.SourceCodeCorePluginFile))
			{
				string fileContent = File.ReadAllText(EditorHelper.SourceCodeCorePluginFile);

				// Replace class name
				regExpr = @"(\bclass\b)(.*)(" + oldPluginNameCore + @")(.*)(\s*{)";
				regExprReplace = @"$1$2" + pluginNameCore + @"$4$5";
				fileContent = Regex.Replace(fileContent, regExpr, regExprReplace, RegexOptions.Multiline);

				regExpr = @"(\bclass\b)(.*)(" + @"__CorePluginClassName__" + @")(.*)(\s*{)";
				regExprReplace = @"$1$2" + pluginNameCore + @"$4$5";
				fileContent = Regex.Replace(fileContent, regExpr, regExprReplace, RegexOptions.Multiline);

				File.WriteAllText(EditorHelper.SourceCodeCorePluginFile, fileContent, Encoding.UTF8);
			}

			// Replace class names: Editor
			if (File.Exists(EditorHelper.SourceCodeEditorPluginFile))
			{
				string fileContent = File.ReadAllText(EditorHelper.SourceCodeEditorPluginFile);

				// Replace class name
				regExpr = @"(\bclass\b)(.*)(" + oldPluginNameEditor + @")(.*)(\s*{)";
				regExprReplace = @"$1$2" + pluginNameEditor + @"$4$5";
				fileContent = Regex.Replace(fileContent, regExpr, regExprReplace, RegexOptions.Multiline);

				regExpr = @"(\bclass\b)(.*)(" + @"__EditorPluginClassName__" + @")(.*)(\s*{)";
				regExprReplace = @"$1$2" + pluginNameEditor + @"$4$5";
				fileContent = Regex.Replace(fileContent, regExpr, regExprReplace, RegexOptions.Multiline);
				
				// Repalce Id property
				regExpr = @"(\boverride\s*string\s*Id\s*{\s*get\s*{\s*return\s*" + '"' + @")(.*)(" + '"' + @"\s*;\s*}\s*})";
				regExprReplace = @"$1" + pluginNameEditor + @"$3";
				fileContent = Regex.Replace(fileContent, regExpr, regExprReplace, RegexOptions.Multiline);

				File.WriteAllText(EditorHelper.SourceCodeEditorPluginFile, fileContent, Encoding.UTF8);
			}
		}

		public static void NotifyObjPrefabApplied(object sender, ObjectSelection obj)
		{
			OnObjectPropertyChanged(sender, new PrefabAppliedEventArgs(obj));
		}
		public static void NotifyObjPropChanged(object sender, ObjectSelection obj, params PropertyInfo[] info)
		{
			OnObjectPropertyChanged(sender, new ObjectPropertyChangedEventArgs(obj, info));
		}
		public static void NotifyObjPropChanged(object sender, ObjectSelection obj, bool persistenceCritical, params PropertyInfo[] info)
		{
			OnObjectPropertyChanged(sender, new ObjectPropertyChangedEventArgs(obj, info, persistenceCritical));
		}

		public static T GetPlugin<T>() where T : EditorPlugin
		{
			return plugins.OfType<T>().FirstOrDefault();
		}

		public static bool DisplayConfirmBreakPrefabLink(ObjectSelection obj = null)
		{
			if (obj == null) obj = DualityEditorApp.Selection;

			var linkQueryObj =
				from o in obj.GameObjects
				where (o.PrefabLink == null && o.AffectedByPrefabLink != null && o.AffectedByPrefabLink.AffectsObject(o)) || (o.PrefabLink != null && o.PrefabLink.ParentLink != null && o.PrefabLink.ParentLink.AffectsObject(o))
				select o.PrefabLink == null ? o.AffectedByPrefabLink : o.PrefabLink.ParentLink;
			var linkQueryCmp =
				from c in obj.Components
				where c.GameObj.AffectedByPrefabLink != null && c.GameObj.AffectedByPrefabLink.AffectsObject(c)
				select c.GameObj.AffectedByPrefabLink;
			var linkList = new List<PrefabLink>(linkQueryObj.Concat(linkQueryCmp).Distinct());
			if (linkList.Count == 0) return true;

			DialogResult result = MessageBox.Show(
				EditorRes.GeneralRes.Msg_ConfirmBreakPrefabLink_Desc, 
				EditorRes.GeneralRes.Msg_ConfirmBreakPrefabLink_Caption, 
				MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
			if (result != DialogResult.Yes) return false;

			foreach (PrefabLink link in linkList) link.Obj.BreakPrefabLink();
			DualityEditorApp.NotifyObjPropChanged(null, 
				new ObjectSelection(linkList.Select(l => l.Obj)), 
				ReflectionInfo.Property_GameObject_PrefabLink);

			return true;
		}

		private static void OnIdling()
		{
			if (Idling != null)
				Idling(null, EventArgs.Empty);
		}
		private static void OnUpdatingEngine()
		{
			if (UpdatingEngine != null)
				UpdatingEngine(null, EventArgs.Empty);
		}
		private static void OnSelectionChanged(object sender, ObjectSelection.Category changedCategoryFallback)
		{
			//if (selectionCurrent == selectionPrevious) return;
			selectionChanging = true;

			if (SelectionChanged != null)
				SelectionChanged(sender, new SelectionChangedEventArgs(selectionCurrent, selectionPrevious, changedCategoryFallback));

			selectionChanging = false;
		}
		private static void OnObjectPropertyChanged(object sender, ObjectPropertyChangedEventArgs args)
		{
			if (args.PersistenceCritical)
			{
				// If a linked GameObject was modified, update its prefab link changelist
				if (!(args is PrefabAppliedEventArgs) && (args.Objects.GameObjects.Any() || args.Objects.Components.Any()))
				{
					HashSet<PrefabLink> changedLinks = new HashSet<PrefabLink>();
					foreach (object o in args.Objects.Objects)
					{
						Component cmp = o as Component;
						GameObject obj = o as GameObject;
						if (cmp == null && obj == null) continue;

						PrefabLink link = obj != null ? obj.AffectedByPrefabLink : cmp.GameObj.AffectedByPrefabLink;
						if (link == null) continue;
						if (cmp != null && !link.AffectsObject(cmp)) continue;
						if (obj != null && !link.AffectsObject(obj)) continue;

						// Handle property changes regarding affected prefab links change lists
						foreach (PropertyInfo info in args.PropInfos)
						{
							if (PushPrefabLinkPropertyChange(link, o, info))
								changedLinks.Add(link);
						}
					}

					foreach (PrefabLink link in changedLinks)
					{
						NotifyObjPropChanged(null, new ObjectSelection(new[] { link.Obj }), ReflectionInfo.Property_GameObject_PrefabLink);
					}
				}

				// When modifying prefabs, apply changes to all linked objects
				if (args.Objects.Resources.OfType<Prefab>().Any())
				{
					foreach (Prefab prefab in args.Objects.Resources.OfType<Prefab>())
					{
						List<PrefabLink> appliedLinks = PrefabLink.ApplyAllLinks(Scene.Current.AllObjects, p => p.Prefab == prefab);
						List<GameObject> changedObjects = new List<GameObject>(appliedLinks.Select(p => p.Obj));
						DualityEditorApp.NotifyObjPrefabApplied(null, new ObjectSelection(changedObjects));
					}
				}

				// If a Resource's Properties are modified, mark Resource for saving
				if (args.Objects.ResourceCount > 0)
				{
					foreach (Resource res in args.Objects.Resources)
					{
						if (Sandbox.IsActive && res is Scene && (res as Scene).IsCurrent) continue;
						FlagResourceUnsaved(res);
					}
				}

				// If a GameObjects's Property is modified, mark current Scene for saving
				if (args.Objects.GameObjects.Any(g => Scene.Current.AllObjects.Contains(g)) ||
					args.Objects.Components.Any(c => Scene.Current.AllObjects.Contains(c.GameObj)))
				{
					FlagResourceUnsaved(Scene.Current);
				}

				// If DualityAppData or DualityUserData is modified, save it
				if (args.Objects.OtherObjectCount > 0)
				{
					// This is probably not the best idea for generalized behaviour, but sufficient for now
					if (args.Objects.OtherObjects.Any(o => o is DualityAppData))
						DualityApp.SaveAppData();
					else if (args.Objects.OtherObjects.Any(o => o is DualityUserData))
						DualityApp.SaveUserData();
				}
			}

			// Fire the actual event
			if (ObjectPropertyChanged != null)
				ObjectPropertyChanged(sender, args);
		}
		private static bool PushPrefabLinkPropertyChange(PrefabLink link, object target, PropertyInfo info)
		{
			if (link == null) return false;

			if (info == ReflectionInfo.Property_GameObject_PrefabLink)
			{
				GameObject obj = target as GameObject;
				if (obj == null) return false;

				PrefabLink parentLink;
				if (obj.PrefabLink == link && (parentLink = link.ParentLink) != null)
				{
					parentLink.PushChange(obj, info, obj.PrefabLink.Clone());
					NotifyObjPropChanged(null, new ObjectSelection(new[] { parentLink.Obj }), info);
				}
				return false;
			}
			else
			{
				link.PushChange(target, info);
				return true;
			}
		}
		
		private static void Application_Idle(object sender, EventArgs e)
		{
			// Trigger idle event if no modal dialog is open.
			if (mainForm.Visible && mainForm.CanFocus)
				OnIdling();

			// Update Duality engine
			System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
			while (AppStillIdle)
			{
				watch.Restart();
				if (!dualityAppSuspended)
				{
					try
					{
						DualityApp.EditorUpdate(editorObjects, Sandbox.IsFreezed);
					}
					catch (Exception exception)
					{
						Log.Editor.WriteError("An error occured during a core update: {0}", Log.Exception(exception));
					}
					OnUpdatingEngine();
				}

				// Assure we'll at least wait 16 ms until updating again.
				while (watch.Elapsed.TotalSeconds < 0.016666d) 
				{
					// Go to sleep if we'd have to wait too long
					if (watch.Elapsed.TotalSeconds < 0.012d)
						System.Threading.Thread.Sleep(1);
					// App wants to do something? Stop waiting.
					else if (!AppStillIdle)
						break;
				}
			}
		}
		private static void Scene_Leaving(object sender, EventArgs e)
		{
			Deselect(null, ObjectSelection.Category.GameObjCmp);
		}
		private static void Scene_Entered(object sender, EventArgs e)
		{
			// Try to restore last GameObject / Component selection
			var objQuery = selectionPrevious.GameObjects.Select(g => Scene.Current.AllObjects.FirstOrDefault(sg => sg.FullName == g.FullName)).NotNull();
			var cmpQuery = selectionPrevious.Components.Select(delegate (Component c)
			{
				GameObject cmpObj = Scene.Current.AllObjects.FirstOrDefault(sg => sg.FullName == c.GameObj.FullName);
				if (cmpObj == null) return null;
				return cmpObj.GetComponent(c.GetType());
			}).NotNull();

			// Append restored selection to current one.
			ObjectSelection objSel = new ObjectSelection(((IEnumerable<object>)objQuery).Concat(cmpQuery));
			if (objSel.ObjectCount > 0) Select(null, objSel, SelectMode.Append);
		}
		private static void Resource_ResourceSaved(object sender, Duality.ResourceEventArgs e)
		{
			if (e.Path == null) return; // Ignore Resources without a path.
			if (e.IsDefaultContent) return; // Ignore default content
			FlagResourceSaved(e.Content.Res);
		}
		
		private static void mainForm_Activated(object sender, EventArgs e)
		{
			// Core plugin reload
			if (needsRecovery)
			{
				needsRecovery = false;
				Log.Editor.Write("Recovering from full plugin reload restart...");
				Log.Editor.PushIndent();
				corePluginReloader.State = ReloadCorePluginDialog.ReloaderState.RecoverFromRestart;
			}
			else if (corePluginReloader.State == ReloadCorePluginDialog.ReloaderState.WaitForPlugins)
			{
				corePluginReloader.State = ReloadCorePluginDialog.ReloaderState.ReloadPlugins;
			}
		}
		private static void mainForm_Deactivate(object sender, EventArgs e)
		{
			// Update source code, in case the user is switching to his IDE without hitting the "open source code" button again
			if (DualityApp.ExecContext != DualityApp.ExecutionContext.Terminated)
				DualityEditorApp.UpdatePluginSourceCode();
		}

		private static void FileEventManager_PluginChanged(object sender, FileSystemEventArgs e)
		{
			string pluginStr = Path.Combine("Plugins", e.Name);
			if (!corePluginReloader.ReloadSchedule.Contains(pluginStr))
				corePluginReloader.ReloadSchedule.Add(pluginStr);
			corePluginReloader.State = ReloadCorePluginDialog.ReloaderState.WaitForPlugins;
		}
	}
}

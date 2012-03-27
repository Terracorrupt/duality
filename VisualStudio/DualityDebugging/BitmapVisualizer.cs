﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.DebuggerVisualizers;

using Duality;
using Duality.Resources;

[assembly: DebuggerVisualizer(
	typeof(DualityDebugging.BitmapDebuggerVisualizer), 
	typeof(DualityDebugging.BitmapDebuggerVisualizerObjectSource), 
	Target = typeof(Bitmap), 
	Description = "Bitmap Visualizer")]

namespace DualityDebugging
{
	public class BitmapDebuggerVisualizerObjectSource : VisualizerObjectSource
	{
		public override void GetData(object target, Stream outgoingData)
		{
			Bitmap bitmap = target as Bitmap;
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(outgoingData, string.Format("Bitmap {0}x{1}", bitmap.Width, bitmap.Height));
			formatter.Serialize(outgoingData, bitmap);
			outgoingData.Flush();
		}
	}

	public class BitmapDebuggerVisualizer : DialogDebuggerVisualizer
	{
		protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
		{
			Stream incomingData = objectProvider.GetData();
			BinaryFormatter formatter = new BinaryFormatter();
			string name = (string)formatter.Deserialize(incomingData);
			Bitmap pixeldata = (Bitmap)formatter.Deserialize(incomingData);
			using (BitmapForm form = new BitmapForm()) {
				form.Text = name;
				form.Bitmap = pixeldata;
				windowService.ShowDialog(form);
			}
		}

		public static void TestShow(object objToVisualize)
		{
			if (objToVisualize is Bitmap)
			{
				var visualizerHost = new VisualizerDevelopmentHost(
					objToVisualize,
					typeof(BitmapDebuggerVisualizer),
					typeof(BitmapDebuggerVisualizerObjectSource));
				visualizerHost.ShowVisualizer();
			}
			else if (objToVisualize is Pixmap)
			{
				var visualizerHost = new VisualizerDevelopmentHost(
					objToVisualize,
					typeof(BitmapDebuggerVisualizer),
					typeof(PixmapDebuggerVisualizerObjectSource));
				visualizerHost.ShowVisualizer();
			}
			else if (objToVisualize is Texture)
			{
				var visualizerHost = new VisualizerDevelopmentHost(
					objToVisualize,
					typeof(BitmapDebuggerVisualizer),
					typeof(TextureDebuggerVisualizerObjectSource));
				visualizerHost.ShowVisualizer();
			}
			else
				throw new ArgumentException("Not supported.", "objToVisualize");

		}
	}
}
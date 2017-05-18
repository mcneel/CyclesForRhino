/**
Copyright 2014-2016 Robert McNeel and Associates

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
**/

using System;
using System.Drawing;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.PlugIns;
using Rhino.Render;
using RhinoCyclesCore.RenderEngines;

namespace CyclesForRhino.CyclesForRhino
{
	public class Plugin : RenderPlugIn
	{
		public override PlugInLoadTime LoadTime =>
			CyclesForRhinoConstants.Ok && RhinoApp.InstallationTypeString.Equals("WIP")
			? PlugInLoadTime.WhenNeeded
			: PlugInLoadTime.Disabled;

		protected override LoadReturnCode OnLoad(ref string errorMessage)
		{
			var shouldi = RhinoApp.InstallationTypeString.Equals("WIP") && CyclesForRhinoConstants.Ok;
			if (shouldi) {
				RhinoApp.WriteLine("Cycles for Rhino ready.");
			} else
			{
				if (!CyclesForRhinoConstants.Ok) errorMessage = "Cycles for Rhino is too old, get a newer version from https://github.com/mcneel/CyclesForRhino/releases/latest";
				else errorMessage = "Cycles for Rhino works only with the WIP.";
				RhinoApp.WriteLine(errorMessage);
			}
			return shouldi ? LoadReturnCode.Success : LoadReturnCode.ErrorShowDialog;
		}
		protected override bool SupportsFeature(RenderFeature feature)
		{
			if (feature == RenderFeature.CustomDecalProperties)
				return false;

			return true;
		}

		protected override Result RenderWindow(RhinoDoc doc, RunMode modes, bool fastPreview, RhinoView view, Rectangle rect, bool inWindow)
		{
			return Result.Failure;
		}

		protected override PreviewRenderTypes PreviewRenderType()
		{
			return PreviewRenderTypes.Progressive;
		}

		/// <summary>
		/// Implement the render entry point.
		/// 
		/// Rhino data is prepared for further conversion in RenderEngine.
		/// </summary>
		/// <param name="doc">Rhino document for which the render command was given</param>
		/// <param name="mode">mode</param>
		/// <param name="fastPreview">True for fast preview.</param>
		/// <returns></returns>
		protected override Result Render(RhinoDoc doc, RunMode mode, bool fastPreview)
		{
			ModalRenderEngine engine = null;
			try
			{
				engine = new ModalRenderEngine(doc, Id);
			} catch(ApplicationException)
			{
				engine = null;
			}
			if(engine == null || !engine.CanRender)
			{
				RhinoApp.WriteLine("Cycles for Rhino cannot render. It works only in WIP.");
				return Result.Failure;
			}

			var renderSize = Rhino.Render.RenderPipeline.RenderSize(doc);

			var pipe = new RhinoCycles.RenderPipeline(doc, mode, this, engine);

			engine.RenderWindow = pipe.GetRenderWindow(true);
			engine.RenderDimension = renderSize;
			engine.Database.RenderDimension = renderSize;

			engine.SetFloatTextureAsByteTexture(false);

			engine.CreateWorld(); // has to be done on main thread, so lets do this just before starting render session

			/* since we're an asynchronous renderer plugin we start the render process
			 * here, but, apart from data conversion and pumping, we fall right through
			 * without a complete render result.
			 */
			var rc = pipe.Render();

			if (Rhino.Render.RenderPipeline.RenderReturnCode.Ok != rc)
			{
				RhinoApp.WriteLine("Rendering failed:" + rc.ToString());
				return Result.Failure;
			}

			return Result.Success;
		}

		/// <summary>
		/// Handler for rendering preview thumbnails.
		/// 
		/// The CreatePreviewEventArgs parameter contains a simple
		/// scene description to be rendered. It contains a set of meshes
		/// and lights. Meshes have RenderMaterials attached to them.
		/// </summary>
		/// <param name="scene">The scene description to render, along with the requested quality setting</param>
		protected override void CreatePreview(CreatePreviewEventArgs scene)
		{
			scene.SkipInitialisation();

			if (scene.Quality == PreviewSceneQuality.Low)
			{
				scene.PreviewImage = null;
				return;
			}

			AsyncRenderContext a_rc = new PreviewRenderEngine(scene, Id);
			var engine = (PreviewRenderEngine)a_rc;

			engine.RenderDimension = scene.PreviewImageSize;
			/* create a window-less, non-document controlled render window */
			engine.RenderWindow = Rhino.Render.RenderWindow.Create(scene.PreviewImageSize);
			engine.Database.RenderDimension = engine.RenderDimension;

			engine.SetFloatTextureAsByteTexture(false);

			engine.CreateWorld();

			/* render the preview scene */
			PreviewRenderEngine.Renderer(engine);

			/* set final preview bitmap, or null if cancelled */
			scene.PreviewImage = engine.RenderWindow.GetBitmap();

#if DEBUG
			var prev =$"{Environment.GetEnvironmentVariable("TEMP")}\\previmg.jpg";
			scene.PreviewImage.Save(prev, System.Drawing.Imaging.ImageFormat.Jpeg);
#endif
		}
	}
}

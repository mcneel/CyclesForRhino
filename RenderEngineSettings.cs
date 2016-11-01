﻿using Eto.Drawing;
using Eto.Forms;
using Rhino;
using Rhino.UI;
using Rhino.UI.Controls;
using RhinoCyclesCore.Core;

namespace CyclesForRhino.CyclesForRhino
{
	public class RenderEngineSettings : EtoCollapsibleSection
	{
		private bool _hidden;

		private Label _sampleLabel;
		private NumericMaskedTextBox<int>_sample;
		private NumericUpDown _sampleInput;
		private Label _seedLabel;
		private NumericUpDown _seedInput;

		private Label _activeViewIsRaytraced;


		public RenderEngineSettings()
		{
			Caption = new LocalizeStringPair("Render engine settings", "Render engine settings");
			InitializeComponents();
			InitializeLayout();
			RegisterHandlers();
		}

		private void InitializeComponents()
		{
			_sampleLabel = new Label()
			{
				Text = "Samples",
				VerticalAlignment = VerticalAlignment.Center
			};
			_sampleInput = new NumericUpDown()
			{
				Increment = 1,
				MaxValue = 500000,
				MinValue = -1,
				DecimalPlaces = 0,
				MaximumDecimalPlaces = 0,
				Value = RcCore.It.EngineSettings.Samples
			};

			_seedLabel = new Label()
			{
				Text = "Seed",
				VerticalAlignment = VerticalAlignment.Center
			};

			_seedInput = new NumericUpDown()
			{
				Increment = 1,
				MaxValue = 500000,
				MinValue = -1,
				DecimalPlaces = 0,
				MaximumDecimalPlaces = 0,
				Value = RcCore.It.EngineSettings.Seed
			};

			_activeViewIsRaytraced = new Label()
			{
				Text = "..."
			};
		}

		private void InitializeLayout()
		{
			TableLayout layout = new TableLayout()
			{
				Padding = 10,
				Spacing = new Size(10, 10),
				Rows =
				{
					new TableRow(_sampleLabel, _sampleInput),
					new TableRow(_seedLabel, _seedInput),
					new TableRow(_activeViewIsRaytraced),
				}
			};

			Content = layout;
		}

		private void RegisterHandlers()
		{
			
		}

		public override bool Hidden
		{
			get
			{
				_hidden = false;
				var pid = Rhino.PlugIns.PlugIn.IdFromName("Cycles for Rhino");
				if (!pid.Equals(Rhino.Render.Utilities.DefaultRenderPlugInId))
				{
					_hidden = true;
				}
				var raytraced = RhinoDoc.ActiveDoc?.Views.ActiveView.RealtimeDisplayMode;
				if ( raytraced == null)
				{
					_hidden = true;
					_activeViewIsRaytraced.Text = "...";
				}
				else
				{
					if (raytraced.HudProductName() == "Raytraced")
					{
						_hidden = false;
						_activeViewIsRaytraced.Text = $"raytraced {raytraced.HudProductName()} {raytraced.HudRendererPaused()}";
					} else
					{
						_activeViewIsRaytraced.Text = "...";
						_hidden = true;
					}
				}

				return _hidden;
			}
		}

		public override int SectionHeight => 100;

		public override LocalizeStringPair Caption { get; }
	}
}
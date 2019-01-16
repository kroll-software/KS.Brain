/*
{*******************************************************************}
{                                                                   }
{       KS-Neuron DotNet Library                                    }
{                                                                   }
{       Copyright (c) 2010 - 2016 by Kroll-Software                 }
{       All Rights Reserved                                         }
{                                                                   }
{   You receive this source code for educational                    }
{   and for research purposes only.                                 }
{                                                                   }
{   You may not use this code or any derived work for               }
{   any applications, except for research and studies.              }
{                                                                   }
{   The intention for publishing this is to document                }
{   the invention by Detlef Kroll.  Altdorf / Switzerland           }
{                                                                   } 
{   You are invited to discuss this with me.                        } 
{   Email to kroll@kroll-software.ch                                } 
{                                                                   } 
{*******************************************************************}
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KS.Foundation;
using SummerGUI;
using SummerGUI.Charting;
using SummerGUI.Charting.PerfCharts;

namespace KS.Brain
{
	public class BrainPerformanceBarWidgetStyle : WidgetStyle
	{
		public BrainPerformanceBarWidgetStyle ()
			: base (SolarizedColors.Base02,
				SolarizedColors.White,
				System.Drawing.Color.Empty)
		{}
	}

	public class BrainPerformanceBar : TableLayoutContainer
	{		
		public PerfChart Chart (int index)
		{
			if (index < 0 || index > 3)
				return null;
			return this.Layout.Table.Rows [0].Cells [index].Widget as PerfChart;
		}
		/*** ***/

		public BrainPerformanceBar (string name)
			: base (name)
		{			
			CellPadding = new System.Drawing.Size(2, 2);
			Style.BackColorBrush.Color = SolarizedColors.Base01;
			Dock = Docking.Fill;

			string[] labels = new string[] {
				"Output", "Threshold", "Energy", "Excitement", "Stimulus"
			};

			//Charts = new PerfChart[4];
			for (int i = 0; i < 5; i++) {
				PerfChart display = new PerfChart ("perfchart"  + (i + 1).ToString());

				display.ChartStyle.ShowHorizontalGridLines = true;
				display.ChartStyle.ShowVerticalGridLines = true;
				display.ChartStyle.HorizontalGridPen = new ChartPen (SolarizedColors.Base03, 1f);
				display.ChartStyle.VerticalGridPen = new ChartPen (SolarizedColors.Base03, 1f);

				if (i % 2 > 0)
					display.DemoMode = PerfChart.DemoModes.Random;
				else
					display.DemoMode = PerfChart.DemoModes.Sinus;

				display.AverageLineColor = MetroColors.Colors[ThreadSafeRandom.Next(MetroColors.Colors.Length)];
				display.LineColor = MetroColors.Colors[ThreadSafeRandom.Next(MetroColors.Colors.Length)];
				display.TimerMode = TimerModes.Simple;
				display.Caption = labels [i];

				this.AddChild (display, i, 0);
				this.Layout.Table.Rows [i].SizeMode = TableSizeModes.Fill;
			}

			this.Stop ();
		}			

		public void Start()
		{
			for (int i = 0; i < 5; i++) {

				if (this.IsDisposed)
					break;

				PerfChart display = this.Children [i] as PerfChart;

				if (display != null) {
					if (i % 2 > 0)
						display.DemoMode = PerfChart.DemoModes.Random;
					else
						display.DemoMode = PerfChart.DemoModes.Sinus;
				}
			}
		}

		public void Stop()
		{
			for (int i = 0; i < 5; i++) {

				if (this.IsDisposed)
					break;

				PerfChart display = this.Children [i] as PerfChart;

				if (display != null) {
					display.DemoMode = PerfChart.DemoModes.None;
				}
			}
		}
	}
}


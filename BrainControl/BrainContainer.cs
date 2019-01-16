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
using OpenTK;
using OpenTK.Input;
using SummerGUI;

namespace KS.Brain
{
	public class BrainContainerWidgetStyle : WidgetStyle
	{
		public BrainContainerWidgetStyle ()
			: base (SolarizedColors.Base01,
				SolarizedColors.Base02,
				System.Drawing.Color.Empty)
		{}
	}

	public class BrainContainer : SplitContainer
	{
		//public BrainToolBar ToolBar { get; protected set; }
		public BrainControl BrainControl { get; protected set; }
		public BrainPerformanceBar BrainCharts { get; protected set; }
		public Brain Brain { get; protected set; }

		public BrainContainer (string name)
			: base (name, SplitOrientation.Vertical, 0.75f)
		{						
			BrainControl = new BrainControl ("brain");
			this.Panel1.AddChild (BrainControl);

			BrainCharts = new BrainPerformanceBar ("brainperform");
			this.Panel2.AddChild (BrainCharts);
		}

		public override bool OnKeyDown (OpenTK.Input.KeyboardKeyEventArgs e)
		{
			if (base.OnKeyDown (e))
				return true;

			switch (e.Key) {
			case Key.F1:
				Panel2Collapsed = !Panel2Collapsed;
				Invalidate ();
				return true;
			}

			return false;
		}

		public void InitBrain(BrainConfiguration configuration)
		{
			Brain = new Brain ();
			Brain.InitBrain (configuration);

			BrainControl.Brain = Brain;
			//BrainCharts.Brain = Brain;
		}

		public void LoadBrain(string filename)
		{			
			Brain = Brain.DeSerializeData (filename);
			BrainControl.Brain = Brain;
		}

		public void Run()
		{
			Brain.Run ();
			Brain.BrainMode = Brain.BrainModes.Dreaming;
		}

		public void Stop()
		{
			Brain.Cancel ();
		}

		public void Clear()
		{
			Brain.Clear ();
		}
					
		protected override void CleanupManagedResources ()
		{
			if (Brain != null)
				Brain.Cancel ();			
			base.CleanupManagedResources ();
		}
	}
}


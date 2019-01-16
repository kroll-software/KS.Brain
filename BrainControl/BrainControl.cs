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

namespace KS.Brain
{
	public class BrainControlWidgetStyle : WidgetStyle
	{
		public BrainControlWidgetStyle ()
			: base (SolarizedColors.Base02,
			        SolarizedColors.Base01,
				SolarizedColors.Base03)
			    //System.Drawing.Color.Empty)
		{}
	}



    public class BrainControl : Widget
    {
        public delegate void NeuronHoverEventHandler(object sender, BrainNeuronEventArgs e);
        public event NeuronHoverEventHandler NeuronHover = null;

		//private System.Drawing.Color ColorBack = SolarizedColors.Base03;
		//private System.Drawing.Color ColorGrid = ColorCheckerColors.Blue;
		//private System.Drawing.Color ColorGrid = System.Drawing.Color.FromArgb(218, SolarizedColors.Base03);
		private System.Drawing.Color ColorGrid = SolarizedColors.Base02;

		private System.Drawing.Color ColorBack = SolarizedColors.Base02;
		//private System.Drawing.Color ColorGrid = SolarizedColors.Base03;

        protected Brain m_Brain = null;
        public Brain Brain
        {
            get
            {
                return m_Brain;
            }
            set
            {
				if (m_Brain != value) {
					m_Brain = value;
					if (m_Brain != null) {
						this.Style.BackColorBrush.Color = ColorGrid;
                        m_Brain.InvalidateBrain += m_Brain_InvalidateBrain;
                        m_Brain.BeforeAutosave += m_Brain_BeforeAutosave;
                        m_Brain.AfterAutosave += m_Brain_AfterAutosave;                        
					} else {
						this.Style.BackColorBrush.Color = ColorCheckerColors.Black;
					}
				}
            }
        }

        void m_Brain_AfterAutosave(object sender, EventArgs e)
        {
            ResumeLayout();
        }

        void m_Brain_BeforeAutosave(object sender, EventArgs e)
        {
            SuspendLayout();
        }

        void m_Brain_InvalidateBrain(object sender, EventArgs e)
        {
            Invalidate();
        }

        private bool m_ShowHistory = false;
        public bool ShowHistory
        {
            get
            {
                return m_ShowHistory;
            }
            set
            {
                m_ShowHistory = value;
                Invalidate();
            }
        }

		Brush[] Brushes;

		private enum NeuronColors
		{	
			Empty,
			Yellow,
			Green,
			Cyan,
			Orange,
			Red,
			Blue,
			Violet,
			ActiveBack,
			InactiveBack,
			Gray,
			Silver
		}

        public BrainControl(string name)
			: base (name, Docking.Fill, new BrainControlWidgetStyle())
        {       
			Brushes = new Brush[] {		
				new SolidBrush(System.Drawing.Color.FromArgb(245, SolarizedColors.Silver)),
				new SolidBrush(System.Drawing.Color.FromArgb(245, SolarizedColors.Yellow)),
				new SolidBrush(System.Drawing.Color.FromArgb(240, SolarizedColors.Green)),
				new SolidBrush(System.Drawing.Color.FromArgb(240, SolarizedColors.Cyan)),
				new SolidBrush(System.Drawing.Color.FromArgb(240, SolarizedColors.Orange)),
				new SolidBrush(System.Drawing.Color.FromArgb(240, SolarizedColors.Red)),
				new SolidBrush(System.Drawing.Color.FromArgb(240, SolarizedColors.Blue)),
				new SolidBrush(System.Drawing.Color.FromArgb(170, SolarizedColors.Violet)),
				new SolidBrush(SolarizedColors.Base2),
				new SolidBrush(SolarizedColors.Base3),
				new SolidBrush(ColorBack),
				//new LinearGradientBrush (SolarizedColors.Base02, SolarizedColors.Base03, GradientDirections.Vertical),
				new SolidBrush(SolarizedColors.Silver)
			};
        }

        //protected override void OnResize(EventArgs e)
        //{
        //    base.OnResize(e);
        //    this.Invalidate();
        //}

		/***
		public override void OnLayout (IKsOpenGLContext ctx, System.Drawing.Rectangle bounds)
		{
			base.OnLayout (ctx, bounds);
			this.Invalidate ();
		}
		***/

		NeuronColors[] DefaultColors = new NeuronColors[]{
			NeuronColors.Empty,
			NeuronColors.Blue,
			NeuronColors.Cyan,
			NeuronColors.Green,
			NeuronColors.Yellow
		};

		NeuronColors[] AutoFireColors = new NeuronColors[]{
			NeuronColors.Empty,
			NeuronColors.Red,
			NeuronColors.Red,
			NeuronColors.Orange,
			NeuronColors.Orange
		};

		NeuronColors[] InterNeuronColors = new NeuronColors[]{
			NeuronColors.Empty,
			NeuronColors.Violet,
			NeuronColors.Violet,
			NeuronColors.Violet,
			NeuronColors.Violet
		};

		private NeuronColors GetNeuronColor(Neuron n)
		{
			if (n.AxonOutput == 0)
				return NeuronColors.Empty;
					
			if (n.FireState == FireStates.AutoFire)
				return AutoFireColors[(int)Math.Min(4, (Math.Abs(n.AxonOutput) + 0.5f))];
			else if (n.NeuronType == NeuronTypes.Interneuron)
				return InterNeuronColors[(int)Math.Min(4, (-n.AxonOutput + 0.5f))];
			else
				return DefaultColors[(int)Math.Min(4, (n.AxonOutput + 0.5f))];
		}

        private bool m_IsPaintingFlag = false;
		public override void OnPaint(IGUIContext ctx, System.Drawing.RectangleF bounds)
        {
			base.OnPaint(ctx, bounds);

			if (m_Brain == null || m_Brain.Layers == null)           
                return;

            if (m_IsPaintingFlag)
                return;

			//if (!System.Threading.Monitor.TryEnter (SyncObject))
			//	return;

			try {
				m_IsPaintingFlag = true;

				float rowHeight = Bounds.Height / Brain.Layers.Count;
				float rowWidth = Bounds.Width;
				float inputWidth = Bounds.Width / Brain.Layers.InputLayer.Classes.Count;
				float outputWidth = Bounds.Width / Brain.Layers.OutputLayer.Classes.Count;

				float hiddenHeight = Bounds.Height - (rowHeight * 2);


				//double squareSize = hiddenHeight * rowWidth;
				//double singleSize = squareSize / globals.CachedNeuronCount;
				//double singleSize = squareSize / (double)m_Brain.Layers.Configuration.NeuronsPerHiddenLayer;

				//float a = (float)Math.Floor(Math.Sqrt(singleSize));

				//float a = (float)Math.Ceiling(Math.Sqrt(m_Brain.CachedNeuronCount));
				//float a = (float)Math.Floor(Math.Sqrt(m_Brain.Layers.Configuration.NeuronsPerHiddenLayer));

				/***
				double aspectRatio = rowWidth / rowHeight;

				float deltaX = (float)(rowWidth) / a;
				float deltaY = (float)(rowHeight - 1) / a;

				deltaX = (float)(aspectRatio * a);
				deltaY = (float)(aspectRatio / a);

				float deltaX = a - 1f;
				float deltaY = a - 1f;
				***/

				float a = (float)Math.Ceiling(Math.Sqrt(m_Brain.CachedNeuronCount));
				float deltaX = (float)(rowWidth - 1f) / a;
				float deltaY = (float)(hiddenHeight - 1f) / a;

				float x = 0;
				float y = 0;

				NeuronColors color = NeuronColors.Empty;

				int classIndex = 0;
				foreach (List<Neuron> nclass in m_Brain.Layers.InputLayer.Classes)
				{
					float lBound = Bounds.Left + 1 + (classIndex * inputWidth);
					float rBound = lBound + inputWidth;
					x = lBound + 1f;
					y = Bounds.Top + 2f;

					foreach (Neuron n in nclass)
					{
						color = GetNeuronColor(n);

						if (color != NeuronColors.Empty) {
							ctx.FillRectangle (Brushes[(int)color], x, y, deltaX - 1, deltaY - 1);	
						}

						x += deltaX;
						if (x > rBound)
						{
							x = lBound + 1;
							y += deltaY;
						}
					}

					classIndex++;
				}

				a = (float)Math.Ceiling(Math.Sqrt(m_Brain.CachedNeuronCount));
				deltaX = (float)(rowWidth - 1f) / a;
				deltaY = (float)(hiddenHeight - 1f) / a;
				x = Bounds.Left + 1f;
				y = Bounds.Top + (1 * rowHeight) + 1f;

				foreach (IBrainLayer layer in m_Brain.Layers.HiddenLayers)
				{
					foreach (Neuron n in layer.Neurons)
					{
						color = GetNeuronColor(n);
						if (color != NeuronColors.Empty) {
							ctx.FillRectangle (Brushes[(int)color], x, y, deltaX - 1, deltaY - 1);	
						}

						x += deltaX;
						if (x > Bounds.Right)
						{
							x = Bounds.Left + 1;
							y += deltaY;
						}
					}
				}	

				float yTop = Bounds.Top + (rowHeight * (Brain.Layers.Count)) - 12f;

				classIndex = 0;
				foreach (List<Neuron> nclass in m_Brain.Layers.OutputLayer.Classes)
				{
					float lBound = Bounds.Left + 1 + (classIndex * outputWidth);
					float rBound = lBound + outputWidth;
					x = lBound + 1f;
					y = yTop;

					foreach (Neuron n in nclass)
					{
						color = GetNeuronColor(n);
						if (color != NeuronColors.Empty) {
							ctx.FillRectangle (Brushes[(int)color], x, y, deltaX - 1, deltaY - 1);	
						}

						x += deltaX;
						if (x > rBound)
						{
							x = lBound + 1;
							y += deltaY;
						}
					}

					classIndex++;
				}

			} catch (Exception ex) {
				ex.LogError ();
			} finally {
				//System.Threading.Monitor.Exit(SyncObject);
				m_IsPaintingFlag = false;
			}
        }

		public Neuron NeuronAtPosition(float X, float Y)
        {
			if (m_Brain == null || m_Brain.Layers == null)
                return null;

			X -= Bounds.Left;
			Y -= Bounds.Top;

            // Find Neuron at Mouse Position
            long neuronCount = m_Brain.CachedNeuronCount;
            float a = (float)Math.Floor(Math.Sqrt(neuronCount));

			float deltaX = (float)(Bounds.Width - 2) / a;
			float deltaY = (float)(Bounds.Height - 2) / a;

			int column = (int)Math.Floor(X / deltaX);
			int row = (int)Math.Floor(Y / deltaY);

            int index = ((int)a * row) + column;

            if (index < 0 || index >= neuronCount)
                return null;
            else
            {
                int nCount = 0;
                foreach (IBrainLayer layer in m_Brain.Layers)
                {
                    if (nCount + layer.Count > index)
                    {
                        return layer[index - nCount];
                    }

                    nCount += layer.Count;
                }

                return null;
            }
        }

		public override void OnMouseDown (OpenTK.Input.MouseButtonEventArgs e)
		{
			base.OnMouseDown (e);
			if (e.Button == OpenTK.Input.MouseButton.Left) {
				Neuron neuron = NeuronAtPosition(e.X, e.Y);

				if (neuron != null)
				{
					neuron.InvokeFire(0.5f, Brain.Iteration);
					this.Invalidate();
				}
			}				
		}

		public override void OnMouseMove (OpenTK.Input.MouseMoveEventArgs e)
		{
			base.OnMouseMove (e);
			if (NeuronHover != null)
				NeuronHover(this, new BrainNeuronEventArgs(NeuronAtPosition(e.X, e.Y)));
		}

		public override void OnMouseLeave (IGUIContext ctx)
		{
			base.OnMouseLeave (ctx);
			if (NeuronHover != null)
				NeuronHover(this, new BrainNeuronEventArgs(null));
		}			
    }
}

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
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using KS.Foundation;


namespace KS.Brain
{
	public class OutputLayer : LayerBase, IBrainLayer
	{
		[JsonIgnore]
		public List<List<Neuron>> Classes { get; private set; }

		public OutputLayer (int layerIndex)
			: base(layerIndex)
		{
			Classes = new List<List<Neuron>> ();
		}

		public override void InitLayer(BrainConfiguration configuration)
		{			
			for (int i = 0; i < configuration.NumOutputClasses; i++) {
				List<Neuron> NeuronClass = new List<Neuron> ();
				Classes.Add (NeuronClass);

				for (int k = 0; k < configuration.NumOutputClassNeurons; k++) {
					int length = (int)(ThreadSafeRandom.NextDouble () * configuration.NumInputClasses) + 1;
					Neuron n = new Neuron (LayerIndex, i, length, NeuronTypes.MirrorNeuron, ConnectionTypes.PreviousLayer, configuration.OutputLayerMaxSynapses);
					m_Neurons.Add (n);
					NeuronClass.Add (n);
				}
			}
		}	

		public override void AfterDeserialization (BrainConfiguration configuration)
		{
			base.AfterDeserialization (configuration);

			for (int i = 0; i < configuration.NumOutputClasses; i++) {
				List<Neuron> NeuronClass = new List<Neuron> ();
				Classes.Add (NeuronClass);
				for (int k = 0; k < configuration.NumOutputClassNeurons; k++) {
					Neuron n = m_Neurons[(i * configuration.NumOutputClassNeurons) + k];
					NeuronClass.Add (n);
				}
			}
		}
	}
}


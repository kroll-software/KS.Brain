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
using System.Runtime.Serialization;


namespace KS.Brain
{
	public class InputLayer : LayerBase, IBrainLayer
	{			
		[JsonIgnore]
		public List<List<Neuron>> Classes { get; private set; }

		public InputLayer (int layerIndex)
			: base(layerIndex)
		{
			Classes = new List<List<Neuron>> ();
		}

		public override void InitLayer(BrainConfiguration configuration)
		{
			for (int i = 0; i < configuration.NumInputClasses; i++) {
				List<Neuron> NeuronClass = new List<Neuron> ();
				Classes.Add (NeuronClass);

				for (int k = 0; k < configuration.NumInputClassNeurons; k++) {					
					Neuron n = new Neuron (LayerIndex, i, configuration.NumInputClasses, NeuronTypes.Perceptron, ConnectionTypes.None);
					m_Neurons.Add (n);
					NeuronClass.Add (n);
				}
			}				
		}
			
		public override void AfterDeserialization (BrainConfiguration configuration)
		{
			base.AfterDeserialization (configuration);

			for (int i = 0; i < configuration.NumInputClasses; i++) {
				List<Neuron> NeuronClass = new List<Neuron> ();
				Classes.Add (NeuronClass);
				for (int k = 0; k < configuration.NumInputClassNeurons; k++) {
					Neuron n = m_Neurons[(i * configuration.NumInputClassNeurons) + k];
					NeuronClass.Add (n);
				}
			}
		}
	}
}


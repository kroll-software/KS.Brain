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
	public class HiddenLayer : LayerBase, IBrainLayer
	{				
		public HiddenLayer (int layerIndex)
			: base(layerIndex)
		{
		}

		public override void InitLayer(BrainConfiguration configuration)
		{
			int neuronsPerColumn = configuration.NeuronsPerHiddenLayer / configuration.NumInputClasses;
			for (int i = 0; i < configuration.NumInputClasses; i++) {
				for (int k = 0; k < neuronsPerColumn; k++) {
					int length = (int)(ThreadSafeRandom.NextDouble () * configuration.NumInputClasses) + 1;
					m_Neurons.Add (new Neuron (LayerIndex, i, length, NeuronTypes.PyramidCell, ConnectionTypes.LocalLayer, configuration.HiddenLayerMaxSynapses));
					m_Neurons.Add (new Neuron (LayerIndex, i, length / 2 + 1, NeuronTypes.Interneuron, ConnectionTypes.LocalLayer, configuration.HiddenLayerMaxSynapses));
				}
			}

			/***
			for (int i = 0; i < configuration.NeuronsPerHiddenLayer; i++) {				
				m_Neurons.Add (new Neuron (LayerIndex, NeuronTypes.PyramidCell, ConnectionTypes.LocalLayer, configuration.HiddenLayerMaxSynapses));
				m_Neurons.Add (new Neuron (LayerIndex, NeuronTypes.Interneuron, ConnectionTypes.LocalLayer, configuration.HiddenLayerMaxSynapses));
			}
			***/
		}		

		public override void AfterDeserialization (BrainConfiguration configuration)
		{
			base.AfterDeserialization (configuration);
		}
	}
}


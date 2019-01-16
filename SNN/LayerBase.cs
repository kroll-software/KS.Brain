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
	public interface IBrainLayer
	{
		int LayerIndex { get; }
		IEnumerable<Neuron> Neurons { get; }
		void InitLayer (BrainConfiguration configuration);
		void AfterDeserialization (BrainConfiguration configuration);
		int Count { get; }
		Neuron FindItemByKey (string key);
		Neuron this [int index] { get; }
		QuickBag<Neuron> FiredNeurons { get; }
		void InitFiredNeurons ();
		void Clear ();
	}

	public abstract class LayerBase : IBrainLayer
	{		
		[JsonIgnore]
		public readonly object SyncObject = new object ();

		public virtual IEnumerable<Neuron> Neurons
		{
			get{
				return m_Neurons;
			}
		}

		[JsonIgnore]
		public QuickBag<Neuron> FiredNeurons { get; private set; }

		public void InitFiredNeurons()
		{
			FiredNeurons = new QuickBag<Neuron> (m_Neurons.Count);
		}
			
		public Neuron this [int index] 
		{
			get{				
				return m_Neurons [index];
			}
		}

		public int Count 
		{ 
			get {
				return m_Neurons.Count;
			}
		}

		public Neuron FindItemByKey (string key)
		{
			return m_Neurons.FindItemByKey (key);
		}

		public int LayerIndex { get; private set; }	
		public abstract void InitLayer (BrainConfiguration configuration);

		public virtual void AfterDeserialization (BrainConfiguration configuration)
		{
			InitFiredNeurons ();
		}

		protected NeuronCollection m_Neurons { get; set; }

		protected LayerBase (int layerIndex)
		{						
			LayerIndex = layerIndex;
			m_Neurons = new NeuronCollection ();
		}

		public virtual void Clear()
		{
			if (m_Neurons != null)
				m_Neurons.Clear ();
			if (FiredNeurons != null)
				FiredNeurons.Clear ();
		}			
	}
}


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
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Linq;
using KS.Foundation;
using Newtonsoft.Json;

namespace KS.Brain
{    
    public class LayerCollection : List<IBrainLayer>
    {
		[JsonIgnore]
        public readonly object SyncObject = new object();

		//[JsonRequired]
		//public BrainConfiguration Configuration { get; set; }

		public void InitLayers(BrainConfiguration configuration)
		{
			//Configuration = configuration;

			foreach (IBrainLayer layer in this) {
				layer.InitLayer (configuration);
				layer.InitFiredNeurons ();
			}

			globals.CachedNeuronCount = HiddenLayers.Sum (l => l.Count);
		}

		public InputLayer InputLayer {
			get{
				if (Count < 1)
					return null;
				return this [0] as InputLayer;
			}
		}

		public OutputLayer OutputLayer {
			get{
				if (Count < 2)
					return null;
				return this [this.Count -1] as OutputLayer;
			}
		}

		public IEnumerable<HiddenLayer> HiddenLayers
		{
			get{
				return this.OfType<HiddenLayer> ();
			}
		}

		public new void Clear()
		{
			this.ForEach(layer => layer.Clear());
			base.Clear();
		}

        public Neuron FindNeuronByKey(string key)
        {
            lock (SyncObject)
            {
                foreach (IBrainLayer layer in this)
                {
                    Neuron n = layer.FindItemByKey(key);
                    if (n != null)
                        return n;
                }

                return null;
            }
        }        

        public int NeuronCount
        {
            get
            {
                lock (SyncObject)
                {
					return this.Sum (l => l.Count);
                }
            }
        }
			
		[OnSerializing]
		internal void OnSerializingMethod(StreamingContext context)
		{
			//if (Configuration == null) {
			//	int iTest = 0;
			//}
		}	

		/***

		[OnSerialized]
		internal void OnSerializedMethod(StreamingContext context)
		{
		}
	
		[OnDeserializing]
		internal void OnDeserializingMethod(StreamingContext context)
		{
		}

		
		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext context)
		{			
			foreach (IBrainLayer layer in this)
			{
				foreach (Neuron neuron in layer.Neurons)
				{
					foreach (Synapse synapse in neuron.Synapses)
					{
						synapse.Axon = FindNeuronByKey(synapse.AxonKey);                        
					}
				}					
			}

			this.ForEach(layer => layer.AfterDeserialization (this.Configuration));
		}
		***/
    }
}

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
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using KS.Foundation;
using Newtonsoft.Json;

namespace KS.Brain
{    
    /// <summary>
    /// Synapes are added to the Neuron.Synapses Collection
    /// Synapse.Axon points to the transmitting Neuron
    /// </summary>
	    
    public class Synapse
    {
		[JsonIgnore]
        public readonly object SyncObject = new object();
                
		[JsonIgnore]
		public Neuron Axon { get; set; }
        
        private string m_AxonKey = "";
        [DefaultValue("")]
        public string AxonKey
        {
            get
            {
                if (Axon != null)
                    return Axon.Key;
                else
                    return m_AxonKey;
            }
            set
            {                
                m_AxonKey = value;                
            }
        }

        public Synapse()
        {
            BirthIteration = globals.Iteration;
        }

		~Synapse ()
		{
			Axon = null;
		}

        private float m_Emphasis = 1.0f;
        public float Emphasis
        {
            get
            {
                return m_Emphasis;
            }
            set
            {
                if (globals.MaxEmphasis > 0f)
                    m_Emphasis = Math.Min(value, globals.MaxEmphasis);
                else
					m_Emphasis = value;
            }
        }

		public void IncreaseEmphasis(float value)
		{
			m_Emphasis += Math.Max(0, 1f - m_Emphasis) * value;

			if (m_Emphasis > 1f)
				this.LogWarning ("m_Emphasis > 1: {0}", m_Emphasis);
		}
			        
		public long BirthIteration { get; set; }        

        public double Performance
        {
            get
            {
                // don't touch the initial values
                if (BirthIteration == 0)
                    return Double.MaxValue;

                long Lifespan = globals.m_Iteration - BirthIteration;
                if (Lifespan == 0)
                    return 0;
                else
                    return (double)m_Emphasis / (double)Lifespan;
            }
        }
    }
}

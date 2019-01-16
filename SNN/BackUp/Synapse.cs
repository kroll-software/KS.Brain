/*
{*******************************************************************}
{                                                                   }
{       KS-Neuron DotNet Library                                    }
{                                                                   }
{       Copyright (c) 2010 - 2014 by Kroll-Software                 }
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
using KS.Foundation;

namespace TheBrain.KSNeuronLib
{    
    /// <summary>
    /// Synapes are added to the Neuron.Synapses Collection
    /// Synapse.Axon points to the transmitting Neuron
    /// </summary>
    public class Synapse : IDisposable
    {
        private readonly object m_SyncObject = new object();
        public object SyncObject
        {
            get
            {
                return m_SyncObject;
            }
        }

        //[XmlIgnore]
        public Neuron Axon = null;
        
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
            m_BirthIteration = globals.Iteration;
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

        private long m_BirthIteration = 0;
        [DefaultValue(0)]
        public long BirthIteration
        {
            get
            {                
                return m_BirthIteration;
            }
            set
            {
                m_BirthIteration = value;
            }
        }

        public double Performance
        {
            get
            {
                // don't touch the initial values
                if (m_BirthIteration == 0)
                    return Double.MaxValue;

                long Lifespan = globals.m_Iteration - m_BirthIteration;
                if (Lifespan == 0)
                    return 0;
                else
                    return (double)m_Emphasis / (double)Lifespan;
            }
        }
    
        // ********* IDisposable **********
        private bool disposed = false;        
        public void Dispose()
        {            
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        // NOTE: Leave out the finalizer altogether if this class doesn't 
        // own unmanaged resources itself, but leave the other methods
        // exactly as they are. 
        ~Synapse()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }
        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    Axon = null;
                }
            }
            disposed = true;
        }
    }
}

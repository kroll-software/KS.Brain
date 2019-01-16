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
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Cluster : IDisposable
    {
        private NeuronCollection m_Neurons = null;
        public NeuronCollection Neurons
        {
            get
            {
                return m_Neurons;
            }
            set
            {
                m_Neurons = value;
            }
        }

        //private int m_ClusterSize = 0;
        //public int ClusterSize
        //{
        //    get
        //    {
        //        return m_ClusterSize;
        //    }
        //    set
        //    {
        //        m_ClusterSize = value;
        //    }
        //}

        public Cluster()
        {
            m_Neurons = new NeuronCollection();
        }

        public Cluster(int count)
        {
            m_Neurons = new NeuronCollection();
            
            for (int i = 0; i < count; i++)
            {
                m_Neurons.Add(new Neuron());
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
        ~Cluster()
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
                    // CleanUp here and set to null                    
                }
            }
            disposed = true;
        }
    }
}

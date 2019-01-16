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
using System.Collections.Generic;
using System.Linq;
using KS.Foundation;

namespace TheBrain.KSNeuronLib
{
    [Serializable]    
    public class ClusterCollection : List<Cluster>
    {
        public readonly object SyncObject = new object();

        public Neuron FindNeuronByKey(string key)
        {
            lock (SyncObject)
            {
                foreach (Cluster cluster in this)
                {
                    Neuron n = cluster.Neurons.FindItemByKey(key);
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
                    int Count = 0;
                    foreach (Cluster c in this)
                    {
                        Count += c.Neurons.Count;
                    }

                    return Count;
                }                
            }
        }        
    }
}

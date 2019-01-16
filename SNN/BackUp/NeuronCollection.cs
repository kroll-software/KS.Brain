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
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Drawing;
using KS.Foundation;

namespace TheBrain.KSNeuronLib
{
    public class NeuronCollection : KeyedCollection<string, Neuron>    
    {
        protected override string GetKeyForItem(Neuron item)
        {
            return item.Key;
        }

        public Neuron FindItemByKey(string key)
        {
            if (this.Contains(key))
                return this[key];
            else
                return null;
        }
    }
}

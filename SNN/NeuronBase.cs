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
	public abstract class NeuronBase //: INeuron
	{
		[JsonIgnore]
		public readonly object SyncObject = new object();

		protected NeuronBase ()
		{
		}

		public int Rank { get; private set; }

		public int CompareTo (INeuron other)
		{
			return this.Rank.CompareTo (other.Rank);
		}
	}
}


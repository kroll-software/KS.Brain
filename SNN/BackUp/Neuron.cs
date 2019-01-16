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
    public class NeuronComparerByExcitementDesc : IComparer<Neuron>
    {
        public int Compare(Neuron item1, Neuron item2)
        {
            if (item1 == item2)
                return 0;

            return item2.Excitement.CompareTo(item1.Excitement);
        }
    }

    public class NeuronComparerByEnergyAndExcitementDesc : IComparer<Neuron>
    {
        public int Compare(Neuron item1, Neuron item2)
        {
            if (item1 == item2)
                return 0;

            return item2.WordCloudValue().CompareTo(item1.WordCloudValue());
        }
    }

    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Neuron : IDisposable
    {
        private readonly object m_SyncObject = new object();
        public object SyncObject
        {
            get
            {
                return m_SyncObject;
            }
        }

        public enum NeuronTypes
        {
            PyramidCell,
            Interneuron
        }

        private NeuronTypes m_NeuronType = NeuronTypes.PyramidCell;
        public NeuronTypes NeuronType
        {
            get
            {
                return m_NeuronType;
            }
            set
            {
                m_NeuronType = value;
                if (m_NeuronType == NeuronTypes.Interneuron)
                    IsEmptyNeuron = true;
            }
        }

        public enum FireStates
        {
            idle,
            fire,
            autofire
        }

        private FireStates m_FireState = FireStates.idle;
        public FireStates FireState
        {
            get
            {
                return m_FireState;
            }
        }


        public bool OnceFired = false;

        // Configuration
        private float m_Threshold = 0.0f;
        [DefaultValue(0f)]
        public float Threshold
        {
            get
            {
                return m_Threshold;
            }
            set
            {
                if (m_Threshold != value)
                {
                    m_Threshold = value;
                }
            }
        }

        private float m_Energy = 0.5f;        
        public float Energy
        {
            get
            {
                return m_Energy;
            }
            set
            {
                if (m_Energy != value)
                {
                    m_Energy = Math.Min(1, Math.Max(0, value));
                }
            }
        }

        private float m_Excitement = 0f;
        [DefaultValue(0f)]
        public float Excitement
        {
            get
            {
                return m_Excitement;
            }
            set
            {
                if (m_Excitement != value)
                {
                    m_Excitement = Math.Min(1, Math.Max(0, value));
                }
            }
        }

        private float m_Stimulus = 0f;
        [DefaultValue(0f)]
        public float Stimulus
        {
            get
            {
                return m_Stimulus;
            }
            set
            {
                if (m_Stimulus != value)
                {
                    m_Stimulus = Math.Min(1, Math.Max(0, value));
                }
            }
        }

        private object m_Tag = null;
        public object Tag
        {
            get
            {
                return m_Tag;
            }
            set
            {
				/***
                if (value != null && !(value is ISerializable || (Attribute.IsDefined(value.GetType(), typeof(SerializableAttribute)))))
                    throw new Exception("Tag-Property must be set to a serializable type or attribute");
                ***/

                m_Tag = value;
            }
        }

        protected string m_Key = null;
        //[XmlAttribute]
        public virtual string Key
        {
            get
            {
                return m_Key;
            }
            set
            {
                if (m_Key != value)
                {
                    m_Key = value;
                }
            }
        }

        protected bool m_IsEmptyNeuron = false;
        [DefaultValue(false)]
        public virtual bool IsEmptyNeuron
        {
            get
            {
                return m_IsEmptyNeuron;
            }
            set
            {
                if (m_IsEmptyNeuron != value)
                {
                    m_IsEmptyNeuron = value;
                }
            }
        }

        public Neuron()
        {
            m_Key = System.Guid.NewGuid().ToString();
        }

        public Neuron(NeuronTypes neuronType)
        {
            m_Key = System.Guid.NewGuid().ToString();

            m_NeuronType = neuronType;
            if (m_NeuronType == NeuronTypes.Interneuron)
                IsEmptyNeuron = true;
        }

        public override int GetHashCode()
        {
            return m_Key.GetHashCode();
        }

        public List<Synapse> Synapses = new List<Synapse>();    // Input        

        private float m_AxonOutput = 0f;                         // Output
        //[XmlIgnore]        
        public float AxonOutput
        {
            get
            {
                return m_AxonOutput;
            }
        }

        private int m_AxonOutputCount = 0;                         // Output
        //[XmlIgnore]        
        public int AxonOutputCount
        {
            get
            {
                return m_AxonOutputCount;
            }
            set
            {
                m_AxonOutputCount = Math.Max(0, value);
            }
        }

        private int m_MaxSynapses = 0;
        [DefaultValue(0)]
        public int MaxSynapses
        {
            get
            {
                return m_MaxSynapses;
            }
            set
            {
                m_MaxSynapses = value;
            }
        }

        private Dictionary<string, int> m_RequestedConnections = new Dictionary<string, int>();

        private bool m_SynapseAddedThisRound = false;
        public bool AddSynapse(Synapse synapse)
        {
            return AddSynapse(synapse, false);
        }

        public bool AddSynapse(Synapse synapse, bool force)
        {
            lock (m_SyncObject)
            {
                if (force)
                {
                    if (m_MaxSynapses > 0 && Synapses.Count >= m_MaxSynapses)
                        return false;

                    Synapses.Add(synapse);
                    return true;
                }

                if (m_MaxSynapses > 0 && m_SynapseAddedThisRound)
                    return false;

                if (m_MaxSynapses == 0 || Synapses.Count == 0)
                {
                    Synapses.Add(synapse);
                    m_SynapseAddedThisRound = true;

                    return true;
                }
                else if (Synapses.Count > m_MaxSynapses)
                {
                    return false;
                }
                else
                {
                    if (m_RequestedConnections.ContainsKey(synapse.Axon.Key))
                    {
                        int score = m_RequestedConnections[synapse.Axon.Key];
                        if (score > 100)    // 7th contact
                        {
                            Synapses.Add(synapse);
                            m_SynapseAddedThisRound = true;
                            m_RequestedConnections.Clear();
                            return true;
                        }
                        else
                        {
                            m_RequestedConnections[synapse.Axon.Key] = score + 40;
                            return false;
                        }
                    }
                    else
                    {
                        if (m_RequestedConnections.Count > 12)
                            //m_RequestedConnections.Remove(m_RequestedConnections.Keys.First());
                            m_RequestedConnections.Remove(m_RequestedConnections.Keys.Skip(ThreadSafeRandom.Next(11)).First());

                        m_RequestedConnections.Add(synapse.Axon.Key, 40);

                        return false;
                    }
                }
            }
        }

        public void DeleteUnderperformingSynapses()
        {
            lock (m_SyncObject)
            {
                if (Synapses != null && Synapses.Count > 0)
                {
                    double AvgPerformance = Synapses.Average(S => S.Performance);
                    Synapses.OrderByDescending(S => S.Emphasis);

                    while (Synapses.Count > 0 && Synapses.Last().Performance < AvgPerformance / 2d)
                        Synapses.Remove(Synapses.Last());
                }
            }

            //if (Synapses.Count < 3)
            //    return;
        }

        private int m_FireCycle = 0;
        [DefaultValue(0)]
        public int FireCycle
        {
            get
            {
                return m_FireCycle;
            }
            set
            {
                m_FireCycle = value;
            }
        }        

        public void Listen()
        {            
            if (m_FireState != FireStates.idle)
                return;

            lock (m_SyncObject)
            {
                m_SynapseAddedThisRound = false;

                m_Energy += (1f - m_Energy) * 0.001f;   // Original
                //m_Energy += (1f - m_Energy) * 0.00025f;  // Test am 8.1.2014
                if (m_Energy > 1f)
                    m_Energy = 1f;

                m_Stimulus -= 0.001f * (m_Stimulus / 10f);
                //m_Stimulus -= 0.005f * m_Stimulus;
                if (m_Stimulus < 0.001f)
                    m_Stimulus = 0.001f;

                float input = 0;

                // much slower than below
                //input = Synapses.Sum(s => s.Axon.AxonOutput * s.Emphasis * (1f - m_Excitement));

                foreach (Synapse synapse in Synapses)
                {
                    input += synapse.Axon.AxonOutput * synapse.Emphasis * (1f - m_Excitement);
                }                

                if (this.m_NeuronType == NeuronTypes.Interneuron)
                {
                    if (m_Threshold < 1f)
                        m_Threshold = 1f;
                }
                else
                {
                    if (m_Threshold < globals.GlobalThreshold)
                        m_Threshold = globals.GlobalThreshold;
                }

                if (input >= m_Threshold)
                {
                    // too noisy here ? I'm quiet
                    if (input > m_Threshold * 3f)
                    {
                        m_Threshold *= 1.5f;                        
                        return;
                    }

                    m_Threshold = input + 0.1f + (float)(ThreadSafeRandom.NextDouble() / 10.0);                    
                    InvokeFireInternal(0.01f);
                }
                else
                {
                    // Adjust Threshold

                    //m_Threshold -= 0.001f;
                    //m_Threshold -= 0.0025f;   // 15. Dezember 2015: release 2.5 times faster
                    m_Threshold -= 0.01f;   // 15. Dezember 2015: release 2.5 times faster

                    if (this.m_NeuronType == NeuronTypes.Interneuron)
                    {
                        if (m_Threshold < 1f)
                            m_Threshold = 1f;
                    }
                    else
                    {
                        if (m_Threshold < globals.GlobalThreshold)
                            m_Threshold = globals.GlobalThreshold;
                    }

                    //m_Excitement -= 0.001f * m_Excitement;
                    ////m_Excitement -= 0.005f * m_Excitement;
                    //if (m_Excitement < 0f)
                    //    m_Excitement = 0f;

                    // ******* ORIGINAL ********
                    //if (m_FireState == FireStates.fire)
                    //    m_Excitement -= 0.00025f * m_Excitement;
                    //else
                    //    m_Excitement = 0f;

                    //switch (m_FireState)
                    //{
                    //    case FireStates.idle:
                    //        //m_Excitement -= 0.005f * m_Excitement;
                    //        m_Excitement = 0f;
                    //        break;

                    //    case FireStates.fire:
                    //        // Do nothing, we rise up excitement
                    //        // or lower just a little to distinct from others
                    //        m_Excitement -= 0.00025f * m_Excitement;
                    //        break;

                    //    case FireStates.autofire:
                    //        m_Excitement = 0f;
                    //        break;
                    //}

                    switch (m_FireState)
                    {
                        case FireStates.idle:
                            m_Excitement -= 0.005f * m_Excitement;
                            break;

                        case FireStates.fire:
                            // Do nothing, we rise up excitement
                            // or lower just a little to distinct from others
                            m_Excitement -= 0.00025f * m_Excitement;
                            break;

                        case FireStates.autofire:
                            m_Excitement = 0f;
                            break;
                    }

                    if (m_Excitement < 0f)
                        m_Excitement = 0f;

                    // No Autofire for Interneurons

                    // When dreaming
                    if (!globals.IsAwake)
                    {                        
                        if (m_Energy > 0.1 && this.m_NeuronType == NeuronTypes.PyramidCell && m_FireState == FireStates.idle)
                        {
                            // For empty still unassigned neurons
                            if (Synapses.Count < 24)                            
                                m_Stimulus = 0.01f;

                            double r = ThreadSafeRandom.NextDouble() * 1000.0;                            

                            if ((m_Energy / (float)globals.CachedNeuronCount) + (m_Energy * m_Stimulus * globals.AutoFireFactor) > r)   // war *100
                                DoAutoFireInternal();                            
                        }
                    }
                }
            }
        }

        public void Awake()
        {
            //m_Energy = 0.5f;
            //m_Excitement = 0;            
            //m_Stimulus = 0;
        }

        private void InvokeFireInternal(float stimulus)
        {
            if (m_FireState != FireStates.idle || m_Energy < 0.1)
                return;

            m_InvokedIteration = globals.Iteration;

            //m_Stimulus += stimulus * (1f - m_Stimulus);
            m_Stimulus += stimulus;
            if (m_Stimulus > 1f)
                m_Stimulus = 1f;

            //m_Excitement += ((1f - m_Excitement) * m_Stimulus);
            //m_Excitement += stimulus * (1f - m_Excitement);
            //if (m_Excitement > 1f)
            //    m_Excitement = 1f;

            //m_FireCycleLength = ThreadSafeRandom.Next(5, 6 + Math.Min(5, (Synapses.Count * 2)));            
            //m_FireDelay = (m_AxonOutputCount * 2) + ThreadSafeRandom.Next(1, 2 + Math.Min(5, (Synapses.Count * 2)));

            //m_FireDelay = ThreadSafeRandom.Next(1, 5);
            //m_FireDelay = MaxSynapses / 4;
            //m_FireDelay = 1;
            m_FireDelay = ThreadSafeRandom.Next(1, 3);
            m_FireCycleLength = ThreadSafeRandom.Next(5, 8);

            OnceFired = true;
            m_FireState = FireStates.fire;
            m_FireCycle = 0;

            Fire();
        }

        public void InvokeFire()
        {
            InvokeFire(0.01f, 1f); // war 0.01f 
        }

        public void InvokeFire(float stimulus)
        {
            InvokeFire(stimulus, 1f);
        }

        private long m_InvokedIteration = 0;
        public long InvokedIteration
        {
            get
            {
                return m_InvokedIteration;
            }
        }

        public void InvokeFire(float stimulus, float energy)
        {
            if (energy < 0)
                energy = 0;

            if (energy > 1f)
                energy = 1f;

            lock (m_SyncObject)
            {                
                m_Energy = energy;
                InvokeFireInternal(stimulus);
            }            
        }

        private void DoAutoFireInternal()
        {
            if (m_Energy < 0.1)
                return;

            //m_FireDelay = ThreadSafeRandom.Next(1, 7);
            //m_FireCycleLength = ThreadSafeRandom.Next(6, 11);

            m_InvokedIteration = globals.Iteration;

            m_FireDelay = ThreadSafeRandom.Next(1, 7);
            m_FireCycleLength = ThreadSafeRandom.Next(5, 11);

            m_FireState = FireStates.autofire;
            m_FireCycle = 0;

            Fire();
        }

        public void DoAutoFire()
        {
            lock (m_SyncObject)
            {
                DoAutoFireInternal();
            }            
        }

        // To prevent the Equilibrium / global self oscilation effects,
        // we use a random frequency
        private int m_FireCycleLength = 5;
        private int m_FireDelay = 1;

        private void FireInternal()
        {
            if (m_FireState == FireStates.idle)
                return;

            if (m_Energy < 0.1)
            {
                m_FireState = FireStates.idle;
                return;
            }

            m_FireDelay--;
            if (m_FireDelay > 0)
                return;

            //if (m_FireCycle == 0 && m_FireState == FireStates.fire)
            //{
            //    //m_Excitement += 0.025f * (1f - m_Excitement);
            //    m_Excitement += 0.1f * (1f - m_Excitement);
            //    //m_Excitement += ((1.0f - m_Excitement) * m_Stimulus) / 5.0f;
            //    if (m_Excitement > 1f)
            //        m_Excitement = 1f;
            //}

            if (m_FireCycle < 6)
            {
                if (m_NeuronType == NeuronTypes.PyramidCell)
                    m_AxonOutput = globals.ActionPotentials[m_FireCycle];
                else
                    m_AxonOutput = -globals.ActionPotentials[m_FireCycle];
            }
            else
            {
                m_AxonOutput = 0;
            }

            //m_Excitement += Math.Abs(m_AxonOutput) * 0.01f * (1f - m_Excitement);
            m_Excitement += Math.Abs(m_AxonOutput) * 0.005f;

            if (m_Excitement > 1f)
                m_Excitement = 1f;


            m_FireCycle++;
            if (m_FireCycle > m_FireCycleLength)
            {
                m_FireCycle = 0;
                m_AxonOutput = 0;

                m_Energy -= 0.05f;
                if (m_Energy < 0f)
                    m_Energy = 0f;

                // ToDo: Feed Energy to damped Input
                if (m_Energy * ThreadSafeRandom.NextDouble() < 0.1)
                {
                    m_FireState = FireStates.idle;
                }
                else
                {
                    m_FireDelay = 1;
                }
            }
        }

        public void Fire()
        {
            lock (m_SyncObject)
            {
                FireInternal();
            }
        }


        public void ResetFireState(bool ResetExcitement)
        {
            m_FireCycle = 0;

            //m_Threshold = globals.GlobalThreshold * 2.0;

            //m_Energy = 0.5f;

            if (ResetExcitement)
                m_Excitement = 0f;
            //m_Stimulus = 0f;

            m_AxonOutput = 0.0f;
            m_FireCycleLength = 5;

            m_FireState = Neuron.FireStates.idle;
        }
        

        protected bool m_Disposed = false;
        protected bool m_IsDisposing = false;

        public bool IsDisposed
        {
            get
            {
                return m_Disposed || m_IsDisposing;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Neuron()
        {
            if (!m_Disposed)
                OnFinalizerCalled();

            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.m_Disposed && !m_IsDisposing)
            {
                m_IsDisposing = true;

                if (disposing)
                {
                    lock (m_SyncObject)
                    {
                        CleanupManagedResources();
                    }
                }

                CleanupUnmanagedResources();
                m_Disposed = true;
            }
        }

        protected virtual void CleanupManagedResources()
        {
        }

        protected virtual void CleanupUnmanagedResources()
        {
            // CleanUp here and set to null
            if (m_Tag != null && m_Tag is IDisposable)
            {
                ((IDisposable)m_Tag).Dispose();
                m_Tag = null;
            }

            if (Synapses != null)
                Synapses.Clear();

            if (m_RequestedConnections != null)
                m_RequestedConnections.Clear();
        }

        protected virtual void OnFinalizerCalled()
        {
            //System.Diagnostics.Debug.WriteLine(String.Format("An instance of {0} was not disposed before garbage collection.", this.GetType().FullName));
        }

        public override string ToString()
        {
            return m_Key;
        }        
    }    
}

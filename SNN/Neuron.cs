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
		    
	public interface INeuron : IComparable<INeuron>
	{
		int Rank { get; }
		int Column { get; }
		int AxonLength { get; }
		FireStates FireState { get; }
		bool OnceFired  { get; }
		float Threshold { get; set; }
		float Energy { get; set; }
		float Excitement { get; set; }
		object Tag { get; set; }
		string Key { get; set; }
		bool IsEmptyNeuron  { get; }
		int MaxSynapses { get; set; }
		bool AddSynapse(Synapse synapse);
		bool AddSynapse(Synapse synapse, bool force);
		void DeleteUnderperformingSynapses();
		int FireCycle { get; set; }
		void Listen ();
		void Awake();
		void InvokeFire();
		void InvokeFire(float stimulus);
		long InvokedIteration { get; }
		void InvokeFire(float stimulus, float energy);
		void DoAutoFire();
		void Fire();
		void ResetFireState(bool ResetExcitement);
	}

	public enum NeuronTypes
	{
		PyramidCell,
		Interneuron,
		Perceptron,
		MirrorNeuron
	}

	public enum ConnectionTypes
	{
		None,
		LocalLayer,
		PreviousLayer
	}

	public enum FireStates
	{
		Idle,
		Fire,
		AutoFire
	}

	public class Neuron : INeuron
    {		
		[JsonIgnore]
        public readonly object SyncObject = new object();               

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

		public ConnectionTypes ConnectionType { get; set; }

		public int Rank { get; private set; }
		public int Column { get; private set; }
		public int AxonLength { get; private set; }

		public int CompareTo (INeuron other)
		{
			int ret = this.Rank.CompareTo (other.Rank);
			if (ret == 0)
				return this.Column.CompareTo (other.Column);
			return ret;
		}			        
			        
		public FireStates FireState { get; private set; }
        
		public bool OnceFired  { get; private set; }

        // Configuration        
		public float Threshold { get; set; }
        
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
			        
		public object Tag { get; set; }
                
		public string Key { get; set; }
        
		public bool IsEmptyNeuron  { get; private set; }
        
		public Neuron(int rank, int column, int length, NeuronTypes neuronType, ConnectionTypes connectionType, int maxSynapses = 0)
        {
            Key = Guid.NewGuid().ToString();
			Rank = rank;
			AxonLength = length;
			Column = column;
			m_NeuronType = neuronType;
			if (m_NeuronType == NeuronTypes.Interneuron)
				IsEmptyNeuron = true;
			ConnectionType = connectionType;
			Synapses = new List<Synapse> ();
			MaxSynapses = maxSynapses;
        }
			        
        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

		public List<Synapse> Synapses { get; private set; }    // Input        

		public float AxonOutput { get; private set; }        

        private int m_AxonOutputCount = 0;                      // Output        
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
			        
		public int MaxSynapses { get; set; }        

        private Dictionary<string, int> m_RequestedConnections = new Dictionary<string, int>();

        private bool m_SynapseAddedThisRound = false;
        public bool AddSynapse(Synapse synapse)
        {
            return AddSynapse(synapse, false);
        }

        public bool AddSynapse(Synapse synapse, bool force)
        {
            lock (SyncObject)
            {
                if (force)
                {
                    if (MaxSynapses > 0 && Synapses.Count >= MaxSynapses)
                        return false;

                    Synapses.Add(synapse);
                    return true;
                }

                if (MaxSynapses > 0 && m_SynapseAddedThisRound)
                    return false;

                if (MaxSynapses == 0 || Synapses.Count == 0)
                {
                    Synapses.Add(synapse);
                    m_SynapseAddedThisRound = true;

                    return true;
                }
                else if (Synapses.Count > MaxSynapses)
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
                            m_RequestedConnections.Remove(m_RequestedConnections.Keys.Skip(ThreadSafeRandom.Next(11)).First());

                        m_RequestedConnections.Add(synapse.Axon.Key, 40);

                        return false;
                    }
                }
            }
        }

        public void DeleteUnderperformingSynapses()
        {
            lock (SyncObject)
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

		public int FireCycle { get; set; }
        
        public void Listen()
        {            
            if (m_NeuronType == NeuronTypes.Perceptron || FireState != FireStates.Idle)
                return;

			if (m_NeuronType == NeuronTypes.MirrorNeuron && globals.GlobalAwakeness < 0)
				return;

            lock (SyncObject)
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

                //foreach (Synapse synapse in Synapses)
				for (int i = 0; i < Synapses.Count; i++)
                {
					Synapse synapse = Synapses [i];
                    input += synapse.Axon.AxonOutput * synapse.Emphasis * (1f - m_Excitement);
                }                

				/***
                if (this.m_NeuronType == NeuronTypes.Interneuron)
                {
                    if (Threshold < 1f)
                        Threshold = 1f;
                }
                else
                {
                    if (Threshold < globals.GlobalThreshold)
                        Threshold = globals.GlobalThreshold;
                }
                ***/

				if (Threshold < globals.GlobalThreshold)
					Threshold = globals.GlobalThreshold;

				/***
				if (input > 0) {
					int iTest = Rank;
				}
				***/

                if (input >= Threshold)
                {
                    // too noisy here ? I'm quiet
                    if (input > Threshold * 3f)
                    {
                        Threshold *= 1.5f;                        
                        return;
                    }

                    Threshold = input + 0.1f + (float)(ThreadSafeRandom.NextDouble() / 10.0);                    
                    InvokeFireInternal(0.01f);
                }
                else
                {
                    // Adjust Threshold


					//m_Threshold -= 0.001f;
					//m_Threshold -= 0.0025f;   // 15. Dezember 2015: release 2.5 times faster
					//Threshold -= 0.01f;   // 15. Dezember 2015: release 2.5 times faster
					//Threshold -= 0.1f;   // Jul 2016: release 10 times faster
					//Threshold -= Threshold * 0.05f;   // Jul 2016: release 10 times faster

						/***
                    if (this.m_NeuronType == NeuronTypes.Interneuron)
                    {
                        if (Threshold < 1f)
                            Threshold = 1f;
                    }
                    else
                    {
                        if (Threshold < globals.GlobalThreshold)
                            Threshold = globals.GlobalThreshold;
                    }
                    ***/

					Threshold = Math.Max (globals.GlobalThreshold, Threshold - (Threshold * 0.05f));

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

                    switch (FireState)
                    {
                        case FireStates.Idle:
                            m_Excitement -= 0.005f * m_Excitement;
                            break;

                        case FireStates.Fire:
                            // Do nothing, we rise up excitement
                            // or lower just a little to distinct from others
                            m_Excitement -= 0.00025f * m_Excitement;
                            break;

                        case FireStates.AutoFire:
                            m_Excitement = 0f;
                            break;
                    }

                    if (m_Excitement < 0f)
                        m_Excitement = 0f;

                    // No Autofire for Interneurons

                    // When dreaming
                    if (!globals.IsAwake)
                    {       						
                        //if (m_Energy > 0.1 && this.m_NeuronType == NeuronTypes.PyramidCell && FireState == FireStates.idle)
						if (m_Energy > 0.1 && FireState == FireStates.Idle)
                        {
							// For empty still unassigned neurons
                            //if (Synapses.Count < 24)                            
							if (Synapses.Count < 6)
							//if (Synapses.Count < 12)
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
            if (FireState != FireStates.Idle || m_Energy < 0.1)
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
            
			m_FireDelay = 1;
			m_FireCycleLength = 4;

			// Original
			//m_FireDelay = ThreadSafeRandom.Next(1, 3);
			//m_FireCycleLength = ThreadSafeRandom.Next(3, 5);

			//m_FireDelay = ThreadSafeRandom.Next(1, 5);
			//m_FireCycleLength = ThreadSafeRandom.Next(1, 5);

			//m_FireCycleLength = ThreadSafeRandom.Next(3, 8);
            //m_FireCycleLength = ThreadSafeRandom.Next(5, 8);

            OnceFired = true;
            FireState = FireStates.Fire;
            FireCycle = 0;

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

            lock (SyncObject)
            {                
                m_Energy = energy;
                InvokeFireInternal(stimulus);
            }            
        }

        private void DoAutoFireInternal()
        {
            if (m_Energy < 0.1 || m_NeuronType == NeuronTypes.Perceptron || m_NeuronType == NeuronTypes.MirrorNeuron)
                return;

            //m_FireDelay = ThreadSafeRandom.Next(1, 7);
            //m_FireCycleLength = ThreadSafeRandom.Next(6, 11);

            m_InvokedIteration = globals.Iteration;

            m_FireDelay = ThreadSafeRandom.Next(1, 7);
            m_FireCycleLength = ThreadSafeRandom.Next(5, 11);

            FireState = FireStates.AutoFire;
            FireCycle = 0;

            Fire();
        }

        public void DoAutoFire()
        {
            lock (SyncObject)
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
            if (FireState == FireStates.Idle)
                return;

            if (m_Energy < 0.1)
            {
                FireState = FireStates.Idle;
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

            if (FireCycle < 6)
            {
				if (m_NeuronType == NeuronTypes.Interneuron)
					AxonOutput = -globals.ActionPotentials[FireCycle];
				else
					AxonOutput = globals.ActionPotentials[FireCycle];                   
            }
            else
            {
                AxonOutput = 0;
            }

            //m_Excitement += Math.Abs(m_AxonOutput) * 0.01f * (1f - m_Excitement);
            m_Excitement += Math.Abs(AxonOutput) * 0.005f;

            if (m_Excitement > 1f)
                m_Excitement = 1f;


            FireCycle++;
            if (FireCycle > m_FireCycleLength)
            {
                FireCycle = 0;
                AxonOutput = 0;

                //m_Energy -= 0.05f;
				m_Energy -= 0.025f;
                if (m_Energy < 0f)
                    m_Energy = 0f;

                // ToDo: Feed Energy to damped Input
                if (m_Energy * ThreadSafeRandom.NextDouble() < 0.1)
                {
                    FireState = FireStates.Idle;
                }
                else
                {
                    m_FireDelay = 1;
                }
            }
        }

        public void Fire()
        {
            lock (SyncObject)
            {
                FireInternal();
            }
        }


        public void ResetFireState(bool ResetExcitement)
        {
            FireCycle = 0;

            //m_Threshold = globals.GlobalThreshold * 2.0;

            //m_Energy = 0.5f;

            if (ResetExcitement)
                m_Excitement = 0f;
            //m_Stimulus = 0f;

            AxonOutput = 0.0f;
            m_FireCycleLength = 5;

            FireState = FireStates.Idle;
        }
        		       
        public override string ToString()
        {
            return Key;
        }        
    }    
}

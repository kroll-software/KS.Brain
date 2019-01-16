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
using System.IO;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KS.Foundation;

namespace TheBrain.KSNeuronLib
{
    [Serializable]
    //[TypeConverter(typeof(ExpandableObjectConverter))]
    public class Brain : IDisposable
    {
        private readonly object m_SyncObject = new object();
        public object SyncObject
        {
            get
            {
                return m_SyncObject;
            }
        }
        
        private bool m_Parallel = true;
        //[XmlIgnore]
        public bool ParallelExecution
        {
            get
            {
                return m_Parallel;
            }
            set
            {
                m_Parallel = value;
            }
        }
        
        public long Iteration
        {
            get
            {
                return globals.m_Iteration;
            }
            set
            {
                globals.m_Iteration = value;
            }
        }
        
        public float GlobalThreshold
        {
            get
            {
                return globals.GlobalThreshold;
            }
            set
            {
                globals.GlobalThreshold = value;
            }
        }
        
        public double AutoFireFactor
        {
            get
            {
                return globals.AutoFireFactor;
            }
            set
            {
                globals.AutoFireFactor = value;
            }
        }

        public double m_SecondsRun
        {
            get
            {
                return globals.SecondsRun;
            }
            set
            {
                globals.SecondsRun = value;
            }
        }

        private bool m_EnableWhatFiresTogetherWiresTogether = false;
        //[XmlIgnore]
        public bool EnableWhatFiresTogetherWiresTogether
        {
            get
            {
                return m_EnableWhatFiresTogetherWiresTogether;
            }
            set
            {
                m_EnableWhatFiresTogetherWiresTogether = value;
            }
        }        
        
        private ClusterCollection m_Clusters = null;
        public ClusterCollection Clusters
        {
            get
            {
                return m_Clusters;
            }
            set
            {
                m_Clusters = value;
            }
        }

        private int m_ProcessorCount = 4;

        public Brain()
        {
            m_ProcessorCount = Environment.ProcessorCount;
        }

        public void InitBrain()
        {
            InitBrain(0);
        }

        public void InitBrain(int NumNeurons)
        {
            //InitBrain(NumNeurons, 30);
            InitBrain(NumNeurons, NumNeurons);
        }

        public void InitBrain(int NumNeurons, int ClusterSize)
        {            
            //int a = (int)Math.Ceiling(Math.Sqrt(NumNeurons));

            //int column = 0;
            //int row = 0;

            int currentcluster = 0;

            m_Clusters = new ClusterCollection();

            Cluster cluster = null;

            for (int i = 0; i < NumNeurons; i++)
            {
                if (currentcluster == 0)
                {
                    cluster = new Cluster();
                    m_Clusters.Add(cluster);
                }                                                

                Neuron neuron = new Neuron(Neuron.NeuronTypes.PyramidCell);
                cluster.Neurons.Add(neuron);

                // Add same number of Interneurons
                //neuron = new Neuron(Neuron.NeuronTypes.Interneuron);
                //cluster.Neurons.Add(neuron);

                currentcluster++;
                if (currentcluster > ClusterSize)
                    currentcluster = 0;
            }
            
            CalcConnectionCount();
            CalcTotalEmphasis();
        }

        public void WakeUp()
        {
            globals.GlobalAwakeness = 1;

            foreach (Cluster cluster in m_Clusters)            
                foreach (Neuron neuron in cluster.Neurons)
                    neuron.Awake();
            
            m_EnableWhatFiresTogetherWiresTogether = false;
        }

        public void GoToSleep()
        {
            globals.GlobalAwakeness = -1;
            m_EnableWhatFiresTogetherWiresTogether = true;
        }
        

        public void AddConnectionRandomDirection(Neuron neuron1, Neuron neuron2)
        {
            AddConnectionRandomDirection(neuron1, neuron2, 0.0f);            
        }

        public void AddConnectionRandomDirection(Neuron neuron1, Neuron neuron2, float EmphasisIncrease)
        {
            if (neuron1 == null || neuron2 == null)
                return;

            // Suppress connections between InterNeurons
            if (neuron1.NeuronType == Neuron.NeuronTypes.Interneuron && neuron2.NeuronType == Neuron.NeuronTypes.Interneuron)
                return;            

            if (neuron1.NeuronType == Neuron.NeuronTypes.Interneuron || neuron2.NeuronType == Neuron.NeuronTypes.Interneuron)
                EmphasisIncrease *= 0.382f;  // golden ratio                        

            int synapseCount1 = 0;
            int synapseCount2 = 0;            

            lock (neuron1.SyncObject)
            {
                synapseCount1 = neuron1.Synapses.Count;                
            }

            lock (neuron2.SyncObject)
            {
                synapseCount2 = neuron2.Synapses.Count;            
            }

            if (neuron1.FireState == Neuron.FireStates.autofire && synapseCount1 > 0)
                return;

            if (neuron2.FireState == Neuron.FireStates.autofire && synapseCount2 > 0)
                return;

            //if (synapseCount2 == 0 || neuron1.NeuronType == Neuron.NeuronTypes.Interneuron)
            //    AddConnection(neuron1, neuron2, EmphasisIncrease);
            //else if (synapseCount1 == 0 || neuron2.NeuronType == Neuron.NeuronTypes.Interneuron)
            //    AddConnection(neuron2, neuron1, EmphasisIncrease);            
            //else 

            if (neuron1.InvokedIteration == neuron2.InvokedIteration)
            {
                if (ThreadSafeRandom.NextDouble() > 0.5)
                    AddConnection(neuron1, neuron2, EmphasisIncrease);
                else
                    AddConnection(neuron2, neuron1, EmphasisIncrease);
            }
            else if (neuron1.InvokedIteration < neuron2.InvokedIteration)            
                AddConnection(neuron1, neuron2, EmphasisIncrease);            
            else            
                AddConnection(neuron2, neuron1, EmphasisIncrease);            
        }

        public void AddConnection(Neuron transmitter, Neuron receptor)
        {
            // AddConnection(transmitter, receptor, 0.1f); // Change am 15. Dezember 2015
            AddConnection(transmitter, receptor, 0.01f);
        }

        public void AddConnection(Neuron transmitter, Neuron receptor, float EmphasisIncrease)
        {
            AddConnection(transmitter, receptor, EmphasisIncrease, false);
        }

        public void AddConnection(Neuron transmitter, Neuron receptor, float EmphasisIncrease, bool force)
        {
            if (transmitter == receptor || transmitter == null)
                return;
                                    
            Synapse synapse = null;

            lock (receptor.SyncObject)
            {
                foreach (Synapse s in receptor.Synapses)
                {
                    if (s.Axon == transmitter)
                    {
                        synapse = s;
                        break;
                    }
                }
            }
            
            if (synapse == null)
            {
                lock (transmitter.SyncObject)
                {
                    foreach (Synapse s in transmitter.Synapses)
                    {
                        if (s.Axon == receptor)
                        {
                            synapse = s;
                            break;
                        }
                    }
                }
            }

            if (synapse != null)
            {
                //lock (synapse.SyncLock)
                //{
                if (globals.MaxEmphasis == 0 || synapse.Emphasis < globals.MaxEmphasis)
                    {
                        float OldEmphasis = synapse.Emphasis;
                        synapse.Emphasis += EmphasisIncrease;
                        globals.IncreaseTotalEmphasis((double)(synapse.Emphasis - OldEmphasis));
                    }
                //}
                return;
            }                    
                            
            synapse = new Synapse();
            synapse.Emphasis = EmphasisIncrease;
            
            synapse.Axon = transmitter;

            if (receptor.AddSynapse(synapse, force))
            {
                lock (transmitter.SyncObject)
                {
                    transmitter.AxonOutputCount++;
                }

                globals.IncreaseTotalEmphasis((double)synapse.Emphasis);
                globals.IncreaseConnectionCount();
            }            
        }

        public void RemoveConnection(Neuron transmitter, Neuron receptor)
        {
            lock (receptor.SyncObject)
            {
                foreach (Synapse s in receptor.Synapses)
                {
                    if (s.Axon == transmitter)
                    {
                        receptor.Synapses.Remove(s);

                        lock (transmitter.SyncObject)
                        {
                            transmitter.AxonOutputCount--;
                        }

                        globals.IncreaseTotalEmphasis(-(double)s.Emphasis);
                        globals.DecreaseConnectionCount();
                        break;
                    }
                }
            }
        }
                
        public int ConnectionCount
        {
            get
            {
                return globals.ConnectionCount;
            }
        }

        public void CalcConnectionCount()
        {            
            int count = 0;

            if (m_Parallel)
            {
                lock (m_Clusters.SyncObject)
                {
                    count = m_Clusters.AsParallel().Sum(C => C.Neurons.Sum(N => N.Synapses.Count));
                }
            }
            else
            {
                foreach (Cluster cluster in m_Clusters)
                    foreach (Neuron neuron in cluster.Neurons)
                        count += neuron.Synapses.Count;
            }

            globals.m_ConnectionCount = count;
        }

        public double TotalEmphasis
        {
            get
            {
                return globals.TotalEmphasis;
            }
        }

        public void CalcTotalEmphasis()
        {            
            float value = 0;

            if (m_Parallel)
            {
                lock (m_Clusters.SyncObject)
                {
                    value = m_Clusters.AsParallel().Sum(C => C.Neurons.Sum(N => N.Synapses.Sum(S => (float)S.Emphasis)));
                }
            }
            else
            {
                foreach (Cluster cluster in m_Clusters)
                    foreach (Neuron neuron in cluster.Neurons)                
                        foreach (Synapse s in neuron.Synapses)
                            value += (float)s.Emphasis;                                    
            }

            globals.m_TotalEmphasis = value;
        }


        public void SetRandomOutput(double quote)
        {
            lock (m_Clusters)
            {
                foreach (Cluster cluster in m_Clusters)
                    foreach (Neuron neuron in cluster.Neurons)
                        if (ThreadSafeRandom.NextDouble() < quote)
                            neuron.InvokeFire(this.Iteration);
            }
        }


        public void WhatFiresTogetherWiresTogether()
        {
            if (!m_EnableWhatFiresTogetherWiresTogether)
                return;

            //return;

            if (m_Parallel)
            {
                Parallel.For(0, m_FiredNeuronsCount - 1, delegate(int i, ParallelLoopState parallelState)                
                {
                    Neuron neuron1 = m_FiredNeurons[i];
                    for (int k = i + 1; k < m_FiredNeuronsCount; k++)
                    {
                        Neuron neuron2 = m_FiredNeurons[k];
                        
                        AddConnectionRandomDirection(neuron1, neuron2, 0.2f);

                        if (m_Cancel)
                            break;
                    }

                    if (m_Cancel)
                        parallelState.Break();                    
                });
            }
            else
            {                
                for (int i = 0; i < m_FiredNeuronsCount - 1; i++)
                {
                    Neuron neuron1 = m_FiredNeurons[i];
                    for (int k = i + 1; k < m_FiredNeuronsCount; k++)
                    {
                        Neuron neuron2 = m_FiredNeurons[k];
                        AddConnectionRandomDirection(neuron1, neuron2, 0.2f);
                    }

                    if (m_Cancel)
                        break;
                }               
            }            
        }


        private Neuron[] m_FiredNeurons = null;
        public Neuron[] FiredNeurons
        {
            get
            {
                return m_FiredNeurons;
            }
        }

        private int m_FiredNeuronsCount = 0;
        public int FiredNeuronsCount
        {
            get
            {
                return m_FiredNeuronsCount;
            }
        }

        private void GuaranteeFiredNeurons()
        {
            int neuronCount = m_Clusters.NeuronCount;
            if (m_FiredNeurons == null || m_FiredNeurons.Length < neuronCount)
            {
                //m_FiredNeurons = new Neuron[neuronCount + 1000];
                m_FiredNeurons = new Neuron[neuronCount + 1];
                m_FiredNeuronsCount = 0;
            }
        }

        private object m_FiredNeuronsLock = new object();

        public void Process()
        {                        
            m_FiredNeuronsCount = 0;

            if (m_Parallel)
            {
                if (m_Clusters.Count > m_ProcessorCount * 2)
                //if (1 == 1)
                {
                    lock (m_Clusters.SyncObject)
                    {
                        Parallel.ForEach<Cluster>(m_Clusters, delegate(Cluster cluster, ParallelLoopState parallelState)
                        {
                            foreach (Neuron neuron in cluster.Neurons)
                                neuron.Listen();

                            if (m_Cancel)
                                parallelState.Break();
                        });
                    }

                    if (!m_Cancel)
                    {
                        lock (m_Clusters.SyncObject)
                        {
                            Parallel.ForEach<Cluster>(m_Clusters, delegate(Cluster cluster, ParallelLoopState parallelState)
                            {
                                foreach (Neuron neuron in cluster.Neurons)
                                {
                                    neuron.Fire();

                                    if (Math.Abs(neuron.AxonOutput) >= 3f && (neuron.FireState == Neuron.FireStates.fire || (neuron.FireState == Neuron.FireStates.autofire && neuron.Synapses.Count == 0)))
                                    {
                                        lock (m_FiredNeuronsLock)
                                        {
                                            m_FiredNeurons[m_FiredNeuronsCount] = neuron;                                            
                                            m_FiredNeuronsCount++;
                                            //Interlocked.Increment(ref m_FiredNeuronsCount);
                                        }
                                        //
                                    }
                                }

                                if (m_Cancel)
                                    parallelState.Break();
                            });
                        }
                    }
                }
                else
                {
                    lock (m_Clusters.SyncObject)
                    {
                        foreach (Cluster cluster in m_Clusters)
                        {
                            Parallel.ForEach<Neuron>(cluster.Neurons, delegate(Neuron neuron, ParallelLoopState parallelState)
                            {
                                neuron.Listen();
                                if (m_Cancel)
                                    parallelState.Break();
                            });
                        }
                    }

                    if (!m_Cancel)
                    {
                        lock (m_Clusters.SyncObject)
                        {
                            foreach (Cluster cluster in m_Clusters)
                            {
                                Parallel.ForEach<Neuron>(cluster.Neurons, delegate(Neuron neuron, ParallelLoopState parallelState)
                                {
                                    neuron.Fire();

                                    if (Math.Abs(neuron.AxonOutput) >= 3f && (neuron.FireState == Neuron.FireStates.fire || (neuron.FireState == Neuron.FireStates.autofire && neuron.Synapses.Count == 0)))
                                    {
                                        lock (m_FiredNeuronsLock)
                                        {
                                            m_FiredNeurons[m_FiredNeuronsCount] = neuron;
                                            m_FiredNeuronsCount++;
                                            //Interlocked.Increment(ref m_FiredNeuronsCount);                                            
                                        }
                                    }

                                    if (m_Cancel)
                                        parallelState.Break();
                                });
                            }
                        }
                    }
                }                                   
                
                if (!m_Cancel)
                {
                    WhatFiresTogetherWiresTogether();
                }
                                
            }
            else
            {   // Not parallel
                foreach (Cluster cluster in m_Clusters)
                {
                    foreach (Neuron neuron in cluster.Neurons)
                    {
                        neuron.Listen();
                        if (m_Cancel)
                            break;
                    }
                }

                if (!m_Cancel)
                {
                    foreach (Cluster cluster in m_Clusters)
                    {
                        foreach (Neuron neuron in cluster.Neurons)
                        {
                            neuron.Fire();

                            if (Math.Abs(neuron.AxonOutput) >= 3f && (neuron.FireState == Neuron.FireStates.fire || (neuron.FireState == Neuron.FireStates.autofire && neuron.Synapses.Count == 0)))
                            {
                                m_FiredNeurons[m_FiredNeuronsCount] = neuron;
                                m_FiredNeuronsCount++;
                            }

                            if (m_Cancel)
                                break;
                        }
                    }

                    if (!m_Cancel)
                        WhatFiresTogetherWiresTogether();
                }                
            }            
        }


        // Long Running        

        public event EventHandler IterationFinished;
        public event EventHandler RunStarted;
        public event EventHandler RunStopped;

        private Task m_Task = null;
        private CancellationTokenSource m_TokenSource = null;
        public CancellationTokenSource TokenSource
        {
            get
            {
                return m_TokenSource;
            }
        }

        private bool m_IsRunning = false;
        private bool m_Cancel = false;
        
        public long CachedNeuronCount
        {
            get
            {                
                if (globals.CachedNeuronCount < 0)
                {
                    if (m_Clusters == null)
                        globals.CachedNeuronCount = 0;
                    else
                        globals.CachedNeuronCount = m_Clusters.NeuronCount;
                }

                return globals.CachedNeuronCount;
            }
        }

        private DateTime m_RunStartTime = DateTime.MinValue;

        public void Run()
        {
            m_IsRunning = true;
            m_Cancel = false;

            if (RunStarted != null)
                RunStarted(this, EventArgs.Empty);

            m_RunStartTime = DateTime.Now;

            m_TokenSource = new CancellationTokenSource();
            // Create a cancellation token from CancellationTokenSource
            CancellationToken cToken = m_TokenSource.Token;
            // Create a task and pass the cancellation token

            GuaranteeFiredNeurons();
            globals.CachedNeuronCount = -1;

            if (1 == 1)
            {
                m_Task = System.Threading.Tasks.Task.Factory.StartNew(delegate
                {
                    RunProcess(cToken);
                }, TaskCreationOptions.LongRunning);
            }
            else
            {
                m_Task = new System.Threading.Tasks.Task(new Action(() =>
                {
                    RunProcess(cToken);
                }), cToken, TaskCreationOptions.PreferFairness);

                TaskScheduler scheduler = TaskScheduler.Current;
                m_Task.Start(scheduler);
            }
        }        

        public IEnumerable<KSNeuronLib.Neuron> GetSortedExcitedNeurons()        
        {
            if (m_FiredNeuronsCount == 0)
                return null;

            BinarySortedList<Neuron> SortedExcitedNeurons = new BinarySortedList<Neuron>(new KSNeuronLib.NeuronComparerByExcitementDesc());
            //BinarySortedList<Neuron> SortedExcitedNeurons = new BinarySortedList<Neuron>(new KSNeuronLib.NeuronComparerByEnergyAndExcitementDesc());
            //SortedSet<KSNeuronLib.Neuron> SortedExcitedNeurons = new SortedSet<KSNeuronLib.Neuron>(new KSNeuronLib.NeuronComparerByEnergyAndExcitementDesc());
            //List<KSNeuronLib.Neuron> SortedExcitedNeurons = new List<Neuron>();

            //foreach (KSNeuronLib.Neuron n in FiredNeurons)
            for (int i = 0; i < m_FiredNeuronsCount; i++)
            {
                Neuron n = m_FiredNeurons[i];
                if (n == null)  // How can this ever happen ?? it does !
                    continue;

                if (!n.IsEmptyNeuron && n.NeuronType != KSNeuronLib.Neuron.NeuronTypes.Interneuron
                        && n.FireState != KSNeuronLib.Neuron.FireStates.autofire
                        && n.Excitement > 0.01f)
                    SortedExcitedNeurons.Add(n);                
            }

            //SortedExcitedNeurons.Sort(new KSNeuronLib.NeuronComparerByExcitementDesc());
                        
            return SortedExcitedNeurons;
        }

        private void RunProcess(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                Process();
                globals.m_Iteration++;                

                if (IterationFinished != null && !ct.IsCancellationRequested)
                {
                    IterationFinished(this, EventArgs.Empty);
                }
            }                        
        }

        public void Cancel()
        {            
            if (m_Task != null && m_TokenSource != null)
            {
                m_TokenSource.Cancel();

                try
                {
                    m_Task.Wait(m_TokenSource.Token);
                }
                //catch (OperationCanceledException)
                catch (Exception)
                {                    
                }                
            }

            m_Cancel = true;

            m_IsRunning = false;

            globals.SecondsRun += ((TimeSpan)DateTime.Now.Subtract(m_RunStartTime)).TotalSeconds;

            if (RunStopped != null)
                RunStopped(this, EventArgs.Empty);
        }

        public void ResetCancel()
        {
            m_Cancel = false;
        }

        public bool IsRunning
        {
            get
            {
                return m_IsRunning;
            }
        }

        public void Clear()
        {
            if (m_Clusters != null)
            {
                lock (m_Clusters.SyncObject)
                {
                    m_Clusters.Clear();
                }
            }            

            if (m_FiredNeurons != null && (!m_IsDisposing || IsDisposed))
            {
                for (int i = 0; i < m_FiredNeurons.Length; i++)
                    m_FiredNeurons[i] = null;
            }

            globals.CachedNeuronCount = -1;
        }

        public void ResetFireStates(bool ResetExcitement)
        {
            if (m_Parallel)
            {
                lock (m_Clusters.SyncObject)
                {
                    Parallel.ForEach<Cluster>(m_Clusters, delegate(Cluster cluster, ParallelLoopState parallelState)
                    {
                        foreach (Neuron n in cluster.Neurons)
                            n.ResetFireState(ResetExcitement);
                    });
                }
            }
            else
            {
                foreach (Cluster cluster in m_Clusters)
                    foreach (Neuron n in cluster.Neurons)
                        n.ResetFireState(ResetExcitement);
            }
        }

        public void DeleteUnderperformingSynapses()
        {
            lock (m_Clusters.SyncObject)
            {
                Parallel.ForEach<Cluster>(m_Clusters, delegate(Cluster cluster, ParallelLoopState parallelState)
                {
                    foreach (Neuron n in cluster.Neurons)
                        n.DeleteUnderperformingSynapses();
                });
            }
        }

        //public void ResetActivityLevels()
        //{
        //    foreach (Neuron n in m_Neurons)
        //    {
        //        n.Excitement = -1.0; // sets it to 0
        //    }
        //}

        public void Reward(float factor, double ActivityThreshold)
        {
            lock (m_Clusters.SyncObject)
            {
                foreach (Cluster cluster in m_Clusters)
                {
                    foreach (Neuron n in cluster.Neurons)
                    {
                        if (n.Excitement > ActivityThreshold && n.FireState != Neuron.FireStates.autofire)
                        {
                            foreach (Synapse s in n.Synapses)
                            {
                                //if (s.SynapseType == SynapseTypes.excite)
                                s.Emphasis *= factor;
                            }

                            // Reset ActivityLevel
                            //n.Excitement = 0.0;
                        }
                    }
                }
            }
        }        

        public void Punish(float factor, double ActivityThreshold)
        {
            lock (m_Clusters.SyncObject)
            {
                foreach (Cluster cluster in m_Clusters)
                {
                    foreach (Neuron n in cluster.Neurons)
                    {
                        if (n.Excitement > ActivityThreshold && n.FireState != Neuron.FireStates.autofire)
                        {
                            foreach (Synapse s in n.Synapses)
                            {
                                //if (s.SynapseType == SynapseTypes.excite)
                                s.Emphasis /= factor;
                            }

                            // Reset ActivityLevel
                            //n.Excitement = 0.0;
                        }
                    }
                }
            }
        }        
        
                
        //public void AddEmptyNeurons(int Count)
        //{
        //    //return;

        //    //int StartNeuronsCount = m_Neurons.Count;
        //    //int iFirst = 0;
        //    //int iLast = StartNeuronsCount;

        //    //for (int i = 0; i < Count; i++)
        //    //{
        //    //    Neuron neuron = new Neuron();                
        //    //    neuron.IsEmptyNeuron = true;
        //    //    this.Neurons.Add(neuron);
        //    //}            
        //}        

        // XML Serialization

        public void BeforeDeserialization()
        {            
        }        

        public void AfterDeserialization()
        {
            if (m_Clusters == null)
                return;

            // ** Rewire Synapses **
            foreach (Cluster cluster in m_Clusters)
            {
                foreach (Neuron neuron in cluster.Neurons)
                {
                    foreach (Synapse synapse in neuron.Synapses)
                    {
                        synapse.Axon = m_Clusters.FindNeuronByKey(synapse.AxonKey);                        
                    }
                }
            }            

            CalcConnectionCount();
            CalcTotalEmphasis();
        }


        /// <summary>
        /// Save data to XML file
        /// </summary>
		/***
        public void SerializeDataXML(string fileName)
        {                        
            BeforeDeserialization();

            XmlSerializer serializer = null;
            TextWriter stream = null;

            try
            {
                serializer = new XmlSerializer(this.GetType());
                stream = new StreamWriter(fileName);
                serializer.Serialize(stream, this);
            }
            catch (Exception ex)
            {                
                throw ex;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }                
            }
        }
               
        public static Brain DeSerializeDataXML(string fileName)
        {                        
            XmlSerializer serializer = null;
            TextReader stream = null;

            Brain retBrain = null;
            
            try
            {
                serializer = new XmlSerializer(typeof(KSNeuronLib.Brain));
                stream = new StreamReader(fileName);
                retBrain = (Brain)serializer.Deserialize(stream);
            }
            catch (Exception ex)
            {                
                throw ex;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }                
            }

            return retBrain;            
        }        
		***/

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
        
        ~Brain()
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
            if (m_Task != null)
            {
                if (m_IsRunning)
                    Cancel();

                //m_Task.Dispose();
                //m_TokenSource.Dispose();                        
            }

            Clear();
        }

        protected virtual void OnFinalizerCalled()
        {
            //System.Diagnostics.Debug.WriteLine(String.Format("An instance of {0} was not disposed before garbage collection.", this.GetType().FullName));
        }        
    }
}

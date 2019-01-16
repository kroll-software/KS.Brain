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
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using KS.Foundation;
using Newtonsoft.Json;


namespace KS.Brain
{    		
	public class BrainConfiguration
	{
		public enum HebbianModes 
		{
			Simple,
			Complex
		}

		public BrainConfiguration() {}

		public HebbianModes HebbianMode { get; set; }
		public int NumInputClasses { get; set; }
		public int NumInputClassNeurons { get; set; }
		public int NumHiddenLayers { get; set; }
		public int NeuronsPerHiddenLayer { get; set; }	
		public int HiddenLayerMaxSynapses { get; set; }	
		public int NumOutputClasses { get; set; }
		public int NumOutputClassNeurons { get; set; }
		public int OutputLayerMaxSynapses { get; set; }
	}

    public class Brain
    {		
		public enum BrainModes
		{
			Awake,
			Dreaming,
			Generating
		}

		[JsonIgnore]
        public readonly object SyncObject = new object();
                
		public bool ParallelExecution { get; set; }
        
		public long Iteration { get; set; }

		[JsonIgnore]
		public string AutosavePath { get; set; }

		[JsonIgnore]
		public long AutosaveIntervalSeconds { get; set; }

		[JsonIgnore]
		public DateTime LastAutosaveDate { get; private set; }
        
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
        
        public float AutoFireFactor
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
			        
		public bool EnableWhatFiresTogetherWiresTogether { get; set; }
                
		public LayerCollection Layers { get; private set; }
        
		private int ProcessorCount = 8;

        public Brain()
        {
            ProcessorCount = Environment.ProcessorCount;
			Layers = new LayerCollection ();
        }			        

		public BrainConfiguration Configuration { get; set; }

		public void InitBrain(BrainConfiguration configuration)
        {            
			Configuration = configuration;

			int layerIndex = 0;
			Layers.Add(new InputLayer(layerIndex++));
			for (int i = 0; i < configuration.NumHiddenLayers; i++)
				Layers.Add(new HiddenLayer(layerIndex++));
			Layers.Add(new OutputLayer(layerIndex++));
			Layers.InitLayers(configuration);

            CalcConnectionCount();
            CalcTotalEmphasis();
        }

		public IEnumerable<Neuron> Neurons
		{
			get{
				return Layers.SelectMany (l => l.Neurons);
			}
		}

		private BrainModes m_BrainMode = BrainModes.Awake;
		[JsonIgnore]
		public BrainModes BrainMode
		{
			get {
				return m_BrainMode;
			}
			set {
				if (m_BrainMode != value) {
					m_BrainMode = value;
					switch (m_BrainMode) {
					case BrainModes.Awake:
						globals.GlobalAwakeness = 1;
						Neurons.ForEach (n => n.Awake ());            
						EnableWhatFiresTogetherWiresTogether = false;
						break;
					case BrainModes.Dreaming:
						globals.GlobalAwakeness = -1;
						EnableWhatFiresTogetherWiresTogether = true;
						break;
					case BrainModes.Generating:
						globals.GlobalAwakeness = 0;
						EnableWhatFiresTogetherWiresTogether = false;
						break;
					}
				}
			}
		}			       
			
		public void AddConnectionRandomDirection(Neuron neuron1, Neuron neuron2, float EmphasisIncrease = 0.2f)
		{
			if (neuron1 == null || neuron2 == null)
				return;

			// Suppress connections between InterNeurons
			if (neuron1.NeuronType == NeuronTypes.Interneuron && neuron2.NeuronType == NeuronTypes.Interneuron)
				return;

			/***
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
			***/

			if (neuron1.FireState == FireStates.AutoFire)
				AddConnection(neuron2, neuron1, EmphasisIncrease);
			else if (neuron2.FireState == FireStates.AutoFire)
				AddConnection(neuron1, neuron2, EmphasisIncrease);
			else if (neuron1.InvokedIteration == neuron2.InvokedIteration)
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
		/*** ***/

        public void AddConnection(Neuron transmitter, Neuron receptor)
        {
            AddConnection(transmitter, receptor, 0.2f, false);
        }

        public void AddConnection(Neuron transmitter, Neuron receptor, float EmphasisIncrease)
        {
            AddConnection(transmitter, receptor, EmphasisIncrease, false);
        }

        public void AddConnection(Neuron transmitter, Neuron receptor, float EmphasisIncrease, bool force)
        {
			//if (ReferenceEquals(transmitter, receptor) || transmitter == null)
			if (transmitter == receptor || transmitter == null)
                return;

			if (transmitter.FireState == FireStates.AutoFire)
				return;

			if (transmitter.AxonLength < Math.Abs (transmitter.Column - receptor.Column))
				return;

			//if (receptor.Rank != transmitter.Rank + 1)
			//	this.LogWarning ("receptor.Rank != transmitter.Rank + 1");

			// Suppress connections between InterNeurons
			if (transmitter.NeuronType == NeuronTypes.Interneuron && receptor.NeuronType == NeuronTypes.Interneuron)
				return;            

			if (transmitter.FireState == FireStates.AutoFire && (receptor.FireState == FireStates.AutoFire || receptor.NeuronType == NeuronTypes.MirrorNeuron))
				return;

			/****
			if (transmitter.NeuronType == Neuron.NeuronTypes.Interneuron || receptor.NeuronType == Neuron.NeuronTypes.Interneuron)
				EmphasisIncrease *= 0.382f;  // golden ratio			
			***/

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
            
			// respect circular references
			if (synapse == null && receptor.ConnectionType == ConnectionTypes.LocalLayer)
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

			if (synapse == null) {
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
			} else {
                //lock (synapse.SyncLock)	// Don't lock here !!
                //{
                if (globals.MaxEmphasis == 0 || synapse.Emphasis < globals.MaxEmphasis)
                {
                    float OldEmphasis = synapse.Emphasis;
                    //synapse.Emphasis += EmphasisIncrease;
					synapse.IncreaseEmphasis (EmphasisIncrease);
                    globals.IncreaseTotalEmphasis((double)(synapse.Emphasis - OldEmphasis));
                }
                //}                
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

			if (ParallelExecution)
            {
				lock (Layers.SyncObject)
                {
					count = Layers.AsParallel().Sum(C => C.Neurons.Sum(N => N.Synapses.Count));
                }
            }
            else
            {
				foreach (IBrainLayer layer in Layers) {
					foreach (Neuron neuron in layer.Neurons)
						count += neuron.Synapses.Count;
				}
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

			if (ParallelExecution)
            {
				lock (Layers.SyncObject)
                {
					value = Layers.AsParallel().Sum(C => C.Neurons.Sum(N => N.Synapses.Sum(S => (float)S.Emphasis)));
                }
            }
            else
            {
				foreach (IBrainLayer layer in Layers) {
					foreach (Neuron neuron in layer.Neurons) {
						foreach (Synapse s in neuron.Synapses)
							value += (float)s.Emphasis;
					}
				}
            }

            globals.m_TotalEmphasis = value;
        }


        public void SetRandomOutput(double quote)
        {
			if (Layers == null || Layers.Count < 1)
				return;

			lock (Layers)
            {
				foreach (IBrainLayer layer in Layers) {
					foreach (Neuron neuron in layer.Neurons) {
						if (ThreadSafeRandom.NextDouble () < quote)
							neuron.InvokeFire (this.Iteration);
					}
				}
            }
        }

		public void SetRandomInput(double quote)
		{
			if (Layers == null || Layers.Count < 1 || !(Layers [0] is InputLayer)) {
				this.LogWarning ("Unable to set random output, a first layer must be of type InputLayer");
				return;
			}

			lock (Layers)
			{				
				foreach (Neuron neuron in Layers[0].Neurons) {
					if (ThreadSafeRandom.NextDouble () < quote)
						neuron.InvokeFire (this.Iteration);
				}
			}
		}			

		public void WhatFiresTogetherWiresTogether()
		{
			switch (Configuration.HebbianMode) {
			case BrainConfiguration.HebbianModes.Complex:
				ComplexWhatFiresTogetherWiresTogether ();
				break;
			default:
				SimpleWhatFiresTogetherWiresTogether ();
				break;
			}
		}

		private void ComplexWhatFiresTogetherWiresTogether()
		{
			if (!EnableWhatFiresTogetherWiresTogether)
				return;			

			if (ParallelExecution) {				
				Parallel.For(1, Layers.Count, i => {
					IBrainLayer layerRecieve = Layers[i];

					for (int m = 0; m < layerRecieve.FiredNeurons.Count; m++) {
						Neuron receptor = layerRecieve.FiredNeurons[m];

						IBrainLayer layerTransmit = layerRecieve;
						int startIndex = 0;

						switch (receptor.ConnectionType) {
						case ConnectionTypes.LocalLayer:
							startIndex = m + 1;
							break;						
						case ConnectionTypes.PreviousLayer:
							layerTransmit = Layers[i - 1];
							break;						
						default:
							continue;
						}

						for (int n = startIndex; n < layerTransmit.FiredNeurons.Count; n++) {
							Neuron transmitter = layerTransmit.FiredNeurons[n];
							if (receptor.Rank == transmitter.Rank)
								AddConnectionRandomDirection(transmitter, receptor);							
							else if (transmitter.ConnectionType == ConnectionTypes.LocalLayer || transmitter.ConnectionType == ConnectionTypes.None)
								AddConnection(transmitter, receptor);
						}
					}
				});
			}
			else
			{                
				for (int i = 1; i < Layers.Count; i++)
				{
					IBrainLayer layerRecieve = Layers[i];

					for (int m = 0; m < layerRecieve.FiredNeurons.Count; m++) {
						Neuron receptor = layerRecieve.FiredNeurons[m];

						IBrainLayer layerTransmit = layerRecieve;
						int startIndex = 0;

						switch (receptor.ConnectionType) {
						case ConnectionTypes.LocalLayer:
							startIndex = m + 1;
							break;						
						case ConnectionTypes.PreviousLayer:
							layerTransmit = Layers[i - 1];
							break;						
						default:
							continue;
						}

						for (int n = startIndex; n < layerTransmit.FiredNeurons.Count; n++) {
							Neuron transmitter = layerTransmit.FiredNeurons[n];
							if (receptor.Rank == transmitter.Rank)
								AddConnectionRandomDirection(transmitter, receptor);							
							else if (transmitter.ConnectionType == ConnectionTypes.LocalLayer || transmitter.ConnectionType == ConnectionTypes.None)
								AddConnection(transmitter, receptor);
						}
					}
				}               
			}            
		}


        private void SimpleWhatFiresTogetherWiresTogether()
        {
            if (!EnableWhatFiresTogetherWiresTogether)
                return;			

			if (ParallelExecution)
			{				
				Parallel.For(0, Layers.Count - 1, i =>
					{
						IBrainLayer layerTransmit = Layers[i];
						IBrainLayer layerRecieve = Layers[i + 1];

						for (int m = 0; m < layerTransmit.FiredNeurons.Count; m++) {
							Neuron neuron1 = layerTransmit.FiredNeurons[m];
							for (int n = 0; n < layerRecieve.FiredNeurons.Count; n++) {
								Neuron neuron2 = layerRecieve.FiredNeurons[n];
								AddConnection(neuron1, neuron2, 0.2f, false);
							}
						}
					});
			}
			else
			{                
				for (int i = 0; i < Layers.Count - 1; i++)
				{
					IBrainLayer layerTransmit = Layers[i];
					IBrainLayer layerRecieve = Layers[i + 1];

					for (int m = 0; m < layerTransmit.FiredNeurons.Count; m++) {
						Neuron neuron1 = layerTransmit.FiredNeurons[m];
						for (int n = 0; n < layerRecieve.FiredNeurons.Count; n++) {
							Neuron neuron2 = layerRecieve.FiredNeurons[n];
							AddConnection(neuron1, neuron2, 0.2f, false);
						}
					}
				}               
			}
        }					       

        public void Process()
        {                                    
			if (ParallelExecution)
            {
				if (Layers.Count >= ProcessorCount * 2)
                {
					lock (Layers.SyncObject)
                    {
						Parallel.ForEach(Layers, layer =>
                        {
                            for (int i = 0; i < layer.Count; i++)
								layer[i].Listen();								
                        });
                    }

                    if (!m_Cancel)
                    {
						lock (Layers.SyncObject)
                        {
                            Parallel.ForEach(Layers, layer =>
                            {
								layer.FiredNeurons.ClearFast();

								for (int i = 0; i < layer.Count; i++) {										
									Neuron neuron = layer[i];
									neuron.Fire();
									//if (Math.Abs (neuron.AxonOutput) >= 3f && (neuron.FireState == Neuron.FireStates.fire || (neuron.FireState == Neuron.FireStates.autofire && neuron.Synapses.Count == 0)))
									if (Math.Abs (neuron.AxonOutput) >= 3f && (neuron.FireState == FireStates.Fire || neuron.FireState == FireStates.AutoFire))
										layer.FiredNeurons.Add(neuron);
								}
                            });
                        }
                    }
                }
                else
                {
					lock (Layers.SyncObject)
                    {
                        foreach (IBrainLayer layer in Layers)
                        {
							Parallel.ForEach (layer.Neurons, neuron => neuron.Listen ());
                        }
                    }

                    if (!m_Cancel)
                    {
						lock (Layers.SyncObject)
                        {
                            foreach (IBrainLayer layer in Layers)
                            {
								layer.FiredNeurons.ClearFast();

								Parallel.ForEach (layer.Neurons, neuron => {
									neuron.Fire ();
									//if (Math.Abs (neuron.AxonOutput) >= 3f && (neuron.FireState == Neuron.FireStates.fire || (neuron.FireState == Neuron.FireStates.autofire && neuron.Synapses.Count == 0)))
									if (Math.Abs (neuron.AxonOutput) >= 3f && (neuron.FireState == FireStates.Fire || neuron.FireState == FireStates.AutoFire))
										layer.FiredNeurons.Add(neuron);
								});
                            }
                        }
                    }
                }                                   
                
                if (!m_Cancel)                
                    WhatFiresTogetherWiresTogether();                                
            }
            else
            {   // Not parallel
                foreach (IBrainLayer layer in Layers)
                {
					if (layer is OutputLayer) {
						int iTest = 0;
					}

                    foreach (Neuron neuron in layer.Neurons)
                    {
                        neuron.Listen();                        
                    }
                }

                if (!m_Cancel)
                {
                    foreach (IBrainLayer layer in Layers)
                    {
						layer.FiredNeurons.ClearFast ();

                        foreach (Neuron neuron in layer.Neurons)
                        {
							neuron.Fire ();
							//if (Math.Abs (neuron.AxonOutput) >= 3f && (neuron.FireState == Neuron.FireStates.fire || (neuron.FireState == Neuron.FireStates.autofire && neuron.Synapses.Count == 0)))
							if (Math.Abs (neuron.AxonOutput) >= 3f && (neuron.FireState == FireStates.Fire || neuron.FireState == FireStates.AutoFire))
								layer.FiredNeurons.Add(neuron);
                        }							
                    }

                    if (!m_Cancel)
                        WhatFiresTogetherWiresTogether();
                }                
            }            
        }


        // Long Running        

        public event EventHandler IterationFinished;
		public event EventHandler InvalidateBrain;
        public event EventHandler RunStarted;
        public event EventHandler RunStopped;
		public event EventHandler BeforeAutosave;
		public event EventHandler AfterAutosave;

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
                    if (Layers == null)
                        globals.CachedNeuronCount = 0;
                    else
                        globals.CachedNeuronCount = Layers.NeuronCount;
                }

                return globals.CachedNeuronCount;
            }
        }

        private DateTime m_RunStartTime = DateTime.MinValue;

        public void Run()
        {
            m_IsRunning = true;
            m_Cancel = false;			           

            m_RunStartTime = DateTime.Now;
			LastAutosaveDate = DateTime.Now;
			//globals.CachedNeuronCount = -1;

            m_TokenSource = new CancellationTokenSource();
            // Create a cancellation token from CancellationTokenSource
            CancellationToken cToken = m_TokenSource.Token;
            // Create a task and pass the cancellation token
			m_Task = System.Threading.Tasks.Task.Factory.StartNew(() => RunProcess(cToken), TaskCreationOptions.LongRunning);

			if (RunStarted != null)
				RunStarted(this, EventArgs.Empty);
        }        
			
        private void RunProcess(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
				try {
					Process();	
				} catch (Exception ex) {
					ex.LogError ("Process()");
				}
                
                globals.m_Iteration++;                

				if (!ct.IsCancellationRequested) {                
					if (IterationFinished != null)                
	                    IterationFinished(this, EventArgs.Empty);                

					if (InvalidateBrain != null)
						InvalidateBrain(this, EventArgs.Empty);					
				}			

				if (!String.IsNullOrEmpty (AutosavePath) && AutosaveIntervalSeconds > 0
					&& (DateTime.Now - LastAutosaveDate).TotalSeconds > AutosaveIntervalSeconds) {
					LastAutosaveDate = DateTime.Now;

					if (BeforeAutosave != null)
						BeforeAutosave (this, EventArgs.Empty);

					this.SerializeData (AutosavePath);

					if (AfterAutosave != null)
						AfterAutosave (this, EventArgs.Empty);

					LastAutosaveDate = DateTime.Now;
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
            if (Layers != null)
            {
                lock (Layers.SyncObject)
                {
                    Layers.Clear();
                }
            }            

            globals.CachedNeuronCount = -1;
        }

        public void ResetFireStates(bool ResetExcitement)
        {
			if (ParallelExecution)
            {
                lock (Layers.SyncObject)
                {
					Parallel.ForEach<IBrainLayer>(Layers, delegate(IBrainLayer layer, ParallelLoopState parallelState)
                    {
                        foreach (Neuron n in layer.Neurons)
                            n.ResetFireState(ResetExcitement);
                    });
                }
            }
            else
            {
				foreach (IBrainLayer layer in Layers)
                    foreach (Neuron n in layer.Neurons)
                        n.ResetFireState(ResetExcitement);
            }
        }

        public void DeleteUnderperformingSynapses()
        {
			lock (Layers.SyncObject)
            {
				Parallel.ForEach<IBrainLayer>(Layers, delegate(IBrainLayer layer, ParallelLoopState parallelState)
                {
                    foreach (Neuron n in layer.Neurons)
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
			lock (Layers.SyncObject)
            {
				foreach (IBrainLayer layer in Layers)
                {
                    foreach (Neuron n in layer.Neurons)
                    {
                        if (n.Excitement > ActivityThreshold && n.FireState != FireStates.AutoFire)
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
			lock (Layers.SyncObject)
            {
				foreach (IBrainLayer layer in Layers)
                {
                    foreach (Neuron n in layer.Neurons)
                    {
                        if (n.Excitement > ActivityThreshold && n.FireState != FireStates.AutoFire)
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
		        

		[OnSerializing]
		internal void OnSerializingMethod(StreamingContext context)
		{
		}

        /// <summary>
        /// Save data to XML file
        /// </summary>
        public void SerializeData(string fileName)
        {                                    
			try {
				this.LogDebug("Before Serializing..");

				string tempFileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + ".tmp");

				lock(SyncObject) {
					using (FileStream fileStream = new FileStream(tempFileName, FileMode.Create))
					{
						this.SerializeToStream(fileStream);
					}
				}

				File.Delete(fileName);
				File.Move(tempFileName, fileName);

				this.LogDebug("Serializing completed.");
			} catch (Exception ex) {
				ex.LogError ();
			}
        }


		//[OnDeserialized]
		//internal void OnDeserializedMethod(StreamingContext context)
		public void AfterDeserialization()
		{
			foreach (IBrainLayer layer in Layers)
			{
				foreach (Neuron neuron in layer.Neurons)
				{
					foreach (Synapse synapse in neuron.Synapses)
					{
						synapse.Axon = Layers.FindNeuronByKey(synapse.AxonKey);                        
					}
				}					
			}

			Layers.ForEach(layer => layer.AfterDeserialization (Configuration));

			CalcConnectionCount();
			CalcTotalEmphasis();
		}

        public static Brain DeSerializeData(string fileName)
        {   			
			try {
				if (!File.Exists(fileName)) {
					fileName.LogWarning("File not found: {0}", fileName);
					return null;
				}

				Brain retBrain = null;

				using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
				{
					retBrain = JsonObjects.Deserialize<Brain>(fileStream);
				}
					
				if (retBrain != null) {					
					retBrain.AfterDeserialization();
					retBrain.AutosavePath = fileName;
					retBrain.AutosaveIntervalSeconds = 600;
					return retBrain;
				}					
			} catch (Exception ex) {
				ex.LogError ();
			}

			return null;
        }

		private bool IsClosing = false;
		public void Close()
		{
			IsClosing = true;
			Cancel();
			Clear();
		}

        ~Brain()
        {
			Close ();
        }        
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KS.Foundation;

namespace KS.Brain
{
    public class BrainNeuronEventArgs : EventArgs
    {
        protected Neuron m_Neuron = null;

        public BrainNeuronEventArgs(Neuron neuron)
        {
            this.m_Neuron = neuron;
        }

        public Neuron Neuron
        {
            get
            {
                return m_Neuron;
            }
        }
    }
}

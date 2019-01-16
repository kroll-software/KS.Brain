using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KS.Foundation;

namespace KS.Brain
{
    public static class NeuronLibExtensions
    {
        public static double WordCloudValue(this Neuron neuron)
        {
            return 3 + ((neuron.Energy + neuron.Excitement) * 20);
        }
    }
}

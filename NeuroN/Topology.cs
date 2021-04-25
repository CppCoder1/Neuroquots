using System;
using System.Collections.Generic;
using System.Text;

namespace ner
{
    class Topology
    {
        public int InputCount { get; set; }

        public int OutputCount { get; set;  }

        public double LearningRate { get; set; }

        public List<int> HiddenLayers { get; set; }

        public Topology(int inputCount, int outputCount,double learningRate, params int[] layers)
        {
            InputCount = inputCount;

            OutputCount = outputCount;

            LearningRate = learningRate;

            HiddenLayers = new List<int>();

            HiddenLayers.AddRange(layers);
        }
    }
}

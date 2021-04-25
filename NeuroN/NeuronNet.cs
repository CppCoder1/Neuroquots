using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ner
{
    class NeuronNet
    {
        public List<Layer> Layers { get; }

        public Topology Topology { get; }

        public NeuronNet(Topology topology)
        {
            Layers = new List<Layer>();
            Topology = topology;
            CreateInputLayers();
            CreateOutputLayers();
            CreateHiddenLayers();
        }

        public Neuron FeedForward(params double[] inputSignals)
        {
            SetSignalsToInputNeurons(inputSignals);
            FeedForwardsAllLayersAfterInputs();
            if(Topology.OutputCount == 1)
            {
                return Layers.Last().Neurons[0];
            }
            else
            {
                return Layers.Last().Neurons.OrderByDescending(n => n.Output).First();
            }
        }

        private void FeedForwardsAllLayersAfterInputs()
        {
            for (int i = 1; i < Layers.Count; i++)
            {
                var layer = Layers[i];
                var previousLayerSignals = Layers[i - 1].GetSignals();

                foreach (var neuron in layer.Neurons)
                {
                    neuron.FeedForward(previousLayerSignals);
                }
            }
        }

        private double SetSignalsToInputNeurons(params double[] inputSignals)
        {
            for (int i = 0; i < inputSignals.Length - 1; i++)
            {
                var signal = new List<double>() { inputSignals[i] };
                var neuron = Layers[0].Neurons[i];

                neuron.FeedForward(signal);
            }
            return 0;
        }

        public double Learn(double[] expected,double[,] inputs, int epoch)
        {
            var error = 0.0;
            for (int i = 0; i < epoch; i++)
            {
                for (int j = 0; j < expected.Length; j++)
                {
                    var output = expected[j];
                    var input = GetRow(inputs, j);
                    error += Backpropagation(output, input);
                }
            }
            var result = error / epoch;
            return result;
        }

        public static double[] GetRow(double[,] matrix, int row)
        {
            var columns = matrix.GetLength(1);
            var array = new double[columns];
            for (int i = 0; i < columns; ++i)
                array[i] = matrix[row, i];
            return array;
        }

        private double Backpropagation(double expected, params double[] inputs)
        {
            var actual = FeedForward(inputs).Output;

            var difference = actual - expected;

            foreach(var neuron in Layers.Last().Neurons)
            {
                neuron.Learn(difference, Topology.LearningRate);
            }

            for(int i = Layers.Count - 2; i >= 0; i--)
            {
                var layer = Layers[i];
                var previousLayer = Layers[i + 1];
                for(int j = 0; j < layer.NeuronCount; j++)
                {
                    var neuron = layer.Neurons[j];

                    for(int k = 0; k < previousLayer.NeuronCount; k++)
                    {
                        var previousNeuron = previousLayer.Neurons[k];

                        var error = previousNeuron.Weights[j] * previousNeuron.Delta;

                        neuron.Learn(error, Topology.LearningRate);
                    }
                }
            }
            var result = Math.Pow(difference, 2);
            return result;
        }

        private void CreateHiddenLayers()
        {
            for (int j = 0; j < Topology.HiddenLayers.Count; j++) 
            {
            var lastLayer = Layers.Last();
            var hiddenNeurons = new List<Neuron>();
            for (int i = 0; i < Topology.HiddenLayers[j]; i++)
            {
                var neuron = new Neuron(lastLayer.NeuronCount);
                hiddenNeurons.Add(neuron);
            }
            var hiddenLayer = new Layer(hiddenNeurons);
            Layers.Add(hiddenLayer);
            }
        }

        private void CreateOutputLayers()
        {
            var lastLayer = Layers.Last();
            var outputNeurons = new List<Neuron>();
            for (int i = 0; i < Topology.OutputCount; i++)
            {
                var neuron = new Neuron(lastLayer.NeuronCount, NeuronType.Output);
                outputNeurons.Add(neuron);
            }
            var outputLayer = new Layer(outputNeurons, NeuronType.Output);
            Layers.Add(outputLayer);
        }

        private void CreateInputLayers()
        {
            var inputNeurons = new List<Neuron>();
            for (int i = 0; i < Topology.InputCount; i++)
            {
                var neuron = new Neuron(1, NeuronType.Input);
                inputNeurons.Add(neuron);
            }
            var inputLayer = new Layer(inputNeurons, NeuronType.Input);
            Layers.Add(inputLayer);
        }

        private double[,] Scaling(double[,] inputs)
        {
            var result = new double[inputs.GetLength(0), inputs.GetLength(1)];

            for(int colum = 0; colum < inputs.GetLength(1); colum++)
            {
                var min = inputs[0, colum];
                var max = inputs[0, colum];

                for(int row = 1; row < inputs.GetLength(0); row++)
                {
                    var Item = inputs[row, colum];
                    if(Item < min)
                    {
                        min = Item;
                    }
                    if(Item > max)
                    {
                        max = Item;
                    }
                }
                var dev = max - min;
                for(int row = 1; row < inputs.GetLength(0); row++)
                {
                    result[row, colum] = (inputs[row, colum] - min) / dev;                 
                }
            }
            return result;
        }

        private double[,] Normalization(double[,] inputs)
        {
            var result = new double[inputs.GetLength(0), inputs.GetLength(1)];

            for (int colum = 0; colum < inputs.GetLength(1); colum++)
            {
                //Среднее значение сигналов нейрона
                var sum = 0.0;
                for (int row = 0; row < inputs.GetLength(0); row++)
                {
                    sum += inputs[row, colum];
                }

                var average = sum / inputs.GetLength(0);
                //Стандартное квадратичное отклонение нейрона
                var error = 0.0;

                for (int row = 0; row < inputs.GetLength(0); row++)
                {
                    error += Math.Pow((inputs[row, colum] - average), 2);
                }

                var standartError = Math.Sqrt(error / inputs.GetLength(0));

                for (int row = 0; row < inputs.GetLength(0); row++)
                {
                    result[row, colum] = (inputs[row, colum] - average) / standartError;
                }
            }
            return result;
        }
    }
}

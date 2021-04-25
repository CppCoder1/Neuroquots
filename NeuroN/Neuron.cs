using System;
using System.Collections.Generic;
using System.Text;

namespace ner
{
    class Neuron
    {
        public List <double> Weights { get; }

        public List<double> Inputs { get; }

        public double Delta { get; private set; }
         
        public NeuronType NeuronType { get; }

        public double Output { get; private set; }

        public Neuron(int inputCount, NeuronType type = NeuronType.Normal)
        {
            NeuronType = type;
            Weights = new List<double>();
            Inputs = new List<double>();
            InitWeightsRandomValues(inputCount);
        }

        private void InitWeightsRandomValues(int inputCount)
        {
            var rnd = new Random();
            for (int i = 0; i < inputCount; i++)
            {
                if (NeuronType == NeuronType.Input)
                {
                    Weights.Add(1);
                }
                else
                {
                    Weights.Add(rnd.NextDouble());
                }
                Inputs.Add(0);
            }
        }

        public double FeedForward(List<double> inputs)
        {
            if (inputs.Count == Weights.Count)
            {
                for(int i = 0; i < inputs.Count; i++)
                {
                    Inputs[i] = inputs[i];
                }
                var sum = 0.0;
                for (int i = 0; i < inputs.Count; i++)
                {
                    sum += inputs[i] * Weights[i];
                }
                if(NeuronType != NeuronType.Input)
                {
                    Output = Sigmoid(sum);
                }
                else
                {
                    Output = sum;
                }
                return Output;
            }
            else
            {
                Console.WriteLine("Ошибка программы, мне вас очень жаль");
                return 0;
            }
        }

        private double Sigmoid(double x)
        {
            var result = 1 / (1 + Math.Pow(Math.E, -x));
            return result;
        }

        private double SigmoidDX(double x)
        {
            var sig = Sigmoid(x);
            var result = sig / (1 - sig);
            return result;
        }

        public void Learn(double error, double learningRate)
        {
            if(NeuronType == NeuronType.Input)
            {
                return;
            }
            else
            {
                Delta = error * SigmoidDX(Output);

                for(int i = 0; i < Weights.Count; i++)
                {
                    var weight = Weights[i];

                    var input = Inputs[i];

                    var newWeight = weight - input * Delta * learningRate;

                    Weights[i] = newWeight;
                }
                
            }
        }

        public override string ToString()
        {
            return Output.ToString();
        }
    }
}

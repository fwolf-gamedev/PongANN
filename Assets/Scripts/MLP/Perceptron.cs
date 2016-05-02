using UnityEngine;
using System.Collections.Generic;

public class Perceptron
{
    public class Input
    {
        public Perceptron InputPerceptron;
        public float Weight;
    }

    public List<Input> inputList = new List<Input>();
    public float State = 0f;
    public float Error = 0f;
    public float Beta = 1f;

    private MLPNetwork network = null;

    public Perceptron(MLPNetwork net)
    {
        network = net;
    }

    public void FeedForward()
    {
        float sum = 0f;

        foreach (Input input in inputList)
        {
            sum += input.InputPerceptron.State * input.Weight;
        }

        State = ThresholdFunc(sum);
    }

    float ThresholdFunc(float input)
    {
        return 1f / (1f + Mathf.Exp(-Beta * input));
    }

    public void AdjustWeight(float currentError)
    {
        for (int i = 0; i < inputList.Count; i++)
        {
            Input input = inputList[i];
            float deltaWeight = network.Gain * currentError * input.InputPerceptron.State;
            input.Weight += deltaWeight;
            Error = currentError;
        }
    }

    public float GetIncomingWeight(Perceptron perceptron)
    {
        foreach(Input input in inputList)
        {
            if (input.InputPerceptron == perceptron)
                return input.Weight;
        }
        return 0;
    }
}

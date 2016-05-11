using UnityEngine;
using System.IO;
using System.Collections.Generic;
using WyrmTale;

/// <summary>
/// Multi Layer Perceptron Network
/// </summary>

public class MLPNetwork : MonoBehaviour
{
    public float Gain = 0.9f;
    public float InitWeightRange = 0.2f;

    public bool ComputeWeights = false;
    public bool LoadWeights = false;
    public bool DoSave = false;
    public bool DoLoad = false;

    public int NbInputPerceptron = 4;
    public int NbOutputPerceptron = 1;
    public List<int> NbPerceptronPerHiddenLayer = new List<int>();

    public List<Perceptron> InputPerceptronList = new List<Perceptron>();
    public List<Perceptron> OutputPerceptronList = new List<Perceptron>();

    public List<List<Perceptron>> HiddenLayerList = new List<List<Perceptron>>();

    // debug info / serialization
    public List<List<float>> HiddenLayerWeights = new List<List<float>>();
    public List<float> OutputLayerWeights = new List<float>();

    public int NbEpoch = 0;

    void Start()
    {
        for (int i = 0; i < NbInputPerceptron; i++)
        {
            Perceptron perceptron = new Perceptron(this);
            InputPerceptronList.Add(perceptron);
        }

        for (int i = 0; i < NbPerceptronPerHiddenLayer.Count; i++)
        {
            int nbHiddenPerceptron = NbPerceptronPerHiddenLayer[i];
            List<Perceptron> hiddenPerceptronList = new List<Perceptron>();
            List<Perceptron> prevPerceptronList = i == 0 ? InputPerceptronList : HiddenLayerList[HiddenLayerList.Count - 1];

            for (int j = 0; j < nbHiddenPerceptron; j++)
            {
                Perceptron perceptron = new Perceptron(this);

                for (int k = 0; k < prevPerceptronList.Count; k++)
                {
                    Perceptron.Input input = new Perceptron.Input();
                    input.InputPerceptron = prevPerceptronList[k];
                    input.Weight = Random.Range(-InitWeightRange, InitWeightRange);
                    perceptron.inputList.Add(input);
                }

                hiddenPerceptronList.Add(perceptron);
            }
            HiddenLayerList.Add(hiddenPerceptronList);
        }

        for (int i = 0; i < NbOutputPerceptron; i++)
        {
            Perceptron perceptron = new Perceptron(this);

            List<Perceptron> prevPerceptronList = NbPerceptronPerHiddenLayer.Count > 0 ? HiddenLayerList[NbPerceptronPerHiddenLayer.Count - 1] : InputPerceptronList;

            for (int j = 0; j < prevPerceptronList.Count; j++)
            {
                Perceptron.Input input = new Perceptron.Input();
                input.InputPerceptron = prevPerceptronList[j];
                input.Weight = Random.Range(-InitWeightRange, InitWeightRange);
                perceptron.inputList.Add(input);
            }

            OutputPerceptronList.Add(perceptron);
        }

        Debug.Log("layers init done");
    }

    void Update()
    {
        // debug
        if (ComputeWeights)
        {
            ComputeWeights = false;
            GenerateWeights();
        }
        if (LoadWeights)
        {
            LoadWeights = false;
            LoadFromWeight();
        }
        if (DoSave)
        {
            DoSave = false;
            Save();
        }
        if (DoLoad)
        {
            DoLoad = false;
            Load();
        }
    }

    public void LearnPattern(List<float> input, List<float> output)
    {
        GenerateOutput(input);
        Backpropagation(output);
        NbEpoch++;
    }

    public void GenerateOutput(List<float> inputs)
    {
        for (int i = 0; i < InputPerceptronList.Count; i++)
            InputPerceptronList[i].State = inputs[i];

        for (int i = 0; i < HiddenLayerList.Count; i++)
        {
            for (int j = 0; j < HiddenLayerList[i].Count; j++)
                HiddenLayerList[i][j].FeedForward();
        }

        for (int i = 0; i < OutputPerceptronList.Count; i++)
            OutputPerceptronList[i].FeedForward();
    }

    // generate debug info
    public void GenerateWeights()
    {
        HiddenLayerWeights.Clear();
        for (int i = 0; i < NbPerceptronPerHiddenLayer.Count; i++)
        {
            HiddenLayerWeights.Add(new List<float>());
            List<Perceptron> hiddenPerceptronList = HiddenLayerList[i];
            for (int j = 0; j < hiddenPerceptronList.Count; j++)
            {
                for (int k = 0; k < hiddenPerceptronList[j].inputList.Count; k++)
                {
                    Perceptron.Input input = hiddenPerceptronList[j].inputList[k];
                    HiddenLayerWeights[i].Add(input.Weight);
                }
            }
        }
        OutputLayerWeights.Clear();
        for (int i = 0; i < OutputPerceptronList.Count; i++)
        {
            for (int j = 0; j < OutputPerceptronList[i].inputList.Count; j++)
            {
                Perceptron.Input input = OutputPerceptronList[i].inputList[j];
                OutputLayerWeights.Add(input.Weight);
            }
        }
    }

    // TODO refactor
    public void LoadFromWeight()
    {
        InputPerceptronList.Clear();
        HiddenLayerList.Clear();
        OutputPerceptronList.Clear();

        for (int i = 0; i < NbInputPerceptron; i++)
        {
            Perceptron perceptron = new Perceptron(this);
            InputPerceptronList.Add(perceptron);
        }

        int weightCount;
        List<Perceptron> prevPerceptronList = null;

        for (int i = 0; i < NbPerceptronPerHiddenLayer.Count; i++)
        {
            HiddenLayerList.Add(new List<Perceptron>());
            weightCount = 0;
            prevPerceptronList = (i == 0) ? InputPerceptronList : HiddenLayerList[i - 1];

            int nbHiddenPerceptron = NbPerceptronPerHiddenLayer[i];

            for (int j = 0; j < nbHiddenPerceptron; j++)
            {
                Perceptron perceptron = new Perceptron(this);

                for (int k = 0; k < prevPerceptronList.Count; k++)
                {
                    Perceptron.Input input = new Perceptron.Input();
                    input.InputPerceptron = prevPerceptronList[k];
                    input.Weight = HiddenLayerWeights[i][weightCount++];
                    perceptron.inputList.Add(input);
                }

                HiddenLayerList[i].Add(perceptron);
            }
        }
        weightCount = 0;

        prevPerceptronList = (NbPerceptronPerHiddenLayer.Count == 0) ? InputPerceptronList : HiddenLayerList[HiddenLayerList.Count - 1];

        for (int i = 0; i < NbOutputPerceptron; i++)
        {
            Perceptron perceptron = new Perceptron(this);

            for (int j = 0; j < prevPerceptronList.Count; j++)
            {
                Perceptron.Input input = new Perceptron.Input();
                input.InputPerceptron = prevPerceptronList[j];
                input.Weight = OutputLayerWeights[weightCount++];
                perceptron.inputList.Add(input);
            }

            OutputPerceptronList.Add(perceptron);
        }
    }

    public List<float> GetOutputs()
    {
        List<float> outputs = new List<float>();
        for (int i = 0; i < OutputPerceptronList.Count; i++)
            outputs.Add(OutputPerceptronList[i].State);

        return outputs;
    }

    private void Backpropagation(List<float> outputs)
    {
        // adjust output perceptrons
        for (int i = 0; i < OutputPerceptronList.Count; i++)
        {
            Perceptron perceptron = OutputPerceptronList[i];
            float state = perceptron.State;

            // calculate error term (gradient)
            float error = state * (1 - state) * (outputs[i] - state);
            // adjust output perceptron weight with error
            perceptron.AdjustWeight(error);
        }

        // adjust hidden peceptrons backward
        for (int i = HiddenLayerList.Count - 1; i >= 0; i--)
        {
            List<Perceptron> nextPerceptronList = (i == (HiddenLayerList.Count - 1)) ? OutputPerceptronList : HiddenLayerList[i + 1];
            List<Perceptron> hiddenPerceptronList = HiddenLayerList[i];
            for (int j = 0; j < hiddenPerceptronList.Count; j++)
            {
                Perceptron perceptron = hiddenPerceptronList[j];
                float state = perceptron.State;

                // calculate error term
                float sum = 0;
                for (int k = 0; k < nextPerceptronList.Count; k++)
                {
                    Perceptron nextPerceptron = nextPerceptronList[k];
                    sum += nextPerceptron.GetIncomingWeight(perceptron) * nextPerceptron.Error;
                }
                float error = state * (1 - state) * sum;

                perceptron.AdjustWeight(error);
            }
        }
    }

    public JSON Serialize()
    {
        JSON result = new JSON();

        result["NbInputPerceptron"] = NbInputPerceptron;
        result["NbOutputPerceptron"] = NbOutputPerceptron;
        int nbHiddenLayers = NbPerceptronPerHiddenLayer.Count;
        result["NbHiddenLayers"] = nbHiddenLayers;
        for (int i = 0; i < nbHiddenLayers; i++)
        {
            result["NbHiddenLayers" + i.ToString()] = NbPerceptronPerHiddenLayer[i];
        }

        int count = 0;
        for (int i = 0; i < NbPerceptronPerHiddenLayer.Count; i++)
        {
            List<float> hiddenLayerWeights = HiddenLayerWeights[i];
            for (int j = 0; j < hiddenLayerWeights.Count; j++, count++)
            {
                result[count.ToString()] = hiddenLayerWeights[j];
            }
        }
        for (int i = 0; i < OutputLayerWeights.Count; i++, count++)
        {
            result[count.ToString()] = OutputLayerWeights[i];
        }

        return result;
    }

    public void Deserialize(JSON source)
    {
        HiddenLayerWeights.Clear();
        OutputLayerWeights.Clear();
        NbPerceptronPerHiddenLayer.Clear();

        NbInputPerceptron = System.Convert.ToInt32(source["NbInputPerceptron"]);
        NbOutputPerceptron = System.Convert.ToInt32(source["NbOutputPerceptron"]);
        int nbHiddenLayers = System.Convert.ToInt32(source["NbHiddenLayers"]);
        for (int i = 0; i < nbHiddenLayers; i++)
        {
            NbPerceptronPerHiddenLayer.Add(System.Convert.ToInt32(source["NbHiddenLayers" + i.ToString()]));
        }

        int count = 0;
        int nbHiddenPerceptronInLastHiddenLayer = NbInputPerceptron;
        for (int i = 0; i < nbHiddenLayers; i++)
        {
            HiddenLayerWeights.Add(new List<float>());

            int nbHiddenPerceptron = NbPerceptronPerHiddenLayer[i];
            int prevNbPerceptron = (i == 0) ? NbInputPerceptron : NbPerceptronPerHiddenLayer[i - 1];
            int nbHiddenLayerWeights = prevNbPerceptron * nbHiddenPerceptron;
            for (int j = 0; j < nbHiddenLayerWeights; j++, count++)
            {
                HiddenLayerWeights[i].Add(System.Convert.ToSingle(source[count.ToString()]));
            }
            nbHiddenPerceptronInLastHiddenLayer = nbHiddenPerceptron; // store nb hidden perceptron in this layer for later
        }

        int nbOutputLayerWeights = nbHiddenPerceptronInLastHiddenLayer * NbOutputPerceptron;
        for (int i = 0; i < nbOutputLayerWeights; i++, count++)
        {
            OutputLayerWeights.Add(System.Convert.ToSingle(source[count.ToString()]));
        }
    }

    public void Save()
    {
        GenerateWeights();

        string serializedData = Serialize().serialized;
        byte[] dataArray = System.Text.Encoding.ASCII.GetBytes(serializedData);
        File.WriteAllBytes("save", dataArray);
    }

    public void Load()
    {
        JSON saveData = new JSON();
        saveData.serialized = File.ReadAllText("save");
        Deserialize(saveData);

        LoadFromWeight();
    }
}

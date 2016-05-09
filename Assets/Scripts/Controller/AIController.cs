using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MLPNetwork))]
public class AIController : PaddleController
{
    public int UpdateFrenquency = 1;
    public float output = 0f;
    public float ErrorAcceptation = 0.1f;

    float expectedOutput;
    public float ExpectedOutput { get { return expectedOutput; } }
    float deltaError;
    public float DeltaError { get { return deltaError; } }
    public bool IsErrorAcceptable { get { return deltaError <= ErrorAcceptation; } }

    public bool IsLearning = false;

    private int frameCount = 0;

    MLPNetwork MLPNet = null;

    List<float> outputList = null;
    List<float> inputList = new List<float>();

	// Use this for initialization
	protected override void Start ()
    {
        base.Start();
        MLPNet = GetComponent<MLPNetwork>();
    }

    float wantedPosY;

    void FixedUpdate ()
    {
        if (++frameCount % UpdateFrenquency != 0)
            return;

        ProcessOutput();
        //if (IsLearning)
        //{
        //    Ball ball = GameMgr.Instance.GetBall();
        //    LearnFromBallPos(ball.transform.position);
        //}

        // deduce paddle move from output
        //wantedPosY = (output - 0.5f) * GameMgr.Instance.CourtHeight;
        wantedPosY = GameMgr.ComputeBallPosCourt(output);
    }

    void ProcessOutput()
    {
        inputList.Clear();
        Ball ball = GameMgr.Instance.GetBall();
        //// ball pos
        //inputList.Add(ball.transform.position.y);
        //// ball velocity
        //inputList.Add(rb.velocity.x);
        //inputList.Add(rb.velocity.y);

        inputList.Add(Ball.GetRoundedPos(ball.transform.position.y));
        //Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
        //inputList.Add(Ball.GetAngleInt(rb.velocity.normalized));

        if (outputList != null)
            outputList.Clear();

        MLPNet.GenerateOutput(inputList);
        outputList = MLPNet.GetOutputs();
        output = outputList[0];
    }


    void Update()
    {
        MoveToPos(wantedPosY);
    }

    public void OnPointLost(Vector3 ballPos)
    {
        if (IsLearning)
            LearnFromBallPos(ballPos);
    }

    public void OnBallCollideAIPaddle(Vector3 ballPos)
    {
        if (IsLearning)
            LearnFromBallPos(ballPos);
    }

    public void OnBallThrown()
    {
        ProcessOutput();
    }

    private void LearnFromBallPos(Vector3 ballPos)
    {
        //int courtHeight = GameMgr.Instance.CourtHeight;
        //expectedOutput = ballPos.y / courtHeight + 0.5f;
        //expectedOutput = Mathf.Max(Mathf.Min(expectedOutput, 1f), 0f);
        expectedOutput = GameMgr.ComputeBallPos0To1(ballPos.y);
        //Debug.Log(expectedOutput);

        deltaError = Mathf.Abs(expectedOutput - output);

        //Ball.BallData colParams = GameMgr.Instance.GetBall().GetBallData;
        //inputList.Clear();
        //inputList.Add(colParams.posY);
        //inputList.Add(GameMgr.ComputeBallPos0To1(colParams.posY));
        //inputList.Add(colParams.direction.x);
        //inputList.Add(colParams.direction.y);
        //inputList.Add(ballPos.x);
        //inputList.Add(ballPos.y);
        //Ball ball = GameMgr.Instance.GetBall();
        //ball.SaveBallData();

        // do learning via backpropagation
        List<float> outputs = new List<float>();
        outputs.Add(expectedOutput);
        MLPNet.LearnPattern(inputList, outputs);
    }
}

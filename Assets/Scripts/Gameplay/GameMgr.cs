using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameMgr : MonoBehaviour {

    static GameMgr instance = null;
    static public GameMgr Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<GameMgr>();
            return instance;
        }
    }

    int courtHeight = 0;
    public int CourtHeight { get { return courtHeight; } }

    int scoreP1 = 0;
    int scoreP2 = 0;

    Text score1Text;
    Text score2Text;
    // debug output text
    Text ballDataPosYText;
    Text ballDataVelocityText;
    Text outputText;
    Text expectedPosYText;
    Text deltaOutputText;

    Ball ball;
    public Ball GetBall() { return ball; }
    bool isBallLaunched = false;
    GameObject playerGao;
    GameObject aiGao;

    AIController ai;
    public AIController AI { get { return ai; } }

    public float TimeScale = 1f;
    public bool TrainingModeOn = false;
    public bool RandomDirOn = false;
    int trainingStep = 0;
    int nbTrainingSteps = 0;

    // Use this for initialization
    void Awake ()
    {
        ball = FindObjectOfType<Ball>();
        playerGao = GameObject.Find("Paddle1");
        aiGao = GameObject.Find("Paddle2");
        ai = aiGao.GetComponent<AIController>();

        score1Text = GameObject.Find("Score1").GetComponent<Text>();
        score2Text = GameObject.Find("Score2").GetComponent<Text>();

        ballDataPosYText = GameObject.Find("BallPosY").GetComponent<Text>();
        ballDataVelocityText = GameObject.Find("BallVelocity").GetComponent<Text>();
        outputText = GameObject.Find("PaddlePosY").GetComponent<Text>();
        expectedPosYText = GameObject.Find("ExpectedPosY").GetComponent<Text>();
        deltaOutputText = GameObject.Find("DeltaError").GetComponent<Text>();

        courtHeight = Mathf.RoundToInt(Camera.main.orthographicSize * 2f);
        nbTrainingSteps = Mathf.RoundToInt(courtHeight / ball.transform.localScale.y);
    }

    void Update()
    {
        Time.timeScale = TimeScale;

        if (TrainingModeOn && isBallLaunched == false)
        {
            Vector3 trainingPos = playerGao.transform.position + Vector3.right * 0.6f;

            if (ai.IsErrorAcceptable == false) // cancel training step to repeat ball trajectory
                trainingStep--;

            trainingPos.y = CourtHeight / 2f - ball.transform.localScale.y / 2f - ball.transform.localScale.y * trainingStep;
            ball.transform.position = trainingPos;
            trainingStep = (trainingStep + 1) % nbTrainingSteps;
            TryLaunchBall();
        }

        ballDataPosYText.text = ball.GetBallData.posY.ToString();
        ballDataVelocityText.text = ball.GetBallData.direction.x.ToString() + " ; " + ball.GetBallData.direction.y.ToString();
        outputText.text = ai.output.ToString();
        expectedPosYText.text = ai.ExpectedOutput.ToString();
        deltaOutputText.color = ai.IsErrorAcceptable ? Color.green : Color.red;
        deltaOutputText.text = ai.DeltaError.ToString();
    }

    void LateUpdate ()
    {
        if (TrainingModeOn == false && isBallLaunched == false)
        {
            ball.transform.position = playerGao.transform.position + Vector3.right * 0.6f;
        }
	}

    public bool IsBallLaunched()
    {
        return isBallLaunched;
    }

    public void TryLaunchBall()
    {
        if (isBallLaunched == false)
        {
            if (TrainingModeOn)
                ball.Launch(RandomDirOn, ai.IsErrorAcceptable == false);
            else
                ball.Launch();
            isBallLaunched = true;
        }
    }

    public void ToggleTrainingMode()
    {
        TrainingModeOn = !TrainingModeOn;
        playerGao.GetComponent<BoxCollider2D>().enabled = !TrainingModeOn;
    }

    public void ToggleLearningActivation()
    {
        ai.IsLearning = !ai.IsLearning;
    }

    public void OnBallExit(bool isLeftSide)
    {
        ball.Rigidbody.velocity = Vector2.zero;
        isBallLaunched = false;

        if (isLeftSide)
        {
            scoreP2++;
            score2Text.text = scoreP2.ToString();
        }
        else
        {
            scoreP1++;
            score1Text.text = scoreP1.ToString();

            ai.OnPointLost(ball.transform.position);
        }
    }
}

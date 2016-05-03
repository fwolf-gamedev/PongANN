using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour {

    public class BallData
    {
        public float posY;
        public Vector2 direction;
    }
    private BallData ballData = new BallData();
    public BallData GetBallData { get { return ballData; } }

    public float InitialSpeed = 10f;
    public float MaxSpeed = 30f;
    public float HitAcceleration = 1f;
    float currentSpeed;
    Rigidbody2D rigidBody;
    public Rigidbody2D Rigidbody { get { return GetComponent<Rigidbody2D>(); } }

	// Use this for initialization
	void Awake () {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    public void Launch(bool useRandomDir = false, bool repeatLastLaunch = false)
    {
        Vector2 dir = Vector2.right;
        if (repeatLastLaunch)
        {
            dir = ballData.direction;
            Vector3 pos = transform.position;
            pos.y = ballData.posY;
            transform.position = pos;
        }
        else if (useRandomDir)
        {
            dir.x = 1f; //Random.Range(0.5f, 1f);
            dir.y = Random.Range(0.5f, 1f);
            if (Random.Range(0, 2) == 0)
                dir.y *= -1f;
            dir.Normalize();
        }

        rigidBody.velocity = dir * InitialSpeed;
        currentSpeed = InitialSpeed;

        ballData.posY = transform.position.y;
        ballData.direction = dir;

        // $$$ Q&D
        GameMgr.Instance.AI.OnBallThrown();
    }

    void Update()
    {
        //if (rigidBody.velocity.magnitude == 0)
        //{
        //    ballData.posY = transform.position.y;
        //    ballData.direction = Vector2.right;
        //}
    }

    float ComputeHitFactor(Vector2 racketPos, float racketHeight)
    {
        return (transform.position.y - racketPos.y) / racketHeight;
    }
    
    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag != "Player")
            return;

        // deal collision with upper or lower part of bracket
        Vector3 colNormal = col.contacts[0].normal;
        if (colNormal.x == 0f)
        {
            float x = ComputeHitFactor(col.transform.position, col.collider.bounds.size.x);
            float sign = colNormal.y;
            Vector2 dir = new Vector2(x, sign).normalized;
            if (dir.magnitude > 0f)
            {
                currentSpeed = Mathf.Min(MaxSpeed, currentSpeed + HitAcceleration);
                GetComponent<Rigidbody2D>().velocity = dir * currentSpeed;
            }
        }
        // collision with front part of the bracket
        else
        {
            float y = ComputeHitFactor(col.transform.position, col.collider.bounds.size.y);
            float sign = colNormal.x;
            Vector2 dir = new Vector2(sign, y).normalized;
            if (dir.magnitude > 0f)
            {
                currentSpeed = Mathf.Min(MaxSpeed, currentSpeed + HitAcceleration);
                GetComponent<Rigidbody2D>().velocity = dir * currentSpeed;
            }
        }

        // learn from ball collision
        AIController ai = col.gameObject.GetComponent<AIController>();
        if (ai != null)
            ai.OnBallCollide(transform.position);
        else
        {
            ballData.posY = transform.position.y;
            ballData.direction = GetComponent<Rigidbody2D>().velocity.normalized;
            // $$$ Q&D
            GameMgr.Instance.AI.OnBallThrown();
        }
    }

}

using UnityEngine;
using System.Collections;

public class PaddleController : MonoBehaviour
{
    public float Speed = 8f;
    protected float Height = 1f;

    private float border = 0f;

    virtual protected void Start()
    {
        Height = transform.localScale.y;
        border = GameMgr.Instance.CourtHeight / 2f -Height / 2f;
    }

    protected void MoveUp()
    {
        if (transform.position.y < border)
            transform.Translate(Vector3.up * Speed * Time.deltaTime);
    }

    protected void MoveDown()
    {
        if (transform.position.y > -border)
            transform.Translate(Vector3.down * Speed * Time.deltaTime);
    }

    protected void MoveToPos(float wantedPosY)
    {
        wantedPosY = Mathf.Max(Mathf.Min(wantedPosY, border), -border);

        if (GameMgr.Instance.TrainingModeOn)
        {
            transform.position = new Vector2(transform.position.x, wantedPosY);
        }
        else
        {
            Vector3 toPos = new Vector3(0f, wantedPosY - transform.position.y, 0f);
            if (toPos.magnitude > 0.1f)
                toPos = toPos.normalized * Speed;

            transform.Translate(toPos * Time.deltaTime);
        }
    }

    protected void LaunchBall()
    {
        GameMgr.Instance.TryLaunchBall();
    }
}

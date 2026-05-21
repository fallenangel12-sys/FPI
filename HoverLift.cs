using UnityEngine;

public class HoverLift : MonoBehaviour
{
    [Header("Hover Settings")]
    public float liftHeight = 1f;
    public float hoverAmplitude = 0.2f;
    public float hoverSpeed = 2f;
    public float liftDuration = 2f;

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool isHovering = false;
    private float liftTimer = 0f;

    private void Start()
    {
        startPos = transform.position;
        targetPos = startPos + Vector3.up * liftHeight;
    }

    public void ActivateHover()
    {
        isHovering = true;
        liftTimer = 0f;
    }

    private void Update()
    {
        if (!isHovering) return;
        
        if (liftTimer < liftDuration)
        {
            liftTimer += Time.deltaTime;
            float t = liftTimer / liftDuration;
            t = Mathf.SmoothStep(0, 1, t);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
        }
        else
        {
            float newY = targetPos.y + Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
            transform.position = new Vector3(targetPos.x, newY, targetPos.z);
        }
    }
}

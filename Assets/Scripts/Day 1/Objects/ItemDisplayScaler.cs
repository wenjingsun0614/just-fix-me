using UnityEngine;

public class HangingBalloonFloat : MonoBehaviour
{
    [Header("Position Float")]
    public float floatAmplitudeY = 0.08f;
    public float floatSpeedY = 1.2f;

    [Header("Sway Rotation")]
    public float swayAngle = 4f;
    public float swaySpeed = 1f;

    private Vector3 startLocalPos;
    private Quaternion startLocalRot;

    void Start()
    {
        startLocalPos = transform.localPosition;
        startLocalRot = transform.localRotation;
    }

    void Update()
    {
        float offsetY = Mathf.Sin(Time.time * floatSpeedY) * floatAmplitudeY;
        float angleZ = Mathf.Sin(Time.time * swaySpeed) * swayAngle;

        transform.localPosition = startLocalPos + new Vector3(0f, offsetY, 0f);
        transform.localRotation = startLocalRot * Quaternion.Euler(0f, 0f, angleZ);
    }
}
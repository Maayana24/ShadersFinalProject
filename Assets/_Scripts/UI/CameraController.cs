using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Slider slider;

    private float radius;
    private float height;
    private float tiltOffset;

    void Start()
    {
        Vector3 offset = transform.position - target.position;
        radius = new Vector2(offset.x, offset.z).magnitude;
        height = offset.y;

        Quaternion lookAtRotation = Quaternion.LookRotation(target.position - transform.position);
        tiltOffset = transform.eulerAngles.x - lookAtRotation.eulerAngles.x;

        float startAngle = Mathf.Atan2(offset.x, offset.z);
        float startValue = startAngle / (360 * Mathf.Deg2Rad);
        if (startValue < 0) startValue += 1;

        slider.SetValueWithoutNotify(startValue);
        slider.onValueChanged.AddListener(MoveCamera);
    }

    void MoveCamera(float value)
    {
        float angle = value * 360 * Mathf.Deg2Rad;

        transform.position = target.position + new Vector3( Mathf.Sin(angle) * radius, height, Mathf.Cos(angle) * radius);

        transform.LookAt(target);
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x + tiltOffset, transform.eulerAngles.y, transform.eulerAngles.z);
    }
}
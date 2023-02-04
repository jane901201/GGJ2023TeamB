using UnityEngine;

public class LineParameters : MonoBehaviour
{
    [Min(0.0f)]
    public float LineWidth = 0.5f;
    [Min(0.001f)]
    public float LineInterval = 0.05f;
}

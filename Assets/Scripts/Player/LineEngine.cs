using System;
using System.Threading;

using Cysharp.Threading.Tasks;

using UnityEngine;

public class LineEngine : MonoBehaviour
{
    [SerializeField]
    [Min(0)]
    private float _speed = 1.0f;
    [SerializeField]
    [Min(0)]
    private float _angularSpeed = 90.0f;

    private Vector3 _currentDirection;

    [SerializeField]
    private GameObject _lineRoot;
    private CancellationTokenSource _tokenSrc;
    
    // Start is called before the first frame update
    void Start()
    {
        _tokenSrc = new CancellationTokenSource();
        _currentDirection = new Vector3(0, -1, 0);
        _UpdateLineRoot().Forget();
    }

    private async UniTaskVoid _UpdateLineRoot()
    {
        var token = _tokenSrc.Token;
        while (!token.IsCancellationRequested)
        {
            _UpdateDirection();
            Transform rootTrans = _lineRoot.transform;
            Vector3 rootPos = rootTrans.position;
            rootTrans.position = rootPos + _currentDirection * _speed;
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }

    private void _UpdateDirection()
    {
        var horizontalInput = SimpleInput.GetAxis("Horizontal");
        var abs = (float)Unity.Mathematics.math.smoothstep(0.3, 1, Mathf.Abs(horizontalInput));
        float deg = abs * _angularSpeed * Time.deltaTime;
        deg *= Mathf.Sign(horizontalInput);
        _currentDirection = Quaternion.AngleAxis(deg, Vector3.forward) * _currentDirection;
    }

    private void OnDestroy()
    {
        _tokenSrc.Cancel();
        _tokenSrc.Dispose();
    }
}
using System.Threading;

using Cysharp.Threading.Tasks;

using UnityEngine;

public class LineRootEngine : MonoBehaviour
{
    [SerializeField]
    [Min(0)]
    private float _speed = 1.0f;
    [SerializeField]
    [Min(0)]
    private float _angularSpeed = 90.0f;
    [SerializeField]
    private Transform _lineRootTransform;
    [SerializeField]
    private Transform _headTransform;

    private Vector3 _currentDirection;
    private CancellationTokenSource _tokenSrc;
    private GameplayPresenter _gameplayPresenter;

    public void Fire(Vector3 position, Vector3 direction, GameplayPresenter gameplayPresenter)
    {
        if(_tokenSrc != null)
            return;
        _tokenSrc = new CancellationTokenSource();
        _lineRootTransform.position = position;
        _currentDirection = direction;
        _gameplayPresenter = gameplayPresenter;
        _UpdateLineRoot().Forget();
    }

    public void Stop()
    {
        _tokenSrc?.Cancel();
        _tokenSrc?.Dispose();
        _tokenSrc = null;
    }

    private async UniTaskVoid _UpdateLineRoot()
    {
        CancellationToken token = _tokenSrc.Token;
        while (!token.IsCancellationRequested)
        {
            _UpdateDirection();
            Vector3 rootPos = _lineRootTransform.position;
            _lineRootTransform.position = rootPos + _currentDirection * (_gameplayPresenter.GetFinalSpeed(_speed) * Time.deltaTime);
            var upward = Vector3.Cross(Vector3.forward, _currentDirection);
            var headDir = Quaternion.LookRotation(Vector3.forward, upward);
            _headTransform.rotation = headDir;
            
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }

    private void _UpdateDirection()
    {
        float horizontalInput = SimpleInput.GetAxis("Horizontal") * -1;
        var abs = (float)Unity.Mathematics.math.smoothstep(0.3, 1, Mathf.Abs(horizontalInput));
        float deg = abs * _angularSpeed * Time.deltaTime;
        deg *= Mathf.Sign(horizontalInput);
        _currentDirection = Quaternion.AngleAxis(deg, Vector3.forward) * _currentDirection;
    }

    private void OnDestroy()
    { 
        _tokenSrc?.Cancel();
        _tokenSrc?.Dispose();
        _tokenSrc = null;
    }
}

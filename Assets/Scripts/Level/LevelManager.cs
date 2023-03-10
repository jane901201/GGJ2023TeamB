using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private SceneObjectSetting _sceneObjectSetting;

    [SerializeField] private List<Vector2Int> _generatedRange = new List<Vector2Int>();
    [SerializeField] private float _unitLength = 3f;
    [SerializeField] private Vector2Int _generatedBounds = new Vector2Int(5, 10);

    [SerializeField] private int _generateThreshold = 1;

    [Header("Player Object")]
    [SerializeField] private GameObject _obj;

    public bool IsInit { get; private set; } = false;

    public string LevelId { get => _sceneObjectSetting != null ? _sceneObjectSetting.LevelId : string.Empty; }
    public float DownLifeSpeed { get => _sceneObjectSetting != null ? _sceneObjectSetting.DownLifeSpeed : 50f; }

    public void Initialize(SceneObjectSetting sceneObjectSetting)
    {
        _sceneObjectSetting = sceneObjectSetting;
        IsInit = true;
        StartCoroutine(_AutoGenerate());
    }

    private IEnumerator _AutoGenerate()
    {
        while (true)
        {
            var currentPlayerPos = _obj.transform.position;
            var grid = _ConvertToGridUnit(currentPlayerPos);
            // Debug.Log(grid);

            var minBound = grid - _generatedBounds;
            minBound.y = Mathf.Max(0, minBound.y);
            var maxBound = grid + _generatedBounds;
            // Debug.Log(minBound);
            // Debug.Log(maxBound);

            int xMin = minBound.x;
            int xMax = maxBound.x;

            _UpdateRange(maxBound.y);

            bool hasGenerated = false;

            for (int yMin = minBound.y ; yMin <= maxBound.y ; yMin++)
            {
                var yMinGeneratedRange = _generatedRange[yMin];
                int leftMinBound = yMinGeneratedRange.x;
                int rightMaxBound = yMinGeneratedRange.y;

                for (int yMax = yMin ; yMax <= maxBound.y ; yMax++)
                {
                    var yMaxGeneratedRange = _generatedRange[yMax];
                    var yMaxIsAllSpace = yMaxGeneratedRange.x == yMaxGeneratedRange.y;
                    leftMinBound = yMaxIsAllSpace ? leftMinBound : Mathf.Min(leftMinBound, yMaxGeneratedRange.x);
                    rightMaxBound = yMaxIsAllSpace ? rightMaxBound : Mathf.Max(rightMaxBound, yMaxGeneratedRange.y);

                    if (leftMinBound == rightMaxBound)
                    {
                        // Debug.LogFormat("({0},{1}) <-> ({2},{3})", xMin, yMin, xMax, yMax);
                        var maxSize = new Vector2Int(xMax-xMin+1, yMax-yMin+1);
                        if (maxSize.x < _generateThreshold || maxSize.y < _generateThreshold)
                        {
                            continue;
                        }

                        var sceneObject = _sceneObjectSetting.RandomPick(yMin, maxSize);

                        if (sceneObject != null)
                        {
                            leftMinBound = xMin;
                            rightMaxBound = xMin;

                            var obj = GameObject.Instantiate(sceneObject.Prefab);
                            obj.transform.position = new Vector3(leftMinBound+sceneObject.Size.x/2f, -(yMin + sceneObject.Size.y/2f - 1)) * _unitLength;
                            if (sceneObject.IsRotable)
                            {
                                obj.transform.eulerAngles = new Vector3(0f, 0f, Random.Range(0, 4) * 90f);
                            }

                            leftMinBound = leftMinBound - 1;
                            rightMaxBound = rightMaxBound + sceneObject.Size.x;

                            for (int y = yMin ; y <= yMin+sceneObject.Size.y-1 ; y++)
                            {
                                // set yMin to yMax 's range to leftMinBound, rightMaxBound
                                _generatedRange[y] = new Vector2Int(leftMinBound, rightMaxBound);
                            }
                            hasGenerated = true;
                            break;
                        }
                    }
                    else
                    {
                        // Debug.LogFormat("({0},{1}) <-> ({2},{3})", xMin, yMin, leftMinBound, yMax);
                        var maxSize1 = new Vector2Int(leftMinBound-xMin+1, yMax-yMin+1);
                        if (maxSize1.x >= _generateThreshold && maxSize1.y >= _generateThreshold)
                        {
                            var sceneObject1 = _sceneObjectSetting.RandomPick(yMin, maxSize1);
                            if (sceneObject1 != null)
                            {
                                var obj = GameObject.Instantiate(sceneObject1.Prefab);
                                obj.transform.position = new Vector3(leftMinBound-sceneObject1.Size.x/2f+1f, -(yMin + sceneObject1.Size.y/2f - 1)) * _unitLength;
                                if (sceneObject1.IsRotable)
                                {
                                    obj.transform.eulerAngles = new Vector3(0f, 0f, Random.Range(0, 4) * 90f);
                                }

                                leftMinBound = leftMinBound - sceneObject1.Size.x;

                                for (int y = yMin ; y <= yMin+sceneObject1.Size.y-1 ; y++)
                                {
                                    _generatedRange[y] = new Vector2Int(leftMinBound, _generatedRange[y].y);
                                }
                                hasGenerated = true;
                                break;
                            }
                            // Debug.Log(sceneObject1?.Prefab);
                        }

                        // Debug.LogFormat("({0},{1}) <-> ({2},{3})", rightMaxBound, yMin, xMax, yMax);
                        var maxSize2 = new Vector2Int(xMax-rightMaxBound+1, yMax-yMin+1);
                        if (maxSize2.x >= _generateThreshold && maxSize2.y >= _generateThreshold)
                        {
                            var sceneObject2 = _sceneObjectSetting.RandomPick(yMin, maxSize2);
                            // Debug.Log(sceneObject2?.Prefab);
                            if (sceneObject2 != null)
                            {
                                var obj = GameObject.Instantiate(sceneObject2.Prefab);
                                obj.transform.position = new Vector3(rightMaxBound+sceneObject2.Size.x/2f, -(yMin + sceneObject2.Size.y/2f - 1)) * _unitLength;
                                if (sceneObject2.IsRotable)
                                {
                                    obj.transform.eulerAngles = new Vector3(0f, 0f, Random.Range(0, 4) * 90f);
                                }

                                rightMaxBound = rightMaxBound + sceneObject2.Size.x;

                                for (int y = yMin ; y <= yMin+sceneObject2.Size.y-1 ; y++)
                                {
                                    _generatedRange[y] = new Vector2Int(_generatedRange[y].x, rightMaxBound);
                                }
                                hasGenerated = true;
                                break;
                            }
                        }
                    }
                }
                if (hasGenerated)
                {
                    break;
                }
            }

            if (hasGenerated)
            {
                continue;
            }

            // while (true)
            // {
            //     if (Input.GetKeyDown(KeyCode.Space))
            //     {
            //         break;
            //     }
            //     yield return null;
            // }

            yield return null;
        }
    }

    private void _UpdateRange(int maxY)
    {
        if (_generatedRange.Count < maxY+1)
        {
            _generatedRange.AddRange(Enumerable.Range(1, maxY + 1 - _generatedRange.Count).Select(_ => new Vector2Int()));
        }
    }

    private Vector2Int _ConvertToGridUnit(Vector2 pos)
    {
        return new Vector2Int((int)(pos.x/_unitLength), -(int)(pos.y/_unitLength));
    }
}

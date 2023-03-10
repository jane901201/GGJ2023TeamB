using System;
using System.Collections.Generic;

using UnityEngine;

using Random = UnityEngine.Random;

public sealed class RandomPickFromAabb : MonoBehaviour, IRebornMechanism
{
    [Range(0.0f, 1.0f)]
    [SerializeField]
    private float _range;

    private LineDrawerManager _lineDrawerManager;
    private Transform _lineRoot;
    private LineParameters _lineParameters;
    
    public void Initialize(LineDrawerManager lineDrawManager, Transform lineRoot, LineParameters parameters)
    {
        _lineDrawerManager = lineDrawManager;
        _lineRoot = lineRoot;
        _lineParameters = parameters;
    }

    private void OnDrawGizmos()
    {
        if (_lineDrawerManager == null)
            return;
        
        Bounds bounds = _lineDrawerManager.GetWholeBounds();
        Vector3 size = bounds.size;
        var shrankBounds = new Bounds
        (
            bounds.center,
            new Vector3(size.x * _range, size.y * _range, size.z)
        );
        
        Gizmos.color = Color.magenta;
        _DrawBoundsGizmos(bounds);
        Gizmos.color = Color.red;
        _DrawBoundsGizmos(shrankBounds);
    }

    private static void _DrawBoundsGizmos(Bounds aabb)
    {
        Vector3 extents = aabb.extents;
        Gizmos.DrawLine(aabb.center + new Vector3(-extents.x, extents.y, -8), aabb.center + new Vector3(-extents.x, -extents.y, -8));
        Gizmos.DrawLine(aabb.center + new Vector3(-extents.x, extents.y, -8), aabb.center + new Vector3(extents.x, extents.y, -8));
        Gizmos.DrawLine(aabb.center + new Vector3(extents.x, extents.y, -8), aabb.center + new Vector3(extents.x, -extents.y, -8));
        Gizmos.DrawLine(aabb.center + new Vector3(extents.x, -extents.y, -8), aabb.center + new Vector3(-extents.x, -extents.y, -8));
    }

    (Vector3 newDir, LineNode lineNode) IRebornMechanism.GetDest()
    {
        float currentRange = _range;
        var segments = new List<(LineRenderer, int)>();
        {
            Bounds bounds = _lineDrawerManager.GetWholeBounds();
            Vector3 size = bounds.size;
            var shrankBounds = new Bounds
            (
                bounds.center,
                new Vector3(size.x * currentRange, size.y * currentRange, size.z)
            );
            List<LineRenderer> renderers = _lineDrawerManager.GetLineRenderers(shrankBounds);
            Vector3 linePosA = _GetLineRootPos();
            foreach (LineRenderer lineRenderer in renderers)
            {
                int count = lineRenderer.positionCount;
                for (var index = 1; index < count; index++)
                {
                    Vector3 position = lineRenderer.GetPosition(index);
                    if (shrankBounds.Contains(position) && Vector3.Distance(linePosA, position) > _lineParameters.CircleRadius)
                        segments.Add((lineRenderer, index));
                }
            }
        }
        while (segments.Count == 0)
        {
            currentRange = (currentRange + 1.0f) / 2;
            Bounds bounds = _lineDrawerManager.GetWholeBounds();
            Vector3 size = bounds.size;
            var shrankBounds = new Bounds
            (
                bounds.center,
                new Vector3(size.x * currentRange, size.y * currentRange, size.z)
            );
            List<LineRenderer> renderers = _lineDrawerManager.GetLineRenderers(shrankBounds); 
            Vector3 linePosB = _GetLineRootPos();
            foreach (LineRenderer lineRenderer in renderers)
            {
                int count = lineRenderer.positionCount;
                for (var index = 1; index < count; index++)
                {
                    Vector3 position = lineRenderer.GetPosition(index);
                    if (shrankBounds.Contains(position) && Vector3.Distance(linePosB, position) > _lineParameters.CircleRadius)
                        segments.Add((lineRenderer, index));
                }
            }
        }
        int segmentIndex = Random.Range(0, segments.Count);
        (LineRenderer line, int ind) = segments[segmentIndex];
        Vector3 from = line.GetPosition(ind - 1);
        Vector3 to = line.GetPosition(ind);
        Vector3 vec = to - from;
        var normal = new Vector3(vec.y, -vec.x, 0);
        normal *= Mathf.Sign(Random.Range(-1, 1));
        return new ValueTuple<Vector3, LineNode>(normal, new LineNode(line, ind));
    }

    private Vector3 _GetLineRootPos()
    {
        Vector3 pos = _lineRoot.transform.position;
        return new Vector3(pos.x, pos.y, 0);
    }
}

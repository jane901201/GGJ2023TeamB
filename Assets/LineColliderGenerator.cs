using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;

public class LineColliderGenerator : MonoBehaviour
{
    private readonly struct LineNode
    {
        public readonly Vector3 Pos;
        public readonly Vector3 To;

        public LineNode(Vector3 pos, Vector3 to)
        {
            Pos = pos;
            To = to;
        }
    }

    [SerializeField]
    private LineParameters _lineParameters;
    [SerializeField]
    private Transform _lineRoot;
    [SerializeField]
    [Min(0.001f)]
    private float _outerAabbSize;
    [SerializeField]
    private LineRenderer _lineRenderer;

    private readonly List<BoxCollider2D> _colliderPool = new List<BoxCollider2D>();
    private readonly List<LineNode> _validNodes = new List<LineNode>();

    private void OnDrawGizmos()
    {
        Vector3 rootPos = _lineRoot.transform.position;
        Bounds outerAabb = _GetAabb(rootPos, _outerAabbSize);
        Gizmos.color = Color.yellow;
        _DrawBoundsGizmos(outerAabb);
        Gizmos.color = Color.cyan;
        foreach (LineNode node in _validNodes)
        {
            _DrawNodeGizmos(node);
        }
    }

    private void OnValidate()
    {
        Assert.IsTrue(_outerAabbSize > _lineParameters.CircleScale);
    }

    private static void _DrawBoundsGizmos(Bounds aabb)
    {
        Vector3 extents = aabb.extents;
        Gizmos.DrawLine(aabb.center + new Vector3(-extents.x, extents.y, 0), aabb.center + new Vector3(-extents.x, -extents.y, 0));
        Gizmos.DrawLine(aabb.center + new Vector3(-extents.x, extents.y, 0), aabb.center + new Vector3(extents.x, extents.y, 0));
        Gizmos.DrawLine(aabb.center + new Vector3(extents.x, extents.y, 0), aabb.center + new Vector3(extents.x, -extents.y, 0));
        Gizmos.DrawLine(aabb.center + new Vector3(extents.x, -extents.y, 0), aabb.center + new Vector3(-extents.x, -extents.y, 0)); 
    }

    private static void _DrawNodeGizmos(LineNode node)
    {
        Gizmos.DrawLine(node.Pos, node.To);
    }

    // Update is called once per frame
    private void Update()
    {
        _validNodes.Clear();

        _BoundPoints();

        _BindColliders();
    }

    private void _BindColliders()
    {
        var index = 0;
        foreach (LineNode node in _validNodes)
        {
            _BindCollider(node.Pos, node.To, index++);
        }

        for (; index < _colliderPool.Count; index++)
        {
            _colliderPool[index].enabled = false;
        }
    }

    private void _BindCollider(Vector3 from ,Vector3 to, int index)
    {
        if (_colliderPool.Count == index)
        {
            var newGo = new GameObject();
            var newCollider = newGo.AddComponent<BoxCollider2D>();
            _colliderPool.Add(newCollider);
        }
        
        BoxCollider2D coll = _colliderPool[index];
        coll.enabled = true;
        Vector3 center = Vector3.Lerp(from, to, 0.5f);
        coll.size = new Vector2(Vector3.Distance(from, to), _lineParameters.LineWidth);
        coll.transform.position = center;
        float angle = Vector3.Angle(to - from, Vector3.right);
        coll.transform.eulerAngles = new Vector3(0, 0, angle);
    }

    private void _BoundPoints()
    {
        Vector3 rootPos = _lineRoot.transform.position;
        Bounds outerAabb = _GetAabb(rootPos, _outerAabbSize);
        int positionCount = _lineRenderer.positionCount;
        for (var nodeIndex = 0; nodeIndex < positionCount - 1; nodeIndex++)
        {
            Vector3 nodePos = _lineRenderer.GetPosition(nodeIndex);
            Vector3 nextPos = _lineRenderer.GetPosition(nodeIndex + 1);
            if (!outerAabb.Contains(nodePos) || Vector3.Distance(rootPos, nextPos) <= _lineParameters.CircleScale)
                continue;
            _validNodes.Add
            (
                new LineNode
                (
                    nodePos,
                    nextPos 
                )
            );
        } 
    }

    private static Bounds _GetAabb(Vector3 pos, float size)
        => new Bounds(pos, new Vector3(size, size, size));
}
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.ReorderableList.Element_Adder_Menu;
using UnityEngine;

public class RopeChain : MonoBehaviour
{
    public class Node
    {
        public Node next;
        public Node prev;
        public Vector2 pos;
        public bool isStart, isEnd;

        public Node(Node next, Node prev, Vector2 pos, bool isStart, bool isEnd)
        {
            this.next = next;
            this.prev = prev;
            this.pos = pos;
            this.isStart = isStart;
            this.isEnd = isEnd;
        }
    }

    public enum Mode {
        PENDULUM,
        CATENARY,
        CANCELED
    }

    public float maxRopeLength = 12f;
    public float maxNodeLength = 0.3f;
    public float catenaryInitialA = 0.01f;
    public float catenaryAccuracy = 0.0001f;

    private LineRenderer lineRenderer;
    private Vector2 _startPos;
    private List<Node> _nodes;
    private int _nodeNum;
    private float _ropeLen;
    private Mode _mode = Mode.CANCELED;
    private Player _player;

    public Vector2 StartPoint { get => _startPos; }
    public float RopeLength { get => _ropeLen; }
    public Vector2 EndPoint { get => _nodes.Last().pos; }
    public float RopeNodeCount { get => _nodeNum; }

    protected void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void LateUpdate() {
        if (_mode == Mode.CANCELED)
        {
            lineRenderer.enabled = false;
            return;
        }

        lineRenderer.enabled = true;
        _nodes[0].pos = _startPos;
        
        if (_mode == Mode.PENDULUM)
        {
            DrawPendulumLine();
        }
        else if (_mode == Mode.CATENARY)
        {
            DrawCatenaryLine();
        }
    }

    private void DrawPendulumLine()
    {
        lineRenderer.positionCount = _nodeNum;
        lineRenderer.enabled = true;

        for (int i = 0; i < _nodeNum; i++)
        {
            if (i == 0) {
                _nodes[i].pos = _startPos;
            } else if (i != _nodeNum - 1) {
                _nodes[i].pos = Vector2.Lerp(_nodes[0].pos, _nodes.Last().pos, i / (_nodeNum - 1));
            }
            lineRenderer.SetPosition(i, _nodes[i].pos);
        }
    }

    private void DrawCatenaryLine() {
        Vector2 endPos = _nodes.Last().pos;

        float dx = endPos.x - _startPos.x;
        float xb = (endPos.x + _startPos.x) / 2;
        float dy = endPos.y - _startPos.y;
        float yb = (endPos.y + _startPos.y) / 2;

        float r = Mathf.Sqrt(Mathf.Pow(_ropeLen, 2) - Mathf.Pow(dy, 2)) / dx;

        float A = catenaryInitialA;

        float left = r * A;
        float right = sinh(A);

        while (left >= right) {
            left = r * A;
            right = sinh(A);
            A += catenaryAccuracy;
        }

        A = A - catenaryAccuracy;

        float a = dx / (2 * A);
        float b = xb - a * tanhi(dy / _ropeLen);
        float c = _startPos.y - a * cosh((_startPos.x - b) / a);

        float x, y;
        for (int i = 0; i < _nodeNum; i++)
        {
            x = Mathf.Lerp(_startPos.x, endPos.x, i / (_nodeNum - 1));
            y = CalculateCatenary(x, a, b, c);
            _nodes[i].pos.x = x;
            _nodes[i].pos.y = y;
            lineRenderer.SetPosition(i, _nodes[i].pos);
        }
    }

    float CalculateCatenary(float x, float a, float b, float c) {
        return a * cosh((x - b) / a) + c;
    }

    public void ChangeMode(bool isPendulum)
    {
        _mode = isPendulum ? Mode.PENDULUM : Mode.CATENARY;
    }

    public Vector2 CreateRope(Vector2 startPos, Vector2 endPos, Player player, bool isPendulum=true)
    {
        _startPos = startPos;
        _player = player;
        _mode = isPendulum ? Mode.PENDULUM : Mode.CATENARY;

        _nodes = new List<Node>();
        Vector2 unitVec = (endPos - _startPos).normalized;
        int numNodes = (int)((endPos - _startPos).magnitude / maxNodeLength) + 1;
        _ropeLen = (numNodes - 1) * maxNodeLength;
        _nodeNum = numNodes;

        for (int i = 0; i < numNodes; i++)
        {
            Vector2 pos = _startPos + unitVec * i * maxNodeLength;
            Node node = new Node(null, i == 0 ? null : _nodes.Last(), pos, i == 0, i == numNodes - 1);
            if (i != 0)
            {
                _nodes.Last().next = node;
            }
            _nodes.Add(node);
        }
        
        return _nodes.Last().pos;
    }

    public void CancelRope() {
        _nodes = null;
        _mode = Mode.CANCELED;
    }

    public void SetEndNodePos(Vector2 pos) {
        _nodes.Last().pos = pos;
    }

    private float cosh(float n) {
        return (Mathf.Exp(n) + Mathf.Exp(-n)) / 2;
    }

    private float sinh(float n) {
        return (Mathf.Exp(n) - Mathf.Exp(-n)) / 2;
    }

    private float tanhi(float n) {
        return .5f * Mathf.Log((1 + n) / (1 - n));
    }
}

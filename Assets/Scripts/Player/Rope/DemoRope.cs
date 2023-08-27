using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using W02;

public class DemoRope : PlayerAbility
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

    public float maxNodeLength = 0.3f;

    private LineRenderer lineRenderer;
    private bool _isRopeCreated = false;
    private bool _wasRopeModeStarted = false;
    private Vector2 _startPos;
    private Vector2 _endPos;
    private List<Node> _nodes;
    private float _gravity = -10f;
    private int _nodeNum;
    private float _ropeLen;
    private Vector2 _playerVel;

    protected override void Awake()
    {
        base.Awake();
        lineRenderer = GetComponent<LineRenderer>();
    }

    protected override void HandleInput()
    {
        CheckClick();

        if (_wasRopeModeStarted)
        {
            Vector2 ropeVec = _endPos - _startPos;
            float angle = Vector2.Angle(ropeVec, Vector2.down);
            float speed = Mathf.Sqrt(2 * _gravity * Mathf.Cos(Mathf.Deg2Rad * angle) * _ropeLen); // ������ � ���� ��ġ������ �ӷ�
            Vector2 tangentVec = new Vector2(-ropeVec.y, ropeVec.x);
            tangentVec.Normalize();
            _playerVel = tangentVec * speed;

            _wasRopeModeStarted = false;
        }

        if (_isRopeCreated)
        {
            Node first = _nodes[0];
            Node last = _nodes.Last();
            Vector2 deltaPos = _playerVel * Time.deltaTime;
            Vector2 targetPos = last.pos + deltaPos;
            Vector2 radiusVec = targetPos - first.pos;
            radiusVec.Normalize();
            radiusVec = radiusVec * _ropeLen;

            // ���� ��ġ ����
            last.pos = first.pos + radiusVec;

            float angle = Vector2.Angle(radiusVec, Vector2.down);
            float acc = _gravity * Mathf.Sin(Mathf.Deg2Rad * angle);
            Vector2 tangentVec = new Vector2(-radiusVec.y, radiusVec.x);
            tangentVec.Normalize();
            Vector2 accelaration = tangentVec * acc;
            _playerVel += accelaration * Time.deltaTime;
            DrawLines();
        } else
        {
            lineRenderer.enabled = false;
        }
    }

    void DrawLines()
    {
        lineRenderer.positionCount = _nodeNum;
        lineRenderer.enabled = true;

        for (int i = 0; i < _nodeNum; i++)
        {
            _nodes[i].pos = Vector2.Lerp(_nodes[0].pos, _nodes.Last().pos, i / (_nodeNum - 1));
            lineRenderer.SetPosition(i, _nodes[i].pos);
        }
        Debug.Log(_nodes[0].pos + " + " + _nodes.Last().pos);
    }

    void CheckClick()
    {
        if (!_isRopeCreated && _hookButtonClicked)
        {
            _startPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            _endPos = _player.transform.position;
            CreateRope();
        }

        if (_isRopeCreated && !_hookButtonClicked)
        {
            _isRopeCreated = false;
        }
    }

    void CreateRope()
    {
        _nodes = new List<Node>();
        Vector2 unitVec = (_endPos - _startPos).normalized;
        int numNodes = (int)((_endPos - _startPos).magnitude / maxNodeLength) + 1;

        for (int i = 0; i < numNodes; i++)
        {
            Vector2 pos = _startPos + unitVec * i;
            Node node = new Node(null, i == 0 ? null : _nodes.Last(), pos, i == 0, i == numNodes - 1);
            if (i != 0) {
                _nodes.Last<Node>().next = node;
            }
            _nodes.Add(node);
        }

        _nodeNum = numNodes;
        _ropeLen = (numNodes - 1) * maxNodeLength;
        _isRopeCreated = true;
        _wasRopeModeStarted = true;
        _controller.SetRopeMode(_nodes.Last());
    }
}

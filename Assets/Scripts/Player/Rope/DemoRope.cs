using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    public float accCoef = 1.2f;
    public float friction = 0.8f;
    public float inputForce = 1.2f;
    [Range(80, 130)]
    public float maxAngle;
    public float maxRopeLen;

    private LineRenderer lineRenderer;
    private bool _isRopeCreated = false;
    private bool _wasRopeModeStarted = false;
    private Vector2 _startPos;
    private Vector2 _endPos;
    private List<Node> _nodes;
    private float _gravity = -30f;
    private int _nodeNum;
    private float _ropeLen;
    private Vector2 _playerVel;
    private float _maxSpeed;
    private bool _isGivingForce;

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
            //Vector2 ropeVec = _endPos - _startPos;
            Vector2 ropeVec = _nodes.Last().pos - _nodes[0].pos;
            float angle = Vector2.Angle(ropeVec, Vector2.down);
            _maxSpeed = Mathf.Sqrt(Mathf.Abs(2 * _gravity * maxRopeLen * (1 - Mathf.Cos(maxAngle))));
            Vector2 tangentVec = new Vector2(ropeVec.x < 0 ? -ropeVec.y : ropeVec.y, ropeVec.x < 0 ? ropeVec.x : -ropeVec.x);
            tangentVec.Normalize();
            _playerVel = tangentVec * 0 ;

            Debug.Log("first: " + _nodes[0].pos + " last: " + _nodes.Last().pos + " start: " + _startPos + " end: " + _endPos);

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

            //포지션 지정
            last.pos = first.pos + radiusVec;

            float angle = Vector2.Angle(radiusVec, Vector2.down);
            float acc = _gravity * Mathf.Sin(angle * Mathf.PI / 180f);
            Vector2 tangentVec = new Vector2(radiusVec.x < 0 ? -radiusVec.y : radiusVec.y, radiusVec.x < 0 ? radiusVec.x : -radiusVec.x);
            tangentVec.Normalize();
            Vector2 accelarationG = tangentVec * Mathf.Abs(acc);
            Vector2 accelarationT = -radiusVec.normalized * Mathf.Pow(_playerVel.magnitude, 2) / _ropeLen;
            Vector2 accelaration = accelarationG + accelarationT;
            Debug.Log(_playerVel + " " + accelaration);
            float lastVelX = _playerVel.x;
            _playerVel += accelaration * Time.deltaTime;

            //if (lastVelX * _playerVel.x < 0)
            //{
            //    _isGivingForce = true;
            //    if (radiusVec.x * _horizontalMove < 0)
            //    {
            //        _playerVel.Normalize();
            //        _playerVel *= inputForce;
            //    }
            //    else
            //    {
            //        _playerVel *= friction;
            //    }
            //}

            if (radiusVec.x * _horizontalMove < 0)
            {
                _playerVel *= inputForce;
            }


            DrawLines();
        } else
        {
            lineRenderer.enabled = false;
        }
    }

    Vector2 rotate(Vector2 v, float delta)
    {
        return new Vector2(
            v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
            v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
        );
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
        _ropeLen = (numNodes - 1) * maxNodeLength;

        for (int i = 0; i < numNodes; i++)
        {
            Vector2 pos = _startPos + unitVec * i * maxNodeLength;
            Node node = new Node(null, i == 0 ? null : _nodes.Last(), pos, i == 0, i == numNodes - 1);
            if (i != 0) {
                _nodes.Last<Node>().next = node;
            }
            _nodes.Add(node);
        }

        _nodeNum = numNodes;
        _isRopeCreated = true;
        _wasRopeModeStarted = true;
        _controller.SetRopeMode(_nodes.Last());
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using W02;
using UnityEngine.InputSystem;

public class Chain : MonoBehaviour
{
	public GameObject player;
	public VirtualRopeLine virtualRope;
	public float detectionLength = 2f;

	private void Update()
    {
		Vector3 mousePosition = Mouse.current.position.ReadValue();
		virtualRope.Draw(player.transform.position, Camera.main.ScreenToWorldPoint(mousePosition), detectionLength);
        if (player.GetComponent<Player>().playerInfo.ropeState == Player.RopeState.HOOKED)
        {
            //player.GetComponent<Player>().ChangeState(Player.State.ROPE);
            this.CreateChain(Vector2.Distance((Vector2)transform.position, Camera.main.ScreenToWorldPoint(mousePosition)));
            this.ChainConnect(transform.position, Camera.main.ScreenToWorldPoint(mousePosition), Vector2.Distance((Vector2)transform.position, Camera.main.ScreenToWorldPoint(mousePosition)), 0.5f);
        }
    }

	private void LateUpdate()
	{
		if (!this.isActive)
		{
			return;
		}
		if (!this.isLinear)
		{
			foreach (Chain.ChainNode chainNode in this.nodes)
			{
				chainNode.MovePoint();
			}
			for (int i = 0; i < this.stiffness; i++)
			{
				foreach (Chain.Line line in this.lines)
				{
					this.ConstrainLine(line);
				}
			}
		}
		if (this.chainNowCount > 1)
		{
			Vector2 position = this.nodes[0].position;
			Vector2 a = this.nodes[this.chainNowCount - 1].position - position;
			for (int j = 0; j < this.chainNowCount; j++)
			{
				Vector2 linearPosition;
				if (this.isLinear)
				{
					linearPosition = Vector2.Lerp(this.nodes[j].position, position + a.normalized * (this.nodeHeight * (float)j), this.linearPercent);
				}
				else
				{
					linearPosition = Vector2.Lerp(this.nodes[j].position, position + a * (float)j / (float)(this.chainNowCount - 1), this.linearPercent);
				}
				this.nodes[j].linearPosition = linearPosition;
			}
		}
		this.Draw();
	}

	public void ChainConnect(Vector2 startPos, Vector2 endPos, float nowChainDistance, float linearPercent)
	{
		this.isActive = true;
		this.isLinear = false;
		this.linearPercent = Mathf.Lerp(this.minLinearPercent, 1f, linearPercent);
		this.chainNowCount = this.ChainCount(nowChainDistance);
		this.chainNowCount = Mathf.Clamp(this.chainNowCount, 0, this.chainMaxCount);
		if (this.chainNowCount < 1)
		{
			for (int i = 0; i <= this.chainMaxCount; i++)
			{
				this.nodes[i].isFixed = true;
				this.nodes[i].isActive = false;
			}
			return;
		}
		for (int j = 0; j <= this.chainMaxCount; j++)
		{
			if (j < this.chainNowCount)
			{
				this.nodes[j].isFixed = false;
				this.nodes[j].isActive = true;
			}
			else
			{
				Chain.ChainNode chainNode = this.nodes[j];
				this.nodes[j].linearPosition = endPos;
				chainNode.position = endPos;
				this.nodes[j].isFixed = true;
				this.nodes[j].isActive = false;
			}
		}
		Chain.ChainNode chainNode2 = this.nodes[0];
		this.nodes[0].linearPosition = startPos;
		chainNode2.position = startPos;
		this.nodes[0].isFixed = true;
		this.nodes[0].isActive = true;
		Chain.ChainNode chainNode3 = this.nodes[this.chainNowCount - 1];
		this.nodes[this.chainNowCount - 1].linearPosition = endPos;
		chainNode3.position = endPos;
		this.nodes[this.chainNowCount - 1].isFixed = true;
		this.nodes[this.chainNowCount - 1].isActive = true;
	}

	public void CreateChain(float chainLength)
	{
		this.chainMaxCount = Mathf.CeilToInt(chainLength / this.nodeHeight);
		chainMaxLength = chainLength;
		for (int i = 0; i <= this.chainMaxCount; i++)
		{
			this.nodes.Add(this.CreateNode(Vector2.right * chainLength / (float)this.chainMaxCount * (float)i, false));
			if (i != 0)
			{
				Transform chainNode = UnityEngine.Object.Instantiate<Transform>((i % 2 == 0) ? this.chain01 : this.chain02, base.transform);
				this.lines.Add(new Chain.Line(chainNode, this.nodes[i - 1], this.nodes[i], this.nodeHeight));
			}
		}
		this.nodes[0].isFixed = (this.nodes[this.chainMaxCount].isFixed = true);
		this.ChainReset();
	}

	public void ChainReset()
	{
		this.isActive = false;
		this.chainNowCount = 0;
		for (int i = 0; i < this.chainMaxCount; i++)
		{
			if (i < this.nodes.Count)
			{
				this.nodes[i].isFixed = false;
				this.nodes[i].isActive = false;
			}
		}
		this.Draw();
	}

	private void Draw()
	{
		foreach (Chain.Line line in this.lines)
		{
			line.Draw();
		}
	}

	private int ChainCount(float length)
	{
		return Mathf.CeilToInt(length / this.nodeHeight);
	}

	private Chain.ChainNode CreateNode(Vector2 position, bool isFixed)
	{
		return new Chain.ChainNode(position, this.gravity, this.friction, isFixed);
	}

	private void ConstrainLine(Chain.Line line)
	{
		Vector2 vector = line.p2.position - line.p1.position;
		float magnitude = vector.magnitude;
		float d = (line.length - magnitude) / magnitude / 2f;
		vector *= d;
		if (line.p2.isFixed)
		{
			if (!line.p1.isFixed)
			{
				line.p1.position -= vector * 2f;
				return;
			}
		}
		else if (line.p1.isFixed)
		{
			if (!line.p2.isFixed)
			{
				line.p2.position += vector * 2f;
				return;
			}
		}
		else
		{
			line.p1.position -= vector;
			line.p2.position += vector;
		}
	}

	[Tooltip("체인 세로 길이")]
	public float nodeHeight = 1f;

	public float gravity = 0.9f;

	public float friction = 0.9f;

	public int stiffness = 12;

	public float linearPercent;

	public float minLinearPercent = 0.5f;

	public List<Chain.ChainNode> nodes = new List<Chain.ChainNode>();

	public List<Chain.Line> lines = new List<Chain.Line>();

	public Transform chain01;

	public Transform chain02;

	public int chainMaxCount;

	private int chainNowCount;

	public float chainMaxLength;

	private bool isLinear;

	private bool isActive;

	[Serializable]
	public class ChainNode
	{
		public void MovePoint()
		{
			if (this.isFixed)
			{
				return;
			}
			float num = (this.position.x - this.oldPosition.x) * this.friction;
			float num2 = (this.position.y - this.oldPosition.y) * this.friction;
			this.oldPosition = this.position;
			this.position.x = this.position.x + num * Time.deltaTime;
			this.position.y = this.position.y + num2 * Time.deltaTime;
			this.position.y = this.position.y - this.gravity * Time.deltaTime;
		}
		public ChainNode(Vector2 position, float gravity, float friction, bool isFixed)
		{
			this.isFixed = isFixed;
			this.gravity = gravity;
			this.friction = friction;
			this.linearPosition = position;
			this.oldPosition = position;
			this.position = position;
		}

		public bool isFixed;

		public bool isActive;

		public Vector2 position;

		public Vector2 oldPosition;

		public Vector2 linearPosition;

		private float gravity;

		private float friction;
	}

	[Serializable]
	public class Line
	{
		public Line(Transform chainNode, Chain.ChainNode p1, Chain.ChainNode p2, float length)
		{
			this.chainNode = chainNode;
			this.p1 = p1;
			this.p2 = p2;
			this.length = length;
		}

		public void Draw()
		{
			if (this.p1.isActive && this.p2.isActive)
			{
				this.chainNode.gameObject.SetActive(true);
				this.chainNode.transform.position = (this.p1.linearPosition + this.p2.linearPosition) * 0.5f;
				this.chainNode.transform.eulerAngles = Vector3.forward * LookAt2D(this.p1.linearPosition, this.p2.linearPosition);
				return;
			}
			this.chainNode.gameObject.SetActive(false);
		}

		public Chain.ChainNode p1;
		public Chain.ChainNode p2;
		public float length;
		public Transform chainNode;
	}
	public static float LookAt2D(Vector3 from, Vector3 to)
	{
		return AngleTo360(Mathf.Atan2(to.y - from.y, to.x - from.x) * 57.29578f - 90f);
	}
	public static float AngleTo360(float angle)
	{
		if (angle >= 0f)
		{
			return angle % 360f;
		}
		return angle % 360f + 360f;
	}
}

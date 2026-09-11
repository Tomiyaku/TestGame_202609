using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    private const float ROPE_HIDE_TIME = 1f;
    private const float ROPE_DEFAULT_WIDTH = 0.1f;
    public enum RopeState
    {
        Hide,
        Extend,
        Connect,
        Disconnect,
        Remove,
    }

    [field:SerializeField]
    public RopeState State = RopeState.Hide;

    private LineRenderer LineRenderer { get; set; } = null;

    [SerializeField]
    private List<ConnectInfo> _connectInfoList = new List<ConnectInfo>();

    public float DisconnectStartTime { get; private set; } = -1;

    void Awake()
    {
        LineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (State == RopeState.Remove)
        {
            RopeManager.Instance.RemoveRope(this);
            return;
        }

        if (State == RopeState.Disconnect)
        {
            float ratio = 1f - Mathf.Clamp((Time.unscaledTime - DisconnectStartTime) / ROPE_HIDE_TIME, 0f, 1f);

            if (ratio <= 0f)
            {
                State = RopeState.Remove;
                RopeManager.Instance.RemoveRope(this);
                return;
            }

            LineRenderer.startWidth = ROPE_DEFAULT_WIDTH * ratio;
            LineRenderer.endWidth = ROPE_DEFAULT_WIDTH * ratio;
        }
    }

    public void Init(Color color)
    {
        LineRenderer.startColor = color;
        LineRenderer.endColor = color;
        LineRenderer.enabled = false;
    }

    public void StartExtend(Color color, Vector3 startPoint)
    {
        LineRenderer.startColor = color;
        LineRenderer.endColor = color;
        LineRenderer.startWidth = ROPE_DEFAULT_WIDTH;
        LineRenderer.endWidth = ROPE_DEFAULT_WIDTH;

        LineRenderer.positionCount = 2;
        LineRenderer.SetPosition(0, startPoint);
        LineRenderer.SetPosition(1, startPoint);
        LineRenderer.enabled = true;

        State = RopeState.Extend;
    }

    public void Connect(ShotHitInfo shotHitInfo, Vector3 startPoint)
    {
        LineRenderer.positionCount = 2;
        LineRenderer.SetPosition(0, shotHitInfo.WorldPoint + new Vector3(0, 0.025f, 0));
        LineRenderer.SetPosition(1, startPoint);
        LineRenderer.enabled = true;

        _connectInfoList.Clear();

        ConnectInfo connectInfo = new ConnectInfo()
        {
            ConnectedBody = shotHitInfo.Rigidbody2D,
            ConnectedWorldPosition = shotHitInfo.WorldPoint,
            ConnectedVertex = null
        };

        _connectInfoList.Add(connectInfo);

        State = RopeState.Connect;
    }

    public void Disconnect()
    {
        DisconnectStartTime = Time.unscaledTime;
        
        State = RopeState.Disconnect;
    }

    public void UpdateExtendPosition(Vector3 position)
    {
        if (State != RopeState.Extend) return;

        LineRenderer.SetPosition(LineRenderer.positionCount - 2, position);
    }

    public void UpdateStartPosition(Vector3 startPoint)
    {
        LineRenderer.SetPosition(LineRenderer.positionCount - 1, startPoint);
    }

    public void Bend(Rigidbody2D connectBody, VertexInfo vertexInfo, Vector3 startPoint)
    {
        LineRenderer.positionCount++;
        LineRenderer.SetPosition(LineRenderer.positionCount - 2, vertexInfo.Position);
        LineRenderer.SetPosition(LineRenderer.positionCount - 1, startPoint);

        ConnectInfo connectInfo = new ConnectInfo()
        {
            ConnectedBody = connectBody,
            ConnectedWorldPosition = vertexInfo.Position,
            ConnectedVertex = vertexInfo
        };

        _connectInfoList.Add(connectInfo);
    }

    public bool ReturnLastBentPoint(Vector3 startPoint, out ConnectInfo outPrevBendConnectInfo)
    {
        outPrevBendConnectInfo = default;

        if (_connectInfoList.Count < 2) return false;

        _connectInfoList.RemoveAt(_connectInfoList.Count - 1);

        outPrevBendConnectInfo = _connectInfoList[_connectInfoList.Count - 1];

        LineRenderer.positionCount--;
        LineRenderer.SetPosition(LineRenderer.positionCount - 1, startPoint);

        return true;
    }

    public bool GetLastBentPoint(out Vector3 outPoint)
    {
        outPoint = Vector3.zero;

        if (!LineRenderer.enabled) return false;
        if (LineRenderer.positionCount < 2) return false;

        outPoint = LineRenderer.GetPosition(LineRenderer.positionCount - 2);

        return true;
    }

    public bool GetPrevBendLine(out Vector3[] outLastBendPointList)
    {
        outLastBendPointList = null;

        if (!LineRenderer.enabled) return false;
        if (_connectInfoList.Count < 2) return false;

        int count = _connectInfoList.Count;

        outLastBendPointList = new Vector3[2] { _connectInfoList[count - 1].ConnectedWorldPosition, _connectInfoList[count - 2].ConnectedWorldPosition };

        return true;
    }
}

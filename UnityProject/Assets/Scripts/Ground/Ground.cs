using CodeIcf.AssetManagement.AdressableManagement;
using CodeIcf.Extensions;
using UnityEditorInternal;
using UnityEngine;

[System.Serializable]
public class VertexInfo
{
    [SerializeField]
    private int _id = 0;
    [SerializeField]
    private Vector2 _position;
    public int Id => _id;
    public Vector2 Position => _position;

    public VertexInfo(int id, Vector2 pos)
    {
        _id = id;
        _position = pos;
    }

    public void Rotation(Vector2 center, float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        float sin = Mathf.Sin(rad);
        float cos = Mathf.Cos(rad);

        Vector2 tpos = Position - center;
        float rx = tpos.x * cos - (Position.y - center.y) * sin + center.x;
        float ry = tpos.x * sin + tpos.y * cos + center.y;

        _position = new Vector2(rx, ry);
    }
}

public class Ground : MonoBehaviour
{
    public BoxCollider2D BoxCollider2D { get; private set; } = null;
    public Rigidbody2D Rigidbody2D { get; private set; } = null;
    [field:SerializeField]
    public VertexInfo[] VertexInfoList { get; private set; } = null;

    void Awake()
    {
        BoxCollider2D = GetComponent<BoxCollider2D>();
        Rigidbody2D = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        VertexInfoList = GetBoxVertexInfoList();

        GroundManager.Instance.AddGround(this);
    }

    private VertexInfo[] GetBoxVertexInfoList()
    {
        if (BoxCollider2D == null) return null;

        VertexInfo[] result = new VertexInfo[4];
        Vector2 position = transform.position.ToVector2() + BoxCollider2D.offset;
        Vector2 scale = transform.localScale;
        result[0] = new VertexInfo(0, new Vector2(position.x - (BoxCollider2D.size.x / 2f * scale.x), position.y + (BoxCollider2D.size.y / 2f * scale.y)));
        result[1] = new VertexInfo(1, new Vector2(position.x + (BoxCollider2D.size.x / 2f * scale.x), position.y + (BoxCollider2D.size.y / 2f * scale.y)));
        result[2] = new VertexInfo(2, new Vector2(position.x + (BoxCollider2D.size.x / 2f * scale.x), position.y - (BoxCollider2D.size.y / 2f * scale.y)));
        result[3] = new VertexInfo(3, new Vector2(position.x - (BoxCollider2D.size.x / 2f * scale.x), position.y - (BoxCollider2D.size.y / 2f * scale.y)));

        foreach (VertexInfo info in result)
        {
            info.Rotation(transform.position.ToVector2(), transform.localEulerAngles.z);
        }

        return result;
    }

    void OnDrawGizmos()
    {
        if( VertexInfoList == null) return;

        Gizmos.color = Color.green;

        foreach (VertexInfo info in VertexInfoList)
        {
            Gizmos.DrawSphere(info.Position, 0.1f);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GroundManager : MonoBehaviour
{
    public static GroundManager Instance { get; private set; } = null;
    [SerializeField]
    private List<Ground> _groundList = new List<Ground>();

    void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddGround(Ground ground)
    {
        if(_groundList.Count == 0) 
        {
            _groundList.Add(ground);
        }
        else
        {
            if( _groundList.FindIndex((g) => g == ground ) < 0) _groundList.Add(ground); 
        }
    }

    public bool CheckShotVectorHit(Vector2 start, Vector2 vec, float distance, out RaycastHit2D outRaycastHit)
    {
        outRaycastHit = default;
        RaycastHit2D[] list = Physics2D.RaycastAll(start, vec, distance, LayerMask.GetMask("Ground"));

        if (list == null || list.Length == 0) return false;

        foreach (RaycastHit2D raycastHit in list)
        {
            if (outRaycastHit == default) outRaycastHit = raycastHit;
            else if (outRaycastHit.distance > raycastHit.distance) outRaycastHit = raycastHit;
        }

        return outRaycastHit != default;
    }

    public bool CheckRopeBendPoint(Vector2 start, float angle, Vector2 direction, float distance, out RaycastHit2D outRaycastHit)
    {
        outRaycastHit = default;
        Vector2 size = new Vector2(0.05f, 0.05f);
        RaycastHit2D[] list = Physics2D.BoxCastAll(start, size, angle, direction, distance / 2f, LayerMask.GetMask("Ground"));

        foreach (RaycastHit2D raycastHit in list)
        {
            BoxCollider2D boxCollider2D = raycastHit.transform.gameObject.GetComponent<BoxCollider2D>();

            if (boxCollider2D == null) continue;

            Vector2[] pointList = new Vector2[4];
            Vector2 position = raycastHit.transform.position;
            Vector2 scale = raycastHit.transform.localScale;
            pointList[0] = new Vector2(position.x - (boxCollider2D.size.x / 2f * scale.x), position.y + (boxCollider2D.size.y / 2f * scale.y));
            pointList[1] = new Vector2(position.x + (boxCollider2D.size.x / 2f * scale.x), position.y + (boxCollider2D.size.y / 2f * scale.y));
            pointList[2] = new Vector2(position.x + (boxCollider2D.size.x / 2f * scale.x), position.y - (boxCollider2D.size.y / 2f * scale.y));
            pointList[3] = new Vector2(position.x - (boxCollider2D.size.x / 2f * scale.x), position.y - (boxCollider2D.size.y / 2f * scale.y));

            foreach (Vector2 point in pointList)
            {
                if (Vector2.Distance(point, raycastHit.point) <= 0.05f)
                {
                    outRaycastHit = raycastHit;
                    break;
                }
            }

            if (outRaycastHit != default) break;
        }

        return outRaycastHit != default;
    }

    public bool CheckRopeBendPoint(VertexInfo current, out VertexInfo outVertexInfo, out Rigidbody2D outRigidbody2d, params Vector2[] positionList)
    {
        outVertexInfo = default;
        outRigidbody2d = null;

        foreach (Ground ground in _groundList)
        {
            VertexInfo[] vertexList = ground.VertexInfoList;

            if (vertexList == null) continue;
            if (vertexList.Length < 3) continue;

            foreach (VertexInfo vertexInfo in vertexList)
            {
                if (vertexInfo == current) continue;

                if (CheckPointInsidePolygon(positionList, vertexInfo.Position))
                {
                    outVertexInfo = vertexInfo;
                    outRigidbody2d = ground.Rigidbody2D;
                    return true;
                }
            }
        }
        return false;
    }

    private bool CheckPointInsidePolygon(Vector2[] posList, Vector2 target)
    {
        float result = 0;

        for (int i = 0; i < posList.Length; i++)
        {
            Vector2 line1 = posList[i] - target;
            Vector2 line2 = posList[(i + 1) % posList.Length] - target;

            float angle = Vector2.Angle(line1, line2);

            result += angle;
        }

        return Mathf.Abs(result) >= 360f;
    }
}

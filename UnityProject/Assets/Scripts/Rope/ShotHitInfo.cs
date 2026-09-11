using UnityEngine;

[System.Serializable]
public struct ShotHitInfo
{
    public Vector2 Vector;
    public Vector3 WorldPoint;
    public Vector3 AnchorPoint;
    public float Distance;
    public Rigidbody2D Rigidbody2D;
}

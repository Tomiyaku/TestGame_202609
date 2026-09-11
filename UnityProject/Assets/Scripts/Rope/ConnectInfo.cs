using UnityEngine;

[System.Serializable]
public struct ConnectInfo
{
    public Rigidbody2D ConnectedBody;
    public Vector3 ConnectedWorldPosition;
    public VertexInfo ConnectedVertex;
}

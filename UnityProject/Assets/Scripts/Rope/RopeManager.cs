using System.Collections.Generic;
using UnityEngine;

public class RopeManager : MonoBehaviour
{
        public static RopeManager Instance { get; private set; } = null;

    [SerializeField]
    private Rope _ropeBaseObject = null;

    [SerializeField]
    private List<Rope> _ropeList = new List<Rope>();


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

    public Rope RentRope()
    {
        Rope rope = Instantiate(_ropeBaseObject);
        rope.transform.parent = transform;

        _ropeList.Add(rope);

        return rope;
    }

    public void RemoveRope(Rope rope)
    {
        _ropeList.Remove(rope);
        Destroy(rope.gameObject);                
    }
}

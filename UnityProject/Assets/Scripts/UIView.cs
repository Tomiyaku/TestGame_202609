using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIView : MonoBehaviour
{
    [SerializeField]
    private Text m_GrabStateText = null;
    [SerializeField]
    private WireTip m_WireTip = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        m_GrabStateText.text = m_WireTip.IsUseConnectAngleLimit ? "角度制限" : "通常";
    }
}

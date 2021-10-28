using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackGroundController : MonoBehaviour
{
    [SerializeField]
    Image[] m_backgrounds = default;

    [SerializeField]
    Sprite[] m_backgroundTypes = default; 

    Animator m_anim = default;

    void Start()
    {
        m_anim = GetComponent<Animator>();
    }

    public void Crossfade(int current, int backgroundType)
    {
        if (current == 1)
        {

        }
    }
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoflowButton : MonoBehaviour
{
    Button m_autoButton = default; 
    Text m_autoflowText = default;
    Animator m_anim = default;

    void Start()
    {
        m_autoButton = GetComponent<Button>();
        m_autoflowText = transform.GetComponentInChildren<Text>();
        m_anim = GetComponent<Animator>();

        m_autoButton.onClick.AddListener(SwitchAutoflow);
    }

    public void SwitchAutoflow()
    {
        if (!DialogManager.Instance.IsAutoflow)
        {
            m_autoflowText.text = "自動再生ON";
            m_anim?.Play("ON");
            DialogManager.Instance.IsAutoflow = true;
        }
        else
        {
            m_autoflowText.text = "自動再生OFF";
            m_anim?.Play("OFF");
            DialogManager.Instance.IsAutoflow = false;
        }
    }
}

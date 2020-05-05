using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CellLayout : MonoBehaviour
{
    [SerializeField]
    Image m_BackgroundImage = null;
    [SerializeField]
    TextMeshProUGUI m_Info = null;

    public string Info
    {
        get => m_Info.text;
        set => m_Info.text = value;
    }

    public void SetStatus(CellStatus s)
    {
        m_Info.text = ((int)s).ToString();
        m_BackgroundImage.color = GetColorByStatus(s);
    }

    Color GetColorByStatus(CellStatus s)
    {
        switch (s)
        {
            case CellStatus.Empty:
                return Color.clear;
            case CellStatus.Black:
                return Color.black;
            case CellStatus.White:
                return Color.white;
            default:
                throw new Exception();
        }
    }

    [ContextMenu("test - Empty")]
    void Test_SetToEmpty()
    {
        SetStatus(CellStatus.Empty);
    }

    [ContextMenu("test - Black")]
    void Test_SetToBlack()
    {
        SetStatus(CellStatus.Black);
    }

    [ContextMenu("test - White")]
    void Test_SetToWhite()
    {
        SetStatus(CellStatus.White);
    }
}
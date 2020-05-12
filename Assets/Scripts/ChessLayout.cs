using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChessLayout : MonoBehaviour, IChessLayout
{
    [SerializeField]
    Image m_Icon = null;
    [SerializeField]
    Color m_WhiteColor = Color.white;
    [SerializeField]
    Color m_BlackColor = Color.black;

    ChessType mChessType;
    public ChessType ChessType 
    {
        get => mChessType;
        set
        {
            mChessType = value;

            switch (mChessType)
            {
                case ChessType.Black:
                    m_Icon.color = m_BlackColor;
                    break;
                case ChessType.White:
                    m_Icon.color = m_WhiteColor;
                    break;
                default:
                    m_Icon.color = m_WhiteColor;
                    break;
            }
        }
    }

    public void AppendTo(Transform t)
    {
        transform.SetParent(t);
    }
}

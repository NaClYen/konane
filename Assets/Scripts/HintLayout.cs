using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HintLayout : MonoBehaviour, IHintLayout
{
    [SerializeField]
    Image m_Icon = null;

    HintType mHintType;
    public HintType HintType
    {
        get => mHintType;
        set
        {
            mHintType = value;

            switch (mHintType)
            {
                case HintType.CanAttack:
                    m_Icon.color = Color.red;
                    break;
                case HintType.CanGoTo:
                    m_Icon.color = Color.magenta; ;
                    break;
                default:
                    m_Icon.color = Color.green;
                    break;
            }
        }
    }

    public void AppendTo(Transform t)
    {
        transform.SetParent(t);

        // reset to fit parent
        GetComponent<RectTransform>().FitToParent();
    }

}

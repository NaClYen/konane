using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CellLayout : MonoBehaviour, ICellLayout
{
    [SerializeField]
    TextMeshProUGUI m_Info = null;

    IInfoCenter mInfoCenter;

    Button mButton;

    int mId;

    void Awake()
    {
        mButton = this.EnsureComponent<Button>();
    }

    public ICellLayout Init(IInfoCenter ic, int id)
    {
        mInfoCenter = ic;
        mId = id;

        void OnClick()
        {
            mInfoCenter.InvokeEvent("cell touched", mId);
        }

        mButton.onClick.AddListener(OnClick);

        return this;
    }

    public string Info
    {
        get => m_Info.text;
        set => m_Info.text = value;
    }

    public Transform Transform => transform;
}

public class FakeCellLayout : ICellLayout
{
    IInfoCenter mInfoCenter;
    int mId;

    public string Info { get; set; }

    public Transform Transform => null;

    public ICellLayout Init(IInfoCenter ic, int id)
    {
        mInfoCenter = ic;
        mId = id;

        return this;
    }

    public void OnClick()
    {
        mInfoCenter.InvokeEvent("cell touched", mId);
    }

}
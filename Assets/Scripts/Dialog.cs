using TMPro;
using UnityEngine;

public class Dialog : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI m_Message = null;

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show(string message)
    {
        m_Message.text = message;
        gameObject.SetActive(true);
    }
}

using TMPro;
using UnityEngine;

public class EmailMenu : MonoBehaviour
{
    [Header("REFERENCES")]
    public TMP_Dropdown reportType;
    public TMP_InputField reportBodyText;
    public TMP_InputField reportSubject;

    public void SendReport()
    {
        EmailHandler.SendEmail((reportType.value == 0 ? "New Feature Idea: " : "Bug: ") + reportSubject.text, reportBodyText.text);
    }
}

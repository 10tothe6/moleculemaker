using TMPro;
using UnityEngine;

public class LabelledLine : MonoBehaviour
{
    public Transform t_line;
    public TextMeshPro tx_label;

    void Update()
    {
        transform.position = t_line.transform.position;
        tx_label.text = (t_line.localScale.y * Exporter.Instance.scalingFactor * 100).ToString() + " cm";
    }
}

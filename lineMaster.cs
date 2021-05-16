using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class lineMaster : MonoBehaviour
{
    public GameObject label;
    public LineRenderer lr;
    public void forceUpdateLabel()
    {
        float angle = Mathf.Rad2Deg * Mathf.Atan2((lr.GetPosition(1).y), lr.GetPosition(1).x);
        float offset = (angle >= 90) ? 180 : 0; 
        label.transform.rotation = Quaternion.Euler(0, 0, -(offset - angle));
        label.transform.position = midpoint(offset);
    }

    public void setLabelText(string txt)
    {
        label.GetComponent<TextMeshProUGUI>().text = txt;
        forceUpdateLabel();
    }
    public Vector2 midpoint(float offset)
    {
        Vector3 mid = (lr.GetPosition(0) + lr.GetPosition(1)) / 2f;
        return new Vector2(mid.x, mid.y + (1 * 0.25f));
    }
}

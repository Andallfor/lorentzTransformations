using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class speedMaster : MonoBehaviour
{
    public TextMeshProUGUI display;
    public Slider sl;

    public void Update()
    {
        display.text = $"v = {System.Math.Round(sl.value, 2)}c";
    }
}

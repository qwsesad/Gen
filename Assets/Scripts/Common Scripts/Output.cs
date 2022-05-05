using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Output : MonoBehaviour
{
    public RectTransform txt;
    public RectTransform content;
    void Update()
    {
        var size = content.sizeDelta;
        size.y = txt.sizeDelta.y+5;
        size.x = txt.sizeDelta.x+5;
        content.sizeDelta = size;
    }
}

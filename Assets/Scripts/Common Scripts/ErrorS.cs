using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorS : MonoBehaviour
{
    public GameObject error;
    public Transform parent;
    public GameObject ALL;
    public Error er;
    public void SetUp(string text)
    {
        er.Problem(error, parent, ALL, text);
    }
}

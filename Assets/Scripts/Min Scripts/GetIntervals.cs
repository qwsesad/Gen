using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetIntervals : MonoBehaviour
{
    public double MinX;
    public double MaxX;
    public double MinY;
    public double MaxY;
    public InputField X1;
    public InputField X2;
    public InputField Y1;
    public InputField Y2;
    public ErrorS er;
    public bool Get()
    {
        if (X1.text != "")
            MinX = double.Parse(X1.text);
        else
        {
            er.SetUp("Enter MinX!");
            return false;
        }
        if (X2.text != "")
            MaxX = double.Parse(X2.text);
        else
        {
            er.SetUp("Enter MaxX!");
            return false;
        }
        if (Y1.text != "")
            MinY = double.Parse(Y1.text);
        else
        {
            MinY = -0.0123455;
        }
        if (Y2.text != "")
            MaxY = double.Parse(Y2.text);
        else
        {
            MaxY = 0.0123455;
        }
        return true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetFunction : MonoBehaviour
{
    public string func;
    public double step;
    public double eps;
    public int maxsteps;
    public int nind;
    //public InputField function;
    //public InputField STEP;
    public InputField EPS;
    public InputField MaxSteps;
    public InputField NInd;
    public ErrorS er;
    public bool Get()
    {
        /*
        if (function.text != "")
            func = function.text;
        else
        {
            er.SetUp("Enter function!");
            return false;
        }
        if (STEP.text != "")
            step = double.Parse(STEP.text);
        else
        {
            er.SetUp("Enter step!");
            return false;
        }
        */
        if (EPS.text != "")
            eps = double.Parse(EPS.text);
        else
        {
            er.SetUp("Enter eps!");
            return false;
        }
        if (MaxSteps.text != "")
            maxsteps = int.Parse(MaxSteps.text);
        else
        {
            er.SetUp("Enter max number of steps!");
            return false;
        }
        if (NInd.text != "")
            nind = int.Parse(NInd.text);
        else
        {
            er.SetUp("Enter number of individuals!");
            return false;
        }
        return true;
    }
}

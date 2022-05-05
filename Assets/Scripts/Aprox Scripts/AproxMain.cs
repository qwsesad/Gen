using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MathNet;
using MathNet.Symbolics;

public class AproxMain : MonoBehaviour
{
    public AproxDraw funcdraw;
    //public ComparisonDraw CompDraw;
    public ErrorS er;
    //public GetIntervals intervals;
    public GetFunction GetFunc;
    //public Dropdown MethodForSolving;
    //public Dropdown MethodForComparison;
    Vector3 Canvpos;
    // Start is called before the first frame update
    void Start()
    {

    }

    void Update()
    {

    }

    public void Solve()
    {
        funcdraw.GetObject().SetActive(true);
        //int method = MethodForSolving.value;

        if (!GetFunc.Get()) //проверяем ввод функции, шага и eps
            return;

        /*
        if (method == 0) //проверяем: выбран ли метод
        {
            er.SetUp("Choose method for solving!");
            return;
        }
        */

        // проверка введённых значений
        {
            if (GetFunc.eps <= 0)
            {
                er.SetUp("eps should be more than 0");
                return;
            }
        }

        // Передаём интервалы, функцию, шаг, eps и всё выполняем
        {

            //funcdraw.func = GetFunc.func;
            funcdraw.eps = GetFunc.eps;
            funcdraw.MaxSteps = GetFunc.maxsteps;
            funcdraw.NInd = GetFunc.nind;
            //funcdraw.Method = method;
            funcdraw.Go();
        }
    }


    private void OnDestroy()
    {
        funcdraw.DestroyObjects();
    }

    public void Exit()
    {
        funcdraw.DestroyObjects();
    }
}

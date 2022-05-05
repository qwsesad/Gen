using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading;
using System.Threading.Tasks;
using MathNet.Symbolics;
using Expr = MathNet.Symbolics.SymbolicExpression;
using Quaternion = UnityEngine.Quaternion;

public class FunctionDraw : MonoBehaviour
{
    public int N; //���������� ����� ��� �������
    //public int Method; //����� �������
    public int NInd; //���������� ������
    public int MaxSteps; //������������ ���������� ���������
    int NRoots; //���������� ������
    double YN; //������ y ������� ���������
    double YV; //������� y ������� ���������
    double XP; //������ x ������� ���������
    double XL; //����� x ������� ���������
    int NBytes;
    public bool MouseDown = false; //�������� ������� ����� ������ ���� � ������� ���������
    bool inside = false; //�������� ��������� ������� �� ������� ���������
    bool shift = false; //�������� ������� �� shift

    public double MinX; //����� �������� ��������� �� x
    public double MaxX; //������ �������� ��������� �� x
    public double MinY; //������ �������� ��������� �� y
    public double MaxY; //������� �������� ��������� �� y
    public double step; //��� �� x 
    public double eps; //����������� ��������
    public List<double> roots; //���� ������
    public string func; //������ �������
    Expr f; //�������
    Vector3 MouseStart; //���������� �����, ��� ������ ���� ���� ������
    Dictionary<string, FloatingPoint> symbols; //�������� �������

    List<Vector3> Tops; //���� ����� ��� �������
    LineRenderer function; //����� �������

    public GameObject x; //prefab ����
    public GameObject XDivision; //prefab ������� �� x
    public GameObject YDivision; //prefab ������� �� y
    public GameObject Circle; //prefab �����
    public GameObject LinePrefab; //prefab �����
    public GameObject Land; //������� ���������
    public ErrorS er; //���������� ������
    public Camera _camera; //������
    public Text Output; //������� ������

    System.Random rd;

    //������ �������
    public void Go()
    {
        DestroyObjects(); //������� ����� � ������ ��� �������, ����� ������� �� ������
        SetUp(); //������ ������ ����������
        CoordinateSystem(); //������ ������� ���������

        IntGen();
        //��������� ��������� ����� �������
        /*switch (Method)
        {
            case 1:
                break;
            case 2:
                break;
            default:
                break;
        }*/
        //Output.text += "Number of uniq dots:" + NRoots.ToString() + "\n";
        //SetRoots(); //����������� ����� �� �������
    }

    //�������������� ����������
    void SetUp()
    {
        rd = new System.Random();
        symbols = new Dictionary<string, FloatingPoint> { { "x", 2.0 } }; //������� ��������� �������
        roots = new List<double>(); //������� ����� ������
        f = Expr.Parse(func); //������� �������

        NRoots = 0; //�������� ���������� ������
        N = 0; //�������� ���������� ����� ��� �����
        NBytes = 32;

        XYPLVN(); //���������� ������� ���������
        MinMaxY(); //����� ����������
        Points(); //��������� �����
    }

    //��������� ������������ �������
    void CoordinateSystem()
    {
        GameObject YLine; //��� y
        GameObject XLine; //��� x
        Quaternion rotation; //�������� ��� ��� y
        Vector3 scalex = new Vector3(transform.localScale.x, 3, 0); //scale ��� x
        Vector3 scaley = new Vector3(transform.localScale.y, 3, 0); //scale ��� y

        double X0 = PreobrX((MaxX + MinX) / 2); //���� ��� ������� ��� y
        double Y0 = PreobrY((MaxY + MinY) / 2); //���� ��� ������� ��� x
        double XJust0 = PreobrX(0); //���� �� x
        double YJust0 = PreobrY(0); //���� �� y
        double XDivisionsStep = (MaxX - MinX) / 5; //��� ��� ������� �� x
        double YDivisionsStep = (MaxY - MinY) / 5; //��� ��� ������� �� y

        //YLine
        if (MinX * MaxX <= 0) //���� ��� y �����
        {
            rotation = new Quaternion(x.transform.rotation.x, x.transform.rotation.y, 1, x.transform.rotation.w);  //��� ��������� ��� x �� 90 ��������, ����� �������� ��� y
            YLine = Instantiate(x, new Vector3((float)XJust0, (float)Y0, 0), x.transform.rotation); //������ ��� y
            YLine.transform.localScale = scaley; //����������� ��� y
            YLine.transform.rotation = rotation; //������������ ��� y
            YLine.tag = "Line"; //����������� tag ��� �������

            //������ �������
            double i = MinY;
            while (i <= MaxY)
            {
                if (i != 0) //���� y �� ����� 0
                {
                    GameObject Division; //����� �������
                    Division = Instantiate(YDivision, new Vector3((float)XJust0, (float)PreobrY(i), 0), YDivision.transform.rotation); //������ �������
                    Division.transform.GetChild(0).gameObject.GetComponent<TextMesh>().text = (((double)((int)(i * 100))) / 100).ToString(); //������ �������
                    Division.tag = "Division"; //����������� tag ��� �������
                    i += YDivisionsStep; //��� � ���������� �������
                }
                else //���� y ����� 0
                    i += YDivisionsStep; //������� �������� �� ����, ��� ������
            }
        }

        //Xline
        if (MinY * MaxY <= 0) //���� ��� x �����
        {
            XLine = Instantiate(x, new Vector3((float)X0, (float)YJust0, 0), x.transform.rotation); //������ ��� x
            XLine.transform.localScale = scalex; //����������� ��� x
            XLine.tag = "Line"; //����������� tag ��� �������

            //������ �������
            double i = MinX;
            while (i <= MaxX)
            {
                if (i != 0) //���� x �� ����� 0
                {
                    GameObject Division; //����� �������
                    Division = Instantiate(XDivision, new Vector3((float)PreobrX(i), (float)YJust0, 0), XDivision.transform.rotation); //������ �������
                    Division.transform.GetChild(0).gameObject.GetComponent<TextMesh>().text = (((double)((int)(i * 100))) / 100).ToString(); //������ �������
                    Division.tag = "Division"; //����������� tag ��� �������
                    i += XDivisionsStep; //��� � ���������� �������
                }
                else //���� x ����� 0
                    i += XDivisionsStep; //������� �������� �� ����, ��� ������
            }
        }
    }

    //����� ����������
    void MinMaxY()
    {
        double Min = 0; //����������� �������� y �� ���������
        double Max = 0; //������������ �������� y �� ���������
        double prom = 0; //���������� �������� �������
        double next = 0; //��������� �������� �������
        double x1 = MinX; //��������� �������� ���������
        double x0 = MinX; //���������� �������� ���������
        bool check = false; //�������� ����������� �������

        //���� ������� �������� (�������� ������� � ������ ���������)
        while (!check && x1 <= MaxX)
        {
            try //���� ������� �� ����� �����������, �� ������������ ������� � catch
            {
                symbols.Remove("x");
                symbols.Add("x", x1);
                Min = Max = next = f.Evaluate(symbols).RealValue; //���������� �������� ������� � ������� ������ �������� ��� �������� � ���������

                check = true;
                prom = next;
                x0 = x1;
                x1 += step;
            }
            catch
            {
                x1 += step;
            }
        }

        // �������� ���� ������ ����������
        while (x1 <= MaxX)
        {
            try //���� ������� �� ����� �����������, �� ������������ ������� � catch
            {
                symbols.Remove("x");
                symbols.Add("x", x1);
                next = f.Evaluate(symbols).RealValue; //���������� �������� �������

                //����� ����������� � ������������ ��������
                if (next > Max)
                    Max = next;
                else if (next < Min)
                    Min = next;

                x0 = x1;
                x1 += step;
                prom = next;
            }
            catch //���� � ����� ������ �������
            {
                x1 += step;
                check = false;
                while (!check && x1 <= MaxX)
                {
                    try
                    {
                        symbols.Remove("x");
                        symbols.Add("x", x1);
                        next = f.Evaluate(symbols).RealValue;
                        check = true;
                        prom = next;
                        x0 = x1;
                        x1 += step;
                    }
                    catch
                    {
                        x1 += step;
                    }
                }
            }
        }

        //������ ���������� ��� y, ���� ��� �� ���� ������ ����������
        if (MinY == -0.0123455)
            MinY = Min;
        if (MaxY == 0.0123455)
            MaxY = Max;
    }

    //��������� ������� � ������� ����� ��� �����
    void Points()
    {
        double prom = 0;
        double x0 = MinX;
        double next = 0;

        Tops = new List<Vector3>(); //����� ���� ����� ��� �����

        while (x0 <= MaxX)
        {
            try //���� ������� �� ����� �����������, �� ������������ ������� � catch
            {
                symbols.Remove("x");
                symbols.Add("x", (x0 + x0 + step) / 2);
                next = f.Evaluate(symbols).RealValue; //��������� �������� ������� � �������� ���������
                symbols.Remove("x");
                symbols.Add("x", x0);
                prom = f.Evaluate(symbols).RealValue; //���������� �������� ������� ��� ����� x

                if (prom <= MaxY && prom >= MinY) //���� �������� ������� � �������� ��������� �� y
                {
                    Tops.Add(new Vector3((float)PreobrX(x0), (float)PreobrY(prom), 0));
                    N++;
                }
                if (next < MinY || next > MaxY) //���� ����� �� �������
                    Line(); //������ ����� �� �������
                x0 += step;
            }
            catch //���� � ����� ������ �������
            {
                Line(); //������ ����� �� �������
                x0 += step;
            }
        }
        Line(); //������ �����
    }

    //��������� �����
    void Line()
    {
        if (N != 0)
        {
            GameObject newline = Instantiate(LinePrefab, Vector3.zero, Quaternion.identity); //������ ����� �����
            function = newline.GetComponent<LineRenderer>(); //����� ����� ���������
            function.positionCount = N; //���������� ����� ��� �����

            //������ �����
            int z = 0;
            foreach (Vector3 top in Tops)
            {
                function.SetPosition(z, top);
                z++;
            }
            N = 0; //�������� ���������� ����� ��� �����
            Tops = new List<Vector3>(); //�������� ������ ����� ��� �����
        }
    }

    //������������ ��������� �� ������� �������� �������
    void SetRoots()
    {
        GameObject Root;
        float Y0 = (float)PreobrY(0);
        foreach (double root in roots)
        {
            Root = Instantiate(Circle, new Vector3((float)PreobrX(root), Y0, 3), Circle.transform.rotation);
        }
    }


    //���������� ��������� ������� ��������� �������
    void XYPLVN()
    {
        XP = transform.position.x + transform.localScale.x / 2;
        XL = transform.position.x - transform.localScale.x / 2;
        YN = transform.position.y - transform.localScale.y / 2;
        YV = transform.position.y + transform.localScale.y / 2;
    }


    double PreobrX(double x)//������������� ���������� x ��� ��
    {
        x = (x - MinX) * (XP - XL) / (MaxX - MinX) + XL;

        return x;
    }

    double PreobrY(double y)//������������� ���������� y ��� ��
    {
        y = (y - MinY) * (YN - YV) / (MinY - MaxY) + YN;
        return y;
    }

    //������� ������ � �����
    public void DestroyObjects()
    {
        GameObject[] Objects;

        Objects = GameObject.FindGameObjectsWithTag("Line");
        foreach (GameObject ob in Objects)
        {
            Destroy(ob);
        }

        Objects = GameObject.FindGameObjectsWithTag("Division");
        foreach (GameObject ob in Objects)
        {
            Destroy(ob);
        }

        Objects = GameObject.FindGameObjectsWithTag("Roots");
        foreach (GameObject ob in Objects)
        {
            Destroy(ob);
        }

        Output.text = "";
    }

    //�������� ������� �� ����� ������ ���� ������ ������� ���������
    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MouseDown = true;
            MouseStart = _camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _camera.transform.position.y));
        }
    }

    //�������� ���������� ����� ������ ���� ������ ������� ���������
    private void OnMouseUp()
    {
        if (Input.GetMouseButtonUp(0))
            MouseDown = false;
    }

    //��������� ������
    public GameObject GetObject()
    {
        return Land;
    }

    //�������� ���������� ������� � ������� ���������
    private void OnMouseEnter()
    {
        inside = true;
    }

    //�������� ������ ������� �� ������� ���������
    private void OnMouseExit()
    {
        inside = false;
    }

    //�������� ������� �� shift � ���������� �������
    private void Shift()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
            shift = true;
        if (Input.GetKeyUp(KeyCode.LeftShift))
            shift = false;
    }

    double Decode(int value)
    {
        return (value * (MaxX - MinX) / (Math.Pow(2, NBytes) - 1) + MinX);
    }

    int Encode(double value)
    {
        return Convert.ToInt32((value - MinX) * (Math.Pow(2, NBytes)) / (MaxX - MinX));
    }

    int Mutation(int value)
    {
        double Pm = 1.0 / NBytes;
        for (int i = 0; i < NBytes; i++)
        {
            if (Pm > rd.NextDouble())
            {
                if (((value >> i) & 1) == 1)
                {
                    int k = 1;
                    for (int j = 0; j < i; j++)
                    {
                        k = (k << 1) + 1;
                    }
                    value = (value >> (i + 1) << (i + 1)) + value & k;
                }
            }
        }
        return value;
    }

    int crossover1(int par1, int par2)
    {
        int z = rd.Next(1, NBytes - 1);
        int one = 1;
        for (int j = 0; j < z; j++)
        {
            one = one << 1 + 1;
        }
        return (par1 << z >> z) + (par2 & one);
    }
    int crossover2(int par1, int par2)
    {
        int z = rd.Next(1, NBytes - 1);
        int one = 1;
        for (int j = 0; j < z; j++)
        {
            one = one << 1 + 1;
        }
        return (par1 * one) + (par2 << z >> z);
    }

    void SetPoints(List<int> points)
    {
        GameObject[] Objects = GameObject.FindGameObjectsWithTag("Roots");
        foreach (GameObject ob in Objects)
        {
            Destroy(ob);
        }

        GameObject Root;
        foreach (int point in points)
        {
            double p = Decode(point);
            symbols.Remove("x");
            symbols.Add("x", p);
            double func = f.Evaluate(symbols).RealValue;
            if (p >= MinX && p <= MaxX)
                Root = Instantiate(Circle, new Vector3((float)PreobrX(p), (float)PreobrY(func), 3), Circle.transform.rotation);
        }
    }
    async private void IntGen()
    {
        int i = 0, cstep = 1;
        double sum = 0;
        bool check = true;
        NBytes = Convert.ToInt32(Math.Log((MaxX - MinX) / eps + 1)) + 1;
        List<int> Dots = new List<int>();
        List<int> NewDots = new List<int>();
        double[] ExParams = new double[NInd];
        for (i = 0; i < NInd; i++)
        {
            double value = rd.NextDouble() * (MaxX - MinX) + MinX;
            Dots.Add(Encode(value));
        }
        for (i = 0; i < NInd; i++)
        {
            symbols.Remove("x");
            symbols.Add("x", Decode(Dots[i]));
            double punch = f.Evaluate(symbols).RealValue;
            ExParams[i] = punch;
            sum += punch;
        }
        Output.text += $"{cstep} ���������: \n";
        for (i = 0; i < NInd; i++)
        {
            Output.text += $"[{ExParams[i]}] ";
        }
        Output.text += "\n";
        SetPoints(Dots);
        await Task.Delay(10);

        while (cstep <= MaxSteps && check)
        {
            check = false;
            for (i = 0; i < NInd; i++)
            {
                symbols.Remove("x");
                symbols.Add("x", Decode(Dots[i]));
                double punch = f.Evaluate(symbols).RealValue;
                ExParams[i] = punch;
                sum += punch;
                if (Math.Abs(ExParams[i] - 4) >= eps)
                {
                    check = true;
                }
                else
                {
                    check = false;
                }
            }
            if (check)
            { 
            double Min = ExParams[0];
            int m = 0;
            for (i = 0; i < NInd; i++)
            {
                if (Min > ExParams[i])
                {
                    Min = ExParams[i];
                    m = i;
                }
                int k = rd.Next(0, NInd - 1);
                int j = rd.Next(0, NInd - 1);
                if (ExParams[k] < ExParams[j])
                    NewDots.Add(Dots[k]);
                else
                    NewDots.Add(Dots[j]);
            }
            NewDots[NInd - 1] = Dots[m];
            Dots[NInd - 1] = Dots[m];
            i = 0;
            while (i < NInd - 1)
            {
                int j = rd.Next(0, NInd - 1);
                int k = rd.Next(0, NInd - 1);
                if (rd.NextDouble() > 0.6)
                {
                    Dots[i] = crossover1(NewDots[j], NewDots[k]);
                    Dots[i+1] = crossover2(NewDots[j], NewDots[k]);
                    i += 2;
                }
                else
                {
                    Dots[i] = NewDots[j];
                    Dots[i+1] = NewDots[k];
                    i += 2;
                }
            }
            for (i = 0; i < NInd; i++)
            {
                int punch = Mutation(Dots[i]);
                symbols.Remove("x");
                symbols.Add("x", Decode(punch));
            }
            SetPoints(Dots);
            await Task.Delay(10);
            cstep++;
            }
        }
        for (i = 0; i < NInd; i++)
        {
            symbols.Remove("x");
            symbols.Add("x", Decode(Dots[i]));
            double punch = f.Evaluate(symbols).RealValue;
            ExParams[i] = punch;
            sum += punch;
        }
        Output.text += $"{cstep - 1} ���������: \n";
        for (i = 0; i < NInd; i++)
        {
            Output.text += $"[{ExParams[i]}] ";
        }
        Output.text += "\n";
        SetPoints(Dots);
    }

    //��������� ������� ���������
    void Update()
    {
        Shift();
        if (MouseDown)
        {
            Vector3 newMousePosition = _camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _camera.transform.position.z));
            double newx = newMousePosition.x - MouseStart.x;
            double newy = newMousePosition.y - MouseStart.y;
            if (Math.Abs(newx) > 10)
            {
                MinX -= newx/1000;
                MaxX -= newx/1000;
            }
            if (Math.Abs(newy) > 1)
            {
                MinY -= newy / 500;
                MaxY -= newy / 500;
            }
            MouseStart.y = newMousePosition.y;
            Go();
        }
        if (inside)
        {
            float iz = Input.GetAxis("Mouse ScrollWheel");
            if (Math.Abs(iz) > 0)
            {
                double z;
                if (Math.Abs(iz) > 1)
                    iz *= (float)0.1;
                else if (Math.Abs(iz) < 1)
                    iz *= (float)10;
                if (!shift)
                {
                    if (Math.Abs(z = MinX - iz) > step)
                        MinX = z;
                    if (Math.Abs(z = MaxX + iz) > step)
                        MaxX = z;
                }
                else
                {
                    if (Math.Abs(z = MinY - iz) > step)
                        MinY = z;
                    if (Math.Abs(z = MaxY + iz) > step)
                        MaxY = z;
                }
                Go();
            }
        }
    }
}

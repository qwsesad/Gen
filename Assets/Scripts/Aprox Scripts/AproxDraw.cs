using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MathNet.Symbolics;
using Expr = MathNet.Symbolics.SymbolicExpression;
using Quaternion = UnityEngine.Quaternion;
using OfficeOpenXml;
using OfficeOpenXml.Style;

public class AproxDraw : MonoBehaviour
{
    public int N; //количество точек для графика
    //public int Method; //метод решения
    public int NInd; //количество особей
    public int MaxSteps; //максимальное количество поколений
    int NRoots; //количество корней
    double YN; //нижний y области рисования
    double YV; //верхний y области рисования
    double XP; //правый x области рисования
    double XL; //левый x области рисования
    double min, max;
    public double[,] Dot = new double [4,2];
    int NBytes;
    public bool MouseDown = false; //проверка нажатия левой кнопки мыши в области рисования
    bool inside = false; //проверка наведения курсора на область рисования
    bool shift = false; //проверка нажатия на shift

    
    public double eps; //ограничение точности
    public List<double> roots; //лист корней
    public string func; //строка функции
    Expr f; //функция
    Vector3 MouseStart; //координаты места, где кнопка мыши была нажата
    Dictionary<string, FloatingPoint> symbols; //основной словарь

    public List<Vector3> Tops; //лист точек 
    LineRenderer function; //линия графика


    public GameObject Circle; //prefab точки
    public GameObject LinePrefab; //prefab линии
    public GameObject Land; //область рисования
    public ErrorS er; //обработчик ошибок
    public Camera _camera; //камера
    public Text Output; //область вывода

    System.Random rd;

    private void Start()
    {
        Tops = new List<Vector3>();
        Dot[0, 0] = Dot[0, 1] = 10000; //MinX
        Dot[1, 0] = Dot[1, 1] = -10000; //MaxX
        Dot[2, 0] = Dot[2, 1] = 10000; //MinY
        Dot[3, 0] = Dot[3, 1] = -10000; //MaxY
    }

    //запуск решения
    public void Go()
    {
        SetUp(); //делаем первые объявления

        //IntGen();
        DoubGen();
        //запускаем выбранный метод решения
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
        //SetRoots(); //расставляем корни на графике
    }

    //первоначальные объявления
    void SetUp()
    {
        Output.text = "";
        rd = new System.Random();
        symbols = new Dictionary<string, FloatingPoint> { { "x", 2.0 } }; //задание основного словаря
        roots = new List<double>(); //задание листа корней
        //f = Expr.Parse(func); //парсинг функции

        NRoots = 0; //обнуляем количество корней
        N = 0; //обнуляем количество точек для линии
        XYPLVN();
    }

    void DestLines()
    {
        GameObject[] Objects;

        Objects = GameObject.FindGameObjectsWithTag("Line");
        foreach (GameObject ob in Objects)
        {
            Destroy(ob);
        }
    }

    //рисование линии
    void Line(double  a, double b)
    {
        GameObject newline = Instantiate(LinePrefab, Vector3.zero, Quaternion.identity); //создаём новую линию
        function = newline.GetComponent<LineRenderer>(); //задаём линии отрисовку
        function.positionCount = 2; //количество точек для линии

        //рисуем линию
        function.SetPosition(0, new Vector3((float)Dot[0,0], (float)(Dot[0, 0] * a + b), 3));
        function.SetPosition(1, new Vector3((float)Dot[1, 0], (float)(Dot[1, 0] * a + b), 3));
    }

    //вычисление координат области рисования
    void XYPLVN()
    {
        XP = transform.position.x + transform.localScale.x / 2;
        XL = transform.position.x - transform.localScale.x / 2;
        YN = transform.position.y - transform.localScale.y / 2;
        YV = transform.position.y + transform.localScale.y / 2;
    }

    //очистка памяти и формы
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

    //проверка нажатия на левую кнопку мыши внутри области рисования
    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MouseDown = true;
            MouseStart = _camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.x*2+10, _camera.transform.position.y));
            Tops.Add(MouseStart);
            Instantiate(Circle, MouseStart, Circle.transform.rotation);
            if (MouseStart.x < Dot[0, 0])
            {
                Dot[0, 0] = MouseStart.x;
                Dot[0, 1] = MouseStart.y;

            }
            if (MouseStart.x > Dot[1, 0])
            {
                Dot[1, 0] = MouseStart.x;
                Dot[1, 1] = MouseStart.y;
            }
            if (MouseStart.y < Dot[2, 1])
            {
                Dot[2, 0] = MouseStart.x;
                Dot[2, 1] = MouseStart.y;
            }
            if (MouseStart.y > Dot[3, 1])
            {
                Dot[3, 0] = MouseStart.x;
                Dot[3, 1] = MouseStart.y;
            }
        }
    }

    //проверка отпускания левой кнопки мыши внутри области рисования
    private void OnMouseUp()
    {
        if (Input.GetMouseButtonUp(0))
            MouseDown = false;
    }

    //отправить объект
    public GameObject GetObject()
    {
        return Land;
    }

    //проверка нахождения курсора в области рисования
    private void OnMouseEnter()
    {
        inside = true;
    }

    //проверка выхода курсора из области рисования
    private void OnMouseExit()
    {
        inside = false;
    }

    //проверка нажатия на shift и отпускания клавиши
    private void Shift()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
            shift = true;
        if (Input.GetKeyUp(KeyCode.LeftShift))
            shift = false;
    }

    //Декодирование
    double Decode(int value)
    {
        return (value * (max - min) / (Math.Pow(2, NBytes) - 1) + min);
    }

    //Кодирование
    int Encode(double value)
    {
        return Convert.ToInt32((value - min) * (Math.Pow(2, NBytes)-1) / (max - min)) ;
    }

    //Мутация для целочисленного кодирования
    int Mutation(int value)
    {
        double Pm = 1.0 / NBytes;
        for (int i = 0; i < NBytes; i++)
        {
            if (Pm > rd.NextDouble())
            {
                if (((value >> i) & 1) == 1)
                {
                    int k = 0;
                    for (int j = 0; j < i; j++)
                    {
                        k = (k << 1) + 1;
                    }
                    value = (value >> (i + 1) << (i + 1)) + value & k;
                }
                else
                {
                    int k = 0;
                    for (int j = 0; j < i; j++)
                    {
                        k = (k << 1) + 1;
                    }
                    value = (((value >> i) + 1) << i) + value & k;
                }
            }
        }
        return value;
    }

    //Мутация для вещественного кодирования
    double MutationDoub(double value)
    {
        double Pm = 1.0 / 2;
        if (Pm > rd.NextDouble())
        {
            value = value + rd.NextDouble() * (Math.Abs(value * 0.07) + Math.Abs(value * 0.07)) - Math.Abs(value * 0.07);
        }
        return value;
    }


    //Функция приспособленности
    double ExParam(double a, double b)
    {
        double Result = 0;
        foreach (Vector3 top in Tops)
        {
            Result += (top.x*a+b-top.y)* (top.x * a + b - top.y);
        }
        return Result;
    }

    //Скрещивание для первого потомка
    int crossover1(int par1, int par2, int z)
    {
        int one = 1;
        for (int j = 0; j < z; j++)
        {
            one = one << 1 + 1;
        }
        return (par1 << z >> z) + (par2 & one);
    }

    //Скрещивание для второго потомка
    int crossover2(int par1, int par2, int z)
    {
        int one = 1;
        for (int j = 0; j < z; j++)
        {
            one = one << 1 + 1;
        }
        return (par1 & one) + (par2 << z >> z);
    }

    //Генетический алгоритм с вещественным кодированием
    async private void DoubGen()
    {
        var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("Rep");
        int i = 0, cstep = 1;
        bool check = true;
        double amin, bmin, amax, bmax;

        min = 1;
        max = 11;
        amin = 1;
        amax = 3;
        bmin = 9;
        bmax = 11;

        double[,] Dots = new double[NInd, 2];
        double[,] NewDots = new double[NInd, 2];
        double[] ExParams = new double[NInd];
        DestLines();
        for (i = 0; i < NInd; i++)
        {
            double value = rd.NextDouble() * (amax - amin) + amin;
            Dots[i, 0] = value;
            value = rd.NextDouble() * (bmax - bmin) + bmin;
            Dots[i, 1] = value;
            Line(Dots[i, 0], Dots[i, 1]);
        }
        Output.text += $"{cstep - 1} \n";
        for (i = 0; i < NInd; i++)
        {
            Output.text += $"{Dots[i, 0]} , {Dots[i, 1]} \n";
        }
        await Task.Delay(10);

        while (cstep <= MaxSteps && check)
        {
            for (i = 0; i < NInd; i++)
            {
                ExParams[i] = ExParam(Dots[i, 0], Dots[i, 1]);
                sheet.Cells[cstep + 1, i + 1].Value = Math.Sqrt(ExParams[i]) / Tops.Count;
            }
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
                {
                    NewDots[i, 0] = Dots[k, 0];
                    NewDots[i, 1] = Dots[k, 1];
                }
                else
                {
                    NewDots[i, 0] = Dots[j, 0];
                    NewDots[i, 1] = Dots[j, 1];
                }
            }
            if (Math.Sqrt(Min) / Tops.Count >= eps)
            {
                NewDots[NInd - 1, 0] = Dots[m, 0];
                NewDots[NInd - 1, 1] = Dots[m, 1];
                Dots[NInd - 1, 0] = Dots[m, 0];
                Dots[NInd - 1, 1] = Dots[m, 1];
                i = 0;
                while (i < NInd - 1)
                {
                    int j = rd.Next(0, NInd - 1);
                    int k = rd.Next(0, NInd - 1);
                    if (rd.NextDouble() > 0.7)
                    {
                        int z = rd.Next(0, 1);
                        Dots[i, 0] = NewDots[j, 0];
                        Dots[i, 1] = NewDots[k, 1];
                        Dots[i + 1, 0] = NewDots[j, 0];
                        Dots[i + 1, 1] = NewDots[k, 1];
                        i += 2;
                    }
                    else
                    {
                        Dots[i, 0] = NewDots[j, 0];
                        Dots[i, 1] = NewDots[j, 1];
                        Dots[i + 1, 0] = NewDots[k, 0];
                        Dots[i + 1, 1] = NewDots[k, 1];
                        i += 2;
                    }
                }
                DestLines();
                for (i = 0; i < NInd; i++)
                {
                    Dots[i, 0] = MutationDoub(Dots[i, 0]);
                    Dots[i, 1] = MutationDoub(Dots[i, 1]);
                    Line(Dots[i, 0], Dots[i, 1]);
                }
                await Task.Delay(10);
                cstep++;
            }
            else
            {
                check = false;
                cstep++;
            }
        }
        Output.text += $"{cstep - 1} \n";
        for (i = 0; i < NInd; i++)
        {
            Output.text += $"{Dots[i, 0]} , {Dots[i, 1]} \n";
        }
        await Task.Delay(10);
        var reportExcel = package.GetAsByteArray();
        File.WriteAllBytes("Report.xlsx", reportExcel);
    }

    //Генетический алгоритм с целочисленным кодированием
    async private void IntGen()
    {
        var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("Rep");
        int i = 0, cstep = 1;
        bool check = true;
        double amin, bmin, amax, bmax;

        min = 1;
        max = 11;
        amin = 1;
        amax = 3;
        bmin = 9;
        bmax = 11;


        NBytes = Convert.ToInt32(Math.Log((max - min) / eps + 1));//
        Debug.Log(NBytes);
        int[,] Dots = new int[NInd,2];
        int[,] NewDots = new int[NInd, 2];
        double[] ExParams = new double[NInd];
        DestLines();

        //формирование первого поколения
        for (i = 0; i < NInd; i++)
        {
            double value = rd.NextDouble() * (amax - amin) + amin;
            Dots[i,0] = Encode(value);
            value = rd.NextDouble() * (bmax - bmin) + bmin;
            Dots[i, 1] = Encode(value);
            Line(Decode(Dots[i, 0]), Decode(Dots[i, 1]));
        }
        Output.text += $"{cstep - 1} \n";
        for (i = 0; i < NInd; i++)
        {
            Output.text += $"{Decode(Dots[i, 0])} , {Decode(Dots[i, 1])} \n";
        }
        await Task.Delay(10);

        //цикл поколений
        while (cstep <= MaxSteps && check == true)
        { 
            //оценка популяций
            for (i = 0; i < NInd; i++)
            {
                ExParams[i] = ExParam(Decode(Dots[i, 0]), Decode(Dots[i, 1]));
                sheet.Cells[cstep + 1, i + 1].Value = Math.Sqrt(ExParams[i])/Tops.Count;
            }

            //поиск лучшей особи и турнирный отбор
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
                {
                    NewDots[i, 0] = Dots[k, 0];
                    NewDots[i, 1] = Dots[k, 1];
                }
                else
                {
                    NewDots[i, 0] = Dots[j, 0];
                    NewDots[i, 1] = Dots[j, 1];
                }
            }
            //проверка на схождение
            if (Min >= eps)
            {
                //скрещивание и допуск лучшей особи
                NewDots[NInd - 1, 0] = Dots[m, 0];
                NewDots[NInd - 1, 1] = Dots[m, 1];
                Dots[NInd - 1, 0] = Dots[m, 0];
                Dots[NInd - 1, 1] = Dots[m, 1];
                i = 0;
                while (i < NInd - 1)
                {
                    int j = rd.Next(0, NInd - 1);
                    int k = rd.Next(0, NInd - 1);
                    if (rd.NextDouble() > 0.7)
                    {
                        int z = rd.Next(1, NBytes);
                        Dots[i, 0] = crossover1(NewDots[j, 0], NewDots[k, 0], z);
                        Dots[i, 1] = crossover1(NewDots[j, 1], NewDots[k, 1], z);
                        z = rd.Next(1, NBytes);
                        Dots[i + 1, 0] = crossover2(NewDots[j, 0], NewDots[k, 0], z);
                        Dots[i + 1, 1] = crossover2(NewDots[j, 1], NewDots[k, 1], z);
                        i += 2;
                    }
                    else
                    {
                        Dots[i, 0] = NewDots[j, 0];
                        Dots[i, 1] = NewDots[j, 1];
                        Dots[i + 1, 0] = NewDots[k, 0];
                        Dots[i + 1, 1] = NewDots[k, 1];
                        i += 2;
                    }
                }
                DestLines();

                //мутации
                for (i = 0; i < NInd; i++)
                {
                    Dots[i, 0] = Mutation(Dots[i, 0]);
                    Dots[i, 1] = Mutation(Dots[i, 1]);
                    Line(Decode(Dots[i, 0]), Decode(Dots[i, 1]));
                }
                await Task.Delay(10);
                cstep++;
            }
            else
            {
                check = false;
                cstep++;
            }
        }
        Output.text += $"{cstep - 1} \n";
        for (i = 0; i < NInd; i++)
        {
            Output.text += $"{Decode(Dots[i, 0])} , {Decode(Dots[i, 1])} \n";
        }
        await Task.Delay(10);
        var reportExcel = package.GetAsByteArray();
        File.WriteAllBytes("Report.xlsx", reportExcel);
    }

    //изменение области рисования
    void Update()
    {
        Shift();
    }
}

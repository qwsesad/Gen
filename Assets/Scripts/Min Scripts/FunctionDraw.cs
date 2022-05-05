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
    public int N; //количество точек для графика
    //public int Method; //метод решения
    public int NInd; //количество особей
    public int MaxSteps; //максимальное количество поколений
    int NRoots; //количество корней
    double YN; //нижний y области рисования
    double YV; //верхний y области рисования
    double XP; //правый x области рисования
    double XL; //левый x области рисования
    int NBytes;
    public bool MouseDown = false; //проверка нажатия левой кнопки мыши в области рисования
    bool inside = false; //проверка наведения курсора на область рисования
    bool shift = false; //проверка нажатия на shift

    public double MinX; //левое значение интервала по x
    public double MaxX; //правое значение интервала по x
    public double MinY; //нижнее значение интервала по y
    public double MaxY; //верхнее значение интервала по y
    public double step; //шаг по x 
    public double eps; //ограничение точности
    public List<double> roots; //лист корней
    public string func; //строка функции
    Expr f; //функция
    Vector3 MouseStart; //координаты места, где кнопка мыши была нажата
    Dictionary<string, FloatingPoint> symbols; //основной словарь

    List<Vector3> Tops; //лист точек для графика
    LineRenderer function; //линия графика

    public GameObject x; //prefab осей
    public GameObject XDivision; //prefab делений по x
    public GameObject YDivision; //prefab делений по y
    public GameObject Circle; //prefab точки
    public GameObject LinePrefab; //prefab линии
    public GameObject Land; //область рисования
    public ErrorS er; //обработчик ошибок
    public Camera _camera; //камера
    public Text Output; //область вывода

    System.Random rd;

    //запуск решения
    public void Go()
    {
        DestroyObjects(); //очищаем форму и память для случаев, когда решение не первое
        SetUp(); //делаем первые объявления
        CoordinateSystem(); //рисуем систему координат

        IntGen();
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
        rd = new System.Random();
        symbols = new Dictionary<string, FloatingPoint> { { "x", 2.0 } }; //задание основного словаря
        roots = new List<double>(); //задание листа корней
        f = Expr.Parse(func); //парсинг функции

        NRoots = 0; //обнуляем количество корней
        N = 0; //обнуляем количество точек для линии
        NBytes = 32;

        XYPLVN(); //координаты области рисования
        MinMaxY(); //поиск интервалов
        Points(); //рисование линии
    }

    //рисование координатной системы
    void CoordinateSystem()
    {
        GameObject YLine; //ось y
        GameObject XLine; //ось x
        Quaternion rotation; //вращение для оси y
        Vector3 scalex = new Vector3(transform.localScale.x, 3, 0); //scale для x
        Vector3 scaley = new Vector3(transform.localScale.y, 3, 0); //scale для y

        double X0 = PreobrX((MaxX + MinX) / 2); //ноль для задания оси y
        double Y0 = PreobrY((MaxY + MinY) / 2); //ноль для задания оси x
        double XJust0 = PreobrX(0); //ноль по x
        double YJust0 = PreobrY(0); //ноль по y
        double XDivisionsStep = (MaxX - MinX) / 5; //шаг для делений по x
        double YDivisionsStep = (MaxY - MinY) / 5; //шаг для делений по y

        //YLine
        if (MinX * MaxX <= 0) //Если ось y видно
        {
            rotation = new Quaternion(x.transform.rotation.x, x.transform.rotation.y, 1, x.transform.rotation.w);  //как повернуть ось x на 90 градусов, чтобы получить ось y
            YLine = Instantiate(x, new Vector3((float)XJust0, (float)Y0, 0), x.transform.rotation); //создаём ось y
            YLine.transform.localScale = scaley; //увеличиваем ось y
            YLine.transform.rotation = rotation; //поварачиваем ось y
            YLine.tag = "Line"; //присваеваем tag для очистки

            //рисуем деления
            double i = MinY;
            while (i <= MaxY)
            {
                if (i != 0) //если y не равно 0
                {
                    GameObject Division; //новое деление
                    Division = Instantiate(YDivision, new Vector3((float)XJust0, (float)PreobrY(i), 0), YDivision.transform.rotation); //создаём деление
                    Division.transform.GetChild(0).gameObject.GetComponent<TextMesh>().text = (((double)((int)(i * 100))) / 100).ToString(); //делаем подпись
                    Division.tag = "Division"; //присваеваем tag для очистки
                    i += YDivisionsStep; //идём к следующему делению
                }
                else //если y равно 0
                    i += YDivisionsStep; //деление рисовать не надо, идём дальше
            }
        }

        //Xline
        if (MinY * MaxY <= 0) //Если ось x видно
        {
            XLine = Instantiate(x, new Vector3((float)X0, (float)YJust0, 0), x.transform.rotation); //создаём ось x
            XLine.transform.localScale = scalex; //увеличиваем ось x
            XLine.tag = "Line"; //присваеваем tag для очистки

            //рисуем деления
            double i = MinX;
            while (i <= MaxX)
            {
                if (i != 0) //если x не равно 0
                {
                    GameObject Division; //новое деление
                    Division = Instantiate(XDivision, new Vector3((float)PreobrX(i), (float)YJust0, 0), XDivision.transform.rotation); //создаём деление
                    Division.transform.GetChild(0).gameObject.GetComponent<TextMesh>().text = (((double)((int)(i * 100))) / 100).ToString(); //делаем подпись
                    Division.tag = "Division"; //присваеваем tag для очистки
                    i += XDivisionsStep; //идём к следующему делению
                }
                else //если x равно 0
                    i += XDivisionsStep; //деление рисовать не надо, идём дальше
            }
        }
    }

    //поиск интервалов
    void MinMaxY()
    {
        double Min = 0; //минимальное значение y на интервале
        double Max = 0; //максимальное значение y на интервале
        double prom = 0; //предыдущее значение функции
        double next = 0; //следующее значение функции
        double x1 = MinX; //следующее значение аргумента
        double x0 = MinX; //предыдущее значение аргумента
        bool check = false; //проверка необходимых условий

        //цикл первого значения (проверка разрыва в начале интервала)
        while (!check && x1 <= MaxX)
        {
            try //если функции не будет вычисляться, то осуществится переход в catch
            {
                symbols.Remove("x");
                symbols.Add("x", x1);
                Min = Max = next = f.Evaluate(symbols).RealValue; //вычисление значение функции и задание первых значений для минимума и максимума

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

        // основной цикл поиска интервалов
        while (x1 <= MaxX)
        {
            try //если функции не будет вычисляться, то осуществится переход в catch
            {
                symbols.Remove("x");
                symbols.Add("x", x1);
                next = f.Evaluate(symbols).RealValue; //вычисление значение функции

                //поиск минимальных и максимальных значений
                if (next > Max)
                    Max = next;
                else if (next < Min)
                    Min = next;

                x0 = x1;
                x1 += step;
                prom = next;
            }
            catch //если в точке разрыв функции
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

        //запись интервалов для y, если они не были заданы изначально
        if (MinY == -0.0123455)
            MinY = Min;
        if (MaxY == 0.0123455)
            MaxY = Max;
    }

    //рисование графика и задание точек для линии
    void Points()
    {
        double prom = 0;
        double x0 = MinX;
        double next = 0;

        Tops = new List<Vector3>(); //задаём лист точек для линии

        while (x0 <= MaxX)
        {
            try //если функции не будет вычисляться, то осуществится переход в catch
            {
                symbols.Remove("x");
                symbols.Add("x", (x0 + x0 + step) / 2);
                next = f.Evaluate(symbols).RealValue; //вычисляем значение функции в середине интервала
                symbols.Remove("x");
                symbols.Add("x", x0);
                prom = f.Evaluate(symbols).RealValue; //вычсисляем значение функции при новом x

                if (prom <= MaxY && prom >= MinY) //если значение функции в пределах интервала по y
                {
                    Tops.Add(new Vector3((float)PreobrX(x0), (float)PreobrY(prom), 0));
                    N++;
                }
                if (next < MinY || next > MaxY) //если вышли за пределы
                    Line(); //рисуем линию до разрыва
                x0 += step;
            }
            catch //если в точке разрыв функции
            {
                Line(); //рисуем линию до разрыва
                x0 += step;
            }
        }
        Line(); //рисуем линию
    }

    //рисование линии
    void Line()
    {
        if (N != 0)
        {
            GameObject newline = Instantiate(LinePrefab, Vector3.zero, Quaternion.identity); //создаём новую линию
            function = newline.GetComponent<LineRenderer>(); //задаём линии отрисовку
            function.positionCount = N; //количество точек для линии

            //рисуем линию
            int z = 0;
            foreach (Vector3 top in Tops)
            {
                function.SetPosition(z, top);
                z++;
            }
            N = 0; //обнуляем крличество точек для линии
            Tops = new List<Vector3>(); //обнуляем списко точек для линии
        }
    }

    //проставление минимумов на графике красными точками
    void SetRoots()
    {
        GameObject Root;
        float Y0 = (float)PreobrY(0);
        foreach (double root in roots)
        {
            Root = Instantiate(Circle, new Vector3((float)PreobrX(root), Y0, 3), Circle.transform.rotation);
        }
    }


    //вычисление координат области рисования графика
    void XYPLVN()
    {
        XP = transform.position.x + transform.localScale.x / 2;
        XL = transform.position.x - transform.localScale.x / 2;
        YN = transform.position.y - transform.localScale.y / 2;
        YV = transform.position.y + transform.localScale.y / 2;
    }


    double PreobrX(double x)//Преобразовать координаты x под СК
    {
        x = (x - MinX) * (XP - XL) / (MaxX - MinX) + XL;

        return x;
    }

    double PreobrY(double y)//Преобразовать координаты y под СК
    {
        y = (y - MinY) * (YN - YV) / (MinY - MaxY) + YN;
        return y;
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
            MouseStart = _camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _camera.transform.position.y));
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
        Output.text += $"{cstep} поколение: \n";
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
        Output.text += $"{cstep - 1} поколение: \n";
        for (i = 0; i < NInd; i++)
        {
            Output.text += $"[{ExParams[i]}] ";
        }
        Output.text += "\n";
        SetPoints(Dots);
    }

    //изменение области рисования
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

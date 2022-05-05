using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using MathNet.Symbolics;
using Expr = MathNet.Symbolics.SymbolicExpression;
using Quaternion = UnityEngine.Quaternion;

public class ComparisonDraw: MonoBehaviour
{
    public int NInt; //количество интервалов
    public int Method; //первый метод
    public int Method2; //второй метод
    int NRoots1; //количество максимальных итераций для первого метода
    int NRoots2; //количество максимальных итераций для второго метода
    int N; //количество делений
    public double J; //переменная для прохода по eps
    double YN; //нижний y области рисования
    double YV; //верхний y области рисования
    double XP; //правый x области рисования
    double XL; //левый x области рисования
    double KMax; //максимальный k по eps
    double KMaxMax = 0; //максимальный k по методам
    public double mineps; //минимальная eps

    public double MinX; //левое значение интервала по x
    public double MaxX; //правое значение интервала по x
    public double MinY; //нижнее значение интервала по y
    public double MaxY; //верхнее значение интервала по y
    public double step; //шаг по x
    public double eps; //точность
    List<List<double>> intervals; //лист интервалов
    public string func; //строка функции
    Expr f; //функция
    Expr FirstDerivative; //первая производная
    Expr SecondDerivative; //вторая производная
    Dictionary<string, FloatingPoint> symbols; //основной словарь
    Dictionary<string, FloatingPoint> symbolsd1; //словарь для первой производной
    Dictionary<string, FloatingPoint> symbolsd2; //словарь для второй производной

    public List<double> Tops; //лист максимальных k
    LineRenderer function; //линия графика

    public GameObject c; //область рисования
    public GameObject x; //prefab осей
    public GameObject XDivision; //prefab делений по x
    public GameObject YDivision; //prefab делений по y
    public GameObject LinePrefab; //prefab линии
    public ErrorS er; //обработчик ошибок
    public Text Output; //область вывода


    //запуск сравнения
    public void Go()
    {
        DestroyObjects(); //очищаем форму и память для случаев, когда сравнение не первое
        SetUp(); //делаем первые объявления
        J = mineps; //минимальный eps 

        //легенда в зависимости от методов
        {
            switch (Method)
            {
                case 1:
                    c.GetComponentInChildren<TextMesh>().text += "Blue color is Newton's Method\n";
                    break;
                case 2:
                    c.GetComponentInChildren<TextMesh>().text += "Blue color is The Dichotomy Method\n";
                    break;
                default:
                    break;
            }
            switch (Method2)
            {
                case 1:
                    c.GetComponentInChildren<TextMesh>().text += "Red color is Newton's Method\n";
                    break;
                case 2:
                    c.GetComponentInChildren<TextMesh>().text += "Red color is The Dichotomy Method\n";
                    break;
                default:
                    break;
            }
        }

        Tops = new List<double>(); //задание листа итераций

        //цикл по точностям для первого метода
        for (int i = 0; i < N; i++)
        {
            switch (Method)
            {
                case 1:
                    Output.text += "Newton's Method\n";
                    NewtonsMethod();
                    break;
                case 2:
                    Output.text += "The Dichotomy Method\n";
                    TheDichotomyMethod();
                    break;
                default:
                    break;
            }
            J *= 10;
        }

        J = mineps; //снова задём минимальный eps

        //цикл по точностям для второго метода
        for (int i = 0; i < N; i++)
        {
            switch (Method2)
            {
                case 1:
                    Output.text += "Newton's Method\n";
                    NewtonsMethod();
                    break;
                case 2:
                    Output.text += "The Dichotomy Method\n";
                    TheDichotomyMethod();
                    break;
                default:
                    break;
            }
            J *= 10;
        }


        if (KMaxMax > 0) //если всё прошло успешно
        {
            CoordinateSystem(); //рисуем систему координат
            Points(new Color(0,0,1,1), new Color(0, 0, 1, 1), new Color(1, 0, 0, 1), new Color(1, 0, 0, 1)); //рисуем графики
        }
    }

    //первоначальные объявления
    void SetUp()
    {
        symbols = new Dictionary<string, FloatingPoint> { { "x", 2.0 } }; //задание основного словаря
        symbolsd1 = new Dictionary<string, FloatingPoint> { { "x", 2.0 } }; //задание словаря для первой производной
        symbolsd2 = new Dictionary<string, FloatingPoint> { { "x", 2.0 } }; //задание словаря для второй производной
        intervals = new List<List<double>>(); //задание листа интервалов

        f = Expr.Parse(func); //парсинг функции
        var x_ = Expr.Variable("x"); //задание переменной, по которой будут вычисляться производные
        FirstDerivative = f.Differentiate(x_); //функция первой производной
        SecondDerivative = FirstDerivative.Differentiate(x_); //функция второй производной

        NInt = 0; //обнуляем количество интервалов
        NRoots1 = 0; //обнуляем количество максимальных итераций для первого метода
        NRoots2 = 0; //обнуляем количество максимальных итераций для второго метода
        N = 5; //количество делений

        //минимальный eps в зависимости от количества делений
        mineps = eps;
        for (int i = 1; i < N; i++)
        {
            mineps /= 10;
        }

        XYPLVN(); //координаты области рисования
        MinMax(); //поиск интервалов
    }

    //рисование координатной системы
    void CoordinateSystem()
    {
        GameObject YLine; //ось y
        GameObject XLine; //ось x
        Quaternion rotation; //вращение для оси y
        Vector3 scalex = new Vector3(transform.localScale.x, 3, 0); //scale для x
        Vector3 scaley = new Vector3(transform.localScale.y, 3, 0); //scale для y

        double X0 = PreobrX(2.5); //ноль для задания оси y
        double Y0 = PreobrY(KMaxMax / 2); //ноль для задания оси x
        double XJust0 = PreobrX(0); //ноль по x
        double YJust0 = PreobrY(0); //ноль по y
        double YDivisionsStep = KMaxMax / 4; //шаг для делений по y

        rotation = new Quaternion(x.transform.rotation.x, x.transform.rotation.y, 1, x.transform.rotation.w); //как повернуть ось x на 90 градусов, чтобы получить ось y
        YLine = Instantiate(x, new Vector3((float)XJust0, (float)Y0, 0), x.transform.rotation); //создаём ось y
        YLine.transform.localScale = scaley; //увеличиваем ось y
        YLine.transform.rotation = rotation; //поварачиваем ось y
        YLine.tag = "Line2"; //присваеваем tag для очистки

        //рисуем деления
        double i = YDivisionsStep;
        while (i <= KMaxMax)
        {
            GameObject Division; //новое деление
            Division = Instantiate(YDivision, new Vector3((float)XJust0, (float)PreobrY(i), 0), YDivision.transform.rotation); //создаём деление
            Division.transform.GetChild(0).gameObject.GetComponent<TextMesh>().text = (i).ToString(); //делаем подпись
            Division.tag = "Division2"; //присваеваем tag для очистки
            i += YDivisionsStep; //идём к следующему делению
        }

        XLine = Instantiate(x, new Vector3((float)X0, (float)YJust0, 0), x.transform.rotation); //создаём ось x
        XLine.transform.localScale = scalex; //увеличиваем ось x
        XLine.tag = "Line2"; //присваеваем tag для очистки

        //рисуем деления
        i = mineps;
        for (int j = 1; j <= N; j++)
        {

            GameObject Division; //новое деление
            Division = Instantiate(XDivision, new Vector3((float)PreobrX(j), (float)YJust0, 0), XDivision.transform.rotation); //создаём деление
            Division.transform.GetChild(0).gameObject.GetComponent<TextMesh>().text = (i).ToString(); //делаем подпись
            Division.tag = "Division2"; //присваеваем tag для очистки
            i *= 10; //идём к следующему делению
        }
    }

    //поиск интервалов
    void MinMax()
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

                try //если производные не будут вычисляться, то значения не войдут в интервалы
                {
                    //проверка необходимых условий интервала
                    if ((prom * next) <= 0 && (Method == 1 || Method2 == 1)) //проверка условия разных знаков и дальнейшие проверки, если испольхуется метод ньютона
                    {
                        double d1_1;
                        double d1_2;

                        symbolsd1.Remove("x");
                        symbolsd1.Add("x", x0);
                        d1_1 = FirstDerivative.Evaluate(symbols).RealValue; //первая производная на левом значении аргумента

                        symbolsd1.Remove("x");
                        symbolsd1.Add("x", x1);
                        d1_2 = FirstDerivative.Evaluate(symbols).RealValue; //первая производная на правом значении аргумента

                        //если первая производныя и на левом значении аргумента, и на правом не равна нулю
                        if (d1_1 * d1_2 != 0)
                        {
                            double d2_1;
                            double d2_2;

                            symbolsd2.Remove("x");
                            symbolsd2.Add("x", x0); 
                            d2_1 = SecondDerivative.Evaluate(symbols).RealValue; //вторая производная на левом значении аргумента

                            symbolsd1.Remove("x");
                            symbolsd1.Add("x", x1);
                            d2_2 = SecondDerivative.Evaluate(symbols).RealValue; //вторая производная на правом значении аргумента

                            //если вторая производныя и на левом значении аргумента, и на правом не равна нулю
                            if (d2_1 * d2_2 != 0)
                            {
                                //добавляем интервал
                                intervals.Add(new List<double>());
                                intervals[NInt].Add(x0);
                                intervals[NInt].Add(x1);
                                NInt++;
                            }
                        }
                    }
                    else if ((prom * next) <= 0) //если разные знаки и используется не метод ньютона
                    {
                        //добавляем интервал
                        intervals.Add(new List<double>());
                        intervals[NInt].Add(x0);
                        intervals[NInt].Add(x1);
                        NInt++;
                    }
                }
                catch 
                {
                    //если где-то производная не вычислились для метода ньютона, то значения аргументов не входят в интервал
                }

                x0 = x1;
                x1 += step;
                prom = next;
            }
            catch //если в точке разрыв функции, пропускаем разрыв
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

    //рисование графиков
    void Points(Color start, Color end, Color start2, Color end2)
    {
        int i = 0;
        if (NRoots1 > 0) //если были вычисления у первого метода
        {
            GameObject newline = Instantiate(LinePrefab, Vector3.zero, Quaternion.identity); //создаём новую линию
            function = newline.GetComponent<LineRenderer>(); //задаём линии отрисовку
            function.positionCount = NRoots1; //количество точек для линии
            function.startColor = start; //начальный цвет
            function.endColor = end; //конечный цвет
            newline.tag = "Line2";

            //рисуем линию
            for (; i < NRoots1; i++)
            {
                function.SetPosition(i, new Vector3((float)PreobrX(i + 1), (float)PreobrY(Tops[i]), 0));
            }
        }
        if (NRoots2 > 0) //если были вычисления у второго метода
        {
            GameObject newline = Instantiate(LinePrefab, Vector3.zero, Quaternion.identity); //создаём новую линию
            function = newline.GetComponent<LineRenderer>(); //задаём линии отрисовку
            function.positionCount = NRoots2; //количество точек для линии
            function.startColor = start2; //начальный цвет
            function.endColor = end2; //конечный цвет
            newline.tag = "Line2";

            //рисуем линию
            for (int t = 0; t < NRoots2; t++)
            {
                function.SetPosition(t, new Vector3((float)PreobrX(t + 1), (float)PreobrY(Tops[i]), 0));
                i++;
            }
        }
    }

    void NewtonsMethod()
    {
        int i;
        double S; //значение функции
        double S1; //значение первой производной
        double S2; //значение второй производной
        double x0; //корень
        bool check; //проверка необходимых условий
        int k; //количество итераций
        double H; //f(x)/f'(x)
        double Min; //минимальное значение первой производной на интервале
        double Max; //максимальное значение второй производной на интервале
        double A = 0; //предыдущее значение функции
        KMax = 0; //максимальное количество итераций при разных точностях

        //цикл по интервалам
        for (i = 0; i < NInt; i++)
        {
            k = 1;
            check = true;
            H = 0;
            x0 = 0;
            Max = 0;
            Min = 0;
            try
            {
                double S11; //первая производная на левом значении интервалов
                double S21; //вторая производная на левом значении интервалов
                double S12; //первая производная на правом значении интервалов
                double S22; //вторая производная на правом значении интервалов
                symbols.Remove("x");
                symbols.Add("x", intervals[i][0]);
                S = f.Evaluate(symbols).RealValue; //вычисление функции на левом значении интервала
                S2 = SecondDerivative.Evaluate(symbols).RealValue; //вычисление второй производной на левом значении интервала
                S1 = FirstDerivative.Evaluate(symbols).RealValue; //вычисление первой производной на левом значении интервала
                S11 = Math.Abs(S1);
                S21 = Math.Abs(S2);

                //выбор первоначального приближения
                if (S * S2 > 0) //если значение функции и второй производной больше нуля на левом значении интервала
                    x0 = intervals[i][0]; //левое значение
                else //если значение функции и второй производной не больше нуля на левом значении интервала
                {
                    symbols.Remove("x");
                    symbols.Add("x", intervals[i][1]);
                    S = f.Evaluate(symbols).RealValue; //вычисление функции на правом значении интервала
                    S2 = SecondDerivative.Evaluate(symbols).RealValue; //вычисление второй производной на правом значении интервала

                    if (S * S2 > 0) //если значение функции и второй производной больше нуля на правом значении интервала 
                    {
                        x0 = intervals[i][1]; //певрое приближение корня
                    }
                    else //если значение функции и второй производной везде не больше нуля
                        check = false; //необходимое условие не выполняется
                }

                S1 = FirstDerivative.Evaluate(symbols).RealValue; //вычисление первой производной на первоначальном приближении
                if (S1 != 0) //если первая производная не равна нулю
                {
                    A = S = f.Evaluate(symbols).RealValue; //вычисление функции
                    H = S / S1;
                }
                else //если первая производная равна нулю
                    check = false; //необходимое условие не выполняется
                x0 -= H; //первая итерация

                symbols.Remove("x");
                symbols.Add("x", intervals[i][1]);
                S12 = Math.Abs(FirstDerivative.Evaluate(symbols).RealValue); //вычисление первой производной на правом значении интервала
                S22 = Math.Abs(SecondDerivative.Evaluate(symbols).RealValue); //вычисление второй производной на правом значении интервала

                //поиск первого минимума для первой производной
                if (S11 < S12)
                    Min = S11;
                else
                    Min = S12;

                //поиск первого максимума для второй производной
                if (S22 > S21)
                    Max = S22;
                else
                    Max = S21;

                //цикл итераций с проверкой необходимых условий и оценкой точности
                while (check && (Math.Abs(Max / (Min * 2) * H * H) >= J))
                {
                    k++;
                    symbols.Remove("x");
                    symbols.Add("x", x0);
                    S1 = FirstDerivative.Evaluate(symbols).RealValue; //вычисление первой производной
                    S2 = SecondDerivative.Evaluate(symbols).RealValue; //вычисление второй производной
                    if (S1 != 0) //если первая производная не равна нулю 
                    {
                        A = S;
                        S = f.Evaluate(symbols).RealValue; //вычисление функции
                        H = S / S1;
                    }
                    else //если первая производная равна нулю
                        check = false; //необходимое условие не выполняется
                    x0 -= H; //следующее приближение корня

                    //поиск минимума для первой производной
                    S11 = Math.Abs(S1);
                    if (S11 < Min)
                        Min = S11;

                    //поиск максимума для второй производной
                    S22 = Math.Abs(S2);
                    if (S22 > Max)
                        Max = S22;

                    if (S2 == 0)
                        check = false;

                    if (Math.Abs(A) < Math.Abs(S)) //если есть разрыв
                        check = false; //необходимое условие не выполняется

                    if (x0 > intervals[i][1] || x0 < intervals[i][0]) //если корень вышел из интервала
                        check = false; //необходимое условие не выполняется

                    if (k > 1000) //если число итераций больше 1000
                    {
                        er.SetUp("Sorry( K > 1000. We have to stop it"); //вывод ошибки
                        check = false; //прерывание цикла во избежании бесконечных или очень долгих вычислений
                    }
                }

                //перед добавление корня в список проверяем были ли выполнены необходимые условия на интервале
                if (check) //если условия выполнены
                {
                    symbols.Remove("x");
                    symbols.Add("x", x0);
                    S = f.Evaluate(symbols).RealValue; //вычисление функции в корне

                    //вывод
                    Output.text += "Root: \n";
                    Output.text += "eps = " + J.ToString() + "\n";
                    Output.text += "a = " + intervals[i][0].ToString() + "\n";
                    Output.text += "b = " + intervals[i][1].ToString() + "\n";
                    Output.text += "k = " + k.ToString() + "\n";
                    Output.text += "x = " + x0.ToString() + "\n";
                    Output.text += "y(x) = " + S.ToString() + "\n\n";

                    //поиск максимального k
                    if (k > KMax)
                        KMax = k;
                }
            }
            catch { }

        }
        //поиск максимального k при разных методах
        if (KMax > KMaxMax) 
            KMaxMax = KMax;
        Tops.Add(KMax);
        NRoots1++;
    }

    //Метод дихотомии
    void TheDichotomyMethod()
    {
        int i;
        double A; //левое значение функции
        double B; //правое значение функции
        double FC; //значение функции по середине
        double c; //аргумент по середине
        double x1; //левое значение интервала
        double x2; //правое значение интервала
        bool check; //проверка необходимых условий
        int k; //количество итераций
        KMax = 0; //максимальное количество итераций

        //цикл по интервалам
        for (i = 0; i < NInt; i++)
        {
            k = 0;
            check = true;
            c = 0;
            FC = 0;
            try //если функции или производные не будут вычисляться, то осуществится переход в catch
            {
                x1 = intervals[i][0];
                x2 = intervals[i][1];
                symbols.Remove("x");
                symbols.Add("x", x1);
                A = f.Evaluate(symbols).RealValue; //вычисление функции на левом значении интервала
                symbols.Remove("x");
                symbols.Add("x", x2); 
                B = f.Evaluate(symbols).RealValue; //вычисление функции на правом значении интервала

                //цикл итераций с проверкой необходимых условий и оценкой точности
                while (check && (Math.Abs(x2 - x1) >= J))
                {
                    c = (x1 + x2) / 2; //вычисление значения аргумента по середине
                    symbols.Remove("x");
                    symbols.Add("x", c);
                    FC = f.Evaluate(symbols).RealValue; //вычисление значения функции по середине

                    if (FC * B >= 0 && Math.Abs(FC) <= Math.Abs(B)) //если одинаковые знаки значения функции по середине и правого значение функции, или одно из значений равно нулю, а также нет разрыва
                    {
                        B = FC; //правое значение функции становится равно значению функции по середине
                        x2 = c; //правое значение интервала становится равно аргументу по середине
                    }
                    else if (FC * A >= 0 && Math.Abs(FC) <= Math.Abs(A)) //если одинаковые знаки значения функции по середине и левого значение функции, или одно из значений равно нулю, а также нет разрыва
                    {
                        A = FC; //левое значение функции становится равно значению функции по середине
                        x1 = c; //левое значение интервала становится равно аргументу по середине
                    }
                    else //если появился разрыв или проблема с интервалами
                        check = false; //необходимое условие не выполняется

                    if (c > intervals[i][1] || c < intervals[i][0]) //если корень вышел из интервала
                        check = false; //необходимое условие не выполняется

                    k++;
                    if (k > 1000) //если число итераций больше 1000
                    {
                        er.SetUp("Sorry( K > 1000. We have to stop it"); //вывод ошибки
                        check = false; //прерывание цикла во избежании бесконечных или очень долгих вычислений
                    } 
                }

                if (check)
                {
                    //вывод
                    Output.text += "Root: \n";
                    Output.text += "eps = " + J.ToString() + "\n";
                    Output.text += "a = " + intervals[i][0].ToString() + "\n";
                    Output.text += "b = " + intervals[i][1].ToString() + "\n";
                    Output.text += "k = " + k.ToString() + "\n";
                    Output.text += "x = " + c.ToString() + "\n";
                    Output.text += "y(x) = " + FC.ToString() + "\n\n";

                    //поиск максимального значения k
                    if (k > KMax)
                        KMax = k;
                }
            }
            catch 
            {
                //если где-то не удалось вычислить функцию, то интервал пропускается
            }


        }

        //поиск максимального k при разных методах
        if (KMax > KMaxMax)
            KMaxMax = KMax;
        Tops.Add(KMax);
        NRoots2++;
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
        x = x * (XP - XL) / 5 + XL;

        return x;
    }

    double PreobrY(double y)//Преобразовать координаты y под СК
    {
        y = y * (YN - YV) / (-KMaxMax) + YN;
        return y;
    }


    //очистка памяти и формы
    public void DestroyObjects()
    {
        GameObject[] Objects;

        Objects = GameObject.FindGameObjectsWithTag("Line2");
        foreach (GameObject ob in Objects)
        {
            Destroy(ob);
        }

        Objects = GameObject.FindGameObjectsWithTag("Division2");
        foreach (GameObject ob in Objects)
        {
            Destroy(ob);
        }

        Output.text = "";
        c.GetComponentInChildren<TextMesh>().text = "";

        KMax = 0;
        KMaxMax = 0;
    }

}

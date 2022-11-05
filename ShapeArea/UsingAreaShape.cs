using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ShapeArea
{
    class UsingAreaShape
    {
        public List<CreateShape> shapes { get; set; } = new();
        public const string p = "((a+b+c)/2)";
        public const string geronFormul = $"[{p}*({p}-a)*({p}-b)*({p}-c)]" /* + "+ [4] - [9]" */;//тестировал несколько корней
        public const string pi = "3.14";
        public const string circleArea = $"{pi}*rad*rad";
        public UsingAreaShape()
        {
            CreateShape.Documentation();

            shapes.Add(new CreateShape("Circle", 3, 0, circleArea, isDefault: true));

            shapes.Add(new CreateShape("Triangle", 0, 3, geronFormul, isDefault: true));
            shapes[1].SetSides(3, 4, 5);

            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("\nВыберите действие\n1 - посмотреть существующие фигуры" +
                    "\n2 - Создать свою фигуру\n3 - Посчитать площадь фигуры, 4 - выход\n");
                string input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        ShowShapes();
                        break;
                    case "2":
                        CreateCustomShape();
                        break;
                    case "3":
                        Calc();
                        break;
                    case "4":
                        exit = true;
                        break;
                }
            }
        }
        
        private void Calc()
        {
            Console.WriteLine("Выберите номер фигуры");
            ShowShapes();
            int index = int.Parse(Console.ReadLine());
            CalculateShape ch = new(shapes[index]);
            float s = ch.Calculate();
        }


        private void ShowShapes()
        {
            Console.WriteLine();
            for (int i = 0; i < shapes.Count; i++)
            {
                Console.WriteLine(i + " : " + shapes[i].name);
            }
        }

        private void CreateCustomShape()
        {
            string name = "";
            int rad = 0;
            int sidesCount = 0;
            string formula = "";
            for (int i = 0; i < 4; i++)
            {
                switch (i)
                {
                    case 0:
                        Console.WriteLine("Задайте имя фигуре");
                        name = Console.ReadLine();
                        break;
                    case 1:
                        Console.WriteLine("Задайте радиус");
                        rad = int.Parse(Console.ReadLine());
                        break;
                    case 2:
                        Console.WriteLine("Задайте количество сторон");
                        sidesCount = int.Parse(Console.ReadLine());
                        break;
                    case 3:
                        Console.WriteLine("Задайте формулу расчета площади фигуры\nк примеру: ((a+b+c)/2)*[25]");
                        formula = Console.ReadLine();
                        break;
                }
                
            }
            shapes.Add(new CreateShape(name, rad, sidesCount, formula));
            Console.WriteLine("Фигуры была успешна добавлена");
        }
    }


    class CalculateShape
    {
        public CreateShape createShape { get; set; }
        private static DataTable Table { get; } = new DataTable();
        public static double Calc(string Expression) => Convert.ToDouble(Table.Compute(Expression, "Не верный формат формулы"));
        private string format { get; set; }
        public CalculateShape(CreateShape createShape)
        {
            this.createShape = createShape;
        }

        private void CalculateOrigins()
        {
            // достаю внутренности корня
            var match = Regex.Matches(format, "\\[(.*?)\\]");
            for (int i = 0; i < match.Count; i++)
            {
                // извлекаю корень
                double mtemp = Math.Sqrt(Convert.ToDouble(new DataTable().Compute(match[i].Groups[1].Value, "")));
                float sqrt = float.Parse(mtemp.ToString());
                // ставлю значение в строку
                format = format.Replace($"[{match[i].Groups[1].Value}]", sqrt.ToString());
                // заменяю запятые точками для дальнейших вычислений
                // P.S. если переводить не целое число в строку, то точки превратятся в запятые
                // а когда придется выполнять обратную операцию(без нижней), то компилятор умрет:)

            }
            format = format.Replace(",", ".");
        }

        private void CalculateRadius()
        {
            format = format.Replace("rad", createShape.radius.ToString());
        }

        private void CalculateSides()
        {
            for (int i = 0; i < createShape.characters.Count; i++)
            {
                string currentChar = createShape.characters[i];
                if (format.Contains(currentChar))
                {
                    format = format.Replace(currentChar, createShape.sides[i].ToString());
                }
            }

        }


        private string CheckRightTriangle()
        {
            int max = createShape.sides.Max();
            double cathets = 0;
            for (int i = 0; i < createShape.sidesCount; i++)
            {
                if (max != createShape.sides[i])
                {
                    cathets += Math.Pow(createShape.sides[i], 2);
                }
            }
            if (Math.Pow(max, 2) == cathets)
            {
                return "";
            }
            return "не ";
        }

        public float Calculate()
        {
            if (createShape.name == null)
            {
                Console.WriteLine("Фигура не найдена");
                return 0;
            }

            format = createShape.formula;

            CalculateRadius();

            CalculateSides();

            CalculateOrigins();

            // вычисляю всё
            float S = float.Parse((new DataTable().Compute(format, "")).ToString());

            Results(S);

            return S;
        }

        private void Results(float S)
        {
            Console.WriteLine($"{createShape.name}:\nS = {S}");
            if (createShape.sidesCount == 3)
            {
                Console.WriteLine($"Ваш треугольник {CheckRightTriangle()}прямоугольный");
            }
        }
    }


    class CreateShape
    {
        public string name { get; set; }
        public int radius { get; set; }
        public int sidesCount { get; set; }
        public List<int> sides { get; set; } = new();
        public string formula { get; set; }
        public List<string> characters { get; set; } = new();
        public string className { get; } = "Create shape";
        private string ClassSay()
        {
            return className + ": ";
        }

        public static void Documentation()
        {
            Console.WriteLine("для написания формулы:\na-z - стороны, " +
                "rad - радиус, (+,-,*,/) - операторы, origin[x] - извлечение из-под корня");
        }


        public CreateShape(string name, int radius, int sidesCount, string formula, bool isDefault = false)
        {
            if (sidesCount < 3 && radius == 0)
            {
                Console.WriteLine(ClassSay() + "Сторон не может быть меньше 3");
                return;
            }
            else if(radius > 0 && sidesCount > 0)
            {
                Console.WriteLine("Такой случай не предусмотрен");
                return;
            }
            else if(sidesCount < 0 || radius < 0)
            {
                Console.WriteLine("Такой формат не допустим");
                return;
            }
            
            this.name = name;
            this.radius = radius;
            this.sidesCount = sidesCount;
            this.formula = formula;
            for (char i = 'a'; i < 'z'; i++)
                characters.Add(i.ToString());
            if (!isDefault)
            {
                Console.WriteLine(ClassSay() + "Введите значения сторон");
                for (int i = 0; i < sidesCount; i++)
                {
                    string s = null;
                    while (s == null)
                    {
                        Console.WriteLine(ClassSay() + $"сторона {characters[i]}");
                        s = Console.ReadLine();
                        try
                        {
                            sides.Add(int.Parse(s));
                        }
                        catch
                        {
                            Console.WriteLine(ClassSay() + "было введено не число");
                            s = null;
                        }
                    }
                }
            }

        }

        public void SetSides(params int[] _sides)
        {
            for (int i = 0; i < sidesCount; i++)
            {
                this.sides.Add(_sides[i]);
            }
        }

    }
}

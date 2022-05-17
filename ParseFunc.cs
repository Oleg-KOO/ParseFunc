using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

class HelloWorld {

    public static class ParseFunc
    {    
        public static ParameterExpression x;
        private static Dictionary<string, Func<Expression, Expression, Expression>> operations;
        
        static ParseFunc()
        {
            operations = new Dictionary<string, Func<Expression, Expression, Expression>>();
            x = Expression.Parameter(typeof(double), "x");

            operations.Add("*", (one, two) => Expression.Multiply(one, two));
            operations.Add("/", (one, two) => Expression.Divide(one, two));
            operations.Add("+", (one, two) => Expression.Add(one, two));
            operations.Add("-", (one, two) => Expression.Subtract(one, two));
            operations.Add("^", (one, two) => Expression.Power(one, two));
            operations.Add("abs", (one, two) => Expression.Call(typeof(Math).GetMethod("Abs", new[] { typeof(double) }), one)); // модуль
            operations.Add("sin", (one, two) => Expression.Call(typeof(Math).GetMethod("Sin", new[] { typeof(double) }), one));
            operations.Add("cos", (one, two) => Expression.Call(typeof(Math).GetMethod("Cos", new[] { typeof(double) }), one));
            operations.Add("lg", (one, two) => Expression.Call(typeof(Math).GetMethod("Log10", new[] { typeof(double) }), one));
            operations.Add("ln", (one, two) => Expression.Call(typeof(Math).GetMethod("Log10", new[] { typeof(double) }), one));
            operations.Add("tg", (one, two) => Expression.Call(typeof(Math).GetMethod("Tan", new[] { typeof(double) }), one));
            operations.Add("Cbrt", (one, two) => Expression.Call(typeof(Math).GetMethod("Cbrt", new[] { typeof(double) }), one)); // кубический корень
            operations.Add("Sqrt", (one, two) => Expression.Call(typeof(Math).GetMethod("Sqrt", new[] { typeof(double) }), one)); // квадратный корень
        }

        public static Func<double, double> GetFunc(string text)
        {
            text = text.Replace(" ", string.Empty);
            var f = Expression.Lambda(GetMathExp(text), x);
            return (Func<double, double>)f.Compile();
        }
        
        public static Expression GetMathExp(string text)
        {
            var textEntry = CheckBrackets(text);
            int brackets = 0; bool flag = true; int position = -1;
            for(int i = 0; i < textEntry.Length; i++)
            {
                var ch = textEntry[i];
                if (i == 0 && ch == '-') continue;
                if (ch == '(') brackets++;
                if (ch == ')') brackets--;
                if (brackets == 0)
                {
                    if (ch == '+' || ch == '-') {position = i; break;}
                    if (ch == '^' && flag) position = i;
                    if ((ch == '*' ||  ch == '/') && flag) {flag = false; position = i;}
                }
            }
            
            if (position > 0)
            {
                var textLeft = textEntry.Substring(0, position);
                var textRight = textEntry.Substring(position + 1);
                var funk = textEntry[position];  
                if (funk == '-') 
                { 
                    funk = '+'; 
                    if (Char.IsNumber(textEntry[position+1])) textRight = "-" + textRight;
                    else textRight = "-1*(" + textRight + ")";
                }
                return operations[funk.ToString()](GetMathExp(textLeft), GetMathExp(textRight));
            }
            return ParseExp(textEntry);
        }
        
        private static Expression ParseExp(string text)
        {
            if (text == "x") return x;
            string command = ""; string number = ""; int index = 0;
            foreach (var ch in text)
            {
                if (Char.IsNumber(ch) || ch == '.' || ch == '-') number += ch;
                if (Char.IsLetter(ch)) command += ch;
                if (ch == '(') break;
                index++;
            }
            if (command != "" && number != "") throw new ArgumentException($"Invalid expression: {text}");

            if (command != "")
                if (operations.ContainsKey(command))
                {
                    var arg = GetMathExp(text.Remove(0, index));
                    return operations[command](arg, Expression.Constant(1.0));
                } else throw new ArgumentException($"Command not found: {command}");
                
            if (number != "") return Expression.Constant(Double.Parse(number));
            else throw new ArgumentException($"Expression not recognized: {text}");
        }
        
        private static string CheckBrackets (string text)
        {
            int brackets = 0;
            bool outBrackets = false;
            if (text[0] == '(') outBrackets = true;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '(') brackets++;
                if (text[i] == ')') brackets--;
                if (brackets == 0 && i < text.Length - 1) outBrackets = false;
            }
            
            if (brackets != 0)
                throw new ArgumentException($"Check parentheses : {text}");
            if (outBrackets == true) return CheckBrackets(text.Substring(1, text.Length - 2));
            return text;
        }
    }
  
  
  static void Main() 
  {
    Console.WriteLine("Hello World");
    var text1 = "(((((-3.1)*x*50.5))))*x";
    var text2 = "-5-(sin(2*x^(-1/2)-5)+3*x)";
    Console.WriteLine(text1);
    Console.WriteLine(text2);

    var e1 = ParseFunc.GetMathExp(text1);
    var e2 = ParseFunc.GetMathExp(text2);
    
    var exp1 = Expression.Lambda(e1, ParseFunc.x);
    var exp2 = Expression.Lambda(e2, ParseFunc.x);
    
    Console.WriteLine("-----------------");
    Console.WriteLine(exp1.Body.ToString());
    Console.WriteLine(exp2.Body.ToString());
    
    
    var f1 = ParseFunc.GetFunc(text1);
    var f2 = ParseFunc.GetFunc(text2);
    
    double arg = 2;
    
    Console.WriteLine("-----------------");
    Console.WriteLine(f1(arg));
    Console.WriteLine(f2(arg));
  }
}

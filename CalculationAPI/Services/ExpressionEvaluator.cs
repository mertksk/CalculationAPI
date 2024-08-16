using System;
using System.Collections.Generic;
using System.Linq;

namespace CalculationAPI.Services
{
    /*This class is to calculate mathematichal operations*/ 
    public class ExpressionEvaluator
    {
        public double Evaluate(string expression)
        {
            var result = Splitter(expression);
            var rpn = ConvertToRPN(result);  
            return Evalutaion(rpn); 
        }

        /*Method simply to extract variables from the initial string */
        private List<string> Splitter(string expression)
        {
            var result = new List<string>();
            var currentNumber = "";

            for (int i = 0; i < expression.Length; i++)
            {
                char c = expression[i];

                if (char.IsDigit(c) || c == '.')
                {
                    currentNumber += c;  
                }
                else
                {
                    if (currentNumber != "")
                    {
                        result.Add(currentNumber);   
                        currentNumber = "";
                    }

                    if (c == '+' || c == '-' || c == '*' || c == '/' || c == '^' || c == '(' || c == ')')
                    {
                        result.Add(c.ToString()); 
                    }
                    else if (c == 'l' && expression.Substring(i, 3) == "log")
                    {
                        result.Add("log");
                        i += 2; 
                    }
                }
            }

            if (currentNumber != "")
            {
                result.Add(currentNumber); 
            }

            return result;
        }

        // Reverse Polish Notation (RPN) using Shunting Yard algorithm via GeeksForGeeks (Used this source ->https://www.geeksforgeeks.org/java-program-to-implement-shunting-yard-algorithm/ )

        private List<string> ConvertToRPN(List<string> result)
        {
            var outputQueue = new Queue<string>();
            var operatorStack = new Stack<string>();

            var precedence = new Dictionary<string, int>
            {
                { "+", 1 }, { "-", 1 },
                { "*", 2 }, { "/", 2 },
                { "^", 3 }, { "log", 4 }
            };

            var rightAssociative = new HashSet<string> { "^" };

            foreach (var i in result)
            {
                if (double.TryParse(i, out _))
                {
                    outputQueue.Enqueue(i); // Numbers go directly to the output
                }
                else if (i == "(")
                {
                    operatorStack.Push(i); // We stack left phranthsesis 
                }
                else if (i == ")")
                {
                    while (operatorStack.Peek() != "(")
                    {
                        outputQueue.Enqueue(operatorStack.Pop()); // It pops operators until reaching to the left phranthesis
                    }
                    operatorStack.Pop(); // Pops the left phranthesis
                }
                else
                {
                    while (operatorStack.Any() && precedence.ContainsKey(operatorStack.Peek()) &&
                           (precedence[operatorStack.Peek()] > precedence[i] ||
                           (precedence[operatorStack.Peek()] == precedence[i] && !rightAssociative.Contains(i))) &&
                           operatorStack.Peek() != "(")
                    {
                        outputQueue.Enqueue(operatorStack.Pop()); // It pops the operators with higher or equal  
                    }
                    operatorStack.Push(i); // It pushes the remaining 
                }
            }

            while (operatorStack.Any())
            {
                outputQueue.Enqueue(operatorStack.Pop()); // It pops the remaining operators
            }

            return outputQueue.ToList();
        }

        // Calculation of Mathematichal Expressions
        private double Evalutaion(List<string> result)
        {
            var stack = new Stack<double>();

            foreach (var i in result)
            {
                if (double.TryParse(i, out double num))
                {
                    stack.Push(num);
                }
                else if (i == "+")
                {
                    var b = stack.Pop();
                    var a = stack.Pop();
                    stack.Push(a + b);
                }
                else if (i == "-")
                {
                    var b = stack.Pop();
                    var a = stack.Pop();
                    stack.Push(a - b);
                }
                else if (i == "*")
                {
                    var b = stack.Pop();
                    var a = stack.Pop();
                    stack.Push(a * b);
                }
                else if (i == "/")
                {
                    var b = stack.Pop();
                    var a = stack.Pop();
                    stack.Push(a / b);
                }
                else if (i == "^")
                {
                    var b = stack.Pop();
                    var a = stack.Pop();
                    stack.Push(Math.Pow(a, b));
                }
                else if (i == "log")
                {
                    var baseNum = stack.Pop();
                    var value = stack.Pop();
                    stack.Push(Math.Log(value, baseNum));
                }
            }

            return stack.Pop();
        }
    }
}
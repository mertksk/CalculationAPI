using System;
using System.Collections.Generic;
using System.Linq;

namespace CalculationAPI.Services
{
    public class ExpressionEvaluator
    {
        // Method to evaluate an expression
        public double Evaluate(string expression)
        {
            var tokens = Tokenize(expression);
            var rpn = ConvertToRPN(tokens); // Convert to Reverse Polish Notation
            return EvaluateRPN(rpn); // Evaluate the RPN expression
        }

        // Tokenizer: split the expression into numbers, operators, and parentheses
        private List<string> Tokenize(string expression)
        {
            var tokens = new List<string>();
            var currentNumber = "";

            for (int i = 0; i < expression.Length; i++)
            {
                char c = expression[i];

                if (char.IsDigit(c) || c == '.')
                {
                    currentNumber += c; // Build the number token
                }
                else
                {
                    if (currentNumber != "")
                    {
                        tokens.Add(currentNumber); // Add the number to the token list
                        currentNumber = "";
                    }

                    if (c == '+' || c == '-' || c == '*' || c == '/' || c == '^' || c == '(' || c == ')')
                    {
                        tokens.Add(c.ToString()); // Add the operator or parenthesis
                    }
                    else if (c == 'l' && expression.Substring(i, 3) == "log")
                    {
                        tokens.Add("log");
                        i += 2; // Move the pointer to the end of "log"
                    }
                }
            }

            if (currentNumber != "")
            {
                tokens.Add(currentNumber); // Add last number if present
            }

            return tokens;
        }

        // Convert infix expression to Reverse Polish Notation (RPN) using Shunting Yard algorithm
        private List<string> ConvertToRPN(List<string> tokens)
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

            foreach (var token in tokens)
            {
                if (double.TryParse(token, out _))
                {
                    outputQueue.Enqueue(token); // Numbers go directly to the output
                }
                else if (token == "(")
                {
                    operatorStack.Push(token); // Left parentheses go on the stack
                }
                else if (token == ")")
                {
                    while (operatorStack.Peek() != "(")
                    {
                        outputQueue.Enqueue(operatorStack.Pop()); // Pop operators until left parenthesis
                    }
                    operatorStack.Pop(); // Pop the left parenthesis
                }
                else
                {
                    while (operatorStack.Any() && precedence.ContainsKey(operatorStack.Peek()) &&
                           (precedence[operatorStack.Peek()] > precedence[token] ||
                           (precedence[operatorStack.Peek()] == precedence[token] && !rightAssociative.Contains(token))) &&
                           operatorStack.Peek() != "(")
                    {
                        outputQueue.Enqueue(operatorStack.Pop()); // Pop operators with higher or equal precedence
                    }
                    operatorStack.Push(token); // Push the current operator
                }
            }

            while (operatorStack.Any())
            {
                outputQueue.Enqueue(operatorStack.Pop()); // Pop remaining operators
            }

            return outputQueue.ToList();
        }

        // Evaluate the RPN expression
        private double EvaluateRPN(List<string> tokens)
        {
            var stack = new Stack<double>();

            foreach (var token in tokens)
            {
                if (double.TryParse(token, out double num))
                {
                    stack.Push(num);
                }
                else if (token == "+")
                {
                    var b = stack.Pop();
                    var a = stack.Pop();
                    stack.Push(a + b);
                }
                else if (token == "-")
                {
                    var b = stack.Pop();
                    var a = stack.Pop();
                    stack.Push(a - b);
                }
                else if (token == "*")
                {
                    var b = stack.Pop();
                    var a = stack.Pop();
                    stack.Push(a * b);
                }
                else if (token == "/")
                {
                    var b = stack.Pop();
                    var a = stack.Pop();
                    stack.Push(a / b);
                }
                else if (token == "^")
                {
                    var b = stack.Pop();
                    var a = stack.Pop();
                    stack.Push(Math.Pow(a, b));
                }
                else if (token == "log")
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiteCAD.Common;
using LiteCAD.CSP;

namespace LiteCAD.PropEditors
{
    public static class RPNCalc
    {
        public static Token[] Tokenize(string text)
        {
            StringBuilder sb = new StringBuilder();
            char[] ops = new char[] { '+', '-', '/', '*' };
            List<Token> tokens = new List<Token>();
            for (int i = 0; i < text.Length; i++)
            {
                if (ops.Contains(text[i]))
                {
                    tokens.Add(new DoubleToken(Helpers.ParseDouble(sb.ToString())));
                    tokens.Add(new ArithmOperationToken(text[i] + ""));
                    sb.Clear();
                    continue;
                }
                sb.Append(text[i]);
            }
            if (sb.Length > 0)
            {
                tokens.Add(new DoubleToken(Helpers.ParseDouble(sb.ToString())));
            }
            return tokens.ToArray();
        }

        public static Token[] GetRPN(string s)
        {
            List<Token> ret = new List<Token>();
            Stack<Token> ss = new Stack<Token>();
            var tkns = Tokenize(s);
            foreach (var item in tkns)
            {
                if (item is DoubleToken)
                {
                    ret.Add(item);
                }
                if (item is ArithmOperationToken aa)
                {
                    if (ss.Count > 0)
                    {
                        var peek = ss.Peek() as ArithmOperationToken;
                        if (peek.Priority >= aa.Priority)
                        {
                            ret.Add(ss.Pop());
                            ss.Push(aa);
                        }
                    }
                    ss.Push(item);
                }
            }

            while (ss.Count > 0)
            {
                ret.Add(ss.Pop());
            }
            return ret.ToArray();
        }

        public static double Solve(string s)
        {
            //1.get rpn
            var rpn = GetRPN(s);
            return SolveRPN(rpn);
        }

        public static double SolveRPN(string s)
        {            
            var tkns = Tokenize(s);
            return SolveRPN(tkns);
        }

        public static double SolveRPN(Token[] tkns)
        {
            Stack<Token> ss = new Stack<Token>();
            foreach (var item in tkns)
            {
                if (item is DoubleToken)
                {
                    ss.Push(item);
                }
                if (item is ArithmOperationToken a)
                {
                    var op2 = ss.Pop() as DoubleToken;
                    var op1 = ss.Pop() as DoubleToken;
                    switch (a.Text)
                    {
                        case "+":
                            ss.Push(new DoubleToken(op1.Value + op2.Value));
                            break;
                        case "-":
                            ss.Push(new DoubleToken(op1.Value - op2.Value));
                            break;
                        case "*":
                        case "x":
                            ss.Push(new DoubleToken(op1.Value * op2.Value));
                            break;
                        case "/":
                            ss.Push(new DoubleToken(op1.Value / op2.Value));
                            break;
                    }
                }
            }
            return (ss.Pop() as DoubleToken).Value;
        }

        public class ArithmOperationToken : Token
        {
            public int Priority
            {
                get
                {
                    if (Text == "+" || Text == "-") return 1;
                    if (Text == "*" || Text == "/") return 2;
                    if (Text == "(" || Text == ")") return 3; 
                    return 0;
                }
            }

            public ArithmOperationToken(string v)
            {
                Text = v;
            }
        }
        public class DoubleToken : Token
        {
            public DoubleToken(double v)
            {
                Value = v;
            }
            public double Value;
        }
    }
}
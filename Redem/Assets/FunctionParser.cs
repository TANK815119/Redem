using Assets.Functions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
    internal static class FunctionParser
    {
        public static Function parse(string input)
        {
            UnityEngine.Debug.Log("Parsing " + input);
            input = input.Replace(" ", "");
            int parenthesis = 0;
            bool surrounded = input[0] == '(';
            for (int i  = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c == '(')
                {
                    parenthesis++;
                }
                if (c == ')')
                {
                    parenthesis--;
                }
                if(parenthesis == 0 && i != input.Length - 1)
                {
                    surrounded = false;
                    break;
                }
            }
            if (surrounded) 
            {
                return parse(input.Substring(1, input.Length - 2));
            }
            parenthesis = 0;
            for (int i = 0; i < input.Length; i++) 
            {
                char c = input[i];
                if(c == '('){
                    parenthesis++;
                }
                if(c == ')')
                {
                    parenthesis--;
                }
                if (parenthesis == 0)
                {
                    if (c == '+')
                    {
                        return new Add(parse(input.Substring(0, i)), parse(input.Substring(i + 1)));
                    }
                    else if (c == '-')
                    {
                        return new Add(parse(input.Substring(0, i)), new Multiply(parse(input.Substring(i + 1)), new Constant(-1)));
                    }
                }
            }
            parenthesis = 0;
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c == '(')
                {
                    parenthesis++;
                }
                if (c == ')')
                {
                    parenthesis--;
                }
                if (parenthesis == 0)
                {
                    if (c == '*')
                    {
                        return new Multiply(parse(input.Substring(0, i)), parse(input.Substring(i + 1)));
                    }
                    else if (c == '/')
                    {
                        return new Divide(parse(input.Substring(0, i)), parse(input.Substring(i + 1)));
                    }
                }
            }

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c == '(')
                {
                    parenthesis++;
                }
                if (c == ')')
                {
                    parenthesis--;
                }
                if (parenthesis == 0)
                {
                    if (c == '^')
                    {
                        return new Exponentiate(parse(input.Substring(0, i)), parse(input.Substring(i + 1)));
                    }
                }
            }

            if (input[0] == '#')
            {
                return new Multiply(parse(input.Substring(1)), new Constant(-1));
            }

            if (input == "x")
            {
                return new XFunction();
            }
            else if (input == "y")
            {
                return new YFunction();
            }

            return new Constant(Double.Parse(input));
        }
    }
}

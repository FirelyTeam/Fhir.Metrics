/*
* Copyright (c) 2014, Furore (info@furore.com) and contributors
* See the file CONTRIBUTORS for details.
*
* This file is licensed under the BSD 3-Clause license
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Fhir.Metrics
{
    public struct Unary
    {
        public int Exponent;
        public string Expression;
        
        public Unary(int exponent, string expression)
        {
            this.Exponent = exponent;
            this.Expression = expression;
        }
        
        public override string ToString()
        {
            return Expression + "^" + Exponent.ToString();
        }
        
        public Exponential Factor()
        {
            return Exponential.Power(Exponential.Exact(Expression), Exponent);
        }
        
        public Exponential Numeric()
        {
            return Exponential.Exact(Expression);
        }
        
        public bool IsNumeric
        {
            get
            {
                double dummy;
                return double.TryParse(this.Expression, out dummy);
            }
        }
    }

    public static class Parser
    {

        public static Regex TokenPattern = new Regex(@"(((?<m>[\.\/])?(?<m>[^\.\/]+))*)?", RegexOptions.Compiled);
        public static Regex Annotations = new Regex(@"{[^{}]*}", RegexOptions.Compiled);

        public static List<string> Tokenize(string expression)
        {
            if(Annotations.IsMatch(expression))
                expression = CanonicalizeAnnotations(expression);

            if (!TokenPattern.IsMatch(expression) || Annotations.IsMatch(expression))
                throw new ArgumentException("Invalid metric expression");

            return TokenPattern.Match(expression).Captures("m").ToList();
        }

        private static string CanonicalizeAnnotations(string expression)
        {
            var annotations = new Regex(@"{[^{}]*}", RegexOptions.Compiled);
            foreach(Match match in annotations.Matches(expression))
            {
                if (match.Index == 0) // Expressions contains just an annotation, e.g. "{rbc}"
                {
                    expression = Metrics.Unity.Symbol;
                }
                else if (expression[match.Index - 1].Equals('/') || expression[match.Index - 1].Equals('.')) // Annotation is part of a multiplication or division, e.g. "/{count}" or "10*3.{RBC}"
                {
                    expression = expression.Remove(match.Index, match.Length);
                    expression = expression.Insert(match.Index, Metrics.Unity.Symbol);
                }
                else // e.g. // Annotation is directly combined with another unit, e.g. "ml{total}"
                {
                    expression = expression.Remove(match.Index, match.Length);
                }
            }

            return expression;
        }

        static bool IsOperator(string token)
        {
            return (token == ".") || (token == "*") || (token == "/");
        }

        static int OperatorExponent(string token)
        {
            switch(token)
            {
                case ".":
                case "*": return 1;
                case "/": return -1;
                default: return 0;
            }
        }

        private static Unary parseUnaryExponent(string expression)
        {
            int exponent = 1;
            Match match = Regex.Match(expression, @"[\+\-]?\d+$");
            string exp = match.Value;
            
            if (!string.IsNullOrEmpty(exp) && exp.Length < expression.Length)
            {
                exponent = Convert.ToInt16(match.Value);

                expression = expression.Remove(expression.Length - exp.Length, exp.Length);
            }
            return new Unary(exponent, expression);
        }
       
        static IEnumerable<Unary> parseTokens(IEnumerable<string> tokens)
        {

            int exponent = 1;
            foreach(string token in tokens)
            {
                if (IsOperator(token))
                {
                    exponent = OperatorExponent(token);
                }
                else
                {
                    Unary u = parseUnaryExponent(token);
                    u.Exponent *= exponent;
                    yield return u;
                }
            }
        }

        public static IEnumerable<Unary> ToUnaryTokens(string expression)
        {
            return parseTokens(Tokenize(expression));
        }
        
        public static IEnumerable<Unary> Numerics(this IEnumerable<Unary> tokens)
        {
            return tokens.Where(u => u.IsNumeric);
        }

        public static IEnumerable<Unary> NonNumerics(this IEnumerable<Unary> tokens)
        {
            return tokens.Where(u => !u.IsNumeric);
        }

    }
}

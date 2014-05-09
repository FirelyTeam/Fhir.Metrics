using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fhir.UnitsSystem   
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
    }

    public class Parser
    {
        public static IEnumerable<string> Tokenize(string s, string pattern)
        {
            MatchCollection matches = Regex.Matches(s, pattern);

            foreach (Group g in matches)
            {
                yield return g.Value;
            }
        }

        public static List<string> Tokenize(string expression)
        {
            string pattern = @"([^\.\/]+|[\.\/])";
            return Parser.Tokenize(expression, pattern).ToList();
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
                    yield return new Unary(exponent, token);
                }
            }
        }

        public static IEnumerable<Unary> ToUnaryTokens(string expression)
        {
            return parseTokens(Tokenize(expression));
        }

    }
}

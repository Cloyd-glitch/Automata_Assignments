using System;
using System.Text;

namespace CCommentMealy
{
    /// <summary>States of the Mealy machine (same as the DFA, S4 is the "just closed" state).</summary>
    internal enum State
    {
        S0,   // start, nothing read yet
        S1,   // just read the opening '/'
        S2,   // inside the comment body, last char was NOT a pending '*'
        S3,   // inside the comment body, last char WAS a '*' (pending close)
        S4,   // just closed the comment by reading "*/"
        Dead  // trap state, input can never be accepted from here
    }

    internal static class CommentMealy
    {
        // Collapse any character that is not '*' or '/' onto the placeholder symbol 'a'.
        private static char ToSymbol(char c) => (c == '*' || c == '/') ? c : 'a';

        // Combined transition + output function:
        //   (state, symbol) -> (next state, output)
        // delta : Q x Sigma -> Q          lambda : Q x Sigma -> {'0','1'}
        // Output is '1' only on the transition that completes "*/".
        private static (State Next, char Output) Step(State current, char symbol)
        {
            switch (current)
            {
                case State.S0:
                    return symbol == '/' ? (State.S1, '0') : (State.Dead, '0');

                case State.S1:
                    return symbol == '*' ? (State.S2, '0') : (State.Dead, '0');

                case State.S2:
                    return symbol == '*' ? (State.S3, '0') : (State.S2, '0');

                case State.S3:
                    if (symbol == '/') return (State.S4, '1'); // "*/" closes the comment
                    if (symbol == '*') return (State.S3, '0'); // "**" stays pending
                    return (State.S2, '0');                    // star not followed by '/'

                case State.S4:        // nothing may follow a completed comment
                case State.Dead:
                default:
                    return (State.Dead, '0');
            }
        }

        /// <summary>Runs the machine and returns one output symbol per input symbol.</summary>
        public static string Run(string input)
        {
            State state = State.S0;
            var output = new StringBuilder();
            foreach (char c in input)
            {
                (state, char o) = Step(state, ToSymbol(c));
                output.Append(o);
            }
            return output.ToString();
        }

        /// <summary>The string is a C-style comment iff the LAST output symbol is '1'.</summary>
        public static bool IsValidComment(string input)
        {
            string output = Run(input);
            return output.Length > 0 && output[^1] == '1';
        }
    }

    internal static class Program
    {
        private static void Main()
        {
            Console.WriteLine("Enter your own strings to test (blank line to quit):");
            string? line;
            while (!string.IsNullOrEmpty(line = Console.ReadLine()))
            {
                string output = CommentMealy.Run(line);
                bool ok = CommentMealy.IsValidComment(line);
                Console.WriteLine($"{line,-16} output {output,-16} -> {(ok ? "ACCEPTED" : "REJECTED")}");
            }
        }
    }
}

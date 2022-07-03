using System;
using System.Collections.Generic;

namespace Endciv
{
    public class CommandParser
    {
        public struct Argument
        {
            public string Value;

            // start position in CommandText
            public int Begin;

            // end position in CommandText
            public int End;

            // argument index
            public int Index;

            public Argument(string value, int begin, int end, int index)
            {
                Value = value;
                Begin = begin;
                End = end;
                Index = index;
            }
        }

        public string CommandLine { get; private set; }

        public Argument Command { get; private set; }

        public List<Argument> Arguments { get; private set; }

        public bool CommandDefined { get { return CommandLine != Command.Value; } }

        public CommandParser()
        {
            Arguments = new List<Argument>();
            Reset();
        }

        public void ParseLine(string input)
        {
            CommandLine = input;
            input = input.Trim();
            if (string.IsNullOrEmpty(input))
            {
                Reset();
                return;
            }
            Arguments.Clear();

            int offset = CommandLine.IndexOf(input);
            if (offset < 0) offset = 0;
            CommandLine = input;

            // find command name
            var idx = input.IndexOf(' ');
            if (idx != -1)
            {
                Command = new Argument(input.Substring(0, idx), offset, offset + idx - 1, -1);

                // find arguments
                for (int i = idx + 1, argIdx = 0; i < input.Length; i++, argIdx++)
                {
                    var begin = i;
                    var arg = ParseArgument(input, ref i);
                    Arguments.Add(new Argument(arg, offset + begin, offset + i, argIdx));
                }
            }
            else
            {
                Command = new Argument(input, offset, offset + input.Length - 1, -1); ;
            }
        }

        public bool ConvertArguments(IList<IArgument> targetArgument, out object[] result)
        {
            if (targetArgument != null)
            {
                if (targetArgument.Count == 0)
                {
                    result = null;
                    return true;
                }
                if (targetArgument.Count == Arguments.Count)
                {
                    result = new object[Arguments.Count];
                    for (int i = 0; i < result.Length; i++)
                    {
                        if (!targetArgument[i].TryParse(Arguments[i].Value, out result[i]))
                        {
                            throw new FormatException(string.Format("Convert exception '{0}' to {1}", Arguments[i].Value, targetArgument[i].ValueType));
                        }
                    }
                    return true;
                }
            }
            result = null;
            return false;
        }

        public void Reset()
        {
            CommandLine = string.Empty;
            Command = new Argument(string.Empty, 0, 0, -1);
            Arguments.Clear();
        }

        private static string ParseArgument(string input, ref int offset)
        {
            // find begin text
            for (; offset < input.Length; offset++)
            {
                if (input[offset] != ' ')
                {
                    break;
                }
            }

            if (offset >= input.Length)
            {
                // argument not found!
                return null;
            }

            char c = input[offset];
            int begin = offset;
            int end;

            if (c == '"' || c == '\'')
            {
                // argument begins with " or '
                // search now the end
                begin++;
                end = input.IndexOf(c, begin);
            }
            else
            {
                // normal argument find next separator
                end = input.IndexOf(' ', begin);
            }

            if (end != -1)
            {
                // end found substring
                offset = end;
                return input.Substring(begin, end - begin);
            }

            // last argument ?
            offset = input.Length - 1;
            return input.Substring(begin, input.Length - begin);
        }
    }
}
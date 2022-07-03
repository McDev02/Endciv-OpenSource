using System;
using System.Collections.Generic;
using System.Text;

namespace Endciv
{
    public class CommandHints
    {
        public struct Hint
        {
            public string RawText;
            public string Text;
            public ICommand Command;

            public Hint(string rawText, ICommand command)
            {
                RawText = Text = rawText;
                Command = command;
            }
        }

        private List<Hint> m_Hints = new List<Hint>();
        private StringBuilder m_TextBuilder = new StringBuilder();        
        public int SelectIndex
        {
            get; private set;            
        }

        public List<Hint> Hints { get { return m_Hints; } }

        public ICommand SelectedCommand { get; private set; }

        public int ArgumentIndex { get; private set; }

        public void Generate(string commandName, IList<ICommand> commands)
        {
            ResetSelection();
            m_Hints.Clear();

            if (string.IsNullOrEmpty(commandName)) return;

            foreach (var cmd in commands)
            {
                if (cmd.Name.StartsWith(commandName, StringComparison.InvariantCultureIgnoreCase))
                {
                    var textCmd = CreateText(cmd, -1);
                    m_Hints.Add(new Hint(textCmd, cmd));

                    if (string.Equals(cmd.Name, commandName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        SelectedCommand = cmd;
                        SelectIndex = m_Hints.Count - 1;
                    }
                }
            }
        }

        public void SelectArgument(int index)
        {
            ArgumentIndex = index;
            for (int i = 0; i < m_Hints.Count; i++)
            {
                var hint = m_Hints[i];
                hint.RawText = hint.Text = CreateText(hint.Command, index);
                m_Hints[i] = hint;
            }
        }

        public void SelectedPrevious()
        {
            if (m_Hints.Count == 0) return;
            DeselectCmd();
            SelectIndex--;
            if (SelectIndex < 0 || SelectIndex >= m_Hints.Count)
                SelectIndex = m_Hints.Count - 1;

            SelectedCommand = m_Hints[SelectIndex].Command;
            SelectCmd();
        }

        public void SelectedNext()
        {
            if (m_Hints.Count == 0) return;
            DeselectCmd();
            SelectIndex++;
            if (SelectIndex < 0 || SelectIndex >= m_Hints.Count)
                SelectIndex = 0;

            SelectedCommand = m_Hints[SelectIndex].Command;
            SelectCmd();
        }

        public void ResetSelection()
        {
            SelectedCommand = null;
            SelectIndex = -1;
            ArgumentIndex = -1;
        }

        private void SelectCmd()
        {
            if (SelectIndex < 0 || SelectIndex >= m_Hints.Count) return;
            var cmd = m_Hints[SelectIndex];
            m_TextBuilder.Length = 0;
            cmd.Text = m_TextBuilder.AppendColor(Util.HTMLColor.cyan, cmd.RawText).ToString();
            m_Hints[SelectIndex] = cmd;
        }

        private void DeselectCmd()
        {
            if (SelectIndex < 0 || SelectIndex >= m_Hints.Count) return;
            var cmd = m_Hints[SelectIndex];
            cmd.Text = cmd.RawText;
            m_Hints[SelectIndex] = cmd;
        }

        // generate: CMD_NAME [ float NAME, int NAME, TYPE NAME ]
        private string CreateText(ICommand cmd, int selectArgIdx)
        {
            m_TextBuilder.Length = 0;
            m_TextBuilder.Append(cmd.Name).Append(' ');
            if (cmd.Arguments != null && cmd.Arguments.Count > 0)
            {
                m_TextBuilder.AppendColor(Util.HTMLColor.grey, "[");
                for (int i = 0; i < cmd.Arguments.Count; i++)
                {
                    var arg = cmd.Arguments[i];
                    if (selectArgIdx == i)
                    {
                        m_TextBuilder
                            .AppendBeginColor(Util.HTMLColor.aqua)
                            .Append(' ')
                            .AppendItalic(arg.ValueType.Name)
                            .Append(' ')
                            .AppendBold(arg.Name)
                            .AppendEndColor()
                            .Append(',');
                    }
                    else
                    {
                        m_TextBuilder
                            .Append(' ')
                            .AppendItalic(arg.ValueType.Name)
                            .Append(' ')
                            .AppendBold(arg.Name)
                            .Append(',');
                    }
                }
                m_TextBuilder.Length -= 1;
                m_TextBuilder.Append(' ').AppendColor(Util.HTMLColor.grey, "]");
            }

            return m_TextBuilder.ToString();
        }
    }
}
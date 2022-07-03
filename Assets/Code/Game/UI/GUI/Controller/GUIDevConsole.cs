using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace Endciv
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CanvasGroup))]
    [DisallowMultipleComponent]
    public partial class GUIDevConsole : GUIAnimatedPanel
    {
        private const int MAX_BUFFER_SIZE = 1024 * 10;

        public Text log;
        public Text suggestionText;
        public InputField inputField;
        public ScrollRect scrollRect;
        public Dropdown hintDropdown;

        private string m_LogText = string.Empty;
        private List<ICommand> m_Commands = new List<ICommand>();        
        private CommandParser m_Parser = new CommandParser();
        private CommandHistory m_History = new CommandHistory();
        private CommandHints m_HintGenerator = new CommandHints();

        private bool m_AddLogStacktrace;

        private System.Text.StringBuilder m_TextBuilder = new System.Text.StringBuilder(MAX_BUFFER_SIZE);
        private bool m_TextBuilderDirty;

        [SerializeField]
        private Color32 m_ForegroundColor = Color.white;

        private bool enforceFocus;
        public bool IsFocused { get { return gameObject.activeInHierarchy && (inputField.isFocused || enforceFocus); } }        

        public Color32 ForegroundColor
        {
            get { return m_ForegroundColor; }
            set { m_ForegroundColor = value; }
        }

        private void Start()
        {
            ForegroundColor = m_ForegroundColor;            
            RegisterCommands();            
        }

        private void Update()
        {
            if (m_TextBuilderDirty)
            {
                m_TextBuilderDirty = false;

                int index = -1;
                for (int i = m_TextBuilder.Length - 1; i >= 0; i++)
                {
                    char c = m_TextBuilder[i];
                    if (c == '\n' || c != ' ')
                    {
                        index = i;
                        break;
                    }
                }
                if (index != -1)
                {
                    m_LogText = m_TextBuilder.ToString(0, index);
                }
                else
                {
                    m_LogText = m_TextBuilder.ToString();
                }
                log.text = m_LogText;
                scrollRect.verticalNormalizedPosition = 0f;
            }
            if (Input.anyKeyDown)
            {
                OnKeyDown();
            }           
        }
        
        public void Register(ICommand command)
        {
            RegisterCommand(command);
        }

        public void RegisterCommand(string commandName, string commandDesc, Action<object[]> callback, params IArgument[] arguments)
        {
            RegisterCommand(new DefaultCommand(commandName, commandDesc, callback, arguments));
        }

        public void RegisterCommand(string commandName, Action<object[]> callback, params IArgument[] arguments)
        {
            RegisterCommand(new DefaultCommand(commandName, callback, arguments));
        }

        public void RegisterCommand(string commandName, Action<object[]> callback)
        {
            RegisterCommand(new DefaultCommand(commandName, callback));
        }

        public void RegisterCommand(ICommand command)
        {
            if (command == null)
                throw new ArgumentNullException("command");


            if (command.Name.Contains(" "))
                throw new ArgumentException(string.Format("Invalid Command name '{0}'", command.Name), "command");



            var comparer = StringComparer.InvariantCultureIgnoreCase;
            for (int i = 0, len = m_Commands.Count; i < len; i++)
            {
                var cmd = m_Commands[i];
                if (cmd == command || comparer.Equals(cmd.Name, command.Name))
                {
                    Debug.LogWarning(string.Format("Replace Command '{0}'", cmd.Name));
                    m_Commands.RemoveAt(i);
                    break;
                }
            }
            m_Commands.Add(command);
            m_Commands.Sort((a, b) => a.Name.CompareTo(b.Name));
        }

        public void ApplySuggestion(string suggestion)
        {
            inputField.text = suggestion;
            suggestionText.text = string.Empty;
        }

        public void OnSuggestionSelected(int selection)
        {          
            if (selection < 0 || selection >= m_HintGenerator.Hints.Count)
                return;
            ApplySuggestion(m_HintGenerator.Hints[selection].Command.Name);            
        }

        private void OnKeyDown()
        {
            if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            {
                if(suggestionText.text != string.Empty && hintDropdown.isActiveAndEnabled)
                {
                    ApplySuggestion(suggestionText.text);
                    hintDropdown.Hide();
                    OnInputChanged();
                }
                else
                    OnExecuteCommand();
            }
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    //
                    // History navigation
                    //
                    string text;
                    if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        text = m_History.HistoryPrevious();
                    }
                    else
                    {
                        text = m_History.HistoryNext();
                    }
                    if (text != null)
                    {                        
                        inputField.text = text;
                        inputField.caretPosition = inputField.text.Length;
                        OnInputChanged();
                    }
                }
                else
                {
                    //
                    // command hint navigation
                    //
                    if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        m_HintGenerator.SelectedPrevious();
                    }
                    else
                    {
                        m_HintGenerator.SelectedNext();
                    }
                    if (m_HintGenerator.SelectedCommand != null)
                    {
                        suggestionText.text = m_HintGenerator.SelectedCommand.Name;
                        //inputField.text = m_HintGenerator.SelectedCommand.Name;
                        inputField.caretPosition = inputField.text.Length;                        
                        //hintDropdown.value = m_HintGenerator.SelectIndex;
                        //hintDropdown.RefreshShownValue();
                    }

                }
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                int index = -1;
                foreach (var arg in m_Parser.Arguments)
                {
                    if (inputField.caretPosition >= arg.Begin && inputField.caretPosition <= arg.End + 1)
                    {
                        index = arg.Index;
                        break;
                    }
                }
                m_HintGenerator.SelectArgument(index);
            }

        }

        public void OnInputChanged()
        {
            m_Parser.ParseLine(inputField.text);
            m_HintGenerator.Generate(m_Parser.Command.Value, m_Commands);
            
            int index = -1;
            foreach (var arg in m_Parser.Arguments)
            {
                if (inputField.caretPosition >= arg.Begin && inputField.caretPosition <= arg.End + 1)
                {
                    index = arg.Index;
                    break;
                }
            }
            m_HintGenerator.SelectArgument(index);

            if (m_HintGenerator.SelectedCommand != null)
            {
                string text = null;
                var cmd = m_HintGenerator.SelectedCommand;
                if (cmd.Arguments != null
                    && m_HintGenerator.ArgumentIndex >= 0
                    && m_HintGenerator.ArgumentIndex < cmd.Arguments.Count)
                {
                    text = cmd.Arguments[m_HintGenerator.ArgumentIndex].Description;
                }
                else
                {
                    text = cmd.Description;
                }
            }
            if (m_HintGenerator.Hints.Count > 0)
            {
                hintDropdown.options.Clear();
                foreach (var hint in m_HintGenerator.Hints)
                {
                    hintDropdown.options.Add(new Dropdown.OptionData(hint.Text));
                }
                hintDropdown.Show();
                //hintDropdown.value = m_HintGenerator.SelectIndex;
                //hintDropdown.RefreshShownValue();
            }
            else
            {
                hintDropdown.options.Clear();
                hintDropdown.Hide();
                suggestionText.text = string.Empty;                
            }
            FocusInputField();
        }     
        
        private void FocusInputField()
        {
            var color = inputField.selectionColor;
            color.a = 0f;
            inputField.selectionColor = color;
            inputField.Select();
            StartCoroutine(DeselectText());
            enforceFocus = true;
        }

        private IEnumerator DeselectText()
        {
            yield return new WaitForEndOfFrame();
            inputField.MoveTextEnd(false);
            var color = inputField.selectionColor;
            color.a = 1f;
            inputField.selectionColor = color;
            enforceFocus = false;
        }

        public void OnExecuteCommand()
        {
            m_Parser.ParseLine(inputField.text);
            if (string.IsNullOrEmpty(m_Parser.Command.Value))
                return;

            ForegroundColor = Color.cyan;
            WriteLineIntoBuffer("> " + m_Parser.CommandLine);

            ICommand command = GetCommand(m_Parser.Command.Value);
            if (command != null)
            {
                try
                {
                    object[] parsedArgs = null;
                    if ((command.ExecuteEmptyArguments && m_Parser.Arguments.Count == 0)
                        || m_Parser.ConvertArguments(command.Arguments, out parsedArgs))
                    {
                        command.Execute(parsedArgs);
                    }
                    else
                    {
                        ForegroundColor = Color.red;
                        WriteLineIntoBuffer("invalid arguments");
                    }
                }
                catch (Exception ex)
                {
                    ForegroundColor = Color.red;
                    WriteLineIntoBuffer(ex.Message);
                }
            }
            else
            {
                ForegroundColor = Color.red;
                WriteLineIntoBuffer("unknown command");
            }

            ResetColor();

            m_History.AddHistory(m_Parser.CommandLine);
            inputField.text = string.Empty;
            m_Parser.Reset();
            OnInputChanged();
        }

        private ICommand GetCommand(string name)
        {
            foreach (var cmd in m_Commands)
            {
                if (string.Equals(cmd.Name, name, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    return cmd;
                }
            }
            return null;
        }

        private void WriteLineIntoBuffer(string text)
        {
            WriteIntoBuffer(text);
            m_TextBuilder.Append('\n');
            if (m_TextBuilder.Length > MAX_BUFFER_SIZE)
            {
                CleanupTextBuffer(m_TextBuilder.Length - MAX_BUFFER_SIZE);
            }
            m_TextBuilderDirty = true;
        }

        private void WriteIntoBuffer(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                m_TextBuilder.AppendBeginColor(m_ForegroundColor);
                int start = m_TextBuilder.Length;
                m_TextBuilder.Append(text);
                m_TextBuilder.Replace("\n", "</color>\n<color=#" + m_ForegroundColor.RawValue().ToString("x") + ">", start, text.Length);
                m_TextBuilder.AppendEndColor();
            }
            else
            {
                m_TextBuilder.Append(text);
            }
            if (m_TextBuilder.Length > MAX_BUFFER_SIZE)
            {
                CleanupTextBuffer(m_TextBuilder.Length - MAX_BUFFER_SIZE);
            }
            m_TextBuilderDirty = true;
            log.text = m_TextBuilder.ToString();            
        }

        private void CleanupTextBuffer(int minRemove)
        {
            for (int i = 0; i < m_TextBuilder.Length; i++)
            {
                if (m_TextBuilder[i] == '\n')
                {
                    if (i <= minRemove) continue;
                    m_TextBuilder.Remove(0, i + 1);
                    break;
                }
            }
        }

        public void ResetColor()
        {
            m_ForegroundColor = Color.white;
        }
    }
}
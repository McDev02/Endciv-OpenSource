using System.Collections.Generic;
using UnityEngine;

namespace Endciv
{
    public class CommandHistory
    {
        public const int MAX = 20;
        private List<string> m_History = new List<string>(MAX);
        private int m_HistoryIndex;

        public void AddHistory(string command)
        {
            if (m_History.Count > MAX)
            {
                m_History.RemoveAt(0);
            }
            m_History.Add(command);
            RestHistoryIndex();
        }

        public void RestHistoryIndex()
        {
            m_HistoryIndex = m_History.Count;
        }

        public string HistoryPrevious()
        {
            if (m_History.Count == 0) return null;
            m_HistoryIndex--;
            m_HistoryIndex = Mathf.Clamp(m_HistoryIndex, 0, m_History.Count - 1);
            return m_History[m_HistoryIndex];
        }

        public string HistoryNext()
        {
            if (m_History.Count == 0) return null;
            m_HistoryIndex++;
            m_HistoryIndex = Mathf.Clamp(m_HistoryIndex, 0, m_History.Count - 1);
            return m_History[m_HistoryIndex];
        }
    }
}
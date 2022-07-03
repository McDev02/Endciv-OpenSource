using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Endciv.Editor
{
	public class LocaEditorWindow : EditorWindow
	{
		class LocaPath : IComparable<LocaPath>
		{
			public LocaPath Parent;
			public List<LocaPath> Children = new List<LocaPath>();

			public bool Foldout = false;
			public string Key;
			public string Text;
            private string m_Path;

			public LocaPath(string key)
			{
				Key = key;
                using (var obj = GlobalObjectPool<System.Text.StringBuilder>.Get())
                    {
                        var sb = obj.Object;
                        BuildFullName(this, sb);
                        var name = sb.Length > 1 ? sb.ToString(0, sb.Length - 1) : null;
                        sb.Length = 0;
                        m_Path = name;
                        //Debug.Log(m_Path);
                    }
			}

            public string Path
            {
                get
                {                    
                    if (m_Path == null)
                    {
                        using (var obj = GlobalObjectPool<System.Text.StringBuilder>.Get())
                        {
                            var sb = obj.Object;
                            BuildFullName(this, sb);
                            var name = sb.Length > 1 ? sb.ToString(0, sb.Length - 1) : null;
                            sb.Length = 0;
                            m_Path = name;
                            return name;
                        }
                    } 
                    else 
                        return m_Path;
                }
                set
                {
                    m_Path = value;
                }

            }

			static void BuildFullName(LocaPath current, System.Text.StringBuilder sb)
			{
				if (current.Parent != null)
				{
					BuildFullName(current.Parent, sb);
					sb.Append(current.Key);
					sb.Append('/');
				}
			}

			public void Clear()
			{
				foreach (var child in Children)
				{
					child.Clear();
				}
				Children.Clear();
				Parent = null;
			}

			public LocaPath GetChild(string path)
			{
				if (string.IsNullOrEmpty(path))
				{
					return null;
				}
				var paths = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
				var current = this;
				for (int i = 0; i < paths.Length; i++)
				{
					var child = current.FindChild(paths[i]);
					if (child == null)
					{
						//Debug.Log("Create node " + paths[i]);
						child = new LocaPath(paths[i]);
						child.Parent = current;
						current.Children.Add(child);
						current.Children.Sort();
					}
					current = child;
				}
				return current;
			}

			private LocaPath FindChild(string name)
			{
				var comp = StringComparer.InvariantCultureIgnoreCase;
				for (int i = 0; i < Children.Count; i++)
				{
					if (comp.Equals(Children[i].Key, name))
					{
						return Children[i];
					}
				}
				return null;
			}

			public int CompareTo(LocaPath other)
			{
				return other.Key.CompareTo(Key);
			}
		}

		const string TOOLTIP =
	@"<b>...</b>
<i>...</i>
<size=16>...</size>
<color=#RRGGBB>...</color>
<color=#RRGGBBAA>...</color>
<color=cyan>...</color>
black	#00 00 00 FF
blue	#00 00 FF FF
brown	#A5 2A 2A FF
cyan 	#00 FF FF FF
darkblue	#00 00 A0 FF
green	#00 80 00 FF
grey	#80 80 80 FF
lightblue	#AD D8 E6 FF
lime	#00 FF 00 FF
magenta	#FF 00 FF FF
maroon	#80 00 00 FF
navy	#00 00 80 FF
olive	#80 80 00 FF
orange	#FF A5 00 FF
purple	#80 00 80 FF
red	#FF 00 00 FF
silver	#C0 C0 C0 FF
teal	#00 80 80 FF
white	#FF FF FF FF
yellow	#FF FF 00 FF
";

		[NonSerialized]
		private static GUIContent m_PreviewTitle = new GUIContent("RichText Preview (?)", TOOLTIP);

		private LocaPath m_Tree = new LocaPath("Loca");
		private DateTime m_LastWriteTime;
		private Vector2 m_ScrollView;
		private LocaPath m_Selected;
		[NonSerialized]
		private GUIStyle m_PreviewStyle;

		private Color m_ColorPicker = Color.clear;
		private string m_ColorHex = "#000000";
		private bool m_ShowControlChar = true;

		[NonSerialized]
		private int m_LocaFileSelection;
		[NonSerialized]
		private GUIContent[] locaFiles;

		[NonSerialized]
		private string m_LastText;
		[NonSerialized]
		private string m_PreviewText;
		[NonSerialized]
		private bool hasDataChanged;

		private string m_CreateLocaKey;
        private string m_SearchInput;

		[MenuItem(EditorHelper.EditorToolsPath + "Localization/Open Loca Editor Old", false, 4)]
		public static void Open()
		{
			GetWindow<LocaEditorWindow>(false, "Loca Editor", true).Show();
		}

		private void OnEnable()
		{
			Reload();
			Load();
		}

		private void OnDisable()
		{
			CheckSaveWarningDisplay();
		}
	
		private void OnGUI()
		{
			//if (m_PreviewStyle == null)
			//{
			//	m_PreviewStyle = new GUIStyle(GUI.skin.box);
			//	m_PreviewStyle.alignment = TextAnchor.UpperLeft;
			//	m_PreviewStyle.richText = true;
			//	m_PreviewStyle.normal.textColor = GUI.skin.label.normal.textColor;
			//	m_PreviewStyle.stretchWidth = true;
			//	m_PreviewStyle.stretchHeight = true;
			//}

			#region toolbar
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

			if (GUILayout.Button("Reload Files", EditorStyles.toolbarButton))
			{
				Reload();
				Load();
			}

			var index = EditorGUILayout.Popup(m_LocaFileSelection, locaFiles, EditorStyles.toolbarPopup);
			if (m_LocaFileSelection != index)
			{
				CheckSaveWarningDisplay();
				m_LocaFileSelection = index;
				Load();
			}

			if (GUILayout.Button("Save", EditorStyles.toolbarButton))
			{
				Save();
			}

			GUILayout.FlexibleSpace();

			EditorGUILayout.EndHorizontal();
			#endregion


			GUILayout.Space(10);

			EditorGUILayout.BeginHorizontal();

			#region Tree view
			EditorGUILayout.BeginVertical(GUILayout.MaxWidth(250));

			EditorGUILayout.BeginHorizontal();
			EditorGUIUtility.labelWidth = 60;
			m_CreateLocaKey = EditorGUILayout.TextField("LocaKey", m_CreateLocaKey);
			if (GUILayout.Button("Create", EditorStyles.miniButton, GUILayout.ExpandWidth(false)) && m_CreateLocaKey != null)
			{
				Create();
			}
			EditorGUIUtility.labelWidth = 0;
			EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 60;
            m_SearchInput = EditorGUILayout.TextField("Search", m_SearchInput);
            EditorGUIUtility.labelWidth = 0;
            EditorGUILayout.EndHorizontal();

            m_ScrollView = EditorGUILayout.BeginScrollView(m_ScrollView);
			EditorGUIUtility.SetIconSize(new Vector2(16, 16));
            if(string.IsNullOrEmpty(m_SearchInput))
            {
                DrawTree(m_Tree);
            }
			else
            {
                DrawFilteredItems(m_Tree, m_SearchInput);
            }
			EditorGUIUtility.SetIconSize(new Vector2());
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndScrollView();

			EditorGUILayout.EndVertical();
			#endregion

			#region Node part
			EditorGUILayout.BeginVertical();

			if (m_Selected != null && m_Selected != m_Tree)
			{
				string value;
                string pathValue;
                EditorGUI.BeginChangeCheck();
                pathValue = WithoutSelectAll(() => EditorGUILayout.DelayedTextField("Path", m_Selected.Path));
                if(EditorGUI.EndChangeCheck())
                {
                    m_Selected.Path = pathValue;
                    var data = new LocalizationManager.LocaDictionary();
                    CollectLocaEntries(m_Tree, data);
                    var sort = new SortedDictionary<string, string>(data, StringComparer.InvariantCultureIgnoreCase);
                    m_Selected = BuildTree(sort, pathValue);
                    ShowSelected();
					hasDataChanged = true;
				}
				//value = EditorGUILayout.TextField("Key", m_Selected.Key, (GUILayoutOption[])null);
				//if (m_Selected.Key != value)
				//{
				//	m_Selected.Key = value;
				//	m_Changed = true;
				//}
                if (m_Selected != null && m_Selected.Path != pathValue)
                {
                    m_Selected.Path = pathValue;
                }
				if (m_Selected != null && m_Selected.Children.Count == 0)
				{
					EditorGUILayout.LabelField("Text");
                    EditorGUI.BeginChangeCheck();
					value = WithoutSelectAll(() => EditorGUILayout.TextArea(m_Selected.Text, GUILayout.MinHeight(60)));
                    if(EditorGUI.EndChangeCheck())
                    {
                        m_Selected.Text = value;
                        hasDataChanged = true;
                    }

					GUILayout.Space(10);

					#region Tools
					EditorGUILayout.LabelField("Tools");

					var rect = GUILayoutUtility.GetRect(100, 100, 16, 16);

					var width = EditorGUIUtility.labelWidth;

					var selectRect = rect;
					selectRect.width = width;
					EditorGUI.SelectableLabel(selectRect, m_ColorHex);

					rect.x += width;
					rect.width -= width;
					var color = EditorGUI.ColorField(rect, m_ColorPicker);
					if (color != m_ColorPicker)
					{
						m_ColorPicker = color;

						var c = (Color32)color;
						if (c.a == 255)
						{
							m_ColorHex = "#"
								+ c.r.ToString("X2")
								+ c.g.ToString("X2")
								+ c.b.ToString("X2");
						}
						else
						{
							m_ColorHex = "#"
								+ c.r.ToString("X2")
								+ c.g.ToString("X2")
								+ c.b.ToString("X2")
								+ c.a.ToString("X2");
						}
					}

					m_ShowControlChar = EditorGUILayout.Toggle("Show Control char", m_ShowControlChar);
					#endregion

					GUILayout.Space(5);

					#region Preview
					if (m_ShowControlChar)
					{
						ShowControlChar();
					}
					else
					{
						m_LastText = null;
						m_PreviewText = m_Selected.Text;
					}

					EditorGUILayout.LabelField(m_PreviewTitle );
					GUILayout.Label(m_PreviewText );
					#endregion
				}
			}
			else
			{
				EditorGUILayout.HelpBox("Select a LocaId", MessageType.Info);
			}

			EditorGUILayout.EndVertical();
			#endregion

			EditorGUILayout.EndHorizontal();
		}

		private void DrawTree(LocaPath node)
		{
			var rect = GUILayoutUtility.GetRect(EditorGUIUtility.labelWidth, 16, EditorStyles.label);

			if (m_Selected == node)
			{
				GUI.Box(rect, GUIContent.none);
			}

			if (node.Children.Count == 0)
			{
				rect = EditorGUI.IndentedRect(rect);
				var cont = EditorGUIUtility.ObjectContent(null, typeof(TextAsset));
				cont.text = node.Key;
				rect.width -= 18;
				if (GUI.Button(rect, cont, EditorStyles.label))
				{
					m_Selected = node;
					EditorGUI.FocusTextInControl(string.Empty);
				}
				if (node.Parent != null)
				{
					rect.x = rect.xMax;
					rect.width = 18;
					if (GUI.Button(rect, "x", EditorStyles.miniButton))
					{
						hasDataChanged = true;
						node.Parent.Children.Remove(node);
					}
				}
			}
			else
			{
				var offset = EditorGUI.IndentedRect(rect);
				offset.x += 10;
				offset.width -= 10;
				if (GUI.Button(offset, ""))
				{
					//node.Foldout ^= true;
					m_Selected = node;
					EditorGUI.FocusTextInControl(string.Empty);
				}
				node.Foldout = EditorGUI.Foldout(rect, node.Foldout, node.Key, false);
				if (node.Foldout)
				{
					EditorGUI.indentLevel += 1;
					for (int i = node.Children.Count - 1; i >= 0; i--)
					{
						DrawTree(node.Children[i]);
					}
					EditorGUI.indentLevel -= 1;
				}
			}
		}

        private void DrawFilteredItems(LocaPath node, string filter)
        {
            if (node.Path != null && node.Children.Count == 0 && node.Path.ToLower().Contains(filter.ToLower()))
            {
                var cont = EditorGUIUtility.ObjectContent(null, typeof(TextAsset));
                cont.text = node.Path;
                EditorGUILayout.BeginHorizontal();
                if(m_Selected == node)
                {
                    GUILayout.Box(cont, EditorStyles.whiteLabel, GUILayout.Width(220));
                }
                else if (GUILayout.Button(cont, EditorStyles.label, GUILayout.Width(220)))
                {
                    m_Selected = node;
                    EditorGUI.FocusTextInControl(string.Empty);
                }
                if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(18), GUILayout.Height(18)))
                {
                    hasDataChanged = true;
                    node.Parent.Children.Remove(node);
                }
                EditorGUILayout.EndHorizontal();
            }
            foreach(var child in node.Children)
            {
                DrawFilteredItems(child, filter);
            }            
        }

		private void Create()
		{
			hasDataChanged = true;

			//Create item in selected category
			string path = m_Selected.Path + "/";
			if (m_Selected.Children.Count == 0)
			{
				path = path.Remove(path.Length - (m_Selected.Key.Length + 1));
			}
			m_CreateLocaKey = path + m_CreateLocaKey;

			m_Selected = m_Tree.GetChild(m_CreateLocaKey);
			m_CreateLocaKey = null;
			EditorGUI.FocusTextInControl(string.Empty);
		}

		private void ShowControlChar()
		{
			if (m_LastText != m_Selected.Text)
			{
				m_LastText = m_Selected.Text;
				using (var obj = GlobalObjectPool<System.Text.StringBuilder>.Get())
				{
					var sb = obj.Object;
					sb.Append(m_LastText);

					sb.Replace("\n", "¶\n");
					sb.Replace(' ', '·');
					sb.Replace("\t", "  →  ");

					m_PreviewText = sb.ToString();
					sb.Length = 0;
				}
			}
		}

		private bool CheckSaveWarningDisplay()
		{
			if (hasDataChanged)
			{
				var save = EditorUtility.DisplayDialog("Save?", "Loca data changed", "Save", "Ignore");
				if (save)
				{
					Save();
					return true;
				}
			}
			return false;
		}

		private void Reload()
		{
			var folder = LocalizationManager.Instance.LocaFolder;

			var files = Directory.GetFiles(folder, "*.dat");
			locaFiles = new GUIContent[files.Length];
			for (int i = 0; i < files.Length; i++)
			{
				locaFiles[i] = new GUIContent(Path.GetFileNameWithoutExtension(files[i]));
			}
		}

		private void Load()
		{
			var path = LocalizationManager.GetLocaFilePath(locaFiles[m_LocaFileSelection].text);
			var dateTime = File.GetLastWriteTime(path);
			if (dateTime != m_LastWriteTime)
			{
				m_LastWriteTime = dateTime;

				var data = LocalizationManager.LoadLocaJsonFile(path);				

				BuildTree(data);
			}
		}

		public void Save()
		{
			var name = locaFiles[m_LocaFileSelection].text;

			var data = new LocalizationManager.LocaDictionary();
			CollectLocaEntries(m_Tree, data);

			LocalizationManager.Instance.Save(name, data);

			hasDataChanged = false;
		}

		private void CollectLocaEntries(LocaPath node, LocalizationManager.LocaDictionary dict)
		{
			if (node.Children.Count == 0)
			{
				if (!string.IsNullOrEmpty(node.Key))
				{
					var path = node.Path;
					if (!dict.ContainsKey(path))
					{
						dict.Add(path, node.Text);
					}
					else
					{
						Debug.LogError("Key exception " + path);
					}
				}
			}
			else
			{
				for (int i = 0; i < node.Children.Count; i++)
				{
					CollectLocaEntries(node.Children[i], dict);
				}
			}
		}

		private LocaPath BuildTree(ICollection<KeyValuePair<string, string>> locaData, string selectedPath = null)
		{
			m_Tree.Clear();
            LocaPath selectedNode = null;
			foreach (var item in locaData)
			{
				var node = m_Tree.GetChild(item.Key);
				node.Text = item.Value;
                if (node.Path == selectedPath)
                    selectedNode = node;
			}
            return selectedNode;
		}

        private void ShowSelected()
        {
            if (m_Selected == null)
                return;
            LocaPath parent = m_Selected.Parent;
            while(parent != null)
            {
                parent.Foldout = true;
                Debug.Log(parent.Path);
                parent = parent.Parent;
            }            
        }

        private T WithoutSelectAll<T>(Func<T> guiCall)
        {
            bool preventSelection = 
				Event.current.type == EventType.MouseDown ||
				Event.current.type == EventType.MouseUp;

            Color oldCursorColor = GUI.skin.settings.cursorColor;

            if (preventSelection)
                GUI.skin.settings.cursorColor = new Color(0, 0, 0, 0);

            T value = guiCall();

            if (preventSelection)
                GUI.skin.settings.cursorColor = oldCursorColor;

            return value;
        }

    }
}
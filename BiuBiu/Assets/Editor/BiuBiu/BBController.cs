using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using ProtoBuf;
using System.IO;

namespace BiuBiu
{
    class BBController
    {
        class ListItem : GUIContent
        {
            public int realIndex = 0;

            public ListItem(int index, string text)
            {
                realIndex = index;
                this.text = text;
            }
        }

        ListItem[] displayNames = null;
        ListItem[] messageNames = null;
        bool enableFilter = false;
        ListItem[] filterMessageNames = null;

        public void Init()
        {
            messageNames = new ListItem[MessageList.messageList.Count];
            for (int i = 0; i < MessageList.messageList.Count; ++i)
            {
                var type = MessageList.messageList[i];
                messageNames[i] = new ListItem(i, type.Name);
            }

            messageOp.SendClickEvent += MessageOp_SendClickEvent;
            BBNetOp.Instance.SetNet(new ProtoNet());
        }

        private void MessageOp_SendClickEvent()
        {
            if (currentMessageData == null)
                return;
            if (displayNames == null || displayNames.Length == 0)
                return;

            var item = displayNames[currentMessageDataIndex];
            var currentType = MessageList.messageList[item.realIndex];
            BBNetOp.Instance.Send(currentType, currentMessageData, messageOp.GetTimes(), messageOp.timeSpan);
        }

        Vector2 mainView;

        public void DrawView()
        {
            if (messageNames == null)
                return;

            mainView = EditorGUILayout.BeginScrollView(mainView);
            EditorGUILayout.BeginVertical();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            DrawMessageListArea();

            EditorGUILayout.Space();

            DrawMessageArea();


            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        Vector2 messageListScrollViewPosition;
        int selectedMessageIndex = 0;

        private Dictionary<int, MessageData> messageDataDic = new Dictionary<int, MessageData>();

        int currentMessageDataIndex = -1;
        MessageData currentMessageData = null;

        MessageOp messageOp = new MessageOp();

        private void DrawMessageStructureArea()
        {
            if (displayNames == null || displayNames.Length == 0)
            {
                currentMessageData = null;
                EditorGUILayout.LabelField("No Message Selected");
                return;
            }

            if (currentMessageDataIndex != selectedMessageIndex)
            {
                currentMessageDataIndex = selectedMessageIndex;

                var messageIndex = displayNames[currentMessageDataIndex].realIndex;
                if (messageIndex < 0 || messageIndex >= MessageList.messageList.Count)
                {
                    currentMessageData = null;
                    EditorGUILayout.LabelField("Selection Index Error");                    
                    return;
                }
                var message = MessageList.messageList[messageIndex];

                if (!messageDataDic.TryGetValue(messageIndex, out currentMessageData))
                {
                    currentMessageData = new MessageData(message);
                    messageDataDic[messageIndex] = currentMessageData;
                }
            }

            if (currentMessageData != null)
                currentMessageData.Draw();
            else
            {
                EditorGUILayout.LabelField("No Message Selected");
            }
        }

        string filter;
        string lastFilter;
        
        private void DrawMessageListArea()
        {
            using (var v = new EditorGUILayout.VerticalScope())
            {
                using (var h = new EditorGUILayout.HorizontalScope())
                {
                    filter = EditorGUILayout.DelayedTextField(filter);
                    if(GUILayout.Button("Clear"))
                    {
                        filter = string.Empty;
                        lastFilter = string.Empty;
                    }
                }

                if(string.IsNullOrEmpty(filter))
                {
                    displayNames = messageNames;
                    if (enableFilter)
                    {
                        if (displayNames != null && displayNames.Length > 0)
                            selectedMessageIndex = 0;
                        else
                            selectedMessageIndex = -1;
                    }
                    enableFilter = false;
                }
                else
                {
                    enableFilter = true;
                    if(lastFilter != filter)
                    {
                        displayNames = FilterMessageNames(filter);
                        if (displayNames != null && displayNames.Length > 0)
                            selectedMessageIndex = 0;
                        else
                            selectedMessageIndex = -1;
                        lastFilter = filter;
                    }
                }

                EditorGUILayout.Space();

                messageListScrollViewPosition = GUILayout.BeginScrollView(messageListScrollViewPosition, GUILayout.ExpandWidth(false));

                selectedMessageIndex = GUILayout.SelectionGrid(selectedMessageIndex, displayNames, 1);

                GUILayout.EndScrollView();
            }                
        }

        private ListItem[] FilterMessageNames(string filter)
        {
            List<ListItem> names = new List<ListItem>();
            foreach(var name in messageNames)
            {
                var targetText = name.text.ToLower();
                var filterText = filter.ToLower();
                if(targetText.Contains(filterText))
                {
                    names.Add(new ListItem(name.realIndex, name.text));
                }
            }
            filterMessageNames = names.ToArray();
            return filterMessageNames;
        }

        Vector2 messageArea;

        private void DrawMessageArea()
        {
            using (var v = new EditorGUILayout.ScrollViewScope(messageArea))
            {
                messageArea = v.scrollPosition;

                DrawMessageOpArea();

                GUILayout.Space(30);

                DrawMessageStructureArea();
            }
        }

        private void DrawMessageOpArea()
        {
            messageOp.Draw();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace BiuBiu
{
    internal class MessageOp
    {
        private int times = 1;
        private bool randomTimes = false;
        private int minTimes = 1;
        private int maxTimes = 2;

        public float timeSpan = 0;

        public event Action SendClickEvent = null;

        public void Draw()
        {
            using(var v = new EditorGUILayout.VerticalScope())
            {
                EditorGUI.DrawRect(v.rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));

                using(var h = new EditorGUILayout.HorizontalScope())
                {
                    times = EditorGUILayout.IntField("Times", times);
                    using(var t = new EditorGUILayout.ToggleGroupScope("Random", randomTimes))
                    {
                        randomTimes = t.enabled;
                        minTimes = EditorGUILayout.DelayedIntField("Min Times", minTimes);
                        maxTimes = EditorGUILayout.DelayedIntField("Max Times", maxTimes);
                        if(minTimes >= maxTimes)
                            maxTimes = minTimes + 1;
                    }
                }

                timeSpan = EditorGUILayout.FloatField("Time Span", timeSpan);
                if(timeSpan < 0)
                    timeSpan = 0;

                if(GUILayout.Button("Send"))
                {
                    OnSendBtnClick();
                }
            }
        }

        private void OnSendBtnClick()
        {
            if(SendClickEvent != null)
                SendClickEvent();
        }

        public int GetTimes()
        {
            if(randomTimes)
            {
                if(minTimes >= maxTimes)
                {
                    Debug.LogFormat("Field {0} Value Invalid <{1}, {2}>", "Times", minTimes, maxTimes);
                    return minTimes;
                }
                return UnityEngine.Random.Range(minTimes, maxTimes);
            }
            else
            {
                return times;
            }
        }
    }
}

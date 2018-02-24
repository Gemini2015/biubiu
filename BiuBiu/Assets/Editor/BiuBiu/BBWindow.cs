using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BiuBiu
{
    public class BBWindow : EditorWindow
    {

        [UnityEditor.MenuItem("BiuBiu/Start")]
        public static void OpenMsgBufferAssistWindow()
        {
            var win = GetWindow<BBWindow>();
            if(win.controller == null)
            {
                win.controller = new BBController();
                win.controller.Init();
            }
        }

        BBController controller = null;
                
        void OnGUI()
        {
            if (controller == null)
                return;

            controller.DrawView();
        }
    }
}


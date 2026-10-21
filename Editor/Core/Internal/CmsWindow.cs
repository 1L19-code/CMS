using System;
using UnityEditor;
using UnityEngine;

namespace CMS.Editor
{
    internal class CmsWindow : EditorWindow
    {
        [MenuItem("CMS/Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<CmsWindow>();
            window.titleContent = new GUIContent("CMS");
            window.Show();
        }

        private void CreateGUI()
        {
            
        }
    }
}
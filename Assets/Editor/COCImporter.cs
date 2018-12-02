using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using DotNetSpeech;

public class COCImporter : EditorWindow {

    [MenuItem("导入/文本")]
    private static void ShowWindow()
    {
        COCImporter window = new COCImporter();
        window.Show();
    }

    void OnGUI()
    {
        if (GUILayout.Button("导入人物关系", GUILayout.Height(100)))
        {
            TextAsset t = Resources.Load<TextAsset>("文本划分/人物关系");
            Init(t.text);
        }
        if (GUILayout.Button("整理文档",GUILayout.Height(100)))
        {
            TextAsset t = Resources.Load<TextAsset>("文本划分/聊天记录");
            TextSplit(t.text);
        }
        if (GUILayout.Button("TTS", GUILayout.Height(100)))
        {
            TestAudio(" 你好，这是一个测试语句");
        }
    }

    #region 文本导入
    Dictionary<string, string> RoleDic = null;
    void Init(string text)
    {
        RoleDic = new Dictionary<string, string>();
        string[] textSource = text.Split(new char[]{'\n'});
        foreach (var t in textSource)
        {
            string[] subt = t.Split(new char[] { '=' });
            RoleDic.Add(subt[0], subt[1].Replace("\r",""));
        }
        EditorUtility.DisplayDialog("", "导入完成，人物数量：" + RoleDic.Count, "OK");
    }
    void TextSplit(string text)
    {
        StringBuilder sb = new StringBuilder();
        string[] textSource = text.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        for(int i = 0; i < textSource.Length; i++)
        {
            string t = textSource[i];
            //if (t.StartsWith("(") || t.StartsWith("（"))
            //{
            //    //超游语句
            //}
            //else if (t.StartsWith("#"))
            //{
            //    //动作语句

            //}
            //else if (t.StartsWith("“") || t.StartsWith("\""))
            //{
            //    //说话语句

            //}
            //else 
            if (Regex.IsMatch(t, @"\(\d*\)"))
            {
                //如果是QQ号，开始替换
                foreach (var kv in RoleDic)
                {
                    if (t.Contains(kv.Key))
                    {
                        sb.AppendLine("【" + kv.Value + "】");
                    }
                }
            }
            else
            {
                if (t != "\r" && !t.Contains("撤回了一条消息"))
                    sb.AppendLine(t);
            }
        }
        StreamWriter sw = new StreamWriter(Application.dataPath + "/Resources/文本划分/输出结果.txt", false);
        sw.Write(sb.ToString());
        sw.Close();
    }
    #endregion

    #region TTS
    void TestAudio(string text)
    {
        SpVoice voice = new SpVoice();
        SpFileStream sfs = new SpFileStream();
        sfs.Open("c:\\1.wav", SpeechStreamFileMode.SSFMCreateForWrite, false);
        voice.AudioOutputStream = sfs;
        voice.Speak(text, SpeechVoiceSpeakFlags.SVSFlagsAsync);
        voice.WaitUntilDone(1000);
        sfs.Close();
    }
    #endregion
}

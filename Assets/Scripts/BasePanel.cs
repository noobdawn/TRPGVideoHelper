using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

public struct Role
{
    public string Name;
    //TTS语音角色
    public string TTS_Role;
    //立绘名
    public string PicName;
    //
    public Texture Tex;
}

public struct Sentence
{
    //说话者，索引Role
    public string Speaker;
    //内容
    public string Text;
}

public class BasePanel : MonoBehaviour {

    [SerializeField]
    Text speakerText;
    [SerializeField]
    Text contentText;
    [SerializeField]
    int SentenceWait;   //每句之间等待的时间，单位(ms)
    [SerializeField]
    RawImage RolePic;
    [SerializeField]
    RawImage ScenePic;
    [SerializeField]
    AudioSource Music;
    [SerializeField]
    AudioSource AudioEffect;

    bool isTTSThreadDone;
    int curIdx;
    string curTTS;
    TTSOperator tts;

    #region 初始化
	void Start () {
	    //载入角色和语音之间的关系
        TextAsset t = Resources.Load<TextAsset>("文本读取/语音关系");
        InitRole(t.text);
        //载入整理后的聊天记录
        t = Resources.Load<TextAsset>("文本读取/文本内容");
        InitChat(t.text);
        curIdx = -1;
        tts = new TTSOperator();
        tts.Init();
        StartPlay();
	}

    Dictionary<string, Role> RoleDic = null;
    void InitRole(string text)
    {
        RoleDic = new Dictionary<string, Role>();
        string[] textSource = text.Split(new char[] { '\n' });
        foreach (var t in textSource)
        {
            string[] subt = t.Split(new char[] { '=' });
            Role r = new Role();
            r.Name = subt[1];
            r.TTS_Role = subt[0];
            r.PicName = subt[2].Replace("\r", "");
            r.Tex = Resources.Load<Texture>("立绘/" + r.PicName);
            RoleDic.Add(subt[1], r);
        }
    }

    Dictionary<int, Sentence> sentenceDic = null;   
    void InitChat(string text)
    {
        sentenceDic = new Dictionary<int, Sentence>();
        string lastSpeaker = "";
        string[] textSource = text.Split(new char[] { '\n' });
        int idx = 0;
        foreach (var t in textSource)
        {
            if (t.StartsWith("【"))
            {
                lastSpeaker = t.Substring(1, t.Length - 3);
                continue;
            }
            Sentence s = new Sentence();
            s.Speaker = lastSpeaker;
            s.Text = t;
            sentenceDic.Add(idx++, s);
        }
    }
    #endregion

    void StartPlay()
    {
        isTTSThreadDone = true;
    }

    public void Next()
    {
        curIdx++;
        if (sentenceDic.ContainsKey(curIdx))
        {
            Sentence s = sentenceDic[curIdx];
            //功能指令！
            if (s.Text.StartsWith("scene "))
            {
                ScenePic.texture = Resources.Load<Texture>("场景/" + s.Text.Substring(6, s.Text.Length - 7));
                Next();
                return;
            }
            else if (s.Text.StartsWith("music "))
            {
                AudioClip music = Resources.Load<AudioClip>("音乐/" + s.Text.Substring(6, s.Text.Length - 7));
                Music.clip = music;
                Music.Play();
                Next();
                return;
            }
            else if (s.Text.StartsWith("audio "))
            {
                AudioClip music = Resources.Load<AudioClip>("音效/" + s.Text.Substring(6, s.Text.Length - 7));
                AudioEffect.clip = music;
                AudioEffect.Play();
                Next();
                return;
            }
            speakerText.text = s.Speaker;
            contentText.text = s.Text;
            curTTS = s.Text;
            //设置立绘
            RolePic.texture = RoleDic[s.Speaker].Tex;
            var trans = RolePic.GetComponent<RectTransform>();
            trans.sizeDelta = new Vector2(RolePic.texture.width, RolePic.texture.height);
        }
        else
        {
            Hide();
        }
    }

    public void Hide()
    {
        GetComponent<RectTransform>().localScale = Vector3.zero;

    }

	void Update () {
        if (!isTTSThreadDone) return;
        Next();
        if (string.IsNullOrEmpty(curTTS)) return;
        isTTSThreadDone = false;
        ThreadPool.QueueUserWorkItem(new WaitCallback(RunThreaded), curTTS);
	}

    private void RunThreaded(object o)
    {
        tts.Translate((string)o);
        Thread.Sleep(SentenceWait);
        isTTSThreadDone = true;
    }


}

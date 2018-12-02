using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DotNetSpeech;

public class TTSOperator {
    private SpVoice spVoice;
    private string outputPath;
    public void Init()
    {
        spVoice = new SpVoice();
    }

    public void Translate(string text, string speaker = "Microsoft Lili", int volume = 10)
    {
        spVoice.Voice = spVoice.GetVoices("name=" + speaker).Item(0);
        spVoice.Volume = volume;
        spVoice.Speak(text, SpeechVoiceSpeakFlags.SVSFDefault);
    }
}

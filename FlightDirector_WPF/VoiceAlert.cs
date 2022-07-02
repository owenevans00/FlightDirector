using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

namespace FlightDirector_WPF
{
    internal static class VoiceAlert
    {
        static SpeechSynthesizer synth;
        static VoiceAlert()
        {
            synth = new SpeechSynthesizer();
            synth.SetOutputToDefaultAudioDevice();
            synth.SelectVoiceByHints(VoiceGender.Female);
        }

        internal static void Alert(string text)
        {
            synth.Speak(text);
        }
    }
}

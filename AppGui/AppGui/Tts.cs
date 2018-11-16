using System;

using System.Media;
using Microsoft.Speech.Synthesis;

namespace AppGui
{
    class Tts
    {
        SpeechSynthesizer tts = null;
        static SoundPlayer player = null;

        public Tts()
        {
            //create sound  player
            player = new SoundPlayer();
            
            //create speech synthesizer
            tts = new SpeechSynthesizer();

            //set voice
            tts.SelectVoiceByHints(VoiceGender.Male, VoiceAge.NotSet, 0, new System.Globalization.CultureInfo("pt-PT"));

            //set function to play audio after synthesis is complete
            tts.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(tts_SpeakCompleted);
        }

        /*
         * Speak
         * 
         * @param text - text to convert
         */
        public void Speak(string text)
        {   
            //create audio stream with speech
            player.Stream = new System.IO.MemoryStream();
            tts.SetOutputToWaveStream(player.Stream);
            tts.SpeakAsync(text);
        }

        /*
         * tts_SpeakCompleted
         */
        void tts_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            if (player.Stream != null)
            {
                //play stream
                player.Stream.Position = 0;
                player.Play();
                player.Stream = null;  //  NEW 2015
            }
        }
    }
}

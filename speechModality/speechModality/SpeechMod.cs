using System;
using mmisharp;
using Microsoft.Speech.Recognition;
using multimodal;
using System.Timers;

namespace speechModality
{
    public class SpeechMod
    {
        private SpeechRecognitionEngine sre;
        private Grammar gr;
        public event EventHandler<SpeechEventArg> Recognized;
        private Tts tts;
        private SemanticValue actualSemantic = null;

        protected virtual void onRecognized(SpeechEventArg msg)
        {
            EventHandler<SpeechEventArg> handler = Recognized;
            if (handler != null)
            {
                handler(this, msg);
            }
        }

        private LifeCycleEvents lce;
        private MmiCommunication mmic;

        public SpeechMod()
        {
            //init LifeCycleEvents..
            lce = new LifeCycleEvents("ASR", "FUSION", "speech-1", "acoustic", "command"); // LifeCycleEvents(string source, string target, string id, string medium, string mode)
            mmic = new MmiCommunication("localhost",9876,"User1", "ASR");  //PORT TO FUSION - uncomment this line to work with fusion later
            //mmic = new MmiCommunication("localhost", 8000, "User1", "ASR"); // MmiCommunication(string IMhost, int portIM, string UserOD, string thisModalityName)

            mmic.Send(lce.NewContextRequest());

            //load pt recognizer
            sre = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("pt-PT"));
            gr = new Grammar(Environment.CurrentDirectory + "\\ptG.grxml", "rootRule");
            sre.LoadGrammar(gr);

            tts = new Tts();

            sre.SetInputToDefaultAudioDevice();
            sre.RecognizeAsync(RecognizeMode.Multiple);
            sre.SpeechRecognized += Sre_SpeechRecognized;
            sre.SpeechHypothesized += Sre_SpeechHypothesized;
        }


        private void Sre_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            onRecognized(new SpeechEventArg() { Text = e.Result.Text, Confidence = e.Result.Confidence, Final = false });
        }

        private void Sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            onRecognized(new SpeechEventArg() { Text = e.Result.Text, Confidence = e.Result.Confidence, Final = true });
            
            if (e.Result.Confidence <= 0.3)
            {
                tts.Speak("Desculpa, não consegui entender.");
                actualSemantic = null;
            }

            if (e.Result.Confidence < 0.4)
            {
                return;
            }

            if(e.Result.Confidence <= 0.7)
            {
                actualSemantic = e.Result.Semantics;

                if (e.Result.Semantics.ContainsKey("closeTab"))
                {
                    if (e.Result.Semantics["closeTab"].Value.ToString() == "CLOSE_TAB")
                    {
                        tts.Speak("Tem a certeza que pretende fechar o separador ?");
                        return;
                    }
                }

                if (e.Result.Semantics.ContainsKey("quitChrome"))
                {
                    if (e.Result.Semantics["quitChrome"].Value.ToString() == "QUIT_CHROME")
                    {
                        tts.Speak("Tem a certeza que pretende fechar o browser?");
                        return;
                    }
                }
                actualSemantic = null;
            }
            else
            {
                if (e.Result.Semantics.ContainsKey("closeTab"))
                {
                    if (e.Result.Semantics["closeTab"].Value.ToString() == "CLOSE_TAB")
                    {
                        actualSemantic = e.Result.Semantics;
                        tts.Speak("Tem a certeza que pretende fechar o separador?");
                        return;
                    }
                }

                if (e.Result.Semantics.ContainsKey("quitChrome"))
                {
                    if (e.Result.Semantics["quitChrome"].Value.ToString() == "QUIT_CHROME")
                    {
                        actualSemantic = e.Result.Semantics;
                        tts.Speak("Tem a certeza que pretende fechar o browser?");
                        return;
                    }
                }
            }

            SemanticValue semanticValue = null;

            if (actualSemantic != null)
            {
                if (e.Result.Semantics.ContainsKey("yes"))
                {
                    switch (e.Result.Semantics["yes"].Value.ToString())
                    {
                        case "AFFIRMATIVE":

                            if (e.Result.Semantics["yes"].Value.ToString() == "AFFIRMATIVE")
                            {
                                tts.Speak("Com certeza.");
                            }
                            break;
                    }
                }
                else
                {
                    if (e.Result.Semantics.ContainsKey("no"))
                    {
                        switch (e.Result.Semantics["no"].Value.ToString())
                        {
                            case "REJECT":
                                if (e.Result.Semantics["no"].Value.ToString() == "REJECT")
                                {
                                    tts.Speak("Ok, desculpa, às vezes ando um pouco distraída.");
                                }
                                return;
                        }
                    }
                }

                semanticValue = actualSemantic;
                actualSemantic = null;
            }
            else
            {
                semanticValue = e.Result.Semantics;
            }

            // if a command was recognized and the confirmation of a previous command was ignored by the user, disable it
            actualSemantic = null;

           
            string json = "{ \"recognized\": [";
            foreach (var resultSemantic in semanticValue)
            {
                json += "\"" + resultSemantic.Value.Value + "\", ";
            }
            json = json.Substring(0, json.Length - 2);
            json += "] }";

            var exNot = lce.ExtensionNotification(e.Result.Audio.StartTime + "", e.Result.Audio.StartTime.Add(e.Result.Audio.Duration) + "", e.Result.Confidence, json);
            mmic.Send(exNot);

        }
    }
}

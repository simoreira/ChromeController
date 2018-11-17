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
        //private Boolean assistantSpeaking, acceptingVoiceInput, awaitingConfirmation, awaitingCloseChromeConfirmation = false;
        private SemanticValue actualSemantic = null;
        Timer speakingTimer;
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
            //mmic = new MmiCommunication("localhost",9876,"User1", "ASR");  //PORT TO FUSION - uncomment this line to work with fusion later
            mmic = new MmiCommunication("localhost", 8000, "User1", "ASR"); // MmiCommunication(string IMhost, int portIM, string UserOD, string thisModalityName)

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

       /* private void OnSpeakingEnded(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("Assistant stopped speaking.");
            assistantSpeaking = false;

        }
        private void Speak(String text, int seconds)
        {
            string str = "<speak version=\"1.0\"";
            str += " xmlns:ssml=\"http://www.w3.org/2001/10/synthesis\"";
            str += " xml:lang=\"pt-PT\">";
            str += text;
            str += "</speak>";

            tts.Speak(str, 0);

            // enable talking flag
            assistantSpeaking = true;
            Console.WriteLine("Assistant speaking.");

            speakingTimer = new Timer(seconds * 1000);
            speakingTimer.Elapsed += OnSpeakingEnded;
            speakingTimer.AutoReset = false;
            speakingTimer.Enabled = true;
        }

        private void Speak(String text)
        {
            Speak(text, 4);
        }*/
       
        private void Sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            onRecognized(new SpeechEventArg() { Text = e.Result.Text, Confidence = e.Result.Confidence, Final = true });

            

            // ignore low confidance levels
            if (e.Result.Confidence < 0.4)
            {
                return;
            }


            if (e.Result.Confidence <= 0.3)
            {
                tts.Speak("Desculpa, não consegui entender.");
                actualSemantic = null;
                return;
            }

            if(e.Result.Confidence <= 0.8)
            {
                actualSemantic = e.Result.Semantics;

                //SemanticValue svalue = (string)e.Result.Semantics["closeTab"].Value.ToString();
                if(e.Result.Semantics.ContainsKey("closeTab"))
                {
                    //Em principio funciona sem o switch case 
                        if(e.Result.Semantics["closeTab"].Value.ToString() == "CLOSE_TAB")
                        {

                            actualSemantic = e.Result.Semantics;
                            tts.Speak("Tem a certeza que pretende fechar?");
                            return; 
                        }

                    
                }
                    //actualSemantic = e.Result.Semantics;
                    //tts.Speak("Tem a certeza que quer fechar separador?");
                
                //Console.WriteLine("closetab: " + e.Result.Semantics["closeTab"].Value.ToString());
                //actualSemantic = null;
            }
            else
            {
                if (e.Result.Semantics.ContainsKey("closeTab"))
                {
                    switch (e.Result.Semantics["closeTab"].Value.ToString())
                    {
                        case "CLOSE_TAB":
                            
                            if (e.Result.Semantics["closeTab"].Value.ToString() == "CLOSE_TAB")
                            {
                                actualSemantic = e.Result.Semantics;
                                tts.Speak("Tem a certeza que pretende fechar?");
                            }

                            break;
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
                            tts.Speak("sim.");
                            break;
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

            //tts.Speak("Comando enviado");

        }
    }
}

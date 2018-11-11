using System;
using Microsoft.Speech.Recognition;


class SpeechRecognizer
{ 
    private SpeechRecognitionEngine sr;

    /*
     * SpeechRecognizer
     * 
     * 
     * @param GName - grammar file name
     */
    public SpeechRecognizer()
    {
        
        //creates the speech recognizer engine
        sr = new SpeechRecognitionEngine();
        sr.SetInputToDefaultAudioDevice();

        
        Grammar gr = CreateGrammar();

        //load Grammar to speech engine
        sr.LoadGrammar(gr);
        
        //assigns a method, to execute when speech is recognized
        sr.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(SpeechRecognized);
        sr.RecognizeAsync(RecognizeMode.Multiple);
        Console.WriteLine("Starting Asynchronous speech recognition...");
    }

    /*
     * SpeechRecognized
     * 
     * EventHandler
     * 
     * 
    */
    public void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
    {
        //gets recognized text
        string text = e.Result.Text;

        Console.WriteLine(text);
    }


    private Grammar CreateGrammar()
    {
        Choices confirm = new Choices(new string[] { "sim" });
        SemanticResultValue confirmClose = new SemanticResultValue(confirm, "sim");

        Choices f = new Choices();
        f.Add(confirmClose);
        GrammarBuilder fGrammar = (GrammarBuilder)f;

        Grammar g = new Grammar((GrammarBuilder)fGrammar);
        return g;
    }



}

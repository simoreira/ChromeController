using System.Windows;

namespace speechModality
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private SpeechMod _sm;
        public MainWindow()
        {
            InitializeComponent();

            _sm = new SpeechMod();
            _sm.Recognized += _sm_Recognized;
        }

        private void _sm_Recognized(object sender, SpeechEventArg e)
        {
            result.Text = e.Text;
            confidence.Text = e.Confidence + "";
            if (e.Final) result.FontWeight = FontWeights.Bold;
            else result.FontWeight = FontWeights.Normal;
        }
    }
}

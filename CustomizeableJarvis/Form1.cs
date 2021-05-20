﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using CustomizeableJarvis.Properties;
using System.Globalization;
using System.Threading;
using System.Diagnostics;

namespace CustomizeableJarvis
{
    public partial class frmMain : Form
    {
        SpeechRecognitionEngine _recognizer = new SpeechRecognitionEngine(); //Creates the Speech Recognition Engine and sets the default language to English-US dialect
        SpeechRecognitionEngine startlistening = new SpeechRecognitionEngine();
        public static SpeechSynthesizer Jarvis = new SpeechSynthesizer(); //Creates the speech synthesizer to allow the program to respond
        Grammar shellcommandgrammar; //Grammar variables allow us to load and unload words into Jarvis's vocabulary and update them during runtime without the need to restart the program
        Grammar webcommandgrammar, socialcommandgrammar, MusicGrammar, VideoGrammar;
        String[] ArrayShellCommands; //These arrays will be loaded with custom commands, responses, and File Locations/URLs
        String[] ArrayShellResponse;
        String[] ArrayShellLocation;
        String[] ArrayWebCommands;
        String[] ArrayWebResponse;
        String[] ArrayWebURL;
        String[] ArraySocialCommands;
        String[] ArraySocialResponse;
        String[] MyMusic;
        String[] MyVideos;
        String[] MyMusicNames; //This will store music names
        String[] MyVideoNames; //This will store video names
        String[] MyMusicPaths; //This list will store only Music file paths
        String[] MyVideoPaths; //This list will store only Video file paths
        TagLib.File music; //This variable is for reading ID3 tags of music files so we can refer to them by Song Title and Artist
        public static string scpath; //These strings will be used to refer to the Shell Command text document
        public static string srpath; //These strings will be used to refer to the Shell Response text document
        public static string slpath; //These strings will be used to refer to the Shell Location text document
        public static string webcpath; //These strings will be used to refer to the Web Command text document
        public static string webrpath; //These strings will be used to refer to the Web Response text document
        public static string weblpath; //These strings will be used to refer to the Web URL text document
        public static string socpath; //These strings will be used to refer to the Social Command text document
        public static string sorpath; //These strings will be used to refer to the Social Response text document
        public static String userName = Environment.UserName; //This variable stores the name of your computer so we can refer to specific file locations or assume your name
        public static String QEvent; //This variable is used periodically to determine which event we are trying to achieve when there are multiple possible outcomes
        int i = 0; //This integer variable will be used for loops and referring to specific elements in arrays
        int SelectedMusicFile;
        int recTimeOut = 0;
        StreamWriter sw; //Sets a variable that allows us to read and write to text documents
        ComboBox cmbxLang;
        Form frmEdit;

        public frmMain()
        {
            InitializeComponent();
            if (Settings.Default.Language == String.Empty)
            {
                AskForACountry();
                Jarvis.SpeakAsyncCancelAll();
            }
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(Settings.Default.Language);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(Settings.Default.Language);
                _recognizer = new SpeechRecognitionEngine(new CultureInfo(Settings.Default.Language));
                startlistening = new SpeechRecognitionEngine(new CultureInfo(Settings.Default.Language));
            }
            catch (Exception ex) { ErrorLog(ex.ToString()); AskForACountry(); }
            lblLanguage.Text = Settings.Default.Language;
            if (Settings.Default.User.ToString() == String.Empty) //Checks for user info. If none is available it sets to default
            { Settings.Default.User = userName; Settings.Default.Save(); Jarvis.SpeakAsync("It is nice to make your acquaintance " + Settings.Default.User + ", my name is JARVIS and I will be your personal digital assistant"); }
            else
            { Jarvis.Speak("Hello " + Settings.Default.User + ", allow me to load the necessary files"); }
            string[] defaultcommands = (File.ReadAllLines(@"Default Commands.txt")); //Loading all default commands into an array
            foreach (string command in defaultcommands)
            {
                lstCommands.Items.Add(command); //We load them into the listbox on start up just in case someone forgets the commands and they can say, "Show listbox"
            }

            Directory.CreateDirectory(@"C:\Users\" + userName + "\\Documents\\Jarvis Custom Commands"); //We create 'Jarvis Custom Commands' folder in the My Documents folder so we have a place to store our text documents
            Settings.Default.ShellC = @"C:\Users\" + userName + "\\Documents\\Jarvis Custom Commands\\Shell Commands.txt"; //We save the text document file locations into our settings even before they've been created so we can refer to them easily and globally
            Settings.Default.ShellR = @"C:\Users\" + userName + "\\Documents\\Jarvis Custom Commands\\Shell Response.txt";
            Settings.Default.ShellL = @"C:\Users\" + userName + "\\Documents\\Jarvis Custom Commands\\Shell Location.txt";
            Settings.Default.WebC = @"C:\Users\" + userName + "\\Documents\\Jarvis Custom Commands\\Web Commands.txt";
            Settings.Default.WebR = @"C:\Users\" + userName + "\\Documents\\Jarvis Custom Commands\\Web Response.txt";
            Settings.Default.WebL = @"C:\Users\" + userName + "\\Documents\\Jarvis Custom Commands\\Web URL.txt";
            Settings.Default.SocC = @"C:\Users\" + userName + "\\Documents\\Jarvis Custom Commands\\Social Commands.txt";
            Settings.Default.SocR = @"C:\Users\" + userName + "\\Documents\\Jarvis Custom Commands\\Social Response.txt";
            Settings.Default.Save();

            scpath = Settings.Default.ShellC; //The text document file locations are passed on to these variables because they are easier to refer to but admittedly is an unnecessary step
            srpath = Settings.Default.ShellR;
            slpath = Settings.Default.ShellL;
            webcpath = Settings.Default.WebC;
            webrpath = Settings.Default.WebR;
            weblpath = Settings.Default.WebL;
            socpath = Settings.Default.SocC;
            sorpath = Settings.Default.SocR;

            if (!File.Exists(scpath)) //This is used to create the Custom Command text documents if they don't already exist and write in default commands so we don't encounter any errors. These text documents should always have at least one valid line in them
            { sw = File.CreateText(scpath); sw.Write("My Documents"); sw.Close(); }
            if (!File.Exists(srpath))
            { sw = File.CreateText(srpath); sw.Write("Right away"); sw.Close(); }
            if (!File.Exists(slpath))
            { sw = File.CreateText(slpath); sw.Write(@"C:\Users\" + userName + "\\Documents"); sw.Close(); }
            if (!File.Exists(webcpath))
            { sw = File.CreateText(webcpath); sw.Write("Open Google"); sw.Close(); }
            if (!File.Exists(webrpath))
            { sw = File.CreateText(webrpath); sw.Write("Very well"); sw.Close(); }
            if (!File.Exists(weblpath))
            { sw = File.CreateText(weblpath); sw.Write("http://www.google.com"); sw.Close(); }
            if (!File.Exists(socpath))
            { sw = File.CreateText(socpath); sw.Write("How are you"); sw.Close(); }
            if (!File.Exists(sorpath))
            { sw = File.CreateText(sorpath); sw.Write("I'm doing well thanks for asking"); sw.Close(); }

            try
            {
                ReadDirectories(); //This procedure is used to load all of our music/video files
            }
            catch (Exception ex){ ErrorLog(ex.ToString()); Jarvis.SpeakAsync("You may need to restart the program in order to access music files"); } //The try catch block is in case of any unkown errors to prevent the program from crashing.

            ArrayShellCommands = File.ReadAllLines(scpath); //This loads all written commands in our Custom Commands text documents into arrays so they can be loaded into our grammars
            ArrayShellResponse = File.ReadAllLines(srpath);
            ArrayShellLocation = File.ReadAllLines(slpath);
            ArrayWebCommands = File.ReadAllLines(webcpath); //This loads all written commands in our Custom Commands text documents into arrays so they can be loaded into our grammars
            ArrayWebResponse = File.ReadAllLines(webrpath);
            ArrayWebURL = File.ReadAllLines(weblpath);
            ArraySocialCommands = File.ReadAllLines(socpath); //This loads all written commands in our Custom Commands text documents into arrays so they can be loaded into our grammars
            ArraySocialResponse = File.ReadAllLines(sorpath);

            //The following try catch blocks load our custom commands into our grammars. The catch block is in case of any blank lines or other unforseeable errors
            try
            { shellcommandgrammar = new Grammar(new GrammarBuilder(new Choices(ArrayShellCommands))); _recognizer.LoadGrammarAsync(shellcommandgrammar); }
            catch
            { Jarvis.SpeakAsync("I've detected an in valid entry in your shell commands, possibly a blank line. Shell commands will cease to work until it is fixed."); }
            try
            { webcommandgrammar = new Grammar(new GrammarBuilder(new Choices(ArrayWebCommands))); _recognizer.LoadGrammarAsync(webcommandgrammar); }
            catch
            { Jarvis.SpeakAsync("I've detected an in valid entry in your web commands, possibly a blank line. Web commands will cease to work until it is fixed."); }
            try
            { socialcommandgrammar = new Grammar(new GrammarBuilder(new Choices(ArraySocialCommands))); _recognizer.LoadGrammarAsync(socialcommandgrammar); }
            catch
            { Jarvis.SpeakAsync("I've detected an in valid entry in your social commands, possibly a blank line. Social commands will cease to work until it is fixed."); }

            _recognizer.SetInputToDefaultAudioDevice(); //Sets Mic input to the default Mic
            Choices chVolume = new Choices();
            for (int inNum = 0; inNum <= 100; inNum++)
            {
                chVolume.Add(Convert.ToString("Set the volume at " + inNum + " percent"));
            }
            _recognizer.LoadGrammarAsync(new Grammar(new GrammarBuilder(chVolume)));
            _recognizer.LoadGrammarAsync(new Grammar(new GrammarBuilder(new Choices(File.ReadAllLines(@"Default Commands.txt"))))); //Load our Default Commands text document so we have commands to start with
            _recognizer.LoadGrammarAsync(new Grammar(new GrammarBuilder(new Choices(AlarmAM)))); //Loads AM times for our alarm feature
            _recognizer.LoadGrammarAsync(new Grammar(new GrammarBuilder(new Choices(AlarmPM)))); //Loads PM times for our alarm feature *Might consider combining them
            _recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(Shell_SpeechRecognized); //These are event handlers that are responsible for carrying out all necessary tasks if a speech event is recognized
            _recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(Social_SpeechRecognized);
            _recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(Web_SpeechRecognized);
            _recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(Default_SpeechRecognized);
            _recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(PlayFile_SpeechRecognized);
            _recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(AlarmClock_SpeechRecognized);
            _recognizer.AudioLevelUpdated += new EventHandler<AudioLevelUpdatedEventArgs>(_recognizer_AudioLevelUpdated);
            _recognizer.SpeechDetected += new EventHandler<SpeechDetectedEventArgs>(_recognizer_SpeechDetected);

            startlistening.SetInputToDefaultAudioDevice();
            startlistening.LoadGrammarAsync(new Grammar(new GrammarBuilder(new Choices("Jarvis")))); //Loads a grammar choice into the speech recognition engine
            startlistening.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(startlistening_SpeechRecognized); //The event handler that allows us to say "JARVIS Come Back Online". Only one Speech Recognition Engine is active at a time.

            Jarvis.SpeakProgress += new EventHandler<SpeakProgressEventArgs>(JARVIS_SpeakProgress);
            Jarvis.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(JARVIS_SpeakCompleted);

            if (Settings.Default.AClockEnbl == true) //Enables the clock if it has been set so you don't have to reset it everytime you open and close the program
            { AlarmTimer.Enabled = true; }

            string tmppath = System.IO.Path.GetTempFileName();
            tmppath = tmppath.Replace(".tmp", ".mp4");
            Stream tmp = System.IO.File.Create(tmppath);
            tmp.Write(Resources.Produce, 0, Resources.Produce.Length);
            tmp.Close();
            axWindowsMediaPlayer1.URL = tmppath;
            axWindowsMediaPlayer1.Size = new System.Drawing.Size(447, 282);
            axWindowsMediaPlayer1.Location = new Point(0, 0);
            axWindowsMediaPlayer1.Visible = true;
            axWindowsMediaPlayer1.Ctlcontrols.play();
            if (Settings.Default.User.ToString() == String.Empty) //Checks for user info. If none is available it sets to default
            { Settings.Default.User = userName; Settings.Default.Save(); Jarvis.SpeakAsync("It is nice to make your acquaintance " + Settings.Default.User + ", my name is JARVIS and I will be your personal digital assistant"); }
            else
            {
                RSSReader.GetWeather();
                if (QEvent == "connected")
                { Jarvis.SpeakAsync("It is good to see you again " + Settings.Default.User + ". The time is " + timenow.GetDateTimeFormats('t')[0] + " and the weather in " + Town + " is " + Condition + " at " + Temperature + " degrees. How can I help?"); }
                else if (QEvent == "failed")
                { Jarvis.SpeakAsync("It is good to see you again " + Settings.Default.User + ". It is currently " + timenow.GetDateTimeFormats('t')[0] + ". What is it you would like me to do first?"); }
            }
            QEvent = "FormLoad";
            this.DragDrop +=new DragEventHandler(frmMain_DragDrop);
            this.DragEnter +=new DragEventHandler(frmMain_DragEnter);
        }

        #region Recognizer Settings
        void _recognizer_SpeechDetected(object sender, SpeechDetectedEventArgs e)
        {
            recTimeOut = 0;
        }

        void _recognizer_AudioLevelUpdated(object sender, AudioLevelUpdatedEventArgs e)
        {
            pbMicrophoneLevel.Value = e.AudioLevel;
        }

        private void tmrSpeech_Tick(object sender, EventArgs e)
        {
            if (recTimeOut == 10)
            {
                _recognizer.RecognizeAsyncCancel();
            }
            else if (recTimeOut == 11)
            {
                startlistening.RecognizeAsync(RecognizeMode.Multiple);
                tmrSpeech.Stop();
                recTimeOut = 0;
            }
            recTimeOut += 1;
        }
        #endregion

        #region Speaking Animation
        void JARVIS_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            pictureBox1.Image = Properties.Resources.NotSpeaking;
        }

        string word;
        int SpeakCount;
        void JARVIS_SpeakProgress(object sender, SpeakProgressEventArgs e)
        {
            word = e.Text.ToLower();
            SpeakCount = 0;
            tmrBgPic.Enabled = true;
        }
        private void tmrBgPic_Tick(object sender, EventArgs e)
        {
            if (word[SpeakCount] == 'a' || word[SpeakCount] == 'e' || word[SpeakCount] == 'i' || word[SpeakCount] == 'o' || word[SpeakCount] == 'u')
            {
                pictureBox1.Image = Properties.Resources.Vowel;
            }
            else
            {
                pictureBox1.Image = Properties.Resources.Consonant;
            }
            if (SpeakCount == word.Length - 1)
            { SpeakCount = 0; tmrBgPic.Enabled = false; pictureBox1.Image = Properties.Resources.NotSpeaking; }
            SpeakCount += 1;
        }
        #endregion

        #region Set Language
        protected void btnSetLang_Click(object sender, EventArgs e)
        {
            LoadSpeechRecognizerLanguage();
        }
        void AskForACountry()
        {
            frmEdit = new Form();
            frmEdit.ShowIcon = false;
            cmbxLang = new ComboBox();
            frmEdit.Size = new Size(199, 113);
            Button btnSetLang = new Button();
            btnSetLang.Size = new Size(88, 23);
            btnSetLang.Text = "Set Language";
            btnSetLang.Location = new Point(40, 39);
            cmbxLang.Location = new Point(28, 12);
            cmbxLang.DropDownStyle = ComboBoxStyle.DropDownList;
            String[] arrayCountry = { "Catalan - Spain", "Chinese - China", "Chinese - Hong Kong", "Chinese - Taiwan", "Danish - Denmark", "Dutch - Netherlands", "English - Australia", "English - Canada", "English - Great Britain", "English - US", "Finnish - Finland", "French - Canada", "French - France", "German - Germany", "Italian - Italy", "Japanese - Japan", "Korean - Korea", "Norwegian - Norway", "Polish - Poland", "Portuguese - Brazil", "Portuguese - Portugal", "Russian - Russia", "Spanish - Mexico", "Spanish - Spain", "Swedish - Sweden" };
            cmbxLang.DataSource = arrayCountry.ToList();
            btnSetLang.Click += new EventHandler(btnSetLang_Click);
            frmEdit.Controls.Add(cmbxLang);
            frmEdit.Controls.Add(btnSetLang);
            Jarvis.SpeakAsync("Please select a country that fits your computer's default language and dialect.");
            MessageBox.Show("Please select a country that fits your computer's default language and dialect.", "Select A Country");
            frmEdit.StartPosition = FormStartPosition.CenterScreen;
            frmEdit.AcceptButton = btnSetLang;
            Jarvis.SpeakAsyncCancelAll();
            frmEdit.ShowDialog();
        }
        void LoadSpeechRecognizerLanguage()
        {
            try
            {
                Jarvis.SpeakAsyncCancelAll();
                String[] arrayCountry = { "Catalan - Spain", "Chinese - China", "Chinese - Hong Kong", "Chinese - Taiwan", "Danish - Denmark", "Dutch - Netherlands", "English - Australia", "English - Canada", "English - Great Britain", "English - United States", "Finnish - Finland", "French - Canada", "French - France", "German - Germany", "Italian - Italy", "Japanese - Japan", "Korean - Korea", "Norwegian - Norway", "Polish - Poland", "Portuguese - Brazil", "Portuguese - Portugal", "Russian - Russia", "Spanish - Mexico", "Spanish - Spain", "Swedish - Sweden" };
                String[] arrayLang = { "ca-ES", "zh-CN", "zh-HK", "zh-TW", "da-DK", "nl-NL", "en-AU", "en-CA", "en-GB", "en-US", "fi-FI", "fr-CA", "fr-FR", "de-DE", "it-IT", "ja-JP", "ko-KR", "nb-NO", "pl-PL", "pt-BR", "pt-PT", "ru-RU", "es-MX", "es-ES", "sv-SE" };
                Settings.Default.Language = arrayLang[cmbxLang.SelectedIndex];
                Settings.Default.Save();
                lblLanguage.Text = Settings.Default.Language;
                Thread.CurrentThread.CurrentCulture = new CultureInfo(Settings.Default.Language);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(Settings.Default.Language);
                // = new SpeechRecognitionEngine(new CultureInfo(Settings.Default.Language));
                //startlistening = new SpeechRecognitionEngine(new CultureInfo(Settings.Default.Language));
                Jarvis.SpeakAsync("You have selected " + arrayCountry[cmbxLang.SelectedIndex] + " as your default language. It is recommended that you restart the program any time you change the Speech Recognition Language.");
                MessageBox.Show("You have selected " + Settings.Default.Language + " as your default language. It is recommended that you restart the program any time you change the Speech Recognition Language.", "Thank You");
                lblLanguage.Text = Settings.Default.Language;
                Jarvis.SpeakAsyncCancelAll();
                frmEdit.Dispose();
            }
            catch (Exception ex)
            {
                ErrorLog(ex.ToString());
                Jarvis.SpeakAsyncCancelAll();
                Settings.Default.Language = string.Empty;
                Jarvis.SpeakAsync("It seems the language you have selected does not match your system's language.");
                MessageBox.Show("It seems the language you have selected does not match your system's language.", "Invalid Selection");
                frmEdit.Dispose();
                frmEdit.Visible = false;
                AskForACountry();
            }
        }
        #endregion

        #region Speech recognized
        void Shell_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            tmrSpeech.Start();
            string speech = e.Result.Text; //Sets the SpeechRecognized event variable to a string variable called speech
            i = 0; //Ensures "i" is = to 0 so we can start our loop from the beginning of our arrays
            try
            {
                foreach (string line in ArrayShellCommands)
                {
                    if (line == speech) //If line == speech it will open the corresponding program/file
                    {
                        System.Diagnostics.Process.Start(ArrayShellLocation[i]); //Opens the program/file of the same elemental position as the ArrayShellCommands command that was equal to speech
                        Jarvis.SpeakAsync(ArrayShellResponse[i]); //Gives the response of the same elemental position as the ArrayShellCommands command that was equal to speech
                    }
                    i += 1; //if the line in ArrayShellCommands does not equal speech it will add 1 to "i" and go through the loop until it finds a match between the line and spoken event
                }
            }
            catch
            {
                i += 1;
                Jarvis.SpeakAsync("Im sorry it appears the shell command " + speech + " on line " + i + " is accompanied by either a blank line or an incorrect file location");
            }
        }

        void Social_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string speech = e.Result.Text;
            i = 0;
            try
            {
                foreach (string line in ArraySocialCommands)
                {
                    if (line == speech)
                    {
                        Jarvis.SpeakAsync(ArraySocialResponse[i]);
                    }
                    i += 1;
                }
            }
            catch
            {
                i += 1;
                Jarvis.SpeakAsync("Please check the " + speech + " social command on line " + i + ". It appears to be missing a proper response");
            }
        }

        void Web_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string speech = e.Result.Text;
            i = 0;
            try
            {
                foreach (string line in ArrayWebCommands)
                {
                    if (line == speech)
                    {
                        System.Diagnostics.Process.Start(ArrayWebURL[i]);
                        Jarvis.SpeakAsync(ArrayWebResponse[i]);
                    }
                    i += 1;
                }
            }
            catch
            {
                i += 1;
                Jarvis.SpeakAsync("Please check the " + speech + "web command on line " + i + ". It appears to be missing a proper response or web U R L");
            }
        }

        void PlayFile_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string speech = e.Result.Text;
            switch (speech)
            {
                case "Play music":
                    QEvent = speech; //Sets the QEvent variable to "Play music" so it can differentiate between music and video files
                    Jarvis.SpeakAsync("Which song");
                    break;
                case "Play video":
                    QEvent = speech; //Sets the QEvent variable to "Play video" so it can differentiate between music and video files
                    Jarvis.SpeakAsync("Which video");
                    break;
                case "Show me a video":
                    QEvent = speech;
                    Jarvis.SpeakAsync("Which video");
                    break;
            }

            if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPlaying)
            {
                for (int inNum = 0; inNum <= 100; inNum++)
                {
                    string comparer = "Set the volume at " + inNum + " percent";
                    if (speech == comparer)
                    {
                        axWindowsMediaPlayer1.settings.volume = inNum;
                        lblVolume.Text = inNum + "%";
                    }
                }
            }
            if (QEvent == "Play music" || QEvent == "Play video")
            { PlayMediaFiles(speech); }
        }
        #endregion

        private void PlayMediaFiles(string speech)
        {
            try
            {
                i = 0;
                if (QEvent == "Play music")
                {
                    foreach (string line in MyMusicNames)
                    {
                        if (line == speech && (MyMusicPaths[i].Contains(".mp3") || MyMusicPaths[i].Contains(".m4a") || MyMusicPaths[i].Contains(".wav")))
                        {
                            axWindowsMediaPlayer1.URL = MyMusicPaths[i];
                            axWindowsMediaPlayer1.Ctlcontrols.play();
                            SelectedMusicFile = i;
                            Jarvis.SpeakAsync(line);
                            QEvent = string.Empty;
                        }
                        i += 1;
                    }
                }
                if (QEvent == "Play video")
                {
                    foreach (string line in MyVideoNames)
                    {
                        if (line == speech && (MyVideoPaths[i].Contains(".mp4") || MyVideoPaths[i].Contains(".avi") || MyVideoPaths[i].Contains(".m4v")))
                        {
                            System.Diagnostics.Process.Start(MyVideoPaths[i]);
                            Jarvis.SpeakAsync(line);
                            QEvent = string.Empty;
                        }
                        i += 1;
                    }
                }
            }
            catch (Exception ex) { ErrorLog(ex.ToString()); Jarvis.SpeakAsync("File names unsuccessfully loaded"); }
        }

        #region Shutdown/Restart/Logoff
        private void ShutdownTimer_Tick(object sender, EventArgs e)
        {
            if (timer == 0)
            {
                lblTimer.Visible = false;
                ComputerTermination();
                ShutdownTimer.Enabled = false;
            }
            else
            {
                timer = timer - 1;
                lblTimer.Text = timer.ToString();
            }
        }
        private void ComputerTermination()
        {
            switch (QEvent)
            {
                case "shutdown":
                    System.Diagnostics.Process.Start("shutdown", "-s");
                    break;
                case "logoff":
                    System.Diagnostics.Process.Start("shutdown", "-l");
                    break;
                case "restart":
                    System.Diagnostics.Process.Start("shutdown", "-r");
                    break;
            }
        }
        #endregion

        private void lblAdd_Click(object sender, EventArgs e)
        {
            Customize customwindow = new Customize();
            customwindow.ShowDialog();
        }

        private void lstCommands_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstCommands.SelectionMode == SelectionMode.One)
            {
                if (QEvent == "Play music file")
                {
                    int num = lstCommands.SelectedIndex;
                    try
                    {
                        System.Diagnostics.Process.Start(MyMusicPaths[num]);
                    }
                    catch { Jarvis.SpeakAsync("The selected index is out of bounds"); }
                }
                else if (QEvent == "Play video file")
                {
                    int num = lstCommands.SelectedIndex;
                    try { System.Diagnostics.Process.Start(MyVideoPaths[num]); }
                    catch { Jarvis.SpeakAsync("The selected index is out of bounds"); }
                }
                else if (QEvent == "Checkfornewemails")
                {
                    int num = lstCommands.SelectedIndex;
                    try { Process.Start(MsgLink[num]); }
                    catch { Jarvis.SpeakAsync("The selected index is out of bounds"); }
                }
            }
        }

        #region Read/Write Media Files
        void ReadDirectories()
        {
            //The first time the program starts it will attempt to load music and video files from your My Videos and My Music directories
            try
            {
                if (Settings.Default.MusicFolder == String.Empty)
                {
                    Settings.Default.MusicFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                    Settings.Default.VideoFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
                    Settings.Default.Save();
                }
                if (!File.Exists(@"C:\Users\" + userName + "\\Documents\\Jarvis Custom Commands\\Filenames.txt") || QEvent == "ReadDirectories")
                {
                    Jarvis.SpeakAsync("I need a few seconds to load audio and video files");
                    sw = File.CreateText(@"C:\Users\" + Environment.UserName + "\\Documents\\Jarvis Custom Commands\\Filenames.txt");
                    MyMusic = Directory.GetFiles(Settings.Default.MusicFolder, "*.mp3", SearchOption.AllDirectories);
                    MyVideos = Directory.GetFiles(Settings.Default.VideoFolder, "*.mp4", SearchOption.AllDirectories);
                    WriteDirectories();
                    MyMusic = Directory.GetFiles(Settings.Default.MusicFolder, "*.m4a", SearchOption.AllDirectories);
                    MyVideos = Directory.GetFiles(Settings.Default.VideoFolder, "*.avi", SearchOption.AllDirectories);
                    WriteDirectories();
                    MyMusic = Directory.GetFiles(Settings.Default.MusicFolder, "*.wav", SearchOption.AllDirectories);
                    MyVideos = Directory.GetFiles(Settings.Default.VideoFolder, "*.mkv", SearchOption.AllDirectories);
                    WriteDirectories();
                    Array.Clear(MyMusic, 0, MyMusic.Count());
                    Array.Clear(MyVideos, 0, MyVideos.Count());
                    sw.Close();
                    Jarvis.SpeakAsync("Libraries are updated");
                }
                String[] AllLines = File.ReadAllLines(@"C:\Users\" + userName + "\\Documents\\Jarvis Custom Commands\\Filenames.txt");
                List<string> mn = new List<string>(); List<string> mp = new List<string>();
                List<string> vn = new List<string>(); List<string> vp = new List<string>();
                i = 0;
                foreach (string line in AllLines)
                {
                    string[] separator = new string[] { "$%$%$" };
                    string fullline = AllLines[i];
                    string[] data = fullline.Split(separator, StringSplitOptions.None);
                    if (data[1].Contains(".mp3") || data[1].Contains(".m4a") || data[1].Contains(".wav"))
                    {
                        mn.Add(data[0]);
                        mp.Add(data[1]);
                    }
                    else if (data[1].Contains(".mp4") || data[1].Contains(".avi") || data[1].Contains(".mkv"))
                    {
                        vn.Add(data[0]);
                        vp.Add(data[1]);
                    }
                    i += 1;
                }
                MyMusicNames = mn.ToArray(); MyMusicPaths = mp.ToArray();
                MyVideoNames = vn.ToArray(); MyVideoPaths = vp.ToArray();
                try
                {
                    MusicGrammar = new Grammar(new GrammarBuilder(new Choices(MyMusicNames))); _recognizer.LoadGrammarAsync(MusicGrammar);
                    VideoGrammar = new Grammar(new GrammarBuilder(new Choices(MyVideoNames))); _recognizer.LoadGrammarAsync(VideoGrammar);
                }
                catch (Exception ex) { ErrorLog(ex.ToString()); Jarvis.SpeakAsync("Unable to load files from Music and Video Directories"); }
            }
            catch (Exception ex)
            {
                ErrorLog(ex.ToString());
                Jarvis.SpeakAsync("You do not have permission to access this folder. I will default your libraries back to My Music and My Videos");
                Settings.Default.MusicFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                Settings.Default.VideoFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
                Settings.Default.Save();
                sw.Close();
                File.Delete(@"C:\Users\" + Environment.UserName + "\\Documents\\Jarvis Custom Commands\\Filenames.txt");
            }
        }

        void WriteDirectories()
        {
            int filecount = 0, countmark = 0;
            foreach (string file in MyMusic)
            {
                string filename = Path.GetFileNameWithoutExtension(file);
                music = TagLib.File.Create(file);
                string Artist = music.Tag.FirstPerformer;
                string Song = music.Tag.Title;
                string adjustedstring = filename;
                string[] badcharacters = { "-", "(", ")", "[", "]", "{", "}", "@", ".", ",", ";", "'", "^", "%", "$", "!", "~", "`" };

                foreach (string bchar in badcharacters)
                {
                    if (filename.Contains(bchar))
                    {
                        filename = filename.Replace(bchar, "");
                        filename = filename.Replace("  ", " ");
                    }
                    try
                    {
                        if (Song.Contains(bchar) || Artist.Contains(bchar))
                        {
                            Song = Song.Replace(bchar, "");
                            Song = Song.Replace("  ", " ");
                        }
                        else if (Artist.Contains(bchar))
                        {
                            Artist = Artist.Replace(bchar, "");
                            Artist = Artist.Replace("  ", " ");
                        }
                    }
                    catch (Exception ex) { ErrorReport(ex.ToString()); }
                }
                try
                {
                    if (Song == null)
                    {
                        sw.WriteLine(adjustedstring.Replace("  ", " ") + "$%$%$" + file);
                    }
                    else if (Artist == null & Song.Length > 0)
                    {
                        sw.WriteLine(Song.Replace("  ", " ") + "$%$%$" + file);
                    }
                    else if (Artist.Length > 0 & Song.Length > 0)
                    {
                        string grammarline = Song.Replace("  ", " ") + " by " + Artist.Replace("  ", " ");
                        sw.WriteLine(grammarline.Replace("  ", " ") + "$%$%$" + file);
                    }
                    filecount += 1; countmark += 1;
                    if (countmark == 600 & filecount <= 600)
                    { Jarvis.SpeakAsync(filecount + " music files have been loaded so far, please be patient"); countmark = 0; }
                    else if (countmark == 600 & filecount > 600)
                    { Jarvis.SpeakAsync(filecount + " files"); countmark = 0; }
                }
                catch (Exception ex) { ErrorReport(ex.ToString()); }
            }
            foreach (string file in MyVideos)
            {
                string videoname = Path.GetFileNameWithoutExtension(file);
                string adjustedstring = videoname;
                string[] badcharacters = { "-", "(", ")", "[", "]", "{", "}", "@", ".", ",", ";", "'", "^", "%", "$", "!", "~", "`" };

                foreach (string bchar in badcharacters)
                {
                    if (videoname.Contains(bchar))
                    { adjustedstring = adjustedstring.Replace(bchar, ""); }
                }
                try
                { sw.WriteLine(adjustedstring.Replace("  ", " ") + "$%$%$" + file); }
                catch (Exception ex) { ErrorReport(ex.ToString()); }
            }
            if (filecount >= 600)
            { Jarvis.SpeakAsync(filecount + " mp3 files were loaded. That's quite a number"); }
        }
        #endregion

        private void lblLanguage_Click(object sender, EventArgs e)
        {
            AskForACountry();
        }

        private void tmrMailCheck_Tick(object sender, EventArgs e)
        {
            if (Settings.Default.GmailUser != String.Empty && Settings.Default.GmailPassword != String.Empty)
            {
                EmailNum = 0;
                RSSReader.CheckForEmails();
            }
        }

        private void tmrMusic_Tick(object sender, EventArgs e)
        {
            int currpos = (int)axWindowsMediaPlayer1.Ctlcontrols.currentPosition;
            int duration = (int)axWindowsMediaPlayer1.currentMedia.duration;
            tbarMusicTime.Maximum = (int)duration;
            if (currpos < duration - 1)
            {
                lblMusicTime.Text = axWindowsMediaPlayer1.Ctlcontrols.currentPositionString;
                tbarMusicTime.Value = (int)currpos;
            }
            else if (currpos == duration - 1)
            {
                if (Settings.Default.Shuffle == true)
                {
                    int arraylength = MyMusicPaths.Count();
                    int Ran = rnd.Next(0, arraylength);
                    SelectedMusicFile = Ran;
                }
                else if (Settings.Default.Shuffle == false)
                {
                    if (SelectedMusicFile == MyMusicPaths.Count() - 1)
                    {
                        SelectedMusicFile = 0;
                    }
                    else
                    {
                        SelectedMusicFile += 1;
                    }
                }
                axWindowsMediaPlayer1.URL = MyMusicPaths[SelectedMusicFile];
                axWindowsMediaPlayer1.Ctlcontrols.play();
            }
        }

        private void axWindowsMediaPlayer1_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            WMPLib.WMPPlayState pstate = axWindowsMediaPlayer1.playState;

            if (pstate == WMPLib.WMPPlayState.wmppsStopped || pstate == WMPLib.WMPPlayState.wmppsMediaEnded)
            {
                if (QEvent == "FormLoad")
                {
                    _recognizer.RecognizeAsync(RecognizeMode.Multiple); //This allows the Speech recognition engine to listen for multiple phrases and continue working rather than just one and quiting
                    axWindowsMediaPlayer1.Visible = false; tmrMusic.Enabled = false; QEvent = String.Empty;
                    axWindowsMediaPlayer1.fullScreen = false;
                }
                Text = "J.A.R.V.I.S. (V.1.1.0)";
            }
            else if (pstate == WMPLib.WMPPlayState.wmppsPlaying)
            {
                string filesourceURL = axWindowsMediaPlayer1.currentMedia.sourceURL;
                if (filesourceURL.Contains(".mp3") || filesourceURL.Contains(".wav") || filesourceURL.Contains(".m4a"))
                {
                    tmrMusic.Start(); 
                    lblMusicTime.Visible = true; 
                    lblVolume.Visible = true;
                    axWindowsMediaPlayer1.Size = new System.Drawing.Size(138, 23);
                    axWindowsMediaPlayer1.Location = new Point(164, 126);
                    axWindowsMediaPlayer1.Visible = true;
                    tbarMusicTime.Maximum = (int)axWindowsMediaPlayer1.Ctlcontrols.currentItem.duration;
                    tbarMusicTime.Visible = true; 
                    tbarVolume.Value = axWindowsMediaPlayer1.settings.volume;
                    Text = "J.A.R.V.I.S. (V.1.1.0) - " + MyMusicNames[SelectedMusicFile];
                }
                else if (QEvent != "FormLoad" && filesourceURL.Contains(".mp4") || filesourceURL.Contains(".avi"))
                {
                    axWindowsMediaPlayer1.fullScreen = true;
                }
            }
        }

        private void tbarMusicTime_Scroll(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.Ctlcontrols.currentPosition = tbarMusicTime.Value;
        }

        private void tbarVolume_Scroll(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.settings.volume = tbarVolume.Value;
            lblVolume.Text = tbarVolume.Value.ToString() + "%";
        }
        private void lblVolume_Click(object sender, EventArgs e)
        {
            if (tbarVolume.Visible == false)
            { tbarVolume.Visible = true; }
            else if (tbarVolume.Visible == true)
            { tbarVolume.Visible = false; }
        }

        private void frmMain_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                string dropName = (string)((DataObject)e.Data).GetFileDropList()[0];
                axWindowsMediaPlayer1.URL = dropName;
                axWindowsMediaPlayer1.Ctlcontrols.play();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void frmMain_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }


        public void ErrorLog(string error)
        {
            MessageBox.Show(error);
            string DTL = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            using (System.IO.StreamWriter sw = System.IO.File.AppendText(DTL + "\\Error Report.txt"))
            {
                sw.WriteLine("~~~" + DateTime.Now + "~~~");
                sw.WriteLine(error.ToString());
                sw.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~");
                sw.WriteLine("");
                sw.Close();

            }
        }
        public void ErrorReport(string error)
        {
            string DTL = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            using (System.IO.StreamWriter sw = System.IO.File.AppendText(DTL + "\\Error Report.txt"))
            {
                sw.WriteLine("~~~" + DateTime.Now + "~~~");
                sw.WriteLine(error.ToString());
                sw.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~");
                sw.WriteLine("");
                sw.Close();

            }
        }
    }
}

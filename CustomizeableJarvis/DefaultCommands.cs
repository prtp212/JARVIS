using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using CustomizeableJarvis.Properties;
using System.Globalization;

namespace CustomizeableJarvis
{
    public partial class frmMain : Form
    {
        Random rnd = new Random();
        public static List<string> MsgList = new List<string>();
        public static List<string> MsgLink = new List<string>();
        int count = 1;
        int timer = 11;
        int EmailNum = 0;
        DateTime timenow = DateTime.Now;
        public static String Temperature, Condition, Humidity, WinSpeed, TFCond, TFHigh, TFLow, Town;

        void Default_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            int ranNum;
            string speech = e.Result.Text;
            switch (speech)
            {
                #region Greetings
                case "Hello":
                case "Hello Jarvis":
                    timenow = DateTime.Now;
                    if (timenow.Hour >= 5 && timenow.Hour < 12)
                    { Jarvis.SpeakAsync("Goodmorning " + Settings.Default.User); }
                    if (timenow.Hour >= 12 && timenow.Hour < 18)
                    { Jarvis.SpeakAsync("Good afternoon " + Settings.Default.User); }
                    if (timenow.Hour >= 18 && timenow.Hour < 24)
                    { Jarvis.SpeakAsync("Good evening " + Settings.Default.User); }
                    if (timenow.Hour < 5)
                    { Jarvis.SpeakAsync("Hello " + Settings.Default.User + ", it's getting late"); }
                    break;

                case "Goodbye":
                case "Goodbye Jarvis":
                case "Close Jarvis":
                    Jarvis.Speak("Farewell");
                    Close();
                    break;

                case "Jarvis":
                    ranNum = rnd.Next(1, 5);
                    if (ranNum == 1) { QEvent = ""; Jarvis.SpeakAsync("Yes sir"); }
                    else if (ranNum == 2) { QEvent = ""; Jarvis.SpeakAsync("Yes?"); }
                    else if (ranNum == 3) { QEvent = ""; Jarvis.SpeakAsync("How may I help?"); }
                    else if (ranNum == 4) { QEvent = ""; Jarvis.SpeakAsync("How may I be of assistance?"); }
                    break;

                case "What's my name?":
                    Jarvis.SpeakAsync(Settings.Default.User);
                    break;

                case "Stop talking":
                    Jarvis.SpeakAsyncCancelAll();
                    ranNum = rnd.Next(1, 5);
                    if (ranNum == 5)
                    { Jarvis.Speak("fine"); }
                    break;
                #endregion

                #region Condition of the Day
                case "What time is it":
                    timenow = DateTime.Now;
                    string time = timenow.GetDateTimeFormats('t')[0];
                    Jarvis.SpeakAsync(time);
                    break;

                case "What day is it":
                    Jarvis.SpeakAsync(DateTime.Today.ToString("dddd"));
                    break;

                case "Whats the date":
                case "Whats todays date":
                    Jarvis.SpeakAsync(DateTime.Today.ToString("dd-MM-yyyy"));
                    break;

                case "Hows the weather":
                case "Whats the weather like":
                case "Whats it like outside":
                    RSSReader.GetWeather();
                    if (QEvent == "connected")
                    { Jarvis.SpeakAsync("The weather in " + Town + " is " + Condition + " at " + Temperature + " degrees. There is a humidity of " + Humidity + " and a windspeed of " + WinSpeed + " miles per hour"); }
                    else if (QEvent == "failed")
                    { Jarvis.SpeakAsync("I seem to be having a bit of trouble connecting to the server. Just look out the window"); }
                    break;

                case "What will tomorrow be like":
                case "Whats tomorrows forecast":
                case "Whats tomorrow like":
                    RSSReader.GetWeather();
                    if (QEvent == "connected")
                    { Jarvis.SpeakAsync("Tomorrows forecast is " + TFCond + " with a high of " + TFHigh + " and a low of " + TFLow); }
                    else if (QEvent == "failed")
                    { Jarvis.SpeakAsync("I could not access the server, are you sure you have the right W O E I D?"); }
                    break;

                case "Whats the temperature":
                case "Whats the temperature outside":
                    RSSReader.GetWeather();
                    if (QEvent == "connected")
                    { Jarvis.SpeakAsync(Temperature + " degrees"); }
                    else if (QEvent == "failed")
                    { Jarvis.SpeakAsync("I could not connect to the weather service"); }
                    break;
                #endregion

                #region Application Commands
                case "Switch Window":
                    SendKeys.SendWait("%{TAB " + count + "}");
                    count += 1;
                    break;

                case "Close window":
                    SendKeys.SendWait("%{F4}");
                    break;

                case "Out of the way":
                    if (WindowState == FormWindowState.Normal)
                    {
                        WindowState = FormWindowState.Minimized;
                        Jarvis.SpeakAsync("My apologies");
                    }
                    break;

                case "Come back":
                    if (WindowState == FormWindowState.Minimized)
                    {
                        Jarvis.SpeakAsync("Alright");
                        WindowState = FormWindowState.Normal;
                    }
                    break;

                case "Show default commands":
                    string[] defaultcommands = (File.ReadAllLines(@"Default Commands.txt"));
                    Jarvis.SpeakAsync("Very well");
                    lstCommands.Items.Clear();
                    lstCommands.SelectionMode = SelectionMode.None;
                    lstCommands.Visible = true;
                    foreach (string command in defaultcommands)
                    {
                        lstCommands.Items.Add(command);
                    }
                    break;

                case "Show shell commands":
                    Jarvis.SpeakAsync("Here we are");
                    lstCommands.Items.Clear();
                    lstCommands.SelectionMode = SelectionMode.None;
                    lstCommands.Visible = true;
                    foreach (string command in ArrayShellCommands)
                    {
                        lstCommands.Items.Add(command);
                    }
                    break;

                case "Show social commands":
                    Jarvis.SpeakAsync("Alright");
                    lstCommands.Items.Clear();
                    lstCommands.SelectionMode = SelectionMode.None;
                    lstCommands.Visible = true;
                    foreach (string command in ArraySocialCommands)
                    {
                        lstCommands.Items.Add(command);
                    }
                    break;

                case "Show web commands":
                    Jarvis.SpeakAsync("Ok");
                    lstCommands.Items.Clear();
                    lstCommands.SelectionMode = SelectionMode.None;
                    lstCommands.Visible = true;
                    foreach (string command in ArrayWebCommands)
                    {
                        lstCommands.Items.Add(command);
                    }
                    break;
                case "Show Music Library":
                    lstCommands.SelectionMode = SelectionMode.One;
                    lstCommands.Items.Clear();
                    lstCommands.Visible = true;
                    Jarvis.SpeakAsync("OK");
                    i = 0;
                    foreach (string file in MyMusicPaths)
                    {
                        lstCommands.Items.Add(MyMusicNames[i]);
                        i += 1;
                    }
                    QEvent = "Play music file";
                    break;

                case "Show Video Library":
                    lstCommands.SelectionMode = SelectionMode.One;
                    lstCommands.Items.Clear();
                    lstCommands.Visible = true;
                    i = 0;
                    foreach (string file in MyVideoPaths)
                    {
                        if (file.Contains(".mp4") || file.Contains(".avi") || file.Contains(".mkv"))
                        { lstCommands.Items.Add(MyVideoNames[i]); i += 1; }
                        else { i += 1; }
                    }
                    QEvent = "Play video file";
                    break;

                case "Show Email List":
                    lstCommands.SelectionMode = SelectionMode.One;
                    lstCommands.Items.Clear();
                    lstCommands.Visible = true;
                    foreach (string line in MsgList)
                    {
                        lstCommands.Items.Add(line);
                    }
                    QEvent = "Checkfornewemails";
                    break;

                case "Show listbox":
                    lstCommands.Visible = true;
                    break;

                case "Hide listbox":
                    lstCommands.Visible = false;
                    break;
                #endregion

                #region Shutdown / Restart / Logoff
                case "Shutdown":
                    if (ShutdownTimer.Enabled == false)
                    {
                        QEvent = "shutdown";
                        Jarvis.SpeakAsync("Are you sure you want to " + QEvent + "?");
                    }
                    break;

                case "Log off":
                    if (ShutdownTimer.Enabled == false)
                    {
                        QEvent = "logoff";
                        Jarvis.SpeakAsync("Are you sure you want to " + QEvent + "?");
                    }
                    break;

                case "Restart":
                    if (ShutdownTimer.Enabled == false)
                    {
                        QEvent = "restart";
                        Jarvis.SpeakAsync("Are you sure you want to " + QEvent + "?");
                    }
                    break;

                case "Abort":
                    if (ShutdownTimer.Enabled == true)
                    {
                        timer = 11;
                        lblTimer.Text = timer.ToString();
                        ShutdownTimer.Enabled = false;
                        lblTimer.Visible = false;
                    }
                    break;
                #endregion

                #region Media Control Commands
                case "Play":
                    axWindowsMediaPlayer1.Ctlcontrols.play();
                    axWindowsMediaPlayer1.Visible = true;
                    break;
                case "Play a random song":
                    int Ran = rnd.Next(0, MyMusicPaths.Count());
                    SelectedMusicFile = Ran;
                    Jarvis.SpeakAsync("I hope you're in the mood for " + MyMusicNames[SelectedMusicFile]);
                    axWindowsMediaPlayer1.URL = MyMusicPaths[SelectedMusicFile];
                    break;
                case "You decide":
                    if (QEvent == "Play music")
                    {
                        Ran = rnd.Next(0, MyMusicPaths.Count());
                        SelectedMusicFile = Ran;
                        Jarvis.SpeakAsync("How about " + MyMusicNames[SelectedMusicFile] + "?");
                        axWindowsMediaPlayer1.URL = MyMusicPaths[SelectedMusicFile];
                    }
                    break;
                case "Pause":
                    tmrMusic.Stop();
                    axWindowsMediaPlayer1.Ctlcontrols.pause();
                    break;
                case "Turn Shuffle On":
                    Settings.Default.Shuffle = true;
                    Settings.Default.Save();
                    Jarvis.SpeakAsync("Shuffle enabled");
                    break;
                case "Turn Shuffle Off":
                    Settings.Default.Shuffle = false;
                    Settings.Default.Save();
                    Jarvis.SpeakAsync("Shuffle disabled");
                    break;
                case "Turn Up":
                    axWindowsMediaPlayer1.settings.volume += 10;
                    lblVolume.Text = axWindowsMediaPlayer1.settings.volume.ToString() + "%";
                    tbarVolume.Value = axWindowsMediaPlayer1.settings.volume;
                    break;
                case "Turn Down":
                    axWindowsMediaPlayer1.settings.volume -= 10;
                    lblVolume.Text = axWindowsMediaPlayer1.settings.volume.ToString() + "%";
                    tbarVolume.Value = axWindowsMediaPlayer1.settings.volume;
                    break;
                case "Mute":
                    axWindowsMediaPlayer1.settings.mute = true;
                    lblVolume.Text = "mute";
                    break;
                case "Unmute":
                    axWindowsMediaPlayer1.settings.mute = false;
                    lblVolume.Text = axWindowsMediaPlayer1.settings.volume.ToString() + "%";
                    break;
                case "Next Song":
                    if (SelectedMusicFile != MyMusicPaths.Count() - 1)
                    {
                        if (Settings.Default.Shuffle == true)
                        {
                            Ran = rnd.Next(0, MyMusicPaths.Count());
                            SelectedMusicFile = Ran;
                        }
                        else if (Settings.Default.Shuffle == false)
                        { SelectedMusicFile += 1; }
                        axWindowsMediaPlayer1.URL = MyMusicPaths[SelectedMusicFile];
                    }
                    break;
                case "Previous Song":
                    if (SelectedMusicFile != 0)
                    {
                        SelectedMusicFile -= 1;
                        axWindowsMediaPlayer1.URL = MyMusicPaths[SelectedMusicFile];
                    }
                    break;
                case "Fast Forward":
                    axWindowsMediaPlayer1.Ctlcontrols.fastForward();
                    break;
                case "Stop Music":
                    tmrMusic.Stop();
                    axWindowsMediaPlayer1.URL = String.Empty;
                    axWindowsMediaPlayer1.Ctlcontrols.stop();
                    lblMusicTime.Visible = false; 
                    lblVolume.Visible = false;
                    axWindowsMediaPlayer1.Visible = false;
                    tbarVolume.Visible = false; 
                    tbarMusicTime.Visible = false;
                    axWindowsMediaPlayer1.fullScreen = false;
                    break;
                case "Fullscreen":
                    try
                    {
                        axWindowsMediaPlayer1.fullScreen = true;
                    }
                    catch { }
                    break;
                case "Exit Fullscreen":
                    axWindowsMediaPlayer1.fullScreen = false;
                    break;
                case "What song is playing":
                    string filesourceURL = axWindowsMediaPlayer1.currentMedia.sourceURL;
                    if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPlaying)
                    { Jarvis.SpeakAsync(MyMusicNames[SelectedMusicFile]); }
                    else
                    { Jarvis.SpeakAsync("No song is currently being played"); }
                    break;
                #endregion

                #region Other Commands
                case "I want to add custom commands":
                case "I want to add a custom command":
                case "I want to add a command":
                    Customize customwindow = new Customize();
                    customwindow.ShowDialog();
                    break;

                case "Update commands":
                    Jarvis.SpeakAsync("This may take a few seconds");
                    _recognizer.UnloadGrammar(shellcommandgrammar);
                    _recognizer.UnloadGrammar(webcommandgrammar);
                    _recognizer.UnloadGrammar(socialcommandgrammar);
                    ArrayShellCommands = File.ReadAllLines(scpath);
                    ArrayShellResponse = File.ReadAllLines(srpath);
                    ArrayShellLocation = File.ReadAllLines(slpath);
                    ArrayWebCommands = File.ReadAllLines(webcpath);
                    ArrayWebResponse = File.ReadAllLines(webrpath);
                    ArrayWebURL = File.ReadAllLines(weblpath);
                    ArraySocialCommands = File.ReadAllLines(socpath);
                    ArraySocialResponse = File.ReadAllLines(sorpath);
                    try
                    { shellcommandgrammar = new Grammar(new GrammarBuilder(new Choices(ArrayShellCommands))); _recognizer.LoadGrammar(shellcommandgrammar); }
                    catch
                    { Jarvis.SpeakAsync("I've detected an in valid entry in your shell commands, possibly a blank line. Shell commands will cease to work until it is fixed."); }
                    try
                    { webcommandgrammar = new Grammar(new GrammarBuilder(new Choices(ArrayWebCommands))); _recognizer.LoadGrammar(webcommandgrammar); }
                    catch
                    { Jarvis.SpeakAsync("I've detected an in valid entry in your web commands, possibly a blank line. Web commands will cease to work until it is fixed."); }
                    try
                    { socialcommandgrammar = new Grammar(new GrammarBuilder(new Choices(ArraySocialCommands))); _recognizer.LoadGrammar(socialcommandgrammar); }
                    catch
                    { Jarvis.SpeakAsync("I've detected an in valid entry in your social commands, possibly a blank line. Social commands will cease to work until it is fixed."); }
                    Jarvis.SpeakAsync("All commands updated");
                    break;

                case "Refresh libraries":
                    Jarvis.SpeakAsync("Loading libraries");
                    try { _recognizer.UnloadGrammar(MusicGrammar); _recognizer.UnloadGrammar(VideoGrammar); }
                    catch { Jarvis.SpeakAsync("Previous grammar was invalid"); }
                    File.Delete(@"C:\Users\" + Environment.UserName + "\\Documents\\Jarvis Custom Commands\\Filenames.txt");
                    QEvent = "ReadDirectories";
                    ReadDirectories();
                    break;
                case "Change video directory":
                    Jarvis.SpeakAsync("Please choose a directory to load your video files");
                    VideoFBD.SelectedPath = Settings.Default.VideoFolder;
                    VideoFBD.Description = "Please select your video directory";
                    DialogResult videoresult = VideoFBD.ShowDialog();
                    if (videoresult == DialogResult.OK)
                    {
                        Settings.Default.VideoFolder = VideoFBD.SelectedPath; Settings.Default.Save();
                        QEvent = "ReadDirectories";
                        ReadDirectories();
                    }
                    break;
                case "Change music directory":
                    Jarvis.SpeakAsync("Please choose a directory to load your music files");
                    MusicFBD.SelectedPath = Settings.Default.MusicFolder;
                    MusicFBD.Description = "Please select your music directory";
                    DialogResult musicresult = MusicFBD.ShowDialog();
                    if (musicresult == DialogResult.OK)
                    {
                        Settings.Default.MusicFolder = MusicFBD.SelectedPath; Settings.Default.Save();
                        QEvent = "ReadDirectories";
                        ReadDirectories();
                    }
                    break;

                case "Stop listening":
                    Jarvis.SpeakAsync("I will await further commands");
                    _recognizer.RecognizeAsyncCancel();
                    startlistening.RecognizeAsync(RecognizeMode.Multiple);
                    break;
                #endregion

                #region Gmail Notification
                case "Check for new emails":
                    QEvent = "Checkfornewemails";
                    Jarvis.SpeakAsyncCancelAll();
                    EmailNum = 0;
                    RSSReader.CheckForEmails();
                    break;
                case "Open the email":
                    try
                    {
                        Jarvis.SpeakAsyncCancelAll();
                        Jarvis.SpeakAsync("Very well");
                        System.Diagnostics.Process.Start(MsgLink[EmailNum]);
                    }
                    catch { Jarvis.SpeakAsync("There are no emails to read"); }
                    break;
                case "Read the email":
                    Jarvis.SpeakAsyncCancelAll();
                    try
                    {
                        Jarvis.SpeakAsync(MsgList[EmailNum]);
                    }
                    catch { Jarvis.SpeakAsync("There are no emails to read"); }
                    break;
                case "Next email":
                    Jarvis.SpeakAsyncCancelAll();
                    try
                    {
                        EmailNum += 1;
                        Jarvis.SpeakAsync(MsgList[EmailNum]);
                    }
                    catch { EmailNum -= 1; Jarvis.SpeakAsync("There are no further emails"); }
                    break;
                case "Previous email":
                    Jarvis.SpeakAsyncCancelAll();
                    try
                    {
                        EmailNum -= 1;
                        Jarvis.SpeakAsync(MsgList[EmailNum]);
                    }
                    catch { EmailNum += 1; Jarvis.SpeakAsync("There are no previous emails"); }
                    break;
                case "Clear email list":
                    Jarvis.SpeakAsyncCancelAll();
                    MsgList.Clear(); MsgLink.Clear(); lstCommands.Items.Clear(); EmailNum = 0; Jarvis.SpeakAsync("Email list has been cleared");
                    break;
                #endregion

                #region Updating
                case "Change Language":
                    AskForACountry();
                    break;
                case "Check for new updates":
                    Jarvis.SpeakAsync("Let me see if Michael has posted anything");
                    RSSReader.CheckBloggerForUpdates();
                    break;
                case "Yes":
                    if (QEvent == "UpdateYesNo")
                    {
                        Jarvis.SpeakAsync("Thank you. I shall initialize the download immediately. Simply uninstall me and then install the new me. Would you like me to open the blog for specific information on the update?");
                        System.Diagnostics.Process.Start(Settings.Default.RecentUpdate);
                        QEvent = "OpenBlog";
                    }
                    else if (QEvent == "OpenBlog")
                    {
                        Jarvis.SpeakAsync("Very well, consider it done");
                        System.Diagnostics.Process.Start("http://michaelcjarvis.blogspot.com/2013/09/michael-cs-customizable-jarvis.html");
                        QEvent = String.Empty;
                    }
                    else if (QEvent == "shutdown" || QEvent == "logoff" || QEvent == "restart")
                    {
                        Jarvis.SpeakAsync("I will begin the countdown to " + QEvent);
                        ShutdownTimer.Enabled = true;
                        lblTimer.Visible = true;
                    }
                    break;
                case "No":
                    if (QEvent == "UpdateYesNo")
                    {
                        Jarvis.SpeakAsync("Very well. I guess I don't need any improvement");
                        Settings.Default.RecentUpdate = String.Empty; Settings.Default.Save();
                        QEvent = String.Empty;
                    }
                    else if (QEvent == "OpenBlog")
                    {
                        Jarvis.SpeakAsync("Learn by doing I suppose");
                        QEvent = String.Empty;
                    }
                    else if (QEvent == "shutdown" || QEvent == "logoff" || QEvent == "restart")
                    {
                        Jarvis.SpeakAsync("My mistake");
                        QEvent = String.Empty;
                    }
                    break;
                #endregion
            }
        }
        void startlistening_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string speech = e.Result.Text;
            switch (speech)
            {
                case "Jarvis":
                    startlistening.RecognizeAsyncCancel();
                    Jarvis.SpeakAsync("Yes?");
                    _recognizer.RecognizeAsync(RecognizeMode.Multiple);
                    break;
            }
        }
    }
}
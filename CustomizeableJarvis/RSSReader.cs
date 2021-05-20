﻿using System;
using System.Linq;
using System.Text;
using CustomizeableJarvis.Properties;
using System.Xml;
using System.Xml.Linq;
using System.Net;

namespace CustomizeableJarvis
{
    class RSSReader
    {
        public static void CheckForEmails()
        {
            string GmailAtomUrl = "https://mail.google.com/mail/feed/atom";

            XmlUrlResolver xmlResolver = new XmlUrlResolver();
            xmlResolver.Credentials = new NetworkCredential(Settings.Default.GmailUser, Settings.Default.GmailPassword);
            XmlTextReader xmlReader = new XmlTextReader(GmailAtomUrl);
            xmlReader.XmlResolver = xmlResolver;
            try
            {
                XNamespace ns = XNamespace.Get("http://purl.org/atom/ns#");
                XDocument xmlFeed = XDocument.Load(xmlReader);


                var emailItems = from item in xmlFeed.Descendants(ns + "entry")
                                 select new
                                 {
                                     Author = item.Element(ns + "author").Element(ns + "name").Value,
                                     Title = item.Element(ns + "title").Value,
                                     Link = item.Element(ns + "link").Attribute("href").Value,
                                     Summary = item.Element(ns + "summary").Value
                                 };
                frmMain.MsgList.Clear(); frmMain.MsgLink.Clear();
                foreach (var item in emailItems)
                {
                    if (item.Title == String.Empty)
                    {
                        frmMain.MsgList.Add("Message from " + item.Author + ", There is no subject and the summary reads, " + item.Summary);
                        frmMain.MsgLink.Add(item.Link);
                    }
                    else
                    {
                        frmMain.MsgList.Add("Message from " + item.Author + ", The subject is " + item.Title + " and the summary reads, " + item.Summary);
                        frmMain.MsgLink.Add(item.Link);
                    }
                }

                if (emailItems.Count() > 0)
                {
                    if (emailItems.Count() == 1)
                    {
                        frmMain.Jarvis.SpeakAsync("You have 1 new email");
                    }
                    else { frmMain.Jarvis.SpeakAsync("You have " + emailItems.Count() + " new emails"); }
                }
                else if (frmMain.QEvent == "Checkfornewemails" && emailItems.Count() == 0)
                { frmMain.Jarvis.SpeakAsync("You have no new emails"); frmMain.QEvent = String.Empty; }
            }
            catch { frmMain.Jarvis.SpeakAsync("You have submitted invalid log in information"); }
        }

        public static void GetWeather()
        {
            try
            {
                string query = String.Format("http://weather.yahooapis.com/forecastrss?w=" + Settings.Default.WOEID.ToString() + "&u=" + Settings.Default.Temperature);
                XmlDocument wData = new XmlDocument();
                wData.Load(query);

                XmlNamespaceManager man = new XmlNamespaceManager(wData.NameTable);
                man.AddNamespace("yweather", "http://xml.weather.yahoo.com/ns/rss/1.0");

                XmlNode channel = wData.SelectSingleNode("rss").SelectSingleNode("channel");
                XmlNodeList nodes = wData.SelectNodes("/rss/channel/item/yweather:forecast", man);

                frmMain.Temperature = channel.SelectSingleNode("item").SelectSingleNode("yweather:condition", man).Attributes["temp"].Value;

                frmMain.Condition = channel.SelectSingleNode("item").SelectSingleNode("yweather:condition", man).Attributes["text"].Value;

                frmMain.Humidity = channel.SelectSingleNode("yweather:atmosphere", man).Attributes["humidity"].Value;

                frmMain.WinSpeed = channel.SelectSingleNode("yweather:wind", man).Attributes["speed"].Value;

                frmMain.Town = channel.SelectSingleNode("yweather:location", man).Attributes["city"].Value;

                frmMain.TFCond = channel.SelectSingleNode("item").SelectSingleNode("yweather:forecast", man).Attributes["text"].Value;

                frmMain.TFHigh = channel.SelectSingleNode("item").SelectSingleNode("yweather:forecast", man).Attributes["high"].Value;

                frmMain.TFLow = channel.SelectSingleNode("item").SelectSingleNode("yweather:forecast", man).Attributes["low"].Value;

                frmMain.QEvent = "connected";
            }
            catch { frmMain.QEvent = "failed"; }
        }

        public static void CheckBloggerForUpdates()
        {
            if (frmMain.QEvent == "UpdateYesNo")
            {
                frmMain.Jarvis.SpeakAsync("There is a new update available. Shall I start the download?");
            }
            else
            {
                String UpdateMessage;
                String UpdateDownloadLink;
                string AtomFeedURL = "http://michaelcjarvisupdatelist.blogspot.com/feeds/posts/default";
                XmlUrlResolver xmlResolver = new XmlUrlResolver();
                XmlTextReader xmlReader = new XmlTextReader(AtomFeedURL);
                xmlReader.XmlResolver = xmlResolver;
                XNamespace ns = XNamespace.Get("http://www.w3.org/2005/Atom");
                XDocument xmlFeed = XDocument.Load(xmlReader);
                var blogPosts = from item in xmlFeed.Descendants(ns + "entry")
                                select new
                                {
                                    Post = item.Element(ns + "content").Value
                                };

                foreach (var item in blogPosts)
                {
                    string[] separator = new string[] { "<br />" };
                    string[] data = item.Post.Split(separator, StringSplitOptions.None);
                    UpdateMessage = data[0];
                    UpdateDownloadLink = data[1];
                    if (UpdateDownloadLink == Properties.Settings.Default.RecentUpdate)
                    {
                        frmMain.QEvent = String.Empty;
                        frmMain.Jarvis.SpeakAsync("No new updates have been posted");
                    }
                    else
                    {
                        frmMain.Jarvis.SpeakAsync("A new update has been posted. The description says, " + UpdateMessage + ".");
                        System.Windows.Forms.MessageBox.Show(UpdateMessage, "Update Message");
                        frmMain.Jarvis.SpeakAsyncCancelAll();
                        frmMain.Jarvis.SpeakAsync("Would you like me to download the update?");
                        frmMain.QEvent = "UpdateYesNo";
                        Properties.Settings.Default.RecentUpdate = UpdateDownloadLink;
                        Properties.Settings.Default.Save();
                    }
                }
            }
        }
    }
}

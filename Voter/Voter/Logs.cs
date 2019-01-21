using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace Voter
{
    /// <summary>
    /// Klasa odpowiada za przechowywanie logów.
    /// </summary>
    class Logs
    {
        /// <summary>
        /// Używany dziennik logów
        /// </summary>
        private ListView logListView;

        /// <summary>
        /// Nazwa (identyfikator) Votera
        /// </summary>
        private string voterName;
        public string VoterName
        {
            set { voterName = value; }
            get { return voterName; }
        }

        /// <summary>
        /// Konstruktor klasy Logger
        /// </summary>
        /// <param name="logger"> dziennik logów (listView)</param>
        public Logs(ListView logger)
        {
            this.logListView = logger;
            this.voterName = "Voter";
        }

        /// <summary>
        /// Delegat na potrzeby dodawania logów (AddLog)
        /// </summary>
        /// <param name="log"></param>
        /// <param name="type"></param>
        /// <param name="time"></param>
        private delegate void LogDelegate(string log, LogType type, bool time = true);

        /// <summary>
        /// Typy logów
        /// </summary>
        public enum LogType { Info, Message, Error, Special };

        /// <summary>
        /// Dodawanie logów
        /// </summary>
        /// <param name="log">wiadomość/log</param>
        /// <param name="type">rodzaj wiadomości</param>
        /// <param name="time">podanie czasu</param>
        public void AddLog(string log, LogType type, bool time = true)
        {
            ListViewItem item = new ListViewItem();

            switch (type)
            {
                case LogType.Info:
                    item.ForeColor = Color.Black;
                    break;
                case LogType.Message:
                    item.ForeColor = Color.Blue;
                    break;
                case LogType.Error:
                    item.ForeColor = Color.Red;
                    break;
                case LogType.Special:
                    item.ForeColor = Color.Gold;
                    break;
            }

            if (time)
            {
                item.Text = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + log;
            }
            else
            {
                item.Text = log;
            }

            if (logListView.InvokeRequired)
            {
                logListView.Invoke(new LogDelegate(AddLog), new object[] { log, type, time });
                return;
            }

            logListView.Items.Add(item);
            logListView.EnsureVisible(logListView.Items.Count - 1);

            try
            {
                using (System.IO.StreamWriter file = new StreamWriter(voterName + "-logs.txt", true))
                {
                    file.Write(" " + DateTime.Now.ToString("HH:mm:ss") + " >>> " + log + Environment.NewLine);
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine("[File logger] >> " + exp);
            }
        }

        public void AddExpToFile(Exception ex)
        {
            string text = ex.ToString();
            try
            {
                using (System.IO.StreamWriter file = new StreamWriter(voterName + "-logs.txt", true))
                {
                    file.Write(" " + DateTime.Now.ToString("HH:mm:ss") + " >>> Wystąpił wyjątek: " + Environment.NewLine + text);
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine("[Exception logger] >> " + exp);
            }
        }
    }
}

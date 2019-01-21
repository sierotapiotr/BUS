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
        /// Lista z logami
        /// </summary>
        private ListView logsListView;

        private string voterName;

        public string VoterName
        {
            set { voterName = value; }
            get { return voterName; }
        }


        /// <summary>
        /// Konstruktow klasy Logs
        /// </summary>
        /// <param name="logsListView">lista wyświetlająca logi</param>
        public Logs(ListView logsListView)
        {
            this.logsListView = logsListView;
            this.voterName = "Voter";
        }


        /// <summary>
        /// Metoda dodająca logi
        /// </summary>
        /// <param name="log">treść wiadomości</param>
        /// <param name="add_time">czas dodania</param>
        /// <param name="flag">rodzaj wiadomości</param>
        /// <param name="anotherThread">flaga dot. innego wątku</param>
        public void addLog(string log, bool add_time, int flag, bool anotherThread = false)
        {
            ListViewItem item = new ListViewItem();

            // WYBÓR KOLOR LINII LOGÓW
            switch (flag)
            {
                case 0:
                    item.ForeColor = Color.Blue;
                    break;
                case 1:
                    item.ForeColor = Color.Black;
                    break;
                case 2:
                    item.ForeColor = Color.Red;
                    break;
                case 3:
                    item.ForeColor = Color.Green;
                    break;
            }
            
            if (add_time)
                item.Text = "[" + DateTime.Now.ToString("HH:mm:ss") + "]" + log;
            else
                item.Text = log;


            if (!anotherThread)
            {
                logsListView.Items.Add(item);
                logsListView.Items[logsListView.Items.Count - 1].EnsureVisible(); 
            }
            else
            {
                try
                {
                    logsListView.Invoke(new MethodInvoker(delegate ()
                    {
                        logsListView.Items.Add(item);
                        logsListView.Items[logsListView.Items.Count - 1].EnsureVisible();
                    })
                    );
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            try
            {
                using (System.IO.StreamWriter file = new StreamWriter(@"Logs\" + voterName + ".txt", true))
                {
                    file.Write(" [" + DateTime.Now.ToString("HH:mm:ss") + "]" + log + Environment.NewLine);

                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }

        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ElectionAuthority
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// Wyświetla logi w polu dziennika logów
        /// </summary>
        private Logger logs;

        /// <summary>
        /// Wczytuje konfigurację z pliku
        /// </summary>
        private Config configuration;

        /// <summary>
        /// Główna klasa aplikacji 
        /// </summary>
        private ElectionAuthority electionAuthority;

        public Form1()
        {
            InitializeComponent();
            logs = new Logger(this.logListView);
            configuration = new Config(this.logs);
            logs.AddLog(Constants.LOG_PROGRAM_START, Logger.LogType.Info);
        }

        private void loadConfigButton_Click(object sender, EventArgs e)
        {
            confOpenFileDialog.ShowDialog();
        }

        private void confOpenFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            configuration.LoadConfiguration(confOpenFileDialog.FileName);
            enableStepTwo();
            electionAuthority = new ElectionAuthority(this.logs, this.configuration, this);
        }

        private void startButton_Click(object sender, EventArgs e)
        {

        }

        private void proxyButton_Click(object sender, EventArgs e)
        {

        }

        private void endButton_Click(object sender, EventArgs e)
        {

        }

        private void countButton_Click(object sender, EventArgs e)
        {

        }


        private void enableStepTwo()
        {
            this.startButton.Enabled = true;
            this.loadConfigButton.Enabled = false;
        }
    }
}

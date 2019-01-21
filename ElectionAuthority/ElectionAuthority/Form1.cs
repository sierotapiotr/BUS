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
        }

        private void loadCandidatesButton_Click(object sender, EventArgs e)
        {
           candOpenFileDialog.ShowDialog();
        }

        private void candOpenFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            configuration.LoadCandidates(candOpenFileDialog.FileName);
            enableStepThree();
            electionAuthority = new ElectionAuthority(this.logs, this.configuration, this);
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            this.electionAuthority.ServerForVoters.Start(configuration.ServerPortForVoters);
            this.electionAuthority.ServerForProxy.Start(configuration.ServerPortForProxy);

            this.electionAuthority.GenerateData();

            enableStepFour();
        }

        private void proxyButton_Click(object sender, EventArgs e)
        {
            this.electionAuthority.SendSLAndTokensToProxy();

            enableStepFive();
        }

        private void endButton_Click(object sender, EventArgs e)
        {
            this.electionAuthority.StopServers();
            enableStepSix();
        }

        private void countButton_Click(object sender, EventArgs e)
        {
            this.electionAuthority.CountVotes();
            this.countButton.Enabled = false;
        }


        private void enableStepTwo()
        {
            this.loadConfigButton.Enabled = false;
            this.loadCandidatesButton.Enabled = true;
        }

        private void enableStepThree()
        {
            this.loadCandidatesButton.Enabled = false;
            this.startButton.Enabled = true;
        }

        private void enableStepFour()
        {
            this.startButton.Enabled = false;
            this.proxyButton.Enabled = true;
        }

        private void enableStepFive()
        {
            this.proxyButton.Enabled = false;
            this.endButton.Enabled = true;
        }

        private void enableStepSix()
        {
            this.endButton.Enabled = false;
            this.countButton.Enabled = true;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.electionAuthority != null)
            {
                if (this.electionAuthority.ServerForProxy != null)
                    this.electionAuthority.ServerForProxy.Stop();
                if (this.electionAuthority.ServerForVoters != null)
                    this.electionAuthority.ServerForVoters.Stop();
            }
        }
    }
}

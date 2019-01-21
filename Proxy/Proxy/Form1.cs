using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Proxy
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// Wyświetla logi w polu dziennika logów
        /// </summary>
        private Logs logs;

        /// <summary>
        /// Wczytuje konfigurację z pliku
        /// </summary>
        private Configuration configuration;

        /// <summary>
        /// Główna klasa aplikacji 
        /// </summary>
        private Proxy proxy;
        
        public Form1()
        {
            InitializeComponent();
            this.logs = new Logs(this.logsListView);
            this.configuration = new Configuration(this.logs);
            logs.AddLog(Constants.PROGRAM_START, Logs.LogType.Info);
        }

        private void configButton_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog();
        }

        private void openFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            configuration.LoadConfiguration(openFileDialog.FileName);
            enableButtonsAfterConfiguration();
            this.proxy = new Proxy(this.logs, this.configuration,this);

        }

        private void connectElectionAuthorityButton_Click(object sender, EventArgs e)
        {
            this.proxy.Client.Connect(configuration.ElectionAuthorityIP, configuration.ElectionAuthorityPort);
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.proxy != null)
            {
                if (this.proxy.Server !=  null )
                    this.proxy.Server.StopServer();
                if (this.proxy.Client != null)
                    this.proxy.Client.DisconnectFromElectionAuthority();
            }
            
        }

        private void startProxyButton_Click(object sender, EventArgs e)
        {
            this.proxy.Server.StartServer(configuration.ProxyPortForVoters);
            this.proxy.GenerateSR();
            this.proxy.GenerateYesNoPosition();


            this.startProxyButton.Enabled = false;
            this.configButton.Enabled = false;
        }

        private void enableButtonsAfterConfiguration()
        {
            this.startProxyButton.Enabled = true;
            this.connectElectionAuthorityButton.Enabled = true;
        }

        public void disableConnectElectionAuthorityButton()
        {
            this.connectElectionAuthorityButton.Enabled = false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Voter
{
    /// <summary>
    /// graficzny interfejs użytkownika
    /// </summary>
    public partial class Form1 : Form
    {
        /// <summary>
        /// wyświetlane logi
        /// </summary>
        private Logs logs;
        /// <summary>
        /// konfiguracja wczytana z pliku xml
        /// </summary>
        private Configuration configuration;
        /// <summary>
        /// aplikacja głosującego
        /// </summary>
        private Voter voter;
        /// <summary>
        /// pola tekstowe z imionami i nazwiskami kandydatów
        /// </summary>
        private List<TextBox> textBoxes;
        public List<TextBox> TextBoxes
        {
            get { return textBoxes; }
        }
        /// potwierdzenie oddanego głosu
        /// </summary>
        private Confirmation confirmation;
        /// <summary>
        /// lista przycisków używanych podczas głosowania
        /// </summary>
        private List<Button[]> voteButtons;
        public List<Button[]> VoteButtons
        {
            get { return voteButtons; }
        }

        /// <summary>
        ///konstruktor klasy Form1
        /// </summary>
        public Form1()
        {
            
            InitializeComponent();
            setColumnWidth();
            this.logs = new Logs(this.logsListView);
            this.confirmation = new Confirmation(this.confirmationListView);
            this.configuration = new Configuration(this.logs);
            this.textBoxes = new List<TextBox>();
            this.voteButtons = new List<Button[]>();

        }
        /// <summary>
        /// Metoda łącząca z EA.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EAConnectButton_Click(object sender, EventArgs e)
        {
            this.voter.ElectionAuthorityClient.Connect(this.configuration.ElectionAuthorityIP, this.configuration.ElectionAuthorityPort, Constants.ELECTION_AUTHORITY);
            this.configButton.Enabled = false;
            
        }

        /// <summary>
        /// Metoda obsługująca kliknięcie przycisku do głosowania.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void voteButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            //Console.WriteLine(clickedButton);
            String[] words = clickedButton.Name.Split(';');
            if (this.voter.VoterBallot.Vote(Convert.ToInt32(words[1]), Convert.ToInt32(words[2])))
            {
                logs.AddLog(Constants.VOTE_DONE, Logs.LogType.Info);
                if (this.voter.VoterBallot.VoteDone())
                {
                    logs.AddLog(Constants.VOTE_FINISH, Logs.LogType.Info);
                    this.disableVoteButtons();
                    this.confirmationBox.Enabled = true;

                }

            }
            else
            {
                logs.AddLog(Constants.VOTE_ERROR, Logs.LogType.Error);
            }

            //Console.WriteLine(words[0] );

        }

        /// <summary>
        /// Deaktywacja przycisków do głosowania.
        /// </summary>
        private void disableVoteButtons()
        {
            for (int i=0; i<this.voteButtons.Count; i++)
            {
                for (int j = 0; j < this.voteButtons[i].Length; j++)
                {
                    this.voteButtons[i].ElementAt(j).Enabled = false;
                }
            }
        }

        /// <summary>
        /// Ustawienie szerokości kolumny z logami.
        /// </summary>
        private void setColumnWidth()
        {
            this.logColumn.Width = this.logsListView.Width - 5;
        }

        /// <summary>
        /// Czynności do wykonania przy zamykaniu aplikacji.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClsing(object sender, FormClosingEventArgs e)
        {
            if (this.voter != null)
            {

                if (this.voter.ElectionAuthorityClient.Connected)
                    this.voter.ElectionAuthorityClient.Disconnect();

                if (this.voter.ProxyClient.Connected)
                    this.voter.ProxyClient.Disconnect();
            }
        }

        /// <summary>
        /// Metoda obsługująca kliknięcie przysku związanego z łączeniem z aplikacją Proxy.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProxyConnectButton_Click(object sender, EventArgs e)
        {
            this.voter.ProxyClient.Connect(configuration.ProxyIP, configuration.ProxyPort, Constants.PROXY);
            this.getSLandSRButton.Enabled = true;
        }

        /// <summary>
        /// Metoda obsługująca kliknięcie przycisku do wczytywania pliku.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void configButton_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog();
        }

        /// <summary>
        /// Metoda (wewnętrzna) obsługująca kliknięcie przycisku do wczytywania pliku.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            configuration.loadConfiguration(openFileDialog.FileName);
            enableButtonsAfterLoadingConfiguration();
            
            this.voter = new Voter(this.logs, this.configuration, this, this.confirmation);
            addFieldsForCandidates(configuration.NumberOfCandidates);

        }

        /// <summary>
        /// Metoda dodająca pola dla kandydatów.
        /// </summary>
        /// <param name="NumberOfCandidates"></param>
        private void addFieldsForCandidates(int NumberOfCandidates)
        {
            for (int i = 0; i < NumberOfCandidates; i++)
            {
                TextBox newTextBox = new TextBox();
                textBoxes.Add(newTextBox);

                Button[] newVoteButtons = new Button[Constants.BALLOTSIZE];
                for (int it = 0; it < Constants.BALLOTSIZE; it++)
                {
                    Button newCandidateButton = new Button();
                    newVoteButtons[it] = newCandidateButton;
                }

                voteButtons.Add(newVoteButtons);
                this.panel1.Controls.Add(newTextBox);
                this.textBoxes[i].Location = new System.Drawing.Point(23, 18 + i * 40);
                this.textBoxes[i].Multiline = true;
                this.textBoxes[i].Name = "Candidate nr" + i;
                this.textBoxes[i].Size = new System.Drawing.Size(200, 40);
                this.textBoxes[i].TabIndex = 0;

                for (int j = 0; j < Constants.BALLOTSIZE; j++)
                {
                    //this.EAConnectButton.Enabled = false;
                    this.voteButtons[i].ElementAt(j).Location = new System.Drawing.Point(240 + j * 75, 17 + i * 40);
                    this.voteButtons[i].ElementAt(j).Name = "Candidate_nr;" + i + ";" + j;
                    this.voteButtons[i].ElementAt(j).Size = new System.Drawing.Size(70, 40);
                    this.voteButtons[i].ElementAt(j).TabIndex = 0;
                    this.voteButtons[i].ElementAt(j).Text = Convert.ToString(j);
                    this.voteButtons[i].ElementAt(j).Enabled = false;
                    this.voteButtons[i].ElementAt(j).UseVisualStyleBackColor = true;
                    this.voteButtons[i].ElementAt(j).Click += new System.EventHandler(voteButton_Click);
                    this.panel1.Controls.Add(voteButtons[i].ElementAt(j));
                }
            }


        }
        /// <summary>
        /// Metoda aktywująca przyciski po wczytaniu konfiguracji.
        /// </summary>
        private void enableButtonsAfterLoadingConfiguration()
        {
            this.ProxyConnectButton.Enabled = true;
            this.EAConnectButton.Enabled = true;
               
        }
        /// <summary>
        /// Metoda wysyłająca zapytanie o SL oraz SR.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void getSLandSRButton_Click(object sender, EventArgs e)
        {
            this.voter.RequestSLandSR();
        }


        /// <summary>
        /// Metoda deaktywująca przycisk do pobrania SL i SR.
        /// </summary>
        public void disableSLAndSRButton()
        {
            this.getSLandSRButton.Enabled = false;
            this.getCandidateListButton.Enabled = true;
        }


        /// <summary>
        /// Metoda obługująca kliknięcie przycisku pobierającego listę kandydatów.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void getCandidateListButton_Click(object sender, EventArgs e)
        {
            this.voter.RequestCandidates();
        }

        /// <summary>
        /// Metoda deaktywująca przycisk łączący z aplikacją proxy.
        /// </summary>
        public void disableConectionProxyButton()
        {
            this.ProxyConnectButton.Enabled = false;

        }

        /// <summary>
        /// Metoda deaktywująca przycisk łączący z aplikacją EA.
        /// </summary>
        public void disableConnectionEAButton()
        {
            this.EAConnectButton.Enabled = false;
        }


        /// <summary>
        /// Metoda deaktywująca przycisk do pobierania listy kandydatów.
        /// </summary>
        public void disableGetCandidateListButton()
        {
            this.getCandidateListButton.Enabled = false;
            //if (this.getYesNoPositionButton.Enabled == false)
            //    this.sendVoteButton.Enabled = true;
        }


        /// <summary>
        /// Metoda przesyłąjąca głos do Proxy.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendVoteButton_Click(object sender, EventArgs e)
        {
            this.voter.SendVoteToProxy();
            this.sendVoteButton.Enabled = false;
            this.confirmationBox.Enabled = false;
                 
        }

        /// <summary>
        /// Metoda obsługująca klinięcie w combobox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void confirmationListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.voter.SetConfirm(this.confirmationBox.SelectedIndex);
            this.sendVoteButton.Enabled = true;
        }

        public int confBox { get; set; }


        private void logsListView_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}

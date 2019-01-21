using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Org.BouncyCastle.Math;

namespace Voter
{
    /// <summary>
    /// Główna klasa zbierająca informacje od głosującego
    /// </summary>
    class Voter
    {
        /// <summary>
        /// wyświetlane logi
        /// </summary>
        private Logs logs;
        /// <summary>
        /// konfiguracja wczytana z pliku
        /// </summary>
        private Configuration configuration;

        /// <summary>
        /// potwierdzenie głosu
        /// </summary>
        private Confirmation confirmation;

        /// <summary>
        /// klient TCP - Proxy
        /// </summary>
        private Client proxyClient;

        /// <summary>
        /// klient TCP - EA
        /// </summary>
        private Client electionAuthorityClient;

        /// <summary>
        /// karta do głosowania
        /// </summary>
        private VoterBallot voterBallot;

        /// <summary>
        /// okno aplikacji
        /// </summary>
        private Form1 form;

   
        public Client ProxyClient
        {
            get { return proxyClient; }
        }

        public Client ElectionAuthorityClient
        {
            get { return electionAuthorityClient; }
        }

        public VoterBallot VoterBallot
        {
            get { return voterBallot; }
        }


        /// <summary>
        /// Konstruktor klasy Voter
        /// </summary>
        /// <param name="logs">wyświetlane logi</param>
        /// <param name="configuration">konfiguracja z pliku</param>
        /// <param name="form">okno aplikacji</param>
        /// <param name="confirmation">potiwerdzenie głosu</param>
        public Voter(Logs logs, Configuration configuration, Form1 form, Confirmation confirmation)
        {
            this.logs = logs;
            this.configuration = configuration;
            this.form = form;
            this.proxyClient = new Client(this.configuration.Name, this.logs, this);
            this.electionAuthorityClient = new Client(this.configuration.Name, this.logs, this);
            this.voterBallot = new VoterBallot(this.configuration.NumberOfCandidates);
            this.confirmation = confirmation;

        }

        /// <summary>
        /// Metoda pytająca o listę kandydatów
        /// </summary>
        public void RequestCandidates()
        {
            string msg = Constants.GET_CANDIDATE_LIST + "&" + this.configuration.Name + "=" + this.voterBallot.SL.ToString();
            this.electionAuthorityClient.SendMessage(msg);
        }

        /// <summary>
        /// Metoda pytająca o Sl i SR
        /// </summary>
        public void RequestSLandSR()
        {
            string msg = Constants.GET_SL_AND_SR + "&" + this.configuration.Name;
            this.proxyClient.SendMessage(msg);
        }


        /// <summary>
        /// Metoda zapisująca listę kandydatów.
        /// </summary>
        /// <param name="message"></param>
        public void SaveCandidateList(string message)
        {
            string[] list = message.Split(';');
            for (int i = 0; i < list.Length; i++)
            {
                this.form.Invoke(new MethodInvoker(delegate ()
                {
                    this.form.TextBoxes[i].Text = list[i];
                    this.form.TextBoxes[i].Enabled = false;
                }));
            }
            DisableGetCandidateListButton();
            EnableVotingButtons();
        }


        /// <summary>
        /// Metoda wysyłająca głos do proxy
        /// </summary>
        public void SendVoteToProxy()
        {
            int[,] table = this.voterBallot.Voted;
            string message = Constants.VOTE + "&" + this.configuration.Name + ";";
            for (int i = 0; i < table.GetLength(0); i++)
            {

                for (int j = 0; j < table.GetLength(1); j++)
                {
                    if (j == table.GetLength(1) - 1 && i == table.GetLength(0) - 1)
                        message = message + table[i, j].ToString();
                    else if (j == table.GetLength(1) - 1 && i != table.GetLength(0) - 1)
                        message = message + table[i, j].ToString() + ";";
                    else
                        message = message + table[i, j].ToString() + ":";
                }
                //vote wyglada tak: VOTE&Voter0;1:0:0:0;1:0:0:0;0:0:0:1;0:0:0:1;0:0:0:1
            }
            message = message + ";" + confirmation.Index.ToString();

            this.proxyClient.SendMessage(message);
        }

        /// <summary>
        /// Metoda wybierająca kolumnę, która ma służyc jako potwierdzenie
        /// </summary>
        /// <param name="column"></param>
        public void SetConfirm(int column)
        {
            for (int i = 0; i < this.voterBallot.Voted.GetLength(0); i++)
            {
                this.confirmation.Column += this.voterBallot.Voted[i, column];
            }

            confirmation.ColumnNumber = column + 1;
        }

        /// <summary>
        /// Metoda zapisująca otrzymane podpisana kolumne oraz token
        /// </summary>
        /// <param name="message"></param>
        public void SaveSignedColumnAndToken(string message)
        {
            string[] words = message.Split(';');

            this.voterBallot.SignedBlindColumn = new BigInteger(words[0]);
            this.voterBallot.Token = new BigInteger(words[1]);

            this.confirmation.SignedColumn = this.voterBallot.SignedBlindColumn;
            this.confirmation.Token = this.voterBallot.Token;

            this.logs.AddLog(Constants.SIGNED_COLUMNS_TOKEN_RECEIVED, Logs.LogType.Info);

            this.confirmation.addConfirm(true);
        }

        /// <summary>
        /// Deaktywacja przycisku "Pobierz SL i SR"
        /// </summary>
        public void DisableSLAndSRButton()
        {
            this.form.Invoke(new MethodInvoker(delegate ()
            {
                this.form.disableSLAndSRButton();
            }
                ));
        }

        /// <summary>
        /// Deaktywacja przycisku "Połącz z proxy"
        /// </summary>
        public void DisableConnectionProxyButton()
        {
            this.form.Invoke(new MethodInvoker(delegate ()
            {
                this.form.disableConectionProxyButton();

            }));
        }
        /// <summary>
        /// Deaktywacja przycisku "Połącz z EA"
        /// </summary>
        public void DisableConnectionEAButton()
        {
            this.form.Invoke(new MethodInvoker(delegate ()
            {
                this.form.disableConnectionEAButton();
            }));
        }
        /// <summary>
        /// Deaktywacja przycisku "Pobierz listę kandydatów"
        /// </summary>
        private void DisableGetCandidateListButton()
        {
            this.form.Invoke(new MethodInvoker(delegate ()
            {
                this.form.disableGetCandidateListButton();
            }));

        }
        /// <summary>
        /// Aktywacja przycisków dot. głosowania
        /// </summary>
        private void EnableVotingButtons()
        {
            for (int i = 0; i < this.configuration.NumberOfCandidates; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    this.form.Invoke(new MethodInvoker(delegate ()
                    {
                        this.form.VoteButtons[i].ElementAt(j).Enabled = true;
                    }));
                }

            }

        }

    }
}

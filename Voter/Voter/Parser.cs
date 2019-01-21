using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Math;

namespace Voter
{
    /// <summary>
    /// klasa parsująca otrzymane wiadomości
    /// </summary>
    class Parser
    {
        /// <summary>
        /// Instancja votera, z którą połączony jest voter.
        /// </summary>
        private Voter voter;
        /// <summary>
        /// klasa reprezentująca logi
        /// </summary>
        private Logs logs;

        /// <summary>
        /// konstruktor klasy parser
        /// </summary>
        /// <param name="logs">logi</param>
        /// <param name="voter">instancja votera</param>
        public Parser(Logs logs, Voter voter)
        {
            this.voter = voter;
            this.logs = logs;
        }

        /// <summary>
        /// Metoda parsująca wiadomości.
        /// </summary>
        /// <param name="message"></param>
        public void ParseMessage(string message)
        {
            string[] elem = message.Split('&');
            switch (elem[0])
            {
                case Constants.SL_AND_SR: //message from Proxy which contains sl and sr number
                    SaveSLAndSR(elem[1]);
                    break;

                case Constants.CONNECTION_SUCCESSFUL:
                    DisableConnectionProxyButton();
                    break;
                case Constants.CONNECTED:
                    DisableConnectionEAButton();
                    break;
                case Constants.CANDIDATE_LIST_RESPONSE:
                    SaveCandidateList(elem[1]);
                    break;
                case Constants.SIGNED_COLUMNS_TOKEN:
                    SaveSignedColumnAndToken(elem[1]);
                    break;

            }
        }

        /// <summary>
        /// Metoda zapisująca podpisaną kolumnę karty z tokenem.
        /// </summary>
        /// <param name="message"></param>
        private void SaveSignedColumnAndToken(string message)
        {
            this.voter.SaveSignedColumnAndToken(message);
        }

        /// <summary>
        /// Metoda zapisująca listę kandydatów
        /// </summary>
        /// <param name="list"></param>
        private void SaveCandidateList(string list)
        {
            this.voter.SaveCandidateList(list);
        }

        /// <summary>
        /// Metoda zapisująca SL oraz SR.
        /// </summary>
        /// <param name="message"></param>
        private void SaveSLAndSR(string message)
        {
            string[] elem = message.Split('=');
            BigInteger SL = new BigInteger(elem[0]);

            BigInteger SR = new BigInteger(elem[1]);

            this.voter.VoterBallot.SL = SL;
            Console.WriteLine("SL = " + SL);
            this.voter.VoterBallot.SR = SR;
            Console.WriteLine("SR = " + SR);

            logs.AddLog(Constants.SL_AND_SR_RECEIVED, Logs.LogType.Info);
            this.voter.DisableSLAndSRButton();
        }

        /// <summary>
        /// Deaktywacja przycisku "Połącz z EA"
        /// </summary>
        private void DisableConnectionEAButton()
        {
            this.voter.DisableConnectionEAButton();

        }

        /// <summary>
        /// Deaktywacja przycisku "Połącz z Proxy"
        /// </summary>
        private void DisableConnectionProxyButton()
        {
            this.voter.DisableConnectionProxyButton();
        }
    }
}

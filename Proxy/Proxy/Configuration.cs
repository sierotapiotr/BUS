using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Proxy
{

    /// <summary>
    /// Configuration class - loads configuration from file and sets up proxy 
    /// </summary>
    class Configuration
    {
        /// <summary>
        /// Wyświetla logi w polu dziennika logów
        /// </summary>
        private Logs logs;

        /// <summary>
        /// Proxy ID
        /// </summary>
        private string proxyID;
        public string ProxyID
        {
            get { return proxyID; }
        }

        /// <summary>
        /// Port do komunikacji z Voterami
        /// </summary>
        private string proxyPortForVoters;
        public string ProxyPortForVoters
        {
            get { return proxyPortForVoters; }
        }

        /// <summary>
        /// Election authority IP
        /// </summary>
        private string electionAuthorityIP;
        public string ElectionAuthorityIP
        {
            get { return electionAuthorityIP; }
        }

        /// <summary>
        /// EA port
        /// </summary>
        private string electionAuthorityPort;
        public string ElectionAuthorityPort
        {
            get { return electionAuthorityPort; }
        }

        /// <summary>
        /// Liczba Voterów
        /// </summary>
        private int numOfVoters;
        public int NumOfVoters
        {
            get { return numOfVoters; }
        }

        /// <summary>
        /// Liczba kandydatów
        /// </summary>
        private int numOfCandidates;
        public int NumOfCandidates
        {
            get { return numOfCandidates; }
        }

        /// <summary>
        /// Konstruktor klasy Configuration
        /// </summary>
        /// <param name="logs">dziennik logów</param>
        public Configuration(Logs logs)
        {
            this.logs = logs;
        }

        /// <summary>
        /// Funkcja pomocnicza do odczytania konfiguracji z pliku XML
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        private List<String> ReadConfig(XmlDocument xml)
        {

            List<String> list = new List<String>();

            foreach (XmlNode xnode in xml.SelectNodes("//Proxy[@ID]"))
            {
                string proxyId = xnode.Attributes[Constants.ID].Value;
                list.Add(proxyId);
                string proxyPortForVoters = xnode.Attributes[Constants.PROXY_PORT].Value;
                list.Add(proxyPortForVoters);
                string electionAuthorityIP = xnode.Attributes[Constants.ELECTION_AUTHORITY_IP].Value;
                list.Add(electionAuthorityIP);
                string electionAuthorityPort = xnode.Attributes[Constants.ELECTION_AUTHORITY_PORT].Value;
                list.Add(electionAuthorityPort);
                string numberOfVoters = xnode.Attributes[Constants.NUMBER_OF_VOTERS].Value;
                list.Add(numberOfVoters);
                string numberOfCandidates = xnode.Attributes[Constants.NUMBER_OF_CANDIDATES].Value;
                list.Add(numberOfCandidates);
            }

            return list;

        }

        /// <summary>
        /// Załadowanie danych z pliku konfiguracyjnego XML
        /// </summary>
        /// <param name="path">ścieżka do pliku XML</param>
        /// <returns></returns>
        public bool LoadConfiguration(string path)
        {
            XmlDocument xml = new XmlDocument();
            try
            {
                xml.Load(path);
                List<String> conf = new List<String>();
                conf = ReadConfig(xml);

                this.proxyID = conf[0];
                this.proxyPortForVoters = conf[1];
                this.electionAuthorityIP = conf[2];
                this.electionAuthorityPort = conf[3];
                this.numOfVoters = Convert.ToInt32(conf[4]);
                this.numOfCandidates = Convert.ToInt32(conf[5]);

                string[] filePath = path.Split('\\');
                logs.AddLog(Constants.CONFIGURATION_LOADED_FROM + filePath[filePath.Length - 1], Logs.LogType.Info);
                return true;
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp);
                logs.AddLog(Constants.CONFIGURATION_ERROR + path, Logs.LogType.Error);
                logs.AddExpToFile(exp);
                return false;
            }
        }
    }
}

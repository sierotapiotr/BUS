using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Voter
{

    /// <summary>
    /// Klasa powiązana z wczytywaniem konfiguracji z pliku XML
    /// </summary>
    class Configuration
    {
        /// <summary>
        /// Logi - służą wyświetlaniu wiadomości
        /// </summary>
        private Logs logs;

        /// <summary>
        /// Unikalne ID Votera
        /// </summary>
        private string voterID;
        /// <summary>
        /// Adres IP Election Authority
        /// </summary>
        private string electionAuthorityIP;
        /// <summary>
        /// Port, na którym nasłuchuje ElectionAuthority
        /// </summary>
        private string electionAuthorityPort;
        /// <summary>
        /// Adres IP Proxy
        /// </summary>
        private string proxyIP;
        /// <summary>
        /// Port, na którym nasłuchuje Proxy
        /// </summary>
        private string proxyPort;
        /// <summary>
        /// Liczba kandydatów biorących udział w głosowaniu.
        /// </summary>
        private int numberOfCandidates;
        /// <summary>
        /// Unikalna nazwa Votera
        /// </summary>
        private string name;


        /// <summary>
        /// Metody typu "getter"
        /// </summary>
        public string VoterID
        {
            get { return voterID; }
        }

        public string ElectionAuthorityIP
        {
            get { return electionAuthorityIP; }
        }

        public string ElectionAuthorityPort
        {
            get { return electionAuthorityPort; }
        }

        public string ProxyIP
        {
            get { return proxyIP; }
        }

        public string ProxyPort
        {
            get { return proxyPort; }
        }


        public int NumberOfCandidates
        {
            get { return numberOfCandidates; }
        }

        public string Name
        {
            get { return name; }
        }


        /// <summary>
        /// Konstruktor klasy konfiguracyjnej
        /// </summary>
        /// <param name="logs">wiadomości wyświetlane w logach</param>
        public Configuration(Logs logs)
        {
            this.logs = logs;
        }

        /// <summary>
        /// Metoda służąca pobraniu konfiguracji z pliku XML
        /// </summary>
        /// <param name="xml">plik wybrany przez użytkownika</param>
        /// <returns></returns>
        private List<String> readConfig(XmlDocument xml)
        {
            List<String> list = new List<String>();

            foreach (XmlNode xnode in xml.SelectNodes("//Voter[@ID]"))
            {
                Console.Write("TEST TEST TEST");
                string voterId = xnode.Attributes[Constants.ID].Value;
                list.Add(voterId);
                string electionAuthorityIP = xnode.Attributes[Constants.ELECTION_AUTHORITY_IP].Value;
                list.Add(electionAuthorityIP);
                string electionAuthorityPort = xnode.Attributes[Constants.ELECTION_AUTHORITY_PORT].Value;
                list.Add(electionAuthorityPort);
                string proxyIP = xnode.Attributes[Constants.PROXY_IP].Value;
                list.Add(proxyIP);
                string proxyPort = xnode.Attributes[Constants.PROXY_PORT].Value;
                list.Add(proxyPort);
                string numberOfCandidates = xnode.Attributes[Constants.NUMBER_OF_CANDIDATES].Value;
                list.Add(numberOfCandidates);
                string name = xnode.Attributes[Constants.NAME].Value;
                list.Add(name);
            }

            return list;
        }

        /// <summary>
        /// Metoda służąca załadowaniu konfiguracji z pliku XML, używa wyniku metody readConfig()
        /// </summary>
        /// <param name="path">ścieżka do pliku konfiguracyjnego</param>
        /// <returns></returns>
        public bool loadConfiguration(string path)
        {
            XmlDocument xml = new XmlDocument();
            try
            {
                xml.Load(path);
                List<String> voterConf = new List<String>();
                voterConf = readConfig(xml);

                this.voterID = voterConf[0];
                this.electionAuthorityIP = voterConf[1];
                this.electionAuthorityPort = voterConf[2];
                this.proxyIP = voterConf[3];
                this.proxyPort = voterConf[4];
                this.numberOfCandidates = Convert.ToInt32(voterConf[5]);
                this.name = voterConf[6];
                this.logs.VoterName = name;

                string[] filePath = path.Split('\\');
                logs.addLog(Constants.CONFIGURATION_LOADED_FROM + filePath[filePath.Length - 1], true, Constants.LOG_INFO, true);
                return true;
             }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }


        }
    }
}


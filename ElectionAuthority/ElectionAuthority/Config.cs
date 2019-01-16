using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ElectionAuthority
{
    /// <summary>
    /// Wczytuje konfigurację z pliku
    /// </summary>
    class Config
    {

        /// <summary>
        /// Wyświetla logi w polu dziennika logów
        /// </summary>
        private Logger logs;

        private string serverPort;
        public string ServerPort
        {
            get { return serverPort; }
        }

        private string numberOfVoters;
        public string NumberOfVoters
        {
            get { return numberOfVoters; }
        }

        private List<string> candidates;
        public List<string> Candidates
        {
            get { return candidates; }
        }

        /// <summary>
        /// Konstruktor klasy Config 
        /// </summary>
        /// <param name="logs"></param>
        public Config(Logger logs)
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
            List<String> conf = new List<String>();

            foreach (XmlNode xnode in xml.GetElementsByTagName("ElectionAuthority"))
            {
                string temp = xnode.Attributes[Constants.SERVER_PORT].Value;
                conf.Add(temp);
                temp = xnode.Attributes[Constants.NUMBER_OF_VOTERS].Value;
                conf.Add(temp);
            }
            return conf;
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

                this.serverPort = conf[0];
                this.numberOfVoters = conf[1];

                string[] filePath = path.Split('\\');
                logs.AddLog(Constants.LOG_CONFIG_FILE + filePath[filePath.Length - 1], Logger.LogType.Info);
                return true;
            }
            catch (Exception exp)
            {
                logs.AddLog(Constants.LOG_CONFIG_FILE_ERROR + path, Logger.LogType.Error);
                Console.WriteLine("[Config load] >> " + exp);
                logs.AddExpToFile(exp);
                return false;
            }
        }

        /// <summary>
        /// Wczytanie listy kandydatów
        /// </summary>
        /// <param name="path">ścieżka do pliku XML</param>
        /// <returns>List of strings with candidates</returns>
        bool LoadCandidates(string path)
        {
            List<string> candidates = new List<string>();
            XmlDocument xml = new XmlDocument();
            try
            {
                xml.Load(path);

                foreach (XmlNode xnode in xml.SelectNodes("//Candidates/Candidate"))
                {
                    string name = xnode.Attributes["name"].Value;
                    candidates.Add(name);
                }

                logs.AddLog(Constants.LOG_CANDIDATES_FILE, Logger.LogType.Info);
                return true;
            }
            catch (Exception exp)
            {
                logs.AddLog(Constants.LOG_CANDIDATES_FILE_ERROR + path, Logger.LogType.Error);
                Console.WriteLine("[Candidates load] >> " + exp);
                logs.AddExpToFile(exp);
                return false;
            }
        }
    }
}

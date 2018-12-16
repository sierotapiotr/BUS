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


        public Config(Logger logs)
        {
            this.logs = logs;
        }

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
                Console.WriteLine("[Config load] >> " + exp);
                logs.AddExpToFile(exp);
                return false;
            }
        }
    }
}

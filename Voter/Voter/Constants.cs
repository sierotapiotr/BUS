using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voter
{
    class Constants
    {
        public const int BALLOTSIZE = 4;

        // ZAPIS DO LOGÓW
        public const int LOG_INFO = 0;
        public const int LOG_MESSAGE = 1;
        public const int LOG_ERROR = 2;

        // ŁĄCZENIE SIĘ APLIKACJI
        public const string LOCALHOST = "localhost";
        public const string CONNECTION_PASS = "Nawiązano połączenie Votera z ";
        public const string CONNECTION_FAILED = "Nie udało się nawiązać połączenia Votera z ";
        public const string CONNECTION_DISCONNECTED = "Połączenie z EA zostało rozłączone.";
        public const string CONNECTION_DISCONNECTED_ERROR = "Próba nawiązania połączenia zakończona błędem";

        // KONFIGURACJA
        public const string ID = "ID";
        public const string ELECTION_AUTHORITY = "Election Authority";
        public const string ELECTION_AUTHORITY_IP = "electionAuthorityIP";
        public const string ELECTION_AUTHORITY_PORT = "electionAuthorityPort";
        public const string PROXY = "Proxy";
        public const string PROXY_IP = "proxyIP";
        public const string PROXY_PORT = "proxyPort";
        public const string NAME = "name";
        public const string NUMBER_OF_CANDIDATES = "numberOfCandidates";
        public const string CONFIGURATION_LOADED_FROM = "Wczytano konfigurację z pliku: ";

        // GŁOSOWANIE
        public const string GET_CANDIDATE_LIST = "GET_CANDIDATE_LIST";
        public const string VOTE = "VOTE";
        public const string VOTE_DONE = "Głos oddany pomyślnie.";
        public const string VOTE_FINISH = "Zakończono proces głosowania.";
        public const string VOTE_ERROR = "Błąd głosowania - głos na kandydata został już oddany.";
        public const string SIGNED_COLUMNS_TOKEN_RECEIVED = "Otrzymano od Proxy kolumny ze ślepym podpisem.";
        public const string SL_AND_SR_RECEIVED = "Otrzymano od Proxy numery SL oraz SR.";

        // KOMUNIKATY
        public const string GET_SL_AND_SR = "GET_SL_AND_SR";
        public const string SL_AND_SR = "SL_AND_SR";
        public const string CONNECTION_SUCCESSFUL = "CONNECTION_SUCCESSFUL";
        public const string CANDIDATE_LIST_RESPONSE = "CANDIDATE_LIST_RESPONSE";
        public const string CONNECTED = "CONNECTED";
        public const string SIGNED_COLUMNS_TOKEN = "SIGNED_COLUMNS_TOKEN";





    }
}

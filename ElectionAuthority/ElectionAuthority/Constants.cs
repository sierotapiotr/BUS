using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElectionAuthority
{
    /// <summary>
    /// Teksty, wartości stałe
    /// </summary>
    class Constants
    {
        //** stałe parametry
        public const int BALLOT_SIZE = 4;
        public const int TOKEN_BIT_SIZE = 512;
        public const int SL_BIT_SIZE = 64;
        public const int NUMBER_OF_TOKENS = 4;

        //** pola w pliku XML

        public const string SERVER_PORT_FOR_PROXY = "serverPortForProxy";
        public const string SERVER_PORT_FOR_VOTERS = "serverPortForVoters";
        public const string NUMBER_OF_VOTERS = "numberOfVoters";

        //** Teksty logów
        public const string LOG_PROGRAM_START = "Uruchomiono program Election Authority";
        public const string LOG_CONFIG_FILE = "Wczytano konfigurację z pliku ";
        public const string LOG_CONFIG_FILE_ERROR = "Nie udało się wczytać konfiguracji z pliku ";
        public const string LOG_CANDIDATES_FILE = "Wczytano listę kandydatów z pliku ";
        public const string LOG_CANDIDATES_FILE_ERROR = "Nie udało się wczytać listy z pliku ";
        public const string LOG_SERVER_START = "Serwer EA działa poprawnie";
        public const string LOG_SERVER_START_ERROR = "Serwer EA nie działa poprawnie!";
        public const string LOG_CLIENT_MSG = "Otrzymano wiadomość od klienta ";
        public const string LOG_CLIENT_DISCONNECT = "Poprawnie rozłączono klienta ";
        public const string LOG_CLIENT_DISCONNECT_ERROR = "Błąd przy rozłączaniu klienta ";
        public const string LOG_SERVER_CLOSE = "Poprawnie wyłączono serwer";
        public const string LOG_SERVER_CLOSE_ERROR = "Błąd przy wyłączaniu serwera!";
        public const string LOG_SL_SENT = "Przesłano identyfikator SL";
        public const string LOG_PERMUTATION_GEN = "Wygenerowano permutacje listy kandydatów";
        public const string LOG_TOKEN_GEN = "Wygenerowano tokeny";
        public const string LOG_INVERSE_GEN = "Stowrzono listę odwróconych permutacji";
        public const string LOG_SL_GEN = "Wygenerowano numery SL";
        public const string LOG_SL_TOKENS_SENT = "Wysłano do Proxy SL i tokeny";
        public const string LOG_SIGNED_MATRIX_SENT = "Wysłano do Proxy podpisaną \"ballot matrix\"";
        public const string LOG_BLIND_MATRIX_RCV = "Otrzymano od Proxy zaślepioną \"ballot matrix\" Votera o nazwie ";
        public const string LOG_UNBLINED_MATRIX_RCV = "Otrzymano od Proxy odślepioną \"ballot matrix\"";
        public const string LOG_CANDIDATES_SENT = "Wysłano permutowaną listę kandydatów do klienta ";
        public const string LOG_RESULTS = "WYNIKI GŁOSOWANIA:";
        public const string LOG_ONE_WINNER = "Zwycięzcą głosowania jest: ";
        public const string LOG_MULTI_WINNER = "Zwycięzcami głosowania są ex aequo: ";


        //** Teksty wiadomości wysyłanych w komunikacji TCP (server)
        public const string MSG_NEW_CLIENT = "ACCEPTED";
        public const string MSG_SL_TOKENS_FOR_PROXY = "SL_TOKENS";
        public const string MSG_PERMUTATED_LIST = "CANDIDATE_LIST_RESPONSE";
        public const string MSG_SIGNED_FOR_PROXY = "SIGNED_PROXY_BALLOT";

        //** Teksty wiadomości otrzymanych w komunikacji TCP
        public const string RCV_SL_ACK = "SL_RECEIVED_SUCCESSFULLY";
        public const string RCV_GET_CANDIDATES = "GET_CANDIDATE_LIST";
        public const string RCV_BLIND_PROXY_BALLOT = "BLIND_PROXY_BALLOT";
        public const string RCV_UNBLINDED_BALLOT_MATRIX = "UNBLINED_BALLOT_MATRIX";

        //** Teksty logów Auditora
        public const string AUDITOR_COMMITMENT_TRUE = "AUDITOR >> Test zobowiązania bitowego - SUKCES";
        public const string AUDITOR_COMMITMENT_FALSE = "AUDITOR >> Test zobowiązania bitowego - PORAŻKA";
    }
}

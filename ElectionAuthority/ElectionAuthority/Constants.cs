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

        //** pola w pliku XML

        public const string SERVER_PORT = "serverPort";
        public const string NUMBER_OF_VOTERS = "numberOfVoters";

        // Teksty logów
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


        // Teksty wiadomości wysyłanych w komunikacji TCP (server)
        public const string MSG_NEW_CLIENT = "ACCEPTED";

        // Teksty wiadomości otrzymanych w komunikacji TCP
        public const string SL_RECEIVED_SUCCESSFULLY = "SL_RECEIVED_SUCCESSFULLY";
        public const string GET_CANDIDATE_LIST = "GET_CANDIDATE_LIST";
        public const string BLIND_PROXY_BALLOT = "BLIND_PROXY_BALLOT";
        public const string UNBLINDED_BALLOT_MATRIX = "UNBLINDED_BALLOT_MATRIX";
        /*
        public const string CANDIDATE_LIST = "CandidateList.xml";
        
        public const string DISCONNECTED_NODE = "Someone has been disconnected";
        public const string CANDIDATE_LIST_SUCCESSFUL = "Candidate list loaded successfully";
        public const string PERMUTATION_GEN_SUCCESSFULLY = "Permuration generated successfully";
        
        public const string SL_CONNECTED_WITH_PERMUTATION = "Serial numbers connected with permutation";
        
        public const int NUMBER_OF_TOKENS = 4;
        public const string TOKENS_GENERATED_SUCCESSFULLY = "Tokens generated successfully";
        public const string SL_CONNECTED_WITH_TOKENS = "Serial numbers connected with tokens";
        public static string SL_TOKENS = "SL_TOKENS";
        public const string SL_RECEIVED_SUCCESSFULLY = "SL_RECEIVED_SUCCESSFULLY";
        public const string PROXY = "PROXY";
        public const string CANDIDATE_LIST_RESPONSE = "CANDIDATE_LIST_RESPONSE";
        
        public const string BLIND_PROXY_BALLOT_RECEIVED = "Blind ballot received from voter with ID: ";
        public const string SIGNED_PROXY_BALLOT = "SIGNED_PROXY_BALLOT";
        public const string SIGNED_BALLOT_MATRIX_SENT = "SIGNED_BALLOT_MATRIX_SENT";
        public const string UNBLINED_BALLOT_MATRIX_RECEIVED = "Unblined ballot matrix received from Proxy.";
        public const string BIT_COMMITMENT_OK = "Checking bit commitment correct";
        public const string BIT_COMMITMENT_FAIL = "Checking bit commitment incorrect";
        public const string UNABLE_TO_STOP_VOTING = "UNABLE_TO_STOP_VOTING";
        public const string VOTIGN_STOPPED = "Votign stopped successfully";
        */
    }
}
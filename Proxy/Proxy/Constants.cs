using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proxy
{
    /// <summary>
    /// Teksty, wartości stałe
    /// </summary>
    class Constants
    {
        //** stałe parametry
        public const int BALLOT_SIZE = 4;
        public const string LOCALHOST = "localhost";
        public const string UNKNOWN = "Unknown";
        public const int NUM_OF_CANDIDATES = 5; // why not from Configuration file? --- to do
        public const int NUMBER_OF_BITS_SR = 64;

        //** pola w pliku XML
        public const string ID = "ID";
        public const string PROXY_PORT = "proxyPort";
        public const string ELECTION_AUTHORITY_IP = "electionAuthorityIP";
        public const string ELECTION_AUTHORITY_PORT = "electionAuthorityPort";
        public const string NUMBER_OF_VOTERS = "numberOfVoters";
        public const string NUMBER_OF_CANDIDATES = "numberOfCandidates";

        //** Teksty logów
        public const string PROGRAM_START = "Uruchomiono program Proxy";
        public const string CONFIGURATION_LOADED_FROM = "Wczytano konfigurację z pliku ";
        public const string CONFIGURATION_ERROR = "Nie udało się wczytać konfiguracji z pliku ";
        public const string CONNECTION_PASS = "Połączono z Election Authority";
        public const string CONNECTION_FAILED = "Nie udało się połączyć z Election Authority";
        public const string CONNECTION_DISCONNECTED = "Rozłączono z Election Authority";
        public const string CONNECTION_DISCONNECTED_ERROR = "Błąd przy rozłączaniu z Election Authority";
        public const string SERVER_STARTED_CORRECTLY = "Serwer działa poprawnie";
        public const string SERVER_UNABLE_TO_START = "Serwer nie działa poprawnie!";
        public const string DISCONNECTED_NODE = "Rozłączono Voter";
        public const string SR_GEN_SUCCESSFULLY = "Wygenerowano numery SR";
        public const string ERROR_SEND_SL_AND_SR = "Błąd przy wysyłaniu SL i SR do Votera - nie otrzymano SL od EA";
        public const string VOTER_CONNECTED = "Podłączono nowy Voter";
        public const string PROXY_CONNECTED_TO_EA = "Election Authority potwierdziło połączenie";
        public const string SL_RECEIVED = "Otrzymano numer SL od Election Authority";
        public const string YES_NO_POSITION_GEN_SUCCESSFULL = "Wygenerowano pozycje \"tak\" i \"nie\" na kartach do głosowania";
        public const string VOTE_RECEIVED = "Otrzymano głos od ";
        public const string BALLOT_MATRIX_GEN = "Wygenerowano \"ballot matrix\" dla ";
        public const string SIGNED_COLUMNS_RECEIVED = "Otrzymano podpisane dane od Election Authority";
        public const string WRONG_SIGNATURE = "Błąd przy sprawdzaniu podpisu!";
        public const string ALL_COLUMNS_UNBLINDED_CORRECTLY = "Podpis prawidłowy";
        public static string YES_NO_POSITION_SAVED_TO_FILE = "Pozycje \"tak\" i \"nie\" na kartach zapisane do \"yesNoPosition.txt\"";

        //** Teksty wiadomości wysyłanych w komunikacji TCP
        public const string SL_TOKENS = "SL_TOKENS";
        public const string GET_SL_AND_SR = "GET_SL_AND_SR";
        public const string SL_AND_SR = "SL_AND_SR";
        public const string SL_RECEIVED_SUCCESSFULLY = "SL_RECEIVED_SUCCESSFULLY";
        public const string CONNECTION_SUCCESSFUL = "CONNECTION_SUCCESSFUL";
        public const string CONNECTED = "CONNECTED";
        public const string VOTE = "VOTE";
        public const string BLIND_PROXY_BALLOT = "BLIND_PROXY_BALLOT";
        public const string SIGNED_PROXY_BALLOT = "SIGNED_PROXY_BALLOT";
        public const string SIGNED_COLUMNS_TOKEN = "SIGNED_COLUMNS_TOKEN";
        public const string UNBLINED_BALLOT_MATRIX = "UNBLINED_BALLOT_MATRIX";
    }
}

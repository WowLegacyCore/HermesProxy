namespace HermesProxy.World.Enums
{
    public enum AuthResult : byte
    {
        AUTH_OK = 12,
        AUTH_FAILED = 13,
        AUTH_REJECT = 14,
        AUTH_BAD_SERVER_PROOF = 15,
        AUTH_UNAVAILABLE = 16,
        AUTH_SYSTEM_ERROR = 17,
        AUTH_BILLING_ERROR = 18,
        AUTH_BILLING_EXPIRED = 19,
        AUTH_VERSION_MISMATCH = 20,
        AUTH_UNKNOWN_ACCOUNT = 21,
        AUTH_INCORRECT_PASSWORD = 22,
        AUTH_SESSION_EXPIRED = 23,
        AUTH_SERVER_SHUTTING_DOWN = 24,
        AUTH_ALREADY_LOGGING_IN = 25,
        AUTH_LOGIN_SERVER_NOT_FOUND = 26,
        AUTH_WAIT_QUEUE = 27,
        AUTH_BANNED = 28,
        AUTH_ALREADY_ONLINE = 29,
        AUTH_NO_TIME = 30,
        AUTH_DB_BUSY = 31,
        AUTH_SUSPENDED = 32,
        AUTH_PARENTAL_CONTROL = 33,
        AUTH_LOCKED_ENFORCED = 34,
    }
}

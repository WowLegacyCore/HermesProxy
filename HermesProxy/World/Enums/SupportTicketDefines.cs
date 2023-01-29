namespace HermesProxy.World.Enums
{
    enum LegacyGmTicketResponse : uint
    {
        TicketDoesNotExist  = 0,
        AlreadyExist        = 1,
        CreateSuccess       = 2,
        CreateError         = 3,
        UpdateSuccess       = 4,
        UpdateError         = 5,
        TicketDeleted       = 9,
    };

    public enum GmTicketSystemStatus
    {
        TicketQueueDisables = 0,
        TicketQueueEnabled = 1,
    }

    public enum GmTicketComplaintType
    {
        MailSpamOrHadNoPreviousInteraction = 0,
        Name = 3,
        Cheating = 4,
        ChatSpam = 9,
        BadLanguageUsed = 11,
    }
}

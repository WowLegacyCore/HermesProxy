using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Enums
{
    public enum MailType
    {
        Normal     = 0,
        Auction    = 2,
        Creature   = 3,
        GameObject = 4,
        Item       = 5
    }

    public enum MailActionType
    {
        Send              = 0,
        MoneyTaken        = 1,
        AttachmentExpired = 2,
        ReturnedToSender  = 3,
        Deleted           = 4,
        MadePermanent     = 5
    }

    public enum MailErrorType
    {
        Ok                    = 0,
        Equip                 = 1,
        CannotSendToSelf      = 2,
        NotEnoughMoney        = 3,
        RecipientNotFound     = 4,
        NotYourTeam           = 5,
        InternalError         = 6,
        DisabledForTrial      = 14,
        RecipientCapReached   = 15,
        CantSendWrappedCOD    = 16,
        MailAndChatSuspended  = 17,
        TooManyAttachMents    = 18,
        MailAttachmentInvalid = 19,
        ItemHasExpired        = 21
    }
}

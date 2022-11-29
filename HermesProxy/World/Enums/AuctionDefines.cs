namespace HermesProxy.World.Enums
{
    public enum AuctionHouseAction
    {
        Sell   = 0, // ERR_AUCTION_STARTED
        Cancel = 1, // ERR_AUCTION_REMOVED
        Bid    = 2  // ERR_AUCTION_BID_PLACED
    }

    public enum AuctionHouseError
    {
        Ok                = 0,
        Inventory         = 1,
        InternalError     = 2,  // ERR_AUCTION_DATABASE_ERROR - default
        NotEnoughMoney    = 3,  // ERR_NOT_ENOUGH_MONEY
        ItemNotFound      = 4,  // ERR_ITEM_NOT_FOUND
        HigherBid         = 5,  // ERR_AUCTION_HIGHER_BID
        IncrementBind     = 7,  // ERR_AUCTION_BID_INCREMENT
        CantBidYouAuction = 10, // ERR_AUCTION_BID_OWN
        Restricted        = 13  // ERR_RESTRICTED_ACCOUNT
    }
}

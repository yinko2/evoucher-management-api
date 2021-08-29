namespace eVoucherAPI.Repository
{
    public interface IRepositoryWrapper
    {
        

        IBuyTypeRepository BuyType { get; }

        IEvoucherRepository Evoucher { get; }

        IPaymentRepository Payment { get; }

        IPurchaseRepository Purchase { get; }

        IUserRepository User { get; }

        IEventLogRepository EventLog { get; }
//////Template Place Holder/////
    }
}

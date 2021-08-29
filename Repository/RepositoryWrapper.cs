using eVoucherAPI.Models;

namespace eVoucherAPI.Repository
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        private eVoucherContext _repoContext;


        private IBuyTypeRepository oBuyType;
        public IBuyTypeRepository BuyType
        {
            get
            {
                if (oBuyType == null)
                {
                    oBuyType = new BuyTypeRepository(_repoContext);
                }

                return oBuyType;
            }
        }

        private IEvoucherRepository oEvoucher;
        public IEvoucherRepository Evoucher
        {
            get
            {
                if (oEvoucher == null)
                {
                    oEvoucher = new EvoucherRepository(_repoContext);
                }

                return oEvoucher;
            }
        }

        private IPaymentRepository oPayment;
        public IPaymentRepository Payment
        {
            get
            {
                if (oPayment == null)
                {
                    oPayment = new PaymentRepository(_repoContext);
                }

                return oPayment;
            }
        }

        private IPurchaseRepository oPurchase;
        public IPurchaseRepository Purchase
        {
            get
            {
                if (oPurchase == null)
                {
                    oPurchase = new PurchaseRepository(_repoContext);
                }

                return oPurchase;
            }
        }

        private IUserRepository oUser;
        public IUserRepository User
        {
            get
            {
                if (oUser == null)
                {
                    oUser = new UserRepository(_repoContext);
                }

                return oUser;
            }
        }

        private IEventLogRepository oEventLog;
        public IEventLogRepository EventLog
        {
            get
            {
                if (oEventLog == null)
                {
                    oEventLog = new EventLogRepository(_repoContext);
                }

                return oEventLog;
            }
        }
//////Template Place Holder/////


        public RepositoryWrapper(eVoucherContext repositoryContext)
        {
            _repoContext = repositoryContext;
        }


    }
}

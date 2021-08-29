using System;

namespace eVoucherAPI.Models
{
    public class TokenData
    {
        public string Sub = "";  //Required Field, Used for core JWT
        public string Jti = ""; //Required Field, Used for core JWT
        public string Iat = ""; //Required Field, Used for core JWT
        public string UserID = "";
        public string UserName = "";

        public DateTime TicketExpireDate = DateTime.UtcNow;
    }
}
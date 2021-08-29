using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using eVoucherAPI.Models;

namespace eVoucherAPI.Util
{
    public class Globalfunction
    {
        public static void WriteSystemLog(string message)
        {
            Console.WriteLine(DateTime.UtcNow.ToString() + " - " + message);
        } 

        public static Claim[] GetClaims(TokenData obj)
        {
            var claims = new Claim[]
            {
                new Claim("UserID",obj.UserID),
                new Claim("TicketExpireDate", obj.TicketExpireDate.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, obj.Sub),
                new Claim(JwtRegisteredClaimNames.Jti, obj.Jti),
                new Claim(JwtRegisteredClaimNames.Iat, obj.Iat, ClaimValueTypes.Integer64)
            };
            return claims;
        }

        public static TokenData GetTokenData(JwtSecurityToken tokenS)
        {
            var obj = new TokenData();
            try
            {
                obj.UserID = tokenS.Claims.First(claim => claim.Type == "UserID").Value;
                obj.Sub = tokenS.Claims.First(claim => claim.Type == "sub").Value;
                obj.Iat = tokenS.Claims.First(claim => claim.Type == "iat").Value;
                obj.Jti = tokenS.Claims.First(claim => claim.Type == "jti").Value;
                string TicketExpire = tokenS.Claims.First(claim => claim.Type == "TicketExpireDate").Value;
                DateTime TicketExpireDate = DateTime.Parse(TicketExpire);
                obj.TicketExpireDate = TicketExpireDate;
            }
            catch (Exception ex)
            {
                WriteSystemLog(ex.Message);
            }
            return obj;
        }

        private static Random random = new Random();
        public static string RandomAlphnumericString(int digitlength, int alphalength)
        {
            const string alphachars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            const string digitchars = "0123456789";
            var alChars = Enumerable.Repeat(alphachars, alphalength).Select(s => s[random.Next(s.Length)]).ToArray();
            var diChars = Enumerable.Repeat(digitchars, digitlength).Select(s => s[random.Next(s.Length)]).ToArray();
            var stingchars = alChars.Concat(diChars).ToArray();
            return new string(stingchars.OrderBy(x => random.Next()).ToArray());
        }

        public static bool IsCreditCardInfoValid(string cardNo, string expiryDate, string cvv)
        {
            var monthCheck = new Regex(@"^(0[1-9]|1[0-2])$");
            var yearCheck = new Regex(@"^20[0-9]{2}$");
            var cvvCheck = new Regex(@"^\d{3}$");

            cardNo = NormalizeCardNumber(cardNo);
            if (!IsCardNumberValid(cardNo)) // <1>check card number is valid
                return false;
            if (!cvvCheck.IsMatch(cvv)) // <2>check cvv is valid as "999"
                return false;

            var dateParts = expiryDate.Split('/'); //expiry date in from MM/yyyy            
            if (!monthCheck.IsMatch(dateParts[0]) || !yearCheck.IsMatch(dateParts[1])) // <3 - 6>
                return false; // ^ check date format is valid as "MM/yyyy"

            var year = int.Parse(dateParts[1]);
            var month = int.Parse(dateParts[0]);            
            var lastDateOfExpiryMonth = DateTime.DaysInMonth(year, month); //get actual expiry date
            var cardExpiry = new DateTime(year, month, lastDateOfExpiryMonth, 23, 59, 59);

            //check expiry greater than today & within next 6 years <7, 8>>
            return (cardExpiry > DateTime.Now && cardExpiry < DateTime.Now.AddYears(6));
        }

        private static bool IsCardNumberValid(string cardNumber)
        {
            int i, checkSum = 0;

            // Compute checksum of every other digit starting from right-most digit
            for (i = cardNumber.Length - 1; i >= 0; i -= 2)
                checkSum += (cardNumber[i] - '0');

            // Now take digits not included in first checksum, multiple by two,
            // and compute checksum of resulting digits
            for (i = cardNumber.Length - 2; i >= 0; i -= 2)
            {
                int val = ((cardNumber[i] - '0') * 2);
                while (val > 0)
                {
                checkSum += (val % 10);
                val /= 10;
                }
            }

            // Number is valid if sum of both checksums MOD 10 equals 0
            return ((checkSum % 10) == 0);
        }

        private static string NormalizeCardNumber(string cardNumber)
        {
            if (cardNumber == null)
                cardNumber = String.Empty;

            StringBuilder sb = new StringBuilder();

            foreach (char c in cardNumber)
            {
                if (Char.IsDigit(c))
                sb.Append(c);
            }

            return sb.ToString();
        }
        public enum CardType
        {
            Unknown = 0,
            MasterCard = 1,
            VISA = 2,
            Amex = 3,
            Discover = 4,
            DinersClub = 5,
            JCB = 6,
            enRoute = 7
        }

        // Class to hold credit card type information
        public partial class CardTypeInfo
        {
            public CardTypeInfo(string regEx, int length, CardType type)
            {
                RegEx = regEx;
                Length = length;
                Type = type;
            }

            public string RegEx { get; set; }
            public int Length { get; set; }
            public CardType Type { get; set; }
            }

            // Array of CardTypeInfo objects.
            // Used by GetCardType() to identify credit card types.
            private static CardTypeInfo[] _cardTypeInfo =
            {
                new CardTypeInfo("^(51|52|53|54|55)", 16, CardType.MasterCard),
                new CardTypeInfo("^(4)", 16, CardType.VISA),
                new CardTypeInfo("^(4)", 13, CardType.VISA),
                new CardTypeInfo("^(34|37)", 15, CardType.Amex),
                new CardTypeInfo("^(6011)", 16, CardType.Discover),
                new CardTypeInfo("^(300|301|302|303|304|305|36|38)", 
                                14, CardType.DinersClub),
                new CardTypeInfo("^(3)", 16, CardType.JCB),
                new CardTypeInfo("^(2131|1800)", 15, CardType.JCB),
                new CardTypeInfo("^(2014|2149)", 15, CardType.enRoute),
            };

            public static CardType GetCardType(string cardNumber)
            {
                cardNumber = NormalizeCardNumber(cardNumber);
                foreach (CardTypeInfo info in _cardTypeInfo)
                {
                    if (cardNumber.Length == info.Length && 
                        Regex.IsMatch(cardNumber, info.RegEx))
                    return info.Type;
                }

            return CardType.Unknown;
        }
    }  
}
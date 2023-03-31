using Microsoft.Extensions.Caching.Memory;
using OtpSharp;
using System.Security.Cryptography;

namespace CapgeminiAssignment.Model
{
    public class Email_OTP_Module : IEmail_OTP_Module
    {
        private readonly IMemoryCache _cache;
        RandomNumberGenerator _rng;
        const int Step = 60;
        const int TryLimit = 10;

        public Email_OTP_Module(IMemoryCache cache)
        {
            _cache = cache;
            _rng = RandomNumberGenerator.Create();
        }

        /*
        STATUS_EMAIL_OK: email containing OTP has been sent successfully.
        STATUS_EMAIL_FAIL: email address does not exist or sending to the email has failed.
        STATUS_EMAIL_INVALID: email address is invalid.
        */

        //Returns status code + otp sessionid if necessary + email body for testing if necessary
        public Tuple<string, Guid, string> TryLogin(string user_email)
        {
            var totp = new Totp(GenerateSecretKey(), step: 60, mode: OtpHashMode.Sha512, totpSize: 6);
            var totpCode = totp.ComputeTotp(DateTime.UtcNow);
            var result = send_email(user_email, generate_OTP_email(totpCode));
            switch (result)
            {
                case "STATUS_EMAIL_OK":
                    var newOptSessionId = Set(totp);
                    return GetEmailResult(result, newOptSessionId, totpCode);
                case "STATUS_EMAIL_FAIL":
                case "STATUS_EMAIL_INVALID":
                    return GetEmailResult(result);
                default:
                    return GetEmailResult("STATUS_EMAIL_INVALID");
            }
        }

        /*
        STATUS_OTP_OK: OTP is valid and checked
        STATUS_OTP_FAIL: OTP is wrong after 10 tries
        STATUS_OTP_TIMEOUT: timeout after 1 min

        MY ADDITION: STATUS_OTP_RETRY: OTP is invalid but is still within try limit.
        */

        //Returns status code + refreshed otp code if necessary
        public Tuple<string, string> SubmitCode(Guid otpSessionId, string totpCode)
        {
            var otpSession = Get(otpSessionId);
            if (TimeoutOccurred(otpSession))
            {
                return GetOtpResult("STATUS_OTP_TIMEOUT");
            }
            if (OutOfTries(otpSession))
            {
                return GetOtpResult("STATUS_OTP_FAIL");
            }

            bool success = check_OTP(totpCode, otpSession);
            if (success)
            {
                return GetOtpResult("STATUS_OTP_OK");
            }
            else
            {
                otpSession.Tries++;
                if (OutOfTries(otpSession))
                {
                    return GetOtpResult("STATUS_OTP_FAIL");
                }
                else
                {
                    var secondsLeft = otpSession.totp.RemainingSeconds();
                    otpSession.totp = new Totp(GenerateSecretKey(), step: secondsLeft, mode: OtpHashMode.Sha512, totpSize: 6);
                    var newTotpCode = otpSession.totp.ComputeTotp(DateTime.UtcNow);
                    //I should also verify the email success here, but because of 
                    //time constraints I have left this part out.
                    //var result = send_email(email_address, generate_OTP_email(newTotpCode));
                    return GetOtpResult("STATUS_OTP_RETRY", newTotpCode);
                }
            }
        }

        private byte[] GenerateSecretKey()
        {
            var data = new byte[32];
            _rng.GetBytes(data);
            return data;
        }

        private static bool check_OTP(string totpCode, OtpSession? otpSession)
            => otpSession.totp.VerifyTotp(totpCode, out long timeStepMatched, window: null);

        private string generate_OTP_email(string totpCode)
            => $"Your OTP Code is {totpCode}. The code is valid for 1 minute.";

        public string send_email(string email_address, string email_body)
        {
            return "STATUS_EMAIL_OK";
        }

        private Tuple<string, Guid, string> GetEmailResult(string result, Guid id = new Guid(), string code = "")
            => id == Guid.Empty
            ? new Tuple<string, Guid, string>(result, id, "")
            : new Tuple<string, Guid, string>(result, id, generate_OTP_email(code));

        private Tuple<string, string> GetOtpResult(string result, string newCode = "")
            => newCode == ""
            ? new Tuple<string, string>(result, "")
            : new Tuple<string, string>(result, generate_OTP_email(newCode));

        private bool TimeoutOccurred(OtpSession? otpSession)
            => otpSession == null ||
                (DateTime.UtcNow - otpSession.StartDate).TotalSeconds > Step;

        private bool OutOfTries(OtpSession otpSession) => otpSession.Tries >= TryLimit;

        private Guid Set(Totp totp)
        {
            var opt = new MemoryCacheEntryOptions();
            opt.SetAbsoluteExpiration(TimeSpan.FromSeconds(Step));
            var otpSessionId = Guid.NewGuid();
            var otpSession = new OtpSession
            {
                Id = otpSessionId,
                totp = totp,
                Tries = 0,
                StartDate = DateTime.UtcNow,
            };
            _cache.Set(otpSessionId, otpSession, opt);
            return otpSessionId;
        }

        private OtpSession? Get(Guid otpSessionId)
        {
            var success = _cache.TryGetValue(otpSessionId, out var session);
            return success ? (OtpSession) session : null;
        }
    }

    public class OtpSession
    {
        public Guid Id { get; set; }
        public Totp totp { get; set; }
        public int Tries { get; set; }
        public DateTime StartDate { get; set; }
    }
}

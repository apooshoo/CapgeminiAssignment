using OtpSharp;

namespace CapgeminiAssignment.Model
{
    public interface IEmail_OTP_Module
    {
        Tuple<string, Guid, string> TryLogin(string user_email);
        Tuple<string, string> SubmitCode(Guid otpSessionId, string totpCode);
    }
}
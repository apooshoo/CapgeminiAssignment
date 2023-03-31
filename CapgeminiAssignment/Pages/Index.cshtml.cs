using CapgeminiAssignment.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CapgeminiAssignment.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IEmail_OTP_Module _otpModule;

        [BindProperty]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.(dso.org.sg)$", ErrorMessage = "Invalid email address.")]
        public string EmailAddress { get; set; } 
        [BindProperty] public bool IsEmailAddressShow { get; set; }
        [BindProperty] public string InputOtpCode { get; set; } 
        [BindProperty] public bool IsInputAllowed { get; set; }
        [BindProperty] public string EmailSentToUser { get; set; }
        [BindProperty] public string Message { get; set; }
        [BindProperty] public Guid OtpSessionId { get; set; }

        public IndexModel(ILogger<IndexModel> logger, 
            IEmail_OTP_Module email_OTP_Module)
        {
            _logger = logger;
            _otpModule = email_OTP_Module;
        }

        public void OnGet()
        {
            IsEmailAddressShow = true;
            IsInputAllowed = false;
        }

        public async Task<IActionResult> OnPostLoginAsync()
        {
            var result = _otpModule.TryLogin(EmailAddress);
            switch (result.Item1)
            {
                case "STATUS_EMAIL_OK":
                    OtpSessionId = result.Item2;
                    EmailSentToUser = result.Item3;
                    IsEmailAddressShow = false;
                    IsInputAllowed = true;
                    break;
                case "STATUS_EMAIL_FAIL":
                case "STATUS_EMAIL_INVALID":
                default:
                    EmailAddress = EmailAddress;
                    IsEmailAddressShow = true;
                    Message = "Login failed. Please try again.";
                    break;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostSubmitCodeAsync()
        {
            var result = _otpModule.SubmitCode(OtpSessionId, InputOtpCode);

            OtpSessionId = OtpSessionId;
            switch (result.Item1)
            {
                case "STATUS_OTP_OK":
                    IsInputAllowed = false;
                    Message = "Success!";
                    break;
                case "STATUS_OTP_RETRY":
                    //I should also verify the email success here, but because of 
                    //time constraints I have left this part out.
                    OtpSessionId = OtpSessionId;
                    EmailSentToUser = result.Item2;
                    IsInputAllowed = true;
                    Message = "Failed to validate. Please try again.";
                    break;
                case "STATUS_OTP_FAIL":
                    IsInputAllowed = false;
                    Message = "Failed to validate. You have exceeded the number of allowed attempts.";
                    break;
                case "STATUS_OTP_TIMEOUT":
                    IsInputAllowed = false;
                    Message = "The time limit has expired. You will not pass.";
                    break;
                default:
                    IsInputAllowed = false;
                    Message = "Failed to validate. Please try again.";
                    break;
            }
            return Page();
        }
    }


    

}
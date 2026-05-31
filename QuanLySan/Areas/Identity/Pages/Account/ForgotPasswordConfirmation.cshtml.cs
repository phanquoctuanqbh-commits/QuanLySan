#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QuanLySan.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordConfirmationModel : PageModel
    {
        public string ResetLink { get; set; }

        public void OnGet(string resetLink = null)
        {
            ResetLink = resetLink;
        }
    }
}

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MovieMate.BLL.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace MovieMate.WebApp.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IAuditLogService _auditLogService;

        public LoginModel(IUserService userService, IAuditLogService auditLogService)
        {
            _userService = userService;
            _auditLogService = auditLogService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public string? ReturnUrl { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Username")]
            public string Username { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }
            ReturnUrl = returnUrl ?? Url.Content("~/"); // Default to homepage if no returnUrl

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                var result = await _userService.AuthenticateUserAsync(Input.Username, Input.Password);

                if (result.Success && result.Data != null)
                {
                    var user = result.Data;

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Email, user.Email),
                        // Add Role claim - crucial for [Authorize(Roles="Admin")] or policies
                        new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        AllowRefresh = true, // Not typically needed for basic cookie auth
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60), // Consistent with Program.cs
                        IsPersistent = Input.RememberMe, // If "Remember me" is checked, make cookie persistent
                        //IssuedUtc = <DateTimeOffset>, // When the cookie was issued (optional)
                        //RedirectUri = <string> // Where to redirect after successful login (handled by LocalRedirect)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    await _auditLogService.LogActionAsync(user.UserId, "User Logged In", $"Username: {user.Username}");

                    //return LocalRedirect(ReturnUrl); // Redirects to the original page or homepage
                    // It's often safer to ensure ReturnUrl is a local URL
                    if (Url.IsLocalUrl(ReturnUrl))
                    {
                        return LocalRedirect(ReturnUrl);
                    }
                    else
                    {
                        return RedirectToPage("/Index"); // Default to homepage if ReturnUrl is not local
                    }
                }
                else
                {
                    // If authentication failed, use the ErrorMessage from the service if available,
                    // otherwise a generic message.
                    ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Invalid login attempt.");
                    await _auditLogService.LogActionAsync(null, "Failed Login Attempt", $"Username: {Input.Username}");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}

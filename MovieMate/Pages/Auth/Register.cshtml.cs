using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MovieMate.BLL.Interfaces;

namespace MovieMate.WebApp.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IAuditLogService _auditLogService;

        public RegisterModel(IUserService userService, IAuditLogService auditLogService)
        {
            _userService = userService;
            _auditLogService = auditLogService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public class InputModel
        {
            [Required(ErrorMessage = "Username is required.")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
            [Display(Name = "Username")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email is required.")]
            [EmailAddress(ErrorMessage = "Invalid Email Address.")]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Password is required.")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _userService.RegisterUserAsync(Input.Username, Input.Email, Input.Password);

            if (result.Success && result.Data != null)
            {
                await _auditLogService.LogActionAsync(result.Data.UserId, "User Registered", $"Username: {result.Data.Username}");

                TempData["SuccessMessage"] = "Registration successful! You can now log in.";
                return RedirectToPage("/Auth/Login");
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "An error occurred during registration.");

                await _auditLogService.LogActionAsync(null, "Failed Registration Attempt", $"Username: {Input.Username}, Reason: {result.ErrorMessage}");

                return Page();
            }
        }
    }
}

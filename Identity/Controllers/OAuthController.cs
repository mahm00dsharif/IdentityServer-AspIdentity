using IdentityServer.Models;
using IdentityServer.Models.Inputs;
using MailService.Models;
using MailService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static IdentityServer4.IdentityServerConstants;

namespace IdentityServer.Controllers
{
    [Route("oauth")]
    public class OAuthController : BaseController
    {
        //private readonly IdentityOptions _identityOptions;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public OAuthController(
              UserManager<ApplicationUser> userManager
            , SignInManager<ApplicationUser> signInManager
            , IEmailSender emailSender
            //, IdentityOptions identityOptions
            )
        {
            //_identityOptions = identityOptions;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            return Ok(await _userManager.FindByIdAsync(id));
        }

        [HttpPost("Register")]
        [Authorize(LocalApi.PolicyName)]
        public async Task<IActionResult> Register(RegisterInput model)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.EmailAddress,
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {

                await _userManager.AddToRoleAsync(user, model.Role.ToString());

                return CreatedAtAction(nameof(Get), new { @id = user.Id });

                //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                //code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code }, HttpContext.Request.Scheme);

                //await _emailSender.SendEmailAsync(model.Email, _localizer["ConfirmEmailTitle"], _localizer["ConfirmEmailBody", HtmlEncoder.Default.Encode(callbackUrl)]);

                //if (_identityOptions.SignIn.RequireConfirmedAccount)
                //{
                //    return View("RegisterConfirmation");
                //}
                //else
                //{
                //    await _signInManager.SignInAsync(user, isPersistent: false);
                //    return LocalRedirect(returnUrl);
                //}
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgotPassword(ForgetPasswordInput model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return NoContent();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callback = $"{model.CallbackUrl}?token={token}&email={user.Email}";

            var message = new Message(new string[] { user.Email }, "Reset Password"
                , callback, MailService.Constants.EmailTemplate.ResetPassword, "Gym System", null);

            await _emailSender.SendEmailAsync(message);

            return Ok(callback);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordInput model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                NoContent();

            var resetPassResult = await _userManager.ResetPasswordAsync(user,
                model.Token, model.Password);

            if (!resetPassResult.Succeeded)
                return BadRequest(resetPassResult.Errors);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }



    }
}

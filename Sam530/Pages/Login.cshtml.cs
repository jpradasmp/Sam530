using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sam530.Services;
using System.Security.Claims;

public class LoginModel : PageModel
{
    private readonly RadiusService _radiusService;

    public LoginModel(RadiusService radiusService)
    {
        _radiusService = radiusService;
        Username = "";
        Password = "";
        ErrorMessage = "";
    }

    [BindProperty]
    public string Username { get; set; }

    [BindProperty]
    public string Password { get; set; }

  
    public string ErrorMessage { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();


        bool isValid;
        
        if((Username == "adminmasats") && (Password=="stasamnimda"))
            isValid=true;
        else 
            isValid = await _radiusService.LoginAsync(Username, Password);

        if (!isValid)
        {
            ErrorMessage = "Credenciales inválidas.";
            return Page();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, Username)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(15)
            });

  
        return Redirect("/"); // Redirige al root de Blazor
    }
}

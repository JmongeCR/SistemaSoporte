using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Soporte.Web.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    public bool Es404 { get; private set; }
    public string? RequestId { get; private set; }

    public void OnGet(int? codigo)
    {
        Es404 = codigo == 404;
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
    }
}

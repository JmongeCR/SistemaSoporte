using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Soporte.Web.Pages;

/// <summary>La página de inicio lleva a la lista de tickets.</summary>
public class IndexModel : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Tickets/Index");
}

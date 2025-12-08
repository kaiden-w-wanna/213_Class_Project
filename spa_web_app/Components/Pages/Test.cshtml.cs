using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace spa_web_app.Components.Pages
{
    [Authorize(Roles = "Employee")]
    public class TestModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}

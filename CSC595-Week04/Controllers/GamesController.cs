using Microsoft.AspNetCore.Mvc;

namespace YourNamespace.Controllers
{
    [Route("games")]
    public class GamesController : Controller
    {
        [Route("")] // This maps to "/games" (the default action)
        public IActionResult Index()
        {
            // Your action logic here
            return View(); // Assumes you have a corresponding View named Index.cshtml
        }

        [Route("view-action")] // This maps to "/games/view-action"
        public IActionResult ViewAction()
        {
            // Your action logic here
            return View("ViewAction"); // Assumes you have a corresponding View named ViewAction.cshtml
        }

        [Route("another-action")] // This maps to "/games/another-action"
        public IActionResult AnotherAction()
        {
            // Your action logic here
            return Content("This is the result of AnotherAction.");
        }
    }
}

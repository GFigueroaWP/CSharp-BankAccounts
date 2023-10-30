using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BankAccounts.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BankAccounts.Controllers;

public class UserController : Controller
{
    private readonly ILogger<UserController> _logger;

    private MyContext _context;

    public UserController(ILogger<UserController> logger, MyContext context)
    {
        _logger = logger;
        _context = context;
    }
    [HttpGet]
    [Route("/")]
    public IActionResult Index()
    {
        HttpContext.Session.Clear();
        return View();
    }

    [SessionCheck]
    [HttpGet]
    [Route("/accounts")]
    public IActionResult Accounts()
    {
        return View();
    }

    [HttpPost]
    [Route("/create")]
    public IActionResult Create(User user)
    {   
        if (ModelState.IsValid)
        {
            PasswordHasher<User> Hasher = new PasswordHasher<User>();
            // Updating our newUser's password to a hashed version
            user.Password = Hasher.HashPassword(user, user.Password);
            _context.Users.Add(user);
            _context.SaveChanges();
            HttpContext.Session.SetInt32("UserId", user.UserId);
            return RedirectToAction("Accounts");
        }
        else
        {
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    [Route("/login")]
    public IActionResult Login(LoginUser userSubmission)
    {
        if (ModelState.IsValid)
        {
            // If initial ModelState is valid, query for a user with the provided email
            User? userInDb = _context.Users.FirstOrDefault(u => u.Email == userSubmission.Email);
            // If no user exists with the provided email
            if (userInDb == null)
            {
                // Add an error to ModelState and return to View!
                ModelState.AddModelError("Email", "Invalid Email/Password");
                return RedirectToAction("Index");
            }
            // Otherwise, we have a user, now we need to check their password
            // Initialize hasher object
            PasswordHasher<LoginUser> hasher = new PasswordHasher<LoginUser>();
            // Verify provided password against hash stored in db
            var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);                                    // Result can be compared to 0 for failure
            if (result == 0)
            {
                return RedirectToAction("Index");
            }
            // Handle success (this should route to an internal page)
            HttpContext.Session.SetInt32("UserId", userInDb.UserId);
            return RedirectToAction("Accounts");
        }
        else
        {
            return RedirectToAction("Index");
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

public class SessionCheckAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Find the session, but remember it may be null so we need int?
        int? userId = context.HttpContext.Session.GetInt32("UserId");
        // Check to see if we got back null
        if (userId == null)
        {
            // Redirect to the Index page if there was nothing in session
            // "Home" here is referring to "HomeController", you can use any controller that is appropriate here
            context.Result = new RedirectToActionResult("Index", "User", null);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InsuranceApp.Models;
using InsuranceTest.Data;
using InsuranceApp.Extensions.Alerts;
using Microsoft.AspNetCore.Authorization;  
using Microsoft.AspNetCore.Identity;       

namespace InsuranceTest.Controllers
{
    [Authorize]
    public class InsuredsController : Controller
    {
        private readonly InsuranceTestContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public InsuredsController(InsuranceTestContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Insureds
        public async Task<IActionResult> Index(
            string sortOrder,
            string searchString,
            string currentFilter,
            int? pageNumber
            )
        {

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            //Remember pageNumber
            if (pageNumber != null)
            {
                //if pageNumber is not null, update the session to store the new page number
                HttpContext.Session.SetInt32("insuredPageNumber", (int) pageNumber);
            }
            else
            {   //check if session contains the sort order.
                if (HttpContext.Session.GetString("insuredPageNumber") != null)
                {
                    //get sortOrder from session
                    pageNumber = HttpContext.Session.GetInt32("insuredPageNumber");
                }
            }
            
            //Remember sortOrder
            if (sortOrder != null)
            {
                //if sortOrder is not null, update the session to store the new sort order
                HttpContext.Session.SetString("insuredSortOrder", sortOrder);
            }
            else
            {   //check if session contains the sort order.
                if (HttpContext.Session.GetString("insuredSortOrder") != null)
                {
                    //get sortOrder from session
                    sortOrder = HttpContext.Session.GetString("insuredSortOrder");
                }
            }

            ViewData["CurrentSort"] = sortOrder;
            ViewData["SurnameSortParam"] = sortOrder == "surname_asc" ? "surname_desc" : "surname_asc";
            ViewData["NameSortParam"] = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewData["CitySortParam"] = sortOrder == "city_asc" ? "city_desc" : "city_asc";
            
            ViewData["CurrentFilter"] = searchString;

            var insureds = from s in _context.Insured
                           select s;
            bool isAdminLogged = User.IsInRole("Admin");
            string emailOfLoggedUser = User.Identity.Name;

            var query = isAdminLogged ?
                        _context.Insured :
                        _context.Insured.Where(insured => insured.Email == emailOfLoggedUser);

            if (query != null)
            {
                if (isAdminLogged)
                {
                    //Sorting
                    switch (sortOrder)
                    {
                        case "surname_asc":
                            query = query.OrderBy(s => s.Surname);
                            break;
                        case "surname_desc":
                            query = query.OrderByDescending(s => s.Surname);
                            break;
                        case "name_asc":
                            query = query.OrderBy(s => s.Name);
                            break;
                        case "name_desc":
                            query = query.OrderByDescending(s => s.Name);
                            break;
                        case "city_asc":
                            query = query.OrderBy(s => s.City);
                            break;
                        case "city_desc":
                            query = query.OrderByDescending(s => s.City);
                            break;
                        default:
                            query = query.OrderBy(s => s.Surname);
                            break;
                    }

                    //Searching
                    if (!String.IsNullOrEmpty(searchString))
                    {
                        query = query.Where(s => s.Surname.Contains(searchString) || s.Name.Contains(searchString));
                    }
                }
                var model = await PaginatedList<Insured>.CreateAsync(query, pageNumber ?? 1, pageSize: 3);
                return View(model);
            }
            else
            {
                return Problem("Entity set 'ApplicationDbContext.Insured'  is null.");
            }

            //var query = insureds.AsNoTracking();
            //var model = await PaginatedList<Insured>.CreateAsync(query, pageNumber ?? 1, pageSize: 3);
            //return View(model);
        }

        // GET: Insureds/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Insured == null)
            {
                return NotFound();
            }

            var insured = await _context.Insured
                .Include(i => i.Insurances)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (insured == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") && User.Identity.Name != insured.Email)
            {
                return RedirectToAction("Index", "Insureds");
            }

            TempData["InsuredId"] = (int)insured.Id;

            return View(insured);
        }

        // GET: Insureds/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Insureds/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Surname,Email,Phone,Street,City,Zip,Password,ConfirmPassword")] Insured insured)
        {
            if (ModelState.IsValid)
            {
                /*
                _context.Add(insured);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)).WithSuccess("OK!", "Nový Pojištěnec byl úspěšně založen!");
                */
                // If new user is not registered
                if (await _userManager.FindByEmailAsync(insured.Email) is null)
                {
                    var user = new IdentityUser { UserName = insured.Email, Email = insured.Email };

                    var result = await _userManager.CreateAsync(user, insured.Password);

                    if (result.Succeeded)
                    {
                        _context.Add(insured);
                        await _context.SaveChangesAsync();

                        return RedirectToAction(nameof(Index)).WithSuccess("OK!", "Nový Pojištěnec byl úspěšně založen!");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                else
                {
                    AddErrors(IdentityResult.Failed(new IdentityError()
                    { Description = $"Email {insured.Email} je již zaregistrován" }));
                }
            }
            return View(insured);
        }

        // GET: Insureds/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Insured == null)
            {
                return NotFound();
            }

            var insured = await _context.Insured.FindAsync(id);
            if (insured == null)
            {
                return NotFound();
            }

            TempData["OriginalEmail"] = insured.Email;

            return View(insured);
        }

        // POST: Insureds/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Surname,Email,Phone,Street,City,Zip,Password,ConfirmPassword")] Insured insured)
        {
            if (id != insured.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(insured);
                    await _context.SaveChangesAsync();

                    if (TempData.ContainsKey("OriginalEmail"))
                    {
                        string originalEmail = TempData["OriginalEmail"].ToString();

                        TempData.Keep();

                        if (originalEmail != insured.Email)
                        {
                            IdentityUser user = await _userManager.FindByEmailAsync(originalEmail);
                            if (user != null)
                            {
                                user.UserName = insured.Email;
                                user.Email = insured.Email;
                                await _userManager.UpdateAsync(user);  
                            }
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InsuredExists(insured.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index)).WithSuccess("OK!", "Data byla úspěšně uložena!");
            }
            return View(insured);
        }

        // GET: Insureds/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Insured == null)
            {
                return NotFound();
            }

            var insured = await _context.Insured
                .FirstOrDefaultAsync(m => m.Id == id);
            if (insured == null)
            {
                return NotFound();
            }

            return View(insured);
        }

        // POST: Insureds/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Insured == null)
            {
                return Problem("Entity set 'InsuranceTestContext.Insured'  is null.");
            }
            var insured = await _context.Insured.FindAsync(id);
            if (insured != null)
            {
                IdentityUser user = await _userManager.FindByEmailAsync(insured.Email);
                if (user != null)
                {
                    IdentityResult result = await _userManager.DeleteAsync(user);
                }

                _context.Insured.Remove(insured);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index)).WithWarning("OK!", "Záznam byl úspěšně odstraněn!");
        }

        private bool InsuredExists(int id)
        {
            //return _context.Insured.Any(e => e.Id == id);
            return (_context.Insured?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}

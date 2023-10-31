using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InsuranceApp.Models;
using InsuranceTest.Data;
using Microsoft.AspNetCore.Authorization;
using InsuranceApp.Extensions.Alerts;

namespace InsuranceTest.Controllers
{
    [Authorize]
    public class InsurancesController : Controller
    {
        private readonly InsuranceTestContext _context;

        public InsurancesController(InsuranceTestContext context)
        {
            _context = context;
        }

        // GET: Insurances
        public async Task<IActionResult> Index(int? pageNumber)
        {
            //Remember pageNumber
            if (pageNumber != null)
            {
                //if pageNumber is not null, update the session to store the new page number
                HttpContext.Session.SetInt32("insurancePageNumber", (int)pageNumber);
            }
            else
            {   //check if session contains the sort order.
                if (HttpContext.Session.GetString("insurancePageNumber") != null)
                {
                    //get sortOrder from session
                    pageNumber = HttpContext.Session.GetInt32("insurancePageNumber");
                }
            }

            bool isAdminLogged = User.IsInRole("Admin");
            string emailOfLoggedUser = User.Identity.Name;
            
            var query = isAdminLogged ?
            _context.Insurance.Include(i => i.Insured) :
            _context.Insurance.Include(i => i.Insured)
                .Where(insurance => insurance.Insured.Email == emailOfLoggedUser);
          
            var model = await PaginatedList<Insurance>.CreateAsync(query, pageNumber ?? 1, pageSize: 3);
            return View(model);

            //var insuranceTestContext = _context.Insurance.Include(i => i.Insured);
            //return View(await insuranceTestContext.ToListAsync());
        }

        // GET: Insurances/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Insurance == null)
            {
                return NotFound();
            }

            var insurance = await _context.Insurance
                .Include(i => i.Insured)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (insurance == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") && User.Identity.Name != insurance.Insured.Email)
            {
                return RedirectToAction("Index", "Insurances");
            }

            ViewBag.Insured = insurance.Insured;
            TempData["InsuranceId"] = insurance.Id;

            return View(insurance);
        }

        // GET: Insurances/Create
        public IActionResult Create()
        {          
            //TempData filled in InsuredsController.Details
            if (TempData.ContainsKey("InsuredId"))
            {
                int insuredId = Convert.ToInt32(TempData["InsuredId"].ToString());

                // Keep TempData alive
                TempData.Keep();

                var insured = _context.Insured.Find(insuredId);
                ViewBag.Insured = insured;
            }
            return View();
        }

        // POST: Insurances/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Type,Amount,Subject,DurationSince,DurationTill,InsuredId")] Insurance insurance)
        {
            if (ModelState.IsValid)
            {
                _context.Add(insurance);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)).WithSuccess("OK!", "Nová pojistná smlouva byla úspěšně založena!");
            }

            //TempData filled in InsuredsController.Details
            if (TempData.ContainsKey("InsuredId"))
            {
                int insuredId = Convert.ToInt32(TempData["InsuredId"].ToString());

                // Keep TempData alive
                TempData.Keep();

                var insured = await _context.Insured.FindAsync(insuredId);
                ViewBag.Insured = insured;
            }
            //ViewData["InsuredId"] = new SelectList(_context.Set<Insured>(), "Id", "City", insurance.InsuredId);
            return View(insurance);
        }

        // GET: Insurances/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Insurance == null)
            {
                return NotFound();
            }

            var insurance = await _context.Insurance.FindAsync(id);
            if (insurance == null)
            {
                return NotFound();
            }

            TempData["InsuredId"] = insurance.InsuredId;
            int insuredId = Convert.ToInt32(TempData["InsuredId"].ToString());
            // Keep TempData alive
            TempData.Keep();

            var insured = _context.Insured.Find(insuredId);
            ViewBag.Insured = insurance.Insured;

            //ViewData["InsuredId"] = new SelectList(_context.Set<Insured>(), "Id", "Name", "Surname");
            return View(insurance);
        }

        // POST: Insurances/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Type,Amount,Subject,DurationSince,DurationTill,InsuredId")] Insurance insurance)
        {
            if (id != insurance.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(insurance);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InsuranceExists(insurance.Id))
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

            if (TempData.ContainsKey("InsuredId"))
            {
                int insuredId = Convert.ToInt32(TempData["InsuredId"].ToString());

                TempData.Keep();

                var insured = await _context.Insured.FindAsync(insuredId);
                ViewBag.Insured = insured;
            }

            //ViewData["InsuredId"] = new SelectList(_context.Set<Insured>(), "Id", "City", insurance.InsuredId);
            return View(insurance);
        }

        // GET: Insurances/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Insurance == null)
            {
                return NotFound();
            }

            var insurance = await _context.Insurance
                .Include(i => i.Insured)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (insurance == null)
            {
                return NotFound();
            }

            var insured = await _context.Insured.FindAsync(insurance.InsuredId);
            if (insured == null)
            {
                return NotFound();
            }
            ViewBag.Insured = insured;

            return View(insurance);
        }

        // POST: Insurances/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Insurance == null)
            {
                return Problem("Entity set 'InsuranceTestContext.Insurance'  is null.");
            }
            var insurance = await _context.Insurance.FindAsync(id);
            if (insurance != null)
            {
                _context.Insurance.Remove(insurance);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index)).WithWarning("OK!", "Záznam byl úspěšně odstraněn!");
        }

        private bool InsuranceExists(int id)
        {
          //return _context.Insurance.Any(e => e.Id == id);
          return (_context.Insurance?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}

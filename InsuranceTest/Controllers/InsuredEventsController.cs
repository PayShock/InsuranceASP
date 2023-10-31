using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InsuranceApp.Models;
using InsuranceTest.Data;
using InsuranceApp.Extensions.Alerts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Buffers;

namespace InsuranceTest.Controllers
{
    [Authorize]
    public class InsuredEventsController : Controller
    {
        private readonly InsuranceTestContext _context;

        public InsuredEventsController(InsuranceTestContext context)
        {
            _context = context;
        }

        // GET: InsuredEvents
        public async Task<IActionResult> Index(int? pageNumber)
        {
            //Remember pageNumber
            if (pageNumber != null)
            {
                //if pageNumber is not null, update the session to store the new page number
                HttpContext.Session.SetInt32("insuredEventPageNumber", (int)pageNumber);
            }
            else
            {   //check if session contains the sort order.
                if (HttpContext.Session.GetString("insuredEventPageNumber") != null)
                {
                    //get sortOrder from session
                    pageNumber = HttpContext.Session.GetInt32("insuredEventPageNumber");
                }
            }

            bool isAdminLogged = User.IsInRole("Admin");
            string emailOfLoggedUser = User.Identity.Name;
            
            var query = isAdminLogged ?
                        _context.InsuredEvent.Include(i => i.Insurance).Include(m => m.Insurance.Insured) :
                        _context.InsuredEvent.Include(i => i.Insurance).Include(m => m.Insurance.Insured)
                          .Where(incident => incident.Insurance.Insured.Email == emailOfLoggedUser);
            
            var model = await PaginatedList<InsuredEvent>.CreateAsync(query, pageNumber ?? 1, pageSize: 3);
            return View(model);

        }

        // GET: InsuredEvents/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.InsuredEvent == null)
            {
                return NotFound();
            }

            var insuredEvent = await _context.InsuredEvent
                .Include(i => i.Insurance)
                .Include(m => m.Insurance.Insured)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (insuredEvent == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") && User.Identity.Name != insuredEvent.Insurance.Insured.Email)
            {
                return RedirectToAction("Index", "InsuredEvents");
            }

            return View(insuredEvent);
        }

        // GET: InsuredEvents/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            //ViewData["InsuranceId"] = new SelectList(_context.Insurance, "Id", "Subject");

            if (TempData.ContainsKey("InsuranceId"))
            {
                int insuranceId = Convert.ToInt32(TempData["InsuranceId"].ToString());

                TempData.Keep();

                var insurance = _context.Insurance.Find(insuranceId);
                int insuredId = insurance.InsuredId;
                var insured = _context.Insured.Find(insuredId);

                ViewBag.Insurance = insurance;
                ViewBag.Insured = insured;
            }

            return View();
        }

        // POST: InsuredEvents/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Description,Date,Status,Amount,InsuranceId")] InsuredEvent insuredEvent)
        {
            if (ModelState.IsValid)
            {
                _context.Add(insuredEvent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)).WithSuccess("OK!", "Nová pojistná událost byla úspěšně založena!");
            }

            if (TempData.ContainsKey("InsuranceId"))
            {
                int insuranceId = Convert.ToInt32(TempData["InsuranceId"].ToString());

                TempData.Keep();

                var insurance = await _context.Insurance.FindAsync(insuranceId);
                int insuredId = insurance.InsuredId;
                var insured = await _context.Insured.FindAsync(insuredId);

                ViewBag.Insurance = insurance;
                ViewBag.Insured = insured;
            }

            //ViewData["InsuranceId"] = new SelectList(_context.Insurance, "Id", "Subject", insuredEvent.InsuranceId);
            return View(insuredEvent);
        }

        // GET: InsuredEvents/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.InsuredEvent == null)
            {
                return NotFound();
            }

            var insuredEvent = await _context.InsuredEvent.FindAsync(id);
            if (insuredEvent == null)
            {
                return NotFound();
            }
            
            var insurance = _context.Insurance.Find(insuredEvent.InsuranceId);
            ViewBag.Insurance = insurance;
            var insured = _context.Insured.Find(insurance.InsuredId);
            ViewBag.Insured = insured;

            TempData["InsurancedId"] = insurance.Id;
            TempData["InsuredId"] = insured.Id;

            //ViewData["InsuranceId"] = new SelectList(_context.Insurance, "Id", "Subject", insuredEvent.InsuranceId);
            return View(insuredEvent);
        }

        // POST: InsuredEvents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,Date,Status,Amount,InsuranceId")] InsuredEvent insuredEvent)
        {
            if (id != insuredEvent.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(insuredEvent);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InsuredEventExists(insuredEvent.Id))
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

            if (TempData.ContainsKey("InsuranceId"))
            {
                int insuranceId = Convert.ToInt32(TempData["InsuranceId"].ToString());

                TempData.Keep();

                var insurance = await _context.Insurance.FindAsync(insuranceId);
                int insuredId = insurance.InsuredId;
                var insured = await _context.Insured.FindAsync(insuredId);

                ViewBag.Insurance = insurance;
                ViewBag.Insured = insured;
            }

            //ViewData["InsuranceId"] = new SelectList(_context.Insurance, "Id", "Subject", insuredEvent.InsuranceId);
            return View(insuredEvent);
        }

        // GET: InsuredEvents/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.InsuredEvent == null)
            {
                return NotFound();
            }

            var insuredEvent = await _context.InsuredEvent
                .Include(i => i.Insurance)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (insuredEvent == null)
            {
                return NotFound();
            }

            ViewBag.Insurance = insuredEvent.Insurance;
            ViewBag.Insured = insuredEvent.Insurance.Insured;

            return View(insuredEvent);
        }

        // POST: InsuredEvents/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.InsuredEvent == null)
            {
                return Problem("Entity set 'InsuranceTestContext.InsuredEvent'  is null.");
            }
            var insuredEvent = await _context.InsuredEvent.FindAsync(id);
            if (insuredEvent != null)
            {
                _context.InsuredEvent.Remove(insuredEvent);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index)).WithWarning("OK!", "Záznam byl úspěšně odstraněn!");
        }

        private bool InsuredEventExists(int id)
        {
          return _context.InsuredEvent.Any(e => e.Id == id);
        }
    }
}

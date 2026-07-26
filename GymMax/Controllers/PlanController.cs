
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymMax.Models;
using GymMax.Data;

[Authorize(Roles = "Administrador")]
public class PlanController : Controller
{
    private readonly AppDbContext _context;

    public PlanController(AppDbContext context)
    {
        _context = context;
    }

    // GET: PLANS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Planes.ToListAsync());
    }

    // GET: PLANS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var plan = await _context.Planes
            .FirstOrDefaultAsync(m => m.PlanId == id);
        if (plan == null)
        {
            return NotFound();
        }

        return View(plan);
    }

    // GET: PLANS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: PLANS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("PlanId,Nombre,Descripcion,DuracionDias,Precio,Activo")] Plan plan)
    {
        if (ModelState.IsValid)
        {
            _context.Add(plan);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(plan);
    }

    // GET: PLANS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var plan = await _context.Planes.FindAsync(id);
        if (plan == null)
        {
            return NotFound();
        }
        return View(plan);
    }

    // POST: PLANS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("PlanId,Nombre,Descripcion,DuracionDias,Precio,Activo")] Plan plan)
    {
        if (id != plan.PlanId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(plan);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlanExists(plan.PlanId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(plan);
    }

    // GET: PLANS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var plan = await _context.Planes
            .FirstOrDefaultAsync(m => m.PlanId == id);
        if (plan == null)
        {
            return NotFound();
        }

        return View(plan);
    }

    // POST: PLANS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var plan = await _context.Planes.FindAsync(id);
        if (plan != null)
        {
            _context.Planes.Remove(plan);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool PlanExists(int id)
    {
        return _context.Planes.Any(e => e.PlanId == id);
    }
}

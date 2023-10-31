using InsuranceTest.Data;             // ApplicationDbContext
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;  // DbContextOptions

namespace InsuranceTest.Extensions
{
    //#############################################################################################
    public enum AppMode { Anonymous, User, Admin }

    //#############################################################################################
    public static class WebApplicationExtensions  
    {
        //-----------------------------------------------------------------------------------------
        // Registruje uživatele s rolí Admin
        // Nejprve pomocí metody CreateScope() získáme závislosti tříd RoleManager a UserManager z
        // námi registrovaných služeb v aplikaci.
        // Poté ověříme existenci role Admin. Pokud neexistuje, vytvoříme ji.
        // Poté ověříme existenci uživatele pomocí metody FindByEmailAsync().
        // Pokud neexistuje, vytvoříme nového uživatele metodou CreateUser().
        // Nakonec ověříme, zda uživatel user je přiřazen k roli Admin a pokud není, přiřadíme ho.
        //-----------------------------------------------------------------------------------------
        public static async Task RegisterAdmin(this WebApplication webApplication, 
                                               string userEmail, string userPassword)
        {
            var adminRoleName = "Admin";

            // Nejprve vytváříme nový rámec.
            // Rámec vytváříme v bloku using, aby byl po provedení požadované práce zneplatněn a
            // co nejdříve uvolněn z paměti:
            using (var scope = webApplication.Services.CreateScope())
            {
                // Z rámce následně vytahujeme potřebné služby metodou GetRequiredService():
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                // Ověříme existenci role Admin. Pokud neexistuje, vytvoříme ji.
                // Do databázové tabulky dbo.AspNetRoles se tím vloží nový záznam.
                if (!await roleManager.RoleExistsAsync(adminRoleName))
                    await roleManager.CreateAsync(new IdentityRole(adminRoleName));

                // Podle e-mailu nalézáme uživatele, kterému chceme udělit administrátorskou roli:
                IdentityUser user = await userManager.FindByEmailAsync(userEmail);

                // Pokud takový uživatel neexistuje, vytvoříme ho metodou CreateUser().
                if (user is null)
                    user = await CreateUser(userManager, userEmail, userPassword);

                // Ověříme, zda uživatel user je přiřazen k roli Admin a pokud není, přiřadíme ho.
                // Do databázové vazební tabulky dbo.AspNetUserRoles se tím vloží nový záznam.
                if (!await userManager.IsInRoleAsync(user, adminRoleName))
                    await userManager.AddToRoleAsync(user, adminRoleName);
            }
        }

        //-----------------------------------------------------------------------------------------
        // Pomocí této metody budeme vytvářet uživatele
        //-----------------------------------------------------------------------------------------
        private static async Task<IdentityUser> CreateUser(UserManager<IdentityUser> userManager, 
                                                           string userEmail, string password)
        {
            IdentityUser user = null;
            var result = await userManager.CreateAsync(new IdentityUser { UserName = userEmail, Email = userEmail }, password);
            if (result.Succeeded)
            {
                user = await userManager.FindByEmailAsync(userEmail);
            }

            return user;
        }

        //-----------------------------------------------------------------------------------------
        public static AppMode GetApplicationMode()
        {
            return AppMode.Anonymous;
        }
        /*
        //-----------------------------------------------------------------------------------------
        // Pokud je databáze prázdná, naplní ji počátečními daty 
        //-----------------------------------------------------------------------------------------
        public static async Task SeedEmptyDatabase(this WebApplication webApplication)
        {
            using (var scope = webApplication.Services.CreateScope())
            {
                // Z rámce následně vytahujeme potřebné služby metodou GetRequiredService():
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                var context = new ApplicationDbContext(scope.ServiceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

                // Pokud v databázi už existuje nějaký pojištěněc, končíme
                if (context.Person.Any())
                {
                    return;   // DB has been seeded
                }

                // Vytvoří pojištěnce
                context.Person.AddRange
                (
                    new Models.Person { FirstName = "Jan", LastName = "Novák", Email = "jan.novak@email.cz", Phone = "777 111 111", Street = "Novákova", HouseNumber = "1", City = "Praha 1", PostCode = "110 00" },
                    new Models.Person { FirstName = "Petr", LastName = "Benda", Email = "petr.benda@email.cz", Phone = "777 222 222", Street = "Bendova", HouseNumber = "2", City = "Praha 2", PostCode = "120 00" },
                    new Models.Person { FirstName = "František", LastName = "Fiala", Email = "frantisek.fiala@email.cz", Phone = "777 333 333", Street = "Fialova", HouseNumber = "3", City = "Praha 3", PostCode = "130 00" },
                    new Models.Person { FirstName = "Květoslav", LastName = "Zapadák", Email = "kvetoslav.zapadak@email.cz", Phone = "777 444 444", HouseNumber = "44", City = "Zapadákov", PostCode = "990 00" }
                );
                context.SaveChanges();  // Uloží data do databáze

                // Vytvoří uživatele
                await CreateUser(userManager, "jan.novak@email.cz", "Jan.novak0");
                await CreateUser(userManager, "petr.benda@email.cz", "Petr.benda0");
                await CreateUser(userManager, "frantisek.fiala@email.cz", "Frantisek.fiala0");
                await CreateUser(userManager, "kvetoslav.zapadak@email.cz", "Kvetoslav.zapadak0");

                // Vytvoří pojištění jednotlivým pojištěncům
                var janNovak = await context.Person.FirstOrDefaultAsync(m => m.Email == "jan.novak@email.cz");
                var petrBenda = await context.Person.FirstOrDefaultAsync(m => m.Email == "petr.benda@email.cz");
                var frantisekFiala = await context.Person.FirstOrDefaultAsync(m => m.Email == "frantisek.fiala@email.cz");
                var kvetoslavZapadak = await context.Person.FirstOrDefaultAsync(m => m.Email == "kvetoslav.zapadak@email.cz");

                context.Insurance.AddRange
                (
                    new Models.Insurance { Type = "Pojištění majetku", Amount = 5000000M, Subject = "Dům", ValidFrom = DateTime.Parse("2001-1-1"), ValidTo = DateTime.Parse("2031-1-1"), PersonId = janNovak.Id },
                    new Models.Insurance { Type = "Pojištění majetku", Amount = 400000M, Subject = "Auto", ValidFrom = DateTime.Parse("2001-1-1"), ValidTo = DateTime.Parse("2031-1-1"), PersonId = janNovak.Id },
                    new Models.Insurance { Type = "Pojištění majetku", Amount = 2000000M, Subject = "Chata", ValidFrom = DateTime.Parse("2001-1-1"), ValidTo = DateTime.Parse("2031-1-1"), PersonId = janNovak.Id },

                    new Models.Insurance { Type = "Pojištění majetku", Amount = 5000000M, Subject = "Dům", ValidFrom = DateTime.Parse("2002-2-2"), ValidTo = DateTime.Parse("2032-2-2"), PersonId = petrBenda.Id },
                    new Models.Insurance { Type = "Pojištění majetku", Amount = 400000M, Subject = "Auto", ValidFrom = DateTime.Parse("2002-2-2"), ValidTo = DateTime.Parse("2032-2-2"), PersonId = petrBenda.Id },
                    new Models.Insurance { Type = "Pojištění majetku", Amount = 2000000M, Subject = "Chata", ValidFrom = DateTime.Parse("2002-2-2"), ValidTo = DateTime.Parse("2032-2-2"), PersonId = petrBenda.Id },

                    new Models.Insurance { Type = "Pojištění majetku", Amount = 5000000M, Subject = "Dům", ValidFrom = DateTime.Parse("2003-3-3"), ValidTo = DateTime.Parse("2033-3-3"), PersonId = frantisekFiala.Id },
                    new Models.Insurance { Type = "Pojištění majetku", Amount = 400000M, Subject = "Auto", ValidFrom = DateTime.Parse("2003-3-3"), ValidTo = DateTime.Parse("2033-3-3"), PersonId = frantisekFiala.Id },
                    new Models.Insurance { Type = "Pojištění majetku", Amount = 2000000M, Subject = "Chata", ValidFrom = DateTime.Parse("2003-3-3"), ValidTo = DateTime.Parse("2033-3-3"), PersonId = frantisekFiala.Id },

                    new Models.Insurance { Type = "Pojištění majetku", Amount = 5000000M, Subject = "Dům", ValidFrom = DateTime.Parse("2004-4-4"), ValidTo = DateTime.Parse("2034-4-4"), PersonId = kvetoslavZapadak.Id },
                    new Models.Insurance { Type = "Pojištění majetku", Amount = 400000M, Subject = "Auto", ValidFrom = DateTime.Parse("2004-4-4"), ValidTo = DateTime.Parse("2034-4-4"), PersonId = kvetoslavZapadak.Id },
                    new Models.Insurance { Type = "Pojištění majetku", Amount = 2000000M, Subject = "Chata", ValidFrom = DateTime.Parse("2004-4-4"), ValidTo = DateTime.Parse("2034-4-4"), PersonId = kvetoslavZapadak.Id }
                );
                context.SaveChanges();  // Uloží data do databáze

                // Vytvoří pojistné události některým pojištěním
                var pojisteniDomu = await context.Insurance.FirstOrDefaultAsync(m => m.Subject == "Dům");
                var pojisteniAuta = await context.Insurance.FirstOrDefaultAsync(m => m.Subject == "Auto");
                var pojisteniChaty = await context.Insurance.FirstOrDefaultAsync(m => m.Subject == "Chata");

                context.Incident.AddRange
                (
                    new Models.Incident { Description = "Vykradení", Date = DateTime.Parse("2010-7-7"), Status = "Vyplaceno", Amount = 10000M, InsuranceId = pojisteniDomu.Id },
                    new Models.Incident { Description = "Autonehoda", Date = DateTime.Parse("2011-8-8"), Status = "Zamítnuto", InsuranceId = pojisteniAuta.Id },
                    new Models.Incident { Description = "Vyhoření", Date = DateTime.Parse("2022-9-1"), Status = "Vyřizuje se", Amount = 2000000M, InsuranceId = pojisteniChaty.Id }
                );
                context.SaveChanges();  // Uloží data do databáze
            }
        }
        */

        //-----------------------------------------------------------------------------------------
    }
}

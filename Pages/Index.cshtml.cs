using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using weather.net.Data;
using weather.net.Models;

namespace weather.net.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ApplicationDbContext _context;
        private UserManager<IdentityUser> _userManager;

        public List<CityWeather> Cities {get; set;}

        [BindProperty]
        public string CityName {get; set;}

        public IndexModel(
            ILogger<IndexModel> logger, 
            ApplicationDbContext context, 
            UserManager<IdentityUser> userManager
        )
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public Task<List<CityWeather>> GetCities() {
            return _context.CitiesWeather
                .Where(cw => cw.CityUsers.Any(
                    cu => cu.UserId == _userManager.GetUserId(User)
                ))
                .ToListAsync();
        }
        public async Task OnGetAsync() {
            Cities = await GetCities();            
        }

        public async Task<IActionResult> OnPostAsync() {
            Cities = await GetCities();

            var City = await _context.CitiesWeather
                .Where(cw => cw.CityName.ToLower() == CityName.ToLower())
                .FirstOrDefaultAsync();
            
            if (City == null) {
                City = new CityWeather{
                    CityName = this.CityName,
                    Temperature = 0,
                };
                _context.CitiesWeather.Add(City);
                await _context.SaveChangesAsync();
            }

            var CityUsers = _context.Set<CityUser>();
            var CurUser = await CityUsers
                .Where(cu => cu.CityId == City.ID && cu.UserId == _userManager.GetUserId(User))
                .FirstOrDefaultAsync();
            if (CurUser == null) {
                CityUsers.Add(
                    new CityUser {
                        UserId = _userManager.GetUserId(User),
                        CityId = City.ID
                    }
                );
            }
            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using weather.net.Data;
using weather.net.Models;

namespace weather.net.Hubs
{
    public interface IWeatherUpdateClient
    {
        Task ReceiveUpdate(int cityId, int temperature);
    }
    
    [Authorize]
    public class WeatherHub : Hub<IWeatherUpdateClient>
    {
        private readonly ApplicationDbContext _db;
        public WeatherHub(
            ApplicationDbContext db
        )
        {
            _db = db;
        }
        public async Task<Boolean> SubscribeCity(int cityId)
        {
            string userId = Context.UserIdentifier;
            var subscribed = await _db.Set<CityUser>()
                .Where(cu => cu.CityId == cityId && cu.UserId == userId)
                .CountAsync()
            ;
            if (subscribed > 0)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"city-{cityId}");
                return true;
            }
            return false;
        }
    }
}
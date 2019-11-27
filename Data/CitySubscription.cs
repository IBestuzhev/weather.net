using Microsoft.AspNetCore.Identity;

namespace weather.net.Models{
    public class CityUser {
        public string UserId {get; set;}
        public IdentityUser User {get; set;}

        public int CityId {get; set;}
        public CityWeather City {get; set;}
    }
}
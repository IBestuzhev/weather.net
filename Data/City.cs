using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace weather.net.Models {
    public class CityWeather {
        public int ID {get; set;}
        public string CityName {get; set;}
        public int Temperature {get; set;}

        public List<CityUser> CityUsers {get; set;}
    }
}
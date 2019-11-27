using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using weather.net.Data;
using weather.net.Hubs;

namespace weather.net.Update
{

    internal interface IScopedWeatherUpdater
    {
        Task DoWork(CancellationToken stoppingToken);
    }

    internal class WeatherUpdateService: IScopedWeatherUpdater
    {
        private readonly ApplicationDbContext _db;
        private readonly IHubContext<WeatherHub, IWeatherUpdateClient> _hubContext;
        public WeatherUpdateService(
            ApplicationDbContext context,
            IHubContext<WeatherHub, IWeatherUpdateClient> hub
        )
        {
            _db = context;
            _hubContext = hub;
        }
        public async Task DoWork(CancellationToken stoppingToken)
        {
            var Rand = new Random();
            var cities = await _db.CitiesWeather.ToListAsync(stoppingToken);
            foreach(var c in cities)
            {
                c.Temperature = Rand.Next(-30, 45);
                await _hubContext.Clients.Group($"city-{c.ID}").ReceiveUpdate(c.ID, c.Temperature);
            }
            await _db.SaveChangesAsync(stoppingToken);
        }
    }
    public class WeatherUpdateWorker : BackgroundService
    {
        private readonly ILogger<WeatherUpdateWorker> _logger;
        public IServiceProvider Services { get; }
        public WeatherUpdateWorker(
            ILogger<WeatherUpdateWorker> logger,
            IServiceProvider S
        )
        {
            _logger = logger;
            Services = S;
        }

        private async Task UpdateWeather(CancellationToken stoppingToken) 
        {
            using (var scope = Services.CreateScope())
            {
                var scopedProcessingService = 
                    scope.ServiceProvider
                        .GetRequiredService<IScopedWeatherUpdater>();

                await scopedProcessingService.DoWork(stoppingToken);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await this.UpdateWeather(stoppingToken);
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}

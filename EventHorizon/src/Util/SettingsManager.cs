using Microsoft.EntityFrameworkCore;
using System;

namespace EventHorizon.src.Util
{
    public class SettingsManager
    {
        private readonly IServiceScopeFactory _scopeFactory;


        public int TimelineDuration { get; set; } = 180;

        public SettingsManager(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            Console.WriteLine("Loading Configuration");
            LoadSettings();

        }

        public void SaveSettings()
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var settingsEntity = dbContext.Settings.Find(1);
            if (settingsEntity == null)
            {
                settingsEntity = new Settings
                {
                    Id = 1,
                    TimelineDuration = TimelineDuration
                };
                dbContext.Settings.Add(settingsEntity);
            }
            else
            {
                settingsEntity.TimelineDuration = TimelineDuration;
                dbContext.Settings.Update(settingsEntity);
            }

            dbContext.SaveChanges();
            Console.WriteLine("Settings saved. Resetting timeline");
            //cannot use DI to prevent DI init Circle
            scope.ServiceProvider.GetRequiredService<TimeLineController>().CompleteReset();
        }

        public void LoadSettings()
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var settingsEntity = dbContext.Settings.AsNoTracking().FirstOrDefault(s => s.Id == 1);
            if (settingsEntity != null)
            {
                TimelineDuration = settingsEntity.TimelineDuration;
                Console.WriteLine("Settings loaded.");
            }
            else
            {
                Console.WriteLine("No settings found, using default values.");
            }
        }
    }
}

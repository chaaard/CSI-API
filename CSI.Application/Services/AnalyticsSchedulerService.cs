using CSI.Application.DTOs;
using CSI.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSI.Application.Services
{
    public class AnalyticsSchedulerService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private Timer _timer;

        public AnalyticsSchedulerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Run the task at 6 am every day
            var now = DateTime.Now;
            var scheduledTime = new DateTime(now.Year, now.Month, now.Day, 7, 43, 0);
            if (now > scheduledTime)
            {
                scheduledTime = scheduledTime.AddDays(1);
            }

            var dueTime = scheduledTime - now;
            _timer = new Timer(DoWork, null, dueTime, TimeSpan.FromDays(1));

            return Task.CompletedTask;
        }


        private void DoWork(object state)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var analyticsService = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();
                var salesParam = new AnalyticsParamsDto();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Dispose();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}

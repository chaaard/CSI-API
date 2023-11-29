using CSI.Application.Interfaces;
using CSI.Domain.Entities;
using CSI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSI.Application.Services
{
    public class LocationService : ILocationService
    {
        private readonly AppDBContext _dbContext;

        public LocationService(AppDBContext dBContext)
        {
            _dbContext = dBContext;
        }

        public async Task<List<Location>> GetLocation()
        {
            var getLocations = new List<Location>();
            getLocations = await _dbContext.Locations.Where(x => x.DeleteFlag == false).ToListAsync();
            return getLocations;
        }
    }
}

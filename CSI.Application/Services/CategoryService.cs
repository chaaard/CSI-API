using CSI.Application.DTOs;
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
    public class CategoryService : ICategoryService
    {
        private readonly AppDBContext _dbContext;
        private readonly IConfiguration _configuration;

        public CategoryService(IConfiguration configuration, AppDBContext dBContext)
        {
            _configuration = configuration;
            _dbContext = dBContext;
        }

        public async Task<List<Category>> GetCategoryAsync()
        {
            var getCategory = new List<Category>();
            getCategory = await _dbContext.Category.Where(x => x.DeleteFlag == false && x.StatusId == 1).ToListAsync();
            return getCategory;
        }
    }
}

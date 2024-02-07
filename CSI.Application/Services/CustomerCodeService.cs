
﻿using CSI.Application.DTOs;
using CSI.Application.Interfaces;
using CSI.Domain.Entities;
using CSI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace CSI.Application.Services
{
    public class CustomerCodeService : ICustomerCodeService
    {
        private readonly AppDBContext _dbContext;
        private readonly IConfiguration _configuration;

        public CustomerCodeService(IConfiguration configuration, AppDBContext dBContext)
        {
            _configuration = configuration;
            _dbContext = dBContext;
        }

        public async Task<(List<CustomerCodeDto>, int totalPages)> GetCustomerCodesAsync(PaginationDto pagination)
        {
            var query = _dbContext.CustomerCodes
                .Where(customerCode => customerCode.DeleteFlag == false)
                .Select(n => new CustomerCodeDto { 
                    Id = n.Id,
                    CustomerNo = n.CustomerNo,
                    CustomerCode = n.CustomerCode,
                    CustomerName = n.CustomerName,
                    DeleteFlag = n.DeleteFlag,
                })
                .AsQueryable();

            // Searching
            if (!string.IsNullOrEmpty(pagination.SearchQuery))
            {
                var searchQuery = $"%{pagination.SearchQuery.ToLower()}%";

                query = query.Where(c =>
                    (EF.Functions.Like(c.CustomerName.ToLower(), searchQuery)) ||
                    (EF.Functions.Like(c.CustomerCode.ToLower(), searchQuery))
                //Add the category column here
                );
            }

            // Sorting
            if (!string.IsNullOrEmpty(pagination.ColumnToSort))
            {
                var sortOrder = pagination.OrderBy == "desc" ? "desc" : "asc";

                switch (pagination.ColumnToSort.ToLower())
                {
                    case "customername":
                        query = sortOrder == "asc" ? query.OrderBy(c => c.CustomerName) : query.OrderByDescending(c => c.CustomerName);
                        break;
                    case "customercode":
                        query = sortOrder == "asc" ? query.OrderBy(c => c.CustomerCode) : query.OrderByDescending(c => c.CustomerCode);
                        break;
                    //Another case here for category
                    default:
                        break;
                }
            }

            var totalItemCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItemCount / pagination.PageSize);

            var customerCodesList = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            return (customerCodesList, totalPages);
        }

        public async Task<List<CustomerCodes>> GetCustomerDdCodesAsync()
        {
            var query = await _dbContext.CustomerCodes
                .Where(customerCode => customerCode.DeleteFlag == false)
                .ToListAsync();

            return query;
        }

        public async Task<CustomerCodes> GetCustomerCodeByIdAsync(int Id)
        {
            var getCustomerCodes = new CustomerCodes();
            getCustomerCodes = await _dbContext.CustomerCodes.Where(x => x.DeleteFlag == false && x.Id == Id).FirstAsync();
            return getCustomerCodes;
        }

        public async Task<CustomerCodes> UpdateCustomerCodeByIdAsync(CustomerCodes customerCode)
        {
            var getCustomerCode = await _dbContext.CustomerCodes.SingleOrDefaultAsync(x => x.Id == customerCode.Id);

            if (getCustomerCode != null)
            {
                getCustomerCode.CustomerName = customerCode.CustomerName;
                getCustomerCode.CustomerCode = customerCode.CustomerCode;
                getCustomerCode.CustomerNo = customerCode.CustomerNo;
                getCustomerCode.DeleteFlag = customerCode.DeleteFlag;
                await _dbContext.SaveChangesAsync();

                return getCustomerCode;
            }
            else
            {
                return new CustomerCodes();
            }
        }

        public async Task<bool> DeleteCustomerCodeByIdAsync(int Id)
        {
            var getCustomerCode = await _dbContext.CustomerCodes.SingleOrDefaultAsync(x => x.Id == Id);

            if (getCustomerCode != null)
            {
                getCustomerCode.DeleteFlag = true;
                await _dbContext.SaveChangesAsync();

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

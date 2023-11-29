using CSI.Application.DTOs;
using CSI.Application.Interfaces;
using CSI.Domain.Entities;
using CSI.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSI.Application.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly AppDBContext _dbContext;
        private readonly IConfiguration _configuration;

        public AnalyticsService(IConfiguration configuration, AppDBContext dBContext)
        {
            _configuration = configuration;
            _dbContext = dBContext;
            _dbContext.Database.SetCommandTimeout(999);

        }

        private async Task DropTables(string strStamp)
        {
            try
            {
                if (_dbContext.Database.GetDbConnection().State == ConnectionState.Closed)
                {
                    await _dbContext.Database.GetDbConnection().OpenAsync();
                }

                var tableNames = new[]
                {
                    $"ANALYTICS_CSHTND{strStamp}",
                    $"ANALYTICS_CSHHDR{strStamp}",
                    $"ANALYTICS_CONDTX{strStamp}",
                    $"ANALYTICS_INVMST{strStamp}",
                    $"ANALYTICS_TBLSTR{strStamp}"
                };

                foreach (var tableName in tableNames)
                {
                    await _dbContext.Database.ExecuteSqlRawAsync($"IF OBJECT_ID('{tableName}', 'U') IS NOT NULL DROP TABLE {tableName}");
                }

                await _dbContext.Database.GetDbConnection().CloseAsync();
            }
            catch (Exception)
            {
                await _dbContext.Database.GetDbConnection().CloseAsync();
                throw;
            }
        }

        public async Task<List<int>> GetDepartments()
        {
            try
            {
                var deptCodes = new List<int>();
                var result = await _dbContext.Departments.ToListAsync();
                foreach (var item in result)
                {
                    deptCodes.Add(item.DeptCode);
                }

                return deptCodes;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<AnalyticsDto>> GetAnalytics(AnalyticsParamsDto analyticsParamsDto)
        { 
            var result = await _dbContext.Analytics.Where(x => x.TransactionDate == analyticsParamsDto.dates[0] && x.LocationId == analyticsParamsDto.storeId[0] && x.CustomerId == analyticsParamsDto.memCode[0])
                .Join(_dbContext.Locations, analytics => analytics.LocationId, location => location.LocationCode, (analytics, location) => new { analytics, location })
                .Select(n => new AnalyticsDto { 
                    Id = n.analytics.Id,
                    CustomerId = n.analytics.CustomerId,
                    LocationName = n.location.LocationName,
                    TransactionDate = n.analytics.TransactionDate,
                    MembershipNo = n.analytics.MembershipNo,
                    CashierNo = n.analytics.CashierNo,
                    RegisterNo = n.analytics.RegisterNo,
                    TransactionNo = n.analytics.TransactionNo,
                    OrderNo = n.analytics.OrderNo,
                    Qty = n.analytics.Qty,
                    Amount = n.analytics.Amount,
                    UserId = n.analytics.UserId,
                    DeleteFlag = n.analytics.DeleteFlag,
                })
                .ToListAsync();

            return result;
        }

        public async Task<List<MatchDto>> GetAnalyticsProofListVariance(AnalyticsParamsDto analyticsParamsDto)
        {
            var result = await _dbContext.Analytics
                .Join(_dbContext.Prooflist, a => a.OrderNo, b => b.OrderNo, ( a, b ) => new { a, b })
                .Where(x => x.a.TransactionDate == analyticsParamsDto.dates[0]
                        && x.a.LocationId == analyticsParamsDto.storeId[0] 
                        && x.a.CustomerId == analyticsParamsDto.memCode[0])
                .Select(n => new MatchDto 
                {
                    AnalyticsId = n.a.Id,
                    AnalyticsTransactionDate = n.a.TransactionDate,
                    AnalyticsOrderNo = n.a.OrderNo,
                    AnalyticsAmount = n.a.Amount,
                    ProofListId = n.b.Id,
                    ProofListTransactionDate = n.b.TransactionDate,
                    ProofListOrderNo = n.b.OrderNo,
                    ProofListAmount = n.b.Amount,
                    Variance = n.a.Amount - (n.b.Amount ?? 0),
                })
                .ToListAsync();

            return result;
        }
    }
}

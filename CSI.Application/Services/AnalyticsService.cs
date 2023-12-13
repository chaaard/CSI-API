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
            DateTime date;
            var result = new List<AnalyticsDto>();
            if (DateTime.TryParse(analyticsParamsDto.dates[0], out date))
            {
                result = await _dbContext.Analytics.Where(x => x.TransactionDate == date && x.LocationId == analyticsParamsDto.storeId[0] && x.CustomerId == analyticsParamsDto.memCode[0])
                 .Join(_dbContext.Locations, analytics => analytics.LocationId, location => location.LocationCode, (analytics, location) => new { analytics, location })
                 .Select(n => new AnalyticsDto
                 {
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
            }
            return result;
        }

        public async Task<decimal?> GetTotalAmountPerMechant(AnalyticsParamsDto analyticsParamsDto)
        {
            DateTime date;
            decimal? result = 0;
            if (DateTime.TryParse(analyticsParamsDto.dates[0], out date))
            {
                result = await _dbContext.Analytics
                    .Where(x => x.TransactionDate == date && x.LocationId == analyticsParamsDto.storeId[0] && analyticsParamsDto.memCode[0].Contains(x.CustomerId))
                    .SumAsync(e => e.Amount);
            }
            return result;
        }


        public async Task<List<MatchDto>> GetAnalyticsProofListVariance(AnalyticsParamsDto analyticsParamsDto)
        {
            try
            {
                var result = await _dbContext.Match
                    .FromSqlRaw(
                        $"SELECT " +
                        $"    a.[Id] AS [AnalyticsId], " +
                        $"    a.CustomerName AS [AnalyticsPartner], " +
                        $"    a.LocationName AS [AnalyticsLocation], " +
                        $"    a.[TransactionDate] AS [AnalyticsTransactionDate], " +
                        $"    a.[OrderNo] AS [AnalyticsOrderNo], " +
                        $"    a.[Amount] AS [AnalyticsAmount], " +
                        $"    p.[Id] AS [ProofListId], " +
                        $"    p.[TransactionDate] AS [ProofListTransactionDate], " +
                        $"    p.[OrderNo] AS [ProofListOrderNo], " +
                        $"    p.[Amount] AS [ProofListAmount] " +
                        $"FROM " +
                        $"    ( " +
                        $"        SELECT " +
                        $"            a.[Id], " +
                        $"            c.CustomerName, " +
                        $"            l.LocationName, " +
                        $"            a.[TransactionDate], " +
                        $"            a.[OrderNo], " +
                        $"            a.[Amount] " +
                        $"        FROM " +
                        $"            [dbo].[tbl_analytics] a " +
                        $"            LEFT JOIN [dbo].[tbl_location] l ON l.LocationCode = a.LocationId " +
                        $"            LEFT JOIN [dbo].[tbl_customer] c ON c.CustomerCode = a.CustomerId " +
                        $"        WHERE " +
                        $"            (CAST(a.TransactionDate AS DATE) = '{analyticsParamsDto.dates[0].ToString()}' AND a.LocationId = {analyticsParamsDto.storeId[0]} AND a.CustomerId = '{analyticsParamsDto.memCode[0]}') " +
                        $"    ) a " +
                        $"FULL OUTER JOIN " +
                        $"    ( " +
                        $"        SELECT " +
                        $"            p.[Id], " +
                        $"            c.CustomerName, " +
                        $"            l.LocationName, " +
                        $"            p.[TransactionDate], " +
                        $"            p.[OrderNo], " +
                        $"            p.[Amount] " +
                        $"        FROM " +
                        $"            [dbo].[tbl_prooflist] p " +
                        $"            LEFT JOIN [dbo].[tbl_location] l ON l.LocationCode = p.StoreId " +
                        $"            LEFT JOIN [dbo].[tbl_customer] c ON c.CustomerCode = p.CustomerId " +
                        $"        WHERE " +
                        $"            (CAST(p.TransactionDate AS DATE) = '{analyticsParamsDto.dates[0].ToString()}' AND p.StoreId = {analyticsParamsDto.storeId[0]} AND p.CustomerId = '{analyticsParamsDto.memCode[0]}' AND p.Amount IS NOT NULL) " +
                        $"    ) p " +
                        $"ON a.[OrderNo] = p.[OrderNo];")
                    .ToListAsync();

                var matchDtos = result.Select(m => new MatchDto
                {
                    AnalyticsId = m.AnalyticsId,
                    AnalyticsPartner = m.AnalyticsPartner,
                    AnalyticsLocation = m.AnalyticsLocation,
                    AnalyticsTransactionDate = m.AnalyticsTransactionDate,
                    AnalyticsOrderNo = m.AnalyticsOrderNo,
                    AnalyticsAmount = m.AnalyticsAmount,
                    ProofListId = m.ProofListId,
                    ProofListTransactionDate = m.ProofListTransactionDate,
                    ProofListOrderNo = m.ProofListOrderNo,
                    ProofListAmount = m.ProofListAmount,
                    Variance = (m.AnalyticsAmount == null || m.ProofListAmount == null) ? 0 : m.AnalyticsAmount - m.ProofListAmount.Value,
                }).ToList();

                return matchDtos;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}

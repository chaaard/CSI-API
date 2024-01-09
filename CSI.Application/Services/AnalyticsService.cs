using AutoMapper;
using CSI.Application.DTOs;
using CSI.Application.Interfaces;
using CSI.Domain.Entities;
using CSI.Infrastructure.Data;
using EFCore.BulkExtensions;
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
        private readonly IMapper _mapper;

        public AnalyticsService(IConfiguration configuration, AppDBContext dBContext, IMapper mapper)
        {
            _configuration = configuration;
            _dbContext = dBContext;
            _mapper = mapper;
            _dbContext.Database.SetCommandTimeout(999);

        }

        public async Task<string> GetDepartments()
        {
            try
            {
                List<string> values = new List<string>();
                using (MsSqlCon db = new MsSqlCon(_configuration))
                {
                    if (db.Con.State == ConnectionState.Closed)
                    {
                        await db.Con.OpenAsync();
                    }

                    var cmd = new SqlCommand();
                    cmd.Connection = db.Con;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = 0;
                    cmd.CommandText = "SELECT DeptCode FROM TrsDept_Table";
                    cmd.ExecuteNonQuery();
                    SqlDataReader sqlDataReader = cmd.ExecuteReader();

                    if (sqlDataReader.HasRows)
                    {
                        while (sqlDataReader.Read())
                        {
                            if (sqlDataReader["DeptCode"].ToString() != null)
                                values.Add(sqlDataReader["DeptCode"].ToString());
                        }
                    }
                    sqlDataReader.Close();
                    await db.Con.CloseAsync();
                }

                return string.Join(", ", (IEnumerable<string>)values); ;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<Location>> GetLocations()
        {
            try
            {
                var locations = new List<Location>();
                locations = await _dbContext.Locations
                    .ToListAsync();

                return locations;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<AnalyticsDto>> GetAnalytics(AnalyticsParamsDto analyticsParamsDto)
        {
            try
            {
                var analytics = await ReturnAnalytics(analyticsParamsDto);

                return analytics;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<List<AnalyticsDto>> ReturnAnalytics(AnalyticsParamsDto analyticsParamsDto)
        {
            var result = await _dbContext.AnalyticsView
                   .FromSqlRaw(
                       $" WITH RankedOrders AS ( " +
                       $"    SELECT  " +
                       $"        a.Id,  " +
                       $"        a.CustomerId,  " +
                       $"        l.LocationName,  " +
                       $"        a.LocationId,  " +
                       $"        a.TransactionDate,  " +
                       $"        a.MembershipNo,  " +
                       $"        a.CashierNo,  " +
                       $"        a.RegisterNo,  " +
                       $"        a.TransactionNo,  " +
                       $"        a.OrderNo,  " +
                       $"        a.Qty,  " +
                       $"        a.Amount,  " +
                       $"        a.SubTotal, " +
                       $"        a.UserId,  " +
                       $"        a.StatusId, " +
                       $"        a.DeleteFlag,  " +
                       $"        ROW_NUMBER() OVER (PARTITION BY a.LocationId, a.TransactionDate, a.OrderNo ORDER BY a.TransactionNo DESC) AS RowNum  " +
                       $"    FROM  " +
                       $"       tbl_analytics a  " +
                       $"   INNER JOIN  " +
                       $"        [dbo].[tbl_location] l ON a.LocationId = l.LocationCode  " +
                       $"    WHERE  " +
                       $"        a.SubTotal >= 0 AND (CAST(a.TransactionDate AS DATE) = '{analyticsParamsDto.dates[0].ToString()}' AND a.LocationId = {analyticsParamsDto.storeId[0]} AND a.CustomerId = '{analyticsParamsDto.memCode[0]}')  " +
                       $" )  " +
                       $" , FilteredOrders AS (  " +
                       $"    SELECT  " +
                       $"        Id,  " +
                       $"        CustomerId,  " +
                       $"        LocationName,  " +
                       $"        TransactionDate,  " +
                       $"        MembershipNo,  " +
                       $"        CashierNo,  " +
                       $"        RegisterNo,  " +
                       $"        TransactionNo,  " +
                       $"        OrderNo,  " +
                       $"        Qty,  " +
                       $"        Amount,  " +
                       $"        SubTotal,  " +
                       $"        UserId,  " +
                       $"        StatusId,  " +
                       $"        DeleteFlag  " +
                       $"    FROM  " +
                       $"        RankedOrders  " +
                       $"    WHERE  " +
                       $"        RowNum = 1  " +
                       $"        OR  " +
                       $"        (RowNum = 2 AND NOT EXISTS (SELECT 1 FROM RankedOrders WHERE RowNum = 1 AND OrderNo = RankedOrders.OrderNo))  " +
                       $" )  " +
                       $" SELECT * FROM FilteredOrders; "
                       )
                   .ToListAsync();

            var analytics = result.Select(n => new AnalyticsDto
            {
                Id = n.Id,
                CustomerId = n.CustomerId,
                LocationName = n.LocationName,
                TransactionDate = n.TransactionDate,
                MembershipNo = n.MembershipNo,
                CashierNo = n.CashierNo,
                RegisterNo = n.RegisterNo,
                TransactionNo = n.TransactionNo,
                OrderNo = n.OrderNo,
                Qty = n.Qty,
                Amount = n.Amount,
                SubTotal = n.SubTotal,
                UserId = n.UserId,
                StatusId = n.StatusId,
                DeleteFlag = n.DeleteFlag,
            }).ToList();

            return analytics;
        }

        public async Task<decimal?> GetTotalAmountPerMechant(AnalyticsParamsDto analyticsParamsDto)
        {
            DateTime date;
            decimal? result = 0;
            if (DateTime.TryParse(analyticsParamsDto.dates[0], out date))
            {
                result = await _dbContext.Analytics
                    .Where(x => x.TransactionDate == date && x.LocationId == analyticsParamsDto.storeId[0] && analyticsParamsDto.memCode[0].Contains(x.CustomerId))
                    .SumAsync(e => e.SubTotal);
            }
            return result;
        }


        public async Task<List<MatchDto>> GetAnalyticsProofListVariance(AnalyticsParamsDto analyticsParamsDto)
        {
            try
            {
                var result = await _dbContext.Match
                    .FromSqlRaw($"WITH RankedData AS (" +
                                $"    SELECT " +
                                $"        a.[Id], " +
                                $"        c.CustomerName, " +
                                $"        l.LocationName, " +
                                $"        a.[TransactionDate], " +
                                $"        a.[OrderNo], " +
                                $"        a.[SubTotal], " +
                                $"        ROW_NUMBER() OVER (PARTITION BY a.[OrderNo] ORDER BY a.[TransactionNo] DESC) AS RowNum " +
                                $"    FROM " +
                                $"        [dbo].[tbl_analytics] a " +
                                $"        LEFT JOIN [dbo].[tbl_location] l ON l.LocationCode = a.LocationId " +
                                $"        LEFT JOIN [dbo].[tbl_customer] c ON c.CustomerCode = a.CustomerId " +
                                $"    WHERE " +
                                $"        (CAST(a.TransactionDate AS DATE) = '{analyticsParamsDto.dates[0].ToString()}' AND a.LocationId = {analyticsParamsDto.storeId[0]} AND a.CustomerId = '{analyticsParamsDto.memCode[0]}')" +
                                $"), " +
                                $"FilteredData AS (" +
                                $"    SELECT " +
                                $"        Id, " +
                                $"        CustomerName, " +
                                $"        LocationName, " +
                                $"        [TransactionDate], " +
                                $"        [OrderNo], " +
                                $"        [SubTotal], " +
                                $"        RowNum " +
                                $"    FROM RankedData " +
                                $"    WHERE RowNum = 1 AND [SubTotal] >= 1" +
                                $") " +
                                $"SELECT " +
                                $"    a.[Id] AS [AnalyticsId], " +
                                $"    a.CustomerName AS [AnalyticsPartner], " +
                                $"    a.LocationName AS [AnalyticsLocation], " +
                                $"    a.[TransactionDate] AS [AnalyticsTransactionDate], " +
                                $"    a.[OrderNo] AS [AnalyticsOrderNo], " +
                                $"    a.[SubTotal] AS [AnalyticsAmount], " +
                                $"    p.[Id] AS [ProofListId], " +
                                $"    p.[TransactionDate] AS [ProofListTransactionDate], " +
                                $"    p.[OrderNo] AS [ProofListOrderNo], " +
                                $"    p.[Amount] AS [ProofListAmount] " +
                                $"FROM " +
                                $"    FilteredData a " +
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
                                $"            (CAST(p.TransactionDate AS DATE) = '{analyticsParamsDto.dates[0].ToString()}' AND p.StoreId = {analyticsParamsDto.storeId[0]} AND p.CustomerId = '{analyticsParamsDto.memCode[0]}' AND p.Amount IS NOT NULL AND p.Amount <> 0 AND p.StatusId != 4) " +
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
                    Variance = (m.AnalyticsAmount == null) ? m.ProofListAmount : (m.ProofListAmount == null) ? m.AnalyticsAmount :  m.AnalyticsAmount - m.ProofListAmount.Value,
                }).ToList();

                return matchDtos;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task RefreshAnalytics(RefreshAnalyticsDto analyticsParam)
        {
            var listResultOne = new List<Analytics>();
            string strFrom = analyticsParam.dates[0].ToString("yyMMdd");
            string strTo = analyticsParam.dates[1].ToString("yyMMdd");
            string strStamp = $"{DateTime.Now.ToString("yyMMdd")}{DateTime.Now.ToString("HHmmss")}{DateTime.Now.Millisecond.ToString()}";
            string getQuery = string.Empty;
            var deptCodeList = await GetDepartments();
            var deptCodes = string.Join(", ", deptCodeList);

            string cstDocCondition = $"CSTDOC IN ({string.Join(", ", analyticsParam.memCode.Select(code => $"''{code}''"))})";
            string storeList = $"CSSTOR IN ({string.Join(", ", analyticsParam.storeId.Select(code => $"{code}"))})";
            try
            {
                await _dbContext.Database.ExecuteSqlRawAsync($"CREATE TABLE ANALYTICS_CSHTND{strStamp} (CSDATE VARCHAR(255), CSSTOR INT, CSREG INT, CSTRAN INT, CSTDOC VARCHAR(50), CSCARD VARCHAR(50), CSDTYP VARCHAR(50), CSTIL INT)");
                // Insert data from MMJDALIB.CSHTND into the newly created table ANALYTICS_CSHTND + strStamp
                await _dbContext.Database.ExecuteSqlRawAsync($"INSERT INTO ANALYTICS_CSHTND{strStamp} (CSDATE, CSSTOR, CSREG, CSTRAN, CSTDOC, CSCARD, CSDTYP, CSTIL)  " +
                                  $"SELECT CSDATE, CSSTOR, CSREG, CSTRAN, CSTDOC, CSCARD, CSDTYP, CSTIL " +
                                  $"FROM OPENQUERY(SNR, 'SELECT CSDATE, CSSTOR, CSREG, CSTRAN, CSTDOC, CSCARD, CSDTYP, CSTIL FROM MMJDALIB.CSHTND WHERE (CSDATE BETWEEN {strFrom} AND {strTo}) AND {cstDocCondition} AND CSDTYP IN (''AR'') AND {storeList}  " +
                                  $"GROUP BY CSDATE, CSSTOR, CSREG, CSTRAN, CSTDOC, CSCARD, CSDTYP, CSTIL ') ");

                // Create the table ANALYTICS_CSHHDR + strStamp
                await _dbContext.Database.ExecuteSqlRawAsync($"CREATE TABLE ANALYTICS_CSHHDR{strStamp} (CSDATE VARCHAR(255), CSSTOR INT, CSREG INT, CSTRAN INT, CSCUST VARCHAR(255), CSTAMT DECIMAL(18,3))");
                // Insert data from MMJDALIB.CSHHDR and ANALYTICS_CSHTND into the newly created table SALES_ANALYTICS_CSHHDR + strStamp
                await _dbContext.Database.ExecuteSqlRawAsync($"INSERT INTO ANALYTICS_CSHHDR{strStamp} (CSDATE, CSSTOR, CSREG, CSTRAN, CSCUST, CSTAMT )  " +
                                  $"SELECT A.CSDATE, A.CSSTOR, A.CSREG, A.CSTRAN, A.CSCUST, A.CSTAMT  " +
                                  $"FROM OPENQUERY(SNR, 'SELECT CSDATE, CSSTOR, CSREG, CSTRAN, CSCUST, CSTAMT FROM MMJDALIB.CSHHDR WHERE (CSDATE BETWEEN {strFrom} AND {strTo}) AND {storeList} ') A  " +
                                  $"INNER JOIN ANALYTICS_CSHTND{strStamp} B  " +
                                  $"ON A.CSDATE = B.CSDATE AND A.CSSTOR = B.CSSTOR AND A.CSREG = B.CSREG AND A.CSTRAN = B.CSTRAN ");
            }
            catch (Exception ex)
            {
                await DropTables(strStamp);
                throw;
            }

            try
            {
                // Create the table ANALYTICS_CONDTX + strStamp
                await _dbContext.Database.ExecuteSqlRawAsync($"CREATE TABLE ANALYTICS_CONDTX{strStamp} (CSDATE VARCHAR(255), CSSTOR INT, CSREG INT, CSTRAN INT, CSSKU INT, CSQTY DECIMAL(18,3),  CSEXPR DECIMAL(18,3), CSEXCS DECIMAL(18,4), CSDSTS INT)");
                // Insert data from MMJDALIB.CONDTX into the newly created table ANALYTICS_CONDTX + strStamp
                await _dbContext.Database.ExecuteSqlRawAsync($"INSERT INTO ANALYTICS_CONDTX{strStamp} (CSDATE, CSSTOR, CSREG, CSTRAN, CSSKU, CSQTY, CSEXPR, CSEXCS, CSDSTS )  " +
                                      $"SELECT A.CSDATE, A.CSSTOR, A.CSREG, A.CSTRAN, A.CSSKU, A.CSQTY, A.CSEXPR, A.CSEXCS, A.CSDSTS  " +
                                      $"FROM OPENQUERY(SNR, 'SELECT CSDATE, CSSTOR, CSREG, CSTRAN, CSSKU, CSQTY, CSEXPR, CSEXCS, CSDSTS FROM MMJDALIB.CONDTX WHERE (CSDATE BETWEEN {strFrom} AND {strTo}) AND {storeList} ') A  " +
                                      $"INNER JOIN ANALYTICS_CSHTND{strStamp} B  " +
                                      $"ON A.CSDATE = B.CSDATE AND A.CSSTOR = B.CSSTOR AND A.CSREG = B.CSREG AND A.CSTRAN = B.CSTRAN WHERE A.CSSKU <> 0 AND A.CSDSTS = '0' ");
            }
            catch (Exception ex)
            {
                await DropTables(strStamp);
                throw;
            }

            try
            {
                // Create the table ANALYTICS_INVMST + strStamp

                await _dbContext.Database.ExecuteSqlRawAsync($"CREATE TABLE ANALYTICS_INVMST{strStamp} (IDESCR VARCHAR(255), IDEPT INT, ISDEPT INT, ICLAS INT, ISCLAS INT, INUMBR INT)");
                // Insert data from MMJDALIB.INVMST into the newly created table ANALYTICS_INVMST + strStamp
                await _dbContext.Database.ExecuteSqlRawAsync($"INSERT INTO ANALYTICS_INVMST{strStamp} (IDESCR, IDEPT, ISDEPT, ICLAS, ISCLAS, INUMBR) " +
                                          $"SELECT A.IDESCR, A.IDEPT, A.ISDEPT, A.ICLAS, A.ISCLAS, A.INUMBR " +
                                          $"FROM OPENQUERY(SNR, 'SELECT DISTINCT IDESCR, IDEPT, ISDEPT, ICLAS, ISCLAS, INUMBR FROM MMJDALIB.INVMST WHERE IDEPT IN ({deptCodes})') A " +
                                          $"INNER JOIN ANALYTICS_CONDTX{strStamp} B  " +
                                          $"ON A.INUMBR = B.CSSKU");
            }
            catch (Exception ex)
            {
                await DropTables(strStamp);
                throw;
            }

            try
            {
                // Create the table ANALYTICS_TBLSTR + strStamp
                await _dbContext.Database.ExecuteSqlRawAsync($"CREATE TABLE ANALYTICS_TBLSTR{strStamp} (STRNUM INT, STRNAM VARCHAR(255))");
                // Insert data from MMJDALIB.TBLSTR into the newly created table ANALYTICS_TBLSTR + strStamp
                await _dbContext.Database.ExecuteSqlRawAsync($"INSERT INTO ANALYTICS_TBLSTR{strStamp} (STRNUM, STRNAM) " +
                                        $"SELECT * FROM OPENQUERY(SNR, 'SELECT STRNUM, STRNAM FROM MMJDALIB.TBLSTR') ");
            }
            catch (Exception ex)
            {
                await DropTables(strStamp);
                throw;
            }

            try
            {
                await _dbContext.Database.ExecuteSqlRawAsync($"INSERT INTO [dbo].[tbl_analytics] (LocationId, TransactionDate, CustomerId, MembershipNo, CashierNo, RegisterNo, TransactionNo, OrderNo, Qty, Amount, SubTotal, UserId, DeleteFlag) " +
                                  $"SELECT C.CSSTOR, C.CSDATE, B.CSTDOC, A.CSCUST,B.CSTIL, C.CSREG, C.CSTRAN, B.CSCARD, SUM(C.CSQTY) AS CSQTY, SUM(C.CSEXPR) AS CSEXPR, A.CSTAMT, NULL AS UserId, 0 AS DeleteFlag   " +
                                  $"FROM ANALYTICS_CSHHDR{strStamp} A " +
                                      $"INNER JOIN ANALYTICS_CSHTND{strStamp} B ON A.CSSTOR = B.CSSTOR AND A.CSDATE = B.CSDATE AND A.CSREG = B.CSREG AND A.CSTRAN = B.CSTRAN  " +
                                      $"INNER JOIN ANALYTICS_CONDTX{strStamp} C ON A.CSSTOR = C.CSSTOR AND A.CSDATE = C.CSDATE AND A.CSREG = C.CSREG AND A.CSTRAN = C.CSTRAN  " +
                                      $"INNER JOIN ANALYTICS_INVMST{strStamp} D ON C.CSSKU = D.INUMBR  " +
                                      $"INNER JOIN ANALYTICS_TBLSTR{strStamp} E ON E.STRNUM = C.CSSTOR  " +
                                  $"GROUP BY C.CSSTOR,  C.CSDATE,  B.CSTDOC,  A.CSCUST,  C.CSREG,  C.CSTRAN,  B.CSCARD,  B.CSTIL,  A.CSTAMT   " +
                                  $"ORDER BY C.CSSTOR, C.CSDATE, C.CSREG ");

                await DropTables(strStamp);
            }
            catch (Exception ex)
            {
                await DropTables(strStamp);
                throw;
            }
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
            catch (Exception ex)
            {
                await _dbContext.Database.GetDbConnection().CloseAsync();
                throw;
            }
        }

        public async Task<bool> SubmitAnalytics(AnalyticsParamsDto analyticsParamsDto)
        {
            DateTime date;
            var isPending = true;
            IQueryable<AdjustmentDto> query = Enumerable.Empty<AdjustmentDto>().AsQueryable();
            if (DateTime.TryParse(analyticsParamsDto.dates[0], out date))
            {
                query = _dbContext.AnalyticsProoflist
                    .GroupJoin(_dbContext.Analytics, ap => ap.AnalyticsId, a => a.Id, (ap, a) => new { ap, a })
                    .SelectMany(x => x.a.DefaultIfEmpty(), (x, a) => new { x.ap, a })
                    .GroupJoin(_dbContext.Prooflist, x => x.ap.ProoflistId, p => p.Id, (x, p) => new { x.ap, x.a, Prooflist = p })
                    .SelectMany(x => x.Prooflist.DefaultIfEmpty(), (x, p) => new { x.ap, x.a, Prooflist = p })
                    .GroupJoin(_dbContext.CustomerCodes, x => x.a.CustomerId, c => c.CustomerCode, (x, c) => new { x.ap, x.a, x.Prooflist, Customer = c })
                    .SelectMany(x => x.Customer.DefaultIfEmpty(), (x, c) => new { x.ap, x.a, x.Prooflist, Customer = c })
                    .GroupJoin(_dbContext.Adjustments, x => x.ap.AdjustmentId, ad => ad.Id, (x, ad) => new { x.ap, x.a, x.Prooflist, x.Customer, Adjustment = ad })
                    .SelectMany(x => x.Adjustment.DefaultIfEmpty(), (x, ad) => new { x.ap, x.a, x.Prooflist, x.Customer, Adjustment = ad })
                    .GroupJoin(_dbContext.Actions, x => x.ap.ActionId, ac => ac.Id, (x, ac) => new { x.ap, x.a, x.Prooflist, x.Customer, x.Adjustment, Action = ac })
                    .SelectMany(x => x.Action.DefaultIfEmpty(), (x, ac) => new { x.ap, x.a, x.Prooflist, x.Customer, x.Adjustment, Action = ac })
                    .GroupJoin(_dbContext.Status, x => x.ap.StatusId, s => s.Id, (x, s) => new { x.ap, x.a, x.Prooflist, x.Customer, x.Adjustment, x.Action, Status = s })
                    .SelectMany(x => x.Status.DefaultIfEmpty(), (x, s) => new { x.ap, x.a, x.Prooflist, x.Customer, x.Adjustment, x.Action, Status = s })
                    .Join(_dbContext.Locations, x => x.a.LocationId, l => l.LocationCode, (x, l) => new { x, l })
                    .Where(x => x.x.a.TransactionDate == date && x.x.a.LocationId == analyticsParamsDto.storeId[0] && x.x.a.CustomerId == analyticsParamsDto.memCode[0])
                    .Select(x => new AdjustmentDto
                    {
                        Id = x.x.ap.Id,
                        CustomerId = x.x.Customer.CustomerName,
                        JoNumber = x.x.a.OrderNo,
                        TransactionDate = x.x.a.TransactionDate,
                        Amount = x.x.a.SubTotal,
                        AdjustmentType = x.x.Action.Action,
                        Status = x.x.Status.StatusName,
                        AdjustmentId = x.x.ap.AdjustmentId,
                        LocationName = x.l.LocationName,
                        AnalyticsId = x.x.ap.AnalyticsId,
                        ProofListId = x.x.ap.ProoflistId
                    })
                    .OrderBy(x => x.Id);
            }

            isPending = await query
                .Where(x => x.Status == "Pending")
                .AnyAsync();

            if (!isPending)
            {
                var result = await ReturnAnalytics(analyticsParamsDto);

                foreach (var analytics in result)
                {
                    analytics.StatusId = 3;
                }

                var analyticsEntityList = result.Select(analyticsDto =>
                {
                    var analyticsEntity = _mapper.Map<Analytics>(analyticsDto);
                    analyticsEntity.StatusId = 3;
                    analyticsEntity.LocationId = analyticsParamsDto.storeId[0];
                    return analyticsEntity;
                }).ToList();

                _dbContext.BulkUpdate(analyticsEntityList);
                await _dbContext.SaveChangesAsync();
            }
            return isPending;
        }

        public async Task<(List<InvoiceDto>, bool)> GenerateInvoiceAnalytics(AnalyticsParamsDto analyticsParamsDto)
        {
            var invoiceAnalytics = new List<InvoiceDto>();
            var isPending = false;
            var result = await ReturnAnalytics(analyticsParamsDto);
            var locationList = await GetLocations();

            isPending = result
                .Where(x => x.StatusId == 5)
                .Any();

            if (isPending)
            {
                return (invoiceAnalytics, isPending);
            }
            else
            {
                foreach (var item in result)
                {
                    var getShortName = locationList
                        .Where(x => x.LocationName.Contains(item.LocationName))
                        .Select(n => new
                        {
                            n.ShortName,
                        })
                        .FirstOrDefault();

                    var invoice = new InvoiceDto
                    {
                        HDR_TRX_NUMBER = item.TransactionNo + "I",
                        HDR_TRX_DATE = item.TransactionDate,
                        HDR_PAYMENT_TYPE = "HS",
                        HDR_BRANCH_CODE = getShortName.ShortName ?? "",
                        HDR_CUSTOMER_NUMBER = item.CustomerId + " P",
                        HDR_CUSTOMER_SITE = getShortName.ShortName ?? "",
                        HDR_PAYMENT_TERM = "0",
                        HDR_BUSINESS_LINE = "1",
                        HDR_BATCH_SOURCE_NAME = "POS",
                        HDR_GL_DATE = item.TransactionDate,
                        HDR_SOURCE_REFERENCE = "HS",
                        DTL_LINE_DESC = "GEI225101523-15",
                        DTL_QUANTITY = 1,
                        DTL_AMOUNT = item.Amount,
                        DTL_VAT_CODE = "",
                        DTL_CURRENCY = "PHP",
                        INVOICE_APPLIED = "0",
                        FILENAME = "SN" + DateTime.Now.ToString("MMddyy_hhmmss") + ".A01"
                    };

                    invoiceAnalytics.Add(invoice);
                }
                return (invoiceAnalytics, isPending);
            }
        }
    }
}

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
            //DateTime date;
            //var result = new List<AnalyticsDto>();
            //if (DateTime.TryParse(analyticsParamsDto.dates[0], out date))
            //{
            //    result = await _dbContext.Analytics.Where(x => x.TransactionDate == date && x.LocationId == analyticsParamsDto.storeId[0] && x.CustomerId == analyticsParamsDto.memCode[0])
            //     .Join(_dbContext.Locations, analytics => analytics.LocationId, location => location.LocationCode, (analytics, location) => new { analytics, location })
            //     .Select(n => new AnalyticsDto
            //     {
            //         Id = n.analytics.Id,
            //         CustomerId = n.analytics.CustomerId,
            //         LocationName = n.location.LocationName,
            //         TransactionDate = n.analytics.TransactionDate,
            //         MembershipNo = n.analytics.MembershipNo,
            //         CashierNo = n.analytics.CashierNo,
            //         RegisterNo = n.analytics.RegisterNo,
            //         TransactionNo = n.analytics.TransactionNo,
            //         OrderNo = n.analytics.OrderNo,
            //         Qty = n.analytics.Qty,
            //         Amount = n.analytics.Amount,
            //         UserId = n.analytics.UserId,
            //         DeleteFlag = n.analytics.DeleteFlag,
            //     })
            //     .ToListAsync();
            //}
            //return result;

            try
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
                    DeleteFlag = n.DeleteFlag,
                }).ToList();

                return analytics;
            }
            catch (Exception ex)
            {
                throw;
            }
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
                    .FromSqlRaw(
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
                        $"    ( " +
                        $"        SELECT " +
                        $"            a.[Id], " +
                        $"            c.CustomerName, " +
                        $"            l.LocationName, " +
                        $"            a.[TransactionDate], " +
                        $"            a.[OrderNo], " +
                        $"            a.[SubTotal] " +
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

        public async Task RefreshAnalytics(RefreshAnalyticsDto analyticsParam)
        {
            var listResultOne = new List<Analytics>();
            string strFrom = analyticsParam.dates[0].ToString("yyMMdd");
            string strTo = analyticsParam.dates[1].ToString("yyMMdd");
            string strStamp = $"{DateTime.Now.ToString("yyMMdd")}{DateTime.Now.ToString("HHmmss")}{DateTime.Now.Millisecond.ToString()}";
            string getQuery = string.Empty;
            var deptCodeList = await GetDepartments();
            var deptCodes = string.Join(", ", deptCodeList);

            //GRABFOOD
            string storeList1 = $"CSSTOR IN ({string.Join(", ", analyticsParam.storeId.Select(code => $"{code}"))})";
            string customerCode1 = string.Empty;
            string customerCode2 = string.Empty;
            if (analyticsParam.memCode.Contains("9999011910"))
            {
                customerCode1 = "9999999902";
                try
                {
                    await _dbContext.Database.ExecuteSqlRawAsync($"CREATE TABLE ANALYTICS_CSHHDR{strStamp} (CSDATE VARCHAR(255), CSSTOR INT, CSREG INT, CSTRAN INT, CSCUST VARCHAR(255), CSTAMT DECIMAL(18,3));");
                    await _dbContext.Database.ExecuteSqlRawAsync($"INSERT INTO ANALYTICS_CSHHDR{strStamp} (CSDATE, CSSTOR, CSREG, CSTRAN, CSCUST, CSTAMT) " +
                                        $"SELECT CSDATE, CSSTOR, CSREG, CSTRAN, CSCUST, CSTAMT  " +
                                        $"FROM OPENQUERY(SNR, 'SELECT CSDATE, CSSTOR, CSREG, CSTRAN, CSCUST, CSTAMT FROM MMJDALIB.CSHHDR WHERE CSDATE BETWEEN {strFrom} AND {strTo} AND {storeList1} AND CSCUST = ''{customerCode1}'' AND CSTSTS = ''0'''); ");
                }
                catch (Exception ex)
                {
                    await DropTables(strStamp);
                    throw;
                }

                try
                {
                    await _dbContext.Database.ExecuteSqlRawAsync($"CREATE TABLE ANALYTICS_CSHTND{strStamp} (CSDATE VARCHAR(255), CSSTOR INT, CSREG INT, CSTRAN INT, CSTDOC VARCHAR(50), CSCARD VARCHAR(50), CSDTYP VARCHAR(50));");
                    await _dbContext.Database.ExecuteSqlRawAsync($"INSERT INTO ANALYTICS_CSHTND{strStamp} (CSDATE, CSSTOR, CSREG, CSTRAN, CSTDOC, CSCARD, CSDTYP)  " +
                                $"SELECT A.CSDATE, A.CSSTOR, A.CSREG, A.CSTRAN, A.CSTDOC, A.CSCARD, A.CSDTYP " +
                                $"FROM OPENQUERY(SNR, 'SELECT CSDATE, CSSTOR, CSREG, CSTRAN, CSTDOC, CSCARD, CSDTYP FROM MMJDALIB.CSHTND WHERE CSDATE BETWEEN {strFrom} AND {strTo} AND {storeList1}') A  " +
                                $"INNER JOIN ANALYTICS_CSHHDR{strStamp} B  " +
                                $"ON A.CSDATE = B.CSDATE AND A.CSSTOR = B.CSSTOR AND A.CSREG = B.CSREG AND A.CSTRAN = B.CSTRAN; ");
                }
                catch (Exception ex)
                {
                    await DropTables(strStamp);
                    throw;
                }

                try
                {
                    // Create the table ANALYTICS_CONDTX + strStamp
                    await _dbContext.Database.ExecuteSqlRawAsync($"CREATE TABLE ANALYTICS_CONDTX{strStamp} (CSDATE VARCHAR(255), CSSTOR INT, CSREG INT, CSTRAN INT, CSQTY DECIMAL(18,3));");
                    // Insert data from MMJDALIB.CONDTX into the newly created table ANALYTICS_CONDTX + strStamp
                    await _dbContext.Database.ExecuteSqlRawAsync($"INSERT INTO ANALYTICS_CONDTX{strStamp} (CSDATE, CSSTOR, CSREG, CSTRAN, CSQTY)  " +
                                            $"SELECT A.CSDATE, A.CSSTOR, A.CSREG, A.CSTRAN, SUM(A.CSQTY) AS CSQTY  " +
                                            $"FROM OPENQUERY(SNR, 'SELECT CSDATE, CSSTOR, CSREG, CSTRAN, CSQTY FROM MMJDALIB.CONDTX WHERE CSSKU <> 0 AND CSDSTS = ''0'' AND CSDATE BETWEEN {strFrom} AND {strTo} AND {storeList1} ') A  " +
                                            $"INNER JOIN ANALYTICS_CSHTND{strStamp} B  " +
                                            $"ON A.CSDATE = B.CSDATE AND A.CSSTOR = B.CSSTOR AND A.CSREG = B.CSREG AND A.CSTRAN = B.CSTRAN " +
                                    $"GROUP BY A.CSDATE, A.CSSTOR, A.CSREG, A.CSTRAN");
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
                    //Insert the data from tbl_analytics
                    await _dbContext.Database.ExecuteSqlRawAsync($"INSERT INTO [dbo].[tbl_analytics] (CustomerId, LocationId, TransactionDate, MembershipNo, CashierNo, RegisterNo, TransactionNo, OrderNo, Qty, Amount, SubTotal, UserId, DeleteFlag) " +
                                        $"SELECT B.CSTDOC, E.STRNUM AS CSSTOR, A.CSDATE, A.CSCUST,'' AS CashierNo, A.CSREG, A.CSTRAN, B.CSCARD, C.CSQTY AS CSQTY, 0 AS CSEXPR, A.CSTAMT, NULL, 0 AS DeleteFlag  " +
                                        $"FROM ANALYTICS_CSHHDR{strStamp} A " +
                                            $"INNER JOIN ANALYTICS_CSHTND{strStamp} B ON A.CSSTOR = B.CSSTOR AND A.CSDATE = B.CSDATE AND A.CSREG = B.CSREG AND A.CSTRAN = B.CSTRAN " +
                                            $"INNER JOIN ANALYTICS_CONDTX{strStamp} C ON A.CSSTOR = C.CSSTOR AND A.CSDATE = C.CSDATE AND A.CSREG = C.CSREG AND A.CSTRAN = C.CSTRAN " +
                                            $"INNER JOIN ANALYTICS_TBLSTR{strStamp} E ON E.STRNUM = A.CSSTOR  " +
                                        $"ORDER BY A.CSSTOR,  A.CSDATE,  A.CSREG,  A.CSTRAN");

                    await DropTables(strStamp);
                }
                catch (Exception ex)
                {
                    await DropTables(strStamp);
                    throw;
                }
            }
            else
            {
                string cstDocCondition = $"CSTDOC IN ({string.Join(", ", analyticsParam.memCode.Select(code => $"''{code}''"))})";
                string storeList = $"CSSTOR IN ({string.Join(", ", analyticsParam.storeId.Select(code => $"{code}"))})";
                try
                {
                    await _dbContext.Database.ExecuteSqlRawAsync($"CREATE TABLE ANALYTICS_CSHTND{strStamp} (CSDATE VARCHAR(255), CSSTOR INT, CSREG INT, CSTRAN INT, CSTDOC VARCHAR(50), CSCARD VARCHAR(50), CSDTYP VARCHAR(50))");
                    // Insert data from MMJDALIB.CSHTND into the newly created table ANALYTICS_CSHTND + strStamp
                    await _dbContext.Database.ExecuteSqlRawAsync($"INSERT INTO ANALYTICS_CSHTND{strStamp} (CSDATE, CSSTOR, CSREG, CSTRAN, CSTDOC, CSCARD, CSDTYP) " +
                                      $"SELECT CSDATE, CSSTOR, CSREG, CSTRAN, CSTDOC, CSCARD, CSDTYP " +
                                      $"FROM OPENQUERY(SNR, 'SELECT CSDATE, CSSTOR, CSREG, CSTRAN, CSTDOC, CSCARD, CSDTYP FROM MMJDALIB.CSHTND WHERE (CSDATE BETWEEN {strFrom} AND {strTo}) AND {cstDocCondition} AND CSDTYP IN(''AR'') AND {storeList} ')");
                    // Create the table ANALYTICS_CSHHDR + strStamp
                    await _dbContext.Database.ExecuteSqlRawAsync($"CREATE TABLE ANALYTICS_CSHHDR{strStamp} (CSDATE VARCHAR(255), CSSTOR INT, CSREG INT, CSTRAN INT, CSCUST VARCHAR(255), CSTAMT DECIMAL(18,3))");
                    // Insert data from MMJDALIB.CSHHDR and ANALYTICS_CSHTND into the newly created table SALES_ANALYTICS_CSHHDR + strStamp
                    await _dbContext.Database.ExecuteSqlRawAsync($"INSERT INTO ANALYTICS_CSHHDR{strStamp} (CSDATE, CSSTOR, CSREG, CSTRAN, CSCUST, CSTAMT) " +
                                      $"SELECT A.CSDATE, A.CSSTOR, A.CSREG, A.CSTRAN, A.CSCUST, A.CSTAMT " +
                                      $"FROM OPENQUERY(SNR, 'SELECT CSDATE, CSSTOR, CSREG, CSTRAN, CSCUST, CSTAMT FROM MMJDALIB.CSHHDR WHERE CSDATE BETWEEN {strFrom} AND {strTo} AND {storeList} ') A " +
                                      $"INNER JOIN ANALYTICS_CSHTND{strStamp} B " +
                                      $"ON A.CSDATE = B.CSDATE AND A.CSSTOR = B.CSSTOR AND A.CSREG = B.CSREG AND A.CSTRAN = B.CSTRAN;");
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
                    await _dbContext.Database.ExecuteSqlRawAsync($"INSERT INTO ANALYTICS_CONDTX{strStamp} (CSDATE, CSSTOR, CSREG, CSTRAN, CSSKU, CSQTY, CSEXPR, CSEXCS, CSDSTS) " +
                                          $"SELECT A.CSDATE, A.CSSTOR, A.CSREG, A.CSTRAN, A.CSSKU, A.CSQTY, A.CSEXPR, A.CSEXCS, A.CSDSTS " +
                                          $"FROM OPENQUERY(SNR, 'SELECT CSDATE, CSSTOR, CSREG, CSTRAN, CSSKU, CSQTY, CSEXPR, CSEXCS, CSDSTS FROM MMJDALIB.CONDTX WHERE CSSKU <> 0 AND CSDSTS = ''0'' AND (CSDATE BETWEEN {strFrom} AND {strTo}) AND {storeList} ') A " +
                                          $"INNER JOIN ANALYTICS_CSHTND{strStamp} B " +
                                          $"ON A.CSDATE = B.CSDATE AND A.CSSTOR = B.CSSTOR AND A.CSREG = B.CSREG AND A.CSTRAN = B.CSTRAN");
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
                                              $"SELECT DISTINCT A.IDESCR, A.IDEPT, A.ISDEPT, A.ICLAS, A.ISCLAS, A.INUMBR " +
                                              $"FROM OPENQUERY(SNR, 'SELECT DISTINCT IDESCR, IDEPT, ISDEPT, ICLAS, ISCLAS, INUMBR FROM MMJDALIB.INVMST WHERE IDEPT IN ({deptCodes})') A " +
                                              $"INNER JOIN ANALYTICS_CONDTX{strStamp} B " +
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
                    //Insert the data from tbl_analytics
                    await _dbContext.Database.ExecuteSqlRawAsync($"INSERT INTO [dbo].[tbl_analytics] (CustomerId, LocationId, TransactionDate, MembershipNo, CashierNo, RegisterNo, TransactionNo, OrderNo, Qty, Amount, SubTotal, UserId, DeleteFlag) " +
                                      $"SELECT B.CSTDOC, E.STRNUM AS CSSTOR, C.CSDATE, A.CSCUST,'' AS CashierNo, C.CSREG, C.CSTRAN, B.CSCARD, SUM(C.CSQTY) AS CSQTY, SUM(C.CSEXPR) AS CSEXPR, A.CSTAMT, NULL , 0 AS DeleteFlag  " +
                                      $"FROM ANALYTICS_CSHHDR{strStamp} A " +
                                          $"INNER JOIN ANALYTICS_CSHTND{strStamp} B ON A.CSSTOR = B.CSSTOR AND A.CSDATE = B.CSDATE AND A.CSREG = B.CSREG AND A.CSTRAN = B.CSTRAN " +
                                          $"INNER JOIN ANALYTICS_CONDTX{strStamp} C ON A.CSSTOR = C.CSSTOR AND A.CSDATE = C.CSDATE AND A.CSREG = C.CSREG AND A.CSTRAN = C.CSTRAN " +
                                          $"INNER JOIN ANALYTICS_INVMST{strStamp} D ON C.CSSKU = D.INUMBR " +
                                          $"INNER JOIN ANALYTICS_TBLSTR{strStamp} E ON E.STRNUM = C.CSSTOR " +
                                      $"GROUP BY B.CSTDOC, E.STRNUM,  C.CSDATE,  A.CSCUST,  C.CSREG,  C.CSTRAN,  B.CSCARD,  A.CSTAMT " +
                                      $"ORDER BY E.STRNUM, C.CSDATE, C.CSREG");

                    await DropTables(strStamp);
                }
                catch (Exception ex)
                {
                    await DropTables(strStamp);
                    throw;
                }
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
    }
}

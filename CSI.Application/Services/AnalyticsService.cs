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

        public async Task<List<Analytics>> SalesAnalytics(AnalyticsParamsDto analyticsParam)
        {
            var listResultOne = new List<Analytics>();
            int store = 217; //Should be dynamic
            string strFrom = analyticsParam.dates[0].ToString("yyMMdd");
            string strTo = analyticsParam.dates[1].ToString("yyMMdd");
            string strStamp = $"{DateTime.Now.ToString("yyMMdd")}{DateTime.Now.ToString("HHmmss")}{DateTime.Now.Millisecond.ToString()}";
            string getQuery = string.Empty;
            var deptCodeList = await GetDepartments();
            var deptCodes = string.Join(", ", deptCodeList);

            string cstDocCondition = $"CSTDOC IN ({string.Join(", ", analyticsParam.memCode.Select(code => $"''{code}''"))})";
            //try
            //{
            //    await _dbContext.Database.ExecuteSqlRawAsync($"CREATE TABLE ANALYTICS_CSHTND{strStamp} (CSDATE VARCHAR(255), CSSTOR INT, CSREG INT, CSTRAN INT, CSTDOC VARCHAR(50), CSCARD VARCHAR(50), CSDTYP VARCHAR(50))");
            //    // Insert data from MMJDALIB.CSHTND into the newly created table SALES_ANALYTICS_CSHTND + strStamp
            //    await _dbContext.Database.ExecuteSqlRawAsync($"INSERT INTO ANALYTICS_CSHTND{strStamp} (CSDATE, CSSTOR, CSREG, CSTRAN, CSTDOC, CSCARD, CSDTYP) " +
            //                      $"SELECT CSDATE, CSSTOR, CSREG, CSTRAN, CSTDOC, CSCARD, CSDTYP " +
            //                      $"FROM OPENQUERY(SNR, 'SELECT CSDATE, CSSTOR, CSREG, CSTRAN, CSTDOC, CSCARD, CSDTYP FROM MMJDALIB.CSHTND WHERE (CSDATE BETWEEN {strFrom} AND {strTo}) AND {cstDocCondition} AND CSDTYP IN(''AR'') AND CSSTOR = {store} ')");
            //    // Create the table SALES_ANALYTICS_CSHHDR + strStamp
            //    await _dbContext.Database.ExecuteSqlRawAsync($"CREATE TABLE ANALYTICS_CSHHDR{strStamp} (CSDATE VARCHAR(255), CSSTOR INT, CSREG INT, CSTRAN INT, CSCUST VARCHAR(255), CSTAMT DECIMAL(18,3))");
            //    // Insert data from MMJDALIB.CSHHDR and SALES_ANALYTICS_CSHTND into the newly created table SALES_ANALYTICS_CSHHDR + strStamp
            //    await _dbContext.Database.ExecuteSqlRawAsync($"INSERT INTO ANALYTICS_CSHHDR{strStamp} (CSDATE, CSSTOR, CSREG, CSTRAN, CSCUST, CSTAMT) " +
            //                      $"SELECT A.CSDATE, A.CSSTOR, A.CSREG, A.CSTRAN, A.CSCUST, A.CSTAMT " +
            //                      $"FROM OPENQUERY(SNR, 'SELECT CSDATE, CSSTOR, CSREG, CSTRAN, CSCUST, CSTAMT FROM MMJDALIB.CSHHDR WHERE CSDATE BETWEEN {strFrom} AND {strTo} AND CSSTOR = {store} ') A " +
            //                      $"INNER JOIN ANALYTICS_CSHTND{strStamp} B " +
            //                      $"ON A.CSDATE = B.CSDATE AND A.CSSTOR = B.CSSTOR AND A.CSREG = B.CSREG AND A.CSTRAN = B.CSTRAN;");
            //}
            //catch (Exception)
            //{
            //    await DropTables(strStamp);
            //    throw;
            //}

            //try
            //{
            //    // Create the table SALES_ANALYTICS_CONDTX + strStamp
            //    await _dbContext.Database.ExecuteSqlRawAsync($"CREATE TABLE ANALYTICS_CONDTX{strStamp} (CSDATE VARCHAR(255), CSSTOR INT, CSREG INT, CSTRAN INT, CSSKU INT, CSQTY DECIMAL(18,3),  CSEXPR DECIMAL(18,3), CSEXCS DECIMAL(18,4), CSDSTS INT)");
            //    // Insert data from MMJDALIB.CONDTX into the newly created table SALES_ANALYTICS_CONDTX + strStamp
            //    await _dbContext.Database.ExecuteSqlRawAsync($"INSERT INTO ANALYTICS_CONDTX{strStamp} (CSDATE, CSSTOR, CSREG, CSTRAN, CSSKU, CSQTY, CSEXPR, CSEXCS, CSDSTS) " +
            //                          $"SELECT A.CSDATE, A.CSSTOR, A.CSREG, A.CSTRAN, A.CSSKU, A.CSQTY, A.CSEXPR, A.CSEXCS, A.CSDSTS " +
            //                          $"FROM OPENQUERY(SNR, 'SELECT CSDATE, CSSTOR, CSREG, CSTRAN, CSSKU, CSQTY, CSEXPR, CSEXCS, CSDSTS FROM MMJDALIB.CONDTX WHERE CSSKU <> 0 AND CSDSTS = ''0'' AND (CSDATE BETWEEN {strFrom} AND {strTo}) AND CSSTOR = {store}') A " +
            //                          $"INNER JOIN ANALYTICS_CSHTND{strStamp} B " +
            //                          $"ON A.CSDATE = B.CSDATE AND A.CSSTOR = B.CSSTOR AND A.CSREG = B.CSREG AND A.CSTRAN = B.CSTRAN");
            //}
            //catch (Exception)
            //{
            //    await DropTables(strStamp);
            //    throw;
            //}

            //try
            //{
            //    // Create the table SALES_ANALYTICS_INVMST + strStamp
            //    await _dbContext.Database.ExecuteSqlRawAsync($"CREATE TABLE ANALYTICS_INVMST{strStamp} (IDESCR VARCHAR(255), IDEPT INT, ISDEPT INT, ICLAS INT, ISCLAS INT, INUMBR INT)");
            //    // Insert data from MMJDALIB.INVMST into the newly created table SALES_ANALYTICS_INVMST + strStamp
            //    await _dbContext.Database.ExecuteSqlRawAsync($"INSERT INTO ANALYTICS_INVMST{strStamp} (IDESCR, IDEPT, ISDEPT, ICLAS, ISCLAS, INUMBR) " +
            //                              $"SELECT DISTINCT A.IDESCR, A.IDEPT, A.ISDEPT, A.ICLAS, A.ISCLAS, A.INUMBR " +
            //                              $"FROM OPENQUERY(SNR, 'SELECT DISTINCT IDESCR, IDEPT, ISDEPT, ICLAS, ISCLAS, INUMBR FROM MMJDALIB.INVMST WHERE IDEPT IN ({deptCodes})') A " +
            //                              $"INNER JOIN ANALYTICS_CONDTX{strStamp} B " +
            //                              $"ON A.INUMBR = B.CSSKU");
            //}
            //catch (Exception)
            //{
            //    await DropTables(strStamp);
            //    throw;
            //}

            //try
            //{
            //    // Create the table SALES_ANALYTICS_TBLSTR + strStamp
            //    await _dbContext.Database.ExecuteSqlRawAsync($"CREATE TABLE ANALYTICS_TBLSTR{strStamp} (STRNUM INT, STRNAM VARCHAR(255))");
            //    // Insert data from MMJDALIB.TBLSTR into the newly created table SALES_ANALYTICS_TBLSTR + strStamp
            //    await _dbContext.Database.ExecuteSqlRawAsync($"INSERT INTO ANALYTICS_TBLSTR{strStamp} (STRNUM, STRNAM) " +
            //                            $"SELECT * FROM OPENQUERY(SNR, 'SELECT STRNUM, STRNAM FROM MMJDALIB.TBLSTR') ");
            //}
            //catch (Exception)
            //{
            //    await DropTables(strStamp);
            //    throw;
            //}

            try
            {
                ////Insert the data from tbl_analytics
                //await _dbContext.Database.ExecuteSqlRawAsync($"INSERT INTO [dbo].[tbl_analytics] (CustomerId, LocationId, TransactionDate, MembershipNo, CashierNo, RegisterNo, TransactionNo, OrderNo, Qty, Amount, UserId, DeleteFlag) " +
                //                  $"SELECT B.CSTDOC, E.STRNUM AS CSSTOR, C.CSDATE, A.CSCUST,'' AS CashierNo, C.CSREG, C.CSTRAN, B.CSCARD, SUM(C.CSQTY) AS CSQTY, SUM(C.CSEXPR) AS CSEXPR, '{analyticsParam.userId}' AS UserId, 0 AS DeleteFlag  " +
                //                  $"FROM ANALYTICS_CSHHDR{strStamp} A " +
                //                      $"INNER JOIN ANALYTICS_CSHTND{strStamp} B ON A.CSSTOR = B.CSSTOR AND A.CSDATE = B.CSDATE AND A.CSREG = B.CSREG AND A.CSTRAN = B.CSTRAN " +
                //                      $"INNER JOIN ANALYTICS_CONDTX{strStamp} C ON A.CSSTOR = C.CSSTOR AND A.CSDATE = C.CSDATE AND A.CSREG = C.CSREG AND A.CSTRAN = C.CSTRAN " +
                //                      $"INNER JOIN ANALYTICS_INVMST{strStamp} D ON C.CSSKU = D.INUMBR " +
                //                      $"INNER JOIN ANALYTICS_TBLSTR{strStamp} E ON E.STRNUM = C.CSSTOR " +
                //                  $"GROUP BY B.CSTDOC, E.STRNUM, C.CSDATE, A.CSCUST, C.CSREG, C.CSTRAN, B.CSCARD " +
                //                  $"ORDER BY E.STRNUM, C.CSDATE, C.CSREG");

                //await DropTables(strStamp);


                string date1 = analyticsParam.dates[0].ToString("MM-dd-yyyy");
                string date2 = analyticsParam.dates[1].ToString("MM-dd-yyyy");

                listResultOne = await _dbContext.Analytics
                    .Where(x => x.TransactionDate >= DateTime.Parse(date1) && x.TransactionDate <= DateTime.Parse(date2) && x.LocationId == store && analyticsParam.memCode.Contains(x.CustomerId))
                    .ToListAsync();
            }
            catch (Exception)
            {
                await DropTables(strStamp);
                throw;
            }
            return listResultOne;
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
    }
}

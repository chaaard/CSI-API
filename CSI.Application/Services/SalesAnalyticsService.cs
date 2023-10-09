using CSI.Application.DTOs;
using CSI.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using CSI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CSI.Application.Services
{
    public class SalesAnalyticsService : ISalesAnalyticsService
    {
        private readonly AppDBContext _dbContext;
        private readonly IConfiguration _configuration;

        public SalesAnalyticsService(IConfiguration configuration, AppDBContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        public async Task<List<SalesAnalyticsDto>> SalesAnalytics(SalesAnalyticsParamsDto salesParam)
        {
            var listResultOne = new List<SalesAnalyticsDto>();
            int store = 201; //Should be dynamic
            string strFrom = salesParam.dates[0].ToString("yyMMdd");
            string strTo = salesParam.dates[1].ToString("yyMMdd");
            string strStamp = $"{DateTime.Now.ToString("yyMMdd")}{DateTime.Now.ToString("HHmmss")}{DateTime.Now.Millisecond.ToString()}";
            string getQuery = string.Empty;
            var deptCodeList = await GetDepartments();
            var deptCodes = string.Join(", ", deptCodeList);

            using (MsSqlCon db = new MsSqlCon(_configuration))
            {
                try
                {
                    if (db.Con.State == ConnectionState.Closed)
                    {
                        await db.Con.OpenAsync();
                    }

                    string cstDocCondition = $"CSTDOC IN ({string.Join(", ", salesParam.memCode.Select(code => $"''{code}''"))})";

                    using (var cmd = new SqlCommand())
                    {
                        cmd.Connection = db.Con;
                        cmd.CommandTimeout = 0;
                        cmd.CommandType = CommandType.Text;
                        // Create the table SALES_ANALYTICS_CSHTND + strStamp
                        cmd.CommandText = $"CREATE TABLE SALES_ANALYTICS_CSHTND{strStamp} (CSDATE VARCHAR(255), CSSTOR INT, CSREG INT, CSTRAN INT, CSTDOC VARCHAR(50), CSCARD VARCHAR(50), CSDTYP VARCHAR(50))";
                        await cmd.ExecuteNonQueryAsync();
                        // Insert data from MMJDALIB.CSHTND into the newly created table SALES_ANALYTICS_CSHTND + strStamp
                        cmd.CommandText = $"INSERT INTO SALES_ANALYTICS_CSHTND{strStamp} (CSDATE, CSSTOR, CSREG, CSTRAN, CSTDOC, CSCARD, CSDTYP) " +
                                          $"SELECT CSDATE, CSSTOR, CSREG, CSTRAN, CSTDOC, CSCARD, CSDTYP " +
                                          $"FROM OPENQUERY(SNR, 'SELECT CSDATE, CSSTOR, CSREG, CSTRAN, CSTDOC, CSCARD, CSDTYP FROM MMJDALIB.CSHTND WHERE (CSDATE BETWEEN {strFrom} AND {strTo}) AND {cstDocCondition} AND CSDTYP IN(''AR'') AND CSSTOR = {store} ')";
                        await cmd.ExecuteNonQueryAsync();
                        // Create the table SALES_ANALYTICS_CSHHDR + strStamp
                        cmd.CommandText = $"CREATE TABLE SALES_ANALYTICS_CSHHDR{strStamp} (CSDATE VARCHAR(255), CSSTOR INT, CSREG INT, CSTRAN INT, CSCUST VARCHAR(255), CSTAMT DECIMAL(18,3))";
                        await cmd.ExecuteNonQueryAsync();
                        // Insert data from MMJDALIB.CSHHDR and SALES_ANALYTICS_CSHTND into the newly created table SALES_ANALYTICS_CSHHDR + strStamp
                        cmd.CommandText = $"INSERT INTO SALES_ANALYTICS_CSHHDR{strStamp} (CSDATE, CSSTOR, CSREG, CSTRAN, CSCUST, CSTAMT) " +
                                          $"SELECT A.CSDATE, A.CSSTOR, A.CSREG, A.CSTRAN, A.CSCUST, A.CSTAMT " +
                                          $"FROM OPENQUERY(SNR, 'SELECT CSDATE, CSSTOR, CSREG, CSTRAN, CSCUST, CSTAMT FROM MMJDALIB.CSHHDR WHERE CSDATE BETWEEN {strFrom} AND {strTo} AND CSSTOR = {store} ') A " +
                                          $"INNER JOIN SALES_ANALYTICS_CSHTND{strStamp} B " +
                                          $"ON A.CSDATE = B.CSDATE AND A.CSSTOR = B.CSSTOR AND A.CSREG = B.CSREG AND A.CSTRAN = B.CSTRAN;";
                        await cmd.ExecuteNonQueryAsync();
                    }

                    await db.Con.CloseAsync();
                }
                catch (Exception)
                {
                    await db.Con.CloseAsync();
                    await DropTables(strStamp, db);
                    throw;
                }

                try
                {
                    if (db.Con.State == ConnectionState.Closed)
                    {
                        await db.Con.OpenAsync();
                    }

                    using (var cmd = new SqlCommand())
                    { 
                        cmd.Connection = db.Con;
                        cmd.CommandTimeout = 0;
                        cmd.CommandType = CommandType.Text;
                        // Create the table SALES_ANALYTICS_CONDTX + strStamp
                        cmd.CommandText = $"CREATE TABLE SALES_ANALYTICS_CONDTX{strStamp} (CSDATE VARCHAR(255), CSSTOR INT, CSREG INT, CSTRAN INT, CSSKU INT, CSQTY DECIMAL(18,3),  CSEXPR DECIMAL(18,3), CSEXCS DECIMAL(18,4), CSDSTS INT)";
                        await cmd.ExecuteNonQueryAsync();
                        // Insert data from MMJDALIB.CONDTX into the newly created table SALES_ANALYTICS_CONDTX + strStamp
                        cmd.CommandText = $"INSERT INTO SALES_ANALYTICS_CONDTX{strStamp} (CSDATE, CSSTOR, CSREG, CSTRAN, CSSKU, CSQTY, CSEXPR, CSEXCS, CSDSTS) " +
                                          $"SELECT A.CSDATE, A.CSSTOR, A.CSREG, A.CSTRAN, A.CSSKU, A.CSQTY, A.CSEXPR, A.CSEXCS, A.CSDSTS " +
                                          $"FROM OPENQUERY(SNR, 'SELECT CSDATE, CSSTOR, CSREG, CSTRAN, CSSKU, CSQTY, CSEXPR, CSEXCS, CSDSTS FROM MMJDALIB.CONDTX WHERE CSSKU <> 0 AND CSDSTS = ''0'' AND (CSDATE BETWEEN {strFrom} AND {strTo}) AND CSSTOR = {store}') A " +
                                          $"INNER JOIN SALES_ANALYTICS_CSHTND{strStamp} B " +
                                          $"ON A.CSDATE = B.CSDATE AND A.CSSTOR = B.CSSTOR AND A.CSREG = B.CSREG AND A.CSTRAN = B.CSTRAN";
                        await cmd.ExecuteNonQueryAsync();
                    }

                    await db.Con.CloseAsync();
                }
                catch (Exception)
                {
                    await db.Con.CloseAsync();
                    await DropTables(strStamp, db);
                    throw;
                }


                try
                {
                    if (db.Con.State == ConnectionState.Closed)
                    {
                        await db.Con.OpenAsync();
                    }

                    using (var cmd = new SqlCommand())
                    {
                        cmd.Connection = db.Con;
                        cmd.CommandTimeout = 0;
                        cmd.CommandType = CommandType.Text;
                        // Create the table SALES_ANALYTICS_INVMST + strStamp
                        cmd.CommandText = $"CREATE TABLE SALES_ANALYTICS_INVMST{strStamp} (IDESCR VARCHAR(255), IDEPT INT, ISDEPT INT, ICLAS INT, ISCLAS INT, INUMBR INT)";
                        await cmd.ExecuteNonQueryAsync();
                        // Insert data from MMJDALIB.INVMST into the newly created table SALES_ANALYTICS_INVMST + strStamp
                        cmd.CommandText = $"INSERT INTO SALES_ANALYTICS_INVMST{strStamp} (IDESCR, IDEPT, ISDEPT, ICLAS, ISCLAS, INUMBR) " +
                                          $"SELECT DISTINCT A.IDESCR, A.IDEPT, A.ISDEPT, A.ICLAS, A.ISCLAS, A.INUMBR " +
                                          $"FROM OPENQUERY(SNR, 'SELECT DISTINCT IDESCR, IDEPT, ISDEPT, ICLAS, ISCLAS, INUMBR FROM MMJDALIB.INVMST WHERE IDEPT IN ({deptCodes})') A " +
                                          $"INNER JOIN SALES_ANALYTICS_CONDTX{strStamp} B " +
                                          $"ON A.INUMBR = B.CSSKU";
                        await cmd.ExecuteNonQueryAsync();
                    }

                    await db.Con.CloseAsync();
                }
                catch (Exception)
                {
                    await db.Con.CloseAsync();
                    await DropTables(strStamp, db);
                    throw;
                }

                try
                {
                    if (db.Con.State == ConnectionState.Closed)
                    {
                        await db.Con.OpenAsync();
                    }

                    using (var cmd = new SqlCommand())
                    {
                        cmd.Connection = db.Con;
                        cmd.CommandTimeout = 0;
                        cmd.CommandType = CommandType.Text;
                        // Create the table SALES_ANALYTICS_TBLSTR + strStamp
                        cmd.CommandText = $"CREATE TABLE SALES_ANALYTICS_TBLSTR{strStamp} (STRNUM INT, STRNAM VARCHAR(255))";
                        await cmd.ExecuteNonQueryAsync();
                        // Insert data from MMJDALIB.TBLSTR into the newly created table SALES_ANALYTICS_TBLSTR + strStamp
                        cmd.CommandText = $"INSERT INTO SALES_ANALYTICS_TBLSTR{strStamp} (STRNUM, STRNAM) " +
                                          $"SELECT * FROM OPENQUERY(SNR, 'SELECT STRNUM, STRNAM FROM MMJDALIB.TBLSTR') ";
                        await cmd.ExecuteNonQueryAsync();
                    }

                    await db.Con.CloseAsync();
                }
                catch (Exception)
                {
                    await db.Con.CloseAsync();
                    await DropTables(strStamp, db);
                    throw;
                }

                try
                {
                    if (db.Con.State == ConnectionState.Closed)
                    {
                        await db.Con.OpenAsync();
                    }


                    using (var cmd = new SqlCommand())
                    {
                        cmd.Connection = db.Con;
                        cmd.CommandTimeout = 0;
                        cmd.CommandType = CommandType.Text;
                        //Insert the data from tbl_sales_analytics
                        cmd.CommandText = $"INSERT INTO[dbo].[tbl_sales_analytics] (CSSTOR, CSDATE, CSCUST, CSREG, CSTRAN, CSCARD, CSQTY, CSEXPR, CSTAMT) " +
                                          $"SELECT E.STRNAM AS CSSTOR, C.CSDATE, A.CSCUST, C.CSREG, C.CSTRAN, B.CSCARD, SUM(C.CSQTY) AS CSQTY, SUM(C.CSEXPR) AS CSEXPR, A.CSTAMT " +
                                          $"FROM SALES_ANALYTICS_CSHHDR{strStamp} A " +
                                              $"INNER JOIN SALES_ANALYTICS_CSHTND{strStamp} B ON A.CSSTOR = B.CSSTOR AND A.CSDATE = B.CSDATE AND A.CSREG = B.CSREG AND A.CSTRAN = B.CSTRAN " +
                                              $"INNER JOIN SALES_ANALYTICS_CONDTX{strStamp} C ON A.CSSTOR = C.CSSTOR AND A.CSDATE = C.CSDATE AND A.CSREG = C.CSREG AND A.CSTRAN = C.CSTRAN " +
                                              $"INNER JOIN SALES_ANALYTICS_INVMST{strStamp} D ON C.CSSKU = D.INUMBR " +
                                              $"INNER JOIN SALES_ANALYTICS_TBLSTR{strStamp} E ON E.STRNUM = C.CSSTOR " +
                                          $"GROUP BY E.STRNAM, C.CSDATE, A.CSCUST, C.CSREG, C.CSTRAN, B.CSCARD, A.CSTAMT " +
                                          $"ORDER BY E.STRNAM, C.CSDATE, C.CSREG";
                        await cmd.ExecuteNonQueryAsync();
                    }

                    await db.Con.CloseAsync();

                    await DropTables(strStamp, db);

                    listResultOne = await _dbContext.SalesAnalytics
                        .Select(result => new SalesAnalyticsDto
                        {
                            CSSTOR = result.CSSTOR,
                            CSDATE = result.CSDATE,
                            CSCUST = result.CSCUST,
                            CSREG = result.CSREG,
                            CSTRAN = result.CSTRAN,
                            CSCARD = result.CSCARD,
                            CSQTY = result.CSQTY,
                            CSEXPR = result.CSEXPR,
                            CSTAMT = result.CSTAMT,
                        })
                        .ToListAsync();

                }
                catch (Exception)
                {

                    await db.Con.CloseAsync();
                    await DropTables(strStamp, db);
                    throw;
                }
            }

            return listResultOne;
        }

        private async Task DropTables(string strStamp, MsSqlCon db)
        {
            var cmd = new SqlCommand();
            cmd.Connection = db.Con;
            cmd.CommandTimeout = 0;
            cmd.CommandType = CommandType.Text;

            try
            {
                if (db.Con.State == ConnectionState.Closed)
                {
                    await db.Con.OpenAsync();
                }

                cmd.CommandText = $"IF OBJECT_ID('SALES_ANALYTICS_CSHTND{strStamp}', 'U') IS NOT NULL " +
                                  $"DROP TABLE SALES_ANALYTICS_CSHTND{strStamp}";
                await cmd.ExecuteNonQueryAsync();

                cmd.CommandText = $"IF OBJECT_ID('SALES_ANALYTICS_CSHHDR{strStamp}', 'U') IS NOT NULL " +
                                  $"DROP TABLE SALES_ANALYTICS_CSHHDR{strStamp}";
                await cmd.ExecuteNonQueryAsync();

                cmd.CommandText = $"IF OBJECT_ID('SALES_ANALYTICS_CONDTX{strStamp}', 'U') IS NOT NULL " +
                                  $"DROP TABLE SALES_ANALYTICS_CONDTX{strStamp}";
                await cmd.ExecuteNonQueryAsync();

                cmd.CommandText = $"IF OBJECT_ID('SALES_ANALYTICS_INVMST{strStamp}', 'U') IS NOT NULL " +
                                  $"DROP TABLE SALES_ANALYTICS_INVMST{strStamp}";
                await cmd.ExecuteNonQueryAsync();

                cmd.CommandText = $"IF OBJECT_ID('SALES_ANALYTICS_TBLSTR{strStamp}', 'U') IS NOT NULL " +
                                  $"DROP TABLE SALES_ANALYTICS_TBLSTR{strStamp}";
                await cmd.ExecuteNonQueryAsync();

                await db.Con.CloseAsync();
            }
            catch (Exception)
            {
                await db.Con.CloseAsync();
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

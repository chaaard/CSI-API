using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSI.Infrastructure.Data
{
    public class MsSqlCon : IDisposable
    {
        public SqlConnection Con;
        private readonly IConfiguration _configuration;

        public MsSqlCon(IConfiguration configuration)
        {
            _configuration = configuration;
            var strConn = _configuration["ConnectionStrings:SQLCon"];
            Con = new SqlConnection(strConn);
            if (this.Con.State == ConnectionState.Closed)
            {
                this.Con.Open();
            }
        }

        public MsSqlCon(string ConnectionString)
        {
            Con = new SqlConnection(ConnectionString);

            if (this.Con.State == ConnectionState.Closed)
            {
                this.Con.Open();
            }
        }

        public void Dispose()
        {
            Con.Close();
        }
    }
}

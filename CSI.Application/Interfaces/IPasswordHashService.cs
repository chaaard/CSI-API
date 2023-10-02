using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSI.Application.Interfaces
{
    public interface IPasswordHashService
    {
        string HashPassword(string password, string salt, int nIterations, int nHash);
        string GenerateSalt(int nSalt);
    }
}

using CSI.Application.DTOs;
using CSI.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSI.Application.Interfaces
{
    public interface IProofListService
    {
        (List<Prooflist>?, string?) ReadProofList(IFormFile file, string customerName, string strClub, string selectedDate);
        Task<List<PortalDto>> GetPortal(PortalParamsDto portalParamsDto);
    }
}

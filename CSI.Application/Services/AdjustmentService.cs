using AutoMapper;
using CSI.Application.DTOs;
using CSI.Application.Interfaces;
using CSI.Domain.Entities;
using CSI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSI.Application.Services
{
    public class AdjustmentService : IAdjustmentService
    {
        private readonly AppDBContext _dbContext;
        private readonly IMapper _mapper;

        public AdjustmentService(AppDBContext dBContext, IMapper mapper)
        {
            _dbContext = dBContext;
            _mapper = mapper;
        }

        public async Task<(List<AdjustmentDto>, int totalPages)> GetAdjustmentsAsync(AdjustmentParams adjustmentParams)
        {
            DateTime date;

            IQueryable<AdjustmentDto> query = Enumerable.Empty<AdjustmentDto>().AsQueryable();
            if (DateTime.TryParse(adjustmentParams.dates[0], out date))
            {
                 query = _dbContext.AnalyticsProoflist
                .Join(_dbContext.Analytics, ap => ap.AnalyticsId, a => a.Id, (ap, a) => new { ap, a })
                .GroupJoin(_dbContext.Prooflist, x => x.ap.ProoflistId, p => p.Id, (x, p) => new { x.ap, x.a, Prooflist = p })
                .SelectMany(x => x.Prooflist.DefaultIfEmpty(), (x, p) => new { x.ap, x.a, Prooflist = p })
                .Join(_dbContext.CustomerCodes, x => x.a.CustomerId, c => c.CustomerCode, (x, c) => new { x.ap, x.a, x.Prooflist, Customer = c })
                .Join(_dbContext.Adjustments, x => x.ap.AdjustmentId, ad => ad.Id, (x, ad) => new { x.ap, x.a, x.Prooflist, x.Customer, Adjustment = ad })
                .Join(_dbContext.Actions, x => x.ap.ActionId, ac => ac.Id, (x, ac) => new { x.ap, x.a, x.Prooflist, x.Customer, x.Adjustment, Action = ac })
                .Join(_dbContext.Status, x => x.ap.StatusId, s => s.Id, (x, s) => new { x.ap, x.a, x.Prooflist, x.Customer, x.Adjustment, x.Action, Status = s })
                .Where(x => x.a.TransactionDate == date && x.a.LocationId == adjustmentParams.storeId[0] && x.a.CustomerId == adjustmentParams.memCode[0])
                .Select(x => new AdjustmentDto
                {
                    Id = x.ap.Id,
                    CustomerId = x.Customer.CustomerName,
                    JoNumber = x.Adjustment.NewJO != null ? x.Adjustment.NewJO : x.a.OrderNo,
                    TransactionDate = x.a.TransactionDate,
                    Amount = x.a.Amount,
                    AdjustmentType = x.Action.Action,
                    Status = x.Status.StatusName
                })
                .OrderBy(x => x.Id);
            }

            var totalItemCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItemCount / adjustmentParams.PageSize);

            var customerCodesList = await query
                .Skip((adjustmentParams.PageNumber - 1) * adjustmentParams.PageSize)
                .Take(adjustmentParams.PageSize)
                .OrderBy(x => x.Id)
                .ToListAsync();

            return (customerCodesList, totalPages);
        }

        public async Task<AnalyticsProoflist> CreateAnalyticsProofList(AnalyticsProoflistDto adjustmentTypeDto)
        {
            var analyticsProoflist = new AnalyticsProoflist();
            var adjustmentId = await CreateAdjustment(adjustmentTypeDto.AdjustmentAddDto);

            if (adjustmentId != 0)
            {
                adjustmentTypeDto.AdjustmentId = adjustmentId;

                analyticsProoflist = _mapper.Map<AnalyticsProoflistDto, AnalyticsProoflist>(adjustmentTypeDto);
                _dbContext.AnalyticsProoflist.Add(analyticsProoflist);
                await _dbContext.SaveChangesAsync();

                return analyticsProoflist;
            }

            return analyticsProoflist;
        }

        public async Task<int> CreateAdjustment(AdjustmentAddDto? adjustmentAddDto)
        {
            try
            {
                var id = 0;
                if (adjustmentAddDto != null)
                {
                    var adjustments = _mapper.Map<AdjustmentAddDto, Adjustments>(adjustmentAddDto);
                    _dbContext.Adjustments.Add(adjustments);
                    await _dbContext.SaveChangesAsync();

                    id = adjustments.Id;

                    return id;
                }
                return id;
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                throw;
            }
        }

        public async Task<List<Reasons>> GetReasonsAsync()
        {
            try
            {
                var reasons = await _dbContext.Reasons
                        .Where(x => x.DeleteFlag == false)
                        .ToListAsync();
              
                return reasons;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> UpdateJO(AnalyticsProoflistDto adjustmentTypeDto)
        {
            var result = false;
            try
            {
                if (adjustmentTypeDto != null)
                {
                    var matchRow = await _dbContext.Analytics
                       .Where(x => x.Id == adjustmentTypeDto.AnalyticsId)
                       .FirstOrDefaultAsync();

                    if (matchRow != null)
                    {
                        matchRow.OrderNo = adjustmentTypeDto?.AdjustmentAddDto?.NewJO;
                        await _dbContext.SaveChangesAsync();
                        result = true;
                    }
                }
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> UpdatePartner(AnalyticsProoflistDto adjustmentTypeDto)
        {
            var result = false;
            try
            {
                if (adjustmentTypeDto != null)
                {
                    var matchRow = await _dbContext.Analytics
                       .Where(x => x.Id == adjustmentTypeDto.AnalyticsId)
                       .FirstOrDefaultAsync();

                    if (matchRow != null)
                    {
                        matchRow.CustomerId = adjustmentTypeDto?.AdjustmentAddDto?.CustomerId;
                        await _dbContext.SaveChangesAsync();
                        result = true;
                    }
                }
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<TransactionDtos> GetTotalCountAmount(TransactionCountAmountDto transactionCountAmountDto)
        {
            DateTime date1;
            DateTime date2;
            DateTime.TryParse(transactionCountAmountDto.dates[0], out date1);
            DateTime.TryParse(transactionCountAmountDto.dates[1], out date2);

            var result = await _dbContext.AnalyticsProoflist
                 .Where(ap => ap.ActionId == transactionCountAmountDto.actionId && ap.StatusId == transactionCountAmountDto.statusId)
                 .Join(
                     _dbContext.Analytics,
                     ap => ap.AnalyticsId,
                     a => a.Id,
                     (ap, a) => new { ap, a }
                 )
                 .Where(joined => joined.a.LocationId == transactionCountAmountDto.storeId[0] && joined.a.TransactionDate >= date1 && joined.a.TransactionDate <= date2)
                 .GroupBy(joined => new { joined.ap.ActionId })
                 .Select(grouped => new TransactionDtos
                 {
                     Count = grouped.Count(),
                     Amount = grouped.Sum(j => j.a.Amount)
                 })
                 .FirstOrDefaultAsync();

            return result ?? new TransactionDtos();
        }
    }
}

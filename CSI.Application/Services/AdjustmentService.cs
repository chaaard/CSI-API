﻿using AutoMapper;
using CSI.Application.DTOs;
using CSI.Application.Interfaces;
using CSI.Domain.Entities;
using CSI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
            var result = await _dbContext.AdjustmentExceptions
                   .FromSqlRaw($"SELECT ap.Id, c.CustomerName, a.OrderNo, a.TransactionDate, a.SubTotal, act.Action, " +
                            $"so.SourceType, st.StatusName, ap.AdjustmentId, lo.LocationName, ap.AnalyticsId, ap.ProoflistId, " +
                            $"adj.OldJO, adj.CustomerIdOld, adj.DisputeReferenceNumber, adj.DisputeAmount, adj.DateDisputeFiled, adj.DescriptionOfDispute, " +
                            $"adj.AccountsPaymentDate, adj.AccountsPaymentTransNo, adj.AccountsPaymentAmount,  adj.ReasonId, " +
                            $"adj.Descriptions " +
                            $"FROM [dbo].[tbl_analytics_prooflist] ap " +
                            $"	LEFT JOIN [dbo].[tbl_analytics] a ON a.Id = ap.AnalyticsId " +
                            $"	LEFT JOIN [dbo].[tbl_prooflist] p ON p.Id = ap.ProoflistId " +
                            $"	LEFT JOIN [dbo].[tbl_customer] c ON c.CustomerCode = a.CustomerId " +
                            $"	LEFT JOIN [dbo].[tbl_action] act ON act.Id = ap.ActionId " +
                            $"	LEFT JOIN [dbo].[tbl_adjustments] adj ON adj.Id = ap.AdjustmentId " +
                            $"	LEFT JOIN [dbo].[tbl_status] st ON st.Id = ap.StatusId " +
                            $"	LEFT JOIN [dbo].[tbl_source] so ON so.Id = ap.SourceId " +
                            $"	LEFT JOIN [dbo].[tbl_location] lo ON lo.LocationCode = a.LocationId " +
                            $"WHERE a.TransactionDate = '{adjustmentParams.dates[0].ToString()}' AND a.LocationId = {adjustmentParams.storeId[0]} AND a.CustomerId = '{adjustmentParams.memCode[0]}' " +
                            $"UNION ALL " +
                            $"SELECT ap.Id, c.CustomerName, p.OrderNo, p.TransactionDate, p.Amount, act.Action,  " +
                            $"	so.SourceType, st.StatusName, ap.AdjustmentId, lo.LocationName, ap.AnalyticsId, ap.ProoflistId, " +
                            $"	adj.OldJO, adj.CustomerIdOld, adj.DisputeReferenceNumber, adj.DisputeAmount, adj.DateDisputeFiled, adj.DescriptionOfDispute, " +
                            $"	adj.AccountsPaymentDate, adj.AccountsPaymentTransNo, adj.AccountsPaymentAmount,  adj.ReasonId, " +
                            $"	adj.Descriptions " +
                            $"FROM [dbo].[tbl_analytics_prooflist] ap " +
                            $"	LEFT JOIN [dbo].[tbl_analytics] a ON a.Id = ap.AnalyticsId " +
                            $"	LEFT JOIN [dbo].[tbl_prooflist] p ON p.Id = ap.ProoflistId " +
                            $"	LEFT JOIN [dbo].[tbl_customer] c ON c.CustomerCode = p.CustomerId " +
                            $"	LEFT JOIN [dbo].[tbl_action] act ON act.Id = ap.ActionId " +
                            $"	LEFT JOIN [dbo].[tbl_adjustments] adj ON adj.Id = ap.AdjustmentId " +
                            $"	LEFT JOIN [dbo].[tbl_status] st ON st.Id = ap.StatusId " +
                            $"	LEFT JOIN [dbo].[tbl_source] so ON so.Id = ap.SourceId " +
                            $"	LEFT JOIN [dbo].[tbl_location] lo ON lo.LocationCode = p.StoreId " +
                            $"WHERE p.TransactionDate = '{adjustmentParams.dates[0].ToString()}' AND p.StoreId = {adjustmentParams.storeId[0]} AND p.CustomerId = '{adjustmentParams.memCode[0]}' AND so.SourceType = 'Portal' " +
                            $" ORDER BY so.SourceType, a.SubTotal ASC ")
                   .ToListAsync();

            query = result.Select(m => new AdjustmentDto
            {
                Id = m.Id,
                CustomerId = m.CustomerName,
                JoNumber = m.OrderNo,
                TransactionDate = m.TransactionDate,
                Amount = m.SubTotal,
                AdjustmentType = m.Action,
                Source = m.SourceType,
                Status = m.StatusName,
                AdjustmentId = m.AdjustmentId,
                LocationName = m.LocationName,
                AnalyticsId = m.AnalyticsId,
                ProofListId = m.ProoflistId,
                OldJo = m.OldJO,
                OldCustomerId = m.CustomerIdOld,
                DisputeReferenceNumber = m.DisputeReferenceNumber,
                DisputeAmount = m.DisputeAmount,
                DateDisputeFiled = m.DateDisputeFiled,
                DescriptionOfDispute = m.DescriptionOfDispute,
                AccountsPaymentDate = m.AccountsPaymentDate,
                AccountsPaymentTransNo = m.AccountsPaymentTransNo,
                AccountsPaymentAmount = m.AccountsPaymentAmount,
                ReasonId = m.ReasonId,
                Descriptions = m.Descriptions
            }).AsQueryable();

            var totalItemCount = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalItemCount / adjustmentParams.PageSize);

            var customerCodesList = query
                .Skip((adjustmentParams.PageNumber - 1) * adjustmentParams.PageSize)
                .Take(adjustmentParams.PageSize)
                .OrderBy(x => x.Source).OrderBy(x => x.Amount)
                .ToList();

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

        public async Task<bool> UpdateAnalyticsProofList(AnalyticsProoflistDto adjustmentTypeDto)
        {
            var result = false;
            var adjustmentStatus = await _dbContext.AnalyticsProoflist
                      .Where(x => x.Id == adjustmentTypeDto.Id)
                      .FirstOrDefaultAsync();

            if (adjustmentStatus != null)
            {
                adjustmentStatus.StatusId = adjustmentTypeDto.StatusId ?? 0;
                adjustmentStatus.ActionId = adjustmentTypeDto.ActionId ?? 0;
                await _dbContext.SaveChangesAsync();
            }

            var adjustments = await _dbContext.Adjustments
                      .Where(x => x.Id == adjustmentTypeDto.AdjustmentId)
                      .FirstOrDefaultAsync();

            if (adjustments != null)
            {
                adjustments.DisputeReferenceNumber = adjustmentTypeDto?.AdjustmentAddDto?.DisputeReferenceNumber;
                adjustments.DisputeAmount = adjustmentTypeDto?.AdjustmentAddDto?.DisputeAmount;
                adjustments.DateDisputeFiled = adjustmentTypeDto?.AdjustmentAddDto?.DateDisputeFiled;
                adjustments.DescriptionOfDispute = adjustmentTypeDto?.AdjustmentAddDto?.DescriptionOfDispute;
                adjustments.AccountsPaymentDate = adjustmentTypeDto?.AdjustmentAddDto?.AccountsPaymentDate;
                adjustments.AccountsPaymentTransNo = adjustmentTypeDto?.AdjustmentAddDto?.AccountsPaymentTransNo;
                adjustments.AccountsPaymentAmount = adjustmentTypeDto?.AdjustmentAddDto?.AccountsPaymentAmount;
                adjustments.ReasonId = adjustmentTypeDto?.AdjustmentAddDto?.ReasonId;
                adjustments.Descriptions = adjustmentTypeDto?.AdjustmentAddDto?.Descriptions;
                await _dbContext.SaveChangesAsync();
                result = true;
            }

            return result;
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

        public async Task<bool> UpdateJO(AnalyticsProoflistDto adjustmentTypeDto)
        {
            var result = false;
            var oldJO = "";
            try
            {
                if (adjustmentTypeDto != null)
                {
                    var matchRow = await _dbContext.Analytics
                       .Where(x => x.Id == adjustmentTypeDto.AnalyticsId)
                       .FirstOrDefaultAsync();


                    if (matchRow != null)
                    {
                        oldJO = matchRow.OrderNo;
                        matchRow.OrderNo = adjustmentTypeDto?.AdjustmentAddDto?.NewJO;
                        await _dbContext.SaveChangesAsync();
                        result = true;
                    }

                    var adjustmentStatus = await _dbContext.AnalyticsProoflist
                     .Where(x => x.Id == adjustmentTypeDto.Id)
                     .FirstOrDefaultAsync();

                    if (adjustmentStatus != null)
                    {
                        adjustmentStatus.ActionId = adjustmentTypeDto.ActionId ?? 0;
                        await _dbContext.SaveChangesAsync();
                    }

                    var adjustments = await _dbContext.Adjustments
                              .Where(x => x.Id == adjustmentTypeDto.AdjustmentId)
                              .FirstOrDefaultAsync();

                    if (adjustments != null)
                    {
                        adjustments.OldJO = oldJO;
                        adjustments.NewJO = adjustmentTypeDto?.AdjustmentAddDto?.NewJO;
                        await _dbContext.SaveChangesAsync();
                        result = true;
                    }

                    if (adjustmentTypeDto.refreshAnalyticsDto != null)
                    {
                        var MatchDto = await GetMatchAnalyticsAndProofList(adjustmentTypeDto.refreshAnalyticsDto);

                        var GetMatch = MatchDto
                            .Where(x => x.AnalyticsId != null)
                            .ToList();

                        var CheckIsNull = GetMatch.Where(x => x.ProofListId == null && x.AnalyticsOrderNo.Contains(adjustmentTypeDto?.AdjustmentAddDto?.NewJO)).Any();

                        adjustmentStatus = await _dbContext.AnalyticsProoflist
                            .Where(x => x.Id == adjustmentTypeDto.Id)
                            .FirstOrDefaultAsync();

                        if (adjustmentStatus != null)
                        {
                            adjustmentStatus.StatusId = CheckIsNull ? 5 : adjustmentTypeDto.StatusId ?? 5;
                            await _dbContext.SaveChangesAsync();
                        }

                        var GetNewException = MatchDto
                            .Where(x => x.AnalyticsId != null && x.ProofListId != null && x.Variance > 0  && x.AnalyticsOrderNo.Contains(adjustmentTypeDto?.AdjustmentAddDto?.NewJO))
                            .ToList();

                        if (GetNewException != null)
                        {
                            foreach (var item in GetNewException)
                            {
                                var param = new AnalyticsProoflistDto
                                {

                                    Id = 0,
                                    AnalyticsId = item.AnalyticsId,
                                    ProoflistId = item.ProofListId,
                                    ActionId = null,
                                    StatusId = 5,
                                    AdjustmentId = 0,
                                    SourceId = (item.AnalyticsId != null && item.ProofListId != null ? 1 : item.AnalyticsId != null ? 1 : item.ProofListId != null ? 2 : 0),
                                    DeleteFlag = false,
                                    AdjustmentAddDto = new AdjustmentAddDto
                                    {
                                        Id = 0,
                                        DisputeReferenceNumber = null,
                                        DisputeAmount = null,
                                        DateDisputeFiled = null,
                                        DescriptionOfDispute = null,
                                        NewJO = null,
                                        CustomerId = null,
                                        AccountsPaymentDate = null,
                                        AccountsPaymentTransNo = null,
                                        AccountsPaymentAmount = null,
                                        ReasonId = null,
                                        Descriptions = null,
                                        DeleteFlag = null,
                                    }
                                };

                                await CreateAnalyticsProofList(param);
                            }
                        }
                    }
                }
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<MatchDto>> GetMatchAnalyticsAndProofList(RefreshAnalyticsDto analyticsParamsDto)
        {
            try
            {
                List<string> memCodeLast6Digits = analyticsParamsDto.memCode.Select(code => code.Substring(Math.Max(0, code.Length - 6))).ToList();
                var result = await _dbContext.Match
                     .FromSqlRaw($"WITH RankedData AS ( " +
                                $"SELECT  " +
                                $"     MAX(a.Id) AS Id, " +
                                $"     MAX(a.LocationName) AS LocationName, " +
                                $"     MAX(a.CustomerName) AS CustomerName, " +
                                $"     MAX(a.TransactionDate) AS TransactionDate, " +
                                $"     a.OrderNo, " +
                                $"     MAX(CAST(a.IsUpload AS INT)) AS IsUpload, " +
                                $"     ABS(a.SubTotal) AS SubTotal  " +
                                $" FROM ( " +
                                $"     SELECT   " +
                                $"        n.[Id], " +
                                $"        n.LocationId, " +
                                $"        n.CustomerId, " +
                                $"        c.CustomerName, " +
                                $"        l.LocationName, " +
                                $"        n.[TransactionDate], " +
                                $"        n.[OrderNo], " +
                                $"       n.[SubTotal], " +
                                $"        n.[IsUpload],   " +
                                $"        ROW_NUMBER() OVER (PARTITION BY n.OrderNo, n.SubTotal ORDER BY n.SubTotal DESC) AS row_num " +
                                $"     FROM tbl_analytics n " +
                                $"        LEFT JOIN [dbo].[tbl_location] l ON l.LocationCode = n.LocationId " +
                                $"        LEFT JOIN [dbo].[tbl_customer] c ON c.CustomerCode = n.CustomerId " +
                                $" ) a " +
                                $" WHERE  " +
                                $"      (CAST(a.TransactionDate AS DATE) = '{analyticsParamsDto.dates[0].ToString()}' AND a.LocationId = {analyticsParamsDto.storeId[0]} AND a.CustomerId LIKE '%{memCodeLast6Digits[0]}%') " +
                                $" GROUP BY  " +
                                $"     a.OrderNo,    " +
                                $"     ABS(a.SubTotal),  " +
                                $"     a.row_num " +
                                $" HAVING " +
                                $"     COUNT(a.OrderNo) = 1 " +
                                $"), " +
                                $"FilteredData AS ( " +
                                $"SELECT " +
                                $"    Id, " +
                                $"    CustomerName, " +
                                $"    LocationName, " +
                                $"    [TransactionDate], " +
                                $"    [OrderNo], " +
                                $"    [SubTotal], " +
                                $"    [IsUpload] " +
                                $"FROM RankedData " +
                                $") " +
                                $"SELECT " +
                                $"a.[Id] AS [AnalyticsId], " +
                                $"a.CustomerName AS [AnalyticsPartner], " +
                                $"a.LocationName AS [AnalyticsLocation], " +
                                $"a.[TransactionDate] AS [AnalyticsTransactionDate], " +
                                $"a.[OrderNo] AS [AnalyticsOrderNo], " +
                                $"a.[SubTotal] AS [AnalyticsAmount], " +
                                $"p.[Id] AS [ProofListId], " +
                                $"p.[TransactionDate] AS [ProofListTransactionDate], " +
                                $"p.[OrderNo] AS [ProofListOrderNo], " +
                                $"p.[Amount] AS [ProofListAmount],  " +
                                $"a.[IsUpload] AS [IsUpload] " +
                            $"FROM  " +
                                $"FilteredData a  " +
                            $"FULL OUTER JOIN  " +
                                $"(  " +
                                    $"SELECT  " +
                                        $"p.[Id], " +
                                        $"c.CustomerName, " +
                                        $"l.LocationName,  " +
                                        $"p.[TransactionDate],  " +
                                        $"p.[OrderNo], " +
                                        $"p.[Amount]  " +
                                   $" FROM " +
                                   $"     [dbo].[tbl_prooflist] p  " +
                                   $"     LEFT JOIN [dbo].[tbl_location] l ON l.LocationCode = p.StoreId " +
                                   $"     LEFT JOIN [dbo].[tbl_customer] c ON c.CustomerCode = p.CustomerId  " +
                                   $" WHERE " +
                                   $"     (CAST(p.TransactionDate AS DATE) = '{analyticsParamsDto.dates[0].ToString()}' AND p.StoreId = {analyticsParamsDto.storeId[0]} AND p.CustomerId LIKE '%{memCodeLast6Digits[0]}%' AND p.Amount IS NOT NULL AND p.Amount <> 0 AND p.StatusId != 4)  " +
                                $") p " +
                            $"ON a.[OrderNo] = p.[OrderNo]" +
                            $"ORDER BY COALESCE(p.Id, a.Id) DESC; ")
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
                    Variance = (m.AnalyticsAmount == null) ? m.ProofListAmount : (m.ProofListAmount == null) ? m.AnalyticsAmount : m.AnalyticsAmount - m.ProofListAmount.Value,
                    IsUpload = Convert.ToBoolean(m.IsUpload),
                }).ToList();

                var updateMatchDto = matchDtos
                    .Where(x => x.ProofListId == null || x.AnalyticsId == null || x.Variance <= -2 || x.Variance >= 2)
                    .ToList();

                return updateMatchDto;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> UpdatePartner(AnalyticsProoflistDto adjustmentTypeDto)
        {
            var result = false;
            var oldCustomerId = "";
            try
            {
                if (adjustmentTypeDto != null)
                {
                    var matchRow = await _dbContext.Analytics
                       .Where(x => x.Id == adjustmentTypeDto.AnalyticsId)
                       .FirstOrDefaultAsync();

                    if (matchRow != null)
                    {
                        oldCustomerId = matchRow.CustomerId;
                        matchRow.CustomerId = adjustmentTypeDto?.AdjustmentAddDto?.CustomerId;
                        await _dbContext.SaveChangesAsync();
                        result = true;
                    }

                    var adjustmentStatus = await _dbContext.AnalyticsProoflist
                    .Where(x => x.Id == adjustmentTypeDto.Id)
                    .FirstOrDefaultAsync();

                    if (adjustmentStatus != null)
                    {
                        adjustmentStatus.ActionId = adjustmentTypeDto.ActionId ?? 0;
                        await _dbContext.SaveChangesAsync();
                    }

                    var adjustments = await _dbContext.Adjustments
                              .Where(x => x.Id == adjustmentTypeDto.AdjustmentId)
                              .FirstOrDefaultAsync();

                    if (adjustments != null)
                    {
                        adjustments.CustomerIdOld = oldCustomerId;
                        adjustments.CustomerId = adjustmentTypeDto?.AdjustmentAddDto?.CustomerId;
                        await _dbContext.SaveChangesAsync();
                        result = true;
                    }

                    if (adjustmentTypeDto.refreshAnalyticsDto != null)
                    {
                        var analyticsParams = new RefreshAnalyticsDto
                        {
                            dates = adjustmentTypeDto.refreshAnalyticsDto.dates.Select(date => date).ToList(),
                            memCode = new List<string> { adjustmentTypeDto?.AdjustmentAddDto?.CustomerId },
                            userId = adjustmentTypeDto.refreshAnalyticsDto.userId,
                            storeId = adjustmentTypeDto.refreshAnalyticsDto.storeId
                        };

                        var MatchDto = await GetMatchAnalyticsAndProofList(analyticsParams);

                        var GetMatch = MatchDto
                            .Where(x => x.AnalyticsId != null)
                            .ToList();

                        var CheckIsNull = GetMatch.Where(x => x.ProofListId == null && x.AnalyticsOrderNo.Contains(adjustmentTypeDto?.AdjustmentAddDto?.NewJO)).Any();

                        adjustmentStatus = await _dbContext.AnalyticsProoflist
                            .Where(x => x.Id == adjustmentTypeDto.Id)
                            .FirstOrDefaultAsync();

                        if (adjustmentStatus != null)
                        {
                            adjustmentStatus.StatusId = CheckIsNull ? 5 : adjustmentTypeDto.StatusId ?? 5;
                            await _dbContext.SaveChangesAsync();
                        }
                    }
                }
                return result;
            }
            catch (Exception)
            {
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
                     Amount = grouped.Sum(j => j.a.SubTotal)
                 })
                 .FirstOrDefaultAsync();

            return result ?? new TransactionDtos();
        }

        public async Task<List<ExceptionDto>> ExportExceptions(AdjustmentParams adjustmentParams)
        {
            var result = new List<ExceptionDto>();
            DateTime date;
            IQueryable<ExceptionDto> query = Enumerable.Empty<ExceptionDto>().AsQueryable();
            if (DateTime.TryParse(adjustmentParams.dates[0], out date))
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
                    .GroupJoin(_dbContext.Source, x => x.ap.StatusId, so => so.Id, (x, so) => new { x.ap, x.a, x.Prooflist, x.Customer, x.Adjustment, x.Action, x.Status, Source = so })
                    .SelectMany(x => x.Source.DefaultIfEmpty(), (x, so) => new { x.ap, x.a, x.Prooflist, x.Customer, x.Adjustment, x.Action, x.Status, Source = so })
                    .Join(_dbContext.Locations, x => x.a.LocationId, l => l.LocationCode, (x, l) => new { x, l })
                    .Where(x => x.x.a.TransactionDate == date && x.x.a.LocationId == adjustmentParams.storeId[0] && x.x.a.CustomerId == adjustmentParams.memCode[0])
                    .Select(x => new ExceptionDto
                    {
                        Id = x.x.ap.Id,
                        CustomerId = x.x.Customer.CustomerName,
                        JoNumber = x.x.a.OrderNo,
                        TransactionDate = x.x.a.TransactionDate,
                        Amount = x.x.a.SubTotal,
                        AdjustmentType = x.x.Action.Action,
                        Source = x.x.Source.SourceType,
                        Status = x.x.Status.StatusName,
                        AdjustmentId = x.x.ap.AdjustmentId,
                        LocationName = x.l.LocationName,
                    })
                    .OrderBy(x => x.Id);

                result = await query.ToListAsync();
            }
            return result;
        }
    }
}

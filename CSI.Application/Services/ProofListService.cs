using CSI.Application.Interfaces;
using CSI.Domain.Entities;
using Microsoft.VisualBasic.FileIO;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using CSI.Infrastructure.Data;
using System.Security.Cryptography.X509Certificates;
using CSI.Application.DTOs;

namespace CSI.Application.Services
{
    public class ProofListService : IProofListService
    {
        private readonly AppDBContext _dbContext;

        public ProofListService(AppDBContext dBContext)
        {
            _dbContext = dBContext;
            _dbContext.Database.SetCommandTimeout(999);

        }

        public (List<Prooflist>?, string?) ReadProofList(IFormFile file)
        {
            int row = 2;
            int rowCount = 0;
            try
            {
                var patientList = new List<Prooflist>();
                using (var stream = file.OpenReadStream())
                {
                    if (file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        using (var package = new ExcelPackage(stream))
                        {
                            if (package.Workbook.Worksheets.Count > 0)
                            {
                                var worksheet = package.Workbook.Worksheets[0]; // Assuming data is in the first worksheet

                                rowCount = worksheet.Dimension.Rows;

                                // Check if the filename contains the word "grabfood"
                                if (file.FileName.Contains("grab", StringComparison.OrdinalIgnoreCase))
                                {
                                    var grabProofList = ExtractGrab(worksheet, rowCount, row);

                                    if (!DataExistsInDatabase(grabProofList.Item1))
                                    {
                                        _dbContext.Prooflist.AddRange(grabProofList.Item1);
                                        _dbContext.SaveChanges();
                                        return (grabProofList);
                                    }
                                    else
                                    {
                                        return (null, "Proof list already uploaded!");
                                    } 
                                }

                                return (null, "No worksheets found in the workbook.");
                            }
                            else
                            {
                                return (null, "No worksheets found in the workbook.");
                            }
                        }
                    }
                    else
                    {
                        return (null, "Unsupported file format. Only XLSX and CSV formats are supported.");
                    }
                } 
            }
            catch (Exception ex)
            {
                return (null, $"Please check error in row {row}: {ex.Message}");
                throw;
            }
        }

        private bool DataExistsInDatabase(List<Prooflist> grabProofList)
        {
            // Check if any item in grabProofList exists in the database
            var anyDataExists = grabProofList.Any(item =>
                _dbContext.Prooflist.Any(x =>
                    x.CustomerId == item.CustomerId &&
                    x.TransactionDate == item.TransactionDate &&
                    x.OrderNo == item.OrderNo
                )
            );

            return anyDataExists;
        }

        public DateTime? GetDateTime(object cellValue)
        {
            if (cellValue != null)
            {
                if (DateTime.TryParse(cellValue.ToString(), out var transactionDate))
                {
                    return transactionDate;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private (List<Prooflist>, string?) ExtractGrab(ExcelWorksheet worksheet, int rowCount, int row)
        {
            var getLocation = _dbContext.Locations.ToList();
            var grabFoodProofList = new List<Prooflist>();
            try
            {
                for (row = 2; row <= rowCount; row++)
                {
                    if (worksheet.Cells[row, 6].Value != null ||
                       worksheet.Cells[row, 16].Value != null ||
                       worksheet.Cells[row, 40].Value != null ||
                       worksheet.Cells[row, 10].Value != null ||
                       worksheet.Cells[row, 3].Value != null)
                    {
                        var transactionDate = GetDateTime(worksheet.Cells[row, 6].Value);
                        var patient = new Prooflist
                        {
                            CustomerId = "9999011955",
                            TransactionDate = transactionDate,
                            OrderNo = worksheet.Cells[row, 16].Value?.ToString(),
                            NonMembershipFee = (decimal?)0.00,
                            PurchasedAmount = (decimal?)0.00,
                            Amount = worksheet.Cells[row, 40].Value != null ? decimal.Parse(worksheet.Cells[row, 40].Value?.ToString()) : null,
                            StatusId = worksheet.Cells[row, 10].Value?.ToString() == "Completed" || worksheet.Cells[row, 10].Value?.ToString() == "Delivered" ? 3 : worksheet.Cells[row, 10].Value?.ToString() == "Cancelled" ? 4 : null,
                            StoreId = GetLocationId(worksheet.Cells[row, 3].Value?.ToString(), getLocation),
                            DeleteFlag = false,
                        };
                        grabFoodProofList.Add(patient);
                    }
                }

                return (grabFoodProofList, rowCount.ToString() + " rows extracted");
            }
            catch (Exception)
            {
                return (grabFoodProofList, "Error extracting proof list.");
            }
        }

        public int? GetLocationId(string? location, List<Location> locations)
        {
            if (location != null || location != string.Empty)
            {
                string locationName = location.Replace("S&R", "KAREILA");

                var getLocationCode = locations.Where(x => x.LocationName.ToLower().Contains(locationName.ToLower()))
                    .Select(n => n.LocationCode)
                    .FirstOrDefault();
                return getLocationCode;
            }
            else
            { 
                return null;
            }
        }

        public async Task<List<PortalDto>> GetPortal(PortalParamsDto portalParamsDto)
        {
            var result = await _dbContext.Prooflist
                .Where(x => x.TransactionDate.HasValue
                            && x.TransactionDate.Value.Date == portalParamsDto.dates[0].Date
                            && x.StoreId == portalParamsDto.storeId[0]
                            && x.CustomerId == portalParamsDto.memCode[0]
                            && x.StatusId != 4)
                .Join(_dbContext.Locations, a => a.StoreId, b => b.LocationCode, (a, b) => new { a, b })
                .Join(_dbContext.Status, c => c.a.StatusId, d => d.Id, (c, d) => new { c, d })
                .Select(n => new PortalDto
                {
                    Id = n.c.a.Id,
                    CustomerId = n.c.a.CustomerId,
                    TransactionDate = n.c.a.TransactionDate,
                    OrderNo = n.c.a.OrderNo,
                    NonMembershipFee = n.c.a.NonMembershipFee,
                    PurchasedAmount = n.c.a.PurchasedAmount,
                    Amount = n.c.a.Amount,
                    Status = n.d.StatusName,
                    StoreName = n.c.b.LocationName,
                    DeleteFlag = n.c.a.DeleteFlag
                })
                .ToListAsync();

            return result;
        }


        //private (List<Prooflist>, string?) ReadMetromart(string filePath)
        //{
        //    int row = 2;
        //    int rowCount = 0;
        //    var metroMartProofList = new List<Prooflist>();

        //    try
        //    {
        //        using (var parser = new TextFieldParser(filePath))
        //        {
        //            parser.TextFieldType = FieldType.Delimited;
        //            parser.SetDelimiters(",");

        //            if (!parser.EndOfData)
        //            {
        //                parser.ReadLine(); 
        //            }

        //            while (!parser.EndOfData)
        //            {
        //                string[] fields = parser.ReadFields();

        //                var transactionDate = DateTime.Parse(fields[3]);
        //                var amount = decimal.Parse(fields[5]) + decimal.Parse(fields[6]);
        //                var patient = new Prooflist
        //                {
        //                    CustomerId = "9999011855",
        //                    TransactionDate = transactionDate,
        //                    OrderNo = fields[1],
        //                    OrderAmount = amount, 
        //                    StatusId = fields[2] == "Completed" ? 3 : fields[2] == "Cancelled" ? 4 : null,
        //                    StoreName = null,
        //                    DeleteFlag = false,
        //                };

        //                metroMartProofList.Add(patient);
        //                rowCount++;
        //            }
        //        }

        //        return (metroMartProofList, rowCount.ToString() + " rows extracted");
        //    }
        //    catch (Exception ex)
        //    {
        //        return (null, $"Please check error in row {row}: {ex.Message}");
        //        throw;
        //    }
        //}

        //private (List<Prooflist>, string?) ReadPickARoo(string filePath)
        //{
        //    int row = 2;
        //    int rowCount = 0;
        //    var pickARooProofList = new List<Prooflist>();

        //    try
        //    {
        //        using (var parser = new TextFieldParser(filePath))
        //        {
        //            parser.TextFieldType = FieldType.Delimited;
        //            parser.SetDelimiters(",");

        //            // Assuming the first row contains headers, and you want to skip it
        //            if (!parser.EndOfData)
        //            {
        //                parser.ReadLine(); // Skip header row
        //            }

        //            while (!parser.EndOfData)
        //            {
        //                string[] fields = parser.ReadFields();

        //                var transactionDate = DateTime.Parse(fields[0]); // Assuming the date is in the first column
        //                var patient = new Prooflist
        //                {
        //                    CustomerId = "9999011931",
        //                    TransactionDate = transactionDate,
        //                    OrderNo = fields[1],
        //                    OrderAmount = decimal.Parse(fields[3]), // Assuming the order amount is in the fourth column
        //                    StatusId = fields[2] == "Completed" ? 3 : fields[2] == "Cancelled" ? 4 : null,
        //                    StoreName = null,
        //                    DeleteFlag = false,
        //                };

        //                pickARooProofList.Add(patient);
        //                rowCount++;
        //            }
        //        }

        //        return (pickARooProofList, rowCount.ToString() + " rows extracted");
        //    }
        //    catch (Exception ex)
        //    {
        //        return (null, $"Please check error in row {row}: {ex.Message}");
        //    }
        //}

        //private (List<Prooflist>, string?) ReadFoodPanda(ExcelWorksheet worksheet, int rowCount, int row)
        //{
        //    var grabFoodProofList = new List<Prooflist>();
        //    for (row = 3; row <= rowCount; row++)
        //    {
        //        DateTime.TryParse(worksheet.Cells[row, 15].Value?.ToString(), out var transactionDate);
        //        var patient = new Prooflist
        //        {
        //            CustomerId = "9999011838",
        //            TransactionDate = transactionDate,
        //            OrderNo = worksheet.Cells[row, 2].Value.ToString(),
        //            OrderAmount = decimal.Parse(worksheet.Cells[row, 19].Value.ToString()),
        //            StatusId = worksheet.Cells[row, 7].Value.ToString() == "Completed" || worksheet.Cells[row, 7].Value.ToString() == "Delivered" ? 3 : worksheet.Cells[row, 7].Value.ToString() == "Cancelled" ? 4 : null,
        //            StoreName = worksheet.Cells[row, 1].Value.ToString(),
        //            DeleteFlag = false,
        //        };

        //        grabFoodProofList.Add(patient);
        //    }

        //    return (grabFoodProofList, rowCount.ToString() + " rows extracted");
        //}
    }
}

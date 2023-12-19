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
using AutoMapper.Configuration.Annotations;

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

        public (List<Prooflist>?, string?) ReadProofList(IFormFile file, string customerName, string strClub, string selectedDate)
        {
            int row = 2;
            int rowCount = 0;
            var club = Convert.ToInt32(strClub);
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
                                if (customerName == "GrabMart" || customerName == "GrabFood")
                                {
                                    var grabProofList = ExtractGrabMartOrFood(worksheet, rowCount, row, customerName, club, selectedDate);
                                    if (grabProofList.Item1 == null)
                                    {
                                        return (null, grabProofList.Item2);
                                    }
                                    else if (!DataExistsInDatabase(grabProofList.Item1))
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
                                else if (customerName == "PickARoo")
                                {
                                    var pickARooProofList = ExtractPickARoo(worksheet, rowCount, row, customerName, club, selectedDate);
                                    if (pickARooProofList.Item1 == null)
                                    {
                                        return (null, pickARooProofList.Item2);
                                    }
                                    else if (!DataExistsInDatabase(pickARooProofList.Item1))
                                    {
                                        _dbContext.Prooflist.AddRange(pickARooProofList.Item1);
                                        _dbContext.SaveChanges();
                                        return (pickARooProofList);
                                    }
                                    else
                                    {
                                        return (null, "Proof list already uploaded!");
                                    }
                                }
                                else if (customerName == "FoodPanda")
                                {
                                    var foodPandaProofList = ExtractFoodPanda(worksheet, rowCount, row, customerName, club, selectedDate);
                                    if (foodPandaProofList.Item1 == null)
                                    {
                                        return (null, foodPandaProofList.Item2);
                                    }
                                    else if (!DataExistsInDatabase(foodPandaProofList.Item1))
                                    {
                                        _dbContext.Prooflist.AddRange(foodPandaProofList.Item1);
                                        _dbContext.SaveChanges();
                                        return (foodPandaProofList);
                                    }
                                    else
                                    {
                                        return (null, "Proof list already uploaded!");
                                    }
                                }
                                else if (customerName == "MetroMart")
                                {
                                    var metroMartProofList = ExtractMetroMart(worksheet, rowCount, row, customerName, club, selectedDate);
                                    if (metroMartProofList.Item1 == null)
                                    {
                                        return (null, metroMartProofList.Item2);
                                    }
                                    else if (!DataExistsInDatabase(metroMartProofList.Item1))
                                    {
                                        _dbContext.Prooflist.AddRange(metroMartProofList.Item1);
                                        _dbContext.SaveChanges();
                                        return (metroMartProofList);
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
                    else if(file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                    {
                        var tempCsvFilePath = Path.GetTempFileName() + ".csv";

                        using (var fileStream = new FileStream(tempCsvFilePath, FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }

                        if (customerName == "GrabMart" || customerName == "GrabFood")
                        {
                            var grabProofList = ExtractCSVGrabMartOrFood(tempCsvFilePath, customerName, club, selectedDate);
                            if (grabProofList.Item1 == null)
                            {
                                return (null, grabProofList.Item2);
                            }
                            else if (!DataExistsInDatabase(grabProofList.Item1))
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
                        else if (customerName == "PickARoo")
                        {
                            var pickARooProofList = ExtractCSVPickARoo(tempCsvFilePath, club, selectedDate);
                            if (pickARooProofList.Item1 == null)
                            {
                                return (null, pickARooProofList.Item2);
                            }
                            else if (!DataExistsInDatabase(pickARooProofList.Item1))
                            {
                                _dbContext.Prooflist.AddRange(pickARooProofList.Item1);
                                _dbContext.SaveChanges();
                                return (pickARooProofList);
                            }
                            else
                            {
                                return (null, "Proof list already uploaded!");
                            }
                        }
                        else if (customerName == "FoodPanda")
                        {
                            var foodPandaProofList = ExtractCSVFoodPanda(tempCsvFilePath, club, selectedDate);
                            if (foodPandaProofList.Item1 == null)
                            {
                                return (null, foodPandaProofList.Item2);
                            }
                            else if (!DataExistsInDatabase(foodPandaProofList.Item1))
                            {
                                _dbContext.Prooflist.AddRange(foodPandaProofList.Item1);
                                _dbContext.SaveChanges();
                                return (foodPandaProofList);
                            }
                            else
                            {
                                return (null, "Proof list already uploaded!");
                            }
                        }
                        else if (customerName == "MetroMart")
                        {
                            var metroMartProofList = ExtractCSVMetroMart(tempCsvFilePath, club, selectedDate);
                            if (metroMartProofList.Item1 == null)
                            {
                                return (null, metroMartProofList.Item2);
                            }
                            else if (!DataExistsInDatabase(metroMartProofList.Item1))
                            {
                                _dbContext.Prooflist.AddRange(metroMartProofList.Item1);
                                _dbContext.SaveChanges();
                                return (metroMartProofList);
                            }
                            else
                            {
                                return (null, "Proof list already uploaded!");
                            }
                        }

                        return (null, "No worksheets.");
                    }
                    else
                    {
                        return (null, "No worksheets.");
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
                    return transactionDate.Date;
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

        private (List<Prooflist>, string?) ExtractGrabMartOrFood(ExcelWorksheet worksheet, int rowCount, int row, string customerName, int club, string selectedDate)
        {
            var getLocation = _dbContext.Locations.ToList();
            var grabFoodProofList = new List<Prooflist>();

            // Define expected headers
            string[] expectedHeaders = { "store name", "created on", "type", "status", "short order id", "net sales" };

            Dictionary<string, int> columnIndexes = new Dictionary<string, int>();

            try
            {
                // Find column indexes based on header names
                for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                {
                    var header = worksheet.Cells[1, col].Text.ToLower().Trim();
                    if (!string.IsNullOrEmpty(header))
                    {
                        columnIndexes[header] = col;
                    }
                }

                // Check if all expected headers exist in the first row
                foreach (var expectedHeader in expectedHeaders)
                {
                    if (!columnIndexes.ContainsKey(expectedHeader))
                    {
                        return (grabFoodProofList, $"Column not found.");
                    }
                }

                for (row = 2; row <= rowCount; row++)
                {
                    if (worksheet.Cells[row, columnIndexes["net sales"]].Value != null ||
                        worksheet.Cells[row, columnIndexes["short order id"]].Value != null ||
                        worksheet.Cells[row, columnIndexes["status"]].Value != null ||
                        worksheet.Cells[row, columnIndexes["created on"]].Value != null ||
                        worksheet.Cells[row, columnIndexes["store name"]].Value != null)
                    {
                       
                        var transactionDate = GetDateTime(worksheet.Cells[row, columnIndexes["created on"]].Value);

                        var chktransactionDate = new DateTime();
                        if (transactionDate.HasValue)
                        {
                            chktransactionDate = transactionDate.Value.Date;
                        }
                        var cnvrtDate = GetDateTime(selectedDate);
                        if (cnvrtDate == chktransactionDate)
                        {
                            var prooflist = new Prooflist
                            {
                                CustomerId = customerName == "GrabMart" ? "9999011955" : "9999011929",
                                TransactionDate = transactionDate,
                                OrderNo = worksheet.Cells[row, columnIndexes["short order id"]].Value?.ToString(),
                                NonMembershipFee = (decimal?)0.00,
                                PurchasedAmount = (decimal?)0.00,
                                Amount = worksheet.Cells[row, columnIndexes["net sales"]].Value != null ? decimal.Parse(worksheet.Cells[row, columnIndexes["net sales"]].Value?.ToString()) : null,
                                StatusId = worksheet.Cells[row, columnIndexes["status"]].Value?.ToString() == "Completed" || worksheet.Cells[row, columnIndexes["status"]].Value?.ToString() == "Delivered" || worksheet.Cells[row, columnIndexes["status"]].Value?.ToString() == "Transferred" ? 3 : worksheet.Cells[row, columnIndexes["status"]].Value?.ToString() == "Cancelled" ? 4 : null,
                                StoreId = club,
                                DeleteFlag = false,
                            };
                            grabFoodProofList.Add(prooflist);
                        }
                        else
                        {
                            return (grabFoodProofList, "Uploaded file transaction dates do not match.");
                        }
                    }
                }

                return (grabFoodProofList, rowCount.ToString() + " rows extracted");
            }
            catch (Exception)
            {
                return (grabFoodProofList, "Error extracting proof list.");
            }
        }

        private (List<Prooflist>, string?) ExtractPickARoo(ExcelWorksheet worksheet, int rowCount, int row, string customerName, int club, string selectedDate)
        {
            var pickARooProofList = new List<Prooflist>();

            // Define expected headers
            string[] expectedHeaders = { "order date", "order number", "order status", "amount" };

            Dictionary<string, int> columnIndexes = new Dictionary<string, int>();

            try
            {
                // Find column indexes based on header names
                for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                {
                    var header = worksheet.Cells[1, col].Text.Trim();
                    if (!string.IsNullOrEmpty(header))
                    {
                        columnIndexes[header] = col;
                    }
                }

                // Check if all expected headers exist in the first row
                foreach (var expectedHeader in expectedHeaders)
                {
                    if (!columnIndexes.ContainsKey(expectedHeader))
                    {
                        return (pickARooProofList, $"Column not found.");
                    }
                }

                for (row = 2; row <= rowCount; row++)
                {
                    if (worksheet.Cells[row, columnIndexes["order date"]].Value != null ||
                        worksheet.Cells[row, columnIndexes["order number"]].Value != null ||
                        worksheet.Cells[row, columnIndexes["order status"]].Value != null ||
                        worksheet.Cells[row, columnIndexes["amount"]].Value != null)
                    {

                        var transactionDate = GetDateTime(worksheet.Cells[row, columnIndexes["order date"]].Value);

                        var chktransactionDate = new DateTime();
                        if (transactionDate.HasValue)
                        {
                            chktransactionDate = transactionDate.Value.Date;
                        }

                        var cnvrtDate = GetDateTime(selectedDate);
                        if (cnvrtDate == chktransactionDate)
                        {
                            var prooflist = new Prooflist
                            {
                                CustomerId = "9999011931",
                                TransactionDate = transactionDate,
                                OrderNo = worksheet.Cells[row, columnIndexes["order number"]].Value?.ToString(),
                                NonMembershipFee = (decimal?)0.00,
                                PurchasedAmount = (decimal?)0.00,
                                Amount = worksheet.Cells[row, columnIndexes["amount"]].Value != null ? decimal.Parse(worksheet.Cells[row, columnIndexes["amount"]].Value?.ToString()) : null,
                                StatusId = worksheet.Cells[row, columnIndexes["order status"]].Value?.ToString() == "Completed" || worksheet.Cells[row, columnIndexes["order status"]].Value?.ToString() == "Delivered" || worksheet.Cells[row, columnIndexes["order status"]].Value?.ToString() == "Transferred" ? 3 : worksheet.Cells[row, columnIndexes["order status"]].Value?.ToString() == "Cancelled" ? 4 : null,
                                StoreId = club,
                                DeleteFlag = false,
                            };
                            pickARooProofList.Add(prooflist);
                        }
                        else
                        {
                            return (pickARooProofList, "Uploaded file transaction dates do not match.");
                        }
                    }
                }

                return (pickARooProofList, rowCount.ToString() + " rows extracted");
            }
            catch (Exception)
            {
                return (pickARooProofList, "Error extracting proof list.");
            }
        }

        private (List<Prooflist>, string?) ExtractFoodPanda(ExcelWorksheet worksheet, int rowCount, int row, string customerName, int club, string selectedDate)
        {
            var foodPandaProofList = new List<Prooflist>();

            // Define expected headers
            string[] expectedHeaders = { "order id", "order status", "delivered at", "subtotal" };

            Dictionary<string, int> columnIndexes = new Dictionary<string, int>();

            try
            {
                // Find column indexes based on header names
                for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                {
                    var header = worksheet.Cells[1, col].Text.Trim();
                    if (!string.IsNullOrEmpty(header))
                    {
                        columnIndexes[header] = col;
                    }
                }

                // Check if all expected headers exist in the first row
                foreach (var expectedHeader in expectedHeaders)
                {
                    if (!columnIndexes.ContainsKey(expectedHeader))
                    {
                        return (foodPandaProofList, $"Column not found.");
                    }
                }

                for (row = 2; row <= rowCount; row++)
                {
                    if (worksheet.Cells[row, columnIndexes["order id"]].Value != null ||
                        worksheet.Cells[row, columnIndexes["order status"]].Value != null ||
                        worksheet.Cells[row, columnIndexes["delivered at"]].Value != null ||
                        worksheet.Cells[row, columnIndexes["subtotal"]].Value != null)
                    {

                        var transactionDate = GetDateTime(worksheet.Cells[row, columnIndexes["delivered at"]].Value);

                        var chktransactionDate = new DateTime();
                        if (transactionDate.HasValue)
                        {
                            chktransactionDate = transactionDate.Value.Date;
                        }
                        var cnvrtDate = GetDateTime(selectedDate);
                        if (cnvrtDate == chktransactionDate)
                        {
                            var prooflist = new Prooflist
                            {
                                CustomerId = "9999011838",
                                TransactionDate = transactionDate,
                                OrderNo = worksheet.Cells[row, columnIndexes["order id"]].Value?.ToString(),
                                NonMembershipFee = (decimal?)0.00,
                                PurchasedAmount = (decimal?)0.00,
                                Amount = worksheet.Cells[row, columnIndexes["subtotal"]].Value != null ? decimal.Parse(worksheet.Cells[row, columnIndexes["subtotal"]].Value?.ToString()) : null,
                                StatusId = worksheet.Cells[row, columnIndexes["order status"]].Value?.ToString() == "Completed" || worksheet.Cells[row, columnIndexes["order status"]].Value?.ToString() == "Delivered" || worksheet.Cells[row, columnIndexes["order status"]].Value?.ToString() == "Transferred" ? 3 : worksheet.Cells[row, columnIndexes["order status"]].Value?.ToString() == "Cancelled" ? 4 : null,
                                StoreId = club,
                                DeleteFlag = false,
                            };
                            foodPandaProofList.Add(prooflist);
                        }
                        else
                        {
                            return (foodPandaProofList, "Uploaded file transaction dates do not match.");
                        }
                    }
                }

                return (foodPandaProofList, rowCount.ToString() + " rows extracted");
            }
            catch (Exception)
            {
                return (foodPandaProofList, "Error extracting proof list.");
            }
        }

        private (List<Prooflist>, string?) ExtractMetroMart(ExcelWorksheet worksheet, int rowCount, int row, string customerName, int club, string selectedDate)
        {
            var metroMartProofList = new List<Prooflist>();

            // Define expected headers
            string[] expectedHeaders = { "jo #", "jo delivery status", "completed date", "non membership fee", "purchased amount" };

            Dictionary<string, int> columnIndexes = new Dictionary<string, int>();

            try
            {
                // Find column indexes based on header names
                for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                {
                    var header = worksheet.Cells[1, col].Text.Trim();
                    if (!string.IsNullOrEmpty(header))
                    {
                        columnIndexes[header] = col;
                    }
                }

                // Check if all expected headers exist in the first row
                foreach (var expectedHeader in expectedHeaders)
                {
                    if (!columnIndexes.ContainsKey(expectedHeader))
                    {
                        return (metroMartProofList, $"Column not found.");
                    }
                }

                for (row = 2; row <= rowCount; row++)
                {
                    if (worksheet.Cells[row, columnIndexes["jo #"]].Value != null ||
                        worksheet.Cells[row, columnIndexes["jo delivery status"]].Value != null ||
                        worksheet.Cells[row, columnIndexes["completed date"]].Value != null ||
                        worksheet.Cells[row, columnIndexes["non membership fee"]].Value != null ||
                        worksheet.Cells[row, columnIndexes["purchased amount"]].Value != null)
                    {

                        var transactionDate = GetDateTime(worksheet.Cells[row, columnIndexes["completed date"]].Value);
                        decimal NonMembershipFee = worksheet.Cells[row, columnIndexes["non membership fee"]].Value != null ? decimal.Parse(worksheet.Cells[row, columnIndexes["non membership fee"]].Value?.ToString()) : 0;
                        decimal PurchasedAmount = worksheet.Cells[row, columnIndexes["purchased amount"]].Value != null ? decimal.Parse(worksheet.Cells[row, columnIndexes["purchased amount"]].Value?.ToString()) : 0;
                        var chktransactionDate = new DateTime();
                        if (transactionDate.HasValue)
                        {
                            chktransactionDate = transactionDate.Value.Date;
                        }
                        var cnvrtDate = GetDateTime(selectedDate);
                        if (cnvrtDate == chktransactionDate)
                        {
                            var prooflist = new Prooflist
                            {
                                CustomerId = "9999011855",
                                TransactionDate = transactionDate,
                                OrderNo = worksheet.Cells[row, columnIndexes["jo #"]].Value?.ToString(),
                                NonMembershipFee = NonMembershipFee,
                                PurchasedAmount = PurchasedAmount,
                                Amount = NonMembershipFee + PurchasedAmount,
                                StatusId = worksheet.Cells[row, columnIndexes["jo delivery status"]].Value?.ToString() == "Completed" || worksheet.Cells[row, columnIndexes["jo delivery status"]].Value?.ToString() == "Delivered" || worksheet.Cells[row, columnIndexes["jo delivery status"]].Value?.ToString() == "Transferred" ? 3 : worksheet.Cells[row, columnIndexes["jo delivery status"]].Value?.ToString() == "Cancelled" ? 4 : null,
                                StoreId = club,
                                DeleteFlag = false,
                            };
                            metroMartProofList.Add(prooflist);
                        }
                        else
                        {
                            return (metroMartProofList, "Uploaded file transaction dates do not match.");
                        }
                    }
                }

                return (metroMartProofList, rowCount.ToString() + " rows extracted");
            }
            catch (Exception)
            {
                return (metroMartProofList, "Error extracting proof list.");
            }
        }

        private (List<Prooflist>, string?) ExtractCSVGrabMartOrFood(string filePath, string customerName, int club, string selectedDate)
        {
            int row = 2;
            int rowCount = 0;
            var grabFoodProofLists = new List<Prooflist>();

            try
            {
                using (var parser = new TextFieldParser(filePath))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    if (!parser.EndOfData)
                    {
                        parser.ReadLine();
                    }

                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();
                        var transactionDate = GetDateTime(fields[5]);
                        var chktransactionDate = new DateTime();
                        if (transactionDate.HasValue)
                        {
                            chktransactionDate = transactionDate.Value.Date;
                        }
                        var cnvrtDate = GetDateTime(selectedDate);
                        if (cnvrtDate == chktransactionDate)
                        {
                            var grabfood = new Prooflist
                            {
                                CustomerId = customerName == "GrabMart" ? "9999011955" : "9999011929",
                                TransactionDate = fields[5].ToString() != "" ? GetDateTime(fields[5]) : null,
                                OrderNo = fields[15],
                                NonMembershipFee = (decimal?)0.00,
                                PurchasedAmount = (decimal?)0.00,
                                Amount = fields[39] != "" ? decimal.Parse(fields[39]) : (decimal?)0.00,
                                StatusId = fields[9] == "Completed" || fields[9] == "Delivered" || fields[9] == "Transferred" ? 3 : fields[9] == "Cancelled" ? 4 : null,
                                StoreId = club,
                                DeleteFlag = false,
                            };

                            grabFoodProofLists.Add(grabfood);
                            rowCount++;
                        }
                        else
                        {
                            return (grabFoodProofLists, "Uploaded file transaction dates do not match.");
                        }
                    }
                }

                return (grabFoodProofLists, rowCount.ToString() + " rows extracted");
            }
            catch (Exception ex)
            {
                return (null, $"Please check error in row {rowCount}: {ex.Message}");
                throw;
            }
        }

        private (List<Prooflist>, string?) ExtractCSVMetroMart(string filePath, int club, string selectedDate)
        {
            int row = 2;
            int rowCount = 0;
            var metroMartProofLists = new List<Prooflist>();

            try
            {
                using (var parser = new TextFieldParser(filePath))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    if (!parser.EndOfData)
                    {
                        parser.ReadLine();
                    }

                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();
                        var chktransactionDate = new DateTime();
                        var transactionDate = GetDateTime(fields[3]);
                        var NonMembershipFee = fields[5] != "" ? decimal.Parse(fields[5]) : (decimal?)0.00;
                        var PurchasedAmount = fields[6] != "" ? decimal.Parse(fields[6]) : (decimal?)0.00;
                        var amount = NonMembershipFee + PurchasedAmount;
                        if (transactionDate.HasValue)
                        {
                            chktransactionDate = transactionDate.Value.Date;
                        }
                        var cnvrtDate = GetDateTime(selectedDate);
                        if (cnvrtDate == chktransactionDate)
                        {
                            var metroMart = new Prooflist
                            {
                                CustomerId = "9999011855",
                                TransactionDate = fields[3].ToString() != "" ? GetDateTime(fields[3]) : null,
                                OrderNo = fields[1],
                                NonMembershipFee = NonMembershipFee,
                                PurchasedAmount = PurchasedAmount,
                                Amount = amount,
                                StatusId = fields[2] == "Completed" || fields[2] == "Delivered" || fields[2] == "Transferred" ? 3 : fields[2] == "Cancelled" ? 4 : null,
                                StoreId = club,
                                DeleteFlag = false,
                            };

                            metroMartProofLists.Add(metroMart);
                            rowCount++;
                        }
                        else
                        {
                            return (metroMartProofLists, "Uploaded file transaction dates do not match.");
                        } 
                    }
                }

                return (metroMartProofLists, rowCount.ToString() + " rows extracted");
            }
            catch (Exception ex)
            {
                return (null, $"Please check error in row {row}: {ex.Message}");
                throw;
            }
        }

        private (List<Prooflist>, string?) ExtractCSVPickARoo(string filePath, int club, string selectedDate)
        {
            int row = 2;
            int rowCount = 0;
            var pickARooProofLists = new List<Prooflist>();

            try
            {
                using (var parser = new TextFieldParser(filePath))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    if (!parser.EndOfData)
                    {
                        parser.ReadLine();
                    }

                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();
                        var chktransactionDate = new DateTime();
                        var transactionDate = GetDateTime(fields[0]);
                        if (transactionDate.HasValue)
                        {
                            chktransactionDate = transactionDate.Value.Date;
                        }
                        var cnvrtDate = GetDateTime(selectedDate);
                        if (cnvrtDate == chktransactionDate)
                        {
                            var pickARoo = new Prooflist
                            {
                                CustomerId = "9999011931",
                                TransactionDate = transactionDate,
                                OrderNo = fields[1],
                                NonMembershipFee = (decimal?)0.00,
                                PurchasedAmount = (decimal?)0.00,
                                Amount = decimal.Parse(fields[3]),
                                StatusId = fields[2] == "Completed" || fields[2] == "Completed" ? 3 : fields[2] == "Cancelled" ? 4 : null,
                                StoreId = club,
                                DeleteFlag = false,
                            };

                            pickARooProofLists.Add(pickARoo);
                            rowCount++;
                        }
                        else
                        {
                            return (pickARooProofLists, "Uploaded file transaction dates do not match.");
                        }
                    }
                }

                return (pickARooProofLists, rowCount.ToString() + " rows extracted");
            }
            catch (Exception ex)
            {
                return (null, $"Please check error in row {row}: {ex.Message}");
                throw;
            }
        }

        private (List<Prooflist>, string?) ExtractCSVFoodPanda(string filePath, int club, string selectedDate)
        {
            int row = 2;
            int rowCount = 0;
            var foodPandaProofLists = new List<Prooflist>();

            try
            {
                using (var parser = new TextFieldParser(filePath))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    if (!parser.EndOfData)
                    {
                        parser.ReadLine();
                    }

                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();
                        var chktransactionDate = new DateTime();
                        var transactionDate = GetDateTime(fields[14]);
                        if (transactionDate.HasValue)
                        {
                            chktransactionDate = transactionDate.Value.Date;
                        }
                        var cnvrtDate = GetDateTime(selectedDate);
                        if (cnvrtDate == chktransactionDate)
                        {
                            var foodPanda = new Prooflist
                            {
                                CustomerId = "9999011838",
                                TransactionDate = transactionDate,
                                OrderNo = fields[1],
                                NonMembershipFee = (decimal?)0.00,
                                PurchasedAmount = (decimal?)0.00,
                                Amount = decimal.Parse(fields[18]),
                                StatusId = fields[6] == "Completed" || fields[6] == "Completed" ? 3 : fields[6] == "Cancelled" ? 4 : null,
                                StoreId = club,
                                DeleteFlag = false,
                            };

                            foodPandaProofLists.Add(foodPanda);
                            rowCount++;
                        }
                        else
                        {
                            return (foodPandaProofLists, "Uploaded file transaction dates do not match.");
                        }
                    }
                }

                return (foodPandaProofLists, rowCount.ToString() + " rows extracted");
            }
            catch (Exception ex)
            {
                return (null, $"Please check error in row {row}: {ex.Message}");
                throw;
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
            var date = GetDateTime(portalParamsDto.dates[0].Date);
            var result = await _dbContext.Prooflist
                .Join(_dbContext.Locations, a => a.StoreId, b => b.LocationCode, (a, b) => new { a, b })
                .Join(_dbContext.Status, c => c.a.StatusId, d => d.Id, (c, d) => new { c, d })
                .Where(x => x.c.a.TransactionDate.Value.Date == date
                    && x.c.a.StoreId == portalParamsDto.storeId[0]
                    && x.c.a.CustomerId == portalParamsDto.memCode[0]
                    && x.c.a.StatusId != 4)
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
    }
}

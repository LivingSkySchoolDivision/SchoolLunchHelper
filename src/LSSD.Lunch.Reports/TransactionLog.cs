using System;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using LSSD.Lunch.Extensions;
using LSSD.Lunch.Reports;

// For help with how to work with OpenXml documents:
// https://docs.microsoft.com/en-us/office/open-xml/how-do-i


namespace LSSD.Lunch.Reports
{
    class TransactionLog
    {
        string _timeZone = string.Empty;

        public void Generate(List<Transaction> AllTransactions, School School, String SelectedTimeZone, string Filename) 
        {       
            _timeZone = SelectedTimeZone;

            if (File.Exists(Filename)) {
                File.Delete(Filename);
            }
            
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(Filename, SpreadsheetDocumentType.Workbook))
            {


                WorkbookPart workbookpart = document.AddWorkbookPart();
                workbookpart.Workbook = new Workbook();

                SheetData sheetData = new SheetData();

                WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(sheetData);

                Sheets sheets = document.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

                Sheet sheetOne = new Sheet() {
                    Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Sheet 1"
                };
                sheets.Append(sheetOne);

                /////

                ExcelHelper.AddPageReportTitleCell(workbookpart, worksheetPart, "A", 1, "LSSD Transaction Log");
                ExcelHelper.AddPageReportTitleCell(workbookpart, worksheetPart, "A", 2, School.Name);
                ExcelHelper.AddPageReportTitleCell(workbookpart, worksheetPart, "A", 3, DateTime.UtcNow.AdjustForTimezone(_timeZone).ToShortDateString());
                ExcelHelper.AddPageReportTitleCell(workbookpart, worksheetPart, "A", 4, DateTime.UtcNow.AdjustForTimezone(_timeZone).ToLongTimeString());

                uint dataRowIndex = 6;

                // Headers
                addHeaderRow(workbookpart, worksheetPart, dataRowIndex);
                dataRowIndex++;

                // TODO: This is EXTREMELY slow.. may need to find another way to do this.
                foreach(Transaction transaction in AllTransactions)
                {
                    addTransactionRow(workbookpart, worksheetPart,dataRowIndex, transaction);
                    dataRowIndex++;
                }

                /////

                workbookpart.Workbook.Save();

                document.Close();

            }
        }

        private void addHeaderRow(WorkbookPart workbookpart, WorksheetPart worksheetPart, uint RowNum)
        {   
            ExcelHelper.AddHeadingCell(workbookpart, worksheetPart, "A", RowNum, "Date");
            ExcelHelper.AddHeadingCell(workbookpart, worksheetPart, "B", RowNum, "Time");
            ExcelHelper.AddHeadingCell(workbookpart, worksheetPart, "C", RowNum, "Student Name");
            ExcelHelper.AddHeadingCell(workbookpart, worksheetPart, "D", RowNum, "Student ID");
            ExcelHelper.AddHeadingCell(workbookpart, worksheetPart, "E", RowNum, "Item");
            ExcelHelper.AddHeadingCell(workbookpart, worksheetPart, "F", RowNum, "Amount");
        }
        
        private void addTransactionRow(WorkbookPart workbookpart, WorksheetPart worksheetPart, uint RowNum, Transaction transaction)
        {   
            ExcelHelper.AddDateCell(workbookpart, worksheetPart, "A", RowNum, transaction.TimestampUTC.AdjustForTimezone(_timeZone).ToShortDateString());
            ExcelHelper.AddTextCell(workbookpart, worksheetPart, "B", RowNum, transaction.TimestampUTC.AdjustForTimezone(_timeZone).ToLongTimeString());
            ExcelHelper.AddTextCell(workbookpart, worksheetPart, "C", RowNum, transaction.StudentName);
            ExcelHelper.AddTextCell(workbookpart, worksheetPart, "D", RowNum, transaction.StudentNumber);
            ExcelHelper.AddTextCell(workbookpart, worksheetPart, "E", RowNum, transaction.ItemDescription);
            ExcelHelper.AddCurrencyCell(workbookpart, worksheetPart, "F", RowNum, transaction.Amount);
        }

    }
}
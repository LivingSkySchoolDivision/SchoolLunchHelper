using System;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using LSSD.Lunch.Extensions;
using LSSD.Lunch.Reports;

// A fairly simple spreadsheet of students with balances
// Constrained by school
// Shows active students and inactive students (identified by a field)

// For help with how to work with OpenXml documents:
// https://docs.microsoft.com/en-us/office/open-xml/how-do-i


namespace LSSD.Lunch.Reports
{
    class StudentBalanceReport
    {
        string _timeZone = string.Empty;

        public void Generate(List<Student> Students, List<Transaction> AllTransactions, School School, string TimeZone, string Filename) 
        {            
            _timeZone = TimeZone;

            if (File.Exists(Filename)) {
                File.Delete(Filename);
            }

            // Organize transactions into dictionary for easier lookups
            Dictionary<Guid, List<Transaction>> transactionDictionary = getTransactionDictionary(AllTransactions);
            
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

                ExcelHelper.AddPageReportTitleCell(workbookpart, worksheetPart, "A", 1, "LSSD Student Balance Report");
                ExcelHelper.AddPageReportTitleCell(workbookpart, worksheetPart, "A", 2, School.Name);
                ExcelHelper.AddPageReportTitleCell(workbookpart, worksheetPart, "A", 3, DateTime.UtcNow.AdjustForTimezone(_timeZone).ToShortDateString());
                ExcelHelper.AddPageReportTitleCell(workbookpart, worksheetPart, "A", 4, DateTime.UtcNow.AdjustForTimezone(_timeZone).ToLongTimeString());

                uint dataRowIndex = 6;

                // Put a quick summary on top
                decimal totalMoneyIn = 0;
                decimal totalMoneyOut = 0;
                decimal totalBalance = 0;

                foreach(Transaction trans in AllTransactions)
                {
                    totalBalance += trans.Amount;

                    if (trans.Amount > (decimal)0.000) 
                    {
                        totalMoneyIn += trans.Amount;
                    }
                    
                    if (trans.Amount < (decimal)0.000) 
                    {
                        totalMoneyOut += trans.Amount;
                    }
                }

                ExcelHelper.AddTextCell(workbookpart, worksheetPart, "A", dataRowIndex, "Total money in");
                ExcelHelper.AddCurrencyCell(workbookpart, worksheetPart, "B", dataRowIndex, totalMoneyIn);
                dataRowIndex++;

                ExcelHelper.AddTextCell(workbookpart, worksheetPart, "A", dataRowIndex, "Total money out");
                ExcelHelper.AddCurrencyCell(workbookpart, worksheetPart, "B", dataRowIndex, (totalMoneyOut*-1));
                dataRowIndex++;

                ExcelHelper.AddTextCell(workbookpart, worksheetPart, "A", dataRowIndex, "Total balance");
                ExcelHelper.AddCurrencyCell(workbookpart, worksheetPart, "B", dataRowIndex, totalBalance);
                dataRowIndex++;

                dataRowIndex++;

                // Headers
                addHeaderRow(workbookpart, worksheetPart, dataRowIndex);
                dataRowIndex++;

                 // Row to start student list on
                foreach(Student student in Students.OrderByDescending(x => x.IsActive).ThenBy(x => x.LastName).ThenBy(x => x.FirstName))
                {
                    List<Transaction> thisStudentTransactions = transactionDictionary.ContainsKey(student.Id) ? transactionDictionary[student.Id] : new List<Transaction>();
                    addStudentRow(workbookpart, worksheetPart, dataRowIndex, student, thisStudentTransactions);
                    dataRowIndex++;
                }

                // space between last row and total row
                dataRowIndex++;

                addSummaryRow(workbookpart, worksheetPart, dataRowIndex, AllTransactions);
                dataRowIndex++;

                
                ExcelHelper.AddTextCell(workbookpart, worksheetPart, "A", dataRowIndex, "Total students");
                ExcelHelper.AddNumberCell(workbookpart, worksheetPart, "B", dataRowIndex, (Students.Count()));

                /////

                workbookpart.Workbook.Save();

                document.Close();

            }
        }

        private Dictionary<Guid, List<Transaction>> getTransactionDictionary(List<Transaction> Transactions)
        {
            Dictionary<Guid, List<Transaction>> returnMe = new Dictionary<Guid, List<Transaction>>();

            foreach(Transaction tran in Transactions)
            {
                if ((tran.StudentID != null) && tran.StudentID != new Guid())
                {
                    Guid nonNullGUID = tran.StudentID ?? new Guid();

                    if (!returnMe.ContainsKey(nonNullGUID))
                    {
                        returnMe.Add(nonNullGUID, new List<Transaction>());
                    }

                    returnMe[nonNullGUID].Add(tran);
                }
            }

            return returnMe;
        }

        private void addHeaderRow(WorkbookPart workbookpart, WorksheetPart worksheetPart, uint RowNum)
        {   
            ExcelHelper.AddHeadingCell(workbookpart, worksheetPart, "A", RowNum, "Last Name");
            ExcelHelper.AddHeadingCell(workbookpart, worksheetPart, "B", RowNum, "First Name");
            ExcelHelper.AddHeadingCell(workbookpart, worksheetPart, "C", RowNum, "Active?");
            ExcelHelper.AddHeadingCell(workbookpart, worksheetPart, "D", RowNum, "Homeroom");
            ExcelHelper.AddHeadingCell(workbookpart, worksheetPart, "E", RowNum, "Total Money In");
            ExcelHelper.AddHeadingCell(workbookpart, worksheetPart, "F", RowNum, "Total Money Out");
            ExcelHelper.AddHeadingCell(workbookpart, worksheetPart, "G", RowNum, "Current Balance");
            ExcelHelper.AddHeadingCell(workbookpart, worksheetPart, "H", RowNum, "Student Owes");
        }

        private void addSummaryRow(WorkbookPart workbookpart, WorksheetPart worksheetPart, uint RowNum, List<Transaction> Transactions)
        {            
            decimal totalMoneyIn = 0;
            decimal totalMoneyOut = 0;
            decimal totalBalance = 0;

            foreach(Transaction trans in Transactions)
            {
                totalBalance += trans.Amount;

                if (trans.Amount > (decimal)0.000) 
                {
                    totalMoneyIn += trans.Amount;
                }
                
                if (trans.Amount < (decimal)0.000) 
                {
                    totalMoneyOut += trans.Amount;
                }
            }

            ExcelHelper.AddTextCell(workbookpart, worksheetPart, "A", RowNum, "TOTALS");
            ExcelHelper.AddCurrencyCell(workbookpart, worksheetPart, "E", RowNum, totalMoneyIn);
            ExcelHelper.AddCurrencyCell(workbookpart, worksheetPart, "F", RowNum, (totalMoneyOut * -1));
            ExcelHelper.AddCurrencyCell(workbookpart, worksheetPart, "G", RowNum, totalBalance);
        }

        private void addStudentRow(WorkbookPart workbookpart, WorksheetPart worksheetPart, uint RowNum, Student Student, List<Transaction> Transactions)
        {            
            decimal totalMoneyIn = 0;
            decimal totalMoneyOut = 0;
            decimal totalBalance = 0;

            foreach(Transaction trans in Transactions)
            {
                totalBalance += trans.Amount;

                if (trans.Amount > (decimal)0.000) 
                {
                    totalMoneyIn += trans.Amount;
                }
                
                if (trans.Amount < (decimal)0.000) 
                {
                    totalMoneyOut += trans.Amount;
                }
            }


            ExcelHelper.AddTextCell(workbookpart, worksheetPart, "A", RowNum, Student.LastName);
            ExcelHelper.AddTextCell(workbookpart, worksheetPart, "B", RowNum, Student.FirstName);
            ExcelHelper.AddTextCell(workbookpart, worksheetPart, "C", RowNum, Student.IsActive.ToYesOrNo());
            ExcelHelper.AddTextCell(workbookpart, worksheetPart, "D", RowNum, Student.HomeRoom);
            ExcelHelper.AddCurrencyCell(workbookpart, worksheetPart, "E", RowNum, totalMoneyIn);
            ExcelHelper.AddCurrencyCell(workbookpart, worksheetPart, "F", RowNum, (totalMoneyOut * -1));
            ExcelHelper.AddCurrencyCell(workbookpart, worksheetPart, "G", RowNum, totalBalance);
            ExcelHelper.AddTextCell(workbookpart, worksheetPart, "H", RowNum, (!(totalBalance >= (decimal)0.00)).ToYesOrNo());
        }
    }
}
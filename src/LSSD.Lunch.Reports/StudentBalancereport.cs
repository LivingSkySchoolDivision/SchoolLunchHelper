using System;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

// A fairly simple spreadsheet of students with balances
// Constrained by school
// Shows active students and inactive students (identified by a field)

// For help with how to work with OpenXml documents:
// https://docs.microsoft.com/en-us/office/open-xml/how-do-i


namespace LSSD.Lunch.Reports
{
    class StudentBalanceReport
    {       
        public void Generate(List<Student> Students, School School, string Filename) {
            if (File.Exists(Filename)) {
                File.Delete(Filename);
            }

            using (SpreadsheetDocument document = SpreadsheetDocument.Create(Filename, SpreadsheetDocumentType.Workbook))
            {

                // Header rows
                //  Report title
                //  School name
                
                // Summary row - all students - total owed
                // Summary row - all students - total spent


                WorkbookPart workbookpart = document.AddWorkbookPart();
                workbookpart.Workbook = new Workbook();

                WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());

                Sheets sheets = document.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

                Sheet sheetOne = new Sheet() {
                    Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Sheet 1"
                };
                sheets.Append(sheetOne);

                workbookpart.Workbook.Save();

                document.Close();               

            }
        }
    }
}
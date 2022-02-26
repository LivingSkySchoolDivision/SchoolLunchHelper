using System;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace LSSD.Lunch.Reports
{
    public static class ExcelHelper
    {   

        public static void AddDateCell(WorkbookPart WorkbookPart, WorksheetPart WorksheetPart, string Column, uint Row, string Date)
        {
            Cell cell = insertCellInWorksheet(Column, Row, WorksheetPart);
            cell.CellValue = new CellValue(Date);
            cell.DataType = new EnumValue<CellValues>(CellValues.Date);            
        }

        public static void AddNumberCell(WorkbookPart WorkbookPart, WorksheetPart WorksheetPart, string Column, uint Row, int Number)
        {
            Cell cell = insertCellInWorksheet(Column, Row, WorksheetPart);
            cell.CellValue = new CellValue(Number);
            cell.DataType = new EnumValue<CellValues>(CellValues.Number);            
        }

        public static void AddCurrencyCell(WorkbookPart WorkbookPart, WorksheetPart WorksheetPart, string Column, uint Row, decimal Number)
        {
            Cell cell = insertCellInWorksheet(Column, Row, WorksheetPart);
            cell.CellValue = new CellValue(Number);
            cell.DataType = new EnumValue<CellValues>(CellValues.Number);
        }

        public static void AddNumberCell(WorkbookPart WorkbookPart, WorksheetPart WorksheetPart, string Column, uint Row, decimal Number)
        {
            Cell cell = insertCellInWorksheet(Column, Row, WorksheetPart);
            cell.CellValue = new CellValue(Number);
            cell.DataType = new EnumValue<CellValues>(CellValues.Number);
        }

        public static void AddPageReportTitleCell(WorkbookPart WorkbookPart, WorksheetPart WorksheetPart, string Column, uint Row, string Text)
        {
            AddTextCell(WorkbookPart, WorksheetPart, Column, Row, Text);
        }
        public static void AddHeadingCell(WorkbookPart WorkbookPart, WorksheetPart WorksheetPart, string Column, uint Row, string Text)
        {
            AddTextCell(WorkbookPart, WorksheetPart, Column, Row, Text);
        }

        public static void AddTextCell(WorkbookPart WorkbookPart, WorksheetPart WorksheetPart, string Column, uint Row, string Text)
        {
            // Save the string in the shared string table
            int stringIndex = addNewSharedString(getSharedStringTable(WorkbookPart), Text);

            // Put the string from the shared string table in the specified cell
            Cell cell = insertCellInWorksheet(Column, Row, WorksheetPart);
            cell.CellValue = new CellValue(stringIndex.ToString());
            cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
        }

        private static int addNewSharedString(SharedStringTablePart SharedStringtable, string Text)
        {
            if (SharedStringtable.SharedStringTable == null)
            {
                SharedStringtable.SharedStringTable = new SharedStringTable();
            }

            int i = 0;

            // Iterate through all the items in the SharedStringTable. If the text already exists, return its index.
            foreach (SharedStringItem item in SharedStringtable.SharedStringTable.Elements<SharedStringItem>())
            {
                if (item.InnerText == Text)
                {
                    return i;
                }

                i++;
            }

            // The text does not exist in the part. Create the SharedStringItem and return its index.
            SharedStringtable.SharedStringTable.AppendChild(new SharedStringItem(new DocumentFormat.OpenXml.Spreadsheet.Text(Text)));
            SharedStringtable.SharedStringTable.Save();

            return i;

        }

        private static SharedStringTablePart getSharedStringTable(WorkbookPart WorkbookPart)
        {
            SharedStringTablePart returnMe;
            if (WorkbookPart.GetPartsOfType<SharedStringTablePart>().Count() > 0)
            {
                returnMe = WorkbookPart.GetPartsOfType<SharedStringTablePart>().First();
            }
            else
            {
                returnMe = WorkbookPart.AddNewPart<SharedStringTablePart>();
            }
            return returnMe;
        }


        private static Cell insertCellInWorksheet(string columnName, uint rowIndex, WorksheetPart worksheetPart)
        {
            Worksheet worksheet = worksheetPart.Worksheet;
            SheetData sheetData = worksheet.GetFirstChild<SheetData>();
            string cellReference = columnName + rowIndex;

            // If the worksheet does not contain a row with the specified row index, insert one.
            Row row;
            if (sheetData.Elements<Row>().Where(r => r.RowIndex == rowIndex).Count() != 0)
            {
                row = sheetData.Elements<Row>().Where(r => r.RowIndex == rowIndex).First();
            }
            else
            {
                row = new Row() { RowIndex = rowIndex };
                sheetData.Append(row);
            }

            // If there is not a cell with the specified column name, insert one.
            if (row.Elements<Cell>().Where(c => c.CellReference.Value == columnName + rowIndex).Count() > 0)
            {
                return row.Elements<Cell>().Where(c => c.CellReference.Value == cellReference).First();
            }
            else
            {
                // Cells must be in sequential order according to CellReference. Determine where to insert the new cell.
                Cell refCell = null;
                foreach (Cell cell in row.Elements<Cell>())
                {
                    if (cell.CellReference.Value.Length == cellReference.Length)
                    {
                    if (string.Compare(cell.CellReference.Value, cellReference, true) > 0)
                    {
                        refCell = cell;
                        break;
                    }
                    }
                }

                Cell newCell = new Cell() { CellReference = cellReference };
                row.InsertBefore(newCell, refCell);

                worksheet.Save();
                return newCell;
            }
        }

        public static bool AddBasicStyles(DocumentFormat.OpenXml.Packaging.SpreadsheetDocument spreadsheet) {
            DocumentFormat.OpenXml.Spreadsheet.Stylesheet stylesheet = spreadsheet.WorkbookPart.WorkbookStylesPart.Stylesheet;

            // Numbering formats (x:numFmts)
            stylesheet.InsertAt<DocumentFormat.OpenXml.Spreadsheet.NumberingFormats>(new DocumentFormat.OpenXml.Spreadsheet.NumberingFormats(), 0);
            // Currency
            stylesheet.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.NumberingFormats>().InsertAt<DocumentFormat.OpenXml.Spreadsheet.NumberingFormat>(
                new DocumentFormat.OpenXml.Spreadsheet.NumberingFormat() {
                    NumberFormatId = 164,
                    FormatCode = "#,##0.00"
                    + "\\ \"" + System.Globalization.CultureInfo.CurrentUICulture.NumberFormat.CurrencySymbol + "\""
                }, 0);

            // Fonts (x:fonts)
            stylesheet.InsertAt<DocumentFormat.OpenXml.Spreadsheet.Fonts>(new DocumentFormat.OpenXml.Spreadsheet.Fonts(), 1);
            stylesheet.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Fonts>().InsertAt<DocumentFormat.OpenXml.Spreadsheet.Font>(
                new DocumentFormat.OpenXml.Spreadsheet.Font() {
                FontSize = new DocumentFormat.OpenXml.Spreadsheet.FontSize() {
                    Val = 11
                },
                FontName = new DocumentFormat.OpenXml.Spreadsheet.FontName() {
                    Val = "Calibri"
                }
                }, 0);

            // Fills (x:fills)
            stylesheet.InsertAt<DocumentFormat.OpenXml.Spreadsheet.Fills>(new DocumentFormat.OpenXml.Spreadsheet.Fills(), 2);
            stylesheet.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Fills>().InsertAt<DocumentFormat.OpenXml.Spreadsheet.Fill>(
                new DocumentFormat.OpenXml.Spreadsheet.Fill() {
                PatternFill = new DocumentFormat.OpenXml.Spreadsheet.PatternFill() {
                    PatternType = new DocumentFormat.OpenXml.EnumValue<DocumentFormat.OpenXml.Spreadsheet.PatternValues>() {
                        Value = DocumentFormat.OpenXml.Spreadsheet.PatternValues.None
                    }
                }
                }, 0);

            // Borders (x:borders)
            stylesheet.InsertAt<DocumentFormat.OpenXml.Spreadsheet.Borders>(new DocumentFormat.OpenXml.Spreadsheet.Borders(), 3);
            stylesheet.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Borders>().InsertAt<DocumentFormat.OpenXml.Spreadsheet.Border>(
                new DocumentFormat.OpenXml.Spreadsheet.Border() {
                LeftBorder = new DocumentFormat.OpenXml.Spreadsheet.LeftBorder(),
                RightBorder = new DocumentFormat.OpenXml.Spreadsheet.RightBorder(),
                TopBorder = new DocumentFormat.OpenXml.Spreadsheet.TopBorder(),
                BottomBorder = new DocumentFormat.OpenXml.Spreadsheet.BottomBorder(),
                DiagonalBorder = new DocumentFormat.OpenXml.Spreadsheet.DiagonalBorder()
                }, 0);

            // Cell style formats (x:CellStyleXfs)
            stylesheet.InsertAt<DocumentFormat.OpenXml.Spreadsheet.CellStyleFormats>(new DocumentFormat.OpenXml.Spreadsheet.CellStyleFormats(), 4);
            stylesheet.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.CellStyleFormats>().InsertAt<DocumentFormat.OpenXml.Spreadsheet.CellFormat>(
                new DocumentFormat.OpenXml.Spreadsheet.CellFormat() {
                NumberFormatId = 0,
                FontId = 0,
                FillId = 0,
                BorderId = 0
                }, 0);

            // Cell formats (x:CellXfs)
            stylesheet.InsertAt<DocumentFormat.OpenXml.Spreadsheet.CellFormats>(new DocumentFormat.OpenXml.Spreadsheet.CellFormats(), 5);
            // General text
            stylesheet.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.CellFormats>().InsertAt<DocumentFormat.OpenXml.Spreadsheet.CellFormat>(
                new DocumentFormat.OpenXml.Spreadsheet.CellFormat() {
                FormatId = 0,
                NumberFormatId = 0
                }, 0);
            // Date
            stylesheet.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.CellFormats>().InsertAt<DocumentFormat.OpenXml.Spreadsheet.CellFormat>(
                new DocumentFormat.OpenXml.Spreadsheet.CellFormat() {
                ApplyNumberFormat = true,
                FormatId = 0,
                NumberFormatId = 22,
                FontId = 0,
                FillId = 0,
                BorderId = 0
                },
                1);
            // Currency
            stylesheet.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.CellFormats>().InsertAt<DocumentFormat.OpenXml.Spreadsheet.CellFormat>(
                new DocumentFormat.OpenXml.Spreadsheet.CellFormat() {
                ApplyNumberFormat = true,
                FormatId = 0,
                NumberFormatId = 164,
                FontId = 0,
                FillId = 0,
                BorderId = 0
                },
                2);
            // Percentage
            stylesheet.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.CellFormats>().InsertAt<DocumentFormat.OpenXml.Spreadsheet.CellFormat>(
                new DocumentFormat.OpenXml.Spreadsheet.CellFormat() {
                ApplyNumberFormat = true,
                FormatId = 0,
                NumberFormatId = 10,
                FontId = 0,
                FillId = 0,
                BorderId = 0
                },
                3);


            return true;
        }

    }
}



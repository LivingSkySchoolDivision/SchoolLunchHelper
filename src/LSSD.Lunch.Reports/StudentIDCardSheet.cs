using System;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using OpenXMLDrawing = DocumentFormat.OpenXml.Drawing;
using OpenXMLDrawingWP = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using OpenXMLPictures = DocumentFormat.OpenXml.Drawing.Pictures;
using NetBarcode;


// For help with how to work with OpenXml documents:
// https://docs.microsoft.com/en-us/office/open-xml/how-do-i

namespace LSSD.Lunch.Reports
{
    class StudentIDCardSheet
    {
        private Dictionary<string, string> barcodeReferenceIDs = new Dictionary<string, string>();

        public void Generate(List<Student> Students, string Filename) {
            if (File.Exists(Filename)) {
                File.Delete(Filename);
            }

            using (WordprocessingDocument document = WordprocessingDocument.Create(Filename, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = document.AddMainDocumentPart();
                
                LSSDDocumentStyles.AddStylesToDocument(document);

                // Add images to document
                // These need to be added first, and then they can be referenced in the rendered document later
                EmbedStudentBarcodeImages(document, Students);

                mainPart.Document = GenerateBody(Students.OrderBy(x => x.HomeRoom).ThenBy(x => x.LastName).ThenBy(x => x.FirstName).ToList());
            }
        }
        
        private Document GenerateBody(List<Student> Students) {

            List<OpenXmlElement> pageParts = new List<OpenXmlElement>();
                        
            int columns = 2;
                        
            // Split students into groups based on columns
            List<List<Student>> studentGroups = new List<List<Student>>();
            
            for(int x = 0; x < Students.Count; x+=columns)
            {
                studentGroups.Add(Students.GetRange(x,Math.Min(Students.Count - x, columns)));
            }
           
            foreach(List<Student> studentGroup in studentGroups) 
            {
                TableRow row = TableHelper.StickyTableRow();

                for(int x = 0; x < columns; x++) 
                {
                    if (studentGroup.Count > x)
                    {
                        row.AppendChild(
                            TableHelper.ContainerCell(
                                generateIDCard(studentGroup[x]),
                                ParagraphHelper.WhiteSpace(),
                                ParagraphHelper.WhiteSpace()
                            )
                        );
                    } else {
                        row.AppendChild(TableHelper.EmptyCell());
                    }
                }

                pageParts.Add(
                    TableHelper.BorderlessTable(row)
                );
            }

            return new Document(new Body(pageParts));
        }

        private void EmbedStudentBarcodeImages(WordprocessingDocument document, List<Student> students)
        {
            if (document != null)
            {
                if (document.MainDocumentPart != null)
                {
                    if (students.Count > 0)
                    {
                        foreach(Student student in students)
                        {
                            Barcode testBarcode = new Barcode(student.StudentId, NetBarcode.Type.Code39, true, 300, 75);
                            ImagePart imagePart = document.MainDocumentPart.AddImagePart(ImagePartType.Jpeg);
                            imagePart.FeedData(new MemoryStream(testBarcode.GetByteArray()));
                            if (!barcodeReferenceIDs.ContainsKey(student.StudentId)) {
                                barcodeReferenceIDs.Add(student.StudentId, document.MainDocumentPart.GetIdOfPart(imagePart));
                            }
                        }
                    }
                }
            }
        }


        private OpenXmlElement generateIDCard(Student student) 
        {
            Table returnMe = new Table(
                new TableLayout() { Type = TableLayoutValues.Autofit },
                new TableWidth() {
                    Type = TableWidthUnitValues.Pct,
                    Width = $"{95 * 50}" // 40% in fiftieths of a percent
                },
                LSSDTableStyles.ThickOutsideBorders("A0A0A0"),
                LSSDTableStyles.WideMargins()
            );            

            returnMe.AppendChild(
                new TableRow(
                    new TableRowProperties(
                        new CantSplit()
                    ),
                    new TableCell(
                        ParagraphHelper.Paragraph(student.Name, LSSDDocumentStyles.IDCardName, JustificationValues.Center)
                    )                  
                )                
            );

            returnMe.AppendChild(
                new TableRow(
                    new TableRowProperties(
                        new CantSplit()
                    ),
                    new TableCell(
                        ParagraphHelper.Paragraph(student.HomeRoom, LSSDDocumentStyles.IDCardHomeRoom, JustificationValues.Center)
                    )                  
                )                
            );

            returnMe.AppendChild(
                new TableRow(
                    new TableCell(
                        new Paragraph(
                            insertImageByStudentId(student.StudentId)
                        ) {
                            ParagraphProperties = new ParagraphProperties(
                                new Justification() { Val = JustificationValues.Center },
                                new SpacingBetweenLines() { Before = "0", After = "0" }
                            ) {
                                ParagraphStyleId = new ParagraphStyleId() {
                                    Val = LSSDDocumentStyles.FieldLabel
                                }
                            }
                        }
                    ).WithWidth(50)
                )
            );

            return returnMe;

        }

        private OpenXmlElement insertImageByStudentId(string studentID)
        {
            if (!string.IsNullOrEmpty(studentID))
            {
                if (barcodeReferenceIDs.ContainsKey(studentID))
                {
                    string referenceId = barcodeReferenceIDs[studentID];

                    Drawing returnMe = new Drawing(
                        new OpenXMLDrawingWP.Inline(
                            new OpenXMLDrawingWP.Extent() { Cx = 2415040, Cy = 336960 }, // { Cx = 1715040, Cy = 336960 },
                            new OpenXMLDrawingWP.EffectExtent()
                            {
                                LeftEdge = 0L,
                                TopEdge = 0L,
                                RightEdge = 0L,
                                BottomEdge = 0L
                            },
                            new OpenXMLDrawingWP.DocProperties()
                            {
                                Id = (UInt32Value)1U,
                                Name = studentID
                            },
                            new OpenXMLDrawingWP.NonVisualGraphicFrameDrawingProperties(
                                new OpenXMLDrawing.GraphicFrameLocks() { NoChangeAspect = true }),
                                new OpenXMLDrawing.Graphic(
                                    new OpenXMLDrawing.GraphicData(
                                        new OpenXMLPictures.Picture(
                                            new OpenXMLPictures.NonVisualPictureProperties(
                                                new OpenXMLPictures.NonVisualDrawingProperties()
                                                {
                                                    Id = (UInt32Value)0U,
                                                    Name = studentID
                                                },
                                                new OpenXMLPictures.NonVisualPictureDrawingProperties()
                                            ),
                                            new OpenXMLPictures.BlipFill(
                                                new OpenXMLDrawing.Blip(
                                                    new OpenXMLDrawing.BlipExtensionList(
                                                        new OpenXMLDrawing.BlipExtension()
                                                        {
                                                            Uri = "{28A0092B-C50C-407E-A947-70E740481C1C}" // Blip extension URI
                                                        })
                                                )
                                                {
                                                    Embed = referenceId,
                                                    CompressionState = OpenXMLDrawing.BlipCompressionValues.Print
                                                },
                                                new OpenXMLDrawing.Stretch(
                                                    new OpenXMLDrawing.FillRectangle()
                                                )
                                            ),
                                            new OpenXMLPictures.ShapeProperties(
                                                new OpenXMLDrawing.Transform2D(
                                                    new OpenXMLDrawing.Offset() { X = 0L, Y = 0L },
                                                    new OpenXMLDrawing.Extents() { Cx = 3858400, Cy = 561600 }), //{ Cx = 2858400, Cy = 561600 }
                                                    new OpenXMLDrawing.PresetGeometry(
                                                        new OpenXMLDrawing.AdjustValueList()
                                                )
                                                    {
                                                        Preset = OpenXMLDrawing.ShapeTypeValues.Rectangle
                                                    }
                                                )
                                            )
                                        )
                                        { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }
                                )
                            )
                        {
                            DistanceFromTop = (UInt32Value)0U,
                            DistanceFromBottom = (UInt32Value)0U,
                            DistanceFromLeft = (UInt32Value)0U,
                            DistanceFromRight = (UInt32Value)0U,
                            EditId = "50D07946" // This needs to be here. The docs don't explain what it is.
                        }
                            );
                    return new Run(returnMe);

                }
            }

            return new Run();
        }

    }
}
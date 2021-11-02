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

                mainPart.Document = GenerateBody(Students);
            }
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
                    Width = $"{40 * 50}" // 40% in fiftieths of a percent
                },
                LSSDTableStyles.ThickOutsideBorders(),
                LSSDTableStyles.WideMargins()
            );            

            returnMe.AppendChild(
                new TableRow(
                    new TableRowProperties(
                        new CantSplit()
                    ),
                    new TableCell(
                        new Paragraph(
                            new Run(
                                new Text(student.Name)
                            )                            
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
                    )
                )
            );

            return returnMe;

        }

        private OpenXmlElement insertImageByStudentId(string studentID)
        {
            //return new Run(new Text("Derp"));

            if (!string.IsNullOrEmpty(studentID))
            {
                if (barcodeReferenceIDs.ContainsKey(studentID))
                {
                    string referenceId = barcodeReferenceIDs[studentID];

                    Drawing returnMe = new Drawing(
                        new OpenXMLDrawingWP.Inline(
                            new OpenXMLDrawingWP.Extent() { Cx = 1715040, Cy = 336960 },
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
                                                    new OpenXMLDrawing.Extents() { Cx = 2858400, Cy = 561600 }),
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

        private Document GenerateBody(List<Student> Students) {

            List<OpenXmlElement> pageParts = new List<OpenXmlElement>();

            // For each student, add an ID card
            // Should be able to fit 6-8 per page
            
            foreach(Student student in Students) 
            {
                pageParts.Add(generateIDCard(student));
                pageParts.Add(ParagraphHelper.WhiteSpace());
                pageParts.Add(ParagraphHelper.WhiteSpace());
                pageParts.Add(ParagraphHelper.WhiteSpace());
            }

            return new Document(new Body(pageParts));
        }


    }
}
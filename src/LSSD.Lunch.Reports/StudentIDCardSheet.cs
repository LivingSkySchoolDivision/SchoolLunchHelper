using System;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;


// For help with how to work with OpenXml documents:
// https://docs.microsoft.com/en-us/office/open-xml/how-do-i

namespace LSSD.Lunch.Reports 
{
    class StudentIDCardSheet
    {
        public void Generate(List<Student> Students, string Filename) {
            if (File.Exists(Filename)) {
                File.Delete(Filename);
            }

            using (WordprocessingDocument document = WordprocessingDocument.Create(Filename, WordprocessingDocumentType.Document)) 
            {
                MainDocumentPart mainPart = document.AddMainDocumentPart();

                // add styles to document here                           
                LSSDDocumentStyles.AddStylesToDocument(document);
                
                mainPart.Document = GenerateBody(Students);                
            }            
        }

        private Document GenerateBody(List<Student> Students) {

            List<OpenXmlElement> pageParts = new List<OpenXmlElement>();

            // For each student, add an ID card
            // Should be able to fit 6-8 per page

            pageParts.AddRange(
                new List<OpenXmlElement>() {
                    new Paragraph(
                        new Run(
                            new Text("TEST")
                        )
                    )  {
                        ParagraphProperties = new ParagraphProperties(
                            new Justification() { Val = JustificationValues.Left },
                            new KeepNext(),
                            new KeepLines(),
                            new SpacingBetweenLines() { Before = "0", After = "0" }
                        ) {
                            ParagraphStyleId = new ParagraphStyleId() {
                                Val = LSSDDocumentStyles.NormalParagraph
                            }
                        }
                    }
                }
            );
            
            return new Document(new Body(pageParts));
        }

        
    }
}
using System;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace LSSD.Lunch.Reports {
    class LSSDDocumentStyles {

        public const string IDCardName = "ID Card Name";
        public const string IDCardHomeRoom = "ID Card Homeroom";

        public const string FieldLabel = "Field Label";
        public const string FieldValue = "Field Value";
        public const string FieldValueBig = "Field Value Big";
        public const string FieldValueYes = "Field Value Yes";
        public const string FieldValueNo = "Field Value No";
        public const string PageTitle = "Page Title";
        public const string SectionTitle = "Section Title";
        public const string SubSectionTitle = "SubSection Title";
        public const string NormalParagraph = "Normal Paragraph";
        public const string Dim = "Dim";
        public const string WhiteSpace = "Whitespace";

        private const string DefaultValueColour = "375ca6";
        private const string FontName = "Times New Roman";

        private static StyleDefinitionsPart addStylePrerequisites(WordprocessingDocument doc)
        {
            if (doc?.MainDocumentPart?.StyleDefinitionsPart == null) {
                StyleDefinitionsPart part;
                part = doc.MainDocumentPart.AddNewPart<StyleDefinitionsPart>();
                Styles root = new Styles();
                root.Save(part);
                return part;
            } else {
                return doc.MainDocumentPart.StyleDefinitionsPart;
            }
        }

        public static void AddStylesToDocument(WordprocessingDocument doc) {
            // This is stupid and Microsoft should feel bad for how this whole SDK works
            addStyles(addStylePrerequisites(doc));
        }

        private static void addStyles(StyleDefinitionsPart stylePart) 
            {
                if (stylePart.Styles == null)
                {
                    stylePart.Styles = new Styles();
                    stylePart.Styles.Save();
                }

                // Create a new paragraph style element and specify some of the attributes.
                stylePart.Styles.Append(new Style(
                    new PrimaryStyle() { Val = OnOffOnlyValues.On }, // Should it show up in the list of styles in the editor
                    new StyleName() { Val = PageTitle }
                ) { 
                    Type = StyleValues.Paragraph,
                    StyleId = PageTitle,
                    CustomStyle = true,
                    Default = false,
                    StyleRunProperties = new StyleRunProperties(
                        new Bold(), 
                        new Color() { ThemeColor = ThemeColorValues.Accent2 }, 
                        new RunFonts() { Ascii = FontName },
                        new FontSize() { Val = "32" } // Double the font size value you see in Word
                    )
                });


                stylePart.Styles.Append(new Style(
                    new PrimaryStyle() { Val = OnOffOnlyValues.On }, // Should it show up in the list of styles in the editor
                    new StyleName() { Val = IDCardName }
                ) { 
                    Type = StyleValues.Paragraph,
                    StyleId = IDCardName,
                    CustomStyle = true,
                    Default = false,
                    StyleRunProperties = new StyleRunProperties(
                        new Bold(),
                        new RunFonts() { Ascii = FontName },
                        new FontSize() { Val = "26" } // Double the font size value you see in Word
                    ){
                        Color = new Color() { Val = "000000" }
                    }
                });

                stylePart.Styles.Append(new Style(
                    new PrimaryStyle() { Val = OnOffOnlyValues.On }, // Should it show up in the list of styles in the editor
                    new StyleName() { Val = IDCardHomeRoom }
                ) { 
                    Type = StyleValues.Paragraph,
                    StyleId = IDCardHomeRoom,
                    CustomStyle = true,
                    Default = false,
                    StyleRunProperties = new StyleRunProperties(
                        new RunFonts() { Ascii = FontName },
                        new FontSize() { Val = "26" } // Double the font size value you see in Word
                    ){
                        Color = new Color() { Val = "000000" }
                    }
                });


                stylePart.Styles.Append(new Style(
                    new PrimaryStyle() { Val = OnOffOnlyValues.On }, // Should it show up in the list of styles in the editor
                    new StyleName() { Val = SectionTitle }
                ) { 
                    Type = StyleValues.Paragraph,
                    StyleId = SectionTitle,
                    CustomStyle = true,
                    Default = false,
                    StyleRunProperties = new StyleRunProperties(
                        new Bold(),
                        new Color() { ThemeColor = ThemeColorValues.Accent2 }, 
                        new RunFonts() { Ascii = FontName },
                        new FontSize() { Val = "24" } // Double the font size value you see in Word
                    )
                });

                stylePart.Styles.Append(new Style(
                    new PrimaryStyle() { Val = OnOffOnlyValues.On }, // Should it show up in the list of styles in the editor
                    new StyleName() { Val = SubSectionTitle }
                ) { 
                    Type = StyleValues.Paragraph,
                    StyleId = SubSectionTitle,
                    CustomStyle = true,
                    Default = false,
                    StyleRunProperties = new StyleRunProperties(
                        new Color() { ThemeColor = ThemeColorValues.Accent2 }, 
                        new RunFonts() { Ascii = FontName },
                        new FontSize() { Val = "20" } // Double the font size value you see in Word
                    )
                });

                stylePart.Styles.Append(new Style(
                    new PrimaryStyle() { Val = OnOffOnlyValues.On }, // Should it show up in the list of styles in the editor
                    new StyleName() { Val = FieldLabel },
                    new ContextualSpacing() { Val = false }
                ) { 
                    Type = StyleValues.Paragraph,
                    StyleId = FieldLabel,
                    CustomStyle = true,
                    Default = false,
                    StyleRunProperties = new StyleRunProperties(
                        new Bold(), 
                        new Color() { ThemeColor = ThemeColorValues.Text1 }, 
                        new RunFonts() { Ascii = FontName },
                        new FontSize() { Val = "26" } // Double the font size value you see in Word
                    )
                });
                
                stylePart.Styles.Append(new Style(
                    new PrimaryStyle() { Val = OnOffOnlyValues.On }, // Should it show up in the list of styles in the editor
                    new StyleName() { Val = NormalParagraph }
                ) { 
                    Type = StyleValues.Paragraph,
                    StyleId = NormalParagraph,
                    CustomStyle = true,
                    Default = false,
                    StyleRunProperties = new StyleRunProperties(
                        new RunFonts() { Ascii = FontName },
                        new FontSize() { Val = "18" } // Double the font size value you see in Word
                    ){
                        Color = new Color() { Val = "000000" }
                    }
                });

                stylePart.Styles.Append(new Style(
                    new PrimaryStyle() { Val = OnOffOnlyValues.On }, // Should it show up in the list of styles in the editor
                    new StyleName() { Val = Dim }
                ) { 
                    Type = StyleValues.Paragraph,
                    StyleId = Dim,
                    CustomStyle = true,
                    Default = false,
                    StyleRunProperties = new StyleRunProperties(
                        new RunFonts() { Ascii = FontName },
                        new FontSize() { Val = "16" } // Double the font size value you see in Word
                    ){
                        Color = new Color() { Val = "A0A0A0" }
                    }
                });

                stylePart.Styles.Append(new Style(
                    new PrimaryStyle() { Val = OnOffOnlyValues.On }, // Should it show up in the list of styles in the editor
                    new StyleName() { Val = FieldValue },
                    new ContextualSpacing() { Val = false }
                ) { 
                    Type = StyleValues.Paragraph,
                    StyleId = FieldValue,
                    CustomStyle = true,
                    Default = false,
                    StyleRunProperties = new StyleRunProperties(
                        new RunFonts() { Ascii = FontName },
                        new FontSize() { Val = "18" } // Double the font size value you see in Word
                    ){
                        Color = new Color() { Val = DefaultValueColour }
                    }
                });

                stylePart.Styles.Append(new Style(
                    new PrimaryStyle() { Val = OnOffOnlyValues.On }, // Should it show up in the list of styles in the editor
                    new StyleName() { Val = FieldValueBig },
                    new ContextualSpacing() { Val = false }
                ) { 
                    Type = StyleValues.Paragraph,
                    StyleId = FieldValueBig,
                    CustomStyle = true,
                    Default = false,
                    StyleRunProperties = new StyleRunProperties(
                        new RunFonts() { Ascii = FontName },
                        new FontSize() { Val = "22" } // Double the font size value you see in Word
                    ){
                        Color = new Color() { Val = DefaultValueColour }
                    }
                });

                stylePart.Styles.Append(new Style(
                    new PrimaryStyle() { Val = OnOffOnlyValues.On }, // Should it show up in the list of styles in the editor
                    new StyleName() { Val = FieldValueYes },
                    new ContextualSpacing() { Val = false }
                ) { 
                    Type = StyleValues.Paragraph,
                    StyleId = FieldValueYes,
                    CustomStyle = true,
                    Default = false,
                    StyleRunProperties = new StyleRunProperties(
                        new Bold(), 
                        new RunFonts() { Ascii = FontName },
                        new FontSize() { Val = "18" } // Double the font size value you see in Word
                    ){
                        Color = new Color() { Val = "007700" }
                    }
                });

                stylePart.Styles.Append(new Style(
                    new PrimaryStyle() { Val = OnOffOnlyValues.On }, // Should it show up in the list of styles in the editor
                    new StyleName() { Val = FieldValueNo },
                    new ContextualSpacing() { Val = false }
                ) { 
                    Type = StyleValues.Paragraph,
                    StyleId = FieldValueNo,
                    CustomStyle = true,
                    Default = false,
                    StyleRunProperties = new StyleRunProperties(
                        new RunFonts() { Ascii = FontName },
                        new FontSize() { Val = "16" } // Double the font size value you see in Word
                    ){
                        Color = new Color() { Val = "C0C0C0" }
                    }
                }); 

                stylePart.Styles.Append(new Style(
                    new PrimaryStyle() { Val = OnOffOnlyValues.Off }, // Should it show up in the list of styles in the editor
                    new StyleName() { Val = WhiteSpace }
                ) { 
                    Type = StyleValues.Paragraph,
                    StyleId = WhiteSpace,
                    CustomStyle = true,
                    Default = false,
                    StyleRunProperties = new StyleRunProperties(
                        new RunFonts() { Ascii = FontName },
                        new FontSize() { Val = "12" } // Double the font size value you see in Word
                    ){
                        Color = new Color() { Val = "C0C0C0" }
                    }
                }); 
            }
    }
}
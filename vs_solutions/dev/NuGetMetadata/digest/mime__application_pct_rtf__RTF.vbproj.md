# mime/application%rtf/RTF.vbproj

- RootNamespace : Microsoft.VisualBasic.MIME.RTF
- AssemblyName  : Microsoft.VisualBasic.MIME.RTF
- TargetFramework: v4.8
- Source files  : 6
- Existing Title: Rich Text Format and Office MathML Document Model
- Existing Desc : Builds rich text format documents in the sciBASIC# framework: a document object model that tracks styled text regions and fonts and emits RTF markup, plus serializable Office Math Markup Language models for equations.
- Existing Tags : 

## Namespaces
- Models  [files: 1]
- Omml  [files: 2]

## Public types
- Class Font (Font.vb) - Font style of the selected text region.
- Class FormatedRegion (FormatedRegion.vb)
- Class HTML (Omml\HTML.vb)
- Class DocumentXmlProperty (Omml\XML.vb)
- Class DocumentProperties (Omml\XML.vb)
- Class OfficeDocumentSettings (Omml\XML.vb)
- Class WordDocument (Omml\XML.vb)
- Class Compatibility (Omml\XML.vb)
- Class mathPr (Omml\XML.vb)
- Structure ValueAttribute (Omml\XML.vb)
- Class Paragraph (Omml\XML.vb)
- Class Font (Omml\XML.vb)
- Class StyleTokens (Omml\XML.vb)
- Class Rtf (Rtf.vb) - Rich text format document object model.(带有格式描述信息的文本文档的对象模型)

## Notable public members
- Public Const RTF_LF As String = "\par"
- Public Const RTF_FONT_STYLE_BOLD As String = "\b"
- Public Const RTF_FONT_STYLE_ITALIC As String = "\i"
- Public Const RTF_FONT_SIZE As String = "\fs"
- Public Const RTF_FONT_STYLE_UNDER_LINE As String = "\ul"
- Public Const RTF_FONT_STYLE_NONE_UNDER_LINE As String = "\ulnone"
- Public Overrides Function ToString() As String
- Public Shared Function FromExistsValue(Font As Font, Color As Color) As Font
- Public Function GenerateRTFTAG(Region As FormatedRegion) As String
- Public Function Clone() As Font
- Public Shared Function FontColorToString(R As Integer, G As Integer, B As Integer) As String
- Public Overloads Shared Function ToString(Color As Color) As String
- Public Property Font As Font
- Public ReadOnly Property Text As String
- Public ReadOnly Property Start As Integer
- Public ReadOnly Property Right As Integer
- Public Function Contains(p As Integer) As Boolean
- Public Overrides Function ToString() As String
- Public Function GenerateDocumentText() As String
- Public ReadOnly Property HaveParFlag As Boolean
- Public Property FontSize As Integer
- Public Property FontBold As Boolean
- Public Property FontFamilyName As String
- Public Property FontItalic As Boolean
- Public Property FontColor As Color
- Public Property FontUnderline As Boolean
- Public Overrides Function ToString() As String
- Public Const WORD_XML_NAMESPACE As String = "xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"" xmlns:w=""u…
- Public Property Head As Head
- Public Function SaveDocument(path As String, Optional encoding As System.Text.Encoding = Nothing) As Boolean
- Public Const WORD_XML_METADATA As String = ""
- Public Property DocumentProperties As Omml.DocumentProperties
- Public Property OfficeDocumentSettings As OfficeDocumentSettings
- Public Property Author As String
- Public Property LastAuthor As String
- Public Property Revision As Integer
- Public Property TotalTime As Integer
- Public Property Created As Date
- Public Property LastSaved As Date
- Public Property Pages As Integer
- Public Property Words As Integer
- Public Property Characters As Long
- Public Property Lines As Integer
- Public Property Paragraphs As Integer
- Public Property CharactersWithSpaces As Long
- Public Property Version As String
- Public Overrides Function ToString() As String
- Public Property AllowPNG
- Public Property TrackMoves As Boolean
- Public Property TrackFormatting
- Public Property PunctuationKerning
- Public Property DrawingGridHorizontalSpacing As String
- Public Property DrawingGridVerticalSpacing As String
- Public Property DisplayHorizontalDrawingGridEvery As Integer
- Public Property DisplayVerticalDrawingGridEvery As Integer
- Public Property UseMarginsForDrawingGridOrigin
- Public Property ValidateAgainstSchemas As Boolean
- Public Property SaveIfXMLInvalid As Boolean
- Public Property IgnoreMixedContent As Boolean
- Public Property AlwaysShowPlaceholderText As Boolean
- ... and 67 more

## Imports
- Microsoft.VisualBasic.Imaging
- Microsoft.VisualBasic.Language
- Microsoft.VisualBasic.Serialization.JSON
- System.Drawing
- System.Text
- System.Xml.Serialization

## File tree
- Font.vb
- FormatedRegion.vb
- Models\Font.vb
- Omml\HTML.vb
- Omml\XML.vb
- Rtf.vb


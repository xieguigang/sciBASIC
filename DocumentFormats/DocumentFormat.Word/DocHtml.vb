Imports System.Text
Imports System.Xml.Serialization

Namespace Omml

    ''' <summary>
    ''' Omml: office microsoft word xml
    ''' </summary>
    ''' <remarks></remarks>
    <XmlRoot("html", namespace:="http://www.w3.org/TR/REC-html40")>
    Public Class DocHtml

        Public Const WORD_XML_NAMESPACE As String = "xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"" xmlns:w=""urn:schemas-microsoft-com:office:word"" xmlns:m=""http://schemas.microsoft.com/office/2004/12/omml"""

        Public Property Head As Head

        Public Function SaveDocument(path As String, Optional encoding As System.Text.Encoding = Nothing) As Boolean
            Throw New NotImplementedException
        End Function

        Private Function InternalCreateDocument() As String
            Throw New NotImplementedException
        End Function
    End Class

    <XmlType("head")>
    Public Class Head

        Public Const WORD_XML_METADATA As String = ""

        <XmlElement("title")> Public Property Title As String
        <XmlIgnore> Public Property Xml As DocumentXmlProperty
    End Class

    <XmlType("xml")>
    Public Class DocumentXmlProperty
        Public Property DocumentProperties As DocumentFormat.Word.Omml.DocumentProperties
        Public Property OfficeDocumentSettings As OfficeDocumentSettings
    End Class

    Public Class DocumentProperties
        Public Property Author As String
        Public Property LastAuthor As String
        Public Property Revision As Integer
        Public Property TotalTime As Integer
        Public Property Created As Date
        Public Property LastSaved As Date
        Public Property Pages As Integer
        Public Property Words As Integer
        Public Property Characters As Long
        Public Property Lines As Integer
        Public Property Paragraphs As Integer
        Public Property CharactersWithSpaces As Long
        Public Property Version As String
    End Class

    Public Class OfficeDocumentSettings
        Public Property AllowPNG
    End Class

    Public Class WordDocument
        Public Property TrackMoves As Boolean
        Public Property TrackFormatting
        Public Property PunctuationKerning
        Public Property DrawingGridHorizontalSpacing As String
        Public Property DrawingGridVerticalSpacing As String
        Public Property DisplayHorizontalDrawingGridEvery As Integer
        Public Property DisplayVerticalDrawingGridEvery As Integer
        Public Property UseMarginsForDrawingGridOrigin
        Public Property ValidateAgainstSchemas As Boolean
        Public Property SaveIfXMLInvalid As Boolean
        Public Property IgnoreMixedContent As Boolean
        Public Property AlwaysShowPlaceholderText As Boolean
        Public Property DoNotUnderlineInvalidXML
        Public Property DoNotPromoteQF
        Public Property LidThemeOther As String
        Public Property LidThemeAsian As String
        Public Property LidThemeComplexScript As String
        Public Property DoNotShadeFormData

        Public Class Compatibility
            Public Property SpaceForUL
            Public Property BalanceSingleByteDoubleByteWidth
            Public Property DoNotLeaveBackslashAlone
            Public Property ULTrailSpace
            Public Property DoNotExpandShiftReturn
            Public Property FootnoteLayoutLikeWW8
            Public Property ShapeLayoutLikeWW8
            Public Property AlignTablesRowByRow
            Public Property ForgetLastTabAlignment
            Public Property AdjustLineHeightInTable
            Public Property LayoutRawTableWidth
            Public Property LayoutTableRowsApart
            Public Property UseWord97LineBreakingRules
            Public Property SelectEntireFieldWithStartOrEnd
            Public Property UseWord2002TableStyleRules
            Public Property DontUseIndentAsNumberingTabStop
            Public Property FELineBreak11
            Public Property WW11IndentRules
            Public Property DontAutofitConstrainedTables
            Public Property AutofitLikeWW11
            Public Property UnderlineTabInNumList
            Public Property HangulWidthLikeWW11
            Public Property UseNormalStyleForList
            Public Property DontVertAlignCellWithSp
            Public Property DontBreakConstrainedForcedTables
            Public Property DontVertAlignInTxbx
            Public Property Word11KerningPairs
            Public Property CachedColBalance
            Public Property UseFELayout
        End Class

        Public Class mathPr
            Public Property mathFont As ValueAttribute
            Public Property brkBin As ValueAttribute
            Public Property brkBinSub As ValueAttribute
            Public Property smallFrac As ValueAttribute
            Public Property dispDef As ValueAttribute
            Public Property lMargin As ValueAttribute
            Public Property rMargin As ValueAttribute
            Public Property defJc As ValueAttribute
            Public Property wrapIndent As ValueAttribute
            Public Property intLim As ValueAttribute
            Public Property naryLim As ValueAttribute
        End Class

        Public Structure ValueAttribute
            <XmlAttribute> Public Property Value As String
        End Structure
    End Class

    Public Class Paragraph

        Public Property Align As String = "left"
        Public Property Style As StyleTokens

        Public Function GenerateDocument() As String
            Dim sBuilder As StringBuilder = New StringBuilder("<p class=MsoNormal ", 1024)
            Call sBuilder.Append("align=" & Align)
            Call sBuilder.Append("style='" & Style.ToString & "' ")

            Return sBuilder.ToString
        End Function
    End Class

    Public Class Font : Inherits ComponentModels.Font

    End Class

    Public Class StyleTokens

        Public Property TextAlign As String
        Public Property MsoLayoutGridAlign As String
        Public Property TextAutospace As String
        Public Property Border As String
        Public Property MsoBorderBottomAlt As String
        Public Property Padding As String
        Public Property MsoPaddingAlt As String

        Public Overrides Function ToString() As String
            Dim Value As StringBuilder = New StringBuilder(1024)
            Dim InternalBuild = Sub(Name As String, svalue As String) If Not String.IsNullOrEmpty(svalue) Then Call Value.Append(Name & ":" & svalue & ";")

            Call InternalBuild("border", Me.Border)
            Call InternalBuild("mso-border-bottom-alt", Me.MsoBorderBottomAlt)
            Call InternalBuild("mso-layout-grid-align", Me.MsoLayoutGridAlign)
            Call InternalBuild("mso-padding-alt", Me.MsoPaddingAlt)
            Call InternalBuild("padding", Me.Padding)
            Call InternalBuild("text-align", Me.TextAlign)
            Call InternalBuild("text-autospace", Me.TextAutospace)

            Return Value.ToString
        End Function
    End Class
End Namespace
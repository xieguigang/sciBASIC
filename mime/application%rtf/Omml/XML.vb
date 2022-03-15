#Region "Microsoft.VisualBasic::91c6161d6997c7beb669113adbb8da1d, sciBASIC#\mime\application%rtf\Omml\XML.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 159
    '    Code Lines: 136
    ' Comment Lines: 0
    '   Blank Lines: 23
    '     File Size: 6.42 KB


    '     Class Head
    ' 
    '         Properties: Title, Xml
    ' 
    '     Class DocumentXmlProperty
    ' 
    '         Properties: DocumentProperties, OfficeDocumentSettings
    ' 
    '     Class DocumentProperties
    ' 
    '         Properties: Author, Characters, CharactersWithSpaces, Created, LastAuthor
    '                     LastSaved, Lines, Pages, Paragraphs, Revision
    '                     TotalTime, Version, Words
    ' 
    '         Function: ToString
    ' 
    '     Class OfficeDocumentSettings
    ' 
    '         Properties: AllowPNG
    ' 
    '     Class WordDocument
    ' 
    '         Properties: AlwaysShowPlaceholderText, DisplayHorizontalDrawingGridEvery, DisplayVerticalDrawingGridEvery, DoNotPromoteQF, DoNotShadeFormData
    '                     DoNotUnderlineInvalidXML, DrawingGridHorizontalSpacing, DrawingGridVerticalSpacing, IgnoreMixedContent, LidThemeAsian
    '                     LidThemeComplexScript, LidThemeOther, PunctuationKerning, SaveIfXMLInvalid, TrackFormatting
    '                     TrackMoves, UseMarginsForDrawingGridOrigin, ValidateAgainstSchemas
    '         Class Compatibility
    ' 
    '             Properties: AdjustLineHeightInTable, AlignTablesRowByRow, AutofitLikeWW11, BalanceSingleByteDoubleByteWidth, CachedColBalance
    '                         DoNotExpandShiftReturn, DoNotLeaveBackslashAlone, DontAutofitConstrainedTables, DontBreakConstrainedForcedTables, DontUseIndentAsNumberingTabStop
    '                         DontVertAlignCellWithSp, DontVertAlignInTxbx, FELineBreak11, FootnoteLayoutLikeWW8, ForgetLastTabAlignment
    '                         HangulWidthLikeWW11, LayoutRawTableWidth, LayoutTableRowsApart, SelectEntireFieldWithStartOrEnd, ShapeLayoutLikeWW8
    '                         SpaceForUL, ULTrailSpace, UnderlineTabInNumList, UseFELayout, UseNormalStyleForList
    '                         UseWord2002TableStyleRules, UseWord97LineBreakingRules, Word11KerningPairs, WW11IndentRules
    ' 
    '         Class mathPr
    ' 
    '             Properties: brkBin, brkBinSub, defJc, dispDef, intLim
    '                         lMargin, mathFont, naryLim, rMargin, smallFrac
    '                         wrapIndent
    ' 
    '         Structure ValueAttribute
    ' 
    '             Properties: Value
    ' 
    ' 
    ' 
    '     Class Paragraph
    ' 
    '         Properties: Align, Style
    ' 
    '         Function: GenerateDocument
    ' 
    '     Class Font
    ' 
    ' 
    ' 
    '     Class StyleTokens
    ' 
    '         Properties: Border, MsoBorderBottomAlt, MsoLayoutGridAlign, MsoPaddingAlt, Padding
    '                     TextAlign, TextAutospace
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Omml

    <XmlType("head")> Public Class Head

        Public Const WORD_XML_METADATA As String = ""

        <XmlElement("title")> Public Property Title As String
        <XmlIgnore> Public Property Xml As DocumentXmlProperty
    End Class

    <XmlType("xml")>
    Public Class DocumentXmlProperty
        Public Property DocumentProperties As Omml.DocumentProperties
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

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
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

    Public Class Font : Inherits Models.Font

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
            Dim value As New StringBuilder(1024)
            Dim build = Sub(Name As String, svalue As String) If Not String.IsNullOrEmpty(svalue) Then Call value.Append(Name & ":" & svalue & ";")

            Call build("border", Me.Border)
            Call build("mso-border-bottom-alt", Me.MsoBorderBottomAlt)
            Call build("mso-layout-grid-align", Me.MsoLayoutGridAlign)
            Call build("mso-padding-alt", Me.MsoPaddingAlt)
            Call build("padding", Me.Padding)
            Call build("text-align", Me.TextAlign)
            Call build("text-autospace", Me.TextAutospace)

            Return value.ToString
        End Function
    End Class
End Namespace

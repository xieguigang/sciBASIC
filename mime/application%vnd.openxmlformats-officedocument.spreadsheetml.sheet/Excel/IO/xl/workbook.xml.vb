#Region "Microsoft.VisualBasic::e88af68f45fec018daa0ed4fc1017e10, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\IO\xl\workbook.xml.vb"

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

    '     Class workbook
    ' 
    '         Properties: AlternateContent, bookViews, calcPr, definedNames, extLst
    '                     fileRecoveryPr, fileVersion, Ignorable, sheets, workbookPr
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Add, GetSheetIDByIndex, GetSheetIDByName
    ' 
    '     Class ext
    ' 
    '         Properties: slicerStyles, timelineStyles, uri, workbookPr
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class slicerStyles
    ' 
    '         Properties: defaultSlicerStyle
    ' 
    '     Class timelineStyles
    ' 
    '         Properties: defaultTimelineStyle
    ' 
    '     Class definedName
    ' 
    '         Properties: hidden, name, value
    ' 
    '     Class AlternateContent
    ' 
    '         Properties: Choice
    ' 
    '     Class Choice
    ' 
    '         Properties: absPath, Requires
    ' 
    '     Class absPath
    ' 
    '         Properties: url
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class fileRecoveryPr
    ' 
    '         Properties: autoRecover
    ' 
    '     Class workbookPr
    ' 
    '         Properties: chartTrackingRefBase, defaultThemeVersion, filterPrivacy
    ' 
    '     Structure calcPr
    ' 
    '         Properties: calcId
    ' 
    '     Structure sheet
    ' 
    '         Properties: name, rid, sheetId
    ' 
    '         Function: ToString
    ' 
    '     Structure workbookView
    ' 
    '         Properties: activeTab, uid, windowHeight, windowWidth, xWindow
    '                     yWindow
    ' 
    '     Structure fileVersion
    ' 
    '         Properties: appName, lastEdited, lowestEdited, rupBuild
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports OpenXML = Microsoft.VisualBasic.MIME.Office.Excel.Model.Xmlns
Imports worksheet = Microsoft.VisualBasic.Language.List(Of Microsoft.VisualBasic.MIME.Office.Excel.XML.xl.sheet)

Namespace XML.xl

    ''' <summary>
    ''' workbook.xml
    ''' </summary>
    <XmlRoot("workbook", [Namespace]:="http://schemas.openxmlformats.org/spreadsheetml/2006/main")>
    Public Class workbook

        Public Property fileVersion As fileVersion
        Public Property bookViews As workbookView()
        Public Property sheets As New worksheet
        Public Property calcPr As calcPr
        Public Property workbookPr As workbookPr
        Public Property fileRecoveryPr As fileRecoveryPr
        Public Property definedNames As definedName()

        Public Property extLst As ext()

        <XmlElement("AlternateContent", [Namespace]:=OpenXML.mc)>
        Public Property AlternateContent As AlternateContent

        <XmlAttribute(NameOf(Ignorable), [Namespace]:=OpenXML.mc)>
        Public Property Ignorable As String

        <XmlNamespaceDeclarations()>
        Public xmlns As XmlSerializerNamespaces

        Sub New()
            xmlns = New XmlSerializerNamespaces

            xmlns.Add("r", OpenXML.r)
            xmlns.Add("mc", OpenXML.mc)
            xmlns.Add("x15", OpenXML.x15)
            xmlns.Add("xr2", OpenXML.xr2)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetSheetIDByIndex(index As Integer) As String
            Return sheets(index).sheetId
        End Function

        ''' <summary>
        ''' 不存在会返回空字符串
        ''' </summary>
        ''' <param name="name$"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetSheetIDByName(name$) As String
            Return sheets _
                .SafeQuery _
                .Where(Function(s) s.name.TextEquals(name)) _
                .FirstOrDefault _
                .rid
        End Function

        Public Function Add(sheetName$) As String
            Dim n$ = sheets.Count + 1

            sheets += New sheet With {
                .name = sheetName,
                .rid = "rId" & n,
                .sheetId = n
            }
            Return "sheet" & n
        End Function
    End Class

    <XmlType("ext", [Namespace]:=OpenXML.x15)>
    Public Class ext

        <XmlAttribute>
        Public Property uri As String
        Public Property workbookPr As workbookPr
        <XmlElement("slicerStyles", [Namespace]:=OpenXML.x14)>
        Public Property slicerStyles As slicerStyles
        Public Property timelineStyles As timelineStyles

        <XmlNamespaceDeclarations()>
        Public xmlns As XmlSerializerNamespaces

        Sub New()
            xmlns = New XmlSerializerNamespaces
            xmlns.Add("x15", OpenXML.x15)
            xmlns.Add("x14", OpenXML.x14)
        End Sub
    End Class

    Public Class slicerStyles
        <XmlAttribute>
        Public Property defaultSlicerStyle As String
    End Class

    Public Class timelineStyles
        <XmlAttribute>
        Public Property defaultTimelineStyle As String
    End Class

    Public Class definedName
        <XmlAttribute> Public Property name As String
        <XmlAttribute> Public Property hidden As String
        <XmlText> Public Property value As String
    End Class

    <XmlRoot("AlternateContent", [Namespace]:=OpenXML.mc)>
    Public Class AlternateContent
        Public Property Choice As Choice
    End Class

    Public Class Choice

        <XmlAttribute>
        Public Property Requires As String
        Public Property absPath As absPath
    End Class

    <XmlRoot("absPath", [Namespace]:=OpenXML.x15ac)>
    Public Class absPath

        <XmlAttribute>
        Public Property url As String

        <XmlNamespaceDeclarations()>
        Public xmlns As XmlSerializerNamespaces

        Sub New()
            xmlns = New XmlSerializerNamespaces
            xmlns.Add("x15ac", OpenXML.x15ac)
        End Sub
    End Class

    Public Class fileRecoveryPr
        <XmlAttribute> Public Property autoRecover As String
    End Class

    Public Class workbookPr
        <XmlAttribute> Public Property defaultThemeVersion As String
        <XmlAttribute> Public Property chartTrackingRefBase As String
        <XmlAttribute> Public Property filterPrivacy As String
    End Class

    Public Structure calcPr
        <XmlAttribute> Public Property calcId As String
    End Structure

    Public Structure sheet

        <XmlAttribute> Public Property name As String
        <XmlAttribute> Public Property sheetId As String

        <XmlAttribute("id", [Namespace]:=OpenXML.r)>
        Public Property rid As String

        Public Overrides Function ToString() As String
            Return name
        End Function
    End Structure

    Public Structure workbookView
        <XmlAttribute> Public Property xWindow As String
        <XmlAttribute> Public Property yWindow As String
        <XmlAttribute> Public Property windowWidth As String
        <XmlAttribute> Public Property windowHeight As String
        <XmlAttribute> Public Property activeTab As String

        <XmlAttribute("uid", [Namespace]:=OpenXML.xr2)>
        Public Property uid As String
    End Structure

    Public Structure fileVersion

        <XmlAttribute> Public Property appName As String
        <XmlAttribute> Public Property lastEdited As String
        <XmlAttribute> Public Property lowestEdited As String
        <XmlAttribute> Public Property rupBuild As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure
End Namespace

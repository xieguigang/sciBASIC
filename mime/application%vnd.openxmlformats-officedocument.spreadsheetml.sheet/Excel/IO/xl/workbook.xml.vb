#Region "Microsoft.VisualBasic::9a4cc889c94fe6bb81f4c960c8314039, ..\sciBASIC#\mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\xl\workbook.xml.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
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

        <XmlElement("AlternateContent", [Namespace]:=mc)>
        Public Property AlternateContent As AlternateContent

        <XmlAttribute(NameOf(Ignorable), [Namespace]:=mc)>
        Public Property Ignorable As String

        <XmlNamespaceDeclarations()>
        Public xmlns As XmlSerializerNamespaces

        Sub New()
            xmlns = New XmlSerializerNamespaces

            xmlns.Add("r", Excel.Xmlns.r)
            xmlns.Add("mc", Excel.Xmlns.mc)
            xmlns.Add("x15", Excel.Xmlns.x15)
            xmlns.Add("xr2", Excel.Xmlns.xr2)
        End Sub

        ''' <summary>
        ''' 不存在会返回空字符串
        ''' </summary>
        ''' <param name="name$"></param>
        ''' <returns></returns>
        Public Function GetSheetIDByName(name$) As String
            Return sheets _
                .SafeQuery _
                .Where(Function(s) s.name.TextEquals(name)) _
                .FirstOrDefault _
                .sheetId
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

    <XmlRoot("AlternateContent", [Namespace]:=mc)>
    Public Class AlternateContent
        Public Property Choice As Choice
    End Class

    Public Class Choice

        <XmlAttribute>
        Public Property Requires As String
        Public Property absPath As absPath
    End Class

    <XmlRoot("absPath", [Namespace]:=x15ac)>
    Public Class absPath

        <XmlAttribute>
        Public Property url As String

        <XmlNamespaceDeclarations()>
        Public xmlns As XmlSerializerNamespaces

        Sub New()
            xmlns = New XmlSerializerNamespaces
            xmlns.Add("x15ac", x15ac)
        End Sub
    End Class

    Public Class fileRecoveryPr
        <XmlAttribute> Public Property autoRecover As String
    End Class

    Public Class workbookPr
        <XmlAttribute>
        Public Property defaultThemeVersion As String
    End Class

    Public Structure calcPr
        <XmlAttribute> Public Property calcId As String
    End Structure

    Public Structure sheet
        <XmlAttribute> Public Property name As String
        <XmlAttribute> Public Property sheetId As String

        <XmlAttribute("id", [Namespace]:=r)>
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

        <XmlAttribute("uid", [Namespace]:=xr2)>
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

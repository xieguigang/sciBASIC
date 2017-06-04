#Region "Microsoft.VisualBasic::af7d03785418bcf92596b69febcdc0bf, ..\sciBASIC#\mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\xl\workbook.xml.vb"

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
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace xl

    Public Class workbook
        Public Property fileVersion As fileVersion
        Public Property bookViews As workbookView()
        Public Property sheets As sheet()
        Public Property calcPr As calcPr
    End Class

    Public Structure calcPr
        <XmlAttribute> Public Property calcId As String
    End Structure

    Public Structure sheet
        <XmlAttribute> Public Property name As String
        <XmlAttribute> Public Property sheetId As String
        <XmlAttribute> Public Property rid As String
    End Structure

    Public Structure workbookView
        <XmlAttribute> Public Property xWindow As String
        <XmlAttribute> Public Property yWindow As String
        <XmlAttribute> Public Property windowWidth As String
        <XmlAttribute> Public Property windowHeight As String
        <XmlAttribute> Public Property activeTab As String
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




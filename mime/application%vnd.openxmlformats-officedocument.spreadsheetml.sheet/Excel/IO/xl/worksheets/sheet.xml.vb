#Region "Microsoft.VisualBasic::91df299201cc9b32932252ecbe3650eb, ..\sciBASIC#\mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\IO\xl\worksheets\sheet.xml.vb"

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

Namespace XML.xl.worksheets

    <XmlRoot("worksheet", [Namespace]:="http://schemas.openxmlformats.org/spreadsheetml/2006/main")>
    Public Class worksheet

        Public Property dimension As dimension

        Public Property cols As col()
        Public Property sheetData As sheetData
        Public Property phoneticPr As phoneticPr
        Public Property pageMargins As pageMargins
        Public Property sheetViews As sheetView()

        <XmlAttribute("uid", [Namespace]:=xr)>
        Public Property uid As String
        <XmlAttribute(NameOf(Ignorable), [Namespace]:=mc)>
        Public Property Ignorable As String

        <XmlNamespaceDeclarations()>
        Public xmlns As XmlSerializerNamespaces

        Sub New()
            xmlns = New XmlSerializerNamespaces
            xmlns.Add("r", r)
            xmlns.Add("mc", mc)
            xmlns.Add("x14ac", x14ac)
            xmlns.Add("xr", xr)
            xmlns.Add("xr2", xr2)
            xmlns.Add("xr3", xr3)
        End Sub

        Public Overrides Function ToString() As String
            Return uid
        End Function
    End Class

    Public Class sheetData
        <XmlElement("row")> Public Property rows As row()
    End Class

    Public Class sheetView
        <XmlAttribute> Public Property tabSelected As String
        <XmlAttribute> Public Property workbookViewId As String
        Public Property selection As selection
    End Class

    Public Class selection
        <XmlAttribute> Public Property activeCell As String
        <XmlAttribute> Public Property sqref As String
    End Class

    Public Structure dimension

        <XmlAttribute>
        Public Property ref As String

        Public Overrides Function ToString() As String
            Return ref
        End Function
    End Structure

    Public Class col
        <XmlAttribute> Public Property min As String
        <XmlAttribute> Public Property max As String
        <XmlAttribute> Public Property width As String
        <XmlAttribute> Public Property style As String
        <XmlAttribute> Public Property bestFit As String
        <XmlAttribute> Public Property customWidth As String
    End Class

    Public Structure row
        <XmlAttribute> Public Property r As String
        <XmlAttribute> Public Property spans As String

        <XmlAttribute(NameOf(dyDescent), [Namespace]:=x14ac)>
        Public Property dyDescent As String

        <XmlAttribute> Public Property ht As String
        <XmlAttribute> Public Property customHeight As String
        <XmlAttribute> Public Property customFormat As String
        <XmlElement("c")> Public Property columns As c()

        Public Overrides Function ToString() As String
            Return columns _
                .Select(Function(c) c.r) _
                .ToArray _
                .GetJson
        End Function
    End Structure

    Public Structure c

        ''' <summary>
        ''' Reference location, cell ID
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property r As String
        <XmlAttribute> Public Property s As String
        ''' <summary>
        ''' Type, if this property value is ``s``, then it means value <see cref="v"/> refernece from <see cref="sharedStrings"/>
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property t As String

        ''' <summary>
        ''' Value
        ''' </summary>
        ''' <returns></returns>
        Public Property v As String

        ''' <summary>
        ''' 返回-1表示非引用类型，即<see cref="v"/>直接可以用作为值
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property sharedStringsRef As Integer
            Get
                If t.TextEquals("s") Then
                    Return Val(v)
                Else
                    Return -1
                End If
            End Get
        End Property

        Public Overrides Function ToString() As String
            Dim value$ = v

            If t.TextEquals("s") Then
                value = $"sharedStrings({value})"
            End If

            Return $"[{r}] {value}"
        End Function
    End Structure

    Public Class pageMargins
        <XmlAttribute> Public Property left As Double
        <XmlAttribute> Public Property right As Double
        <XmlAttribute> Public Property top As Double
        <XmlAttribute> Public Property bottom As Double
        <XmlAttribute> Public Property header As Double
        <XmlAttribute> Public Property footer As Double
    End Class
End Namespace

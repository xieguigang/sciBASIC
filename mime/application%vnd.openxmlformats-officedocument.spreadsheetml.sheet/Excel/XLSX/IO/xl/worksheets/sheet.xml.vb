#Region "Microsoft.VisualBasic::efc3cd498e15724e9065707f5330f680, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\IO\xl\worksheets\sheet.xml.vb"

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

    '   Total Lines: 251
    '    Code Lines: 180 (71.71%)
    ' Comment Lines: 27 (10.76%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 44 (17.53%)
    '     File Size: 8.53 KB


    '     Class worksheet
    ' 
    '         Properties: cols, conditionalFormattings, dimension, drawing, hyperlinks
    '                     Ignorable, pageMargins, pageSetup, phoneticPr, sheetData
    '                     sheetFormatPr, sheetViews, uid
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class drawing
    ' 
    '         Properties: id
    ' 
    '     Class hyperlink
    ' 
    '         Properties: id, ref
    ' 
    '     Class conditionalFormatting
    ' 
    '         Properties: cfRule, sqref
    ' 
    '     Class cfRule
    ' 
    '         Properties: [operator], colorScale, dxfId, formula, priority
    '                     type
    ' 
    '     Class colorScale
    ' 
    '         Properties: cfvo, colors
    ' 
    '     Class sheetFormatPr
    ' 
    '         Properties: defaultRowHeight, dyDescent
    ' 
    '     Class sheetData
    ' 
    '         Properties: rows
    ' 
    '     Class sheetView
    ' 
    '         Properties: pane, selection, tabSelected, workbookViewId, zoomScale
    '                     zoomScaleNormal
    ' 
    '     Class pane
    ' 
    '         Properties: activePane, state, topLeftCell, ySplit
    ' 
    '     Class selection
    ' 
    '         Properties: activeCell, pane, sqref
    ' 
    '     Structure dimension
    ' 
    '         Properties: ref
    ' 
    '         Function: ToString
    ' 
    '     Class col
    ' 
    '         Properties: bestFit, customWidth, max, min, style
    '                     width
    ' 
    '     Structure row
    ' 
    '         Properties: collapsed, columns, customFormat, customHeight, dyDescent
    '                     hidden, ht, outlineLevel, r, s
    '                     spans
    ' 
    '         Function: ToString
    ' 
    '     Structure [is]
    ' 
    '         Properties: t
    ' 
    '         Function: ToString
    ' 
    '     Structure c
    ' 
    '         Properties: [is], r, s, sharedStringsRef, t
    '                     v
    ' 
    '         Function: GetValueString, ToString
    ' 
    '     Class pageMargins
    ' 
    '         Properties: bottom, footer, header, left, right
    '                     top
    ' 
    '     Class pageSetup
    ' 
    '         Properties: id, orientation, paperSize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON
Imports OpenXML = Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Model.Xmlns

Namespace XLSX.XML.xl.worksheets

    ''' <summary>
    ''' 保存数据所使用到的工作表的对象模型
    ''' </summary>
    <XmlRoot("worksheet", [Namespace]:="http://schemas.openxmlformats.org/spreadsheetml/2006/main")>
    Public Class worksheet

        Public Property dimension As dimension

        Public Property cols As col()
        Public Property sheetData As sheetData
        Public Property phoneticPr As phoneticPr
        Public Property pageMargins As pageMargins
        Public Property pageSetup As pageSetup
        Public Property sheetViews As sheetView()
        Public Property sheetFormatPr As sheetFormatPr

        <XmlElement("conditionalFormatting")>
        Public Property conditionalFormattings As conditionalFormatting()
        Public Property hyperlinks As hyperlink()
        Public Property drawing As drawing

        <XmlAttribute("uid", [Namespace]:=OpenXML.xr)>
        Public Property uid As String
        <XmlAttribute(NameOf(Ignorable), [Namespace]:=OpenXML.mc)>
        Public Property Ignorable As String

        <XmlNamespaceDeclarations()>
        Public xmlns As XmlSerializerNamespaces

        Sub New()
            xmlns = New XmlSerializerNamespaces
            xmlns.Add("r", OpenXML.r)
            xmlns.Add("mc", OpenXML.mc)
            xmlns.Add("x14ac", OpenXML.x14ac)
            xmlns.Add("xr", OpenXML.xr)
            xmlns.Add("xr2", OpenXML.xr2)
            xmlns.Add("xr3", OpenXML.xr3)
            xmlns.Add("x", "http://schemas.openxmlformats.org/spreadsheetml/2006/main")
        End Sub

        Public Overrides Function ToString() As String
            Return uid
        End Function
    End Class

    Public Class drawing
        <XmlAttribute("id", [Namespace]:=OpenXML.r)>
        Public Property id As String
    End Class

    Public Class hyperlink
        <XmlAttribute> Public Property ref As String
        <XmlAttribute("id", [Namespace]:=OpenXML.r)>
        Public Property id As String
    End Class

    Public Class conditionalFormatting

        ''' <summary>
        ''' 单元格的引用范围
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute>
        Public Property sqref As String
        Public Property cfRule As cfRule

    End Class

    Public Class cfRule
        <XmlAttribute> Public Property type As String
        <XmlAttribute> Public Property dxfId As String
        <XmlAttribute> Public Property priority As String
        <XmlAttribute> Public Property [operator] As String

        Public Property formula As String
        Public Property colorScale As colorScale
    End Class

    Public Class colorScale
        <XmlElement("cfvo")>
        Public Property cfvo As StringValue()
        <XmlElement("color")>
        Public Property colors As ColorValue()
    End Class

    Public Class sheetFormatPr
        <XmlAttribute>
        Public Property defaultRowHeight As String
        <XmlAttribute("dyDescent", [Namespace]:=OpenXML.x14ac)>
        Public Property dyDescent As String
    End Class

    Public Class sheetData
        <XmlElement("row")> Public Property rows As row()
    End Class

    Public Class sheetView
        <XmlAttribute> Public Property zoomScale As String
        <XmlAttribute> Public Property zoomScaleNormal As String
        <XmlAttribute> Public Property tabSelected As String
        <XmlAttribute> Public Property workbookViewId As String
        Public Property selection As selection
        Public Property pane As pane
    End Class

    Public Class pane
        <XmlAttribute> Public Property ySplit As String
        <XmlAttribute> Public Property topLeftCell As String
        <XmlAttribute> Public Property activePane As String
        <XmlAttribute> Public Property state As String
    End Class

    Public Class selection
        <XmlAttribute> Public Property activeCell As String
        <XmlAttribute> Public Property sqref As String
        <XmlAttribute> Public Property pane As String
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
        <XmlAttribute> Public Property s As String

        <XmlAttribute(NameOf(dyDescent), [Namespace]:=OpenXML.x14ac)>
        Public Property dyDescent As String

        <XmlAttribute> Public Property ht As String
        <XmlAttribute> Public Property customHeight As String
        <XmlAttribute> Public Property customFormat As String
        <XmlElement("c")> Public Property columns As c()

        <XmlAttribute> Public Property hidden As String
        <XmlAttribute> Public Property outlineLevel As Integer
        <XmlAttribute> Public Property collapsed As String

        Public Overrides Function ToString() As String
            Return columns _
                .Select(Function(c) c.r) _
                .ToArray _
                .GetJson
        End Function
    End Structure

    Public Structure [is]

        Public Property t As String

        Public Overrides Function ToString() As String
            Return t
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
        Public Property [is] As [is]

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

        ''' <summary>
        ''' 当前单元格内的数据可能会存储在<see cref="v"/>属性或者<see cref="[is]"/>属性之中
        ''' </summary>
        ''' <returns></returns>
        Public Function GetValueString() As String
            If [is].t Is Nothing Then
                Return v
            Else
                Return [is].t
            End If
        End Function

        Public Overrides Function ToString() As String
            Dim value$ = GetValueString()

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

    Public Class pageSetup
        <XmlAttribute> Public Property paperSize As String
        <XmlAttribute> Public Property orientation As String
        <XmlAttribute("id", [Namespace]:=OpenXML.r)>
        Public Property id As String
    End Class
End Namespace

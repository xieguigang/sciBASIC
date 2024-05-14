#Region "Microsoft.VisualBasic::843414ab28881a3714d04d7668a0c9d2, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\IO\xl\styles.xml.vb"

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

    '   Total Lines: 195
    '    Code Lines: 150
    ' Comment Lines: 3
    '   Blank Lines: 42
    '     File Size: 6.30 KB


    '     Class styles
    ' 
    '         Properties: borders, cellStyles, cellStyleXfs, cellXfs, dxfs
    '                     extLst, fills, fonts, Ignorable, tableStyles
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class List
    ' 
    '         Properties: count
    ' 
    '     Class tableStyles
    ' 
    '         Properties: defaultPivotStyle, defaultTableStyle
    ' 
    '     Class tableStyle
    ' 
    '         Properties: defaultPivotStyle, defaultTableStyle, elements, name, pivot
    '                     table
    ' 
    '     Class tableStyleElement
    ' 
    '         Properties: dxfId, type
    ' 
    '     Class dxfs
    ' 
    ' 
    ' 
    '     Class dxf
    ' 
    '         Properties: fill, font
    ' 
    '     Class cellStyles
    ' 
    ' 
    ' 
    '     Class cellStyle
    ' 
    '         Properties: builtinId, name, xfId
    ' 
    '     Class cellXfs
    ' 
    ' 
    ' 
    '     Class cellStyleXfs
    ' 
    ' 
    ' 
    '     Class xf
    ' 
    '         Properties: applyAlignment, applyBorder, applyFill, applyFont, applyNumberFormat
    '                     applyProtection, borderId, fillId, fontId, numFmtId
    '                     xfId
    ' 
    '     Class borders
    ' 
    ' 
    ' 
    '     Class border
    ' 
    '         Properties: bottom, diagonal, left, right, top
    ' 
    '     Class fills
    ' 
    ' 
    ' 
    '     Class fill
    ' 
    '         Properties: patternFill
    ' 
    '     Class patternFill
    ' 
    '         Properties: bgColor, fgColor, patternType
    ' 
    '     Class fonts
    ' 
    '         Properties: fonts, knownFonts
    ' 
    '     Class font
    ' 
    '         Properties: b, charset, color, family, i
    '                     name, scheme, sz, u
    ' 
    '     Class StringValue
    ' 
    '         Properties: type, val
    ' 
    '         Function: ToString
    ' 
    '     Class ColorValue
    ' 
    '         Properties: indexed, lastClr, rgb, theme
    ' 
    '         Function: ToString
    ' 
    '     Class Flag
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Imaging
Imports OpenXML = Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Model.Xmlns

Namespace XLSX.XML.xl

    <XmlRoot("styleSheet", [Namespace]:="http://schemas.openxmlformats.org/spreadsheetml/2006/main")>
    Public Class styles

        Public Property fonts As fonts
        Public Property fills As fills
        Public Property borders As borders
        Public Property cellStyleXfs As cellStyleXfs
        Public Property cellXfs As cellXfs
        Public Property cellStyles As cellStyles
        Public Property dxfs As dxfs
        Public Property tableStyles As tableStyles
        Public Property extLst As ext()

        <XmlAttribute("Ignorable", [Namespace]:=OpenXML.mc)>
        Public Property Ignorable As String

        <XmlNamespaceDeclarations()>
        Public xmlns As XmlSerializerNamespaces

        Sub New()
            xmlns = New XmlSerializerNamespaces

            xmlns.Add("mc", OpenXML.mc)
            xmlns.Add("x14ac", OpenXML.x14ac)
            xmlns.Add("x16r2", OpenXML.x16r2)
        End Sub

    End Class

    Public Class List(Of T)
        <XmlAttribute> Public Property count As Integer
    End Class

    Public Class tableStyles : Inherits List(Of tableStyle)
        <XmlAttribute> Public Property defaultTableStyle As String
        <XmlAttribute> Public Property defaultPivotStyle As String
    End Class

    Public Class tableStyle : Inherits List(Of tableStyleElement)
        <XmlAttribute> Public Property defaultTableStyle As String
        <XmlAttribute> Public Property defaultPivotStyle As String
        <XmlAttribute> Public Property name As String
        <XmlAttribute> Public Property pivot As String
        <XmlAttribute> Public Property table As String
        <XmlElement("tableStyleElement")>
        Public Property elements As tableStyleElement()
    End Class

    Public Class tableStyleElement
        <XmlAttribute> Public Property type As String
        <XmlAttribute> Public Property dxfId As String
    End Class

    Public Class dxfs : Inherits List(Of dxf)

    End Class

    Public Class dxf
        Public Property font As font
        Public Property fill As fill
    End Class

    Public Class cellStyles : Inherits List(Of cellStyle)

    End Class

    Public Class cellStyle
        <XmlAttribute> Public Property name As String
        <XmlAttribute> Public Property xfId As String
        <XmlAttribute> Public Property builtinId As String
    End Class

    Public Class cellXfs : Inherits List(Of xf)

    End Class

    Public Class cellStyleXfs : Inherits List(Of xf)

    End Class

    Public Class xf
        <XmlAttribute> Public Property xfId As String
        <XmlAttribute> Public Property applyFont As String
        <XmlAttribute> Public Property numFmtId As String
        <XmlAttribute> Public Property fontId As String
        <XmlAttribute> Public Property fillId As String
        <XmlAttribute> Public Property borderId As String
        <XmlAttribute> Public Property applyNumberFormat As String
        <XmlAttribute> Public Property applyFill As String
        <XmlAttribute> Public Property applyBorder As String
        <XmlAttribute> Public Property applyAlignment As String
        <XmlAttribute> Public Property applyProtection As String
    End Class

    Public Class borders : Inherits List(Of border)

    End Class

    Public Class border
        Public Property left As String
        Public Property right As String
        Public Property top As String
        Public Property bottom As String
        Public Property diagonal As String
    End Class

    Public Class fills : Inherits List(Of fill)

    End Class

    Public Class fill
        Public Property patternFill As patternFill
    End Class

    Public Class patternFill
        Public Property patternType As String
        Public Property fgColor As ColorValue
        Public Property bgColor As ColorValue
    End Class

    Public Class fonts : Inherits List(Of font)
        <XmlAttribute("knownFonts", [Namespace]:=OpenXML.x14ac)>
        Public Property knownFonts As String
        <XmlElement>
        Public Property fonts As font()
    End Class

    ''' <summary>
    ''' Font style in Xlsx
    ''' </summary>
    Public Class font
        Public Property b As Flag
        Public Property i As Flag
        Public Property u As Flag
        Public Property sz As StringValue
        Public Property color As ColorValue
        Public Property name As StringValue
        Public Property family As StringValue
        Public Property charset As StringValue
        Public Property scheme As StringValue
    End Class

    Public Class StringValue

        <XmlAttribute>
        Public Property type As String
        <XmlAttribute>
        Public Property val As String

        Public Overrides Function ToString() As String
            Return val
        End Function

        Public Shared Widening Operator CType(str As String) As StringValue
            Return New StringValue With {
                .val = str
            }
        End Operator
    End Class

    Public Class ColorValue : Inherits StringValue

        <XmlAttribute> Public Property theme As String
        <XmlAttribute> Public Property rgb As String
        <XmlAttribute> Public Property indexed As String
        <XmlAttribute> Public Property lastClr As String

        Public Overrides Function ToString() As String
            Return rgb
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(color As Color) As ColorValue
            Return New ColorValue With {
                .rgb = color.ToHtmlColor
            }
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(color As String) As ColorValue
            Return color.TranslateColor
        End Operator
    End Class

    Public Class Flag
    End Class
End Namespace

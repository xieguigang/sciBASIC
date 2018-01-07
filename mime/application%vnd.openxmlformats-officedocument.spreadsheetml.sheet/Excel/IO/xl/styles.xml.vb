#Region "Microsoft.VisualBasic::c6d530ab5f8c4634918aae7ef92a50bf, ..\sciBASIC#\mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\IO\xl\styles.xml.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
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

Namespace XML.xl

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

        <XmlAttribute("Ignorable", [Namespace]:=Excel.Xmlns.mc)>
        Public Property Ignorable As String

        <XmlNamespaceDeclarations()>
        Public xmlns As XmlSerializerNamespaces

        Sub New()
            xmlns = New XmlSerializerNamespaces

            xmlns.Add("mc", Excel.Xmlns.mc)
            xmlns.Add("x14ac", Excel.Xmlns.x14ac)
            xmlns.Add("x16r2", Excel.Xmlns.x16r2)
        End Sub

    End Class

    Public Class List(Of T)
        <XmlAttribute> Public Property count As Integer
    End Class

    Public Class tableStyles : Inherits List(Of tableStyle)

    End Class

    Public Class tableStyle
        <XmlAttribute> Public Property defaultTableStyle As String
        <XmlAttribute> Public Property defaultPivotStyle As String
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
        <XmlAttribute("knownFonts", [Namespace]:=Excel.Xmlns.x14ac)>
        Public Property knownFonts As String
        <XmlElement>
        Public Property fonts As font()
    End Class

    Public Class font
        Public Property b As Bold
        Public Property sz As StringValue
        Public Property color As ColorValue
        Public Property name As StringValue
        Public Property family As StringValue
        Public Property scheme As StringValue
    End Class

    Public Class StringValue
        <XmlAttribute> Public Property val As String
    End Class

    Public Class ColorValue : Inherits StringValue
        <XmlAttribute> Public Property theme As String
        <XmlAttribute> Public Property rgb As String
        <XmlAttribute> Public Property indexed As String
        <XmlAttribute> Public Property lastClr As String
    End Class

    Public Class Bold
    End Class
End Namespace

#Region "Microsoft.VisualBasic::730672e552bd208222b944940fa8c937, gr\Microsoft.VisualBasic.Imaging\d3js\CSS.vb"

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

    '   Total Lines: 78
    '    Code Lines: 58 (74.36%)
    ' Comment Lines: 3 (3.85%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 17 (21.79%)
    '     File Size: 2.58 KB


    '     Class DirectedForceGraph
    ' 
    '         Properties: link, node, text
    ' 
    '         Function: ToString
    ' 
    '     Class Font
    ' 
    '         Properties: color, font, font_size
    ' 
    '         Function: ToString
    ' 
    '     Class CssValue
    ' 
    '         Properties: opacity, stroke, strokeOpacity, strokeWidth
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.MIME.Html

Namespace d3js.SVG.CSS

    ''' <summary>
    ''' Style generator for the value of <see cref="XmlMeta.CSS.style"/>
    ''' </summary>
    Public Class DirectedForceGraph

        Public Property node As CssValue
        Public Property link As CssValue
        Public Property text As Font

        Public Overrides Function ToString() As String
            Dim sb As New StringBuilder

            Call sb.AppendLine(".node {")
            Call sb.AppendLine(node.ToString)
            Call sb.AppendLine("}")
            Call sb.AppendLine(".link {")
            Call sb.AppendLine(link.ToString)
            Call sb.AppendLine("}")
            Call sb.AppendLine(".text {")
            Call sb.AppendLine(text.ToString)
            Call sb.AppendLine("}")

            Return sb.ToString
        End Function
    End Class

    Public Class Font

        Public Property font As String = FontFace.MicrosoftYaHei
        Public Property color As String = "gray"

        <Field("font-size")>
        Public Property font_size As Integer = 10

        Public Overrides Function ToString() As String
            Dim sb As New StringBuilder

            If Not String.IsNullOrEmpty(font) Then
                sb.AppendLine("font: " & font)
            End If
            If Not String.IsNullOrEmpty(color) Then
                sb.AppendLine("color: " & color)
            End If
            If font_size <> 0 Then
                sb.AppendLine("font-size: " & font_size)
            End If

            Return sb.ToString
        End Function
    End Class

    Public Class CssValue

        <DataFrameColumn> Public Property stroke As String
        <DataFrameColumn("stroke-width")> Public Property strokeWidth As String
        <DataFrameColumn("stroke-opacity")> Public Property strokeOpacity As String
        <DataFrameColumn> Public Property opacity As String

        Public Overrides Function ToString() As String
            Dim sb As New StringBuilder

            For Each prop In DataFrameColumnAttribute.LoadMapping(Of CssValue).Values
                Dim value As Object = prop.GetValue(Me)
                If Not value Is Nothing Then
                    Call sb.AppendLine("    " & $"{prop.Identity}: {Scripting.ToString(value)};")
                End If
            Next

            Return sb.ToString
        End Function
    End Class
End Namespace

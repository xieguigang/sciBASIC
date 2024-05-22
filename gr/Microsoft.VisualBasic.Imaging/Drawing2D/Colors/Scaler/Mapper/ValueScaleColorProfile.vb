#Region "Microsoft.VisualBasic::afd199c6c0bc89d4c399c59dbfc59e18, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Colors\Scaler\Mapper\ValueScaleColorProfile.vb"

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

    '   Total Lines: 52
    '    Code Lines: 39 (75.00%)
    ' Comment Lines: 3 (5.77%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (19.23%)
    '     File Size: 1.94 KB


    '     Class ValueScaleColorProfile
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetColor
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports std = System.Math

Namespace Drawing2D.Colors.Scaler

    ''' <summary>
    ''' scale(color_schema_term_name), apply for the heatmap plot
    ''' </summary>
    Public Class ValueScaleColorProfile : Inherits ColorProfile

        Dim valueRange As DoubleRange
        Dim indexRange As DoubleRange
        Dim logarithm%

        Public Sub New(data As IEnumerable(Of NamedValue(Of Double)), colorSchema$, level%, Optional logarithm% = 0)
            MyBase.New(colorSchema)

            With data.ToArray
                Dim minX As Double = Aggregate item In .AsEnumerable Into Min(item.Value)
                Dim maxX As Double = Aggregate item In .AsEnumerable Into Max(item.Value)

                If logarithm > 0 Then
                    valueRange = New Double() {std.Log(minX, logarithm), std.Log(maxX, logarithm)}
                Else
                    valueRange = New Double() {minX, maxX}
                End If

                indexRange = New Double() {0.0, .Length - 1}
                colors = Designer.CubicSpline(colors, n:=level)
            End With
        End Sub

        Public Overrides Function GetColor(item As NamedValue(Of Double)) As Color
            Dim termValue# = If(logarithm > 0, std.Log(item.Value, logarithm), item.Value)
            Dim index As Integer = valueRange.ScaleMapping(termValue, indexRange)
            Dim color As Color

            If index >= colors.Length - 1 Then
                color = colors.Last
            ElseIf index <= 0 Then
                color = colors.First
            Else
                color = colors(index)
            End If

            Return color
        End Function
    End Class
End Namespace

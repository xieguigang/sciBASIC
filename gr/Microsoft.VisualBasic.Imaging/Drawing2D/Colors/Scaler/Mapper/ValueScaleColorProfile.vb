#Region "Microsoft.VisualBasic::978d617ec7cffd492c83a45b2bcb2f16, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Colors\Scaler\Mapper\ValueScaleColorProfile.vb"

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

    '   Total Lines: 57
    '    Code Lines: 39 (68.42%)
    ' Comment Lines: 8 (14.04%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (17.54%)
    '     File Size: 2.11 KB


    '     Class ValueScaleColorProfile
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetColor
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports std = System.Math

Namespace Drawing2D.Colors.Scaler

    ''' <summary>
    ''' scale(color_schema_term_name), apply for the heatmap plot
    ''' </summary>
    Public Class ValueScaleColorProfile : Inherits ColorProfile

        ''' <summary>
        ''' data range of the real world sample value, example as temperature value for heatmap
        ''' </summary>
        Dim valueRange As DoubleRange
        ''' <summary>
        ''' data range for the color array
        ''' </summary>
        Dim indexRange As DoubleRange
        ''' <summary>
        ''' base value for the log function for scale of the input value.
        ''' </summary>
        Dim logarithm%

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="data">
        ''' the real world sample value, example as temperature value for heatmap
        ''' </param>
        ''' <param name="colorSchema">the color map name, usually be the <see cref="ScalerPalette"/> enum string value.</param>
        ''' <param name="level">color map depth levels</param>
        ''' <param name="logarithm">
        ''' base value for the log function for scale of the input value. zero or negative value mean no log function calls
        ''' </param>
        Public Sub New(data As IEnumerable(Of NamedValue(Of Double)), colorSchema$, level%, Optional logarithm% = 0)
            Call Me.New(values:=(From xi As NamedValue(Of Double) In data Select xi.Value), colorSchema, level, logarithm)
        End Sub

        Sub New(values As IEnumerable(Of Double), colorSchema$, level%, Optional logarithm% = 0)
            Call MyBase.New(colorSchema)

            With values.ToArray
                Dim minX As Double = Aggregate xi As Double In .AsEnumerable Into Min(xi)
                Dim maxX As Double = Aggregate xi As Double In .AsEnumerable Into Max(xi)

                If logarithm > 0 Then
                    valueRange = New Double() {std.Log(minX, logarithm), std.Log(maxX, logarithm)}
                Else
                    valueRange = New Double() {minX, maxX}
                End If

                indexRange = New Double() {0.0, .Length - 1}
                colors = Designer.CubicSpline(colors, n:=level)
            End With
        End Sub

        ''' <summary>
        ''' get color use the item value
        ''' </summary>
        ''' <param name="item"></param>
        ''' <returns>this function scale the <paramref name="item"/> its <see cref="NamedValue(Of Double).Value"/> to a color</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function GetColor(item As NamedValue(Of Double)) As Color
            Return GetColor(item.Value)
        End Function

        Public Overloads Function GetColor(val As Double) As Color
            Dim termValue# = If(logarithm > 0, std.Log(val, logarithm), val)
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

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
#Region "Microsoft.VisualBasic::7c9a57fa8355ed5fbb23b1eed50d02fa, Data_science\Mathematica\Math\Math\Distributions\Summary\ToString.vb"

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

    '   Total Lines: 185
    '    Code Lines: 110 (59.46%)
    ' Comment Lines: 47 (25.41%)
    '    - Xml Docs: 80.85%
    ' 
    '   Blank Lines: 28 (15.14%)
    '     File Size: 7.80 KB


    '     Module ToString
    ' 
    '         Function: Center, FmtNum, FmtPair, FmtPercent, ToDisplayText
    ' 
    '         Sub: Print, Row
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Globalization
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports std = System.Math

Namespace Distributions.Summary

    Public Module ToString

        ' ##### Terminal formatted display #####

        ''' <summary>
        ''' All of the numbers in the display view are formatted in invariant
        ''' culture, so the output text is always in english style on any
        ''' operating system locale.
        ''' </summary>
        ReadOnly Locale As CultureInfo = CultureInfo.InvariantCulture

        ''' <summary>
        ''' Print the formatted summary view of this <see cref="SampleDistribution"/>
        ''' to the terminal console output.
        ''' </summary>
        ''' <param name="title">An optional title text at the top of the view.</param>
        ''' <param name="output">
        ''' Optional target text writer (for example a log file writer).
        ''' By default the view is written to the console standard output.
        ''' </param>
        ''' 
        <Extension>
        Public Sub Print(sample As SampleDistribution,
                         Optional title As String = "Sample Distribution Summary",
                         Optional output As TextWriter = Nothing)

            Dim writer As TextWriter = If(output, Console.Out)
            Call writer.WriteLine(ToDisplayText(sample, title))
        End Sub

        ''' <summary>
        ''' Build a nicely formatted english summary text of this sample
        ''' distribution, suitable for the terminal display.
        ''' </summary>
        ''' <remarks>
        ''' The view text is pure ASCII characters and all of the numbers are
        ''' culture-invariant formatted, so it can be displayed correctly on
        ''' any terminal environment. NaN or infinite values will be shown
        ''' as ``N/A``.
        ''' </remarks>
        ''' 
        <Extension>
        Public Function ToDisplayText(sample As SampleDistribution, Optional title As String = "Sample Distribution Summary") As String
            Const LabelW As Integer = 20
            Const ValueW As Integer = 26
            Const ViewW As Integer = 49   ' = 1 + LabelW + 2 + ValueW

            Dim sb As New StringBuilder
            Dim border As New String("="c, ViewW)
            Dim sep As New String("-"c, ViewW)

            Call sb.AppendLine(border)
            Call sb.AppendLine(Center(If(title, "Sample Distribution Summary"), ViewW))
            Call sb.AppendLine(border)

            ' 1. basic statistics of the sample
            Call Row(sb, "Sample Size", sample.size.ToString("N0", Locale), LabelW, ValueW)
            Call Row(sb, "Sum", FmtNum(sample.sum), LabelW, ValueW)
            Call Row(sb, "Mean", FmtNum(sample.average), LabelW, ValueW)
            Call Row(sb, "Std Deviation", FmtNum(sample.stdErr), LabelW, ValueW)
            Call Row(sb, "Variance", FmtNum(sample.variance), LabelW, ValueW)
            Call Row(sb, "Coeff. of Variation", FmtPercent(sample.CV), LabelW, ValueW)
            Call Row(sb, "Minimum", FmtNum(sample.min), LabelW, ValueW)
            Call Row(sb, "Maximum", FmtNum(sample.max), LabelW, ValueW)
            Call Row(sb, "Range", FmtNum(sample.range), LabelW, ValueW)

            Call sb.AppendLine(sep)

            ' 2. center tendency
            Call Row(sb, "Median (P50)", FmtNum(sample.median), LabelW, ValueW)
            ' note: the default value of mode field is 0 for the empty sample
            Call Row(sb, "Mode", If(sample.size = 0, "N/A", FmtNum(sample.mode)), LabelW, ValueW)

            Call sb.AppendLine(sep)

            ' 3. quantiles
            If sample.quantile Is Nothing OrElse sample.quantile.Length < 5 Then
                Call Row(sb, "Quantiles", "N/A", LabelW, ValueW)
            Else
                Dim quantile = sample.quantile

                Call sb.AppendLine(" Quantiles:")
                Call Row(sb, "  [  0%] Min", FmtNum(quantile(0)), LabelW, ValueW)
                Call Row(sb, "  [ 25%] Q1", FmtNum(quantile(1)), LabelW, ValueW)
                Call Row(sb, "  [ 50%] Median", FmtNum(quantile(2)), LabelW, ValueW)
                Call Row(sb, "  [ 75%] Q3", FmtNum(quantile(3)), LabelW, ValueW)
                Call Row(sb, "  [100%] Max", FmtNum(quantile(4)), LabelW, ValueW)
            End If

            Call sb.AppendLine(sep)

            ' 4. mean confidence interval and the IQR outlier boundaries
            If sample.size = 0 Then
                Call Row(sb, "95% CI of Mean", "N/A", LabelW, ValueW)
            Else
                Call Row(sb, "95% CI of Mean", FmtPair(sample.CI95Range), LabelW, ValueW)
            End If

            Call Row(sb, "Outlier Boundary", FmtPair(sample.outlierBoundary), LabelW, ValueW)

            Call sb.AppendLine(border)

            Return sb.ToString.TrimEnd()
        End Function

        ''' <summary>
        ''' Append one aligned row: ``&lt;label> : &lt;value>``
        ''' </summary>
        Private Sub Row(sb As StringBuilder, label As String, value As String, labelW As Integer, valueW As Integer)
            Call sb.Append(" "c)
            Call sb.Append(label.PadRight(labelW))
            Call sb.Append(": ")
            Call sb.AppendLine(If(value, "N/A").PadLeft(valueW))
        End Sub

        Private Function Center(text As String, width As Integer) As String
            If text.Length >= width Then
                Return text
            Else
                Return New String(" "c, (width - text.Length) \ 2) & text
            End If
        End Function

        ''' <summary>
        ''' Format a two values interval as ``[a, b]``
        ''' </summary>
        Private Function FmtPair(interval As Double()) As String
            If interval Is Nothing OrElse interval.Length < 2 Then Return "N/A"
            If Double.IsNaN(interval(0)) OrElse Double.IsNaN(interval(1)) Then Return "N/A"
            Return "[" & FmtNum(interval(0)) & ", " & FmtNum(interval(1)) & "]"
        End Function

        ''' <summary>
        ''' Format a numeric value for display: ``N/A`` for NaN, scientific
        ''' notation for very large or very small magnitude, otherwise
        ''' fixed-point number with the thousands separators.
        ''' </summary>
        Private Function FmtNum(x As Double) As String
            If Double.IsNaN(x) Then
                Return "N/A"
            ElseIf Double.IsPositiveInfinity(x) Then
                Return "+Inf"
            ElseIf Double.IsNegativeInfinity(x) Then
                Return "-Inf"
            ElseIf x = 0 Then
                Return "0"
            Else
                Dim a As Double = std.Abs(x)

                If a >= 1.0E+15 OrElse a < 0.0001 Then
                    ' too large or too small for the fixed-point display
                    Return x.ToString("E4", Locale)
                Else
                    Return x.ToString("N4", Locale)
                End If
            End If
        End Function

        ''' <summary>
        ''' Format a ratio value (like CV) as the percentage text.
        ''' </summary>
        Private Function FmtPercent(ratio As Double) As String
            If Double.IsNaN(ratio) OrElse Double.IsInfinity(ratio) Then
                Return "N/A"
            Else
                Dim p As Double = ratio * 100.0

                If std.Abs(p) >= 1000000.0 Then
                    Return p.ToString("E3", Locale) & " %"
                Else
                    Return p.ToString("0.####", Locale) & " %"
                End If
            End If
        End Function

    End Module
End Namespace

#Region "Microsoft.VisualBasic::80718309bfaf05260017f46b9c68201c, ..\sciBASIC#\Data_science\Mathematica\Plot\Plots\g\Axis\AxisProvider.vb"

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

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Graphic.Axis

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' ```vbnet
    ''' Dim axis$ = "(min,max),tick=steps,n=parts"
    ''' 
    ''' ' example as
    ''' Dim axis$ = "(0,100),tick=10"
    ''' Dim axis$ = "(100,1000),n=10"
    ''' ```
    ''' </remarks>
    Public Class AxisProvider

        Public Property Range As DoubleRange
        Public Property n As Integer
        Public Property Tick As Double

        Sub New()
        End Sub

        Sub New(data#())
            Range = New DoubleRange(data.Min, data.Max)
            Tick = data(1) - data(0)
        End Sub

        Public Function AxisTicks() As Double()
            If n = 0 Then
                Return Range.GetAxisByTick(Tick)
            Else
                Return Range.GetAxisByTick(tick:=Range.Length / n)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return AxisTicks().GetJson
        End Function

        Public Shared Function TryParse(s$) As AxisProvider
            Dim range As String = Regex.Match(s, "\(.+?\)").Value
            Dim tick$ = Regex.Match(s, "tick=[^,]+", RegexICSng).Value
            Dim n$ = Regex.Match(s, "n=[^,]+", RegexICSng).Value

            Return New AxisProvider With {
                .n = n.GetTagValue("=", trim:=True).Value.ParseInteger,
                .Range = DoubleRange.op_Implicit(range),
                .Tick = tick.GetTagValue("=", trim:=True).Value.ParseNumeric
            }
        End Function

        Public Shared Widening Operator CType(expression$) As AxisProvider
            Return TryParse(expression)
        End Operator

        Public Shared Narrowing Operator CType(axis As AxisProvider) As Double()
            Return axis.AxisTicks
        End Operator
    End Class
End Namespace

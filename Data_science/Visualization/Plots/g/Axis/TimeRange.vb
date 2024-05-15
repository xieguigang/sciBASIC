#Region "Microsoft.VisualBasic::19e4e2db62875ce04243c900e161ee53, Data_science\Visualization\Plots\g\Axis\TimeRange.vb"

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

    '   Total Lines: 45
    '    Code Lines: 36
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 1.53 KB


    '     Class TimeRange
    ' 
    '         Properties: [From], [To], Ticks
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Scaler, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.ValueTypes

Namespace Graphic.Axis

    Public Class TimeRange

        Public ReadOnly Property [From] As Date
        Public ReadOnly Property [To] As Date
        Public ReadOnly Property Ticks As Date()

        ReadOnly timeRange As DoubleRange

        Sub New(from As Date, [to] As Date)
            Dim ticks#() = New Double() {
                from.UnixTimeStamp, [to].UnixTimeStamp
            }.CreateAxisTicks

            Me.From = CLng(ticks(0)).FromUnixTimeStamp
            Me.To = CLng(ticks.Last).FromUnixTimeStamp
            Me.Ticks = ticks _
                .Select(Function(d) CLng(d).FromUnixTimeStamp) _
                .ToArray
            Me.timeRange = ticks
        End Sub

        Public Function Scaler(range As DoubleRange) As Func(Of Date, Double)
            Return Function(d) As Double
                       Return timeRange.ScaleMapping(d.UnixTimeStamp, range)
                   End Function
        End Function

        Public Overrides Function ToString() As String
            Return $"[{From}, {[To]}]"
        End Function

        Public Overloads Shared Widening Operator CType(dateList As Date()) As TimeRange
            Dim order = dateList _
                .OrderBy(Self(Of Date)) _
                .ToArray
            Return New TimeRange(order(0), order.Last)
        End Operator
    End Class
End Namespace

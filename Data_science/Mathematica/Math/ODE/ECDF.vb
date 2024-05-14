#Region "Microsoft.VisualBasic::453e093b2ec0cc369e5e17f6f49ac3a3, Data_science\Mathematica\Math\ODE\ECDF.vb"

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

    '   Total Lines: 38
    '    Code Lines: 29
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 1.30 KB


    ' Class ECDF
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: eval, indexing
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports stdNum = System.Math

Public Class ECDF

    ReadOnly range As (range As DoubleRange, y As DoubleRange, sign As Integer)()

    Sub New(v As IEnumerable(Of Double), range As Integer())
        Me.range = indexing(v.ToArray, range.Select(Function(i) CDbl(i)).ToArray).ToArray
    End Sub

    Sub New(y As Double(), x As Double())
        Me.range = indexing(y, x).ToArray
    End Sub

    Private Shared Iterator Function indexing(y As Double(), x As Double()) As IEnumerable(Of (range As DoubleRange, y As DoubleRange, Integer))
        For i As Integer = 1 To x.Length - 1
            Dim xmin As Double = x(i - 1)
            Dim xmax As Double = x(i)
            Dim ymin As Double = y(i - 1)
            Dim ymax As Double = y(i)
            Dim sign As Integer = stdNum.Sign(ymax - ymin)

            Yield (New DoubleRange(xmin, xmax), New DoubleRange(ymin, ymax), sign)
        Next
    End Function

    Public Function eval(i As Double) As Double
        Dim n = range.Where(Function(r) r.range.IsInside(i)).First
        Dim p As Double = n.range.ScaleMapping(i, n.y)

        If n.sign < 0 Then
            p = n.y.Max - p
        End If

        Return p
    End Function
End Class

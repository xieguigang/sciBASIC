#Region "Microsoft.VisualBasic::665989b8f14294efc71d355d28fb42f4, Data_science\Mathematica\SignalProcessing\SignalProcessing\Filters\RamerDouglasPeuckerFilter.vb"

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

    '   Total Lines: 74
    '    Code Lines: 52
    ' Comment Lines: 12
    '   Blank Lines: 10
    '     File Size: 3.00 KB


    '     Class RamerDouglasPeuckerFilter
    ' 
    '         Properties: Epsilon
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: filter, ramerDouglasPeuckerFunction
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace Filters

    ''' <summary>
    ''' Filters data using Ramer-Douglas-Peucker algorithm with specified tolerance
    ''' <a href="http://en.wikipedia.org/wiki/Ramer-Douglas-Peucker_algorithm">Ramer-Douglas-Peucker algorithm</a>
    ''' 
    ''' @author Rzeźnik </summary>
    Public Class RamerDouglasPeuckerFilter
        Implements DataFilter

        Private epsilonField As Double

        ''' 
        ''' <param name="epsilon">
        '''            epsilon in Ramer-Douglas-Peucker algorithm (maximum distance
        '''            of a point in data between original curve and simplified
        '''            curve) </param>
        Public Sub New(epsilon As Double)
            If epsilon <= 0 Then
                Throw New ArgumentException("Epsilon nust be > 0")
            End If
            epsilonField = epsilon
        End Sub

        Public Overridable Function filter(data As Double()) As Double() Implements DataFilter.filter
            Return ramerDouglasPeuckerFunction(data, 0, data.Length - 1)
        End Function

        ''' 
        ''' <returns> {@code epsilon} </returns>
        Public Overridable Property Epsilon As Double
            Get
                Return epsilonField
            End Get
            Set(value As Double)
                If value <= 0 Then
                    Throw New ArgumentException("Epsilon nust be > 0")
                End If
                epsilonField = value
            End Set
        End Property

        Protected Friend Overridable Function ramerDouglasPeuckerFunction(points As Double(), startIndex As Integer, endIndex As Integer) As Double()
            Dim dmax As Double = 0
            Dim idx = 0
            Dim a As Double = endIndex - startIndex
            Dim b = points(endIndex) - points(startIndex)
            Dim c = -(b * startIndex - a * points(startIndex))
            Dim norm = std.Sqrt(a ^ 2 + b ^ 2)
            For i = startIndex + 1 To endIndex - 1
                Dim distance = std.Abs(b * i - a * points(i) + c) / norm
                If distance > dmax Then
                    idx = i
                    dmax = distance
                End If
            Next
            If dmax >= epsilonField Then
                Dim recursiveResult1 = ramerDouglasPeuckerFunction(points, startIndex, idx)
                Dim recursiveResult2 = ramerDouglasPeuckerFunction(points, idx, endIndex)
                Dim result = New Double(recursiveResult1.Length - 1 + recursiveResult2.Length - 1) {}
                Array.Copy(recursiveResult1, 0, result, 0, recursiveResult1.Length - 1)
                Array.Copy(recursiveResult2, 0, result, recursiveResult1.Length - 1, recursiveResult2.Length)
                Return result
            Else
                Return New Double() {points(startIndex), points(endIndex)}
            End If
        End Function


    End Class

End Namespace

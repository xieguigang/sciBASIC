#Region "Microsoft.VisualBasic::3cd829278b61636ed7b387469991f70c, Data_science\Graph\Model\Tree\KdTree\ApproximateNearNeighbor\VectorAccessor.vb"

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

    '   Total Lines: 55
    '    Code Lines: 41 (74.55%)
    ' Comment Lines: 4 (7.27%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (18.18%)
    '     File Size: 2.09 KB


    '     Class VectorAccessor
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: activate, getByDimension, GetDimensions, metric, nodeIs
    ' 
    '         Sub: setByDimensin
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Correlations.DistanceMethods

Namespace KdTree.ApproximateNearNeighbor

    Friend Class VectorAccessor : Inherits KdNodeAccessor(Of TagVector)

        Dim dims As Dictionary(Of String, Integer)
        Dim dimKeys As String()

        ''' <summary>
        ''' create an accessor for access the n-dimension vector
        ''' </summary>
        ''' <param name="m"></param>
        Sub New(m As Integer)
            dims = m _
                .Sequence _
                .ToDictionary(Function(k) k.ToString,
                              Function(k)
                                  Return k
                              End Function)
            dimKeys = dims.Keys.ToArray
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub setByDimension(x As TagVector, dimName As String, value As Double)
            x.vector(dims(dimName)) = value
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function GetDimensions() As String()
            Return dimKeys
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function metric(a As TagVector, b As TagVector) As Double
            Return a.vector.EuclideanDistance(b.vector)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function getByDimension(x As TagVector, dimName As String) As Double
            Return x.vector(dims(dimName))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function nodeIs(a As TagVector, b As TagVector) As Boolean
            Return a.index = b.index
        End Function

        Public Overrides Function activate() As TagVector
            Return New TagVector With {.vector = 0.0.Repeats(times:=dims.Count).ToArray, .index = -1}
        End Function
    End Class
End Namespace

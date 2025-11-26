#Region "Microsoft.VisualBasic::c3755fef8d9cea06934f8bddf5dd789c, Data_science\DataMining\UMAP\KNN\KDTree\KDAccessor.vb"

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

    '   Total Lines: 51
    '    Code Lines: 41 (80.39%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (19.61%)
    '     File Size: 1.92 KB


    '     Class KDAccessor
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: activate, getByDimension, GetDimensions, metric, nodeIs
    ' 
    '         Sub: setByDimension
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.GraphTheory.KdTree
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Correlations.DistanceMethods

Namespace KNN.KDTreeMethod

    Public Class KDAccessor : Inherits KdNodeAccessor(Of KDPoint)

        ReadOnly indexMaps As Dictionary(Of String, Integer)

        Sub New(dims As Integer)
            indexMaps = dims _
                .Sequence _
                .ToDictionary(Function(i) i.ToString,
                              Function(i)
                                  Return i
                              End Function)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub setByDimension(x As KDPoint, dimName As String, value As Double)
            x.vector(indexMaps(dimName)) = value
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function GetDimensions() As String()
            Return indexMaps.Keys.ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function metric(a As KDPoint, b As KDPoint) As Double
            Return a.vector.EuclideanDistance(b.vector)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function getByDimension(x As KDPoint, dimName As String) As Double
            Return x.vector(indexMaps(dimName))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function nodeIs(a As KDPoint, b As KDPoint) As Boolean
            Return a Is b
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function activate() As KDPoint
            Return New KDPoint
        End Function
    End Class
End Namespace

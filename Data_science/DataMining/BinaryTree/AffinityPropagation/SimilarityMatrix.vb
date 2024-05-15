#Region "Microsoft.VisualBasic::030afa7b4d4407f19f2639ed5dcd5dac, Data_science\DataMining\BinaryTree\AffinityPropagation\SimilarityMatrix.vb"

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

    '   Total Lines: 40
    '    Code Lines: 27
    ' Comment Lines: 7
    '   Blank Lines: 6
    '     File Size: 1.50 KB


    '     Module SimilarityMatrix
    ' 
    '         Function: (+2 Overloads) SparseSimilarityMatrix
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.Evaluation
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.Correlations.DistanceMethods

Namespace AffinityPropagation

    Public Module SimilarityMatrix

        ''' <summary>
        ''' Create the similarity matrix with a user defined distance measure
        ''' </summary>
        ''' <param name="ptr"></param>
        ''' <param name="distance"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function SparseSimilarityMatrix(ptr As ClusterEntity(), distance As IMetric) As Edge()
            Dim items = New Edge(ptr.Length * ptr.Length - 1) {}
            Dim p = 0

            For i As Integer = 0 To ptr.Length - 1 - 1
                For j As Integer = i + 1 To ptr.Length - 1
                    items(p) = New Edge(i, j, distance(ptr(i), ptr(j)))
                    items(p + 1) = New Edge(j, i, distance(ptr(i), ptr(j)))
                    p += 2
                Next
            Next

            Return items
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function SparseSimilarityMatrix(ptr As ClusterEntity()) As Edge()
            Return ptr.SparseSimilarityMatrix(distance:=Function(a, b) -a.EuclideanDistance(b))
        End Function
    End Module
End Namespace

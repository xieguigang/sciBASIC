#Region "Microsoft.VisualBasic::0b2ea9db96ab4ac68646060a9ec8ebd2, Data_science\DataMining\DataMining\Clustering\FuzzyCMeans\Algorithm\Utils.vb"

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

    '     Module AlgorithmsUtils
    ' 
    '         Function: DifferenceMatrix, MakeInitialSeeds
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports stdNum = System.Math

Namespace FuzzyCMeans

    Public Module AlgorithmsUtils

        ''' <summary>
        ''' </summary>
        ''' <param name="matrix1"></param>
        ''' <param name="matrix2"></param>
        ''' <returns></returns>
        <Extension>
        Public Function DifferenceMatrix(matrix1 As IEnumerable(Of List(Of Double)), matrix2 As List(Of Double)()) As List(Of List(Of Double))
            Dim diff As New List(Of List(Of Double))()
            Dim l% = matrix1.First.Count
            Dim line As List(Of Double)

            For Each row As SeqValue(Of List(Of Double)) In matrix1.SeqIterator
                Dim rowDifferences As New List(Of Double)()

                line = (+row)

                For j As Integer = 0 To l - 1
                    rowDifferences.Add(stdNum.Abs(line(j) - matrix2(row.i)(j)))
                Next

                diff.Add(rowDifferences)
            Next

            Return diff
        End Function

        Public Function MakeInitialSeeds(coordinates As List(Of FuzzyCMeansEntity), numberOfClusters As Integer) As List(Of FuzzyCMeansEntity)
            Dim coordinatesCopy As List(Of FuzzyCMeansEntity) = coordinates.AsList()
            Dim initialClusterCenters As New List(Of FuzzyCMeansEntity)()
            Dim random As Random = randf.seeds
            Dim clusterCenterPointNumber As Integer

            For i As Integer = 0 To numberOfClusters - 1
                clusterCenterPointNumber = random.[Next](0, coordinatesCopy.Count)
                initialClusterCenters.Add(coordinatesCopy(clusterCenterPointNumber))
                coordinatesCopy.RemoveAt(clusterCenterPointNumber)
            Next

            Return initialClusterCenters
        End Function
    End Module
End Namespace

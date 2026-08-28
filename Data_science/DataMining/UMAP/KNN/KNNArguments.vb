#Region "Microsoft.VisualBasic::bc6cf39e51b0a829daa8044b62847971, Data_science\DataMining\UMAP\KNN\KNNArguments.vb"

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

    '   Total Lines: 32
    '    Code Lines: 21 (65.62%)
    ' Comment Lines: 4 (12.50%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (21.88%)
    '     File Size: 888 B


    '     Structure KNNArguments
    ' 
    '         Properties: bandwidth, k, localConnectivity, nIter
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON
Imports std = System.Math

Namespace KNN

    ''' <summary>
    ''' the argument collection of the KNN search and the nearest neighbour 
    ''' descent procedure
    ''' </summary>
    Public Structure KNNArguments

        ''' <summary>
        ''' nNeighbors
        ''' </summary>
        ''' <returns></returns>
        Public Property k As Integer
        Public Property localConnectivity As Double
        ''' <summary>
        ''' the number of the iterations of the binary search of the sigma 
        ''' parameter in the smooth knn distance procedure
        ''' </summary>
        ''' <returns></returns>
        Public Property nIter As Integer
        Public Property bandwidth As Double

        ''' <summary>
        ''' the number of the random projection trees of the rp-forest.
        ''' </summary>
        ''' <returns>
        ''' a value that is less than or equals to zero means use the 
        ''' adaptive formula: ``5 + round(sqrt(n) / 20)``
        ''' </returns>
        Public Property nTrees As Integer
        ''' <summary>
        ''' the max size of the leaf node of the random projection tree.
        ''' </summary>
        ''' <returns>
        ''' a value that is less than or equals to zero means use the 
        ''' adaptive formula: ``max(10, k)``
        ''' </returns>
        Public Property leafSize As Integer
        ''' <summary>
        ''' the max number of the candidate neighbours of each vertex that 
        ''' is used by the nearest neighbour descent procedure
        ''' </summary>
        ''' <returns></returns>
        Public Property maxCandidates As Integer
        ''' <summary>
        ''' the early stop threshold of the nearest neighbour descent: the 
        ''' iteration will be stopped when the number of the updated 
        ''' neighbours is less than ``delta * k * n``
        ''' </summary>
        ''' <returns></returns>
        Public Property delta As Double
        ''' <summary>
        ''' the sample rate of the nearest neighbour descent: a lower value 
        ''' means a faster but more approximate neighbour graph.
        ''' </summary>
        ''' <returns></returns>
        Public Property rho As Double
        ''' <summary>
        ''' init the neighbour graph via the random projection tree forest?
        ''' </summary>
        ''' <returns></returns>
        Public Property rpTreeInit As Boolean
        ''' <summary>
        ''' the number of the iterations of the nearest neighbour descent 
        ''' procedure.
        ''' </summary>
        ''' <returns>
        ''' a value that is less than or equals to zero means use the 
        ''' adaptive formula: ``max(5, floor(round(log2(n))))``
        ''' </returns>
        Public Property nDescentIters As Integer
        ''' <summary>
        ''' the parallelism configuration of the KNN search procedure
        ''' </summary>
        ''' <returns></returns>
        Public Property parallelism As ParallelConfig

        Sub New(k As Integer,
                Optional localConnectivity As Double = 1,
                Optional nIter As Integer = 64,
                Optional bandwidth As Double = 1,
                Optional nTrees As Integer = 0,
                Optional leafSize As Integer = 0,
                Optional maxCandidates As Integer = 50,
                Optional delta As Double = 0.001F,
                Optional rho As Double = 0.5F,
                Optional rpTreeInit As Boolean = True,
                Optional nDescentIters As Integer = 0,
                Optional parallelism As ParallelConfig = Nothing)

            Me.k = k
            Me.localConnectivity = localConnectivity
            Me.nIter = nIter
            Me.bandwidth = bandwidth
            Me.nTrees = nTrees
            Me.leafSize = leafSize
            Me.maxCandidates = maxCandidates
            Me.delta = delta
            Me.rho = rho
            Me.rpTreeInit = rpTreeInit
            Me.nDescentIters = nDescentIters
            Me.parallelism = If(parallelism, ParallelConfig.Sequential)
        End Sub

        ''' <summary>
        ''' get the effective number of the random projection trees
        ''' </summary>
        ''' <param name="n"></param>
        ''' <returns></returns>
        Public Function GetNumOfTrees(n As Integer) As Integer
            If nTrees > 0 Then
                Return nTrees
            Else
                Return 5 + Round(std.Sqrt(n) / 20)
            End If
        End Function

        ''' <summary>
        ''' get the effective leaf size of the random projection tree
        ''' </summary>
        ''' <returns></returns>
        Public Function GetLeafSize() As Integer
            If leafSize > 0 Then
                Return leafSize
            Else
                Return std.Max(10, k)
            End If
        End Function

        ''' <summary>
        ''' get the effective number of the iterations of the nearest 
        ''' neighbour descent procedure
        ''' </summary>
        ''' <param name="n"></param>
        ''' <returns></returns>
        Public Function GetDescentIters(n As Integer) As Integer
            If nDescentIters > 0 Then
                Return nDescentIters
            Else
                Return std.Max(5, CInt(std.Floor(std.Round(std.Log(n, 2)))))
            End If
        End Function

        ''' <summary>
        ''' Handle python3 rounding down from 0.5 discrpancy
        ''' </summary>
        ''' <param name="n"></param>
        ''' <returns></returns>
        Private Shared Function Round(n As Double) As Integer
            If n = 0.5 Then
                Return 0
            Else
                Return std.Floor(std.Round(n))
            End If
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

    End Structure
End Namespace

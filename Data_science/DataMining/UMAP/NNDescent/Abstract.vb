#Region "Microsoft.VisualBasic::a36f76431f49fdbf60bf9c8886cbdd76, Data_science\DataMining\UMAP\NNDescent\Abstract.vb"

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

    '   Total Lines: 12
    '    Code Lines: 9 (75.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 3 (25.00%)
    '     File Size: 503 B


    ' Interface NNDescentFn
    ' 
    '     Function: NNDescent
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.UMAP.KNN

Public Interface NNDescentFn

    ''' <summary>
    ''' run the nearest neighbour descent for build the approximated knn graph
    ''' </summary>
    ''' <param name="data">the input data matrix</param>
    ''' <param name="leafArray">
    ''' the leaf nodes of the random projection forest, which is used for 
    ''' seed the initial state of the neighbour graph.
    ''' </param>
    ''' <param name="nNeighbors">the number of the nearest neighbours</param>
    ''' <param name="nIters">the max number of the descent iterations</param>
    ''' <param name="maxCandidates">
    ''' the max number of the candidate neighbours of each vertex
    ''' </param>
    ''' <param name="delta">
    ''' the early stop threshold of the descent iteration
    ''' </param>
    ''' <param name="rho">the sample rate of the descent procedure</param>
    ''' <param name="rpTreeInit">
    ''' init the neighbour graph via the random projection forest?
    ''' </param>
    ''' <param name="parallelism">the parallelism configuration</param>
    ''' <returns></returns>
    Function NNDescent(data As Double()(), leafArray As Integer()(), nNeighbors As Integer,
                       Optional nIters As Integer = 10,
                       Optional maxCandidates As Integer = 50,
                       Optional delta As Double = 0.001F,
                       Optional rho As Double = 0.5F,
                       Optional rpTreeInit As Boolean = True,
                       Optional parallelism As ParallelConfig = Nothing) As KNNState

End Interface

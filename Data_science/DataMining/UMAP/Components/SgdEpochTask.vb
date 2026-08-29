#Region "Microsoft.VisualBasic::bbba230b3e859f980fa0eda416c45e0a, Data_science\DataMining\UMAP\Components\SgdEpochTask.vb"

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

    '   Total Lines: 54
    '    Code Lines: 19 (35.19%)
    ' Comment Lines: 28 (51.85%)
    '    - Xml Docs: 82.14%
    ' 
    '   Blank Lines: 7 (12.96%)
    '     File Size: 2.15 KB


    ' Class SgdEpochTask
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Solve
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Parallel

''' <summary>
''' implements the SGD optimization loop of one epoch in parallel
''' </summary>
''' <remarks>
''' The UMAP optimization is a stochastic gradient descent on a shared 
''' embedding vector, two different edges may hit the same vertex at the 
''' same time. This class applies the Hogwild style parallel optimization 
''' (the same as the ``parallel`` option of the official python UMAP): the 
''' gradient of each edge is applied to the shared embedding vector without 
''' any lock.
''' 
''' As a consequence the embedding result is no longer bit-level reproducible 
''' between each run, but the convergence of the optimization is not affected 
''' at all.
''' </remarks>
Friend NotInheritable Class SgdEpochTask : Inherits VectorTask

    ReadOnly umap As Umap
    ReadOnly epoch As Integer
    ReadOnly clipValue As Double

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="workLen">
    ''' the number of the edges of the graph
    ''' </param>
    ''' <param name="umap"></param>
    ''' <param name="epoch">the index of the current epoch</param>
    ''' <param name="clipValue">the gradient clipping value</param>
    ''' <param name="workers">the number of the worker threads</param>
    Sub New(workLen As Integer, umap As Umap, epoch As Integer, clipValue As Double, Optional workers As Integer? = Nothing)
        MyBase.New(workLen, workers:=workers)

        Me.umap = umap
        Me.epoch = epoch
        Me.clipValue = clipValue
    End Sub

    Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
        ' each worker creates its own span view object, all of these views 
        ' are bound to the same shared embedding vector, so that the write 
        ' operation of one worker is visible to all of the other workers
        Dim embedding As Span(Of Double) = umap.GetEmbeddingSpan()

        For i As Integer = start To ends
            Call umap.RunIterate(i, epoch, clipValue, embedding)
        Next
    End Sub

End Class


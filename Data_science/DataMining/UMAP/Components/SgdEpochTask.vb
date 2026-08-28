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
        ' the Span(Of T) is a ref struct, so that it can not be shared 
        ' between the worker threads: each worker creates its own span 
        ' object which points to the shared embedding vector.
        Dim embedding As Span(Of Double) = umap.GetEmbeddingSpan()

        For i As Integer = start To ends
            Call umap.RunIterate(i, epoch, clipValue, embedding)
        Next
    End Sub

End Class

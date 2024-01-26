Namespace KdTree.ApproximateNearNeighbor

    ''' <summary>
    ''' k neighbors of a item row
    ''' </summary>
    Public Structure KNeighbors

        ''' <summary>
        ''' the score cutoff maybe applied, so the neighbors size may smaller than the given k
        ''' </summary>
        Dim size As Integer
        Dim indices As Integer()
        Dim weights As Double()

        Sub New(size As Integer, indices As Integer(), weights As Double())
            Me.size = size
            Me.indices = indices
            Me.weights = weights
        End Sub

    End Structure
End Namespace
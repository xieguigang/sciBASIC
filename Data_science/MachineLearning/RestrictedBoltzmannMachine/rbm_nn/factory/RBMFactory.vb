Namespace nn.rbm.factory

    ''' <summary>
    ''' Created by kenny on 5/16/14.
    ''' </summary>
    Public Interface RBMFactory
        Function build(numVisibleNodes As Integer, numHiddenNodes As Integer) As RBM
    End Interface

End Namespace

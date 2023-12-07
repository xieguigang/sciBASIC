Namespace nn.rbm.deep
    ''' <summary>
    ''' Created by kenny on 5/16/14.
    ''' </summary>
    Public Class LayerParameters

        Private numRBMSField As Integer = 1

        Private visibleUnitsPerRBMField As Integer = 1

        Private hiddenUnitsPerRBMField As Integer = 1

        Public ReadOnly Property NumRBMS As Integer
            Get
                Return numRBMSField
            End Get
        End Property

        Public Function setNumRBMS(numRBMS As Integer) As LayerParameters
            numRBMSField = numRBMS
            Return Me
        End Function

        Public ReadOnly Property VisibleUnitsPerRBM As Integer
            Get
                Return visibleUnitsPerRBMField
            End Get
        End Property

        Public Function setVisibleUnitsPerRBM(visibleUnitsPerRBM As Integer) As LayerParameters
            visibleUnitsPerRBMField = visibleUnitsPerRBM
            Return Me
        End Function

        Public ReadOnly Property HiddenUnitsPerRBM As Integer
            Get
                Return hiddenUnitsPerRBMField
            End Get
        End Property

        Public Function setHiddenUnitsPerRBM(hiddenUnitsPerRBM As Integer) As LayerParameters
            hiddenUnitsPerRBMField = hiddenUnitsPerRBM
            Return Me
        End Function
    End Class

End Namespace

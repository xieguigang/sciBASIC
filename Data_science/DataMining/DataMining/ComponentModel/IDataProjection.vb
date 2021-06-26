Namespace ComponentModel

    Public MustInherit Class IDataProjection

        Public MustOverride ReadOnly Property dimension As Integer

        ''' <summary>
        ''' get projection result
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride Function GetEmbedding() As Double()()

    End Class
End Namespace
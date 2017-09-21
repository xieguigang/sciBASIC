Namespace d3js

    Public Module ModuleAPI

        ''' <summary>
        ''' A D3 plug-in for automatic label placement using simulated annealing that easily 
        ''' incorporates into existing D3 code, with syntax mirroring other D3 layouts.
        ''' </summary>
        ''' <returns></returns>
        Public Function labeler() As Labeler
            Return New Labeler
        End Function
    End Module
End Namespace
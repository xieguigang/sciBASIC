Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace ComponentModel

    Public MustInherit Class TraceBackAlgorithm

        Protected traceback As TraceBackIterator

        Sub New()
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns>
        ''' this function is a safe function, will never returns the null value
        ''' </returns>
        Public Function GetTraceBack() As IEnumerable(Of NamedCollection(Of String))
            If traceback Is Nothing Then
                Return New NamedCollection(Of String)() {}
            Else
                Return traceback.GetTraceback
            End If
        End Function

    End Class
End Namespace
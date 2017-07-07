Namespace DATA

    Public Class DataFrame

        ''' <summary>
        ''' ```vbnet
        ''' Dim df As DataFrame
        ''' 
        ''' df.DATA(, {"field1"}) = ...
        ''' ```
        ''' </summary>
        ''' <param name="rows"></param>
        ''' <param name="fields"></param>
        ''' <returns></returns>
        Public Property DATA(Optional rows As IEnumerable(Of Integer) = Nothing, Optional fields As IEnumerable(Of String) = Nothing) As Object
            Get
                Throw New NotImplementedException
            End Get
            Set(value As Object)

            End Set
        End Property
    End Class
End Namespace
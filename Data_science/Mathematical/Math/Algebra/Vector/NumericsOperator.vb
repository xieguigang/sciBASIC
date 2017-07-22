''' <summary>
''' Numerics operator pool
''' </summary>
Public Class NumericsOperator

    Public ReadOnly Property unary As Dictionary(Of String, Func(Of Object, Object))
    Public ReadOnly Property binary As Dictionary(Of String, Func(Of Object, Object, Object))

    Public Shared Function GetOperators(type As Type) As NumericsOperator
        Select Case type
            Case GetType(Integer)
            Case GetType(Double)
            Case GetType(Single)
            Case GetType(Byte)
            Case GetType(Long)
            Case Else
                Throw New NotImplementedException
        End Select
    End Function
End Class

Imports System

Namespace LP

    Public Class InfeasibleException
        Inherits Exception

        Public Sub New(message As String)
            MyBase.New(message)
        End Sub
    End Class

    Public Class UnboundedException
        Inherits Exception

        Public Sub New(message As String)
            MyBase.New(message)
        End Sub
    End Class
End Namespace
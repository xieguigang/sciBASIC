Imports System

Namespace LP

    ''' <summary>
    ''' Created on 05/05/16.
    ''' </summary>
    Public Class UnboundedException
        Inherits Exception

        Public Sub New(ByVal message As String)
            MyBase.New(message)
        End Sub
    End Class

End Namespace
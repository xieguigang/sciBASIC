Imports System

Namespace Framework.Optimization.LBFGSB
    Public Class LBFGSBException
        Inherits Exception

        Private Const serialVersionUID As Long = 1L

        Public Sub New(message As String)
            MyBase.New(message)
        End Sub
        Public Sub New()
            MyBase.New()
        End Sub

    End Class

End Namespace

Imports System.Collections.Generic
Imports System.Text

Namespace YAML.Syntax
    Public Class Scalar
        Inherits DataItem
        Public Text As String


        Public Sub New()
            Me.Text = [String].Empty
        End Sub

        Public Overrides Function ToString() As String
            Return Text
        End Function
    End Class
End Namespace

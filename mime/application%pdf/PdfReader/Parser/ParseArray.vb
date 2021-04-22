Imports System.Collections.Generic

Namespace PdfReader
    Public Class ParseArray
        Inherits ParseObjectBase

        Public Sub New(ByVal objects As List(Of ParseObjectBase))
            Me.Objects = objects
        End Sub

        Public Property Objects As List(Of ParseObjectBase)
    End Class
End Namespace

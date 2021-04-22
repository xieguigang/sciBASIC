Imports System

Namespace PdfReader
    Public Class ParseResolveEventArgs
        Inherits EventArgs

        Public Property Id As Integer
        Public Property Gen As Integer
        Public Property [Object] As ParseObjectBase
    End Class
End Namespace

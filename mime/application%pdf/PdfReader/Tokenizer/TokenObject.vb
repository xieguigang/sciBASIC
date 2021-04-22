Namespace PdfReader
    Public MustInherit Class TokenObject
        Public Shared ReadOnly ArrayOpen As TokenArrayOpen = New TokenArrayOpen()
        Public Shared ReadOnly ArrayClose As TokenArrayClose = New TokenArrayClose()
        Public Shared ReadOnly DictionaryOpen As TokenDictionaryOpen = New TokenDictionaryOpen()
        Public Shared ReadOnly DictionaryClose As TokenDictionaryClose = New TokenDictionaryClose()
        Public Shared ReadOnly Empty As TokenEmpty = New TokenEmpty()

        Public Sub New()
        End Sub
    End Class

    Public Class TokenArrayOpen
        Inherits TokenObject

        Public Sub New()
        End Sub
    End Class

    Public Class TokenArrayClose
        Inherits TokenObject

        Public Sub New()
        End Sub
    End Class

    Public Class TokenDictionaryOpen
        Inherits TokenObject

        Public Sub New()
        End Sub
    End Class

    Public Class TokenDictionaryClose
        Inherits TokenObject

        Public Sub New()
        End Sub
    End Class

    Public Class TokenEmpty
        Inherits TokenObject

        Public Sub New()
        End Sub
    End Class
End Namespace

Namespace VBProj

    Public Class VBDocument

        ''' <summary>
        ''' relative path to the vbproj file
        ''' </summary>
        ''' <returns></returns>
        Public Property FileName As String
        ''' <summary>
        ''' namespace imports list
        ''' </summary>
        ''' <returns></returns>
        Public Property [Imports] As String()
        ''' <summary>
        ''' language symbols that parsed from current vb.net source file document text
        ''' </summary>
        ''' <returns></returns>
        Public Property Types As Dictionary(Of String, LanguageSymbolType)

    End Class

    Public Class [Imports]

        ''' <summary>
        ''' Imports XXX
        ''' </summary>
        ''' <returns></returns>
        Public Property [Namespace] As String
        ''' <summary>
        ''' Imports X = XXX
        ''' </summary>
        ''' <returns></returns>
        Public Property [Alias] As String

        Public Overrides Function ToString() As String
            If [Alias].StringEmpty(, True) Then
                Return $"Imports {[Namespace]}"
            Else
                Return $"Imports {[Alias]} = {[Namespace]}"
            End If
        End Function

    End Class

End Namespace
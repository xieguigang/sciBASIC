Namespace Models

    ''' <summary>
    ''' A source file referenced by the debug symbols (the ``*.vb`` / ``*.cs`` / ``*.cpp``
    ''' source that was compiled into the binary).
    ''' </summary>
    Public Class SourceDocument

        ''' <summary>
        ''' Absolute (or sometimes relative) path of the source file as stored inside the PDB.
        ''' </summary>
        Public Property FilePath As String

        ''' <summary>
        ''' Optional remapped URL of the source file on a Git host (e.g. GitHub), filled by
        ''' <see cref="Extensions.PointLocal2Github"/>. Empty until remapped.
        ''' </summary>
        Public Property GitHubUrl As String

        ''' <summary>
        ''' Language of the source file. For the classic PDB this is usually empty; for the
        ''' Portable PDB this is the language GUID (C# / VB / F# / ...) from the Document table.
        ''' </summary>
        Public Property Language As Guid

        ''' <summary>
        ''' Hash algorithm GUID used to compute <see cref="Checksum"/>. Empty when unknown.
        ''' </summary>
        Public Property HashAlgorithm As Guid

        ''' <summary>
        ''' Source file content checksum, may be empty when not embedded.
        ''' </summary>
        Public Property Checksum As Byte()

        ''' <summary>
        ''' Human readable language name derived from <see cref="Language"/>.
        ''' </summary>
        Public ReadOnly Property LanguageName As String
            Get
                If Language = LanguageGuids.CSharp Then
                    Return "C#"
                ElseIf Language = LanguageGuids.VisualBasic Then
                    Return "Visual Basic"
                ElseIf Language = LanguageGuids.FSharp Then
                    Return "F#"
                ElseIf Language = LanguageGuids.Cpp Then
                    Return "C++"
                ElseIf Language = Guid.Empty Then
                    Return "Unknown"
                Else
                    Return Language.ToString()
                End If
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return FilePath
        End Function
    End Class
End Namespace
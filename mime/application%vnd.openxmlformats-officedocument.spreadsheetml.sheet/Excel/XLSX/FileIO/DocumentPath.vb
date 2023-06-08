Namespace XLSX.FileIO

    ''' <summary>
    ''' Class to manage XML document paths
    ''' </summary>
    Friend Class DocumentPath

        ''' <summary>
        ''' Gets or sets the Filename
        ''' File name of the document
        ''' </summary>
        Public Property Filename As String

        ''' <summary>
        ''' Gets or sets the Path
        ''' Path of the document
        ''' </summary>
        Public Property Path As String

        ''' <summary>
        ''' Initializes a new instance of the <see cref="DocumentPath"/> class
        ''' </summary>
        Public Sub New()
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="DocumentPath"/> class
        ''' </summary>
        ''' <param name="filename">File name of the document.</param>
        ''' <param name="path">Path of the document.</param>
        Public Sub New(filename As String, path As String)
            Me.Filename = filename
            Me.Path = path
        End Sub

        ''' <summary>
        ''' Method to return the full path of the document
        ''' </summary>
        ''' <returns>Full path.</returns>
        Public Function GetFullPath() As String
            Dim lastCharOfPath As Char = Path.Last

            If lastCharOfPath = System.IO.Path.AltDirectorySeparatorChar OrElse lastCharOfPath = System.IO.Path.DirectorySeparatorChar Then
                Return System.IO.Path.AltDirectorySeparatorChar.ToString() & Path & Filename
            Else
                Return System.IO.Path.AltDirectorySeparatorChar.ToString() & Path & System.IO.Path.AltDirectorySeparatorChar.ToString() & Filename
            End If
        End Function
    End Class
End Namespace
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace FileIO

    ''' <summary>
    ''' File includes search tools
    ''' </summary>
    Public Class Includes

        Dim __innerDIRs As List(Of String)

        Sub New(ParamArray DIR As String())
            __innerDIRs = New List(Of String)(DIR)
        End Sub

        ''' <summary>
        ''' Add includes directory into the search path.
        ''' </summary>
        ''' <param name="DIR"></param>
        Public Sub Add(DIR As String)
            __innerDIRs += DIR
        End Sub

        ''' <summary>
        ''' Get the absolutely file path from the includes file's relative path.
        ''' </summary>
        ''' <param name="relPath"></param>
        ''' <returns></returns>
        Public Function GetPath(relPath As String) As String
            For Each DIR As String In __innerDIRs
                Dim path As String = DIR & "/" & relPath
                path = FileIO.FileSystem.GetFileInfo(path).FullName
                If path.FileExists Then
                    Return path
                End If
            Next

            Return Nothing
        End Function

        Public Overrides Function ToString() As String
            Return "Searchs in directories: " & __innerDIRs.GetJson
        End Function
    End Class
End Namespace
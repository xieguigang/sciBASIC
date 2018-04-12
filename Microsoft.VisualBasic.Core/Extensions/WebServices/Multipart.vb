Imports System.IO

Namespace Net.Http

    Public Class MultipartForm

        Dim buffer As New MemoryStream

        Const Boundary$ = "------WebKitFormBoundaryBpijhG6dKsQpCMdN--"

        Public Sub Add(name$, value$)
            Using writer As New StreamWriter(buffer) With {.NewLine = vbCrLf}
                Call writer.WriteLine(Boundary)

            End Using
        End Sub


    End Class
End Namespace
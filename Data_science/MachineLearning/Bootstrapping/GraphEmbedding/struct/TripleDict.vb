Imports System.IO
Imports System.Text

Namespace GraphEmbedding.struct

    Public Class TripleDict

        Public pTripleDict As Dictionary(Of String, Boolean) = Nothing

        Public Sub New()
        End Sub

        Public Overridable Function tripleDict() As Dictionary(Of String, Boolean)
            Return pTripleDict
        End Function

        Public Overridable Sub load(fnInput As String)
            pTripleDict = New Dictionary(Of String, Boolean)()
            Dim reader As StreamReader = New StreamReader(New FileStream(fnInput, FileMode.Open, FileAccess.Read), Encoding.UTF8)

            Dim line = ""
            While Not String.ReferenceEquals((CSharpImpl.__Assign(line, reader.ReadLine())), Nothing)
                pTripleDict(line.Trim()) = True
            End While
            reader.Close()
        End Sub

        Private Class CSharpImpl
            <Obsolete("Please refactor calling code to use normal Visual Basic assignment")>
            Shared Function __Assign(Of T)(ByRef target As T, value As T) As T
                target = value
                Return value
            End Function
        End Class
    End Class

End Namespace

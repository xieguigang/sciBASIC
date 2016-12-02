Imports Microsoft.VisualBasic.Serialization.JSON

Public Class config

    Public Property models As List(Of String)
    Public Property references As List(Of String)

    Public Shared ReadOnly Property DefaultFile As String = App.LocalData & "/config.json"

    Public Shared Function Load() As config
        If DefaultFile.FileExists Then
            Try
                Return DefaultFile.ReadAllText.LoadObject(Of config)
            Catch ex As Exception
                GoTo NEW_CONFIG
            End Try
        Else
NEW_CONFIG:
            Dim [new] As New config With {
                  .models = New List(Of String),
                  .references = New List(Of String)
              }
            Call [new].GetJson.SaveTo(DefaultFile)
            Return [new]
        End If
    End Function

    Public Sub Save()
        Call GetJson.SaveTo(DefaultFile)
    End Sub
End Class

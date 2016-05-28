Imports Microsoft.VisualBasic.Serialization

Namespace Debugging

    Public Class Config

        Public Property level As DebuggerLevels = DebuggerLevels.On
        Public Property mute As Boolean = False

        Public Shared ReadOnly Property DefaultFile As String =
            App.ProductProgramData & "/debugger-config.json"

        Public Shared Function Load() As Config
            Try
                Dim cfg As Config =
                    IO.File.ReadAllText(DefaultFile).LoadObject(Of Config)

                If cfg Is Nothing Then
                    Return New Config().Save()
                Else
                    Return cfg
                End If
            Catch ex As Exception
                Return New Config().Save()
            End Try
        End Function

        Private Function Save() As Config
            Call Me.GetJson.SaveTo(DefaultFile)
            Return Me
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace



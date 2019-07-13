#Region "Microsoft.VisualBasic::e4116dfe0b06870e2dcf53a489a4fb5e, Microsoft.VisualBasic.Core\ApplicationServices\Debugger\Config.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class Config
    ' 
    '         Properties: DefaultFile, level, mute
    ' 
    '         Function: Load, Save, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ApplicationServices.Debugging

    Public Class Config

        Public Property level As DebuggerLevels = DebuggerLevels.On
        Public Property mute As Boolean = False

        Public Shared ReadOnly Property DefaultFile As String =
            App.LocalData & "/debugger-config.json"

        Public Shared Function Load() As Config
            Try
                Dim cfg As Config =
                    IO.File.ReadAllText(DefaultFile).LoadJSON(Of Config)

                If cfg Is Nothing Then
                    Return New Config().Save()
                Else
                    Return cfg
                End If
            Catch ex As Exception When Not DefaultFile.FileExists
                Return New Config().Save()
            Catch ex As Exception
                Call App.LogException(ex)

                ' 假若是多进程的并行计算任务，则可能会出现默认配置文件被占用的情况，
                ' 这个也无能为力了， 只能够放弃读取， 直接使用默认的调试器参数
                Return New Config
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

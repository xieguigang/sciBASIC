#Region "Microsoft.VisualBasic::6e17b548bcfaada27b0f6ae0fa1ba1e9, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ApplicationServices\Debugger\Config.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.

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
                    IO.File.ReadAllText(DefaultFile).LoadObject(Of Config)

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

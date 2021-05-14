#Region "Microsoft.VisualBasic::7b74aa6a7d2fafcd821ef5ab07425c46, Microsoft.VisualBasic.Core\src\My\Framework\Config.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:

    '     Class Config
    ' 
    '         Properties: DefaultFile, environment, level, mute, updates
    ' 
    '         Function: Load, Save, saveDefault, ToString
    ' 
    '         Sub: fetchConfig
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
#If netcore5 = 1 Then
Imports Microsoft.VisualBasic.ApplicationServices.Development.NetCore5
#End If
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace My.FrameworkInternal

    Public Class Config

        Public Property level As DebuggerLevels = DebuggerLevels.On
        Public Property mute As Boolean = False

        ''' <summary>
        ''' 整个框架的环境配置项目，请注意，通过命令行进行配置是优先于这个配置文件的配置值的
        ''' 可以在命令行中使用``--config-framework``命令来配置默认数据
        ''' </summary>
        ''' <returns></returns>
        Public Property environment As Dictionary(Of String, String)

        Public Property updates As Date
            Get
                Return Now
            End Get
            Set(value As Date)
                ' readonly do nothing
            End Set
        End Property

        Public Shared ReadOnly Property DefaultFile As String
            Get
                Return App.LocalData & "/runtime-framework-config.json"
            End Get
        End Property

        Public Shared Function Load() As Config
            If Not DefaultFile.FileExists Then
                Return saveDefault()
            End If

            Try
                Dim cfg As Config = IO.File.ReadAllText(DefaultFile).LoadJSON(Of Config)

                If cfg Is Nothing Then
                    Return saveDefault()
                Else
                    Return cfg
                End If
            Catch ex As Exception When Not DefaultFile.FileExists
                Return saveDefault()
            Catch ex As Exception
                Call App.LogException(ex)

                ' 假若是多进程的并行计算任务，则可能会出现默认配置文件被占用的情况，
                ' 这个也无能为力了， 只能够放弃读取， 直接使用默认的调试器参数
                Return New Config
            End Try
        End Function

        Private Shared Function saveDefault() As Config
            Dim config As New Config With {
                .environment = New Dictionary(Of String, String)
            }
            ' scan all assembly and then load config name?
            ' only load Microsoft.VisualBasic
            Dim modules = App.HOME _
                .EnumerateFiles("*.dll") _
                .Where(Function(file)
                           Return file.BaseName.StartsWith("Microsoft.VisualBasic")
                       End Function) _
                .ToArray

            For Each file As String In modules
#If UNIX Then
                Try
                    Call fetchConfig(config, file)
                Catch ex As Exception
                    ' ignores the bugs in environment
                    ' mono on linux
                    ' Could not load file or assembly 'PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' or one of its dependencies.
                    Call ex.Message.Print
                    Call file.Warning
                End Try
#Else
                Call fetchConfig(config, file)
#End If
            Next

            Return config.Save
        End Function

        Private Shared Sub fetchConfig(config As Config, file$)
            Dim assembly As Assembly = Assembly.LoadFile(file)

#If netcore5 = 1 Then
            Call deps.TryHandleNetCore5AssemblyBugs(package:=assembly)
#End If

            Dim configNames As FrameworkConfigAttribute() = assembly.GetTypes _
                .Select(Function(type)
                            Return type.GetCustomAttributes(Of FrameworkConfigAttribute)
                        End Function) _
                .IteratesALL _
                .ToArray

            For Each configName As FrameworkConfigAttribute In configNames
                config.environment(configName.Name) = ""
            Next
        End Sub

        Private Function Save() As Config
            Call Me _
                .GetJson(indent:=True) _
                .SaveTo(DefaultFile)
            Return Me
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace

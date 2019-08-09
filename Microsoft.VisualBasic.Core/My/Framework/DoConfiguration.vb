Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports CLI = Microsoft.VisualBasic.CommandLine.CommandLine

Namespace My.FrameworkInternal

    ''' <summary>
    ''' Do configuration
    ''' </summary>
    Module DoConfiguration

        <Extension>
        Friend Sub ConfigFrameworkRuntime(configuration As Config, args As CLI)
            Dim envir As Dictionary(Of String, String) = args.EnvironmentVariables
            Dim disableLoadOptions As Boolean = args.GetBoolean("--load_options.disable")
            Dim name$

            ' load config from config file.
            For Each config In configuration.environment.SafeQuery
                Call App.JoinVariable(config.Key, config.Value)
            Next

            ' --load_options.disable 开关将会禁止所有的环境项目的设置
            ' 但是环境变量任然会进行加载设置
            If Not disableLoadOptions AndAlso Not envir.IsNullOrEmpty Then
                If envir.ContainsKey("Proxy") Then
                    WebServiceUtils.Proxy = envir("Proxy")
                    Call $"[Config] webUtils_proxy={WebServiceUtils.Proxy}".__INFO_ECHO
                End If
                If envir.ContainsKey("setwd") Then
                    App.CurrentDirectory = envir("setwd")
                    Call $"[Config] current_work_directory={App.CurrentDirectory}".__INFO_ECHO
                End If
                If envir.ContainsKey("buffer_size") Then
                    Call App.SetBufferSize(envir!buffer_size)
                End If
            End If

            ' config value from commandline will overrides the config value that loaded 
            ' from the config json file.

            ' /@var=name "value"
            For Each var As NamedValue(Of String) In args.ParameterList
                With var
                    If InStr(.Name, "/@var=", CompareMethod.Text) = 1 Then
                        name = .Name _
                               .GetTagValue("=") _
                               .Value

                        Call App.JoinVariable(name, .Value)
                    End If
                End With
            Next
        End Sub
    End Module
End Namespace
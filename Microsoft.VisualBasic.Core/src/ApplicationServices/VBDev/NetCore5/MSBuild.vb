Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.Linq

Namespace ApplicationServices.Development.NetCore5

    ''' <summary>
    ''' .NET core msbuild command wrapper
    ''' 
    ''' ```
    ''' dotnet msbuild
    ''' ```
    ''' </summary>
    Public Class MSBuild

        ''' <summary>
        ''' get dotnet version value
        ''' </summary>
        ''' <returns>
        ''' nothing means dotnet msbuild is not installed
        ''' </returns>
        Public Shared ReadOnly Property version As Version
            Get
                Static ver As Version = getVersion()
                Return ver
            End Get
        End Property

        Private Shared Function getVersion() As Version
            Dim verStr As String = MSBuild.dotnetShell("msbuild --version", False) _
                    .DoCall(AddressOf Strings.Trim) _
                    .LineTokens _
                    .LastOrDefault

            If verStr.StringEmpty Then
                Return Nothing
            Else
                Dim ver As Version = Version.Parse(verStr)
                Return ver
            End If
        End Function

        ''' <summary>
        ''' shell dotnet commandline and then returns the standard output of the dotnet command.
        ''' </summary>
        ''' <param name="arguments"></param>
        ''' <returns></returns>
        Private Shared Function dotnetShell(arguments As String, split As Boolean) As String
            Return arguments _
                .LineTokens _
                .Select(AddressOf Strings.Trim) _
                .JoinBy(" ") _
                .DoCall(Function(argv)
                            Return PipelineProcess.Call("dotnet", argv, debug:=split)
                        End Function)
        End Function

        Public Shared Function BuildVsSolution(sln As String,
                                               Optional vsConfig As String = "Release|x64",
                                               Optional rebuild As Boolean = True) As String

            Dim configTokens As String() = vsConfig.Split("|"c)
            Dim arguments As String = $"msbuild 
                ""{sln}"" 
                {If(rebuild, "-t:Rebuild", "")} 
                /p:Configuration=""{configTokens(0)}"" 
                /p:Platform=""{configTokens(1)}"" 
                -detailedSummary:True 
                -verbosity:minimal
            "

            Return dotnetShell(arguments, True)
        End Function

    End Class
End Namespace
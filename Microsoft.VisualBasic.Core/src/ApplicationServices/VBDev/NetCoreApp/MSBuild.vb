#Region "Microsoft.VisualBasic::f416ffc084a36a78ed0349447b314559, Microsoft.VisualBasic.Core\src\ApplicationServices\VBDev\NetCoreApp\MSBuild.vb"

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


    ' Code Statistics:

    '   Total Lines: 75
    '    Code Lines: 47
    ' Comment Lines: 18
    '   Blank Lines: 10
    '     File Size: 2.60 KB


    '     Class MSBuild
    ' 
    '         Properties: version
    ' 
    '         Function: BuildVsSolution, dotnetShell, getVersion
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.Linq

Namespace ApplicationServices.Development.NetCoreApp

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

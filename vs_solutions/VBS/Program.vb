#Region "Microsoft.VisualBasic::93e3a4e721bf00b4bf827974187cd2a3, vs_solutions\VBS\Program.vb"

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

    '   Total Lines: 38
    '    Code Lines: 25 (65.79%)
    ' Comment Lines: 6 (15.79%)
    '    - Xml Docs: 83.33%
    ' 
    '   Blank Lines: 7 (18.42%)
    '     File Size: 1.54 KB


    ' Module Program
    ' 
    '     Function: Main
    ' 
    '     Sub: PrintUsage
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine
Imports VBScriptHost.Script

Module Program

    ''' <summary>
    ''' vbs ./script.vb --arg1=xxx --arg2=xxx
    ''' vbs make-project ./script.vb [--verbose] [--no-build] [--force]
    ''' </summary>
    ''' <param name="args"></param>
    Public Function Main(args As String()) As Integer
        If args.IsNullOrEmpty Then
            Call PrintUsage()

            Return 0
        End If

        ' 子命令分发: make-project 之外的第一个词元一律视为脚本文件路径(保持原有行为不变)
        If String.Equals(args(0), "make-project", StringComparison.OrdinalIgnoreCase) Then
            Return MakeProject.Run(args)
        End If

        Dim cmdl As CommandLine = CommandLine.BuildFromArguments(args, NoSubCommand:=False)
        Dim scriptFile As String = args(0)
        Dim verbose As Boolean = cmdl("--verbose")
        Dim vectorize As Boolean = Not cmdl("--no-vectorize")
        Dim vbs As ScriptParseResult = VBScript.ParseScript(scriptFile, verbose:=verbose, vectorize:=vectorize)

        Using script As ScriptRuntime = vbs.CompileScript(debug:=verbose)
            Return script.Run(args)
        End Using
    End Function

    Private Sub PrintUsage()
        Call Console.WriteLine("vbs </path/to/script.vb> [--arg1=val1 --arg2=val2 ...] [--verbose] [--no-vectorize]")
        Call Console.WriteLine("vbs make-project </path/to/script.vb> [--verbose] [--no-build] [--force] [--no-vectorize]")
    End Sub
End Module


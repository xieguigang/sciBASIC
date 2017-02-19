#Region "Microsoft.VisualBasic::15888fd702100b29303dca213c2e19ed, ..\sciBASIC#\docs\guides\Example\CLI_Example\Program.vb"

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

Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Serialization.JSON

Module Program

    Public Function Main() As Integer
        Return GetType(CLI).RunCLI(App.CommandLine)
    End Function
End Module

Module CLI

    <ExportAPI("/API1",
               Info:="Puts the brief description of this API command at here.",
               Usage:="/API1 /msg ""Puts the CLI usage syntax at here""",
               Example:="/API1 /msg ""Hello world!!!""")>
    Public Function API1(args As CommandLine) As Integer
        Call Console.WriteLine(args("/msg"))
        Return 0
    End Function

    <ExportAPI("/Test.CLI_Scripting", Usage:="/Test.CLI_Scripting </var> <value> /@set <var=value>;<var=value>")>
    Public Function ScriptingTest(args As CommandLine) As Integer
        For Each var$ In args.Keys
            Call $"{var} --> {args(var)}".__DEBUG_ECHO
        Next

        Call "".__DEBUG_ECHO
        Call args.GetJson(True).__DEBUG_ECHO

        Return 0
    End Function
End Module

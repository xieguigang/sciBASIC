#Region "Microsoft.VisualBasic::c26f70a30569396ad107756a71f9af19, ..\sciBASIC#\docs\guides\Example\CLI_Example\Program.vb"

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

Imports System.ComponentModel
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService.SharedORM
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Serialization.JSON

Module Program

    Public Function Main() As Integer
        Return GetType(CLI).RunCLI(App.CommandLine)
    End Function
End Module

<[Namespace]("testName", Description:="Test code comments...")>
Module CLI

    <ExportAPI("/API1",
               Info:="Puts the brief description of this API command at here.",
               Usage:="/API1 /msg ""Puts the CLI usage syntax at here"" [/msg2 <test default value, default=2333 yes or not?>]",
               Example:="/API1 /msg ""Hello world!!!"" /msg2 ""test default value, default=2333 yes "" or not?""")>
    <Argument("/msg", False, CLITypes.String,
              AcceptTypes:={GetType(String)},
              Description:="The output message text.",
              Out:=False)>
    <Group(CLIGrouping.TestGroup1)>
    Public Function API1(args As CommandLine) As Integer
        Call Console.WriteLine(args("/msg"))
        Return 0
    End Function

    <ExportAPI("/Test.CLI_Scripting",
               Usage:="/Test.CLI_Scripting /var <value> [/@set <var=value>;<var=value>]")>
    <Group(CLIGrouping.TestGroup2)>
    Public Function ScriptingTest(args As CommandLine) As Integer
        For Each var$ In args.Keys
            Call $"{var} --> {args(var)}".__DEBUG_ECHO
        Next

        Call "".__DEBUG_ECHO
        Call args.GetJson(True).__DEBUG_ECHO

        Return 0
    End Function

    <ExportAPI("/Code.vb",
               Usage:="/Code.vb /namespace <vb_class.namespace> [/booleanTest /boolean2.test /out <test output file, default=code.vb>]")>
    Public Function vbCode(args As CommandLine) As Integer
        Dim vb As New VisualBasic(GetType(CLI), args <= "/namespace")
        Dim out = args.GetValue("/out", "./code.vb")
        Return vb.GetSourceCode _
            .SaveTo(out) _
            .CLICode
    End Function

    <ExportAPI("/DEP.heatmap")>
    <Description("Generates the heatmap plot input data. The default label profile is using for the iTraq result.")>
    <Usage("/DEP.heatmap /data <Directory/csv_file> [/schema <color_schema, default=RdYlGn:c11> /no-clrev /KO.class /annotation <annotation.csv> /hide.labels /cluster.n <default=6> /sampleInfo <sampleinfo.csv> /non_DEP.blank /title ""Heatmap of DEPs log2FC"" /t.log2 /tick <-1> /size <size, default=2000,3000> /out <out.DIR>]")>
    <Argument("/non_DEP.blank", True, CLITypes.Boolean,
              Description:="If this parameter present, then all of the non-DEP that bring by the DEP set union, will strip as blank on its foldchange value, and set to 1 at finally. Default is reserve this non-DEP foldchange value.")>
    <Argument("/KO.class", True, CLITypes.Boolean,
              AcceptTypes:={GetType(Boolean)},
              Description:="If this argument was set, then the KO class information for uniprotID will be draw on the output heatmap.")>
    <Argument("/sampleInfo_looooooooooooooooooooooong_name", True, CLITypes.File,
              Extensions:="*.csv",
              Description:="Describ the experimental group information")>
    <Argument("/data", False, CLITypes.File, PipelineTypes.std_in,
              Description:="This file path parameter can be both a directory which contains a set of DEPs result or a single csv file path.")>
    <Argument("/hide.labels", True, CLITypes.Boolean,
              Description:="Hide the row labels?")>
    <Argument("/cluster.n", True, CLITypes.Integer,
              Description:="Expects the kmeans cluster result number, default is output 6 kmeans clusters.")>
    <Argument("/schema", True, CLITypes.String,
              Description:="The color patterns of the heatmap visualize, by default is using ``ColorBrewer`` colors.")>
    <Argument("/out", True, CLITypes.File,
              Extensions:="*.csv, *.svg, *.png, jpg, gif, tiff",
              Description:="A directory path where will save the output heatmap plot image and the kmeans cluster details info.")>
    <Argument("/title", True,
              Description:="The main title of this chart plot.")>
    Public Function CLIHelpInfoDemo(args As CommandLine) As Integer

    End Function
End Module

Public Class CLIGrouping

    Public Const TestGroup1 As String = "Test Group1"
    Public Const TestGroup2 As String = "Test Group2"

End Class

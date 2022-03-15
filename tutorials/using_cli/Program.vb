#Region "Microsoft.VisualBasic::18d0ca50b5ba65a7d12c250127146042, sciBASIC#\tutorials\using_cli\Program.vb"

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

    '   Total Lines: 100
    '    Code Lines: 74
    ' Comment Lines: 1
    '   Blank Lines: 25
    '     File Size: 3.99 KB


    ' Module Program
    ' 
    '     Function: Main
    ' 
    ' Module CLI
    ' 
    '     Function: API1, CLIHelpInfoDemo, ScriptingTest, vbCode
    ' 
    ' Class CLIGrouping
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Drawing
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

    <ExportAPI("/DEP.heatmap.scatter.3D")>
    <Description("Visualize the DEPs' kmeans cluster result by using 3D scatter plot.")>
    <Usage("/DEP.heatmap.scatter.3D /in <kmeans.csv> /sampleInfo <sampleInfo.csv> [/cluster.prefix <default=""cluster: #""> /size <default=1600,1400> /schema <default=clusters> /view.angle <default=30,60,-56.25> /view.distance <default=2500> /out <out.png>]")>
    <Argument("/in", False, CLITypes.File, PipelineTypes.std_in,
              Extensions:="*.csv",
              Description:="The kmeans cluster result from ``/DEP.heatmap`` command.")>
    <Argument("/sampleInfo", False, CLITypes.File,
              Extensions:="*.csv",
              Description:="Sample info fot grouping the matrix column data and generates the 3d plot ``<x,y,z>`` coordinations.")>
    <Argument("/cluster.prefix", True, CLITypes.String,
              Description:="The term prefix of the kmeans cluster name when display on the legend title.")>
    <Argument("/size", True,
              AcceptTypes:={GetType(Size)},
              Description:="The output 3D scatter plot image size.")>
    <Argument("/view.angle", True,
              Description:="The view angle of the 3D scatter plot objects, in 3D direction of ``<X>,<Y>,<Z>``")>
    <Argument("/view.distance", True, CLITypes.Integer,
              Description:="The view distance from the 3D camera screen to the 3D objects.")>
    <Argument("/out", True, CLITypes.File,
              Extensions:="*.png, *.svg",
              Description:="The file path of the output plot image.")>
    Public Function CLIHelpInfoDemo(args As CommandLine) As Integer

    End Function
End Module

Public Class CLIGrouping

    Public Const TestGroup1 As String = "Test Group1"
    Public Const TestGroup2 As String = "Test Group2"

End Class












' test comment

#Region "Microsoft.VisualBasic::c400715298d23517e1b11c16a8016e2a, vs_solutions\JavaScript\test\ArtifactSyncTests.vb"

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

    '   Total Lines: 72
    '    Code Lines: 44 (61.11%)
    ' Comment Lines: 19 (26.39%)
    '    - Xml Docs: 84.21%
    ' 
    '   Blank Lines: 9 (12.50%)
    '     File Size: 3.19 KB


    ' Class ArtifactSyncTests
    ' 
    '     Function: ArtifactDirectory
    ' 
    '     Sub: ArtifactMatchesGeneratorOutput, DemoScriptRunsOnInterpreter
    ' 
    ' /********************************************************************************/

#End Region

Option Strict On
Option Explicit On

Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime
Imports Xunit

''' <summary>
''' Keeps <c>test/artifacts/GeneratedScript.vb</c> — the checked-in translation of
''' <c>test/artifacts/demo.js</c> — byte-identical with the current
''' <see cref="CodeGenerator"/> output.
''' <para>
''' The artifact is compiled into the test assembly (SDK globbing), so it also
''' acts as a build-time COMPILE check for generated code: a code generator change
''' that emits uncompilable VB breaks the build, not just a test.
''' </para>
''' <para>
''' Regenerate after intentional code-generator changes:
''' <c>set LITEJS_REGEN_ARTIFACTS=1</c> then run this test.
''' </para>
''' </summary>
Public Class ArtifactSyncTests

    ''' <summary>Feature demonstration script that the artifact is generated from.</summary>
    Public Const DemoFile As String = "demo.js"
    Public Const ArtifactFile As String = "GeneratedScript.vb"
    Public Const ArtifactClassName As String = "GeneratedScript"

    <Fact>
    Public Sub ArtifactMatchesGeneratorOutput()
        Dim dir = ArtifactDirectory()
        Dim source = File.ReadAllText(System.IO.Path.Combine(dir, DemoFile))
        Dim generated = New CodeGenerator().Generate(Parser.Parse(source), ArtifactClassName)
        Dim target = System.IO.Path.Combine(dir, ArtifactFile)

        ' System.Environment: the VB runtime module of the same name shadows it
        If System.Environment.GetEnvironmentVariable("LITEJS_REGEN_ARTIFACTS") = "1" Then
            File.WriteAllText(target, generated)
            Return
        End If

        Dim committed = File.ReadAllText(target)
        Assert.True(committed = generated,
                    $"artifacts/{ArtifactFile} is stale — regenerate it with LITEJS_REGEN_ARTIFACTS=1 " &
                    "after code-generator changes")
    End Sub

    ''' <summary>The demo script must also run cleanly on the interpreter.</summary>
    <Fact>
    Public Sub DemoScriptRunsOnInterpreter()
        Dim source = File.ReadAllText(System.IO.Path.Combine(ArtifactDirectory(), DemoFile))
        Dim io As ScriptIO = New LogTextIO
        Dim engine As New Interpreter(io)
        ' the demo uses the CLR reflection bridge: hand it a real host object
        engine.DefineGlobal("bridge", New ClrHostFixture())
        engine.Run(Parser.Parse(source))
        Dim lines = DirectCast(io, LogTextIO).Lines
        Assert.True(lines.Count > 0, "the demo script produced no output")
    End Sub

    Private Shared Function ArtifactDirectory() As String
        ' walk up from the test binaries to the project source folder
        Dim dir = New DirectoryInfo(AppContext.BaseDirectory)
        While dir IsNot Nothing AndAlso Not Directory.Exists(System.IO.Path.Combine(dir.FullName, "artifacts"))
            dir = dir.Parent
        End While
        Assert.True(dir IsNot Nothing, "artifacts folder not found")
        Return System.IO.Path.Combine(dir.FullName, "artifacts")
    End Function

End Class

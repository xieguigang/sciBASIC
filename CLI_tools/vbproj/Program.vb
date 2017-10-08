#Region "Microsoft.VisualBasic::4c5795b6ed644def1c1daf4b3d83123d, ..\sciBASIC#\CLI_tools\vbproj\Program.vb"

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
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Emit.CodeDOM_VBC.CodeHelper
Imports System.Text

Module Program

    Public Function Main(args As String()) As Integer
        Return GetType(Program).RunCLI(App.CommandLine)
    End Function

    Const Condition$ = " '$(Configuration)|$(Platform)' == '%s|%s' "

    ''' <summary>
    ''' 为某个解决方案的文件夹之中的每一个项目文件配置统一的输出文件夹
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <ExportAPI("/config.output")>
    <Usage("/config.output /in <*.vbproj/DIR> /output <DIR> /c 'config=<Name>;platform=<type>'")>
    <Description("Config the output location of the project file.")>
    Public Function ConfigOutputPath(args As CommandLine) As Integer
        Dim [in] As String = args <= "/in"
        Dim output As String = args.GetFullDIRPath("/output")
        Dim c As Dictionary(Of String, String) = args.GetDictionary("/c")
        Dim files$()
        Dim condition$ = ""

        With c
            Try
                condition$ = Program.Condition <= {
                    !config, !platform
                }.StringFormat
            Catch ex As Exception
                ex = New Exception(.GetJson, ex)
                Throw ex
            Finally
                Call $" ==> {condition}".__INFO_ECHO
            End Try
        End With

        If [in].FileExists Then
            files = {[in]}
        Else
            files = (ls - l - r - "*.vbproj" <= [in]).ToArray
        End If

        For Each xml As String In files
            Dim vbproj As Project = xml.LoadXml(Of Project)(,, AddressOf Project.RemoveNamespace)
            Dim config = vbproj.GetProfile(condition$)
            Dim relOut$ = RelativePath(xml.ParentPath, output, appendParent:=False) ' 获取得到的是相对于vbproj文件的目标文件夹的相对路径

            If config Is Nothing Then
                Call $"Project: {xml.GetFullPath} didn't have target config profile, ignore this project item...".EchoLine
                Continue For
            End If

#If DEBUG Then
            xml = xml.TrimSuffix & "_updated.vbproj"
#End If
            config.OutputPath = relOut
            vbproj.Save(xml, Encodings.UTF8)
        Next

        Return 0
    End Function

    <ExportAPI("/strings.enum.Code")>
    <Usage("/strings.enum.code /in <data.txt> /name <enumName> [/pascal /out <out.vb>]")>
    Public Function GenerateEnumFromString(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim name$ = args <= "/name"
        Dim pascal As Boolean = args.IsTrue("/pascal")
        Dim out$ = (args <= "/out") Or ([in].TrimSuffix & "_" & name & ".vb").AsDefault
        Dim members$() = [in].ReadAllLines

        Return members _
            .EnumCodeHelper(name,, pascalStyle:=pascal) _
            .SaveTo(out, Encoding.UTF8) _
            .CLICode
    End Function
End Module

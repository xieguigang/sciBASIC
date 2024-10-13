#Region "Microsoft.VisualBasic::6270730e9c4fce791d04b058465e0fa5, Microsoft.VisualBasic.Core\src\CommandLine\Interpreters\View\SDKManual.vb"

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

    '   Total Lines: 312
    '    Code Lines: 233 (74.68%)
    ' Comment Lines: 30 (9.62%)
    '    - Xml Docs: 86.67%
    ' 
    '   Blank Lines: 49 (15.71%)
    '     File Size: 14.27 KB


    '     Module SDKManual
    ' 
    '         Function: HelpSummary, LaunchManual, MarkdownDoc
    ' 
    '         Sub: AppSummary
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Globalization
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.Utility
Imports Microsoft.VisualBasic.CommandLine.Grouping
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.My.JavaScript
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Text
Imports ASCII = Microsoft.VisualBasic.Text.ASCII
Imports AssemblyMeta = Microsoft.VisualBasic.ApplicationServices.Development.AssemblyInfo
Imports VBCore = Microsoft.VisualBasic.App

Namespace CommandLine.ManView

    ''' <summary>
    ''' Generates the help document in markdown format.
    ''' (生成markdown格式的帮助文件)
    ''' </summary>
    Module SDKManual

        ''' <summary>
        ''' 这个是用于在终端上面显示的无格式的文本输出
        ''' </summary>
        ''' <param name="CLI"></param>
        ''' <returns></returns>
        <Extension>
        Public Function LaunchManual(CLI As Interpreter) As Integer
            Dim assm As AssemblyMeta = ApplicationInfoUtils.FromTypeModule(CLI.Type)
            Dim title As String = $"{Application.ProductName} [version {Application.ProductVersion}]" & vbCrLf &
                vbCrLf &
                "## " & assm.AssemblyTitle & vbCrLf &
                vbCrLf &
                "Description: " & assm.AssemblyDescription & vbCrLf &
                "Company:     " & assm.AssemblyCompany & vbCrLf &
                assm.AssemblyCopyright  ' 首页

            Dim sb As New StringBuilder

            Call sb.AppendLine($"Module AssemblyName: {App.ExecutablePath.ToFileURL}")
            Call sb.AppendLine($"Root namespace: " & CLI.ToString)
            Call sb.AppendLine(vbCrLf & vbCrLf & CLI.HelpSummary(False))

            Dim firstPage As String = sb.ToString
            Dim pages As List(Of String) = {
                DebuggerArgs.DebuggerHelps,
                CLI.Type.NamespaceEntry.Description
            }.AsList

            pages += LinqAPI.Exec(Of String) <=
 _
                From api As SeqValue(Of APIEntryPoint)
                In CLI.APIList.SeqIterator(offset:=1)
                Let index As String = api.i & ".   "
                Let info As String = api.value.HelpInformation
                Select index & info

            Call New IndexedManual(pages, title).ShowManual()

            Return 0
        End Function

        ''' <summary>
        ''' 这个是用于保存于文件之中的markdown格式的有格式标记的文本输出
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function MarkdownDoc(App As Interpreter) As String
            Dim sb As New StringBuilder($"# { VisualBasic.App.ProductName} [version { VisualBasic.App.Version}]")
            Dim type As Type = App.Type
            Dim assm As AssemblyMeta = ApplicationInfoUtils.FromTypeModule(App.Type)

            Call sb.AppendLine()
            Call sb.AppendLine("> " & App.Type.NamespaceEntry.Description.LineTokens.JoinBy(vbCrLf & "> "))
            Call sb.AppendLine()
            Call sb.AppendLine("<!--more-->")
            Call sb.AppendLine()
            Call sb.AppendLine($"**{assm.AssemblyTitle}**<br/>")
            Call sb.AppendLine($"_{assm.AssemblyDescription}_<br/>")
            Call sb.AppendLine(assm.AssemblyCopyright)
            Call sb.AppendLine()

            Call sb.AppendLine($"**Module AssemblyName**: {type.Assembly.Location.BaseName}<br/>")
            Call sb.AppendLine($"**Root namespace**: ``{App.Type.FullName}``<br/>")

            Dim helps As ExceptionHelp = type.GetAttribute(Of ExceptionHelp)

            If Not helps Is Nothing Then
                Call sb.AppendLine()
                Call sb.AppendLine("------------------------------------------------------------")
                Call sb.AppendLine("If you are having trouble debugging this Error, first read the best practices tutorial for helpful tips that address many common problems:")
                Call sb.AppendLine("> " & helps.Documentation)
                Call sb.AppendLine()
                Call sb.AppendLine()
                Call sb.AppendLine("The debugging facility Is helpful To figure out what's happening under the hood:")
                Call sb.AppendLine("> " & helps.Debugging)
                Call sb.AppendLine()
                Call sb.AppendLine()
                Call sb.AppendLine("If you're still stumped, you can try get help from author directly from E-mail:")
                Call sb.AppendLine("> " & helps.EMailLink)
                Call sb.AppendLine()
            End If

            Call sb.AppendLine(vbCrLf & vbCrLf & App.HelpSummary(True))
            Call sb.AppendLine()
            Call sb.AppendLine("## CLI API list")
            Call sb.AppendLine("--------------------------")

            For Each i As SeqValue(Of APIEntryPoint) In App.APIList.SeqIterator
                Dim api As APIEntryPoint = i.value

                Call sb.Append($"<h3 id=""{api.Name}""> {i.i + 1}. ")
                Call sb.AppendLine(api.HelpInformation(md:=True) _
                    .LineTokens _
                    .Select(Function(s) s.Trim) _
                    .JoinBy(vbCrLf))

                If api.Arguments.Count > 0 Then
                    Dim prints = api.Arguments _
                        .Where(Function(x) Not x.Value.AcceptTypes.IsNullOrEmpty) _
                        .ToArray

                    If Not prints.Length = 0 Then
                        Call sb.AppendLine("##### Accepted Types")

                        For Each param As NamedValue(Of ArgumentAttribute) In prints
                            Call sb.AppendLine("###### " & param.Name)

                            For Each pType As Type In param.Value.AcceptTypes
                                Call sb.AppendLine(Activity.DisplayType(pType))
                            Next
                        Next
                    End If
                End If
            Next

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="assem"></param>
        ''' <param name="description">命令行的使用功能描述信息文本</param>
        ''' <param name="SYNOPSIS">命令行的使用语法</param>
        ''' <param name="write"></param>
        <Extension>
        Public Sub AppSummary(assem As AssemblyMeta, description$, SYNOPSIS$, write As TextWriter)
            Dim descr = assem.AssemblyDescription

            Call write.WriteLine()

            Call write.WriteLine(" // ")
            Call write.WriteLine(" // " & Strings.Trim(descr) Or "[No description]".AsDefault)
            Call write.WriteLine(" // ")
            Call write.WriteLine(" // VERSION:   " & (assem.AssemblyVersion Or "1.0.0.*".AsDefault))
            Call write.WriteLine(" // ASSEMBLY:  " & assem.AssemblyFullName)
            Call write.WriteLine(" // COPYRIGHT: " & assem.AssemblyCopyright.Replace("©", "(c)"))
            Call write.WriteLine(" // GUID:      " & assem.Guid)
            Call write.WriteLine(" // BUILT:     " & assem.BuiltTime.ToString(New CultureInfo("en-US")))
            Call write.WriteLine(" // ")

            Call write.WriteLine()
            Call write.WriteLine()

            For Each line$ In Paragraph.SplitParagraph(description, 110)
                Call write.WriteLine(" " & line$)
            Next

            If Not SYNOPSIS.StringEmpty Then
                Call write.WriteLine()
                Call write.WriteLine()
                Call write.WriteLine("SYNOPSIS")
                Call write.WriteLine(SYNOPSIS)
                Call write.WriteLine()
            End If

            Call write.Flush()
        End Sub

        ''' <summary>
        ''' Returns the summary brief help information of all of the commands in current cli interpreter.
        ''' (枚举出本CLI解释器之中的所有的命令的帮助的摘要信息)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' <param name="markdown">Output in markdown format?</param>
        <Extension>
        Public Function HelpSummary(App As Interpreter, markdown As Boolean) As String
            Dim sb As New StringBuilder(1024)
            Dim nameMaxLen% = App.APIList _
                .Select(Function(x) Len(x.Name)) _
                .Max

            If Not markdown Then
                Call ($"{VBCore.AssemblyName} command [/argument argument-value...] [/@set environment-variable=value...]") _
                    .DoCall(Sub(SYNOPSIS)
                                Call VBCore.Info.AppSummary(
                                    description:=App.Info.Description,
                                    SYNOPSIS:=SYNOPSIS,
                                    write:=New StringWriter(sb)
                                )
                            End Sub)
            End If

            Call sb.AppendLine(ListAllCommandsPrompt)
            Call sb.AppendLine()

            Dim gg As New Grouping(CLI:=App)
            Dim print = Sub(list As IEnumerable(Of APIEntryPoint), left$)
                            If markdown Then
                                Call sb.AppendLine("|Function API|Info|")
                                Call sb.AppendLine("|------------|----|")
                            End If

                            For Each API As APIEntryPoint In list
                                If Not markdown Then
                                    Dim indent% = 3 + nameMaxLen - Len(API.Name)
                                    Dim blank$ = New String(c:=" "c, count:=indent)
                                    Dim lines As String() = Paragraph _
                                        .SplitParagraph(API.Info, 90 - nameMaxLen) _
                                        .ToArray
                                    Dim line$ = $"{left}{API.Name}:  {blank}{lines.FirstOrDefault}"

                                    Call sb.AppendLine(line)

                                    If lines.Length > 1 Then
                                        For Each line$ In lines.Skip(1)
                                            Call sb.AppendLine(left & New String(" "c, nameMaxLen + 6) & line$)
                                        Next
                                    End If
                                Else
                                    Call sb.AppendLine(
                                        $"|[{API.Name}](#{API.Name})|{API.Info.LineTokens.JoinBy("<br />")}|")
                                End If
                            Next

                            Call sb.AppendLine()
                            Call sb.AppendLine()
                        End Sub

            If gg.GroupData.ContainsKey(JavaScriptObject.undefined) Then
                If markdown Then
                    Call sb.AppendLine("##### Generic function API list")
                End If

                Dim undefines = gg.GroupData(JavaScriptObject.undefined)
                Call print(undefines.Data, " ")
            Else
                ' 2017-1-20
                ' 命令行解释器之中已经定义完了所有的API，所以这里已经没有未定义分组的API了
            End If

            If gg.GroupData.Count > 1 AndAlso Not markdown Then
                Call sb.AppendLine("API list that with functional grouping")
                Call sb.AppendLine()
            End If

            For Each g As SeqValue(Of Groups) In gg _
                .Where(Function(list) list.Name <> JavaScriptObject.undefined) _
                .SeqIterator(offset:=1)

                If markdown Then
                    Call sb.AppendLine($"##### {g.i}. {g.value.Name}")
                Else
                    Call sb.AppendLine($"{g.i}. {g.value.Name}")
                End If

                Dim describ$ = Trim(g.value.Description)
                Dim indent As New String(" "c, (g.i & ". ").Length)

                If Not String.IsNullOrEmpty(describ) Then
                    Call sb.AppendLine()

                    If markdown Then
                        Call sb.AppendLine(describ)
                    Else
                        For Each line$ In Paragraph.SplitParagraph(describ, 110)
                            Call sb.AppendLine(indent & line)
                        Next
                    End If
                End If

                Call sb.AppendLine()
                Call sb.AppendLine()
                Call print(g.value.Data, left:=indent)
            Next

            If Not markdown Then
                Call sb.AppendLine(New String("-"c, 100))
                Call sb.AppendLine()
                Call sb.AppendLine("   " & $"1. You can using ""{AssemblyName} ??<commandName>"" for getting more details command help.")
                Call sb.AppendLine("   " & $"2. Using command ""{AssemblyName} /CLI.dev [---echo]"" for CLI pipeline development.")
                Call sb.AppendLine("   " & $"3. Using command ""{AssemblyName} /i"" for enter interactive console mode.")
                Call sb.AppendLine("   " & $"4. Using command ""{AssemblyName} /STACK:xxMB"" for adjust the application stack size, example as '/STACK:64MB'.")
            End If

            Return sb.ToString.TrimEnd(ASCII.CR, ASCII.LF, " "c)
        End Function

        Public Const ListAllCommandsPrompt As String = "All of the command that available in this program has been list below:"
    End Module
End Namespace

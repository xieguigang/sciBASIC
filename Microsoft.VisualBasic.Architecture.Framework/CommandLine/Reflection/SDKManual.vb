#Region "Microsoft.VisualBasic::a2f9b71ee6fd4e72b2abc8001bf45576, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\CommandLine\Reflection\SDKManual.vb"

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

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Debugging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.SoftwareToolkits
Imports Microsoft.VisualBasic.Terminal.Utility

Namespace CommandLine.Reflection

    Module SDKManual

        Public ReadOnly Property DocPath As String = $"{App.ExecutablePath.TrimSuffix}.md"

        ''' <summary>
        ''' 这个是用于在终端上面显示的无格式的文本输出
        ''' </summary>
        ''' <param name="CLI"></param>
        ''' <returns></returns>
        <Extension>
        Public Function LaunchManual(CLI As Interpreter) As Integer
            Dim assm As ApplicationDetails = ApplicationDetails.FromTypeModule(CLI.Type)
            Dim title As String = $"{Application.ProductName} [version {Application.ProductVersion}]" & vbCrLf &
                vbCrLf &
                "## " & assm.ProductTitle & vbCrLf &
                vbCrLf &
                "Description: " & assm.ProductDescription & vbCrLf &
                "Company:     " & assm.CompanyName & vbCrLf &
                assm.CopyRightsDetail  ' 首页

            Dim sb As New StringBuilder

            Call sb.AppendLine($"Module AssemblyName: {App.ExecutablePath.ToFileURL}")
            Call sb.AppendLine($"Root namespace: " & CLI.ToString)
            Call sb.AppendLine(vbCrLf & vbCrLf & CLI.HelpSummary(False))

            Dim firstPage As String = sb.ToString
            Dim pages As String() = {
                DebuggerArgs.DebuggerHelps,
                CLI.Type.NamespaceEntry.Description
            }

            pages += LinqAPI.MakeList(Of String) <=
 _
                From api As SeqValue(Of APIEntryPoint)
                In CLI.APIList.SeqIterator(offset:=1)
                Let index As String = api.i & ".   "
                Select index & api.obj.HelpInformation

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
            Dim sb As New StringBuilder($"# {Application.ProductName} [version {Application.ProductVersion}]")
            Dim type As Type = App.Type
            Dim assm As ApplicationDetails = ApplicationDetails.FromTypeModule(App.Type)

            Call sb.AppendLine()
            Call sb.AppendLine("> " & App.Type.NamespaceEntry.Description.lTokens.JoinBy(vbCrLf & "> "))
            Call sb.AppendLine()
            Call sb.AppendLine("<!--more-->")
            Call sb.AppendLine()
            Call sb.AppendLine($"**{assm.ProductTitle}**")
            Call sb.AppendLine($"_{assm.ProductDescription}_")
            Call sb.AppendLine(assm.CopyRightsDetail)
            Call sb.AppendLine()

            Call sb.AppendLine($"**Module AssemblyName**: {type.Assembly.Location.ToFileURL}")
            Call sb.AppendLine($"**Root namespace**: ``{App.Type.FullName}``")

            Dim helps As ExceptionHelp = type.GetAttribute(Of ExceptionHelp)

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


            Call sb.AppendLine(vbCrLf & vbCrLf & App.HelpSummary(True))
            Call sb.AppendLine()
            Call sb.AppendLine("## CLI API list")
            Call sb.AppendLine("--------------------------")

            For Each i As SeqValue(Of APIEntryPoint) In App.APIList.SeqIterator
                Dim api As APIEntryPoint = i.obj

                Call sb.Append($"<h3 id=""{api.Name}""> {i.i + 1}. ")
                Call sb.AppendLine(api.HelpInformation(md:=True) _
                    .lTokens _
                    .Select(Function(s) s.Trim) _
                    .JoinBy(vbCrLf))

                If api.Arguments.Count > 0 Then
                    Call sb.AppendLine("##### Accepted Types")

                    For Each param As NamedValue(Of Argument) In api.Arguments
                        Call sb.AppendLine("###### " & param.Name)

                        For Each pType As Type In param.x.AcceptTypes.SafeQuery
                            Call sb.AppendLine(Actives.DisplType(pType))
                        Next
                    Next
                End If
            Next

            Return sb.ToString
        End Function

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
            Dim nameMaxLen As Integer =
                App.APIList.Select(Function(x) Len(x.Name)).Max

            Call sb.AppendLine(ListAllCommandsPrompt)
            Call sb.AppendLine()

            If markdown Then
                Call sb.AppendLine("|Function API|Info|")
                Call sb.AppendLine("|------------|----|")
            End If

            For Each commandInfo As APIEntryPoint In App.APIList
                If Not markdown Then
                    Dim blank As String =
                        New String(c:=" "c, count:=nameMaxLen - Len(commandInfo.Name))
                    Dim line As String = $" {commandInfo.Name}:  {blank}{commandInfo.Info}"

                    Call sb.AppendLine(line)
                Else
                    Call sb.AppendLine($"|[{commandInfo.Name}](#{commandInfo.Name})|{commandInfo.Info}|")
                End If
            Next

            Return sb.ToString
        End Function

        Public Const ListAllCommandsPrompt As String = "All of the command that available in this program has been list below:"
    End Module
End Namespace

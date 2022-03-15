#Region "Microsoft.VisualBasic::354efdba57a08f8ffa12f354e40b0265, sciBASIC#\Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\InteractiveIODevice\Shell.vb"

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

    '   Total Lines: 64
    '    Code Lines: 34
    ' Comment Lines: 21
    '   Blank Lines: 9
    '     File Size: 2.20 KB


    '     Class Shell
    ' 
    '         Properties: autoCompleteCandidates, dev, History, ps1, Quite
    '                     shell
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Run
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Terminal.STDIO__
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash

Namespace ApplicationServices.Terminal

    ''' <summary>
    ''' Shell model for console.
    ''' </summary>
    Public Class Shell

        Public ReadOnly Property ps1 As PS1
        Public ReadOnly Property shell As Action(Of String)
        ''' <summary>
        ''' a candidate list for implements auto-complete for console input.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property autoCompleteCandidates As New List(Of String)
        Public ReadOnly Property dev As IConsole

        ''' <summary>
        ''' Command text for exit the shell loop 
        ''' 
        ''' (默认的退出文本为vim的 ``:q`` 命令)
        ''' </summary>
        ''' <returns></returns>
        Public Property Quite As String = ":q"
        Public Property History As String = ":h"

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="ps1">The commandline prompt prefix headers.</param>
        ''' <param name="exec">How to execute the command line input.</param>
        Sub New(ps1 As PS1, exec As Action(Of String), Optional dev As IConsole = Nothing)
            Me.ps1 = ps1
            Me.shell = exec
            Me.dev = If(dev, New Terminal)
        End Sub

        ''' <summary>
        ''' 执行一个配置好的命令行模型, 代码会被一直阻塞在这里
        ''' </summary>
        Public Sub Run()
            Dim cli As Value(Of String) = ""

            Do While App.Running
                Call dev.Write(ps1.ToString)

                If Strings.Trim((cli = dev.ReadLine)).StringEmpty OrElse
                    cli.Value = vbCrLf OrElse
                    cli.Value = vbCr OrElse
                    cli.Value = vbLf Then

                    Call _shell("")
                ElseIf cli.Value.TextEquals(Quite) Then
                    Exit Do
                Else
                    Call _shell(cli)
                End If
            Loop
        End Sub
    End Class
End Namespace

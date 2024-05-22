#Region "Microsoft.VisualBasic::eaac9e96bab91106ab0a2587de35a1de, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\InteractiveIODevice\Shell.vb"

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

    '   Total Lines: 79
    '    Code Lines: 42 (53.16%)
    ' Comment Lines: 24 (30.38%)
    '    - Xml Docs: 91.67%
    ' 
    '   Blank Lines: 13 (16.46%)
    '     File Size: 2.51 KB


    '     Interface IShellDevice
    ' 
    '         Function: ReadLine
    ' 
    '         Sub: SetPrompt
    ' 
    '     Class Shell
    ' 
    '         Properties: History, ps1, Quite, shell, ttyDev
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Run
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Terminal.LineEdit
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash

Namespace ApplicationServices.Terminal

    Public Interface IShellDevice

        Sub SetPrompt(s As String)
        Function ReadLine() As String

    End Interface

    ''' <summary>
    ''' Shell model for console.
    ''' </summary>
    Public Class Shell

        Public ReadOnly Property ps1 As PS1
        ''' <summary>
        ''' engine for execute the command, example as execute script text in ``R#``.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property shell As Action(Of String)
        Public ReadOnly Property ttyDev As IShellDevice
            Get
                Return dev
            End Get
        End Property

        ''' <summary>
        ''' Command text for exit the shell loop 
        ''' 
        ''' (默认的退出文本为vim的 ``:q`` 命令)
        ''' </summary>
        ''' <returns></returns>
        Public Property Quite As String = ":q"
        Public Property History As String = ":h"

        Dim WithEvents dev As IShellDevice

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="ps1">The commandline prompt prefix headers.</param>
        ''' <param name="exec">How to execute the command line input.</param>
        ''' <param name="dev">
        ''' <see cref="LineReader"/>
        ''' </param>
        Sub New(ps1 As PS1, exec As Action(Of String), Optional dev As IShellDevice = Nothing)
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
                Call dev.SetPrompt(ps1.ToString)

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

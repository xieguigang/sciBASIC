#Region "Microsoft.VisualBasic::0d80f2526ce3dc38d41616f7bc3c7360, Microsoft.VisualBasic.Core\CommandLine\InteropService\Abstract.vb"

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

'     Class InteropService
' 
'         Properties: Path
' 
'         Function: RunDotNetApp, RunProgram, ToString
' 
'     Class NullOrDefault
' 
'         Properties: value
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: ToString
' 
'     Class CLIBuilder
' 
'         Function: ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text

Namespace CommandLine.InteropService

    ''' <summary>
    ''' The class object which can interact with the target commandline program.
    ''' (与目标命令行程序进行命令行交互的编程接口，本类型的对象的作用主要是生成命令行参数)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class InteropService : Inherits CLIBuilder

        ''' <summary>
        ''' App path
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Path As String
            Get
                Return _executableAssembly
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(app As String)
            _executableAssembly = app
        End Sub

        ''' <summary>
        ''' Assembly path for the target invoked program.
        ''' </summary>
        ''' <remarks></remarks>
        Protected Friend _executableAssembly As String

        Public Function RunDotNetApp(args$) As IIORedirectAbstract
            Return App.Shell(_executableAssembly, args, CLR:=True)
        End Function

        Public Shared Function GetLastError(proc As IIORedirectAbstract) As ExceptionData
            Dim out$ = proc.StandardOutput
            Dim err$ = out _
                .Match("^\[INFOM .+?\] \[Log\].+$", RegexOptions.Multiline) _
                .StringReplace("\[.+?\] \[Log\]\s+", "") _
                .Trim(" "c, ASCII.TAB, ASCII.CR, ASCII.LF)
            Dim logs As List(Of String()) = err.ReadAllText _
                .lTokens _
                .FlagSplit(Function(s) s.IsPattern("[=]+")) _
                .AsList

            Dim typeINF = logs(-3).Where(Function(s) Not s.StringEmpty).First.Trim(":"c)
            Dim message = logs(-2).Where(Function(s) Not s.StringEmpty).Select(Function(s) Mid(s, 9).Trim).ToArray
            Dim tracess = logs(-1).Where(Function(s) Not s.StringEmpty).Select(Function(s) Mid(s, 6).Trim).ToArray

            Return ExceptionData.CreateInstance(message, tracess, typeINF)
        End Function

        ''' <summary>
        ''' 请注意，这个函数只是生成了具体的进程调用对象，还需要手动调用
        ''' <see cref="IIORedirectAbstract.Run()"/>或者
        ''' <see cref="IIORedirectAbstract.Start(Boolean)"/>
        ''' 方法才会启动目标进程
        ''' </summary>
        ''' <param name="args$"></param>
        ''' <returns></returns>
        Public Function RunProgram(args$) As IIORedirectAbstract
            Return App.Shell(_executableAssembly, args, CLR:=False)
        End Function

        Public Overrides Function ToString() As String
            If String.IsNullOrEmpty(_executableAssembly) Then
                Return MyBase.ToString()
            Else
                Return _executableAssembly
            End If
        End Function
    End Class

    ''' <summary>
    ''' Default value
    ''' </summary>
    <AttributeUsage(AttributeTargets.Field, AllowMultiple:=False, Inherited:=True)>
    Public Class NullOrDefault : Inherits Attribute

        Public ReadOnly Property value As String

        Sub New(Optional _default As String = "")
            value = _default
        End Sub

        Public Overrides Function ToString() As String
            Return value
        End Function
    End Class

    Public MustInherit Class CLIBuilder

        Public Overrides Function ToString() As String
            Return Me.GetCLI
        End Function
    End Class
End Namespace

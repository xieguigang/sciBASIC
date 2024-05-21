#Region "Microsoft.VisualBasic::a7c126e75a02b479b7be6f1d0006e357, Microsoft.VisualBasic.Core\src\CommandLine\InteropService\Abstract.vb"

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

    '   Total Lines: 183
    '    Code Lines: 101
    ' Comment Lines: 55
    '   Blank Lines: 27
    '     File Size: 6.86 KB


    '     Class InteropService
    ' 
    '         Properties: dotnetcoreApp, IORedirect, IsAvailable, Path
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: CreateSlave, GetDotnetCoreCommandLine, GetLastCLRException, GetLastError, RunDotNetApp
    '                   RunProgram, ToString
    ' 
    '         Sub: SetDotNetCoreDll
    ' 
    '     Interface AppDriver
    ' 
    '         Properties: App
    ' 
    '     Class CLIBuilder
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.CommandLine.InteropService.Pipeline
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser

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
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return _executableAssembly
            End Get
        End Property

        ''' <summary>
        ''' 这个只读属性返回目标可执行文件是否有效
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsAvailable As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                ' exe file should be exists on file system 
                ' and it also should be non-empty for 
                ' execute the program
                Return _executableAssembly.FileExists(True)
            End Get
        End Property

        ''' <summary>
        ''' 默认是不做IO重定向的
        ''' </summary>
        ''' <returns></returns>
        Public Property IORedirect As Boolean = False
        Public Property dotnetcoreApp As Boolean = False

        Sub New()
        End Sub

        ''' <summary>
        ''' 通过应用程序的可执行文件路径来构建命令行的交互对象
        ''' </summary>
        ''' <param name="app">the target exe file path</param>
        ''' <remarks>
        ''' this module build dotnet call for clr on unix system automatically.
        ''' </remarks>
        Sub New(app As String)
            _executableAssembly = app
            _executableDll = app.ChangeSuffix("dll")
        End Sub

        ''' <summary>
        ''' Assembly path for the target invoked program.
        ''' </summary>
        ''' <remarks></remarks>
        Protected Friend _executableAssembly As String
        ''' <summary>
        ''' .NET Core dll corresponding to run <see cref="_executableAssembly"/>.
        ''' </summary>
        Protected Friend _executableDll As String

        Dim lastProc As IIORedirectAbstract

        Public Sub SetDotNetCoreDll()
            _executableDll = _executableAssembly.ChangeSuffix("dll")
        End Sub

        Public Function RunDotNetApp(args$) As IIORedirectAbstract
#If NETCOREAPP Then
            lastProc = App.Shell(_executableDll, args, CLR:=True)
#Else
            lastProc = App.Shell(_executableAssembly, args, CLR:=True)
#End If
            Return lastProc
        End Function

        ''' <summary>
        ''' 线程不安全
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetLastCLRException() As ExceptionData
            Return GetLastError(lastProc)
        End Function

        Public Shared Function GetLastError(proc As IIORedirectAbstract) As ExceptionData
            Dim out$ = proc?.StandardOutput Or EmptyString
            Dim err$ = out _
                .Match("^\[INFOM .+?\] \[Log\].+$", RegexOptions.Multiline) _
                .StringReplace("\[.+?\] \[Log\]\s+", "") _
                .Trim(" "c, ASCII.TAB, ASCII.CR, ASCII.LF)
            Dim logs As List(Of String()) = err _
                .ReadAllText(throwEx:=False, suppress:=True) _
                .LineTokens _
                .FlagSplit(Function(s) s.IsPattern("[=]+")) _
                .AsList

            If logs.IsNullOrEmpty Then
                Return Nothing
            End If

            Dim typeINF = logs(-3).Where(Function(s) Not s.StringEmpty).First.Trim(":"c)
            Dim message = logs(-2).Where(Function(s) Not s.StringEmpty).Select(Function(s) Mid(s, 9).Trim).ToArray
            Dim tracess = logs(-1).Where(Function(s) Not s.StringEmpty).Select(Function(s) Mid(s, 6).Trim).ToArray

            Return ExceptionData.CreateInstance(message, tracess, typeINF)
        End Function

        Public Function CreateSlave(args As String, Optional workdir As String = Nothing) As RunSlavePipeline
            If dotnetcoreApp Then
                Return New RunSlavePipeline("dotnet", GetDotnetCoreCommandLine(args), workdir)
            Else
                Return New RunSlavePipeline(_executableAssembly, args, workdir)
            End If
        End Function

        Public Function GetDotnetCoreCommandLine(args As String) As String
            Return $"""{_executableDll}"" {args}"
        End Function

        ''' <summary>
        ''' 运行非.NET应用程序
        ''' 请注意，这个函数只是生成了具体的进程调用对象，还需要手动调用
        ''' <see cref="IIORedirectAbstract.Run()"/>或者
        ''' <see cref="IIORedirectAbstract.Start(Boolean)"/>
        ''' 方法才会启动目标进程
        ''' </summary>
        ''' <param name="args$"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function RunProgram(args$, Optional stdin$ = Nothing) As IIORedirectAbstract
#If NETCOREAPP Then
            Return App.Shell(_executableDll, args, CLR:=False, stdin:=stdin)
#Else
            Return App.Shell(_executableAssembly, args, CLR:=False, stdin:=stdin)
#End If
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
    '''应用程序的执行驱动抽象接口, 这个抽象接口是为了兼容命令行应用和Docker环境下的命令行应用而设置的
    ''' </summary>
    Public Interface AppDriver

        ''' <summary>
        ''' 命令行命令或者可执行文件的路径
        ''' </summary>
        ''' <returns></returns>
        Property App As String



    End Interface

    Public MustInherit Class CLIBuilder

        Public Overrides Function ToString() As String
            Return Me.GetCLI
        End Function
    End Class
End Namespace

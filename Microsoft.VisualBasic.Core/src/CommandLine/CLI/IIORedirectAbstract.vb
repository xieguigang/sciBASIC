#Region "Microsoft.VisualBasic::9507a5e3cd6b2ec30a9b908f6e9aa232, Microsoft.VisualBasic.Core\src\CommandLine\CLI\IIORedirectAbstract.vb"

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

    '   Total Lines: 39
    '    Code Lines: 10
    ' Comment Lines: 24
    '   Blank Lines: 5
    '     File Size: 1.96 KB


    '     Interface IIORedirectAbstract
    ' 
    '         Properties: Bin, CLIArguments, StandardOutput
    ' 
    '         Function: Run, Start
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace CommandLine

    Public Interface IIORedirectAbstract

        ReadOnly Property Bin As String
        ReadOnly Property CLIArguments As String

        ''' <summary>
        ''' The target invoked process event has been exit with a specific return code.(目标派生子进程已经结束了运行并且返回了一个错误值)
        ''' </summary>
        ''' <param name="exitCode"></param>
        ''' <param name="exitTime"></param>
        ''' <remarks></remarks>
        Event ProcessExit(exitCode As Integer, exitTime As String)

        ''' <summary>
        ''' Gets the standard output for the target invoke process.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property StandardOutput As String

        ''' <summary>
        ''' Start the target process. If the target invoked process is currently on the running state, 
        ''' then this function will returns the -100 value as error code and print the warning 
        ''' information on the system console.(启动目标进程)
        ''' </summary>
        ''' <param name="WaitForExit">Indicate that the program code wait for the target process exit or not?(参数指示应用程序代码是否等待目标进程的结束)</param>
        ''' <returns>当发生错误的时候会返回错误代码，当当前的进程任然处于运行的状态的时候，程序会返回-100错误代码并在终端之上打印出警告信息</returns>
        ''' <remarks></remarks>
        Function Start(Optional WaitForExit As Boolean = False) As Integer
        ''' <summary>
        ''' 启动目标子进程，然后等待执行完毕并返回退出代码(请注意，在进程未执行完毕之前，整个线程会阻塞在这里)
        ''' </summary>
        ''' <returns></returns>
        Function Run() As Integer
    End Interface
End Namespace

#Region "Microsoft.VisualBasic::f5dbb4e948844ce357c23baf15f6487c, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\STDIO__\Shell.vb"

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

    '   Total Lines: 55
    '    Code Lines: 27
    ' Comment Lines: 18
    '   Blank Lines: 10
    '     File Size: 1.89 KB


    '     Module Shell
    ' 
    '         Function: GetConsoleWindow, SetConsoleCtrlHandler, ShowWindow
    ' 
    '         Sub: HideConsoleWindow, ShowConsoleWindows
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.CommandLine.Parsers

Namespace ApplicationServices.Terminal.STDIO__

    '
    ' * Created by SharpDevelop.
    ' * User: WORKGROUP
    ' * Date: 2015/2/26
    ' * Time: 0:13
    ' * 
    ' * To change this template use Tools | Options | Coding | Edit Standard Headers.
    ' 

    Public Module Shell

        ''' <summary>
        ''' You can create a console window In a Windows Forms project.  Project + properties, turn off "Enable application framework" 
        ''' And Set Startup Object To "Sub Main". 
        ''' 
        ''' Modify the Application.Run() statement To create the proper startup form, If necessary.
        ''' </summary>
        ''' <returns></returns>
        Public Declare Auto Function AllocConsole Lib "kernel32.dll" () As Boolean

        <DllImport("user32.dll")>
        Public Function ShowWindow(hWnd As IntPtr, nCmdShow As Integer) As Boolean
        End Function

        <DllImport("kernel32")>
        Public Function GetConsoleWindow() As IntPtr
        End Function

        <DllImport("Kernel32")>
        Private Function SetConsoleCtrlHandler(handler As EventHandler, add As Boolean) As Boolean
        End Function

        Private ReadOnly hConsole As IntPtr = GetConsoleWindow()

        Public Sub HideConsoleWindow()
            If IntPtr.Zero <> hConsole Then
                Call ShowWindow(hConsole, 0)
            End If
        End Sub

        ''' <summary>
        ''' 为WinForm应用程序分配一个终端窗口，这个函数一般是在Debug模式之下进行程序调试所使用的
        ''' </summary>
        Public Sub ShowConsoleWindows()
            If IntPtr.Zero <> hConsole Then
                Call ShowWindow(hConsole, 1)
            End If
        End Sub
    End Module
End Namespace

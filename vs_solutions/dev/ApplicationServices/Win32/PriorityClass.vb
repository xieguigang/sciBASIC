#Region "Microsoft.VisualBasic::d2fb9c656753077274a4c14b6c3ef9d6, sciBASIC#\vs_solutions\dev\ApplicationServices\Win32\PriorityClass.vb"

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

    '   Total Lines: 43
    '    Code Lines: 17
    ' Comment Lines: 21
    '   Blank Lines: 5
    '     File Size: 1.76 KB


    '     Module PriorityClass
    ' 
    '         Function: PriorityClass
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Win32

    ''' <summary>
    ''' Process priority class helper.
    ''' </summary>
    Public Module PriorityClass

        ''' <summary>
        ''' 当前进程句柄  
        ''' </summary>
        ''' <returns></returns>
        Public Declare Function GetCurrentProcess Lib "kernel32" () As Integer
        Public Declare Function SetPriorityClass Lib "kernel32" (hProcess As Integer, dwPriorityClass As Integer) As Integer

        ''' <summary>
        ''' 新进程应该有非常低的优先级——只有在系统空闲的时候才能运行。基本值是4  
        ''' </summary>
        Public Const IDLE_PRIORITY_CLASS = &H40
        ''' <summary>
        ''' 新进程有非常高的优先级，它优先于大多数应用程序。基本值是13。注意尽量避免采用这个优先级  
        ''' </summary>
        Public Const HIGH_PRIORITY_CLASS = &H80
        ''' <summary>
        ''' 标准优先级。如进程位于前台，则基本值是9；如在后台，则优先值是7  
        ''' </summary>
        Public Const NORMAL_PRIORITY_CLASS = &H20

        ''' <summary>
        ''' Set Priority Class for current process.
        ''' </summary>
        ''' <param name="priority"><see cref="IDLE_PRIORITY_CLASS"/>, <see cref="HIGH_PRIORITY_CLASS"/>, <see cref="NORMAL_PRIORITY_CLASS"/></param>
        ''' <returns></returns>
        Public Function PriorityClass(priority As Integer) As Boolean
            Dim CurrentProcesshWnd As Integer = GetCurrentProcess

            If (SetPriorityClass(CurrentProcesshWnd, priority) = 0) Then
                Return False
            Else
                Return True
            End If
        End Function
    End Module
End Namespace

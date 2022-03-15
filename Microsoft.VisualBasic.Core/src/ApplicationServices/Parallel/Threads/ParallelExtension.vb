#Region "Microsoft.VisualBasic::ea56ce42c81b5bfd44b7321219c14f86, sciBASIC#\Microsoft.VisualBasic.Core\src\ApplicationServices\Parallel\Threads\ParallelExtension.vb"

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

    '   Total Lines: 93
    '    Code Lines: 37
    ' Comment Lines: 49
    '   Blank Lines: 7
    '     File Size: 3.77 KB


    '     Module ParallelExtension
    ' 
    '         Function: AsyncTask, DoEvents, RunTask
    ' 
    '         Sub: DoEvents
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Threading

Namespace Parallel

    ''' <summary>
    ''' Parallel based on the threading
    ''' </summary>
    ''' 
    <HideModuleName>
    Public Module ParallelExtension

        ''' <summary>
        ''' Application.DoEvents() proxy in winform, Processes all Windows messages currently in the message queue.
        ''' </summary>
        ''' <remarks>
        ''' this function will fixed the errors on centos linux system:
        ''' 
        ''' ```
        ''' Unhandled Exception:
        ''' System.Reflection.TargetInvocationException: Exception has been thrown by the target Of an invocation. 
        ''' ---> System.TypeInitializationException: The type initializer For 'System.Windows.Forms.XplatUI' threw an exception. 
        ''' ---> System.ArgumentNullException: Could not open display (X-Server required. Check your DISPLAY environment variable)
        ''' 
        ''' Parameter name :  Display
        '''   at System.Windows.Forms.XplatUIX11.SetDisplay (System.IntPtr display_handle) [0x00408] In &lt;01b7792664764a0a8aecd9a1e8220761>:0 
        '''   at System.Windows.Forms.XplatUIX11..ctor () [0x00077] In &lt;01b7792664764a0a8aecd9a1e8220761>:0 
        '''   at System.Windows.Forms.XplatUIX11.GetInstance () [0x00019] In &lt;01b7792664764a0a8aecd9a1e8220761>:0 
        '''   at System.Windows.Forms.XplatUI..cctor () [0x000c0] In &lt;01b7792664764a0a8aecd9a1e8220761>:0 
        '''    --- End of inner exception stack trace ---
        '''   at System.Windows.Forms.Application.DoEvents () [0x00000] In &lt;01b7792664764a0a8aecd9a1e8220761>:0
        ''' ```
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Sub DoEvents()
#If netcore5 = 0 Then
#If UNIX = False Then
            Call Application.DoEvents()
#End If
#End If
        End Sub

        ''' <summary>
        ''' execute the given <paramref name="func"/>, and hen do events
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="func"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Function DoEvents(Of T)(func As Func(Of T)) As T
            Dim result As T = func()
#If netcore5 = 0 Then
#If UNIX = False Then
            Call DoEvents()
#End If
#End If
            Return result
        End Function

        ''' <summary>
        ''' Start a new thread and then returns the background thread task handle.
        ''' </summary>
        ''' <param name="start"></param>
        ''' <returns>
        ''' the thread method <see cref="Thread.Start()"/> is already be called
        ''' </returns>
        <Extension>
        <DebuggerStepThrough>
        Public Function RunTask(start As Threading.ThreadStart) As Thread
            Dim thread As New Thread(start)
            thread.Start()
            Return thread
        End Function

        ' 2018-10-6
        ' 下面的这个函数会导致函数调用的时候重载失败
        '<MethodImpl(MethodImplOptions.AggressiveInlining)>
        '<Extension> Public Function RunTask(method As Action) As Thread
        '    Return New ThreadStart(AddressOf method.Method.Invoke).RunTask
        'End Function

        ''' <summary>
        ''' 运行一个后台任务
        ''' </summary>
        ''' <param name="start"></param>
        ''' <returns></returns>
        Public Function AsyncTask(start As Threading.ThreadStart) As IAsyncResult
            Return start.BeginInvoke(Nothing, Nothing)
        End Function
    End Module
End Namespace

#Region "Microsoft.VisualBasic::3a46354dc94f39f89efea4c5036ec35a, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ShellProgressBar\TaskbarProgress.vb"

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

    '   Total Lines: 68
    '    Code Lines: 51
    ' Comment Lines: 3
    '   Blank Lines: 14
    '     File Size: 2.33 KB


    '     Module TaskbarProgress
    ' 
    ' 
    '         Enum TaskbarStates
    ' 
    ' 
    ' 
    ' 
    '         Interface ITaskbarList3
    ' 
    '             Sub: ActivateTab, AddTab, DeleteTab, HrInit, MarkFullscreenWindow
    '                  SetActiveAlt, SetProgressState, SetProgressValue
    ' 
    '         Class TaskbarInstance
    ' 
    ' 
    ' 
    '  
    ' 
    '     Function: GetConsoleWindow
    ' 
    '     Sub: SetState, SetValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Runtime.InteropServices

Namespace ApplicationServices.Terminal.ProgressBar.ShellProgressBar
    Public Module TaskbarProgress
        Public Enum TaskbarStates
            NoProgress = 0
            Indeterminate = &H1
            Normal = &H2
            [Error] = &H4
            Paused = &H8
        End Enum

        <ComImport()>
        <Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")>
        <InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
        Private Interface ITaskbarList3
            ' ITaskbarList
            <PreserveSig>
            Sub HrInit()

            <PreserveSig>
            Sub AddTab(hwnd As IntPtr)

            <PreserveSig>
            Sub DeleteTab(hwnd As IntPtr)

            <PreserveSig>
            Sub ActivateTab(hwnd As IntPtr)

            <PreserveSig>
            Sub SetActiveAlt(hwnd As IntPtr)

            ' ITaskbarList2
            <PreserveSig>
            Sub MarkFullscreenWindow(hwnd As IntPtr,
            <MarshalAs(UnmanagedType.Bool)> fFullscreen As Boolean)

            ' ITaskbarList3
            <PreserveSig>
            Sub SetProgressValue(hwnd As IntPtr, ullCompleted As ULong, ullTotal As ULong)

            <PreserveSig>
            Sub SetProgressState(hwnd As IntPtr, state As TaskbarStates)
        End Interface

        <ComImport>
        <Guid("56fdf344-fd6d-11d0-958a-006097c9a090")>
        <ClassInterface(ClassInterfaceType.None)>
        Private Class TaskbarInstance
        End Class

        <DllImport("kernel32.dll")>
        Private Function GetConsoleWindow() As IntPtr
        End Function

        Private ReadOnly _taskbarInstance As ITaskbarList3 = CType(New TaskbarInstance(), ITaskbarList3)
        Private ReadOnly _taskbarSupported As Boolean = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)

        Public Sub SetState(taskbarState As TaskbarStates)
            If _taskbarSupported Then Call _taskbarInstance.SetProgressState(GetConsoleWindow(), taskbarState)
        End Sub

        Public Sub SetValue(progressValue As Double, progressMax As Double)
            If _taskbarSupported Then Call _taskbarInstance.SetProgressValue(GetConsoleWindow(), progressValue, progressMax)
        End Sub
    End Module
End Namespace

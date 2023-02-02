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

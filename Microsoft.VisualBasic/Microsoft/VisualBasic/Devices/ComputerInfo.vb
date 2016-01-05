Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Diagnostics
Imports System.Globalization
Imports System.Management
Imports System.Runtime.InteropServices
Imports System.Security
Imports System.Security.Permissions

Namespace Microsoft.VisualBasic.Devices
    <DebuggerTypeProxy(GetType(ComputerInfoDebugView)), HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)> _
    Public Class ComputerInfo
        ' Properties
        <CLSCompliant(False)> _
        Public ReadOnly Property AvailablePhysicalMemory As UInt64
            <SecuritySafeCritical> _
            Get
                Return Me.MemoryStatus.AvailablePhysicalMemory
            End Get
        End Property

        <CLSCompliant(False)> _
        Public ReadOnly Property AvailableVirtualMemory As UInt64
            <SecuritySafeCritical> _
            Get
                Return Me.MemoryStatus.AvailableVirtualMemory
            End Get
        End Property

        Public ReadOnly Property InstalledUICulture As CultureInfo
            Get
                Return CultureInfo.InstalledUICulture
            End Get
        End Property

        Private ReadOnly Property MemoryStatus As InternalMemoryStatus
            Get
                If (Me.m_InternalMemoryStatus Is Nothing) Then
                    Me.m_InternalMemoryStatus = New InternalMemoryStatus
                End If
                Return Me.m_InternalMemoryStatus
            End Get
        End Property

        Public ReadOnly Property OSFullName As String
            <SecuritySafeCritical> _
            Get
                Try 
                    Dim str2 As String = "Name"
                    Dim ch As Char = "|"c
                    Dim str3 As String = Conversions.ToString(Me.OSManagementBaseObject.Properties.Item(str2).Value)
                    If str3.Contains(Conversions.ToString(ch)) Then
                        Return str3.Substring(0, str3.IndexOf(ch))
                    End If
                    Return str3
                Catch exception As COMException
                    Return Me.OSPlatform
                End Try
            End Get
        End Property

        Private ReadOnly Property OSManagementBaseObject As ManagementBaseObject
            <SecurityCritical> _
            Get
                Dim queryOrClassName As String = "Win32_OperatingSystem"
                If (Me.m_OSManagementObject Is Nothing) Then
                    Dim query As New SelectQuery(queryOrClassName)
                    Dim objects As ManagementObjectCollection = New ManagementObjectSearcher(query).Get
                    If (objects.Count <= 0) Then
                        Throw ExceptionUtils.GetInvalidOperationException("DiagnosticInfo_FullOSName", New String(0  - 1) {})
                    End If
                    Dim enumerator As ManagementObjectEnumerator = objects.GetEnumerator
                    enumerator.MoveNext
                    Me.m_OSManagementObject = enumerator.Current
                End If
                Return Me.m_OSManagementObject
            End Get
        End Property

        Public ReadOnly Property OSPlatform As String
            Get
                Return Environment.OSVersion.Platform.ToString
            End Get
        End Property

        Public ReadOnly Property OSVersion As String
            Get
                Return Environment.OSVersion.Version.ToString
            End Get
        End Property

        <CLSCompliant(False)> _
        Public ReadOnly Property TotalPhysicalMemory As UInt64
            <SecuritySafeCritical> _
            Get
                Return Me.MemoryStatus.TotalPhysicalMemory
            End Get
        End Property

        <CLSCompliant(False)> _
        Public ReadOnly Property TotalVirtualMemory As UInt64
            <SecuritySafeCritical> _
            Get
                Return Me.MemoryStatus.TotalVirtualMemory
            End Get
        End Property


        ' Fields
        Private m_InternalMemoryStatus As InternalMemoryStatus = Nothing
        <SecurityCritical> _
        Private m_OSManagementObject As ManagementBaseObject = Nothing

        ' Nested Types
        Friend NotInheritable Class ComputerInfoDebugView
            ' Methods
            Public Sub New(RealClass As ComputerInfo)
                Me.m_InstanceBeingWatched = RealClass
            End Sub


            ' Properties
            <DebuggerBrowsable(DebuggerBrowsableState.RootHidden)> _
            Public ReadOnly Property AvailablePhysicalMemory As UInt64
                Get
                    Return Me.m_InstanceBeingWatched.AvailablePhysicalMemory
                End Get
            End Property

            <DebuggerBrowsable(DebuggerBrowsableState.RootHidden)> _
            Public ReadOnly Property AvailableVirtualMemory As UInt64
                Get
                    Return Me.m_InstanceBeingWatched.AvailableVirtualMemory
                End Get
            End Property

            <DebuggerBrowsable(DebuggerBrowsableState.RootHidden)> _
            Public ReadOnly Property InstalledUICulture As CultureInfo
                Get
                    Return Me.m_InstanceBeingWatched.InstalledUICulture
                End Get
            End Property

            <DebuggerBrowsable(DebuggerBrowsableState.RootHidden)> _
            Public ReadOnly Property OSPlatform As String
                Get
                    Return Me.m_InstanceBeingWatched.OSPlatform
                End Get
            End Property

            <DebuggerBrowsable(DebuggerBrowsableState.RootHidden)> _
            Public ReadOnly Property OSVersion As String
                Get
                    Return Me.m_InstanceBeingWatched.OSVersion
                End Get
            End Property

            <DebuggerBrowsable(DebuggerBrowsableState.RootHidden)> _
            Public ReadOnly Property TotalPhysicalMemory As UInt64
                Get
                    Return Me.m_InstanceBeingWatched.TotalPhysicalMemory
                End Get
            End Property

            <DebuggerBrowsable(DebuggerBrowsableState.RootHidden)> _
            Public ReadOnly Property TotalVirtualMemory As UInt64
                Get
                    Return Me.m_InstanceBeingWatched.TotalVirtualMemory
                End Get
            End Property


            ' Fields
            <DebuggerBrowsable(DebuggerBrowsableState.Never)> _
            Private m_InstanceBeingWatched As ComputerInfo
        End Class

        Private Class InternalMemoryStatus
            ' Methods
            Friend Sub New()
            End Sub

            <SecurityCritical> _
            Private Sub Refresh()
                If Me.m_IsOldOS Then
                    Me.m_MemoryStatus = New MEMORYSTATUS
                    NativeMethods.GlobalMemoryStatus(Me.m_MemoryStatus)
                Else
                    Me.m_MemoryStatusEx = New MEMORYSTATUSEX
                    Me.m_MemoryStatusEx.Init
                    If Not NativeMethods.GlobalMemoryStatusEx(Me.m_MemoryStatusEx) Then
                        Throw ExceptionUtils.GetWin32Exception("DiagnosticInfo_Memory", New String(0  - 1) {})
                    End If
                End If
            End Sub


            ' Properties
            Friend ReadOnly Property AvailablePhysicalMemory As UInt64
                <SecurityCritical> _
                Get
                    Me.Refresh
                    If Me.m_IsOldOS Then
                        Return CULng(Me.m_MemoryStatus.dwAvailPhys)
                    End If
                    Return Me.m_MemoryStatusEx.ullAvailPhys
                End Get
            End Property

            Friend ReadOnly Property AvailableVirtualMemory As UInt64
                <SecurityCritical> _
                Get
                    Me.Refresh
                    If Me.m_IsOldOS Then
                        Return CULng(Me.m_MemoryStatus.dwAvailVirtual)
                    End If
                    Return Me.m_MemoryStatusEx.ullAvailVirtual
                End Get
            End Property

            Friend ReadOnly Property TotalPhysicalMemory As UInt64
                <SecurityCritical> _
                Get
                    Me.Refresh
                    If Me.m_IsOldOS Then
                        Return CULng(Me.m_MemoryStatus.dwTotalPhys)
                    End If
                    Return Me.m_MemoryStatusEx.ullTotalPhys
                End Get
            End Property

            Friend ReadOnly Property TotalVirtualMemory As UInt64
                <SecurityCritical> _
                Get
                    Me.Refresh
                    If Me.m_IsOldOS Then
                        Return CULng(Me.m_MemoryStatus.dwTotalVirtual)
                    End If
                    Return Me.m_MemoryStatusEx.ullTotalVirtual
                End Get
            End Property


            ' Fields
            Private m_IsOldOS As Boolean = (Environment.OSVersion.Version.Major < 5)
            Private m_MemoryStatus As MEMORYSTATUS
            Private m_MemoryStatusEx As MEMORYSTATUSEX
        End Class
    End Class
End Namespace


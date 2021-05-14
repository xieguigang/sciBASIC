#Region "Microsoft.VisualBasic::3c118018626760676a3fc9601c964582, Microsoft.VisualBasic.Core\src\ApplicationServices\Tools\OSVersionInfo.vb"

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

    '     Module OSVersionInfo
    ' 
    ' 
    '         Enum SoftwareArchitecture
    ' 
    ' 
    ' 
    ' 
    '         Enum ProcessorArchitecture
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '         Delegate Function
    ' 
    '             Properties: Edition, OSBits, ProcessorBits, ProgramBits
    '         Enum WindowsNameList
    ' 
    '             null, Windows2000, Windows3_1, Windows7, Windows8
    '             Windows95, Windows95OSR2, Windows98, Windows98SecondEdition, WindowsCE
    '             WindowsMe, WindowsNT3_51, WindowsNT4_0, WindowsNT4_0Server, WindowsServer2003
    '             WindowsServer2008, WindowsServer2008R2, WindowsServer2012, WindowsVista, WindowsXP
    ' 
    ' 
    ' 
    '         Structure OSVERSIONINFOEX
    ' 
    '             Properties: WindowsName
    ' 
    '             Function: GetProductInfo, GetSystemMetrics, GetVersion, GetVersionEx
    ' 
    '             Sub: GetNativeSystemInfo, GetSystemInfo
    ' 
    '         Structure SYSTEM_INFO
    ' 
    ' 
    ' 
    '         Structure _PROCESSOR_INFO_UNION
    ' 
    ' 
    ' 
    '  
    ' 
    '     Properties: BuildVersion, MajorVersion, MinorVersion, RevisionVersion, ServicePack
    '                 Version, VersionString
    ' 
    '     Function: GetIsWow64ProcessDelegate, GetProcAddress, Is32BitProcessOn64BitProcessor, LoadLibrary
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#Region "USINGS"
Imports System.Runtime.InteropServices
Imports System.Runtime.InteropServices.Marshal
#End Region

Namespace ApplicationServices

    ''' <summary>
    ''' Provides detailed information about the host operating system.(用于判断操作系统的具体版本信息的工具)
    ''' </summary>
    Public Module OSVersionInfo

#Region "ENUMS"
        Public Enum SoftwareArchitecture
            Unknown = 0
            Bit32 = 1
            Bit64 = 2
        End Enum

        Public Enum ProcessorArchitecture
            Unknown = 0
            Bit32 = 1
            Bit64 = 2
            Itanium64 = 3
        End Enum
#End Region

#Region "DELEGATE DECLARATION"
        Private Delegate Function IsWow64ProcessDelegate(<[In]> handle As IntPtr, <Out> ByRef isWow64Process As Boolean) As Boolean
#End Region

#Region "BITS"
        ''' <summary>
        ''' Determines if the current application is 32 or 64-bit.
        ''' </summary>
        Public ReadOnly Property ProgramBits() As SoftwareArchitecture
            Get
                Dim pbits As SoftwareArchitecture = SoftwareArchitecture.Unknown

                Dim test As System.Collections.IDictionary = Environment.GetEnvironmentVariables()

                Select Case IntPtr.Size * 8
                    Case 64
                        pbits = SoftwareArchitecture.Bit64


                    Case 32
                        pbits = SoftwareArchitecture.Bit32

                    Case Else

                        pbits = SoftwareArchitecture.Unknown

                End Select

                ' int getOSArchitecture()
                '{
                '    string pa = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
                '    return ((String.IsNullOrEmpty(pa) || String.Compare(pa, 0, "x86", 0, 3, true) == 0) ? 32 : 64);
                '}



                'ProcessorArchitecture pbits = ProcessorArchitecture.Unknown;

                'try
                '{
                '    SYSTEM_INFO l_System_Info = new SYSTEM_INFO();
                '    GetSystemInfo(ref l_System_Info);

                '    switch (l_System_Info.uProcessorInfo.wProcessorArchitecture)
                '    {
                '        case 9: // PROCESSOR_ARCHITECTURE_AMD64
                '            pbits = ProcessorArchitecture.Bit64;
                '            break;
                '        case 6: // PROCESSOR_ARCHITECTURE_IA64
                '            pbits = ProcessorArchitecture.Itanium64;
                '            break;
                '        case 0: // PROCESSOR_ARCHITECTURE_INTEL
                '            pbits = ProcessorArchitecture.Bit32;
                '            break;
                '        default: // PROCESSOR_ARCHITECTURE_UNKNOWN
                '            pbits = ProcessorArchitecture.Unknown;
                '            break;
                '    }
                '}
                'catch
                '{
                '     Ignore
                '}

                'return pbits;
                Return pbits
            End Get
        End Property

        Public ReadOnly Property OSBits() As SoftwareArchitecture
            Get
                Dim osbits__1 As SoftwareArchitecture = SoftwareArchitecture.Unknown

                Select Case IntPtr.Size * 8
                    Case 64
                        osbits__1 = SoftwareArchitecture.Bit64


                    Case 32
                        If Is32BitProcessOn64BitProcessor() Then
                            osbits__1 = SoftwareArchitecture.Bit64
                        Else
                            osbits__1 = SoftwareArchitecture.Bit32
                        End If

                    Case Else

                        osbits__1 = SoftwareArchitecture.Unknown

                End Select

                Return osbits__1
            End Get
        End Property

        ''' <summary>
        ''' Determines if the current processor is 32 or 64-bit.
        ''' </summary>
        Public ReadOnly Property ProcessorBits() As ProcessorArchitecture
            Get
                Dim pbits As ProcessorArchitecture = ProcessorArchitecture.Unknown

                Try
                    Dim l_System_Info As New SYSTEM_INFO()
                    GetNativeSystemInfo(l_System_Info)

                    Select Case l_System_Info.uProcessorInfo.wProcessorArchitecture
                        Case 9
                            ' PROCESSOR_ARCHITECTURE_AMD64
                            pbits = ProcessorArchitecture.Bit64

                        Case 6
                            ' PROCESSOR_ARCHITECTURE_IA64
                            pbits = ProcessorArchitecture.Itanium64

                        Case 0
                            ' PROCESSOR_ARCHITECTURE_INTEL
                            pbits = ProcessorArchitecture.Bit32

                        Case Else
                            ' PROCESSOR_ARCHITECTURE_UNKNOWN
                            pbits = ProcessorArchitecture.Unknown

                    End Select
                    ' Ignore
                Catch
                End Try

                Return pbits
            End Get
        End Property
#End Region

#Region "EDITION"
        Private s_Edition As String
        ''' <summary>
        ''' Gets the edition of the operating system running on this computer.
        ''' </summary>
        Public ReadOnly Property Edition() As String
            Get
                If s_Edition IsNot Nothing Then
                    Return s_Edition
                End If
                '***** RETURN *****//
                Dim edition__1 As String = String.Empty

                Dim osVersion As OperatingSystem = Environment.OSVersion
                Dim osVersionInfo As New OSVERSIONINFOEX()
                osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(GetType(OSVERSIONINFOEX))

                If GetVersion(osVersionInfo) Then
                    Dim majorVersion As Integer = osVersion.Version.Major
                    Dim minorVersion As Integer = osVersion.Version.Minor
                    Dim productType As Byte = osVersionInfo.wProductType
                    Dim suiteMask As Short = osVersionInfo.wSuiteMask

                    '#Region "VERSION 4"
                    If majorVersion = 4 Then
                        If productType = VER_NT_WORKSTATION Then
                            ' Windows NT 4.0 Workstation
                            edition__1 = "Workstation"
                        ElseIf productType = VER_NT_SERVER Then
                            If (suiteMask And VER_SUITE_ENTERPRISE) <> 0 Then
                                ' Windows NT 4.0 Server Enterprise
                                edition__1 = "Enterprise Server"
                            Else
                                ' Windows NT 4.0 Server
                                edition__1 = "Standard Server"
                            End If
                        End If
                        '#End Region

                        '#Region "VERSION 5"
                    ElseIf majorVersion = 5 Then
                        If productType = VER_NT_WORKSTATION Then
                            If (suiteMask And VER_SUITE_PERSONAL) <> 0 Then
                                edition__1 = "Home"
                            Else
                                If GetSystemMetrics(86) = 0 Then
                                    ' 86 == SM_TABLETPC
                                    edition__1 = "Professional"
                                Else
                                    edition__1 = "Tablet Edition"
                                End If
                            End If
                        ElseIf productType = VER_NT_SERVER Then
                            If minorVersion = 0 Then
                                If (suiteMask And VER_SUITE_DATACENTER) <> 0 Then
                                    ' Windows 2000 Datacenter Server
                                    edition__1 = "Datacenter Server"
                                ElseIf (suiteMask And VER_SUITE_ENTERPRISE) <> 0 Then
                                    ' Windows 2000 Advanced Server
                                    edition__1 = "Advanced Server"
                                Else
                                    ' Windows 2000 Server
                                    edition__1 = "Server"
                                End If
                            Else
                                If (suiteMask And VER_SUITE_DATACENTER) <> 0 Then
                                    ' Windows Server 2003 Datacenter Edition
                                    edition__1 = "Datacenter"
                                ElseIf (suiteMask And VER_SUITE_ENTERPRISE) <> 0 Then
                                    ' Windows Server 2003 Enterprise Edition
                                    edition__1 = "Enterprise"
                                ElseIf (suiteMask And VER_SUITE_BLADE) <> 0 Then
                                    ' Windows Server 2003 Web Edition
                                    edition__1 = "Web Edition"
                                Else
                                    ' Windows Server 2003 Standard Edition
                                    edition__1 = "Standard"
                                End If
                            End If
                        End If
                        '#End Region

                        '#Region "VERSION 6"
                    ElseIf majorVersion = 6 Then
                        Dim ed As Integer
                        If GetProductInfo(majorVersion, minorVersion, osVersionInfo.wServicePackMajor, osVersionInfo.wServicePackMinor, ed) Then
                            Select Case ed
                                Case PRODUCT_BUSINESS
                                    edition__1 = "Business"

                                Case PRODUCT_BUSINESS_N
                                    edition__1 = "Business N"

                                Case PRODUCT_CLUSTER_SERVER
                                    edition__1 = "HPC Edition"

                                Case PRODUCT_CLUSTER_SERVER_V
                                    edition__1 = "HPC Edition without Hyper-V"

                                Case PRODUCT_DATACENTER_SERVER
                                    edition__1 = "Datacenter Server"

                                Case PRODUCT_DATACENTER_SERVER_CORE
                                    edition__1 = "Datacenter Server (core installation)"

                                Case PRODUCT_DATACENTER_SERVER_V
                                    edition__1 = "Datacenter Server without Hyper-V"

                                Case PRODUCT_DATACENTER_SERVER_CORE_V
                                    edition__1 = "Datacenter Server without Hyper-V (core installation)"

                                Case PRODUCT_EMBEDDED
                                    edition__1 = "Embedded"

                                Case PRODUCT_ENTERPRISE
                                    edition__1 = "Enterprise"

                                Case PRODUCT_ENTERPRISE_N
                                    edition__1 = "Enterprise N"

                                Case PRODUCT_ENTERPRISE_E
                                    edition__1 = "Enterprise E"

                                Case PRODUCT_ENTERPRISE_SERVER
                                    edition__1 = "Enterprise Server"

                                Case PRODUCT_ENTERPRISE_SERVER_CORE
                                    edition__1 = "Enterprise Server (core installation)"

                                Case PRODUCT_ENTERPRISE_SERVER_CORE_V
                                    edition__1 = "Enterprise Server without Hyper-V (core installation)"

                                Case PRODUCT_ENTERPRISE_SERVER_IA64
                                    edition__1 = "Enterprise Server for Itanium-based Systems"

                                Case PRODUCT_ENTERPRISE_SERVER_V
                                    edition__1 = "Enterprise Server without Hyper-V"

                                Case PRODUCT_ESSENTIALBUSINESS_SERVER_MGMT
                                    edition__1 = "Essential Business Server MGMT"

                                Case PRODUCT_ESSENTIALBUSINESS_SERVER_ADDL
                                    edition__1 = "Essential Business Server ADDL"

                                Case PRODUCT_ESSENTIALBUSINESS_SERVER_MGMTSVC
                                    edition__1 = "Essential Business Server MGMTSVC"

                                Case PRODUCT_ESSENTIALBUSINESS_SERVER_ADDLSVC
                                    edition__1 = "Essential Business Server ADDLSVC"

                                Case PRODUCT_HOME_BASIC
                                    edition__1 = "Home Basic"

                                Case PRODUCT_HOME_BASIC_N
                                    edition__1 = "Home Basic N"

                                Case PRODUCT_HOME_BASIC_E
                                    edition__1 = "Home Basic E"

                                Case PRODUCT_HOME_PREMIUM
                                    edition__1 = "Home Premium"

                                Case PRODUCT_HOME_PREMIUM_N
                                    edition__1 = "Home Premium N"

                                Case PRODUCT_HOME_PREMIUM_E
                                    edition__1 = "Home Premium E"

                                Case PRODUCT_HOME_PREMIUM_SERVER
                                    edition__1 = "Home Premium Server"

                                Case PRODUCT_HYPERV
                                    edition__1 = "Microsoft Hyper-V Server"

                                Case PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT
                                    edition__1 = "Windows Essential Business Management Server"

                                Case PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING
                                    edition__1 = "Windows Essential Business Messaging Server"

                                Case PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY
                                    edition__1 = "Windows Essential Business Security Server"

                                Case PRODUCT_PROFESSIONAL
                                    edition__1 = "Professional"

                                Case PRODUCT_PROFESSIONAL_N
                                    edition__1 = "Professional N"

                                Case PRODUCT_PROFESSIONAL_E
                                    edition__1 = "Professional E"

                                Case PRODUCT_SB_SOLUTION_SERVER
                                    edition__1 = "SB Solution Server"

                                Case PRODUCT_SB_SOLUTION_SERVER_EM
                                    edition__1 = "SB Solution Server EM"

                                Case PRODUCT_SERVER_FOR_SB_SOLUTIONS
                                    edition__1 = "Server for SB Solutions"

                                Case PRODUCT_SERVER_FOR_SB_SOLUTIONS_EM
                                    edition__1 = "Server for SB Solutions EM"

                                Case PRODUCT_SERVER_FOR_SMALLBUSINESS
                                    edition__1 = "Windows Essential Server Solutions"

                                Case PRODUCT_SERVER_FOR_SMALLBUSINESS_V
                                    edition__1 = "Windows Essential Server Solutions without Hyper-V"

                                Case PRODUCT_SERVER_FOUNDATION
                                    edition__1 = "Server Foundation"

                                Case PRODUCT_SMALLBUSINESS_SERVER
                                    edition__1 = "Windows Small Business Server"

                                Case PRODUCT_SMALLBUSINESS_SERVER_PREMIUM
                                    edition__1 = "Windows Small Business Server Premium"

                                Case PRODUCT_SMALLBUSINESS_SERVER_PREMIUM_CORE
                                    edition__1 = "Windows Small Business Server Premium (core installation)"

                                Case PRODUCT_SOLUTION_EMBEDDEDSERVER
                                    edition__1 = "Solution Embedded Server"

                                Case PRODUCT_SOLUTION_EMBEDDEDSERVER_CORE
                                    edition__1 = "Solution Embedded Server (core installation)"

                                Case PRODUCT_STANDARD_SERVER
                                    edition__1 = "Standard Server"

                                Case PRODUCT_STANDARD_SERVER_CORE
                                    edition__1 = "Standard Server (core installation)"

                                Case PRODUCT_STANDARD_SERVER_SOLUTIONS
                                    edition__1 = "Standard Server Solutions"

                                Case PRODUCT_STANDARD_SERVER_SOLUTIONS_CORE
                                    edition__1 = "Standard Server Solutions (core installation)"

                                Case PRODUCT_STANDARD_SERVER_CORE_V
                                    edition__1 = "Standard Server without Hyper-V (core installation)"

                                Case PRODUCT_STANDARD_SERVER_V
                                    edition__1 = "Standard Server without Hyper-V"

                                Case PRODUCT_STARTER
                                    edition__1 = "Starter"

                                Case PRODUCT_STARTER_N
                                    edition__1 = "Starter N"

                                Case PRODUCT_STARTER_E
                                    edition__1 = "Starter E"

                                Case PRODUCT_STORAGE_ENTERPRISE_SERVER
                                    edition__1 = "Enterprise Storage Server"

                                Case PRODUCT_STORAGE_ENTERPRISE_SERVER_CORE
                                    edition__1 = "Enterprise Storage Server (core installation)"

                                Case PRODUCT_STORAGE_EXPRESS_SERVER
                                    edition__1 = "Express Storage Server"

                                Case PRODUCT_STORAGE_EXPRESS_SERVER_CORE
                                    edition__1 = "Express Storage Server (core installation)"

                                Case PRODUCT_STORAGE_STANDARD_SERVER
                                    edition__1 = "Standard Storage Server"

                                Case PRODUCT_STORAGE_STANDARD_SERVER_CORE
                                    edition__1 = "Standard Storage Server (core installation)"

                                Case PRODUCT_STORAGE_WORKGROUP_SERVER
                                    edition__1 = "Workgroup Storage Server"

                                Case PRODUCT_STORAGE_WORKGROUP_SERVER_CORE
                                    edition__1 = "Workgroup Storage Server (core installation)"

                                Case PRODUCT_UNDEFINED
                                    edition__1 = "Unknown product"

                                Case PRODUCT_ULTIMATE
                                    edition__1 = "Ultimate"

                                Case PRODUCT_ULTIMATE_N
                                    edition__1 = "Ultimate N"

                                Case PRODUCT_ULTIMATE_E
                                    edition__1 = "Ultimate E"

                                Case PRODUCT_WEB_SERVER
                                    edition__1 = "Web Server"

                                Case PRODUCT_WEB_SERVER_CORE
                                    edition__1 = "Web Server (core installation)"

                            End Select
                        End If
                        '#End Region
                    End If
                End If

                s_Edition = edition__1
                Return edition__1
            End Get
        End Property
#End Region

#Region "NAME"

        Public Enum WindowsNameList
            ''' <summary>
            ''' Linux/MAC
            ''' </summary>
            null

            ''' <summary>
            ''' Windows 3.1
            ''' </summary>
            Windows3_1
            ''' <summary>
            ''' Windows CE
            ''' </summary>
            WindowsCE
            ''' <summary>
            ''' Windows 95 OSR2
            ''' </summary>
            Windows95OSR2
            ''' <summary>
            ''' Windows 95
            ''' </summary>
            Windows95
            ''' <summary>
            ''' Windows 98 Second Edition
            ''' </summary>
            Windows98SecondEdition
            ''' <summary>
            ''' Windows 98
            ''' </summary>
            Windows98
            ''' <summary>
            ''' Windows Me
            ''' </summary>
            WindowsMe
            ''' <summary>
            ''' Windows NT 3.51
            ''' </summary>
            WindowsNT3_51
            ''' <summary>
            ''' Windows NT 4.0
            ''' </summary>
            WindowsNT4_0
            ''' <summary>
            ''' Windows NT 4.0 Server
            ''' </summary>
            WindowsNT4_0Server
            ''' <summary>
            ''' Windows 2000
            ''' </summary>
            Windows2000
            ''' <summary>
            ''' Windows XP
            ''' </summary>
            WindowsXP
            ''' <summary>
            ''' Windows Server 2003
            ''' </summary>
            WindowsServer2003
            ''' <summary>
            ''' Windows Vista
            ''' </summary>
            WindowsVista
            ''' <summary>
            ''' Windows Server 2008
            ''' </summary>
            WindowsServer2008
            ''' <summary>
            ''' Windows 7
            ''' </summary>
            Windows7
            ''' <summary>
            ''' Windows Server 2008 R2
            ''' </summary>
            WindowsServer2008R2
            ''' <summary>
            ''' Windows 8
            ''' </summary>
            Windows8
            ''' <summary>
            ''' Windows Server 2012
            ''' </summary>
            WindowsServer2012
        End Enum

        Private s_Name As WindowsNameList
        ''' <summary>
        ''' Gets the name of the operating system running on this computer.
        ''' </summary>
        Public ReadOnly Property WindowsName() As WindowsNameList
            Get
                If s_Name <> WindowsNameList.null Then
                    Return s_Name
                End If

                '***** RETURN *****//

                Dim osVersion As OperatingSystem = Environment.OSVersion
                Dim osVersionInfo As New OSVERSIONINFOEX()
                osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(GetType(OSVERSIONINFOEX))

                If GetVersion(osVersionInfo) Then
                    Dim majorVersion As Integer = osVersion.Version.Major
                    Dim minorVersion As Integer = osVersion.Version.Minor

                    Select Case osVersion.Platform
                        Case PlatformID.Win32S
                            s_Name = WindowsNameList.Windows3_1 '    name__1 = "Windows 3.1"

                        Case PlatformID.WinCE
                            s_Name = WindowsNameList.WindowsCE '      name__1 = "Windows CE"

                        Case PlatformID.Win32Windows
                            If True Then
                                If majorVersion = 4 Then
                                    Dim csdVersion As String = osVersionInfo.szCSDVersion
                                    Select Case minorVersion
                                        Case 0
                                            If csdVersion = "B" OrElse csdVersion = "C" Then
                                                s_Name = WindowsNameList.Windows95OSR2 '       name__1 = "Windows 95 OSR2"
                                            Else
                                                s_Name = WindowsNameList.Windows95 '          name__1 = "Windows 95"
                                            End If

                                        Case 10
                                            If csdVersion = "A" Then
                                                s_Name = WindowsNameList.Windows98SecondEdition '            name__1 = "Windows 98 Second Edition"
                                            Else
                                                s_Name = WindowsNameList.Windows98 '      name__1 = "Windows 98"
                                            End If

                                        Case 90
                                            s_Name = WindowsNameList.WindowsMe '     name__1 = "Windows Me"

                                    End Select
                                End If

                            End If
                        Case PlatformID.Win32NT
                            If True Then
                                Dim productType As Byte = osVersionInfo.wProductType

                                Select Case majorVersion
                                    Case 3
                                        s_Name = WindowsNameList.WindowsNT3_51 '    name__1 = "Windows NT 3.51"

                                    Case 4
                                        Select Case productType
                                            Case 1
                                                s_Name = WindowsNameList.WindowsNT4_0 '        name__1 = "Windows NT 4.0"

                                            Case 3
                                                s_Name = WindowsNameList.WindowsNT4_0Server '     name__1 = "Windows NT 4.0 Server"

                                        End Select

                                    Case 5
                                        Select Case minorVersion
                                            Case 0
                                                s_Name = WindowsNameList.Windows2000 '        name__1 = "Windows 2000"

                                            Case 1
                                                s_Name = WindowsNameList.WindowsXP '       name__1 = "Windows XP"

                                            Case 2
                                                s_Name = WindowsNameList.WindowsServer2003 '       name__1 = "Windows Server 2003"

                                        End Select

                                    Case 6
                                        Select Case minorVersion
                                            Case 0
                                                Select Case productType
                                                    Case 1
                                                        s_Name = WindowsNameList.WindowsVista '                                                    name__1 = "Windows Vista"

                                                    Case 3
                                                        s_Name = WindowsNameList.WindowsServer2008 '    name__1 = "Windows Server 2008"

                                                End Select


                                            Case 1
                                                Select Case productType
                                                    Case 1
                                                        s_Name = WindowsNameList.Windows7 '    name__1 = "Windows 7"

                                                    Case 3
                                                        s_Name = WindowsNameList.WindowsServer2008R2 '   name__1 = "Windows Server 2008 R2"

                                                End Select

                                            Case 2
                                                Select Case productType
                                                    Case 1
                                                        s_Name = WindowsNameList.Windows8 '      name__1 = "Windows 8"

                                                    Case 3
                                                        s_Name = WindowsNameList.WindowsServer2012 '        name__1 = "Windows Server 2012"

                                                End Select

                                        End Select

                                End Select

                            End If
                    End Select

                Else

                    s_Name = WindowsNameList.null

                End If

                Return s_Name
            End Get
        End Property
#End Region

#Region "PINVOKE"

#Region "GET"
#Region "PRODUCT INFO"
        <DllImport("Kernel32.dll")>
        Friend Function GetProductInfo(osMajorVersion As Integer, osMinorVersion As Integer, spMajorVersion As Integer, spMinorVersion As Integer, ByRef edition As Integer) As Boolean
        End Function
#End Region

#Region "VERSION"

        ''' <summary>
        ''' ##### GetVersionEx Function
        ''' 
        ''' _GetVersionEx may be altered Or unavailable for releases after Windows 8.1. Instead, use the Version Helper functions_
        ''' 
        ''' With the release Of Windows 8.1, the behavior Of the GetVersionEx API has changed In the value it will Return For the 
        ''' operating system version. The value returned by the GetVersionEx Function now depends On how the application Is 
        ''' manifested.
        ''' Applications Not manifested for Windows 8.1 Or Windows 10 will return the Windows 8 OS version value (6.2). Once an 
        ''' application Is manifested for a given operating system version, GetVersionEx will always return the version that the 
        ''' application Is manifested for in future releases. To manifest your applications for Windows 8.1 Or Windows 10, refer 
        ''' to Targeting your application for Windows.
        ''' 
        ''' **Syntax**
        ''' 
        ''' ```C
        ''' BOOL WINAPI GetVersionEx(
        '''   _Inout_ LPOSVERSIONINFO lpVersionInfo
        ''' );
        ''' ```
        ''' 
        ''' </summary>
        ''' <param name="osVersionInfo">An OSVersionInfo Or OSVERSIONINFOEX structure that receives the operating system information.
        ''' Before calling the GetVersionEx Function, set the dwOSVersionInfoSize member of the structure as appropriate to indicate 
        ''' which data structure Is being passed to this function.
        ''' </param>
        ''' <returns>If the Then Function succeeds, the Return value Is a nonzero value.
        ''' If the Then Function fails, the Return value Is zero. To Get extended Error information, Call GetLastError. 
        ''' The Function fails If you specify an invalid value For the dwOSVersionInfoSize member Of the OSVERSIONINFO 
        ''' Or OSVERSIONINFOEX Structure.
        ''' </returns>
        ''' <remarks>
        ''' Identifying the current operating system Is usually Not the best way To determine whether a particular operating system 
        ''' feature Is present. This Is because the operating system may have had New features added In a redistributable DLL. Rather 
        ''' than Using GetVersionEx To determine the operating system platform Or version number, test For the presence Of the feature 
        ''' itself. For more information, see Operating System Version.
        ''' The GetSystemMetrics function provides additional information about the current operating system.
        ''' 
        ''' |Product|Setting|
        ''' |-------|-------|
        ''' |Windows XP Media Center Edition|SM_MEDIACENTER|
        ''' |Windows XP Starter Edition|SM_STARTER|
        ''' |Windows XP Tablet PC Edition|SM_TABLETPC|
        ''' |Windows Server 2003 R2|SM_SERVERR2|
        '''
        ''' To check for specific operating systems Or operating system features, use the IsOS function. The GetProductInfo function retrieves the product type.
        ''' To retrieve information for the operating system on a remote computer, use the NetWkstaGetInfo function, the Win32_OperatingSystem WMI class, Or the OperatingSystem property of the IADsComputer interface.
        ''' To compare the current system version to a required version, use the VerifyVersionInfo function instead of using GetVersionEx to perform the comparison yourself.
        ''' 
        ''' If compatibility Then mode Is In effect, the GetVersionEx Function reports the operating system As it identifies itself, which may Not 
        ''' be the operating system that Is installed. For example, If compatibility mode Is In effect, GetVersionEx reports the operating system 
        ''' that Is selected For application compatibility.
        ''' 
        ''' **Examples**
        '''
        ''' When using the GetVersionEx function to determine whether your application Is running on a particular version of the operating system, 
        ''' check for version numbers that are greater than Or equal to the desired version numbers. This ensures that the test succeeds for later 
        ''' versions of the operating system. For example, if your application requires Windows XP Or later, use the following test.
        ''' 
        ''' ```C
        ''' #include &lt;windows.h>
        ''' #include &lt;stdio.h>
        '''
        ''' Void main()
        ''' {
        '''     OSVersionInfo osvi;
        '''     BOOL bIsWindowsXPorLater;
        '''
        '''     ZeroMemory(&amp;osvi, SizeOf(OSVersionInfo));
        '''     osvi.dwOSVersionInfoSize = SizeOf(OSVersionInfo);
        '''
        '''     GetVersionEx(&amp;osvi);
        '''
        '''     bIsWindowsXPorLater =
        '''        ((osvi.dwMajorVersion > 5) ||
        '''        ((osvi.dwMajorVersion == 5) &amp;&amp; (osvi.dwMinorVersion >= 1) ));
        ''' 
        '''     if(bIsWindowsXPorLater) 
        '''         printf("The system meets the requirements.\n");
        '''     else printf("The system does not meet the requirements.\n");
        ''' }
        ''' ```
        '''
        ''' For an example that identifies the current operating system, see Getting the System Version.
        ''' 
        ''' **Requirements**
        '''
        ''' | | |
        ''' |-|-|
        ''' |Minimum supported client|Windows 2000 Professional [desktop apps only]|
        ''' |Minimum supported server|Windows 2000 Server [desktop apps only]|
        ''' |Header|Winbase.h(include Windows.h)|
        ''' |Library|Kernel32.lib|
        ''' |DLL|Kernel32.dll|
        ''' |Unicode And ANSI names|GetVersionExW(Unicode) And GetVersionExA (ANSI)|
        ''' </remarks>
        <DllImport("kernel32.dll")>
        Private Function GetVersionEx(ByRef osVersionInfo As OSVERSIONINFOEX) As Boolean
        End Function

        Private Function GetVersion(ByRef osVersionInfo As OSVERSIONINFOEX) As Boolean
            If Environment.OSVersion.Platform = PlatformID.Unix OrElse
                Environment.OSVersion.Platform = PlatformID.MacOSX Then
                Return False
            Else
                Return GetVersionEx(osVersionInfo)
            End If
        End Function
#End Region

#Region "SYSTEMMETRICS"
        <DllImport("user32")>
        Public Function GetSystemMetrics(nIndex As Integer) As Integer
        End Function
#End Region

#Region "SYSTEMINFO"
        <DllImport("kernel32.dll")>
        Public Sub GetSystemInfo(<MarshalAs(UnmanagedType.Struct)> ByRef lpSystemInfo As SYSTEM_INFO)
        End Sub

        <DllImport("kernel32.dll")>
        Public Sub GetNativeSystemInfo(<MarshalAs(UnmanagedType.Struct)> ByRef lpSystemInfo As SYSTEM_INFO)
        End Sub
#End Region

#End Region

#Region "OSVERSIONINFOEX"
        <StructLayout(LayoutKind.Sequential)>
        Public Structure OSVERSIONINFOEX
            Public dwOSVersionInfoSize As Integer
            Public dwMajorVersion As Integer
            Public dwMinorVersion As Integer
            Public dwBuildNumber As Integer
            Public dwPlatformId As Integer
            <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=128)>
            Public szCSDVersion As String
            Public wServicePackMajor As Short
            Public wServicePackMinor As Short
            Public wSuiteMask As Short
            Public wProductType As Byte
            Public wReserved As Byte
        End Structure
#End Region

#Region "SYSTEM_INFO"
        <StructLayout(LayoutKind.Sequential)>
        Public Structure SYSTEM_INFO
            Friend uProcessorInfo As _PROCESSOR_INFO_UNION
            Public dwPageSize As UInteger
            Public lpMinimumApplicationAddress As IntPtr
            Public lpMaximumApplicationAddress As IntPtr
            Public dwActiveProcessorMask As IntPtr
            Public dwNumberOfProcessors As UInteger
            Public dwProcessorType As UInteger
            Public dwAllocationGranularity As UInteger
            Public dwProcessorLevel As UShort
            Public dwProcessorRevision As UShort
        End Structure
#End Region

#Region "_PROCESSOR_INFO_UNION"
        <StructLayout(LayoutKind.Explicit)>
        Public Structure _PROCESSOR_INFO_UNION
            <FieldOffset(0)>
            Friend dwOemId As UInteger
            <FieldOffset(0)>
            Friend wProcessorArchitecture As UShort
            <FieldOffset(2)>
            Friend wReserved As UShort
        End Structure
#End Region

#Region "64 BIT OS DETECTION"
        <DllImport("kernel32", SetLastError:=True, CallingConvention:=CallingConvention.Winapi)>
        Public Function LoadLibrary(libraryName As String) As IntPtr
        End Function

        <DllImport("kernel32", SetLastError:=True, CallingConvention:=CallingConvention.Winapi)>
        Public Function GetProcAddress(hwnd As IntPtr, procedureName As String) As IntPtr
        End Function
#End Region

#Region "PRODUCT"
        Private Const PRODUCT_UNDEFINED As Integer = &H0
        Private Const PRODUCT_ULTIMATE As Integer = &H1
        Private Const PRODUCT_HOME_BASIC As Integer = &H2
        Private Const PRODUCT_HOME_PREMIUM As Integer = &H3
        Private Const PRODUCT_ENTERPRISE As Integer = &H4
        Private Const PRODUCT_HOME_BASIC_N As Integer = &H5
        Private Const PRODUCT_BUSINESS As Integer = &H6
        Private Const PRODUCT_STANDARD_SERVER As Integer = &H7
        Private Const PRODUCT_DATACENTER_SERVER As Integer = &H8
        Private Const PRODUCT_SMALLBUSINESS_SERVER As Integer = &H9
        Private Const PRODUCT_ENTERPRISE_SERVER As Integer = &HA
        Private Const PRODUCT_STARTER As Integer = &HB
        Private Const PRODUCT_DATACENTER_SERVER_CORE As Integer = &HC
        Private Const PRODUCT_STANDARD_SERVER_CORE As Integer = &HD
        Private Const PRODUCT_ENTERPRISE_SERVER_CORE As Integer = &HE
        Private Const PRODUCT_ENTERPRISE_SERVER_IA64 As Integer = &HF
        Private Const PRODUCT_BUSINESS_N As Integer = &H10
        Private Const PRODUCT_WEB_SERVER As Integer = &H11
        Private Const PRODUCT_CLUSTER_SERVER As Integer = &H12
        Private Const PRODUCT_HOME_SERVER As Integer = &H13
        Private Const PRODUCT_STORAGE_EXPRESS_SERVER As Integer = &H14
        Private Const PRODUCT_STORAGE_STANDARD_SERVER As Integer = &H15
        Private Const PRODUCT_STORAGE_WORKGROUP_SERVER As Integer = &H16
        Private Const PRODUCT_STORAGE_ENTERPRISE_SERVER As Integer = &H17
        Private Const PRODUCT_SERVER_FOR_SMALLBUSINESS As Integer = &H18
        Private Const PRODUCT_SMALLBUSINESS_SERVER_PREMIUM As Integer = &H19
        Private Const PRODUCT_HOME_PREMIUM_N As Integer = &H1A
        Private Const PRODUCT_ENTERPRISE_N As Integer = &H1B
        Private Const PRODUCT_ULTIMATE_N As Integer = &H1C
        Private Const PRODUCT_WEB_SERVER_CORE As Integer = &H1D
        Private Const PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT As Integer = &H1E
        Private Const PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY As Integer = &H1F
        Private Const PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING As Integer = &H20
        Private Const PRODUCT_SERVER_FOUNDATION As Integer = &H21
        Private Const PRODUCT_HOME_PREMIUM_SERVER As Integer = &H22
        Private Const PRODUCT_SERVER_FOR_SMALLBUSINESS_V As Integer = &H23
        Private Const PRODUCT_STANDARD_SERVER_V As Integer = &H24
        Private Const PRODUCT_DATACENTER_SERVER_V As Integer = &H25
        Private Const PRODUCT_ENTERPRISE_SERVER_V As Integer = &H26
        Private Const PRODUCT_DATACENTER_SERVER_CORE_V As Integer = &H27
        Private Const PRODUCT_STANDARD_SERVER_CORE_V As Integer = &H28
        Private Const PRODUCT_ENTERPRISE_SERVER_CORE_V As Integer = &H29
        Private Const PRODUCT_HYPERV As Integer = &H2A
        Private Const PRODUCT_STORAGE_EXPRESS_SERVER_CORE As Integer = &H2B
        Private Const PRODUCT_STORAGE_STANDARD_SERVER_CORE As Integer = &H2C
        Private Const PRODUCT_STORAGE_WORKGROUP_SERVER_CORE As Integer = &H2D
        Private Const PRODUCT_STORAGE_ENTERPRISE_SERVER_CORE As Integer = &H2E
        Private Const PRODUCT_STARTER_N As Integer = &H2F
        Private Const PRODUCT_PROFESSIONAL As Integer = &H30
        Private Const PRODUCT_PROFESSIONAL_N As Integer = &H31
        Private Const PRODUCT_SB_SOLUTION_SERVER As Integer = &H32
        Private Const PRODUCT_SERVER_FOR_SB_SOLUTIONS As Integer = &H33
        Private Const PRODUCT_STANDARD_SERVER_SOLUTIONS As Integer = &H34
        Private Const PRODUCT_STANDARD_SERVER_SOLUTIONS_CORE As Integer = &H35
        Private Const PRODUCT_SB_SOLUTION_SERVER_EM As Integer = &H36
        Private Const PRODUCT_SERVER_FOR_SB_SOLUTIONS_EM As Integer = &H37
        Private Const PRODUCT_SOLUTION_EMBEDDEDSERVER As Integer = &H38
        Private Const PRODUCT_SOLUTION_EMBEDDEDSERVER_CORE As Integer = &H39
        'private const int ???? = 0x0000003A;
        Private Const PRODUCT_ESSENTIALBUSINESS_SERVER_MGMT As Integer = &H3B
        Private Const PRODUCT_ESSENTIALBUSINESS_SERVER_ADDL As Integer = &H3C
        Private Const PRODUCT_ESSENTIALBUSINESS_SERVER_MGMTSVC As Integer = &H3D
        Private Const PRODUCT_ESSENTIALBUSINESS_SERVER_ADDLSVC As Integer = &H3E
        Private Const PRODUCT_SMALLBUSINESS_SERVER_PREMIUM_CORE As Integer = &H3F
        Private Const PRODUCT_CLUSTER_SERVER_V As Integer = &H40
        Private Const PRODUCT_EMBEDDED As Integer = &H41
        Private Const PRODUCT_STARTER_E As Integer = &H42
        Private Const PRODUCT_HOME_BASIC_E As Integer = &H43
        Private Const PRODUCT_HOME_PREMIUM_E As Integer = &H44
        Private Const PRODUCT_PROFESSIONAL_E As Integer = &H45
        Private Const PRODUCT_ENTERPRISE_E As Integer = &H46
        Private Const PRODUCT_ULTIMATE_E As Integer = &H47
        'private const int PRODUCT_UNLICENSED = 0xABCDABCD;
#End Region

#Region "VERSIONS"
        Private Const VER_NT_WORKSTATION As Integer = 1
        Private Const VER_NT_DOMAIN_CONTROLLER As Integer = 2
        Private Const VER_NT_SERVER As Integer = 3
        Private Const VER_SUITE_SMALLBUSINESS As Integer = 1
        Private Const VER_SUITE_ENTERPRISE As Integer = 2
        Private Const VER_SUITE_TERMINAL As Integer = 16
        Private Const VER_SUITE_DATACENTER As Integer = 128
        Private Const VER_SUITE_SINGLEUSERTS As Integer = 256
        Private Const VER_SUITE_PERSONAL As Integer = 512
        Private Const VER_SUITE_BLADE As Integer = 1024
#End Region

#End Region

#Region "SERVICE PACK"
        ''' <summary>
        ''' Gets the service pack information of the operating system running on this computer.
        ''' </summary>
        Public ReadOnly Property ServicePack() As String
            Get
                Dim servicePack__1 As String = String.Empty
                Dim osVersionInfo As New OSVERSIONINFOEX()

                osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(GetType(OSVERSIONINFOEX))

                If GetVersion(osVersionInfo) Then
                    servicePack__1 = osVersionInfo.szCSDVersion
                End If

                Return servicePack__1
            End Get
        End Property
#End Region

#Region "VERSION"
#Region "BUILD"
        ''' <summary>
        ''' Gets the build version number of the operating system running on this computer.
        ''' </summary>
        Public ReadOnly Property BuildVersion() As Integer
            Get
                Return Environment.OSVersion.Version.Build
            End Get
        End Property
#End Region

#Region "FULL"
#Region "STRING"
        ''' <summary>
        ''' Gets the full version string of the operating system running on this computer.
        ''' </summary>
        Public ReadOnly Property VersionString() As String
            Get
                Return Environment.OSVersion.Version.ToString()
            End Get
        End Property
#End Region

#Region "VERSION"
        ''' <summary>
        ''' Gets the full version of the operating system running on this computer.
        ''' </summary>
        Public ReadOnly Property Version() As Version
            Get
                Return Environment.OSVersion.Version
            End Get
        End Property
#End Region
#End Region

#Region "MAJOR"
        ''' <summary>
        ''' Gets the major version number of the operating system running on this computer.
        ''' </summary>
        Public ReadOnly Property MajorVersion() As Integer
            Get
                Return Environment.OSVersion.Version.Major
            End Get
        End Property
#End Region

#Region "MINOR"
        ''' <summary>
        ''' Gets the minor version number of the operating system running on this computer.
        ''' </summary>
        Public ReadOnly Property MinorVersion() As Integer
            Get
                Return Environment.OSVersion.Version.Minor
            End Get
        End Property
#End Region

#Region "REVISION"
        ''' <summary>
        ''' Gets the revision version number of the operating system running on this computer.
        ''' </summary>
        Public ReadOnly Property RevisionVersion() As Integer
            Get
                Return Environment.OSVersion.Version.Revision
            End Get
        End Property
#End Region
#End Region

#Region "64 BIT OS DETECTION"
        Private Function GetIsWow64ProcessDelegate() As IsWow64ProcessDelegate
            Dim handle As IntPtr = LoadLibrary("kernel32")

            If handle <> IntPtr.Zero Then
                Dim fnPtr As IntPtr = GetProcAddress(handle, "IsWow64Process")

                If fnPtr <> IntPtr.Zero Then
                    Return DirectCast(GetDelegateForFunctionPointer(CType(fnPtr, IntPtr), GetType(IsWow64ProcessDelegate)), IsWow64ProcessDelegate)
                End If
            End If

            Return Nothing
        End Function

        Private Function Is32BitProcessOn64BitProcessor() As Boolean
            Dim fnDelegate As IsWow64ProcessDelegate = GetIsWow64ProcessDelegate()

            If fnDelegate Is Nothing Then
                Return False
            End If

            Dim isWow64 As Boolean
            Dim retVal As Boolean = fnDelegate.Invoke(Process.GetCurrentProcess().Handle, isWow64)

            If retVal = False Then
                Return False
            End If

            Return isWow64
        End Function
#End Region
    End Module
End Namespace

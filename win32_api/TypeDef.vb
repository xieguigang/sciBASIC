#Region "Microsoft.VisualBasic::c24f14d8ea19263a9df95f32ddf3a628, ..\visualbasic_App\Win32API\TypeDef.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

''' <summary>
''' Win32 API type definitions
''' </summary>
Public Module TypeDef
    ' ------------------------------------------------------------------------
    '
    '    WIN32API.TXT -- Win32 API Declarations for Visual Basic
    '
    '              Copyright (C) 1994 Microsoft Corporation
    '
    '
    '  This file contains only the Const, Type,
    ' and Public Declare statements for  Win32 APIs.
    '
    '  You have a royalty-free right to use, modify, reproduce and distribute
    '  this file (and/or any modified version) in any way you find useful,
    '  provided that you agree that Microsoft has no warranty, obligation or
    '  liability for its contents.  Refer to the Microsoft Windows Programmer's
    '  Reference for further information.
    '
    ' ------------------------------------------------------------------------

    ' Type definitions for Windows' basic types.
    Public Const ANYSIZE_ARRAY As Short = 1
    Public Structure RECT
        'UPGRADE_NOTE: Left ???? Left_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
        Dim Left_Renamed As Integer
        Dim Top As Integer
        'UPGRADE_NOTE: Right ???? Right_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
        Dim Right_Renamed As Integer
        Dim Bottom As Integer
    End Structure

    Public Structure RECTL
        'UPGRADE_NOTE: Left ???? Left_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
        Dim Left_Renamed As Integer
        Dim Top As Integer
        'UPGRADE_NOTE: Right ???? Right_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
        Dim Right_Renamed As Integer
        Dim Bottom As Integer
    End Structure

    Public Structure POINTAPI
        Dim X As Integer
        Dim Y As Integer
    End Structure

    Public Structure POINTL
        Dim X As Integer
        Dim Y As Integer
    End Structure

    Public Structure Size
        Dim cx As Integer
        Dim cy As Integer
    End Structure

    Public Structure POINTS
        Dim X As Short
        Dim Y As Short
    End Structure

    Public Structure Msg
        Dim hWnd As Integer
        Dim message As Integer
        Dim wParam As Integer
        Dim lParam As Integer
        Dim time As Integer
        Dim pt As POINTAPI
    End Structure


    Public Const DELETE As Integer = &H10000
    Public Const READ_CONTROL As Integer = &H20000
    Public Const WRITE_DAC As Integer = &H40000
    Public Const WRITE_OWNER As Integer = &H80000
    Public Const SYNCHRONIZE As Integer = &H100000


    Public Const STANDARD_RIGHTS_READ As Integer = (READ_CONTROL)
    Public Const STANDARD_RIGHTS_WRITE As Integer = (READ_CONTROL)
    Public Const STANDARD_RIGHTS_EXECUTE As Integer = (READ_CONTROL)
    Public Const STANDARD_RIGHTS_REQUIRED As Integer = &HF0000
    Public Const STANDARD_RIGHTS_ALL As Integer = &H1F0000

    Public Const SPECIFIC_RIGHTS_ALL As Short = &HFFFFS


    Public Structure SID_IDENTIFIER_AUTHORITY
        <VBFixedArray(6)> Dim Value() As Byte

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim Value(6)
        End Sub
    End Structure

    Public Const SID_REVISION As Short = (1) '  Current revision level
    Public Const SID_MAX_SUB_AUTHORITIES As Short = (15)
    Public Const SID_RECOMMENDED_SUB_AUTHORITIES As Short = (1) ' Will change to around 6 in a future release.

    Public Const SidTypeUser As Short = 1
    Public Const SidTypeGroup As Short = 2
    Public Const SidTypeDomain As Short = 3
    Public Const SidTypeAlias As Short = 4
    Public Const SidTypeWellKnownGroup As Short = 5
    Public Const SidTypeDeletedAccount As Short = 6
    Public Const SidTypeInvalid As Short = 7
    Public Const SidTypeUnknown As Short = 8

    Public Structure SID_AND_ATTRIBUTES
        Dim Sid As Integer
        Dim Attributes As Integer
    End Structure

    ' ///////////////////////////////////////////////////////////////////////////
    '                                                                          //
    '  Universal well-known SIDs                                               //
    '                                                                          //
    '      Null SID              S-1-0-0                                       //
    '      World                 S-1-1-0                                       //
    '      Local                 S-1-2-0                                       //
    '      Creator Owner ID      S-1-3-0                                       //
    '      Creator Group ID      S-1-3-1                                       //
    '                                                                          //
    '      (Non-unique IDs)      S-1-4                                         //
    '                                                                          //
    ' ///////////////////////////////////////////////////////////////////////////
    Public Const SECURITY_NULL_RID As Short = &H0S
    Public Const SECURITY_WORLD_RID As Short = &H0S
    Public Const SECURITY_LOCAL_RID As Short = &H0S

    Public Const SECURITY_CREATOR_OWNER_RID As Short = &H0S
    Public Const SECURITY_CREATOR_GROUP_RID As Short = &H1S


    ' ///////////////////////////////////////////////////////////////////////////
    '                                                                          //
    '  NT well-known SIDs                                                      //
    '                                                                          //
    '      NT Authority          S-1-5                                         //
    '      Dialup                S-1-5-1                                       //
    '                                                                          //
    '      Network               S-1-5-2                                       //
    '      Batch                 S-1-5-3                                       //
    '      Interactive           S-1-5-4                                       //
    '      Service               S-1-5-6                                       //
    '      AnonymousLogon        S-1-5-7       (aka null logon session)        //
    '                                                                          //
    '      (Logon IDs)           S-1-5-5-X-Y                                   //
    '                                                                          //
    '      (NT non-unique IDs)   S-1-5-0x15-...                                //
    '                                                                          //
    '      (Built-in domain)     s-1-5-0x20                                    //
    '                                                                          //
    ' ///////////////////////////////////////////////////////////////////////////

    Public Const SECURITY_DIALUP_RID As Short = &H1S
    Public Const SECURITY_NETWORK_RID As Short = &H2S
    Public Const SECURITY_BATCH_RID As Short = &H3S
    Public Const SECURITY_INTERACTIVE_RID As Short = &H4S
    Public Const SECURITY_SERVICE_RID As Short = &H6S
    Public Const SECURITY_ANONYMOUS_LOGON_RID As Short = &H7S
    Public Const SECURITY_LOGON_IDS_RID As Short = &H5S
    Public Const SECURITY_LOCAL_SYSTEM_RID As Short = &H12S
    Public Const SECURITY_NT_NON_UNIQUE As Short = &H15S
    Public Const SECURITY_BUILTIN_DOMAIN_RID As Short = &H20S


    ' ///////////////////////////////////////////////////////////////////////////
    '                                                                          //
    '  well-known domain relative sub-authority values (RIDs)...               //
    '                                                                          //
    ' ///////////////////////////////////////////////////////////////////////////

    Public Const DOMAIN_USER_RID_ADMIN As Short = &H1F4S
    Public Const DOMAIN_USER_RID_GUEST As Short = &H1F5S

    Public Const DOMAIN_GROUP_RID_ADMINS As Short = &H200S
    Public Const DOMAIN_GROUP_RID_USERS As Short = &H201S
    Public Const DOMAIN_GROUP_RID_GUESTS As Short = &H202S


    Public Const DOMAIN_ALIAS_RID_ADMINS As Short = &H220S
    Public Const DOMAIN_ALIAS_RID_USERS As Short = &H221S
    Public Const DOMAIN_ALIAS_RID_GUESTS As Short = &H222S
    Public Const DOMAIN_ALIAS_RID_POWER_USERS As Short = &H223S
    Public Const DOMAIN_ALIAS_RID_ACCOUNT_OPS As Short = &H224S
    Public Const DOMAIN_ALIAS_RID_SYSTEM_OPS As Short = &H225S
    Public Const DOMAIN_ALIAS_RID_PRINT_OPS As Short = &H226S
    Public Const DOMAIN_ALIAS_RID_BACKUP_OPS As Short = &H227S
    Public Const DOMAIN_ALIAS_RID_REPLICATOR As Short = &H228S


    '  Allocate the System Luid.  The first 1000 LUIDs are reserved.
    '  Use #999 here0x3E7 = 999)

    '  end_ntifs

    ' //////////////////////////////////////////////////////////////////////
    '                                                                     //
    '                           User and Group related SID attributes     //
    '                                                                     //
    ' //////////////////////////////////////////////////////////////////////

    '  Group attributes

    Public Const SE_GROUP_MANDATORY As Short = &H1S
    Public Const SE_GROUP_ENABLED_BY_DEFAULT As Short = &H2S
    Public Const SE_GROUP_ENABLED As Short = &H4S
    Public Const SE_GROUP_OWNER As Short = &H8S
    Public Const SE_GROUP_LOGON_ID As Integer = &HC0000000

    '  User attributes

    '  (None yet defined.)

    ' ----------------
    '  Kernel Section
    ' ----------------

    Public Const FILE_BEGIN As Short = 0
    Public Const FILE_CURRENT As Short = 1
    Public Const FILE_END As Short = 2

    Public Const FILE_FLAG_WRITE_THROUGH As Integer = &H80000000
    Public Const FILE_FLAG_OVERLAPPED As Integer = &H40000000
    Public Const FILE_FLAG_NO_BUFFERING As Integer = &H20000000
    Public Const FILE_FLAG_RANDOM_ACCESS As Integer = &H10000000
    Public Const FILE_FLAG_SEQUENTIAL_SCAN As Integer = &H8000000
    Public Const FILE_FLAG_DELETE_ON_CLOSE As Integer = &H4000000
    Public Const FILE_FLAG_BACKUP_SEMANTICS As Integer = &H2000000
    Public Const FILE_FLAG_POSIX_SEMANTICS As Integer = &H1000000

    Public Const CREATE_NEW As Short = 1
    Public Const CREATE_ALWAYS As Short = 2
    Public Const OPEN_EXISTING As Short = 3
    Public Const OPEN_ALWAYS As Short = 4
    Public Const TRUNCATE_EXISTING As Short = 5

    ' Define the dwOpenMode values for CreateNamedPipe
    Public Const PIPE_ACCESS_INBOUND As Short = &H1S
    Public Const PIPE_ACCESS_OUTBOUND As Short = &H2S
    Public Const PIPE_ACCESS_DUPLEX As Short = &H3S

    ' Define the Named Pipe End flags for GetNamedPipeInfo
    Public Const PIPE_CLIENT_END As Short = &H0S
    Public Const PIPE_SERVER_END As Short = &H1S

    ' Define the dwPipeMode values for CreateNamedPipe
    Public Const PIPE_WAIT As Short = &H0S
    Public Const PIPE_NOWAIT As Short = &H1S
    Public Const PIPE_READMODE_BYTE As Short = &H0S
    Public Const PIPE_READMODE_MESSAGE As Short = &H2S
    Public Const PIPE_TYPE_BYTE As Short = &H0S
    Public Const PIPE_TYPE_MESSAGE As Short = &H4S

    ' Define the well known values for CreateNamedPipe nMaxInstances
    Public Const PIPE_UNLIMITED_INSTANCES As Short = 255

    ' Define the Security Quality of Service bits to be passed
    '  into CreateFile
    Public Const SECURITY_CONTEXT_TRACKING As Integer = &H40000
    Public Const SECURITY_EFFECTIVE_ONLY As Integer = &H80000

    Public Const SECURITY_SQOS_PRESENT As Integer = &H100000
    Public Const SECURITY_VALID_SQOS_FLAGS As Integer = &H1F0000

    Public Structure OVERLAPPED
        Dim Internal As Integer
        Dim InternalHigh As Integer
        Dim offset As Integer
        Dim OffsetHigh As Integer
        Dim hEvent As Integer
    End Structure

    Public Structure SECURITY_ATTRIBUTES
        Dim nLength As Integer
        Dim lpSecurityDescriptor As Integer
        Dim bInheritHandle As Integer
    End Structure

    Public Structure PROCESS_INFORMATION
        Dim hProcess As Integer
        Dim hThread As Integer
        Dim dwProcessId As Integer
        Dim dwThreadId As Integer
    End Structure

    Public Structure FILETIME
        Dim dwLowDateTime As Integer
        Dim dwHighDateTime As Integer
    End Structure

    Public Structure SystemTime
        Dim wYear As Short
        Dim wMonth As Short
        Dim wDayOfWeek As Short
        Dim wDay As Short
        Dim wHour As Short
        Dim wMinute As Short
        Dim wSecond As Short
        Dim wMilliseconds As Short
    End Structure

    '  Serial provider type.
    Public Const SP_SERIALCOMM As Integer = &H1

    '  Provider SubTypes
    Public Const PST_UNSPECIFIED As Integer = &H0
    Public Const PST_RS232 As Integer = &H1
    Public Const PST_PARALLELPORT As Integer = &H2
    Public Const PST_RS422 As Integer = &H3
    Public Const PST_RS423 As Integer = &H4
    Public Const PST_RS449 As Integer = &H5
    Public Const PST_FAX As Integer = &H21
    Public Const PST_SCANNER As Integer = &H22
    Public Const PST_NETWORK_BRIDGE As Integer = &H100
    Public Const PST_LAT As Integer = &H101
    Public Const PST_TCPIP_TELNET As Integer = &H102
    Public Const PST_X25 As Integer = &H103

    '  Provider capabilities flags.
    Public Const PCF_DTRDSR As Integer = &H1
    Public Const PCF_RTSCTS As Integer = &H2
    Public Const PCF_RLSD As Integer = &H4
    Public Const PCF_PARITY_CHECK As Integer = &H8
    Public Const PCF_XONXOFF As Integer = &H10
    Public Const PCF_SETXCHAR As Integer = &H20
    Public Const PCF_TOTALTIMEOUTS As Integer = &H40
    Public Const PCF_INTTIMEOUTS As Integer = &H80
    Public Const PCF_SPECIALCHARS As Integer = &H100
    Public Const PCF_16BITMODE As Integer = &H200

    '  Comm provider settable parameters.
    Public Const SP_PARITY As Integer = &H1
    Public Const SP_BAUD As Integer = &H2
    Public Const SP_DATABITS As Integer = &H4
    Public Const SP_STOPBITS As Integer = &H8
    Public Const SP_HANDSHAKING As Integer = &H10
    Public Const SP_PARITY_CHECK As Integer = &H20
    Public Const SP_RLSD As Integer = &H40

    '  Settable baud rates in the provider.
    Public Const BAUD_075 As Integer = &H1
    Public Const BAUD_110 As Integer = &H2
    Public Const BAUD_134_5 As Integer = &H4
    Public Const BAUD_150 As Integer = &H8
    Public Const BAUD_300 As Integer = &H10
    Public Const BAUD_600 As Integer = &H20
    Public Const BAUD_1200 As Integer = &H40
    Public Const BAUD_1800 As Integer = &H80
    Public Const BAUD_2400 As Integer = &H100
    Public Const BAUD_4800 As Integer = &H200
    Public Const BAUD_7200 As Integer = &H400
    Public Const BAUD_9600 As Integer = &H800
    Public Const BAUD_14400 As Integer = &H1000
    Public Const BAUD_19200 As Integer = &H2000
    Public Const BAUD_38400 As Integer = &H4000
    Public Const BAUD_56K As Integer = &H8000
    Public Const BAUD_128K As Integer = &H10000
    Public Const BAUD_115200 As Integer = &H20000
    Public Const BAUD_57600 As Integer = &H40000
    Public Const BAUD_USER As Integer = &H10000000

    '  Settable Data Bits
    Public Const DATABITS_5 As Integer = &H1
    Public Const DATABITS_6 As Integer = &H2
    Public Const DATABITS_7 As Integer = &H4
    Public Const DATABITS_8 As Integer = &H8
    Public Const DATABITS_16 As Integer = &H10
    Public Const DATABITS_16X As Integer = &H20

    '  Settable Stop and Parity bits.
    Public Const STOPBITS_10 As Integer = &H1
    Public Const STOPBITS_15 As Integer = &H2
    Public Const STOPBITS_20 As Integer = &H4
    Public Const PARITY_NONE As Integer = &H100
    Public Const PARITY_ODD As Integer = &H200
    Public Const PARITY_EVEN As Integer = &H400
    Public Const PARITY_MARK As Integer = &H800
    Public Const PARITY_SPACE As Integer = &H1000

    Public Structure COMMPROP
        Dim wPacketLength As Short
        Dim wPacketVersion As Short
        Dim dwServiceMask As Integer
        Dim dwReserved1 As Integer
        Dim dwMaxTxQueue As Integer
        Dim dwMaxRxQueue As Integer
        Dim dwMaxBaud As Integer
        Dim dwProvSubType As Integer
        Dim dwProvCapabilities As Integer
        Dim dwSettableParams As Integer
        Dim dwSettableBaud As Integer
        Dim wSettableData As Short
        Dim wSettableStopParity As Short
        Dim dwCurrentTxQueue As Integer
        Dim dwCurrentRxQueue As Integer
        Dim dwProvSpec1 As Integer
        Dim dwProvSpec2 As Integer
        <VBFixedArray(1)> Dim wcProvChar() As Short

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim wcProvChar(1)
        End Sub
    End Structure

    'Type COMSTAT
    '        fCtsHold As Long
    '        fDsrHold As Long
    '        fRlsdHold As Long
    '        fXoffHold As Long
    '        fXoffSent As Long
    '        fEof As Long
    '        fTxim As Long
    '        fReserved As Long
    '        cbInQue As Long
    '        cbOutQue As Long
    'End Type

    Public Structure COMSTAT
        Dim fBitFields As Integer 'See Comment in Win32API.Txt
        Dim cbInQue As Integer
        Dim cbOutQue As Integer
    End Structure
    ' The eight actual COMSTAT bit-sized data fields within the four bytes of fBitFields can be manipulated by bitwise logical And/Or operations.
    ' FieldName     Bit #     Description
    ' ---------     -----     ---------------------------
    ' fCtsHold        1       Tx waiting for CTS signal
    ' fDsrHold        2       Tx waiting for DSR signal
    ' fRlsdHold       3       Tx waiting for RLSD signal
    ' fXoffHold       4       Tx waiting, XOFF char rec'd
    ' fXoffSent       5       Tx waiting, XOFF char sent
    ' fEof            6       EOF character sent
    ' fTxim           7       character waiting for Tx
    ' fReserved       8       reserved (25 bits)

    '  DTR Control Flow Values.
    Public Const DTR_CONTROL_DISABLE As Short = &H0S
    Public Const DTR_CONTROL_ENABLE As Short = &H1S
    Public Const DTR_CONTROL_HANDSHAKE As Short = &H2S

    '  RTS Control Flow Values
    Public Const RTS_CONTROL_DISABLE As Short = &H0S
    Public Const RTS_CONTROL_ENABLE As Short = &H1S
    Public Const RTS_CONTROL_HANDSHAKE As Short = &H2S
    Public Const RTS_CONTROL_TOGGLE As Short = &H3S

    'Type DCB
    '        DCBlength As Long
    '        BaudRate As Long
    '        fBinary As Long
    '        fParity As Long
    '        fOutxCtsFlow As Long
    '        fOutxDsrFlow As Long
    '        fDtrControl As Long
    '        fDsrSensitivity As Long
    '        fTXContinueOnXoff As Long
    '        fOutX As Long
    '        fInX As Long
    '        fErrorChar As Long
    '        fNull As Long
    '        fRtsControl As Long
    '        fAbortOnError As Long
    '        fDummy2 As Long
    '        wReserved As Integer
    '        XonLim As Integer
    '        XoffLim As Integer
    '        ByteSize As Byte
    '        Parity As Byte
    '        StopBits As Byte
    '        XonChar As Byte
    '        XoffChar As Byte
    '        ErrorChar As Byte
    '        EofChar As Byte
    '        EvtChar As Byte
    'End Type

    Public Structure DCB
        Dim DCBlength As Integer
        Dim BaudRate As Integer
        Dim fBitFields As Integer 'See Comments in Win32API.Txt
        Dim wReserved As Short
        Dim XonLim As Short
        Dim XoffLim As Short
        Dim ByteSize As Byte
        Dim Parity As Byte
        Dim StopBits As Byte
        Dim XonChar As Byte
        Dim XoffChar As Byte
        Dim ErrorChar As Byte
        Dim EofChar As Byte
        Dim EvtChar As Byte
        Dim wReserved1 As Short 'Reserved; Do Not Use
    End Structure
    ' The fourteen actual DCB bit-sized data fields within the four bytes of fBitFields can be manipulated by bitwise logical And/Or operations.
    ' FieldName             Bit #     Description
    ' -----------------     -----     ------------------------------
    ' fBinary                 1       binary mode, no EOF check
    ' fParity                 2       enable parity checking
    ' fOutxCtsFlow            3       CTS output flow control
    ' fOutxDsrFlow            4       DSR output flow control
    ' fDtrControl             5       DTR flow control type (2 bits)
    ' fDsrSensitivity         7       DSR sensitivity
    ' fTXContinueOnXoff       8       XOFF continues Tx
    ' fOutX                   9       XON/XOFF out flow control
    ' fInX                   10       XON/XOFF in flow control
    ' fErrorChar             11       enable error replacement
    ' fNull                  12       enable null stripping
    ' fRtsControl            13       RTS flow control (2 bits)
    ' fAbortOnError          15       abort reads/writes on error
    ' fDummy2                16       reserved

    Public Structure COMMTIMEOUTS
        Dim ReadIntervalTimeout As Integer
        Dim ReadTotalTimeoutMultiplier As Integer
        Dim ReadTotalTimeoutConstant As Integer
        Dim WriteTotalTimeoutMultiplier As Integer
        Dim WriteTotalTimeoutConstant As Integer
    End Structure

    Public Structure SYSTEM_INFO
        Dim dwOemID As Integer
        Dim dwPageSize As Integer
        Dim lpMinimumApplicationAddress As Integer
        Dim lpMaximumApplicationAddress As Integer
        Dim dwActiveProcessorMask As Integer
        Dim dwNumberOrfProcessors As Integer
        Dim dwProcessorType As Integer
        Dim dwAllocationGranularity As Integer
        Dim dwReserved As Integer
    End Structure

    ' Global Memory Flags
    Public Const GMEM_FIXED As Short = &H0S
    Public Const GMEM_MOVEABLE As Short = &H2S
    Public Const GMEM_NOCOMPACT As Short = &H10S
    Public Const GMEM_NODISCARD As Short = &H20S
    Public Const GMEM_ZEROINIT As Short = &H40S
    Public Const GMEM_MODIFY As Short = &H80S
    Public Const GMEM_DISCARDABLE As Short = &H100S
    Public Const GMEM_NOT_BANKED As Short = &H1000S
    Public Const GMEM_SHARE As Short = &H2000S
    Public Const GMEM_DDESHARE As Short = &H2000S
    Public Const GMEM_NOTIFY As Short = &H4000S
    Public Const GMEM_LOWER As Short = GMEM_NOT_BANKED
    Public Const GMEM_VALID_FLAGS As Short = &H7F72S
    Public Const GMEM_INVALID_HANDLE As Short = &H8000S

    Public Const GHND As Boolean = (GMEM_MOVEABLE Or GMEM_ZEROINIT)
    Public Const GPTR As Boolean = (GMEM_FIXED Or GMEM_ZEROINIT)

    ' Flags returned by GlobalFlags (in addition to GMEM_DISCARDABLE)
    Public Const GMEM_DISCARDED As Short = &H4000S
    Public Const GMEM_LOCKCOUNT As Short = &HFFS

    Public Structure MEMORYSTATUS
        Dim dwLength As Integer
        Dim dwMemoryLoad As Integer
        Dim dwTotalPhys As Integer
        Dim dwAvailPhys As Integer
        Dim dwTotalPageFile As Integer
        Dim dwAvailPageFile As Integer
        Dim dwTotalVirtual As Integer
        Dim dwAvailVirtual As Integer
    End Structure

    ' Local Memory Flags
    Public Const LMEM_FIXED As Short = &H0S
    Public Const LMEM_MOVEABLE As Short = &H2S
    Public Const LMEM_NOCOMPACT As Short = &H10S
    Public Const LMEM_NODISCARD As Short = &H20S
    Public Const LMEM_ZEROINIT As Short = &H40S
    Public Const LMEM_MODIFY As Short = &H80S
    Public Const LMEM_DISCARDABLE As Short = &HF00S
    Public Const LMEM_VALID_FLAGS As Short = &HF72S
    Public Const LMEM_INVALID_HANDLE As Short = &H8000S

    Public Const LHND As Integer = (LMEM_MOVEABLE + LMEM_ZEROINIT)
    Public Const LPTR As Integer = (LMEM_FIXED + LMEM_ZEROINIT)

    Public Const NONZEROLHND As Short = (LMEM_MOVEABLE)
    Public Const NONZEROLPTR As Short = (LMEM_FIXED)

    ' Flags returned by LocalFlags (in addition to LMEM_DISCARDABLE)
    Public Const LMEM_DISCARDED As Short = &H4000S
    Public Const LMEM_LOCKCOUNT As Short = &HFFS

    '  dwCreationFlag values

    Public Const DEBUG_PROCESS As Short = &H1S
    Public Const DEBUG_ONLY_THIS_PROCESS As Short = &H2S

    Public Const CREATE_SUSPENDED As Short = &H4S

    Public Const DETACHED_PROCESS As Short = &H8S

    Public Const CREATE_NEW_CONSOLE As Short = &H10S

    Public Const NORMAL_PRIORITY_CLASS As Short = &H20S
    Public Const IDLE_PRIORITY_CLASS As Short = &H40S
    Public Const HIGH_PRIORITY_CLASS As Short = &H80S
    Public Const REALTIME_PRIORITY_CLASS As Short = &H100S

    Public Const CREATE_NEW_PROCESS_GROUP As Short = &H200S

    Public Const CREATE_NO_WINDOW As Integer = &H8000000

    Public Const PROFILE_USER As Integer = &H10000000
    Public Const PROFILE_KERNEL As Integer = &H20000000
    Public Const PROFILE_SERVER As Integer = &H40000000

    Public Const MAXLONG As Integer = &H7FFFFFFF
    Public Const THREAD_BASE_PRIORITY_MIN As Short = -2
    Public Const THREAD_BASE_PRIORITY_MAX As Short = 2
    Public Const THREAD_BASE_PRIORITY_LOWRT As Short = 15
    Public Const THREAD_BASE_PRIORITY_IDLE As Short = -15
    Public Const THREAD_PRIORITY_LOWEST As Short = THREAD_BASE_PRIORITY_MIN
    Public Const THREAD_PRIORITY_BELOW_NORMAL As Integer = (THREAD_PRIORITY_LOWEST + 1)
    Public Const THREAD_PRIORITY_NORMAL As Short = 0
    Public Const THREAD_PRIORITY_HIGHEST As Short = THREAD_BASE_PRIORITY_MAX
    Public Const THREAD_PRIORITY_ABOVE_NORMAL As Short = (THREAD_PRIORITY_HIGHEST - 1)
    Public Const THREAD_PRIORITY_ERROR_RETURN As Integer = (MAXLONG)

    Public Const THREAD_PRIORITY_TIME_CRITICAL As Short = THREAD_BASE_PRIORITY_LOWRT
    Public Const THREAD_PRIORITY_IDLE As Short = THREAD_BASE_PRIORITY_IDLE

    ' ++ BUILD Version: 0093     Increment this if a change has global effects

    ' Copyright (c) 1990-1995  Microsoft Corporation

    ' Module Name:

    '     winnt.h

    ' Abstract:

    '     This module defines the 32-Bit Windows types and constants that are
    '     defined by NT, but exposed through the Win32 API.

    ' Revision History:
    Public Const APPLICATION_ERROR_MASK As Integer = &H20000000
    Public Const ERROR_SEVERITY_SUCCESS As Short = &H0S
    Public Const ERROR_SEVERITY_INFORMATIONAL As Integer = &H40000000
    Public Const ERROR_SEVERITY_WARNING As Integer = &H80000000
    Public Const ERROR_SEVERITY_ERROR As Integer = &HC0000000


    Public Const MINCHAR As Short = &H80S
    Public Const MAXCHAR As Short = &H7FS
    Public Const MINSHORT As Short = &H8000S
    Public Const MAXSHORT As Short = &H7FFFS
    Public Const MINLONG As Integer = &H80000000
    Public Const MAXByte As Short = &HFFS
    Public Const MAXWORD As Short = &HFFFFS
    Public Const MAXDWORD As Short = &HFFFFS
    '
    '  Calculate the byte offset of a field in a Public Structure of type type.
    '  *  Language IDs.
    '  *
    '  *  The following two combinations of primary language ID and
    '  *  sublanguage ID have special semantics:
    '  *
    '  *    Primary Language ID   Sublanguage ID      Result
    '  *    -------------------   ---------------     ------------------------
    '  *    LANG_NEUTRAL          SUBLANG_NEUTRAL     Language neutral
    '  *    LANG_NEUTRAL          SUBLANG_DEFAULT     User default language
    '  *    LANG_NEUTRAL          SUBLANG_SYS_DEFAULT System default language
    '  */
    '
    '  *  Primary language IDs.
    '  */
    Public Const LANG_NEUTRAL As Short = &H0S

    Public Const LANG_BULGARIAN As Short = &H2S
    Public Const LANG_CHINESE As Short = &H4S
    Public Const LANG_CROATIAN As Short = &H1AS
    Public Const LANG_CZECH As Short = &H5S
    Public Const LANG_DANISH As Short = &H6S
    Public Const LANG_DUTCH As Short = &H13S
    Public Const LANG_ENGLISH As Short = &H9S
    Public Const LANG_FINNISH As Short = &HBS
    Public Const LANG_FRENCH As Short = &HCS
    Public Const LANG_GERMAN As Short = &H7S
    Public Const LANG_GREEK As Short = &H8S
    Public Const LANG_HUNGARIAN As Short = &HES
    Public Const LANG_ICELANDIC As Short = &HFS
    Public Const LANG_ITALIAN As Short = &H10S
    Public Const LANG_JAPANESE As Short = &H11S
    Public Const LANG_KOREAN As Short = &H12S
    Public Const LANG_NORWEGIAN As Short = &H14S
    Public Const LANG_POLISH As Short = &H15S
    Public Const LANG_PORTUGUESE As Short = &H16S
    Public Const LANG_ROMANIAN As Short = &H18S
    Public Const LANG_RUSSIAN As Short = &H19S
    Public Const LANG_SLOVAK As Short = &H1BS
    Public Const LANG_SLOVENIAN As Short = &H24S
    Public Const LANG_SPANISH As Short = &HAS
    Public Const LANG_SWEDISH As Short = &H1DS
    Public Const LANG_TURKISH As Short = &H1FS

    '
    '  *  Sublanguage IDs.
    '  *
    '  *  The name immediately following SUBLANG_ dictates which primary
    '  *  language ID that sublanguage ID can be combined with to form a
    '  *  valid language ID.
    '  */
    Public Const SUBLANG_NEUTRAL As Short = &H0S '  language neutral
    Public Const SUBLANG_DEFAULT As Short = &H1S '  user default
    Public Const SUBLANG_SYS_DEFAULT As Short = &H2S '  system default

    Public Const SUBLANG_CHINESE_TRADITIONAL As Short = &H1S '  Chinese (Taiwan)
    Public Const SUBLANG_CHINESE_SIMPLIFIED As Short = &H2S '  Chinese (PR China)
    Public Const SUBLANG_CHINESE_HONGKONG As Short = &H3S '  Chinese (Hong Kong)
    Public Const SUBLANG_CHINESE_SINGAPORE As Short = &H4S '  Chinese (Singapore)
    Public Const SUBLANG_DUTCH As Short = &H1S '  Dutch
    Public Const SUBLANG_DUTCH_BELGIAN As Short = &H2S '  Dutch (Belgian)
    Public Const SUBLANG_ENGLISH_US As Short = &H1S '  English (USA)
    Public Const SUBLANG_ENGLISH_UK As Short = &H2S '  English (UK)
    Public Const SUBLANG_ENGLISH_AUS As Short = &H3S '  English (Australian)
    Public Const SUBLANG_ENGLISH_CAN As Short = &H4S '  English (Canadian)
    Public Const SUBLANG_ENGLISH_NZ As Short = &H5S '  English (New Zealand)
    Public Const SUBLANG_ENGLISH_EIRE As Short = &H6S '  English (Irish)
    Public Const SUBLANG_FRENCH As Short = &H1S '  French
    Public Const SUBLANG_FRENCH_BELGIAN As Short = &H2S '  French (Belgian)
    Public Const SUBLANG_FRENCH_CANADIAN As Short = &H3S '  French (Canadian)
    Public Const SUBLANG_FRENCH_SWISS As Short = &H4S '  French (Swiss)
    Public Const SUBLANG_GERMAN As Short = &H1S '  German
    Public Const SUBLANG_GERMAN_SWISS As Short = &H2S '  German (Swiss)
    Public Const SUBLANG_GERMAN_AUSTRIAN As Short = &H3S '  German (Austrian)
    Public Const SUBLANG_ITALIAN As Short = &H1S '  Italian
    Public Const SUBLANG_ITALIAN_SWISS As Short = &H2S '  Italian (Swiss)
    Public Const SUBLANG_NORWEGIAN_BOKMAL As Short = &H1S '  Norwegian (Bokma
    Public Const SUBLANG_NORWEGIAN_NYNORSK As Short = &H2S '  Norwegian (Nynorsk)
    Public Const SUBLANG_PORTUGUESE As Short = &H2S '  Portuguese
    Public Const SUBLANG_PORTUGUESE_BRAZILIAN As Short = &H1S '  Portuguese (Brazilian)
    Public Const SUBLANG_SPANISH As Short = &H1S '  Spanish (Castilian)
    Public Const SUBLANG_SPANISH_MEXICAN As Short = &H2S '  Spanish (Mexican)
    Public Const SUBLANG_SPANISH_MODERN As Short = &H3S '  Spanish (Modern)

    '
    '  *  Sorting IDs.
    '  *
    '  */
    Public Const SORT_DEFAULT As Short = &H0S '  sorting default

    Public Const SORT_JAPANESE_XJIS As Short = &H0S '  Japanese0xJIS order
    Public Const SORT_JAPANESE_UNICODE As Short = &H1S '  Japanese Unicode order

    Public Const SORT_CHINESE_BIG5 As Short = &H0S '  Chinese BIG5 order
    Public Const SORT_CHINESE_UNICODE As Short = &H1S '  Chinese Unicode order

    Public Const SORT_KOREAN_KSC As Short = &H0S '  Korean KSC order
    Public Const SORT_KOREAN_UNICODE As Short = &H1S '  Korean Unicode order

    '  The FILE_READ_DATA and FILE_WRITE_DATA constants are also defined in
    '  devioctl.h as FILE_READ_ACCESS and FILE_WRITE_ACCESS. The values for these
    '  constants *MUST* always be in sync.
    '  The values are redefined in devioctl.h because they must be available to
    '  both DOS and NT.
    '

    Public Const FILE_READ_DATA As Short = (&H1S) '  file pipe
    Public Const FILE_LIST_DIRECTORY As Short = (&H1S) '  directory

    Public Const FILE_WRITE_DATA As Short = (&H2S) '  file pipe
    Public Const FILE_ADD_FILE As Short = (&H2S) '  directory

    Public Const FILE_APPEND_DATA As Short = (&H4S) '  file
    Public Const FILE_ADD_SUBDIRECTORY As Short = (&H4S) '  directory
    Public Const FILE_CREATE_PIPE_INSTANCE As Short = (&H4S) '  named pipe

    Public Const FILE_READ_EA As Short = (&H8S) '  file directory
    Public Const FILE_READ_PROPERTIES As Short = FILE_READ_EA

    Public Const FILE_WRITE_EA As Short = (&H10S) '  file directory
    Public Const FILE_WRITE_PROPERTIES As Short = FILE_WRITE_EA

    Public Const FILE_EXECUTE As Short = (&H20S) '  file
    Public Const FILE_TRAVERSE As Short = (&H20S) '  directory

    Public Const FILE_DELETE_CHILD As Short = (&H40S) '  directory

    Public Const FILE_READ_ATTRIBUTES As Short = (&H80S) '  all

    Public Const FILE_WRITE_ATTRIBUTES As Short = (&H100S) '  all

    Public Const FILE_ALL_ACCESS As Boolean = (STANDARD_RIGHTS_REQUIRED Or SYNCHRONIZE Or &H1FFS)

    Public Const FILE_GENERIC_READ As Boolean = (STANDARD_RIGHTS_READ Or FILE_READ_DATA Or FILE_READ_ATTRIBUTES Or FILE_READ_EA Or SYNCHRONIZE)


    Public Const FILE_GENERIC_WRITE As Boolean = (STANDARD_RIGHTS_WRITE Or FILE_WRITE_DATA Or FILE_WRITE_ATTRIBUTES Or FILE_WRITE_EA Or FILE_APPEND_DATA Or SYNCHRONIZE)


    Public Const FILE_GENERIC_EXECUTE As Boolean = (STANDARD_RIGHTS_EXECUTE Or FILE_READ_ATTRIBUTES Or FILE_EXECUTE Or SYNCHRONIZE)

    Public Const FILE_SHARE_READ As Short = &H1S
    Public Const FILE_SHARE_WRITE As Short = &H2S
    Public Const FILE_ATTRIBUTE_READONLY As Short = &H1S
    Public Const FILE_ATTRIBUTE_HIDDEN As Short = &H2S
    Public Const FILE_ATTRIBUTE_SYSTEM As Short = &H4S
    Public Const FILE_ATTRIBUTE_DIRECTORY As Short = &H10S
    Public Const FILE_ATTRIBUTE_ARCHIVE As Short = &H20S
    Public Const FILE_ATTRIBUTE_NORMAL As Short = &H80S
    Public Const FILE_ATTRIBUTE_TEMPORARY As Short = &H100S
    Public Const FILE_ATTRIBUTE_COMPRESSED As Short = &H800S
    Public Const FILE_NOTIFY_CHANGE_FILE_NAME As Short = &H1S
    Public Const FILE_NOTIFY_CHANGE_DIR_NAME As Short = &H2S
    Public Const FILE_NOTIFY_CHANGE_ATTRIBUTES As Short = &H4S
    Public Const FILE_NOTIFY_CHANGE_SIZE As Short = &H8S
    Public Const FILE_NOTIFY_CHANGE_LAST_WRITE As Short = &H10S
    Public Const FILE_NOTIFY_CHANGE_SECURITY As Short = &H100S
    Public Const MAILSLOT_NO_MESSAGE As Short = (-1)
    Public Const MAILSLOT_WAIT_FOREVER As Short = (-1)
    Public Const FILE_CASE_SENSITIVE_SEARCH As Short = &H1S
    Public Const FILE_CASE_PRESERVED_NAMES As Short = &H2S
    Public Const FILE_UNICODE_ON_DISK As Short = &H4S
    Public Const FILE_PERSISTENT_ACLS As Short = &H8S
    Public Const FILE_FILE_COMPRESSION As Short = &H10S
    Public Const FILE_VOLUME_IS_COMPRESSED As Short = &H8000S
    Public Const IO_COMPLETION_MODIFY_STATE As Short = &H2S
    Public Const IO_COMPLETION_ALL_ACCESS As Boolean = (STANDARD_RIGHTS_REQUIRED Or SYNCHRONIZE Or &H3S)
    Public Const DUPLICATE_CLOSE_SOURCE As Short = &H1S
    Public Const DUPLICATE_SAME_ACCESS As Short = &H2S

    ' //////////////////////////////////////////////////////////////////////
    '                                                                     //
    '                              ACCESS MASK                            //
    '                                                                     //
    ' //////////////////////////////////////////////////////////////////////

    '
    '   Define the access mask as a longword sized Public Structure divided up as
    '   follows:

    '       typedef struct _ACCESS_MASK {
    '           WORD   SpecificRights;
    '           Byte  StandardRights;
    '           Byte  AccessSystemAcl : 1;
    '           Byte  Reserved : 3;
    '           Byte  GenericAll : 1;
    '           Byte  GenericExecute : 1;
    '           Byte  GenericWrite : 1;
    '           Byte  GenericRead : 1;
    '       } ACCESS_MASK;
    '       typedef ACCESS_MASK *PACCESS_MASK;
    '
    '   but to make life simple for programmer's we'll allow them to specify
    '   a desired access mask by simply OR'ing together mulitple single rights
    '   and treat an access mask as a DWORD.  For example
    '
    '       DesiredAccess = DELETE  Or  READ_CONTROL
    '
    '   So we'll Public Declare ACCESS_MASK as DWORD
    '

    '  begin_ntddk begin_nthal begin_ntifs

    ' //////////////////////////////////////////////////////////////////////
    '                                                                     //
    '                              ACCESS TYPES                           //
    '                                                                     //
    ' //////////////////////////////////////////////////////////////////////


    '  begin_ntddk begin_nthal begin_ntifs
    '
    '   The following are masks for the predefined standard access types

    '  AccessSystemAcl access type

    Public Const ACCESS_SYSTEM_SECURITY As Integer = &H1000000

    '  MaximumAllowed access type

    Public Const MAXIMUM_ALLOWED As Integer = &H2000000

    '   These are the generic rights.

    Public Const GENERIC_READ As Integer = &H80000000
    Public Const GENERIC_WRITE As Integer = &H40000000
    Public Const GENERIC_EXECUTE As Integer = &H20000000
    Public Const GENERIC_ALL As Integer = &H10000000

    '   Define the generic mapping array.  This is used to denote the
    '   mapping of each generic access right to a specific access mask.

    Public Structure GENERIC_MAPPING
        Dim GenericRead As Integer
        Dim GenericWrite As Integer
        Dim GenericExecute As Integer
        Dim GenericAll As Integer
    End Structure


    ' //////////////////////////////////////////////////////////////////////
    '                                                                     //
    '                         LUID_AND_ATTRIBUTES                         //
    '                                                                     //
    ' //////////////////////////////////////////////////////////////////////
    '
    Public Structure Luid
        Dim lowpart As Integer
        Dim highpart As Integer
    End Structure


    Public Structure LUID_AND_ATTRIBUTES
        Dim pLuid As Luid
        Dim Attributes As Integer
    End Structure

    ' //////////////////////////////////////////////////////////////////////
    '                                                                     //
    '                          ACL  and  ACE                              //
    '                                                                     //
    ' //////////////////////////////////////////////////////////////////////

    '
    '   Define an ACL and the ACE format.  The Public Structure of an ACL header
    '   followed by one or more ACEs.  Pictorally the Public Structure of an ACL header
    '   is as follows:
    '
    '   The current AclRevision is defined to be ACL_REVISION.
    '
    '   AclSize is the size, in bytes, allocated for the ACL.  This includes
    '   the ACL header, ACES, and remaining free space in the buffer.
    '
    '   AceCount is the number of ACES in the ACL.
    '

    '  begin_ntddk begin_ntifs
    '  This is the *current* ACL revision

    Public Const ACL_REVISION As Short = (2)

    '  This is the history of ACL revisions.  Add a new one whenever
    '  ACL_REVISION is updated

    Public Const ACL_REVISION1 As Short = (1)
    Public Const ACL_REVISION2 As Short = (2)

    Public Structure ACL
        Dim AclRevision As Byte
        Dim Sbz1 As Byte
        Dim AclSize As Short
        Dim AceCount As Short
        Dim Sbz2 As Short
    End Structure

    ' typedef ACL *PACL;

    '  end_ntddk

    '   The Public Structure of an ACE is a common ace header followed by ace type
    '   specific data.  Pictorally the Public Structure of the common ace header is
    '   as follows:

    '   AceType denotes the type of the ace, there are some predefined ace
    '   types
    '
    '   AceSize is the size, in bytes, of ace.
    '
    '   AceFlags are the Ace flags for audit and inheritance, defined Integerly.

    Public Structure ACE_HEADER
        Dim AceType As Byte
        Dim AceFlags As Byte
        Dim AceSize As Integer
    End Structure
    '
    '   The following are the predefined ace types that go into the AceType
    '   field of an Ace header.

    Public Const ACCESS_ALLOWED_ACE_TYPE As Short = &H0S
    Public Const ACCESS_DENIED_ACE_TYPE As Short = &H1S
    Public Const SYSTEM_AUDIT_ACE_TYPE As Short = &H2S
    Public Const SYSTEM_ALARM_ACE_TYPE As Short = &H3S

    '   The following are the inherit flags that go into the AceFlags field
    '   of an Ace header.

    Public Const OBJECT_INHERIT_ACE As Short = &H1S
    Public Const CONTAINER_INHERIT_ACE As Short = &H2S
    Public Const NO_PROPAGATE_INHERIT_ACE As Short = &H4S
    Public Const INHERIT_ONLY_ACE As Short = &H8S
    Public Const VALID_INHERIT_FLAGS As Short = &HFS


    '   The following are the currently defined ACE flags that go into the
    '   AceFlags field of an ACE header.  Each ACE type has its own set of
    '   AceFlags.
    '
    '   SUCCESSFUL_ACCESS_ACE_FLAG - used only with system audit and alarm ACE
    '   types to indicate that a message is generated for successful accesses.
    '
    '   FAILED_ACCESS_ACE_FLAG - used only with system audit and alarm ACE types
    '   to indicate that a message is generated for failed accesses.

    '   SYSTEM_AUDIT and SYSTEM_ALARM AceFlags
    '
    '   These control the signaling of audit and alarms for success or failure.

    Public Const SUCCESSFUL_ACCESS_ACE_FLAG As Short = &H40S
    Public Const FAILED_ACCESS_ACE_FLAG As Short = &H80S


    '
    '   We'll define the Public Structure of the predefined ACE types.  Pictorally
    '   the Public Structure of the predefined ACE's is as follows:

    '   Mask is the access mask associated with the ACE.  This is either the
    '   access allowed, access denied, audit, or alarm mask.
    '
    '   Sid is the Sid associated with the ACE.
    '
    '   The following are the four predefined ACE types.
    '   Examine the AceType field in the Header to determine
    '   which Public Structure is appropriate to use for casting.


    Public Structure ACCESS_ALLOWED_ACE
        Dim Header As ACE_HEADER
        Dim Mask As Integer
        Dim SidStart As Integer
    End Structure

    Public Structure ACCESS_DENIED_ACE
        Dim Header As ACE_HEADER
        Dim Mask As Integer
        Dim SidStart As Integer
    End Structure


    Public Structure SYSTEM_AUDIT_ACE
        Dim Header As ACE_HEADER
        Dim Mask As Integer
        Dim SidStart As Integer
    End Structure

    Public Structure SYSTEM_ALARM_ACE
        Dim Header As ACE_HEADER
        Dim Mask As Integer
        Dim SidStart As Integer
    End Structure

    '   The following declarations are used for setting and querying information
    '   about and ACL.  First are the various information classes available to
    '   the user.
    '

    Public Const AclRevisionInformation As Short = 1
    Public Const AclSizeInformation As Short = 2
    '
    '   This record is returned/sent if the user is requesting/setting the
    '   AclRevisionInformation
    '

    Public Structure ACL_REVISION_INFORMATION
        Dim AclRevision As Integer
    End Structure

    '
    '   This record is returned if the user is requesting AclSizeInformation
    '

    Public Structure ACL_SIZE_INFORMATION
        Dim AceCount As Integer
        Dim AclBytesInUse As Integer
        Dim AclBytesFree As Integer
    End Structure

    ' //////////////////////////////////////////////////////////////////////
    '                                                                     //
    '                              SECURITY_DESCRIPTOR                    //
    '                                                                     //
    ' //////////////////////////////////////////////////////////////////////
    '
    '   Define the Security Descriptor and related data types.
    '   This is an opaque data structure.
    '

    '  begin_ntddk begin_ntifs
    '
    '  Current security descriptor revision value
    '

    Public Const SECURITY_DESCRIPTOR_REVISION As Short = (1)
    Public Const SECURITY_DESCRIPTOR_REVISION1 As Short = (1)

    '  end_ntddk

    '
    '  Minimum length, in bytes, needed to build a security descriptor
    '  (NOTE: This must manually be kept consistent with the)
    '  (sizeof(SECURITY_DESCRIPTOR)                         )
    '

    Public Const SECURITY_DESCRIPTOR_MIN_LENGTH As Short = (20)



    Public Const SE_OWNER_DEFAULTED As Short = &H1S
    Public Const SE_GROUP_DEFAULTED As Short = &H2S
    Public Const SE_DACL_PRESENT As Short = &H4S
    Public Const SE_DACL_DEFAULTED As Short = &H8S
    Public Const SE_SACL_PRESENT As Short = &H10S
    Public Const SE_SACL_DEFAULTED As Short = &H20S
    Public Const SE_SELF_RELATIVE As Short = &H8000S

    '
    '   Where:
    '
    '       SE_OWNER_DEFAULTED - This boolean flag, when set, indicates that the
    '           SID pointed to by the Owner field was provided by a
    '           defaulting mechanism rather than explicitly provided by the
    '           original provider of the security descriptor.  This may
    '           affect the treatment of the SID with respect to inheritence
    '           of an owner.
    '
    '       SE_GROUP_DEFAULTED - This boolean flag, when set, indicates that the
    '           SID in the Group field was provided by a defaulting mechanism
    '           rather than explicitly provided by the original provider of
    '           the security descriptor.  This may affect the treatment of
    '           the SID with respect to inheritence of a primary group.
    '
    '       SE_DACL_PRESENT - This boolean flag, when set, indicates that the
    '           security descriptor contains a discretionary ACL.  If this
    '           flag is set and the Dacl field of the SECURITY_DESCRIPTOR is
    '           null, then a null ACL is explicitly being specified.
    '
    '       SE_DACL_DEFAULTED - This boolean flag, when set, indicates that the
    '           ACL pointed to by the Dacl field was provided by a defaulting
    '           mechanism rather than explicitly provided by the original
    '           provider of the security descriptor.  This may affect the
    '           treatment of the ACL with respect to inheritence of an ACL.
    '           This flag is ignored if the DaclPresent flag is not set.
    '
    '       SE_SACL_PRESENT - This boolean flag, when set,  indicates that the
    '           security descriptor contains a system ACL pointed to by the
    '           Sacl field.  If this flag is set and the Sacl field of the
    '           SECURITY_DESCRIPTOR is null, then an empty (but present)
    '           ACL is being specified.
    '
    '       SE_SACL_DEFAULTED - This boolean flag, when set, indicates that the
    '           ACL pointed to by the Sacl field was provided by a defaulting
    '           mechanism rather than explicitly provided by the original
    '           provider of the security descriptor.  This may affect the
    '           treatment of the ACL with respect to inheritence of an ACL.
    '           This flag is ignored if the SaclPresent flag is not set.
    '
    '       SE_SELF_RELATIVE - This boolean flag, when set, indicates that the
    '           security descriptor is in self-relative form.  In this form,
    '           all fields of the security descriptor are contiguous in memory
    '           and all pointer fields are expressed as offsets from the
    '           beginning of the security descriptor.  This form is useful
    '           for treating security descriptors as opaque data structures
    '           for transmission in communication protocol or for storage on
    '           secondary media.
    '
    '
    '
    '  In general, this data Public Structure should be treated opaquely to ensure future
    '  compatibility.
    '
    '

    Public Structure SECURITY_DESCRIPTOR
        Dim Revision As Byte
        Dim Sbz1 As Byte
        Dim Control As Integer
        Dim Owner As Integer
        Dim Group As Integer
        Dim Sacl As ACL
        Dim Dacl As ACL
    End Structure


    '  Where:
    '
    '      Revision - Contains the revision level of the security
    '          descriptor.  This allows this Public Structure to be passed between
    '          systems or stored on disk even though it is expected to
    '          change in the future.
    '
    '      Control - A set of flags which qualify the meaning of the
    '          security descriptor or individual fields of the security
    '          descriptor.
    '
    '      Owner - is a pointer to an SID representing an object's owner.
    '          If this field is null, then no owner SID is present in the
    '          security descriptor.  If the security descriptor is in
    '          self-relative form, then this field contains an offset to
    '          the SID, rather than a pointer.
    '
    '      Group - is a pointer to an SID representing an object's primary
    '          group.  If this field is null, then no primary group SID is
    '          present in the security descriptor.  If the security descriptor
    '          is in self-relative form, then this field contains an offset to
    '          the SID, rather than a pointer.
    '
    '      Sacl - is a pointer to a system ACL.  This field value is only
    '          valid if the DaclPresent control flag is set.  If the
    '          SaclPresent flag is set and this field is null, then a null
    '          ACL  is specified.  If the security descriptor is in
    '          self-relative form, then this field contains an offset to
    '          the ACL, rather than a pointer.
    '
    '      Dacl - is a pointer to a discretionary ACL.  This field value is
    '          only valid if the DaclPresent control flag is set.  If the
    '          DaclPresent flag is set and this field is null, then a null
    '          ACL (unconditionally granting access) is specified.  If the
    '          security descriptor is in self-relative form, then this field
    '          contains an offset to the ACL, rather than a pointer.
    '



    ' //////////////////////////////////////////////////////////////////////
    '                                                                     //
    '                Privilege Related Data Structures                    //
    '                                                                     //
    ' //////////////////////////////////////////////////////////////////////


    '  Privilege attributes
    '

    Public Const SE_PRIVILEGE_ENABLED_BY_DEFAULT As Short = &H1S
    Public Const SE_PRIVILEGE_ENABLED As Short = &H2S
    Public Const SE_PRIVILEGE_USED_FOR_ACCESS As Integer = &H80000000

    '
    '  Privilege Set Control flags
    '

    Public Const PRIVILEGE_SET_ALL_NECESSARY As Short = (1)

    '
    '   Privilege Set - This is defined for a privilege set of one.
    '                   If more than one privilege is needed, then this structure
    '                   will need to be allocated with more space.
    '
    '   Note: don't change this Public Structure without fixing the INITIAL_PRIVILEGE_SET
    '   Public Structure (defined in se.h)
    '

    Public Structure PRIVILEGE_SET
        Dim PrivilegeCount As Integer
        Dim Control As Integer
        <VBFixedArray(ANYSIZE_ARRAY)> Dim Privilege() As LUID_AND_ATTRIBUTES

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim Privilege(ANYSIZE_ARRAY)
        End Sub
    End Structure


    '//////////////////////////////////////////////////////////////////////
    '                                                                     //
    '                NT Defined Privileges                                //
    '                                                                     //
    ' //////////////////////////////////////////////////////////////////////

    Public Const SE_CREATE_TOKEN_NAME As String = "SeCreateTokenPrivilege"
    Public Const SE_ASSIGNPRIMARYTOKEN_NAME As String = "SeAssignPrimaryTokenPrivilege"
    Public Const SE_LOCK_MEMORY_NAME As String = "SeLockMemoryPrivilege"
    Public Const SE_INCREASE_QUOTA_NAME As String = "SeIncreaseQuotaPrivilege"
    Public Const SE_UNSOLICITED_INPUT_NAME As String = "SeUnsolicitedInputPrivilege"
    Public Const SE_MACHINE_ACCOUNT_NAME As String = "SeMachineAccountPrivilege"
    Public Const SE_TCB_NAME As String = "SeTcbPrivilege"
    Public Const SE_SECURITY_NAME As String = "SeSecurityPrivilege"
    Public Const SE_TAKE_OWNERSHIP_NAME As String = "SeTakeOwnershipPrivilege"
    Public Const SE_LOAD_DRIVER_NAME As String = "SeLoadDriverPrivilege"
    Public Const SE_SYSTEM_PROFILE_NAME As String = "SeSystemProfilePrivilege"
    Public Const SE_SYSTEMTIME_NAME As String = "SeSystemtimePrivilege"
    Public Const SE_PROF_SINGLE_PROCESS_NAME As String = "SeProfileSingleProcessPrivilege"
    Public Const SE_INC_BASE_PRIORITY_NAME As String = "SeIncreaseBasePriorityPrivilege"
    Public Const SE_CREATE_PAGEFILE_NAME As String = "SeCreatePagefilePrivilege"
    Public Const SE_CREATE_PERMANENT_NAME As String = "SeCreatePermanentPrivilege"
    Public Const SE_BACKUP_NAME As String = "SeBackupPrivilege"
    Public Const SE_RESTORE_NAME As String = "SeRestorePrivilege"
    Public Const SE_SHUTDOWN_NAME As String = "SeShutdownPrivilege"
    Public Const SE_DEBUG_NAME As String = "SeDebugPrivilege"
    Public Const SE_AUDIT_NAME As String = "SeAuditPrivilege"
    Public Const SE_SYSTEM_ENVIRONMENT_NAME As String = "SeSystemEnvironmentPrivilege"
    Public Const SE_CHANGE_NOTIFY_NAME As String = "SeChangeNotifyPrivilege"
    Public Const SE_REMOTE_SHUTDOWN_NAME As String = "SeRemoteShutdownPrivilege"


    ' //////////////////////////////////////////////////////////////////
    '                                                                 //
    '            Security Quality Of Service                          //
    '                                                                 //
    '                                                                 //
    ' //////////////////////////////////////////////////////////////////

    '  begin_ntddk begin_nthal begin_ntifs
    '
    '  Impersonation Level
    '
    '  Impersonation level is represented by a pair of bits in Windows.
    '  If a new impersonation level is added or lowest value is changed from
    '  0 to something else, fix the Windows CreateFile call.
    '

    Public Const SecurityAnonymous As Short = 1
    Public Const SecurityIdentification As Short = 2

    '//////////////////////////////////////////////////////////////////////
    '                                                                     //
    '                Registry API Constants                                //
    '                                                                     //
    ' //////////////////////////////////////////////////////////////////////

    ' Reg Data Types...
    Public Const REG_NONE As Short = 0 ' No value type
    Public Const REG_SZ As Short = 1 ' Unicode nul terminated string
    Public Const REG_EXPAND_SZ As Short = 2 ' Unicode nul terminated string
    Public Const REG_BINARY As Short = 3 ' Free form binary
    Public Const REG_DWORD As Short = 4 ' 32-bit number
    Public Const REG_DWORD_LITTLE_ENDIAN As Short = 4 ' 32-bit number (same as REG_DWORD)
    Public Const REG_DWORD_BIG_ENDIAN As Short = 5 ' 32-bit number
    Public Const REG_LINK As Short = 6 ' Symbolic Link (unicode)
    Public Const REG_MULTI_SZ As Short = 7 ' Multiple Unicode strings
    Public Const REG_RESOURCE_LIST As Short = 8 ' Resource list in the resource map
    Public Const REG_FULL_RESOURCE_DESCRIPTOR As Short = 9 ' Resource list in the hardware description
    Public Const REG_RESOURCE_REQUIREMENTS_LIST As Short = 10
    Public Const REG_CREATED_NEW_KEY As Short = &H1S ' New Registry Key created
    Public Const REG_OPENED_EXISTING_KEY As Short = &H2S ' Existing Key opened
    Public Const REG_WHOLE_HIVE_VOLATILE As Short = &H1S ' Restore whole hive volatile
    Public Const REG_REFRESH_HIVE As Short = &H2S ' Unwind changes to last flush
    Public Const REG_NOTIFY_CHANGE_NAME As Short = &H1S ' Create or delete (child)
    Public Const REG_NOTIFY_CHANGE_ATTRIBUTES As Short = &H2S
    Public Const REG_NOTIFY_CHANGE_LAST_SET As Short = &H4S ' Time stamp
    Public Const REG_NOTIFY_CHANGE_SECURITY As Short = &H8S
    Public Const REG_LEGAL_CHANGE_FILTER As Boolean = (REG_NOTIFY_CHANGE_NAME Or REG_NOTIFY_CHANGE_ATTRIBUTES Or REG_NOTIFY_CHANGE_LAST_SET Or REG_NOTIFY_CHANGE_SECURITY)
    Public Const REG_LEGAL_OPTION As Boolean = (REG_OPTION_RESERVED Or REG_OPTION_NON_VOLATILE Or REG_OPTION_VOLATILE Or REG_OPTION_CREATE_LINK Or REG_OPTION_BACKUP_RESTORE)

    ' Reg Create Type Values...
    Public Const REG_OPTION_RESERVED As Short = 0 ' Parameter is reserved
    Public Const REG_OPTION_NON_VOLATILE As Short = 0 ' Key is preserved when system is rebooted
    Public Const REG_OPTION_VOLATILE As Short = 1 ' Key is not preserved when system is rebooted
    Public Const REG_OPTION_CREATE_LINK As Short = 2 ' Created key is a symbolic link
    Public Const REG_OPTION_BACKUP_RESTORE As Short = 4 ' open for backup or restore

    ' Reg Key Security Options
    ' Public Const READ_CONTROL As Integer = &H20000
    Public Const KEY_QUERY_VALUE As Short = &H1S
    Public Const KEY_SET_VALUE As Short = &H2S
    Public Const KEY_CREATE_SUB_KEY As Short = &H4S
    Public Const KEY_ENUMERATE_SUB_KEYS As Short = &H8S
    Public Const KEY_NOTIFY As Short = &H10S
    Public Const KEY_CREATE_LINK As Short = &H20S
    Public Const KEY_READ As Boolean = ((STANDARD_RIGHTS_READ Or KEY_QUERY_VALUE Or KEY_ENUMERATE_SUB_KEYS Or KEY_NOTIFY) And (Not SYNCHRONIZE))
    Public Const KEY_WRITE As Boolean = ((STANDARD_RIGHTS_WRITE Or KEY_SET_VALUE Or KEY_CREATE_SUB_KEY) And (Not SYNCHRONIZE))
    'Public Const KEY_EXECUTE As Boolean = (KEY_READ)
    Public Const KEY_ALL_ACCESS As Boolean = ((STANDARD_RIGHTS_ALL Or KEY_QUERY_VALUE Or KEY_SET_VALUE Or KEY_CREATE_SUB_KEY Or KEY_ENUMERATE_SUB_KEYS Or KEY_NOTIFY Or KEY_CREATE_LINK) And (Not SYNCHRONIZE))
    'Public Const STANDARD_RIGHTS_READ As Integer = (READ_CONTROL)
    'Public Const STANDARD_RIGHTS_WRITE As Integer = (READ_CONTROL)
    Public Const KEY_EXECUTE As Boolean = ((KEY_READ) And (Not SYNCHRONIZE))

    ' end winnt.txt

    ' Debug APIs
    Public Const EXCEPTION_DEBUG_EVENT As Short = 1
    Public Const CREATE_THREAD_DEBUG_EVENT As Short = 2
    Public Const CREATE_PROCESS_DEBUG_EVENT As Short = 3
    Public Const EXIT_THREAD_DEBUG_EVENT As Short = 4
    Public Const EXIT_PROCESS_DEBUG_EVENT As Short = 5
    Public Const LOAD_DLL_DEBUG_EVENT As Short = 6
    Public Const UNLOAD_DLL_DEBUG_EVENT As Short = 7
    Public Const OUTPUT_DEBUG_STRING_EVENT As Short = 8
    Public Const RIP_EVENT As Short = 9

    Public Const EXCEPTION_MAXIMUM_PARAMETERS As Short = 15

    Public Structure EXCEPTION_RECORD
        Dim ExceptionCode As Integer
        Dim ExceptionFlags As Integer
        Dim pExceptionRecord As Integer ' Pointer to an EXCEPTION_RECORD structure
        Dim ExceptionAddress As Integer
        Dim NumberParameters As Integer
        <VBFixedArray(EXCEPTION_MAXIMUM_PARAMETERS)> Dim ExceptionInformation() As Integer

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim ExceptionInformation(EXCEPTION_MAXIMUM_PARAMETERS)
        End Sub
    End Structure

    Public Structure EXCEPTION_DEBUG_INFO
        'UPGRADE_WARNING: ?? pExceptionRecord ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"”
        Dim pExceptionRecord As EXCEPTION_RECORD
        Dim dwFirstChance As Integer

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            pExceptionRecord.Initialize()
        End Sub
    End Structure

    Public Structure CREATE_THREAD_DEBUG_INFO
        Dim hThread As Integer
        Dim lpThreadLocalBase As Integer
        Dim lpStartAddress As Integer
    End Structure

    Public Structure CREATE_PROCESS_DEBUG_INFO
        Dim hFile As Integer
        Dim hProcess As Integer
        Dim hThread As Integer
        Dim lpBaseOfImage As Integer
        Dim dwDebugInfoFileOffset As Integer
        Dim nDebugInfoSize As Integer
        Dim lpThreadLocalBase As Integer
        Dim lpStartAddress As Integer
        Dim lpImageName As Integer
        Dim fUnicode As Short
    End Structure

    Public Structure EXIT_THREAD_DEBUG_INFO
        Dim dwExitCode As Integer
    End Structure

    Public Structure EXIT_PROCESS_DEBUG_INFO
        Dim dwExitCode As Integer
    End Structure

    Public Structure LOAD_DLL_DEBUG_INFO
        Dim hFile As Integer
        Dim lpBaseOfDll As Integer
        Dim dwDebugInfoFileOffset As Integer
        Dim nDebugInfoSize As Integer
        Dim lpImageName As Integer
        Dim fUnicode As Short
    End Structure

    Public Structure UNLOAD_DLL_DEBUG_INFO
        Dim lpBaseOfDll As Integer
    End Structure

    Public Structure OUTPUT_DEBUG_STRING_INFO
        Dim lpDebugStringData As String
        Dim fUnicode As Short
        Dim nDebugStringLength As Short
    End Structure

    Public Structure RIP_INFO
        Dim dwError As Integer
        Dim dwType As Integer
    End Structure

    ' GetDriveType return values
    Public Const DRIVE_REMOVABLE As Short = 2
    Public Const DRIVE_FIXED As Short = 3
    Public Const DRIVE_REMOTE As Short = 4
    Public Const DRIVE_CDROM As Short = 5
    Public Const DRIVE_RAMDISK As Short = 6

    Public Const FILE_TYPE_UNKNOWN As Short = &H0S
    Public Const FILE_TYPE_DISK As Short = &H1S
    Public Const FILE_TYPE_CHAR As Short = &H2S
    Public Const FILE_TYPE_PIPE As Short = &H3S
    Public Const FILE_TYPE_REMOTE As Short = &H8000S

    Public Const STD_INPUT_HANDLE As Short = -10
    Public Const STD_OUTPUT_HANDLE As Short = -11
    Public Const STD_ERROR_HANDLE As Short = -12

    Public Const NOPARITY As Short = 0
    Public Const ODDPARITY As Short = 1
    Public Const EVENPARITY As Short = 2
    Public Const MARKPARITY As Short = 3
    Public Const SPACEPARITY As Short = 4

    Public Const ONESTOPBIT As Short = 0
    Public Const ONE5STOPBITS As Short = 1
    Public Const TWOSTOPBITS As Short = 2

    Public Const IGNORE As Short = 0 '  Ignore signal
    Public Const INFINITE As Short = &HFFFFS '  Infinite timeout

    ' Comm Baud Rate indices
    Public Const CBR_110 As Short = 110
    Public Const CBR_300 As Short = 300
    Public Const CBR_600 As Short = 600
    Public Const CBR_1200 As Short = 1200
    Public Const CBR_2400 As Short = 2400
    Public Const CBR_4800 As Short = 4800
    Public Const CBR_9600 As Short = 9600
    Public Const CBR_14400 As Short = 14400
    Public Const CBR_19200 As Short = 19200
    Public Const CBR_38400 As Integer = 38400
    Public Const CBR_56000 As Integer = 56000
    Public Const CBR_57600 As Integer = 57600
    Public Const CBR_115200 As Integer = 115200
    Public Const CBR_128000 As Integer = 128000
    Public Const CBR_256000 As Integer = 256000

    ' Error Flags
    Public Const CE_RXOVER As Short = &H1S '  Receive Queue overflow
    Public Const CE_OVERRUN As Short = &H2S '  Receive Overrun Error
    Public Const CE_RXPARITY As Short = &H4S '  Receive Parity Error
    Public Const CE_FRAME As Short = &H8S '  Receive Framing error
    Public Const CE_BREAK As Short = &H10S '  Break Detected
    Public Const CE_TXFULL As Short = &H100S '  TX Queue is full
    Public Const CE_PTO As Short = &H200S '  LPTx Timeout
    Public Const CE_IOE As Short = &H400S '  LPTx I/O Error
    Public Const CE_DNS As Short = &H800S '  LPTx Device not selected
    Public Const CE_OOP As Short = &H1000S '  LPTx Out-Of-Paper
    Public Const CE_MODE As Short = &H8000S '  Requested mode unsupported

    Public Const IE_BADID As Short = (-1) '  Invalid or unsupported id
    Public Const IE_OPEN As Short = (-2) '  Device Already Open
    Public Const IE_NOPEN As Short = (-3) '  Device Not Open
    Public Const IE_MEMORY As Short = (-4) '  Unable to allocate queues
    Public Const IE_DEFAULT As Short = (-5) '  Error in default parameters
    Public Const IE_HARDWARE As Short = (-10) '  Hardware Not Present
    Public Const IE_BYTESIZE As Short = (-11) '  Illegal Byte Size
    Public Const IE_BAUDRATE As Short = (-12) '  Unsupported BaudRate

    ' Events
    Public Const EV_RXCHAR As Short = &H1S '  Any Character received
    Public Const EV_RXFLAG As Short = &H2S '  Received certain character
    Public Const EV_TXEMPTY As Short = &H4S '  Transmitt Queue Empty
    Public Const EV_CTS As Short = &H8S '  CTS changed state
    Public Const EV_DSR As Short = &H10S '  DSR changed state
    Public Const EV_RLSD As Short = &H20S '  RLSD changed state
    Public Const EV_BREAK As Short = &H40S '  BREAK received
    Public Const EV_ERR As Short = &H80S '  Line status error occurred
    Public Const EV_RING As Short = &H100S '  Ring signal detected
    Public Const EV_PERR As Short = &H200S '  Printer error occured
    Public Const EV_RX80FULL As Short = &H400S '  Receive buffer is 80 percent full
    Public Const EV_EVENT1 As Short = &H800S '  Provider specific event 1
    Public Const EV_EVENT2 As Short = &H1000S '  Provider specific event 2

    ' Escape Functions
    Public Const SETXOFF As Short = 1 '  Simulate XOFF received
    Public Const SETXON As Short = 2 '  Simulate XON received
    Public Const SETRTS As Short = 3 '  Set RTS high
    Public Const CLRRTS As Short = 4 '  Set RTS low
    Public Const SETDTR As Short = 5 '  Set DTR high
    Public Const CLRDTR As Short = 6 '  Set DTR low
    Public Const RESETDEV As Short = 7 '  Reset device if possible
    Public Const SETBREAK As Short = 8 'Set the device break line
    Public Const CLRBREAK As Short = 9 ' Clear the device break line

    '  PURGE function flags.
    Public Const PURGE_TXABORT As Short = &H1S '  Kill the pending/current writes to the comm port.
    Public Const PURGE_RXABORT As Short = &H2S '  Kill the pending/current reads to the comm port.
    Public Const PURGE_TXCLEAR As Short = &H4S '  Kill the transmit queue if there.
    Public Const PURGE_RXCLEAR As Short = &H8S '  Kill the typeahead buffer if there.

    Public Const LPTx As Short = &H80S '  Set if ID is for LPT device

    '  Modem Status Flags
    Public Const MS_CTS_ON As Integer = &H10
    Public Const MS_DSR_ON As Integer = &H20
    Public Const MS_RING_ON As Integer = &H40
    Public Const MS_RLSD_ON As Integer = &H80

    ' WaitSoundState() Constants
    Public Const S_QUEUEEMPTY As Short = 0
    Public Const S_THRESHOLD As Short = 1
    Public Const S_ALLTHRESHOLD As Short = 2

    ' Accent Modes
    Public Const S_NORMAL As Short = 0
    Public Const S_LEGATO As Short = 1
    Public Const S_STACCATO As Short = 2

    ' SetSoundNoise() Sources
    Public Const S_PERIOD512 As Short = 0 '  Freq = N/512 high pitch, less coarse hiss
    Public Const S_PERIOD1024 As Short = 1 '  Freq = N/1024
    Public Const S_PERIOD2048 As Short = 2 '  Freq = N/2048 low pitch, more coarse hiss
    Public Const S_PERIODVOICE As Short = 3 '  Source is frequency from voice channel (3)
    Public Const S_WHITE512 As Short = 4 '  Freq = N/512 high pitch, less coarse hiss
    Public Const S_WHITE1024 As Short = 5 '  Freq = N/1024
    Public Const S_WHITE2048 As Short = 6 '  Freq = N/2048 low pitch, more coarse hiss
    Public Const S_WHITEVOICE As Short = 7 '  Source is frequency from voice channel (3)

    Public Const S_SERDVNA As Short = (-1) '  Device not available
    Public Const S_SEROFM As Short = (-2) '  Out of memory
    Public Const S_SERMACT As Short = (-3) '  Music active
    Public Const S_SERQFUL As Short = (-4) '  Queue full
    Public Const S_SERBDNT As Short = (-5) '  Invalid note
    Public Const S_SERDLN As Short = (-6) '  Invalid note length
    Public Const S_SERDCC As Short = (-7) '  Invalid note count
    Public Const S_SERDTP As Short = (-8) '  Invalid tempo
    Public Const S_SERDVL As Short = (-9) '  Invalid volume
    Public Const S_SERDMD As Short = (-10) '  Invalid mode
    Public Const S_SERDSH As Short = (-11) '  Invalid shape
    Public Const S_SERDPT As Short = (-12) '  Invalid pitch
    Public Const S_SERDFQ As Short = (-13) '  Invalid frequency
    Public Const S_SERDDR As Short = (-14) '  Invalid duration
    Public Const S_SERDSR As Short = (-15) '  Invalid source
    Public Const S_SERDST As Short = (-16) '  Invalid state

    Public Const NMPWAIT_WAIT_FOREVER As Short = &HFFFFS
    Public Const NMPWAIT_NOWAIT As Short = &H1S
    Public Const NMPWAIT_USE_DEFAULT_WAIT As Short = &H0S
    Public Const FS_CASE_IS_PRESERVED As Short = FILE_CASE_PRESERVED_NAMES
    Public Const FS_CASE_SENSITIVE As Short = FILE_CASE_SENSITIVE_SEARCH
    Public Const FS_UNICODE_STORED_ON_DISK As Short = FILE_UNICODE_ON_DISK
    Public Const FS_PERSISTENT_ACLS As Short = FILE_PERSISTENT_ACLS

    Public Const SECTION_QUERY As Short = &H1S
    Public Const SECTION_MAP_WRITE As Short = &H2S
    Public Const SECTION_MAP_READ As Short = &H4S
    Public Const SECTION_MAP_EXECUTE As Short = &H8S
    Public Const SECTION_EXTEND_SIZE As Short = &H10S
    Public Const SECTION_ALL_ACCESS As Boolean = STANDARD_RIGHTS_REQUIRED Or SECTION_QUERY Or SECTION_MAP_WRITE Or SECTION_MAP_READ Or SECTION_MAP_EXECUTE Or SECTION_EXTEND_SIZE

    Public Const FILE_MAP_COPY As Short = SECTION_QUERY
    Public Const FILE_MAP_WRITE As Short = SECTION_MAP_WRITE
    Public Const FILE_MAP_READ As Short = SECTION_MAP_READ
    Public Const FILE_MAP_ALL_ACCESS As Boolean = SECTION_ALL_ACCESS

    ' OpenFile() Flags
    Public Const OF_READ As Short = &H0S
    Public Const OF_WRITE As Short = &H1S
    Public Const OF_READWRITE As Short = &H2S
    Public Const OF_SHARE_COMPAT As Short = &H0S
    Public Const OF_SHARE_EXCLUSIVE As Short = &H10S
    Public Const OF_SHARE_DENY_WRITE As Short = &H20S
    Public Const OF_SHARE_DENY_READ As Short = &H30S
    Public Const OF_SHARE_DENY_NONE As Short = &H40S
    Public Const OF_PARSE As Short = &H100S
    Public Const OF_DELETE As Short = &H200S
    Public Const OF_VERIFY As Short = &H400S
    Public Const OF_CANCEL As Short = &H800S
    Public Const OF_CREATE As Short = &H1000S
    Public Const OF_PROMPT As Short = &H2000S
    Public Const OF_EXIST As Short = &H4000S
    Public Const OF_REOPEN As Short = &H8000S

    Public Const OFS_MAXPATHNAME As Short = 128

    ' OpenFile() Structure
    Public Structure OFSTRUCT
        Dim cBytes As Byte
        Dim fFixedDisk As Byte
        Dim nErrCode As Short
        Dim Reserved1 As Short
        Dim Reserved2 As Short
        <VBFixedArray(OFS_MAXPATHNAME)> Dim szPathName() As Byte

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim szPathName(OFS_MAXPATHNAME)
        End Sub
    End Structure

    Public Declare Function InterlockedIncrement Lib "kernel32" (ByRef lpAddend As Integer) As Integer
    Public Declare Function InterlockedDecrement Lib "kernel32" (ByRef lpAddend As Integer) As Integer
    Public Declare Function InterlockedExchange Lib "kernel32" (ByRef Target As Integer, Value As Integer) As Integer

    ' Loader Routines
    Public Declare Function GetModuleFileName Lib "kernel32" Alias "GetModuleFileNameA" (hModule As Integer, lpFileName As String, nSize As Integer) As Integer
    Public Declare Function GetModuleHandle Lib "kernel32" Alias "GetModuleHandleA" (lpModuleName As String) As Integer

    'UPGRADE_WARNING: ?? PROCESS_INFORMATION ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? STARTUPINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_WARNING: ?? SECURITY_ATTRIBUTES ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? SECURITY_ATTRIBUTES ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreateProcess Lib "kernel32" Alias "CreateProcessA" (lpApplicationName As String, lpCommandLine As String, ByRef lpProcessAttributes As SECURITY_ATTRIBUTES, ByRef lpThreadAttributes As SECURITY_ATTRIBUTES, bInheritHandles As Integer, dwCreationFlags As Integer, ByRef lpEnvironment As Object, lpCurrentDriectory As String, ByRef lpStartupInfo As STARTUPINFO, ByRef lpProcessInformation As PROCESS_INFORMATION) As Integer

    Public Declare Function SetProcessShutdownParameters Lib "kernel32" (dwLevel As Integer, dwFlags As Integer) As Integer
    Public Declare Function GetProcessShutdownParameters Lib "kernel32" (ByRef lpdwLevel As Integer, ByRef lpdwFlags As Integer) As Integer

    Public Declare Sub FatalAppExit Lib "kernel32" Alias "FatalAppExitA" (uAction As Integer, lpMessageText As String)
    'UPGRADE_WARNING: ?? STARTUPINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Sub GetStartupInfo Lib "kernel32" Alias "GetStartupInfoA" (ByRef lpStartupInfo As STARTUPINFO)
    Public Declare Function GetCommandLine Lib "kernel32" Alias "GetCommandLineA" () As String
    Public Declare Function GetEnvironmentVariable Lib "kernel32" Alias "GetEnvironmentVariableA" (lpName As String, lpBuffer As String, nSize As Integer) As Integer
    Public Declare Function SetEnvironmentVariable Lib "kernel32" Alias "SetEnvironmentVariableA" (lpName As String, lpValue As String) As Integer
    Public Declare Function ExpandEnvironmentStrings Lib "kernel32" Alias "ExpandEnvironmentStringsA" (lpSrc As String, lpDst As String, nSize As Integer) As Integer

    Public Declare Function LoadLibrary Lib "kernel32" Alias "LoadLibraryA" (lpLibFileName As String) As Integer
    Public Declare Function LoadLibraryEx Lib "kernel32" Alias "LoadLibraryExA" (lpLibFileName As String, hFile As Integer, dwFlags As Integer) As Integer

    Public Const DONT_RESOLVE_DLL_REFERENCES As Short = &H1S

    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function LoadModule Lib "kernel32" (lpModuleName As String, ByRef lpParameterBlock As Object) As Integer
    Public Declare Function FreeLibrary Lib "kernel32" (hLibModule As Integer) As Integer
    Public Declare Function WinExec Lib "kernel32" (lpCmdLine As String, nCmdShow As Integer) As Integer

    Public Declare Sub DebugBreak Lib "kernel32" ()
    Public Declare Function ContinueDebugEvent Lib "kernel32" (dwProcessId As Integer, dwThreadId As Integer, dwContinueStatus As Integer) As Integer
    Public Declare Function DebugActiveProcess Lib "kernel32" (dwProcessId As Integer) As Integer

    Public Structure CRITICAL_SECTION
        Dim dummy As Integer
    End Structure

    'UPGRADE_WARNING: ?? CRITICAL_SECTION ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Sub InitializeCriticalSection Lib "kernel32" (ByRef lpCriticalSection As CRITICAL_SECTION)

    'UPGRADE_WARNING: ?? CRITICAL_SECTION ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Sub EnterCriticalSection Lib "kernel32" (ByRef lpCriticalSection As CRITICAL_SECTION)
    'UPGRADE_WARNING: ?? CRITICAL_SECTION ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Sub LeaveCriticalSection Lib "kernel32" (ByRef lpCriticalSection As CRITICAL_SECTION)
    'UPGRADE_WARNING: ?? CRITICAL_SECTION ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Sub DeleteCriticalSection Lib "kernel32" (ByRef lpCriticalSection As CRITICAL_SECTION)
    Public Declare Function SetEvent Lib "kernel32" (hEvent As Integer) As Integer
    Public Declare Function ResetEvent Lib "kernel32" (hEvent As Integer) As Integer
    Public Declare Function PulseEvent Lib "kernel32" (hEvent As Integer) As Integer
    Public Declare Function ReleaseSemaphore Lib "kernel32" (hSemaphore As Integer, lReleaseCount As Integer, ByRef lpPreviousCount As Integer) As Integer
    Public Declare Function ReleaseMutex Lib "kernel32" (hMutex As Integer) As Integer
    Public Declare Function WaitForSingleObject Lib "kernel32" (hHandle As Integer, dwMilliseconds As Integer) As Integer
    Public Declare Function WaitForMultipleObjects Lib "kernel32" (nCount As Integer, ByRef lpHandles As Integer, bWaitAll As Integer, dwMilliseconds As Integer) As Integer
    Public Declare Sub Sleep Lib "kernel32" (dwMilliseconds As Integer)
    Public Declare Sub OutputDebugString Lib "kernel32" Alias "OutputDebugStringA" (lpOutputString As String)
    Public Declare Function GetVersion Lib "kernel32" () As Integer

    'UPGRADE_WARNING: ?? OFSTRUCT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function OpenFile Lib "kernel32" (lpFileName As String, ByRef lpReOpenBuff As OFSTRUCT, wStyle As Integer) As Integer

    ' GetTempFileName() Flags
    '
    Public Const TF_FORCEDRIVE As Short = &H80S

    Public Declare Function GetTempFileName Lib "kernel32" Alias "GetTempFileNameA" (lpszPath As String, lpPrefixString As String, wUnique As Integer, lpTempFileName As String) As Integer
    Public Declare Function SetHandleCount Lib "kernel32" (wNumber As Integer) As Integer
    Public Declare Function GetLogicalDrives Lib "kernel32" () As Integer
    Public Declare Function LockFile Lib "kernel32" (hFile As Integer, dwFileOffsetLow As Integer, dwFileOffsetHigh As Integer, nNumberOfBytesToLockLow As Integer, nNumberOfBytesToLockHigh As Integer) As Integer
    Public Declare Function UnlockFile Lib "kernel32" (hFile As Integer, dwFileOffsetLow As Integer, dwFileOffsetHigh As Integer, nNumberOfBytesToUnlockLow As Integer, nNumberOfBytesToUnlockHigh As Integer) As Integer
    'UPGRADE_WARNING: ?? OVERLAPPED ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function LockFileEx Lib "kernel32" (hFile As Integer, dwFlags As Integer, dwReserved As Integer, nNumberOfBytesToLockLow As Integer, nNumberOfBytesToLockHigh As Integer, ByRef lpOverlapped As OVERLAPPED) As Integer

    Public Const LOCKFILE_FAIL_IMMEDIATELY As Short = &H1S
    Public Const LOCKFILE_EXCLUSIVE_LOCK As Short = &H2S

    'UPGRADE_WARNING: ?? OVERLAPPED ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function UnlockFileEx Lib "kernel32" (hFile As Integer, dwReserved As Integer, nNumberOfBytesToUnlockLow As Integer, nNumberOfBytesToUnlockHigh As Integer, ByRef lpOverlapped As OVERLAPPED) As Integer

    Public Structure BY_HANDLE_FILE_INFORMATION
        Dim dwFileAttributes As Integer
        Dim ftCreationTime As FILETIME
        Dim ftLastAccessTime As FILETIME
        Dim ftLastWriteTime As FILETIME
        Dim dwVolumeSerialNumber As Integer
        Dim nFileSizeHigh As Integer
        Dim nFileSizeLow As Integer
        Dim nNumberOfLinks As Integer
        Dim nFileIndexHigh As Integer
        Dim nFileIndexLow As Integer
    End Structure

    'UPGRADE_WARNING: ?? BY_HANDLE_FILE_INFORMATION ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetFileInformationByHandle Lib "kernel32" (hFile As Integer, ByRef lpFileInformation As BY_HANDLE_FILE_INFORMATION) As Integer
    Public Declare Function GetFileType Lib "kernel32" (hFile As Integer) As Integer
    Public Declare Function GetFileSize Lib "kernel32" (hFile As Integer, ByRef lpFileSizeHigh As Integer) As Integer
    Public Declare Function GetStdHandle Lib "kernel32" (nStdHandle As Integer) As Integer
    Public Declare Function SetStdHandle Lib "kernel32" (nStdHandle As Integer, nHandle As Integer) As Integer
    'UPGRADE_WARNING: ?? OVERLAPPED ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function WriteFile Lib "kernel32" (hFile As Integer, ByRef lpBuffer As Object, nNumberOfBytesToWrite As Integer, ByRef lpNumberOfBytesWritten As Integer, ByRef lpOverlapped As OVERLAPPED) As Integer
    'UPGRADE_WARNING: ?? OVERLAPPED ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function ReadFile Lib "kernel32" (hFile As Integer, ByRef lpBuffer As Object, nNumberOfBytesToRead As Integer, ByRef lpNumberOfBytesRead As Integer, ByRef lpOverlapped As OVERLAPPED) As Integer
    Public Declare Function FlushFileBuffers Lib "kernel32" (hFile As Integer) As Integer
    'UPGRADE_WARNING: ?? OVERLAPPED ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function DeviceIoControl Lib "kernel32" (hDevice As Integer, dwIoControlCode As Integer, ByRef lpInBuffer As Object, nInBufferSize As Integer, ByRef lpOutBuffer As Object, nOutBufferSize As Integer, ByRef lpBytesReturned As Integer, ByRef lpOverlapped As OVERLAPPED) As Integer
    Public Declare Function SetEndOfFile Lib "kernel32" (hFile As Integer) As Integer
    Public Declare Function SetFilePointer Lib "kernel32" (hFile As Integer, lDistanceToMove As Integer, ByRef lpDistanceToMoveHigh As Integer, dwMoveMethod As Integer) As Integer
    Public Declare Function FindClose Lib "kernel32" (hFindFile As Integer) As Integer
    'UPGRADE_WARNING: ?? FILETIME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? FILETIME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? FILETIME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetFileTime Lib "kernel32" (hFile As Integer, ByRef lpCreationTime As FILETIME, ByRef lpLastAccessTime As FILETIME, ByRef lpLastWriteTime As FILETIME) As Integer
    'UPGRADE_WARNING: ?? FILETIME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? FILETIME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? FILETIME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetFileTime Lib "kernel32" (hFile As Integer, ByRef lpCreationTime As FILETIME, ByRef lpLastAccessTime As FILETIME, ByRef lpLastWriteTime As FILETIME) As Integer
    Public Declare Function CloseHandle Lib "kernel32" (hObject As Integer) As Integer
    Public Declare Function DuplicateHandle Lib "kernel32" (hSourceProcessHandle As Integer, hSourceHandle As Integer, hTargetProcessHandle As Integer, ByRef lpTargetHandle As Integer, dwDesiredAccess As Integer, bInheritHandle As Integer, dwOptions As Integer) As Integer

    Public Declare Function GetDriveType Lib "kernel32" Alias "GetDriveTypeA" (nDrive As String) As Integer

    Public Declare Function GlobalAlloc Lib "kernel32" (wFlags As Integer, dwBytes As Integer) As Integer
    Public Declare Function GlobalFree Lib "kernel32" (hMem As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function GlobalHandle Lib "kernel32" (ByRef wMem As Object) As Integer
    Public Declare Function GlobalLock Lib "kernel32" (hMem As Integer) As Integer
    Public Declare Function GlobalReAlloc Lib "kernel32" (hMem As Integer, dwBytes As Integer, wFlags As Integer) As Integer

    Public Declare Function GlobalSize Lib "kernel32" (hMem As Integer) As Integer
    Public Declare Function GlobalUnlock Lib "kernel32" (hMem As Integer) As Integer
    Public Declare Function GlobalFlags Lib "kernel32" (hMem As Integer) As Integer
    'UPGRADE_WARNING: ?? MEMORYSTATUS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Sub GlobalMemoryStatus Lib "kernel32" (ByRef lpBuffer As MEMORYSTATUS)

    Public Const LNOTIFY_OUTOFMEM As Short = 0
    Public Const LNOTIFY_MOVE As Short = 1
    Public Const LNOTIFY_DISCARD As Short = 2

    Public Declare Function LocalAlloc Lib "kernel32" (wFlags As Integer, wBytes As Integer) As Integer
    Public Declare Function LocalFree Lib "kernel32" (hMem As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function LocalHandle Lib "kernel32" (ByRef wMem As Object) As Integer
    Public Declare Function LocalLock Lib "kernel32" (hMem As Integer) As Integer
    Public Declare Function LocalReAlloc Lib "kernel32" (hMem As Integer, wBytes As Integer, wFlags As Integer) As Integer

    Public Declare Function LocalSize Lib "kernel32" (hMem As Integer) As Integer
    Public Declare Function LocalUnlock Lib "kernel32" (hMem As Integer) As Integer
    Public Declare Function LocalFlags Lib "kernel32" (hMem As Integer) As Integer

    Public Structure MEMORY_BASIC_INFORMATION
        Dim BaseAddress As Integer
        Dim AllocationBase As Integer
        Dim AllocationProtect As Integer
        Dim RegionSize As Integer
        Dim State As Integer
        Dim Protect As Integer
        Dim lType As Integer
    End Structure

    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function FlushInstructionCache Lib "kernel32" (hProcess As Integer, ByRef lpBaseAddress As Object, dwSize As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function VirtualAlloc Lib "kernel32" (ByRef lpAddress As Object, dwSize As Integer, flAllocationType As Integer, flProtect As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function VirtualFree Lib "kernel32" (ByRef lpAddress As Object, dwSize As Integer, dwFreeType As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function VirtualProtect Lib "kernel32" (ByRef lpAddress As Object, dwSize As Integer, flNewProtect As Integer, ByRef lpflOldProtect As Integer) As Integer
    'UPGRADE_WARNING: ?? MEMORY_BASIC_INFORMATION ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function VirtualQuery Lib "kernel32" (ByRef lpAddress As Object, ByRef lpBuffer As MEMORY_BASIC_INFORMATION, dwLength As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function VirtualProtectEx Lib "kernel32" (hProcess As Integer, ByRef lpAddress As Object, dwSize As Integer, flNewProtect As Integer, ByRef lpflOldProtect As Integer) As Integer
    'UPGRADE_WARNING: ?? MEMORY_BASIC_INFORMATION ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function VirtualQueryEx Lib "kernel32" (hProcess As Integer, ByRef lpAddress As Object, ByRef lpBuffer As MEMORY_BASIC_INFORMATION, dwLength As Integer) As Integer
    Public Declare Function HeapCreate Lib "kernel32" (flOptions As Integer, dwInitialSize As Integer, dwMaximumSize As Integer) As Integer
    Public Declare Function HeapDestroy Lib "kernel32" (hHeap As Integer) As Integer
    Public Declare Function HeapAlloc Lib "kernel32" (hHeap As Integer, dwFlags As Integer, dwBytes As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function HeapReAlloc Lib "kernel32" (hHeap As Integer, dwFlags As Integer, ByRef lpMem As Object, dwBytes As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function HeapFree Lib "kernel32" (hHeap As Integer, dwFlags As Integer, ByRef lpMem As Object) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function HeapSize Lib "kernel32" (hHeap As Integer, dwFlags As Integer, ByRef lpMem As Object) As Integer
    Public Declare Function GetProcessHeap Lib "kernel32" () As Integer
    'UPGRADE_WARNING: ?? FILETIME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? FILETIME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? FILETIME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? FILETIME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetProcessTimes Lib "kernel32" (hProcess As Integer, ByRef lpCreationTime As FILETIME, ByRef lpExitTime As FILETIME, ByRef lpKernelTime As FILETIME, ByRef lpUserTime As FILETIME) As Integer
    Public Declare Function OpenProcess Lib "kernel32" (dwDesiredAccess As Integer, bInheritHandle As Integer, dwProcessId As Integer) As Integer
    Public Declare Function GetCurrentProcess Lib "kernel32" () As Integer
    Public Declare Function GetCurrentProcessId Lib "kernel32" () As Integer
    Public Declare Sub ExitProcess Lib "kernel32" (uExitCode As Integer)
    Public Declare Function TerminateProcess Lib "kernel32" (hProcess As Integer, uExitCode As Integer) As Integer
    Public Declare Function GetExitCodeProcess Lib "kernel32" (hProcess As Integer, ByRef lpExitCode As Integer) As Integer

    Public Declare Function GetLastError Lib "kernel32" () As Integer
    Public Declare Sub SetLastError Lib "kernel32" (dwErrCode As Integer)

    Public Const SLE_ERROR As Short = &H1S
    Public Const SLE_MINORERROR As Short = &H2S
    Public Const SLE_WARNING As Short = &H3S

    Public Declare Sub SetLastErrorEx Lib "user32" (dwErrCode As Integer, dwType As Integer)
    'UPGRADE_WARNING: ?? OVERLAPPED ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetOverlappedResult Lib "kernel32" (hFile As Integer, ByRef lpOverlapped As OVERLAPPED, ByRef lpNumberOfBytesTransferred As Integer, bWait As Integer) As Integer

    Public Const SEM_FAILCRITICALERRORS As Short = &H1S
    Public Const SEM_NOGPFAULTERRORBOX As Short = &H2S
    Public Const SEM_NOOPENFILEERRORBOX As Short = &H8000S

    ' Predefined Resource Types
    Public Const RT_CURSOR As Short = 1
    Public Const RT_BITMAP As Short = 2
    Public Const RT_ICON As Short = 3
    Public Const RT_MENU As Short = 4
    Public Const RT_DIALOG As Short = 5
    Public Const RT_STRING As Short = 6
    Public Const RT_FONTDIR As Short = 7
    Public Const RT_FONT As Short = 8
    Public Const RT_ACCELERATOR As Short = 9
    Public Const RT_RCDATA As Short = 10


    Public Const DDD_RAW_TARGET_PATH As Short = &H1S
    Public Const DDD_REMOVE_DEFINITION As Short = &H2S
    Public Const DDD_EXACT_MATCH_ON_REMOVE As Short = &H4S

    Public Const MAX_PATH As Short = 260


    Public Const MOVEFILE_REPLACE_EXISTING As Short = &H1S
    Public Const MOVEFILE_COPY_ALLOWED As Short = &H2S
    Public Const MOVEFILE_DELAY_UNTIL_REBOOT As Short = &H4S

    Public Structure EVENTLOGRECORD
        Dim Length As Integer '  Length of full record
        Dim Reserved As Integer '  Used by the service
        Dim RecordNumber As Integer '  Absolute record number
        Dim TimeGenerated As Integer '  Seconds since 1-1-1970
        Dim TimeWritten As Integer 'Seconds since 1-1-1970
        Dim EventID As Integer
        Dim EventType As Short
        Dim NumStrings As Short
        Dim EventCategory As Short
        Dim ReservedFlags As Short '  For use with paired events (auditing)
        Dim ClosingRecordNumber As Integer 'For use with paired events (auditing)
        Dim StringOffset As Integer '  Offset from beginning of record
        Dim UserSidLength As Integer
        Dim UserSidOffset As Integer
        Dim DataLength As Integer
        Dim DataOffset As Integer '  Offset from beginning of record
    End Structure

    'UPGRADE_WARNING: ?? SECURITY_ATTRIBUTES ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreateNamedPipe Lib "kernel32" Alias "CreateNamedPipeA" (lpName As String, dwOpenMode As Integer, dwPipeMode As Integer, nMaxInstances As Integer, nOutBufferSize As Integer, nInBufferSize As Integer, nDefaultTimeOut As Integer, ByRef lpSecurityAttributes As SECURITY_ATTRIBUTES) As Integer
    Public Declare Function GetNamedPipeHandleState Lib "kernel32" Alias "GetNamedPipeHandleStateA" (hNamedPipe As Integer, ByRef lpState As Integer, ByRef lpCurInstances As Integer, ByRef lpMaxCollectionCount As Integer, ByRef lpCollectDataTimeout As Integer, lpUserName As String, nMaxUserNameSize As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function CallNamedPipe Lib "kernel32" Alias "CallNamedPipeA" (lpNamedPipeName As String, ByRef lpInBuffer As Object, nInBufferSize As Integer, ByRef lpOutBuffer As Object, nOutBufferSize As Integer, ByRef lpBytesRead As Integer, nTimeOut As Integer) As Integer
    Public Declare Function WaitNamedPipe Lib "kernel32" Alias "WaitNamedPipeA" (lpNamedPipeName As String, nTimeOut As Integer) As Integer
    Public Declare Function SetVolumeLabel Lib "kernel32" Alias "SetVolumeLabelA" (lpRootPathName As String, lpVolumeName As String) As Integer
    Public Declare Sub SetFileApisToOEM Lib "kernel32" ()
    Public Declare Function GetVolumeInformation Lib "kernel32" Alias "GetVolumeInformationA" (lpRootPathName As String, lpVolumeNameBuffer As String, nVolumeNameSize As Integer, ByRef lpVolumeSerialNumber As Integer, ByRef lpMaximumComponentLength As Integer, ByRef lpFileSystemFlags As Integer, lpFileSystemNameBuffer As String, nFileSystemNameSize As Integer) As Integer
    Public Declare Function ClearEventLog Lib "ADVAPI32.DLL" Alias "ClearEventLogA" (hEventLog As Integer, lpBackupFileName As String) As Integer
    Public Declare Function BackupEventLog Lib "ADVAPI32.DLL" Alias "BackupEventLogA" (hEventLog As Integer, lpBackupFileName As String) As Integer
    Public Declare Function CloseEventLog Lib "ADVAPI32.DLL" (hEventLog As Integer) As Integer
    Public Declare Function DeregisterEventSource Lib "ADVAPI32.DLL" (hEventLog As Integer) As Integer
    Public Declare Function GetNumberOfEventLogRecords Lib "ADVAPI32.DLL" (hEventLog As Integer, ByRef NumberOfRecords As Integer) As Integer
    Public Declare Function GetOldestEventLogRecord Lib "ADVAPI32.DLL" (hEventLog As Integer, ByRef OldestRecord As Integer) As Integer
    Public Declare Function OpenEventLog Lib "ADVAPI32.DLL" (lpUNCServerName As String, lpSourceName As String) As Integer
    Public Declare Function RegisterEventSource Lib "ADVAPI32.DLL" Alias "RegisterEventSourceA" (lpUNCServerName As String, lpSourceName As String) As Integer
    Public Declare Function OpenBackupEventLog Lib "ADVAPI32.DLL" Alias "OpenBackupEventLogA" (lpUNCServerName As String, lpFileName As String) As Integer
    'UPGRADE_WARNING: ?? EVENTLOGRECORD ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ReadEventLog Lib "ADVAPI32.DLL" Alias "ReadEventLogA" (hEventLog As Integer, dwReadFlags As Integer, dwRecordOffset As Integer, ByRef lpBuffer As EVENTLOGRECORD, nNumberOfBytesToRead As Integer, ByRef pnBytesRead As Integer, ByRef pnMinNumberOfBytesNeeded As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function ReportEvent Lib "ADVAPI32.DLL" Alias "ReportEventA" (hEventLog As Integer, wType As Integer, wCategory As Integer, dwEventID As Integer, ByRef lpUserSid As Object, wNumStrings As Integer, dwDataSize As Integer, lpStrings As Integer, ByRef lpRawData As Object) As Integer

    ' Security APIs
    Public Const TokenUser As Short = 1
    Public Const TokenGroups As Short = 2
    Public Const TokenPrivileges As Short = 3
    Public Const TokenOwner As Short = 4
    Public Const TokenPrimaryGroup As Short = 5
    Public Const TokenDefaultDacl As Short = 6
    Public Const TokenSource As Short = 7
    Public Const TokenType As Short = 8
    Public Const TokenImpersonationLevel As Short = 9
    Public Const TokenStatistics As Short = 10

    Public Structure TOKEN_GROUPS
        Dim GroupCount As Integer
        <VBFixedArray(ANYSIZE_ARRAY)> Dim Groups() As SID_AND_ATTRIBUTES

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim Groups(ANYSIZE_ARRAY)
        End Sub
    End Structure

    Public Declare Function DuplicateToken Lib "ADVAPI32.DLL" (ExistingTokenHandle As Integer, ByRef Impersonationlevel As Short, ByRef DuplicateTokenHandle As Integer) As Integer
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetKernelObjectSecurity Lib "ADVAPI32.DLL" (handle As Integer, RequestedInformation As Integer, ByRef pSecurityDescriptor As SECURITY_DESCRIPTOR, nLength As Integer, ByRef lpnLengthNeeded As Integer) As Integer
    Public Declare Function ImpersonateNamedPipeClient Lib "ADVAPI32.DLL" (hNamedPipe As Integer) As Integer
    Public Declare Function ImpersonateSelf Lib "ADVAPI32.DLL" (ByRef Impersonationlevel As Short) As Integer
    Public Declare Function RevertToSelf Lib "ADVAPI32.DLL" () As Integer
    'UPGRADE_WARNING: ?? PRIVILEGE_SET ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? GENERIC_MAPPING ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function AccessCheck Lib "ADVAPI32.DLL" (ByRef pSecurityDescriptor As SECURITY_DESCRIPTOR, ClientToken As Integer, DesiredAccess As Integer, ByRef GenericMapping As GENERIC_MAPPING, ByRef PrivilegeSet As PRIVILEGE_SET, ByRef PrivilegeSetLength As Integer, ByRef GrantedAccess As Integer, Status As Integer) As Integer

    Public Structure TOKEN_PRIVILEGES
        Dim PrivilegeCount As Integer
        <VBFixedArray(ANYSIZE_ARRAY)> Dim Privileges() As LUID_AND_ATTRIBUTES

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim Privileges(ANYSIZE_ARRAY)
        End Sub
    End Structure

    Public Declare Function OpenProcessToken Lib "ADVAPI32.DLL" (ProcessHandle As Integer, DesiredAccess As Integer, ByRef TokenHandle As Integer) As Integer
    Public Declare Function OpenThreadToken Lib "ADVAPI32.DLL" (ThreadHandle As Integer, DesiredAccess As Integer, OpenAsSelf As Integer, ByRef TokenHandle As Integer) As Integer

    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function GetTokenInformation Lib "ADVAPI32.DLL" (TokenHandle As Integer, ByRef TokenInformationClass As Short, ByRef TokenInformation As Object, TokenInformationLength As Integer, ByRef ReturnLength As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function SetTokenInformation Lib "ADVAPI32.DLL" (TokenHandle As Integer, ByRef TokenInformationClass As Short, ByRef TokenInformation As Object, TokenInformationLength As Integer) As Integer
    'UPGRADE_WARNING: ?? TOKEN_PRIVILEGES ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? TOKEN_PRIVILEGES ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function AdjustTokenPrivileges Lib "ADVAPI32.DLL" (TokenHandle As Integer, DisableAllPrivileges As Integer, ByRef NewState As TOKEN_PRIVILEGES, BufferLength As Integer, ByRef PreviousState As TOKEN_PRIVILEGES, ByRef ReturnLength As Integer) As Integer
    'UPGRADE_WARNING: ?? TOKEN_GROUPS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? TOKEN_GROUPS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function AdjustTokenGroups Lib "ADVAPI32.DLL" (TokenHandle As Integer, ResetToDefault As Integer, ByRef NewState As TOKEN_GROUPS, BufferLength As Integer, ByRef PreviousState As TOKEN_GROUPS, ByRef ReturnLength As Integer) As Integer
    'UPGRADE_WARNING: ?? PRIVILEGE_SET ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function PrivilegeCheck Lib "ADVAPI32.DLL" (ClientToken As Integer, ByRef RequiredPrivileges As PRIVILEGE_SET, pfResult As Integer) As Integer
    'UPGRADE_WARNING: ?? GENERIC_MAPPING ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function AccessCheckAndAuditAlarm Lib "ADVAPI32.DLL" Alias "AccessCheckAndAuditAlarmA" (SubsystemName As String, ByRef HandleId As Object, ObjectTypeName As String, ObjectName As String, ByRef SecurityDescriptor As SECURITY_DESCRIPTOR, DesiredAccess As Integer, ByRef GenericMapping As GENERIC_MAPPING, ObjectCreation As Integer, ByRef GrantedAccess As Integer, AccessStatus As Integer, pfGenerateOnClose As Integer) As Integer
    'UPGRADE_WARNING: ?? PRIVILEGE_SET ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function ObjectOpenAuditAlarm Lib "kernel32" Alias "ObjectOpenAuditAlarmA" (SubsystemName As String, ByRef HandleId As Object, ObjectTypeName As String, ObjectName As String, ByRef pSecurityDescriptor As SECURITY_DESCRIPTOR, ClientToken As Integer, DesiredAccess As Integer, GrantedAccess As Integer, ByRef Privileges As PRIVILEGE_SET, ObjectCreation As Integer, AccessGranted As Integer, GenerateOnClose As Integer) As Integer
    'UPGRADE_WARNING: ?? PRIVILEGE_SET ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function ObjectPrivilegeAuditAlarm Lib "ADVAPI32.DLL" Alias "ObjectPrivilegeAuditAlarmA" (SubsystemName As String, ByRef HandleId As Object, ClientToken As Integer, DesiredAccess As Integer, ByRef Privileges As PRIVILEGE_SET, AccessGranted As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function ObjectCloseAuditAlarm Lib "ADVAPI32.DLL" Alias "ObjectCloseAuditAlarmA" (SubsystemName As String, ByRef HandleId As Object, GenerateOnClose As Integer) As Integer
    'UPGRADE_WARNING: ?? PRIVILEGE_SET ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function PrivilegedServiceAuditAlarm Lib "ADVAPI32.DLL" Alias "PrivilegedServiceAuditAlarmA" (SubsystemName As String, ServiceName As String, ClientToken As Integer, ByRef Privileges As PRIVILEGE_SET, AccessGranted As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function IsValidSid Lib "ADVAPI32.DLL" (ByRef pSid As Object) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function EqualSid Lib "ADVAPI32.DLL" (ByRef pSid1 As Object, ByRef pSid2 As Object) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function EqualPrefixSid Lib "ADVAPI32.DLL" (ByRef pSid1 As Object, ByRef pSid2 As Object) As Integer
    Public Declare Function GetSidLengthRequired Lib "ADVAPI32.DLL" (nSubAuthorityCount As Byte) As Integer
    'UPGRADE_WARNING: ?? SID_IDENTIFIER_AUTHORITY ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function AllocateAndInitializeSid Lib "ADVAPI32.DLL" (ByRef pIdentifierAuthority As SID_IDENTIFIER_AUTHORITY, nSubAuthorityCount As Byte, nSubAuthority0 As Integer, nSubAuthority1 As Integer, nSubAuthority2 As Integer, nSubAuthority3 As Integer, nSubAuthority4 As Integer, nSubAuthority5 As Integer, nSubAuthority6 As Integer, nSubAuthority7 As Integer, ByRef lpPSid As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Sub FreeSid Lib "ADVAPI32.DLL" (ByRef pSid As Object)
    'UPGRADE_WARNING: ?? SID_IDENTIFIER_AUTHORITY ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function InitializeSid Lib "ADVAPI32.DLL" (ByRef Sid As Object, ByRef pIdentifierAuthority As SID_IDENTIFIER_AUTHORITY, nSubAuthorityCount As Byte) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function GetSidIdentifierAuthority Lib "ADVAPI32.DLL" (ByRef pSid As Object) As SID_IDENTIFIER_AUTHORITY
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function GetSidSubAuthority Lib "ADVAPI32.DLL" (ByRef pSid As Object, nSubAuthority As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function GetSidSubAuthorityCount Lib "ADVAPI32.DLL" (ByRef pSid As Object) As Byte
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function GetLengthSid Lib "ADVAPI32.DLL" (ByRef pSid As Object) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function CopySid Lib "ADVAPI32.DLL" (nDestinationSidLength As Integer, ByRef pDestinationSid As Object, ByRef pSourceSid As Object) As Integer
    Public Declare Function AreAllAccessesGranted Lib "ADVAPI32.DLL" (GrantedAccess As Integer, DesiredAccess As Integer) As Integer
    Public Declare Function AreAnyAccessesGranted Lib "ADVAPI32.DLL" (GrantedAccess As Integer, DesiredAccess As Integer) As Integer
    'UPGRADE_WARNING: ?? GENERIC_MAPPING ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Sub MapGenericMask Lib "ADVAPI32.DLL" (ByRef AccessMask As Integer, ByRef GenericMapping As GENERIC_MAPPING)
    'UPGRADE_WARNING: ?? ACL ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function IsValidAcl Lib "ADVAPI32.DLL" (ByRef pAcl As ACL) As Integer
    'UPGRADE_WARNING: ?? ACL ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function InitializeAcl Lib "ADVAPI32.DLL" (ByRef pAcl As ACL, nAclLength As Integer, dwAclRevision As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_WARNING: ?? ACL ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetAclInformation Lib "ADVAPI32.DLL" (ByRef pAcl As ACL, ByRef pAclInformation As Object, nAclInformationLength As Integer, dwAclInformationClass As Short) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_WARNING: ?? ACL ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetAclInformation Lib "ADVAPI32.DLL" (ByRef pAcl As ACL, ByRef pAclInformation As Object, nAclInformationLength As Integer, dwAclInformationClass As Short) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_WARNING: ?? ACL ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function AddAce Lib "ADVAPI32.DLL" (ByRef pAcl As ACL, dwAceRevision As Integer, dwStartingAceIndex As Integer, ByRef pAceList As Object, nAceListLength As Integer) As Integer
    'UPGRADE_WARNING: ?? ACL ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function DeleteAce Lib "ADVAPI32.DLL" (ByRef pAcl As ACL, dwAceIndex As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_WARNING: ?? ACL ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetAce Lib "ADVAPI32.DLL" (ByRef pAcl As ACL, dwAceIndex As Integer, ByRef pAce As Object) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_WARNING: ?? ACL ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function AddAccessAllowedAce Lib "ADVAPI32.DLL" (ByRef pAcl As ACL, dwAceRevision As Integer, AccessMask As Integer, ByRef pSid As Object) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_WARNING: ?? ACL ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function AddAccessDeniedAce Lib "ADVAPI32.DLL" (ByRef pAcl As ACL, dwAceRevision As Integer, AccessMask As Integer, ByRef pSid As Object) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_WARNING: ?? ACL ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function AddAuditAccessAce Lib "ADVAPI32.DLL" (ByRef pAcl As ACL, dwAceRevision As Integer, dwAccessMask As Integer, ByRef pSid As Object, bAuditSuccess As Integer, bAuditFailure As Integer) As Integer
    'UPGRADE_WARNING: ?? ACL ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function FindFirstFreeAce Lib "ADVAPI32.DLL" (ByRef pAcl As ACL, ByRef pAce As Integer) As Integer
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function InitializeSecurityDescriptor Lib "ADVAPI32.DLL" (ByRef pSecurityDescriptor As SECURITY_DESCRIPTOR, dwRevision As Integer) As Integer
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function IsValidSecurityDescriptor Lib "ADVAPI32.DLL" (ByRef pSecurityDescriptor As SECURITY_DESCRIPTOR) As Integer
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetSecurityDescriptorLength Lib "ADVAPI32.DLL" (ByRef pSecurityDescriptor As SECURITY_DESCRIPTOR) As Integer

    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetSecurityDescriptorControl Lib "ADVAPI32.DLL" (ByRef pSecurityDescriptor As SECURITY_DESCRIPTOR, ByRef pControl As Short, ByRef lpdwRevision As Integer) As Integer
    'UPGRADE_WARNING: ?? ACL ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetSecurityDescriptorDacl Lib "ADVAPI32.DLL" (ByRef pSecurityDescriptor As SECURITY_DESCRIPTOR, bDaclPresent As Integer, ByRef pDacl As ACL, bDaclDefaulted As Integer) As Integer
    'UPGRADE_WARNING: ?? ACL ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetSecurityDescriptorDacl Lib "ADVAPI32.DLL" (ByRef pSecurityDescriptor As SECURITY_DESCRIPTOR, ByRef lpbDaclPresent As Integer, ByRef pDacl As ACL, ByRef lpbDaclDefaulted As Integer) As Integer
    'UPGRADE_WARNING: ?? ACL ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetSecurityDescriptorSacl Lib "ADVAPI32.DLL" (ByRef pSecurityDescriptor As SECURITY_DESCRIPTOR, bSaclPresent As Integer, ByRef pSacl As ACL, bSaclDefaulted As Integer) As Integer
    'UPGRADE_WARNING: ?? ACL ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetSecurityDescriptorSacl Lib "ADVAPI32.DLL" (ByRef pSecurityDescriptor As SECURITY_DESCRIPTOR, lpbSaclPresent As Integer, ByRef pSacl As ACL, lpbSaclDefaulted As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetSecurityDescriptorOwner Lib "ADVAPI32.DLL" (ByRef pSecurityDescriptor As SECURITY_DESCRIPTOR, ByRef pOwner As Object, bOwnerDefaulted As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetSecurityDescriptorOwner Lib "ADVAPI32.DLL" (ByRef pSecurityDescriptor As SECURITY_DESCRIPTOR, ByRef pOwner As Object, lpbOwnerDefaulted As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetSecurityDescriptorGroup Lib "ADVAPI32.DLL" (ByRef pSecurityDescriptor As SECURITY_DESCRIPTOR, ByRef pGroup As Object, bGroupDefaulted As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetSecurityDescriptorGroup Lib "ADVAPI32.DLL" (ByRef pSecurityDescriptor As SECURITY_DESCRIPTOR, ByRef pGroup As Object, lpbGroupDefaulted As Integer) As Integer
    'UPGRADE_WARNING: ?? GENERIC_MAPPING ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreatePrivateObjectSecurity Lib "ADVAPI32.DLL" (ByRef ParentDescriptor As SECURITY_DESCRIPTOR, ByRef CreatorDescriptor As SECURITY_DESCRIPTOR, ByRef NewDescriptor As SECURITY_DESCRIPTOR, IsDirectoryObject As Integer, Token As Integer, ByRef GenericMapping As GENERIC_MAPPING) As Integer
    'UPGRADE_WARNING: ?? GENERIC_MAPPING ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetPrivateObjectSecurity Lib "ADVAPI32.DLL" (SecurityInformation As Integer, ByRef ModificationDescriptor As SECURITY_DESCRIPTOR, ByRef ObjectsSecurityDescriptor As SECURITY_DESCRIPTOR, ByRef GenericMapping As GENERIC_MAPPING, Token As Integer) As Integer
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetPrivateObjectSecurity Lib "ADVAPI32.DLL" (ByRef ObjectDescriptor As SECURITY_DESCRIPTOR, SecurityInformation As Integer, ByRef ResultantDescriptor As SECURITY_DESCRIPTOR, DescriptorLength As Integer, ByRef ReturnLength As Integer) As Integer
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function DestroyPrivateObjectSecurity Lib "ADVAPI32.DLL" (ByRef ObjectDescriptor As SECURITY_DESCRIPTOR) As Integer
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function MakeSelfRelativeSD Lib "ADVAPI32.DLL" (ByRef pAbsoluteSecurityDescriptor As SECURITY_DESCRIPTOR, ByRef pSelfRelativeSecurityDescriptor As SECURITY_DESCRIPTOR, ByRef lpdwBufferLength As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_WARNING: ?? ACL ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? ACL ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function MakeAbsoluteSD Lib "ADVAPI32.DLL" (ByRef pSelfRelativeSecurityDescriptor As SECURITY_DESCRIPTOR, ByRef pAbsoluteSecurityDescriptor As SECURITY_DESCRIPTOR, ByRef lpdwAbsoluteSecurityDescriptorSize As Integer, ByRef pDacl As ACL, ByRef lpdwDaclSize As Integer, ByRef pSacl As ACL, ByRef lpdwSaclSize As Integer, ByRef pOwner As Object, ByRef lpdwOwnerSize As Integer, ByRef pPrimaryGroup As Object, ByRef lpdwPrimaryGroupSize As Integer) As Integer
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetFileSecurity Lib "ADVAPI32.DLL" Alias "SetFileSecurityA" (lpFileName As String, SecurityInformation As Integer, ByRef pSecurityDescriptor As SECURITY_DESCRIPTOR) As Integer
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetFileSecurity Lib "ADVAPI32.DLL" Alias "GetFileSecurityA" (lpFileName As String, RequestedInformation As Integer, ByRef pSecurityDescriptor As SECURITY_DESCRIPTOR, nLength As Integer, ByRef lpnLengthNeeded As Integer) As Integer
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetKernelObjectSecurity Lib "ADVAPI32.DLL" (handle As Integer, SecurityInformation As Integer, ByRef SecurityDescriptor As SECURITY_DESCRIPTOR) As Integer
    Public Declare Function FindFirstChangeNotification Lib "kernel32" Alias "FindFirstChangeNotificationA" (lpPathName As String, bWatchSubtree As Integer, dwNotifyFilter As Integer) As Integer
    Public Declare Function FindNextChangeNotification Lib "kernel32" (hChangeHandle As Integer) As Integer
    Public Declare Function FindCloseChangeNotification Lib "kernel32" (hChangeHandle As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function VirtualLock Lib "kernel32" (ByRef lpAddress As Object, dwSize As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function VirtualUnlock Lib "kernel32" (ByRef lpAddress As Object, dwSize As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function MapViewOfFileEx Lib "kernel32" (hFileMappingObject As Integer, dwDesiredAccess As Integer, dwFileOffsetHigh As Integer, dwFileOffsetLow As Integer, dwNumberOfBytesToMap As Integer, ByRef lpBaseAddress As Object) As Integer
    Public Declare Function SetPriorityClass Lib "kernel32" (hProcess As Integer, dwPriorityClass As Integer) As Integer
    Public Declare Function GetPriorityClass Lib "kernel32" (hProcess As Integer) As Integer

    Public Structure CONTEXT
        Dim FltF0 As Double
        Dim FltF1 As Double
        Dim FltF2 As Double
        Dim FltF3 As Double
        Dim FltF4 As Double
        Dim FltF5 As Double
        Dim FltF6 As Double
        Dim FltF7 As Double
        Dim FltF8 As Double
        Dim FltF9 As Double
        Dim FltF10 As Double
        Dim FltF11 As Double
        Dim FltF12 As Double
        Dim FltF13 As Double
        Dim FltF14 As Double
        Dim FltF15 As Double
        Dim FltF16 As Double
        Dim FltF17 As Double
        Dim FltF18 As Double
        Dim FltF19 As Double
        Dim FltF20 As Double
        Dim FltF21 As Double
        Dim FltF22 As Double
        Dim FltF23 As Double
        Dim FltF24 As Double
        Dim FltF25 As Double
        Dim FltF26 As Double
        Dim FltF27 As Double
        Dim FltF28 As Double
        Dim FltF29 As Double
        Dim FltF30 As Double
        Dim FltF31 As Double
        Dim IntV0 As Double
        Dim IntT0 As Double
        Dim IntT1 As Double
        Dim IntT2 As Double
        Dim IntT3 As Double
        Dim IntT4 As Double
        Dim IntT5 As Double
        Dim IntT6 As Double
        Dim IntT7 As Double
        Dim IntS0 As Double
        Dim IntS1 As Double
        Dim IntS2 As Double
        Dim IntS3 As Double
        Dim IntS4 As Double
        Dim IntS5 As Double
        Dim IntFp As Double
        Dim IntA0 As Double
        Dim IntA1 As Double
        Dim IntA2 As Double
        Dim IntA3 As Double
        Dim IntA4 As Double
        Dim IntA5 As Double
        Dim IntT8 As Double
        Dim IntT9 As Double
        Dim IntT10 As Double
        Dim IntT11 As Double
        Dim IntRa As Double
        Dim IntT12 As Double
        Dim IntAt As Double
        Dim IntGp As Double
        Dim IntSp As Double
        Dim IntZero As Double
        Dim Fpcr As Double
        Dim SoftFpcr As Double
        Dim Fir As Double
        Dim Psr As Integer
        Dim ContextFlags As Integer
        <VBFixedArray(4)> Dim Fill() As Integer

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim Fill(4)
        End Sub
    End Structure

    Public Structure EXCEPTION_POINTERS
        'UPGRADE_WARNING: ?? pExceptionRecord ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"”
        Dim pExceptionRecord As EXCEPTION_RECORD
        'UPGRADE_WARNING: ?? ContextRecord ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"”
        Dim ContextRecord As CONTEXT

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            pExceptionRecord.Initialize()
            ContextRecord.Initialize()
        End Sub
    End Structure

    Public Structure LDT_BYTES ' Defined for use in LDT_ENTRY Type
        Dim BaseMid As Byte
        Dim Flags1 As Byte
        Dim Flags2 As Byte
        Dim BaseHi As Byte
    End Structure

    Public Structure LDT_ENTRY
        Dim LimitLow As Short
        Dim BaseLow As Short
        Dim HighWord As Integer ' Can use LDT_BYTES Type
    End Structure

    Public Declare Sub FatalExit Lib "kernel32" (code As Integer)
    Public Declare Function GetEnvironmentStrings Lib "kernel32" Alias "GetEnvironmentStringsA" () As String
    Public Declare Sub RaiseException Lib "kernel32" (dwExceptionCode As Integer, dwExceptionFlags As Integer, nNumberOfArguments As Integer, ByRef lpArguments As Integer)
    'UPGRADE_WARNING: ?? EXCEPTION_POINTERS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function UnhandledExceptionFilter Lib "kernel32" (ByRef ExceptionInfo As EXCEPTION_POINTERS) As Integer

    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_WARNING: ?? SECURITY_ATTRIBUTES ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreateThread Lib "kernel32" (ByRef lpThreadAttributes As SECURITY_ATTRIBUTES, dwStackSize As Integer, ByRef lpStartAddress As Integer, ByRef lpParameter As Object, dwCreationFlags As Integer, ByRef lpThreadId As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_WARNING: ?? SECURITY_ATTRIBUTES ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreateRemoteThread Lib "kernel32" (hProcess As Integer, ByRef lpThreadAttributes As SECURITY_ATTRIBUTES, dwStackSize As Integer, ByRef lpStartAddress As Integer, ByRef lpParameter As Object, dwCreationFlags As Integer, ByRef lpThreadId As Integer) As Integer
    Public Declare Function GetCurrentThread Lib "kernel32" () As Integer
    Public Declare Function GetCurrentThreadId Lib "kernel32" () As Integer
    Public Declare Function SetThreadPriority Lib "kernel32" (hThread As Integer, nPriority As Integer) As Integer
    Public Declare Function GetThreadPriority Lib "kernel32" (hThread As Integer) As Integer
    'UPGRADE_WARNING: ?? FILETIME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? FILETIME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? FILETIME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? FILETIME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetThreadTimes Lib "kernel32" (hThread As Integer, ByRef lpCreationTime As FILETIME, ByRef lpExitTime As FILETIME, ByRef lpKernelTime As FILETIME, ByRef lpUserTime As FILETIME) As Integer
    Public Declare Sub ExitThread Lib "kernel32" (dwExitCode As Integer)
    Public Declare Function TerminateThread Lib "kernel32" (hThread As Integer, dwExitCode As Integer) As Integer
    Public Declare Function GetExitCodeThread Lib "kernel32" (hThread As Integer, ByRef lpExitCode As Integer) As Integer
    'UPGRADE_WARNING: ?? LDT_ENTRY ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetThreadSelectorEntry Lib "kernel32" (hThread As Integer, dwSelector As Integer, ByRef lpSelectorEntry As LDT_ENTRY) As Integer

    ' COMM declarations
    'UPGRADE_WARNING: ?? DCB ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetCommState Lib "kernel32" (hCommDev As Integer, ByRef lpDCB As DCB) As Integer
    'UPGRADE_WARNING: ?? COMMTIMEOUTS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetCommTimeouts Lib "kernel32" (hFile As Integer, ByRef lpCommTimeouts As COMMTIMEOUTS) As Integer
    'UPGRADE_WARNING: ?? DCB ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetCommState Lib "kernel32" (nCid As Integer, ByRef lpDCB As DCB) As Integer
    'UPGRADE_WARNING: ?? COMMTIMEOUTS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetCommTimeouts Lib "kernel32" (hFile As Integer, ByRef lpCommTimeouts As COMMTIMEOUTS) As Integer
    Public Declare Function PurgeComm Lib "kernel32" (hFile As Integer, dwFlags As Integer) As Integer
    'UPGRADE_WARNING: ?? DCB ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function BuildCommDCB Lib "kernel32" Alias "BuildCommDCBA" (lpDef As String, ByRef lpDCB As DCB) As Integer
    'UPGRADE_WARNING: ?? COMMTIMEOUTS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? DCB ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function BuildCommDCBAndTimeouts Lib "kernel32" Alias "BuildCommDCBAndTimeoutsA" (lpDef As String, ByRef lpDCB As DCB, ByRef lpCommTimeouts As COMMTIMEOUTS) As Integer
    'UPGRADE_NOTE: cChar ???? cChar_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
    Public Declare Function TransmitCommChar Lib "kernel32" (nCid As Integer, cChar_Renamed As Byte) As Integer
    Public Declare Function SetCommBreak Lib "kernel32" (nCid As Integer) As Integer
    Public Declare Function SetCommMask Lib "kernel32" (hFile As Integer, dwEvtMask As Integer) As Integer
    Public Declare Function ClearCommBreak Lib "kernel32" (nCid As Integer) As Integer
    'UPGRADE_WARNING: ?? COMSTAT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ClearCommError Lib "kernel32" (hFile As Integer, ByRef lpErrors As Integer, ByRef lpStat As COMSTAT) As Integer
    Public Declare Function SetupComm Lib "kernel32" (hFile As Integer, dwInQueue As Integer, dwOutQueue As Integer) As Integer
    Public Declare Function EscapeCommFunction Lib "kernel32" (nCid As Integer, nFunc As Integer) As Integer
    Public Declare Function GetCommMask Lib "kernel32" (hFile As Integer, ByRef lpEvtMask As Integer) As Integer
    'UPGRADE_WARNING: ?? COMMPROP ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetCommProperties Lib "kernel32" (hFile As Integer, ByRef lpCommProp As COMMPROP) As Integer
    Public Declare Function GetCommModemStatus Lib "kernel32" (hFile As Integer, ByRef lpModemStat As Integer) As Integer
    'UPGRADE_WARNING: ?? OVERLAPPED ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function WaitCommEvent Lib "kernel32" (hFile As Integer, ByRef lpEvtMask As Integer, ByRef lpOverlapped As OVERLAPPED) As Integer

    Public Declare Function SetTapePosition Lib "kernel32" (hDevice As Integer, dwPositionMethod As Integer, dwPartition As Integer, dwOffsetLow As Integer, dwOffsetHigh As Integer, bimmediate As Integer) As Integer
    Public Declare Function GetTapePosition Lib "kernel32" (hDevice As Integer, dwPositionType As Integer, ByRef lpdwPartition As Integer, ByRef lpdwOffsetLow As Integer, ByRef lpdwOffsetHigh As Integer) As Integer
    Public Declare Function PrepareTape Lib "kernel32" (hDevice As Integer, dwOperation As Integer, bimmediate As Integer) As Integer
    Public Declare Function EraseTape Lib "kernel32" (hDevice As Integer, dwEraseType As Integer, bimmediate As Integer) As Integer
    Public Declare Function CreateTapePartition Lib "kernel32" (hDevice As Integer, dwPartitionMethod As Integer, dwCount As Integer, dwSize As Integer) As Integer
    Public Declare Function WriteTapemark Lib "kernel32" (hDevice As Integer, dwTapemarkType As Integer, dwTapemarkCount As Integer, bimmediate As Integer) As Integer
    Public Declare Function GetTapeStatus Lib "kernel32" (hDevice As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function GetTapeParameters Lib "kernel32" (hDevice As Integer, dwOperation As Integer, ByRef lpdwSize As Integer, ByRef lpTapeInformation As Object) As Integer

    Public Const GET_TAPE_MEDIA_INFORMATION As Short = 0
    Public Const GET_TAPE_DRIVE_INFORMATION As Short = 1

    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function SetTapeParameters Lib "kernel32" (hDevice As Integer, dwOperation As Integer, ByRef lpTapeInformation As Object) As Integer

    Public Const SET_TAPE_MEDIA_INFORMATION As Short = 0
    Public Const SET_TAPE_DRIVE_INFORMATION As Short = 1

    'UPGRADE_NOTE: Beep ???? Beep_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
    Public Declare Function Beep_Renamed Lib "kernel32" (dwFreq As Integer, dwDuration As Integer) As Integer

    Public Declare Function MulDiv Lib "kernel32" (nNumber As Integer, nNumerator As Integer, nDenominator As Integer) As Integer

    'UPGRADE_WARNING: ?? SystemTime ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Sub GetSystemTime Lib "kernel32" (ByRef lpSystemTime As SystemTime)
    'UPGRADE_WARNING: ?? SystemTime ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetSystemTime Lib "kernel32" (ByRef lpSystemTime As SystemTime) As Integer
    'UPGRADE_WARNING: ?? SystemTime ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Sub GetLocalTime Lib "kernel32" (ByRef lpSystemTime As SystemTime)
    'UPGRADE_WARNING: ?? SystemTime ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetLocalTime Lib "kernel32" (ByRef lpSystemTime As SystemTime) As Integer
    'UPGRADE_WARNING: ?? SYSTEM_INFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Sub GetSystemInfo Lib "kernel32" (ByRef lpSystemInfo As SYSTEM_INFO)

    Public Structure TIME_ZONE_INFORMATION
        Dim Bias As Integer
        <VBFixedArray(32)> Dim StandardName() As Short
        Dim StandardDate As SystemTime
        Dim StandardBias As Integer
        <VBFixedArray(32)> Dim DaylightName() As Short
        Dim DaylightDate As SystemTime
        Dim DaylightBias As Integer

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim StandardName(32)
            ReDim DaylightName(32)
        End Sub
    End Structure

    'UPGRADE_WARNING: ?? TIME_ZONE_INFORMATION ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetTimeZoneInformation Lib "kernel32" (ByRef lpTimeZoneInformation As TIME_ZONE_INFORMATION) As Integer
    'UPGRADE_WARNING: ?? TIME_ZONE_INFORMATION ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetTimeZoneInformation Lib "kernel32" (ByRef lpTimeZoneInformation As TIME_ZONE_INFORMATION) As Integer

    ' Routines to convert back and forth
    ' between system time and file time

    'UPGRADE_WARNING: ?? FILETIME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? SystemTime ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SystemTimeToFileTime Lib "kernel32" (ByRef lpSystemTime As SystemTime, ByRef lpFileTime As FILETIME) As Integer
    'UPGRADE_WARNING: ?? FILETIME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? FILETIME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function FileTimeToLocalFileTime Lib "kernel32" (ByRef lpFileTime As FILETIME, ByRef lpLocalFileTime As FILETIME) As Integer
    'UPGRADE_WARNING: ?? FILETIME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? FILETIME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function LocalFileTimeToFileTime Lib "kernel32" (ByRef lpLocalFileTime As FILETIME, ByRef lpFileTime As FILETIME) As Integer
    'UPGRADE_WARNING: ?? SystemTime ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? FILETIME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function FileTimeToSystemTime Lib "kernel32" (ByRef lpFileTime As FILETIME, ByRef lpSystemTime As SystemTime) As Integer
    'UPGRADE_WARNING: ?? FILETIME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? FILETIME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CompareFileTime Lib "kernel32" (ByRef lpFileTime1 As FILETIME, ByRef lpFileTime2 As FILETIME) As Integer
    'UPGRADE_WARNING: ?? FILETIME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function FileTimeToDosDateTime Lib "kernel32" (ByRef lpFileTime As FILETIME, lpFatDate As Integer, lpFatTime As Integer) As Integer
    'UPGRADE_WARNING: ?? FILETIME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function DosDateTimeToFileTime Lib "kernel32" (wFatDate As Integer, wFatTime As Integer, ByRef lpFileTime As FILETIME) As Integer
    Public Declare Function GetTickCount Lib "kernel32" () As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function FormatMessage Lib "kernel32" Alias "FormatMessageA" (dwFlags As Integer, ByRef lpSource As Object, dwMessageId As Integer, dwLanguageId As Integer, lpBuffer As String, nSize As Integer, ByRef Arguments As Integer) As Integer

    Public Const FORMAT_MESSAGE_ALLOCATE_BUFFER As Short = &H100S
    Public Const FORMAT_MESSAGE_IGNORE_INSERTS As Short = &H200S
    Public Const FORMAT_MESSAGE_FROM_STRING As Short = &H400S
    Public Const FORMAT_MESSAGE_FROM_HMODULE As Short = &H800S
    Public Const FORMAT_MESSAGE_FROM_SYSTEM As Short = &H1000S
    Public Const FORMAT_MESSAGE_ARGUMENT_ARRAY As Short = &H2000S
    Public Const FORMAT_MESSAGE_MAX_WIDTH_MASK As Short = &HFFS

    'UPGRADE_WARNING: ?? SECURITY_ATTRIBUTES ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreatePipe Lib "kernel32" (ByRef phReadPipe As Integer, ByRef phWritePipe As Integer, ByRef lpPipeAttributes As SECURITY_ATTRIBUTES, nSize As Integer) As Integer
    'UPGRADE_WARNING: ?? OVERLAPPED ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ConnectNamedPipe Lib "kernel32" (hNamedPipe As Integer, ByRef lpOverlapped As OVERLAPPED) As Integer
    Public Declare Function DisconnectNamedPipe Lib "kernel32" (hNamedPipe As Integer) As Integer
    Public Declare Function SetNamedPipeHandleState Lib "kernel32" (hNamedPipe As Integer, ByRef lpMode As Integer, ByRef lpMaxCollectionCount As Integer, ByRef lpCollectDataTimeout As Integer) As Integer
    Public Declare Function GetNamedPipeInfo Lib "kernel32" (hNamedPipe As Integer, ByRef lpFlags As Integer, ByRef lpOutBufferSize As Integer, ByRef lpInBufferSize As Integer, ByRef lpMaxInstances As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function PeekNamedPipe Lib "kernel32" (hNamedPipe As Integer, ByRef lpBuffer As Object, nBufferSize As Integer, ByRef lpBytesRead As Integer, ByRef lpTotalBytesAvail As Integer, ByRef lpBytesLeftThisMessage As Integer) As Integer
    'UPGRADE_WARNING: ?? OVERLAPPED ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function TransactNamedPipe Lib "kernel32" (hNamedPipe As Integer, ByRef lpInBuffer As Object, nInBufferSize As Integer, ByRef lpOutBuffer As Object, nOutBufferSize As Integer, ByRef lpBytesRead As Integer, ByRef lpOverlapped As OVERLAPPED) As Integer

    'UPGRADE_WARNING: ?? SECURITY_ATTRIBUTES ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreateMailslot Lib "kernel32" Alias "CreateMailslotA" (lpName As String, nMaxMessageSize As Integer, lReadTimeout As Integer, ByRef lpSecurityAttributes As SECURITY_ATTRIBUTES) As Integer
    Public Declare Function GetMailslotInfo Lib "kernel32" (hMailslot As Integer, ByRef lpMaxMessageSize As Integer, ByRef lpNextSize As Integer, ByRef lpMessageCount As Integer, ByRef lpReadTimeout As Integer) As Integer
    Public Declare Function SetMailslotInfo Lib "kernel32" (hMailslot As Integer, lReadTimeout As Integer) As Integer
    'Public Declare Function MapViewOfFile Lib "kernel32" (hFileMappingObject As Integer, dwDesiredAccess As Integer, dwFileOffsetHigh As Integer, dwFileOffsetLow As Integer, dwNumberOfBytesToMap As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function FlushViewOfFile Lib "kernel32" (ByRef lpBaseAddress As Object, dwNumberOfBytesToFlush As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function UnmapViewOfFile Lib "kernel32" (ByRef lpBaseAddress As Object) As Integer

    Public Declare Function lstrcmp Lib "kernel32" Alias "lstrcmpA" (lpString1 As String, lpString2 As String) As Integer
    Public Declare Function lstrcmpi Lib "kernel32" Alias "lstrcmpiA" (lpString1 As String, lpString2 As String) As Integer
    Public Declare Function lstrlen Lib "kernel32" Alias "lstrlenA" (lpString As String) As Integer

    Public Declare Function lopen Lib "kernel32" Alias "_lopen" (lpPathName As String, iReadWrite As Integer) As Integer
    Public Declare Function lclose Lib "kernel32" Alias "_lclose" (hFile As Integer) As Integer
    Public Declare Function lcreat Lib "kernel32" Alias "_lcreat" (lpPathName As String, iAttribute As Integer) As Integer
    Public Declare Function llseek Lib "kernel32" Alias "_llseek" (hFile As Integer, lOffset As Integer, iOrigin As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function lread Lib "kernel32" Alias "_lread" (hFile As Integer, ByRef lpBuffer As Object, wBytes As Integer) As Integer
    Public Declare Function lwrite Lib "kernel32" Alias "_lwrite" (hFile As Integer, lpBuffer As String, wBytes As Integer) As Integer

    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function hread Lib "kernel32" Alias "_hread" (hFile As Integer, ByRef lpBuffer As Object, lBytes As Integer) As Integer
    Public Declare Function hwrite Lib "kernel32" Alias "_hwrite" (hFile As Integer, lpBuffer As String, lBytes As Integer) As Integer

    Public Declare Function TlsAlloc Lib "kernel32" () As Integer

    Public Const TLS_OUT_OF_INDEXES As Short = &HFFFFS

    Public Declare Function TlsGetValue Lib "kernel32" (dwTlsIndex As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function TlsSetValue Lib "kernel32" (dwTlsIndex As Integer, ByRef lpTlsValue As Object) As Integer
    Public Declare Function TlsFree Lib "kernel32" (dwTlsIndex As Integer) As Integer
    Public Declare Function SleepEx Lib "kernel32" (dwMilliseconds As Integer, bAlertable As Integer) As Integer
    Public Declare Function WaitForSingleObjectEx Lib "kernel32" (hHandle As Integer, dwMilliseconds As Integer, bAlertable As Integer) As Integer
    Public Declare Function WaitForMultipleObjectsEx Lib "kernel32" (nCount As Integer, ByRef lpHandles As Integer, bWaitAll As Integer, dwMilliseconds As Integer, bAlertable As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function BackupRead Lib "kernel32" (hFile As Integer, ByRef lpBuffer As Byte, nNumberOfBytesToRead As Integer, ByRef lpNumberOfBytesRead As Integer, bAbort As Integer, bProcessSecurity As Integer, ByRef lpContext As Object) As Integer
    Public Declare Function BackupSeek Lib "kernel32" (hFile As Integer, dwLowBytesToSeek As Integer, dwHighBytesToSeek As Integer, ByRef lpdwLowByteSeeked As Integer, ByRef lpdwHighByteSeeked As Integer, ByRef lpContext As Integer) As Integer
    Public Declare Function BackupWrite Lib "kernel32" (hFile As Integer, ByRef lpBuffer As Byte, nNumberOfBytesToWrite As Integer, ByRef lpNumberOfBytesWritten As Integer, bAbort As Integer, bProcessSecurity As Integer, ByRef lpContext As Integer) As Integer

    ' Stream ID type
    Public Structure WIN32_STREAM_ID
        Dim dwStreamID As Integer
        Dim dwStreamAttributes As Integer
        Dim dwStreamSizeLow As Integer
        Dim dwStreamSizeHigh As Integer
        Dim dwStreamNameSize As Integer
        Dim cStreamName As Byte
    End Structure

    ' Stream IDs
    Public Const BACKUP_DATA As Short = &H1S
    Public Const BACKUP_EA_DATA As Short = &H2S
    Public Const BACKUP_SECURITY_DATA As Short = &H3S
    Public Const BACKUP_ALTERNATE_DATA As Short = &H4S
    Public Const BACKUP_LINK As Short = &H5S

    '   Stream Attributes
    Public Const STREAM_MODIFIED_WHEN_READ As Short = &H1S
    Public Const STREAM_CONTAINS_SECURITY As Short = &H2S

    '  Dual Mode API below this line. Dual Mode Types also included.

    Public Const STARTF_USESHOWWINDOW As Short = &H1S
    Public Const STARTF_USESIZE As Short = &H2S
    Public Const STARTF_USEPOSITION As Short = &H4S
    Public Const STARTF_USECOUNTCHARS As Short = &H8S
    Public Const STARTF_USEFILLATTRIBUTE As Short = &H10S
    Public Const STARTF_RUNFULLSCREEN As Short = &H20S '  ignored for non-x86 platforms
    Public Const STARTF_FORCEONFEEDBACK As Short = &H40S
    Public Const STARTF_FORCEOFFFEEDBACK As Short = &H80S
    Public Const STARTF_USESTDHANDLES As Short = &H100S

    Public Structure STARTUPINFO
        Dim cb As Integer
        Dim lpReserved As String
        Dim lpDesktop As String
        Dim lpTitle As String
        Dim dwX As Integer
        Dim dwY As Integer
        Dim dwXSize As Integer
        Dim dwYSize As Integer
        Dim dwXCountChars As Integer
        Dim dwYCountChars As Integer
        Dim dwFillAttribute As Integer
        Dim dwFlags As Integer
        Dim wShowWindow As Short
        Dim cbReserved2 As Short
        Dim lpReserved2 As Byte
        Dim hStdInput As Integer
        Dim hStdOutput As Integer
        Dim hStdError As Integer
    End Structure

    Public Const SHUTDOWN_NORETRY As Short = &H1S

    Public Structure WIN32_FIND_DATA
        Dim dwFileAttributes As Integer
        Dim ftCreationTime As FILETIME
        Dim ftLastAccessTime As FILETIME
        Dim ftLastWriteTime As FILETIME
        Dim nFileSizeHigh As Integer
        Dim nFileSizeLow As Integer
        Dim dwReserved0 As Integer
        Dim dwReserved1 As Integer
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(MAX_PATH), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=MAX_PATH)> Public cFileName() As Char
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(14), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=14)> Public cAlternate() As Char
    End Structure

    'UPGRADE_WARNING: ?? SECURITY_ATTRIBUTES ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreateMutex Lib "kernel32" Alias "CreateMutexA" (ByRef lpMutexAttributes As SECURITY_ATTRIBUTES, bInitialOwner As Integer, lpName As String) As Integer
    Public Declare Function OpenMutex Lib "kernel32" Alias "OpenMutexA" (dwDesiredAccess As Integer, bInheritHandle As Integer, lpName As String) As Integer
    'UPGRADE_WARNING: ?? SECURITY_ATTRIBUTES ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreateEvent Lib "kernel32" Alias "CreateEventA" (ByRef lpEventAttributes As SECURITY_ATTRIBUTES, bManualReset As Integer, bInitialState As Integer, lpName As String) As Integer
    Public Declare Function OpenEvent Lib "kernel32" Alias "OpenEventA" (dwDesiredAccess As Integer, bInheritHandle As Integer, lpName As String) As Integer
    'UPGRADE_WARNING: ?? SECURITY_ATTRIBUTES ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreateSemaphore Lib "kernel32" Alias "CreateSemaphoreA" (ByRef lpSemaphoreAttributes As SECURITY_ATTRIBUTES, lInitialCount As Integer, lMaximumCount As Integer, lpName As String) As Integer
    Public Declare Function OpenSemaphore Lib "kernel32" Alias "OpenSemaphoreA" (dwDesiredAccess As Integer, bInheritHandle As Integer, lpName As String) As Integer
    'UPGRADE_WARNING: ?? SECURITY_ATTRIBUTES ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreateFileMapping Lib "kernel32" Alias "CreateFileMappingA" (hFile As Integer, ByRef lpFileMappigAttributes As SECURITY_ATTRIBUTES, flProtect As Integer, dwMaximumSizeHigh As Integer, dwMaximumSizeLow As Integer, lpName As String) As Integer
    Public Declare Function OpenFileMapping Lib "kernel32" Alias "OpenFileMappingA" (dwDesiredAccess As Integer, bInheritHandle As Integer, lpName As String) As Integer
    Public Declare Function GetLogicalDriveStrings Lib "kernel32" Alias "GetLogicalDriveStringsA" (nBufferLength As Integer, lpBuffer As String) As Integer

    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function IsBadReadPtr Lib "kernel32" (ByRef lp As Object, ucb As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function IsBadWritePtr Lib "kernel32" (ByRef lp As Object, ucb As Integer) As Integer
    Public Declare Function IsBadStringPtr Lib "kernel32" Alias "IsBadStringPtrA" (lpsz As String, ucchMax As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function IsBadHugeReadPtr Lib "kernel32" (ByRef lp As Object, ucb As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function IsBadHugeWritePtr Lib "kernel32" (ByRef lp As Object, ucb As Integer) As Integer

    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function LookupAccountSid Lib "ADVAPI32.DLL" Alias "LookupAccountSidA" (lpSystemName As String, ByRef Sid As Object, name As String, ByRef cbName As Integer, ReferencedDomainName As String, ByRef cbReferencedDomainName As Integer, ByRef peUse As Short) As Integer

    Public Declare Function LookupAccountName Lib "ADVAPI32.DLL" Alias "LookupAccountNameA" (lpSystemName As String, lpAccountName As String, ByRef Sid As Integer, ByRef cbSid As Integer, ReferencedDomainName As String, ByRef cbReferencedDomainName As Integer, ByRef peUse As Short) As Integer
    'UPGRADE_WARNING: ?? LARGE_INTEGER ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function LookupPrivilegeValue Lib "ADVAPI32.DLL" Alias "LookupPrivilegeValueA" (lpSystemName As String, lpName As String, ByRef lpLuid As LARGE_INTEGER) As Integer
    'UPGRADE_WARNING: ?? LARGE_INTEGER ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function LookupPrivilegeName Lib "ADVAPI32.DLL" Alias "LookupPrivilegeNameA" (lpSystemName As String, ByRef lpLuid As LARGE_INTEGER, lpName As String, ByRef cbName As Integer) As Integer
    Public Declare Function LookupPrivilegeDisplayName Lib "ADVAPI32.DLL" Alias "LookupPrivilegeDisplayNameA" (lpSystemName As String, lpName As String, lpDisplayName As String, ByRef cbDisplayName As Integer, ByRef lpLanguageID As Integer) As Integer
    'UPGRADE_WARNING: ?? LARGE_INTEGER ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function AllocateLocallyUniqueId Lib "ADVAPI32.DLL" (ByRef Luid As LARGE_INTEGER) As Integer

    Public Declare Function GetComputerName Lib "kernel32" Alias "GetComputerNameA" (lpBuffer As String, ByRef nSize As Integer) As Integer
    Public Declare Function SetComputerName Lib "kernel32" Alias "SetComputerNameA" (lpComputerName As String) As Integer
    Public Declare Function GetUserName Lib "ADVAPI32.DLL" Alias "GetUserNameA" (lpBuffer As String, ByRef nSize As Integer) As Integer

    ' Performance counter API's
    'UPGRADE_WARNING: ?? LARGE_INTEGER ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function QueryPerformanceCounter Lib "kernel32" (ByRef lpPerformanceCount As LARGE_INTEGER) As Integer
    'UPGRADE_WARNING: ?? LARGE_INTEGER ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function QueryPerformanceFrequency Lib "kernel32" (ByRef lpFrequency As LARGE_INTEGER) As Integer

    '  Abnormal termination codes
    Public Const TC_NORMAL As Short = 0
    Public Const TC_HARDERR As Short = 1
    Public Const TC_GP_TRAP As Short = 2
    Public Const TC_SIGNAL As Short = 3


    ' Procedure declarations, constant definitions, and macros
    ' for the NLS component

    ' String Length Maximums
    Public Const MAX_LEADBYTES As Short = 12 '  5 ranges, 2 bytes ea., 0 term.

    ' MBCS and Unicode Translation Flags.

    Public Const MB_PRECOMPOSED As Short = &H1S '  use precomposed chars
    Public Const MB_COMPOSITE As Short = &H2S '  use composite chars
    Public Const MB_USEGLYPHCHARS As Short = &H4S '  use glyph chars, not ctrl chars

    Public Const WC_DEFAULTCHECK As Short = &H100S '  check for default char
    Public Const WC_COMPOSITECHECK As Short = &H200S '  convert composite to precomposed
    Public Const WC_DISCARDNS As Short = &H10S '  discard non-spacing chars
    Public Const WC_SEPCHARS As Short = &H20S '  generate separate chars
    Public Const WC_DEFAULTCHAR As Short = &H40S '  replace w/ default char

    ' Character Type Flags.
    Public Const CT_CTYPE1 As Short = &H1S '  ctype 1 information
    Public Const CT_CTYPE2 As Short = &H2S '  ctype 2 information
    Public Const CT_CTYPE3 As Short = &H4S '  ctype 3 information

    ' CType 1 Flag Bits.
    Public Const C1_UPPER As Short = &H1S '  upper case
    Public Const C1_LOWER As Short = &H2S '  lower case
    Public Const C1_DIGIT As Short = &H4S '  decimal digits
    Public Const C1_SPACE As Short = &H8S '  spacing characters
    Public Const C1_PUNCT As Short = &H10S '  punctuation characters
    Public Const C1_CNTRL As Short = &H20S '  control characters
    Public Const C1_BLANK As Short = &H40S '  blank characters
    Public Const C1_XDIGIT As Short = &H80S '  other digits
    Public Const C1_ALPHA As Short = &H100S '  any letter

    ' CType 2 Flag Bits.

    Public Const C2_LEFTTORIGHT As Short = &H1S '  left to right
    Public Const C2_RIGHTTOLEFT As Short = &H2S '  right to left

    Public Const C2_EUROPENUMBER As Short = &H3S '  European number, digit
    Public Const C2_EUROPESEPARATOR As Short = &H4S '  European numeric separator
    Public Const C2_EUROPETERMINATOR As Short = &H5S '  European numeric terminator
    Public Const C2_ARABICNUMBER As Short = &H6S '  Arabic number
    Public Const C2_COMMONSEPARATOR As Short = &H7S '  common numeric separator

    Public Const C2_BLOCKSEPARATOR As Short = &H8S '  block separator
    Public Const C2_SEGMENTSEPARATOR As Short = &H9S '  segment separator
    Public Const C2_WHITESPACE As Short = &HAS '  white space
    Public Const C2_OTHERNEUTRAL As Short = &HBS '  other neutrals

    Public Const C2_NOTAPPLICABLE As Short = &H0S '  no implicit directionality

    ' CType 3 Flag Bits.
    Public Const C3_NONSPACING As Short = &H1S '  nonspacing character
    Public Const C3_DIACRITIC As Short = &H2S '  diacritic mark
    Public Const C3_VOWELMARK As Short = &H4S '  vowel mark
    Public Const C3_SYMBOL As Short = &H8S '  symbols

    Public Const C3_NOTAPPLICABLE As Short = &H0S '  ctype 3 is not applicable

    ' String Flags.
    Public Const NORM_IGNORECASE As Short = &H1S '  ignore case
    Public Const NORM_IGNORENONSPACE As Short = &H2S '  ignore nonspacing chars
    Public Const NORM_IGNORESYMBOLS As Short = &H4S '  ignore symbols

    ' Locale Independent Mapping Flags.
    Public Const MAP_FOLDCZONE As Short = &H10S '  fold compatibility zone chars
    Public Const MAP_PRECOMPOSED As Short = &H20S '  convert to precomposed chars
    Public Const MAP_COMPOSITE As Short = &H40S '  convert to composite chars
    Public Const MAP_FOLDDIGITS As Short = &H80S '  all digits to ASCII 0-9

    ' Locale Dependent Mapping Flags.
    Public Const LCMAP_LOWERCASE As Short = &H100S '  lower case letters
    Public Const LCMAP_UPPERCASE As Short = &H200S '  upper case letters
    Public Const LCMAP_SORTKEY As Short = &H400S '  WC sort key (normalize)
    Public Const LCMAP_BYTEREV As Short = &H800S '  byte reversal

    ' Sorting Flags.
    Public Const SORT_STRINGSORT As Short = &H1000S '  use string sort method

    ' Code Page Default Values.
    Public Const CP_ACP As Short = 0 '  default to ANSI code page
    Public Const CP_OEMCP As Short = 1 '  default to OEM  code page

    ' Country Codes.

    Public Const CTRY_DEFAULT As Short = 0

    Public Const CTRY_AUSTRALIA As Short = 61 '  Australia
    Public Const CTRY_AUSTRIA As Short = 43 '  Austria
    Public Const CTRY_BELGIUM As Short = 32 '  Belgium
    Public Const CTRY_BRAZIL As Short = 55 '  Brazil
    Public Const CTRY_CANADA As Short = 2 '  Canada
    Public Const CTRY_DENMARK As Short = 45 '  Denmark
    Public Const CTRY_FINLAND As Short = 358 '  Finland
    Public Const CTRY_FRANCE As Short = 33 '  France
    Public Const CTRY_GERMANY As Short = 49 '  Germany
    Public Const CTRY_ICELAND As Short = 354 '  Iceland
    Public Const CTRY_IRELAND As Short = 353 '  Ireland
    Public Const CTRY_ITALY As Short = 39 '  Italy
    Public Const CTRY_JAPAN As Short = 81 '  Japan
    Public Const CTRY_MEXICO As Short = 52 '  Mexico
    Public Const CTRY_NETHERLANDS As Short = 31 '  Netherlands
    Public Const CTRY_NEW_ZEALAND As Short = 64 '  New Zealand
    Public Const CTRY_NORWAY As Short = 47 '  Norway
    Public Const CTRY_PORTUGAL As Short = 351 '  Portugal
    Public Const CTRY_PRCHINA As Short = 86 '  PR China
    Public Const CTRY_SOUTH_KOREA As Short = 82 '  South Korea
    Public Const CTRY_SPAIN As Short = 34 '  Spain
    Public Const CTRY_SWEDEN As Short = 46 '  Sweden
    Public Const CTRY_SWITZERLAND As Short = 41 '  Switzerland
    Public Const CTRY_TAIWAN As Short = 886 '  Taiwan
    Public Const CTRY_UNITED_KINGDOM As Short = 44 '  United Kingdom
    Public Const CTRY_UNITED_STATES As Short = 1 '  United States

    ' Locale Types.
    ' These types are used for the GetLocaleInfoW NLS API routine.

    ' LOCALE_NOUSEROVERRIDE is also used in GetTimeFormatW and GetDateFormatW.
    Public Const LOCALE_NOUSEROVERRIDE As Integer = &H80000000 '  do not use user overrides

    Public Const LOCALE_ILANGUAGE As Short = &H1S '  language id
    Public Const LOCALE_SLANGUAGE As Short = &H2S '  localized name of language
    Public Const LOCALE_SENGLANGUAGE As Short = &H1001S '  English name of language
    Public Const LOCALE_SABBREVLANGNAME As Short = &H3S '  abbreviated language name
    Public Const LOCALE_SNATIVELANGNAME As Short = &H4S '  native name of language
    Public Const LOCALE_ICOUNTRY As Short = &H5S '  country code
    Public Const LOCALE_SCOUNTRY As Short = &H6S '  localized name of country
    Public Const LOCALE_SENGCOUNTRY As Short = &H1002S '  English name of country
    Public Const LOCALE_SABBREVCTRYNAME As Short = &H7S '  abbreviated country name
    Public Const LOCALE_SNATIVECTRYNAME As Short = &H8S '  native name of country
    Public Const LOCALE_IDEFAULTLANGUAGE As Short = &H9S '  default language id
    Public Const LOCALE_IDEFAULTCOUNTRY As Short = &HAS '  default country code
    Public Const LOCALE_IDEFAULTCODEPAGE As Short = &HBS '  default code page

    Public Const LOCALE_SLIST As Short = &HCS '  list item separator
    Public Const LOCALE_IMEASURE As Short = &HDS '  0 = metric, 1 = US

    Public Const LOCALE_SDECIMAL As Short = &HES '  decimal separator
    Public Const LOCALE_STHOUSAND As Short = &HFS '  thousand separator
    Public Const LOCALE_SGROUPING As Short = &H10S '  digit grouping
    Public Const LOCALE_IDIGITS As Short = &H11S '  number of fractional digits
    Public Const LOCALE_ILZERO As Short = &H12S '  leading zeros for decimal
    Public Const LOCALE_SNATIVEDIGITS As Short = &H13S '  native ascii 0-9

    Public Const LOCALE_SCURRENCY As Short = &H14S '  local monetary symbol
    Public Const LOCALE_SINTLSYMBOL As Short = &H15S '  intl monetary symbol
    Public Const LOCALE_SMONDECIMALSEP As Short = &H16S '  monetary decimal separator
    Public Const LOCALE_SMONTHOUSANDSEP As Short = &H17S '  monetary thousand separator
    Public Const LOCALE_SMONGROUPING As Short = &H18S '  monetary grouping
    Public Const LOCALE_ICURRDIGITS As Short = &H19S '  # local monetary digits
    Public Const LOCALE_IINTLCURRDIGITS As Short = &H1AS '  # intl monetary digits
    Public Const LOCALE_ICURRENCY As Short = &H1BS '  positive currency mode
    Public Const LOCALE_INEGCURR As Short = &H1CS '  negative currency mode

    Public Const LOCALE_SDATE As Short = &H1DS '  date separator
    Public Const LOCALE_STIME As Short = &H1ES '  time separator
    Public Const LOCALE_SSHORTDATE As Short = &H1FS '  short date format string
    Public Const LOCALE_SLONGDATE As Short = &H20S '  long date format string
    Public Const LOCALE_STIMEFORMAT As Short = &H1003S '  time format string
    Public Const LOCALE_IDATE As Short = &H21S '  short date format ordering
    Public Const LOCALE_ILDATE As Short = &H22S '  long date format ordering
    Public Const LOCALE_ITIME As Short = &H23S '  time format specifier
    Public Const LOCALE_ICENTURY As Short = &H24S '  century format specifier
    Public Const LOCALE_ITLZERO As Short = &H25S '  leading zeros in time field
    Public Const LOCALE_IDAYLZERO As Short = &H26S '  leading zeros in day field
    Public Const LOCALE_IMONLZERO As Short = &H27S '  leading zeros in month field
    Public Const LOCALE_S1159 As Short = &H28S '  AM designator
    Public Const LOCALE_S2359 As Short = &H29S '  PM designator

    Public Const LOCALE_SDAYNAME1 As Short = &H2AS '  long name for Monday
    Public Const LOCALE_SDAYNAME2 As Short = &H2BS '  long name for Tuesday
    Public Const LOCALE_SDAYNAME3 As Short = &H2CS '  long name for Wednesday
    Public Const LOCALE_SDAYNAME4 As Short = &H2DS '  long name for Thursday
    Public Const LOCALE_SDAYNAME5 As Short = &H2ES '  long name for Friday
    Public Const LOCALE_SDAYNAME6 As Short = &H2FS '  long name for Saturday
    Public Const LOCALE_SDAYNAME7 As Short = &H30S '  long name for Sunday
    Public Const LOCALE_SABBREVDAYNAME1 As Short = &H31S '  abbreviated name for Monday
    Public Const LOCALE_SABBREVDAYNAME2 As Short = &H32S '  abbreviated name for Tuesday
    Public Const LOCALE_SABBREVDAYNAME3 As Short = &H33S '  abbreviated name for Wednesday
    Public Const LOCALE_SABBREVDAYNAME4 As Short = &H34S '  abbreviated name for Thursday
    Public Const LOCALE_SABBREVDAYNAME5 As Short = &H35S '  abbreviated name for Friday
    Public Const LOCALE_SABBREVDAYNAME6 As Short = &H36S '  abbreviated name for Saturday
    Public Const LOCALE_SABBREVDAYNAME7 As Short = &H37S '  abbreviated name for Sunday
    Public Const LOCALE_SMONTHNAME1 As Short = &H38S '  long name for January
    Public Const LOCALE_SMONTHNAME2 As Short = &H39S '  long name for February
    Public Const LOCALE_SMONTHNAME3 As Short = &H3AS '  long name for March
    Public Const LOCALE_SMONTHNAME4 As Short = &H3BS '  long name for April
    Public Const LOCALE_SMONTHNAME5 As Short = &H3CS '  long name for May
    Public Const LOCALE_SMONTHNAME6 As Short = &H3DS '  long name for June
    Public Const LOCALE_SMONTHNAME7 As Short = &H3ES '  long name for July
    Public Const LOCALE_SMONTHNAME8 As Short = &H3FS '  long name for August
    Public Const LOCALE_SMONTHNAME9 As Short = &H40S '  long name for September
    Public Const LOCALE_SMONTHNAME10 As Short = &H41S '  long name for October
    Public Const LOCALE_SMONTHNAME11 As Short = &H42S '  long name for November
    Public Const LOCALE_SMONTHNAME12 As Short = &H43S '  long name for December
    Public Const LOCALE_SABBREVMONTHNAME1 As Short = &H44S '  abbreviated name for January
    Public Const LOCALE_SABBREVMONTHNAME2 As Short = &H45S '  abbreviated name for February
    Public Const LOCALE_SABBREVMONTHNAME3 As Short = &H46S '  abbreviated name for March
    Public Const LOCALE_SABBREVMONTHNAME4 As Short = &H47S '  abbreviated name for April
    Public Const LOCALE_SABBREVMONTHNAME5 As Short = &H48S '  abbreviated name for May
    Public Const LOCALE_SABBREVMONTHNAME6 As Short = &H49S '  abbreviated name for June
    Public Const LOCALE_SABBREVMONTHNAME7 As Short = &H4AS '  abbreviated name for July
    Public Const LOCALE_SABBREVMONTHNAME8 As Short = &H4BS '  abbreviated name for August
    Public Const LOCALE_SABBREVMONTHNAME9 As Short = &H4CS '  abbreviated name for September
    Public Const LOCALE_SABBREVMONTHNAME10 As Short = &H4DS '  abbreviated name for October
    Public Const LOCALE_SABBREVMONTHNAME11 As Short = &H4ES '  abbreviated name for November
    Public Const LOCALE_SABBREVMONTHNAME12 As Short = &H4FS '  abbreviated name for December
    Public Const LOCALE_SABBREVMONTHNAME13 As Short = &H100FS

    Public Const LOCALE_SPOSITIVESIGN As Short = &H50S '  positive sign
    Public Const LOCALE_SNEGATIVESIGN As Short = &H51S '  negative sign
    Public Const LOCALE_IPOSSIGNPOSN As Short = &H52S '  positive sign position
    Public Const LOCALE_INEGSIGNPOSN As Short = &H53S '  negative sign position
    Public Const LOCALE_IPOSSYMPRECEDES As Short = &H54S '  mon sym precedes pos amt
    Public Const LOCALE_IPOSSEPBYSPACE As Short = &H55S '  mon sym sep by space from pos amt
    Public Const LOCALE_INEGSYMPRECEDES As Short = &H56S '  mon sym precedes neg amt
    Public Const LOCALE_INEGSEPBYSPACE As Short = &H57S '  mon sym sep by space from neg amt

    ' Time Flags for GetTimeFormatW.
    Public Const TIME_NOMINUTESORSECONDS As Short = &H1S '  do not use minutes or seconds
    Public Const TIME_NOSECONDS As Short = &H2S '  do not use seconds
    Public Const TIME_NOTIMEMARKER As Short = &H4S '  do not use time marker
    Public Const TIME_FORCE24HOURFORMAT As Short = &H8S '  always use 24 hour format

    ' Date Flags for GetDateFormatW.
    Public Const DATE_SHORTDATE As Short = &H1S '  use short date picture
    Public Const DATE_LONGDATE As Short = &H2S '  use long date picture


    ' Code Page Dependent APIs

    Public Declare Function IsValidCodePage Lib "kernel32" (CodePage As Integer) As Integer
    Public Declare Function GetACP Lib "kernel32" () As Integer
    Public Declare Function GetOEMCP Lib "kernel32" () As Integer
    'UPGRADE_WARNING: ?? CPINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetCPInfo Lib "kernel32" (CodePage As Integer, ByRef lpCPInfo As CPINFO) As Integer
    'Public Declare Function IsDBCSLeadByte Lib "kernel32" (bTestChar As Byte) As Integer
    Public Declare Function MultiByteToWideChar Lib "kernel32" (CodePage As Integer, dwFlags As Integer, lpMultiByteStr As String, cchMultiByte As Integer, lpWideCharStr As String, cchWideChar As Integer) As Integer
    Public Declare Function WideCharToMultiByte Lib "kernel32" (CodePage As Integer, dwFlags As Integer, lpWideCharStr As String, cchWideChar As Integer, lpMultiByteStr As String, cchMultiByte As Integer, lpDefaultChar As String, lpUsedDefaultChar As Integer) As Integer

    ' Locale Dependent APIs

    Public Declare Function CompareString Lib "kernel32" Alias "CompareStringA" (Locale As Integer, dwCmpFlags As Integer, lpString1 As String, cchCount1 As Integer, lpString2 As String, cchCount2 As Integer) As Integer
    Public Declare Function LCMapString Lib "kernel32" Alias "LCMapStringA" (Locale As Integer, dwMapFlags As Integer, lpSrcStr As String, cchSrc As Integer, lpDestStr As String, cchDest As Integer) As Integer
    Public Declare Function GetLocaleInfo Lib "kernel32" Alias "GetLocaleInfoA" (Locale As Integer, LCType As Integer, lpLCData As String, cchData As Integer) As Integer
    'UPGRADE_WARNING: ?? SystemTime ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetTimeFormat Lib "kernel32" Alias "GetTimeFormatA" (Locale As Integer, dwFlags As Integer, ByRef lpTime As SystemTime, lpFormat As String, lpTimeStr As String, cchTime As Integer) As Integer
    'UPGRADE_WARNING: ?? SystemTime ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetDateFormat Lib "kernel32" Alias "GetDateFormatA" (Locale As Integer, dwFlags As Integer, ByRef lpDate As SystemTime, lpFormat As String, lpDateStr As String, cchDate As Integer) As Integer
    Public Declare Function SetThreadLocale Lib "kernel32" (Locale As Integer) As Integer
    Public Declare Function GetSystemDefaultLangID Lib "kernel32" () As Short
    Public Declare Function GetUserDefaultLangID Lib "kernel32" () As Short
    Public Declare Function GetSystemDefaultLCID Lib "kernel32" () As Integer
    Public Declare Function GetUserDefaultLCID Lib "kernel32" () As Integer

    ' Locale Independent APIs

    Public Declare Function GetStringTypeA Lib "kernel32" (lcid As Integer, dwInfoType As Integer, lpSrcStr As String, cchSrc As Integer, ByRef lpCharType As Integer) As Integer
    Public Declare Function FoldString Lib "kernel32" Alias "FoldStringA" (dwMapFlags As Integer, lpSrcStr As String, cchSrc As Integer, lpDestStr As String, cchDest As Integer) As Integer

    ' *************************************************************************
    ' *                                                                         *
    ' * winnls.h -- NLS procedure declarations, constant definitions and macros *
    ' *                                                                         *
    ' * Copyright (c) 1991-1995, Microsoft Corp. All rights reserved.           *
    ' *                                                                         *
    ' **************************************************************************/

    ' *  Calendar Types.
    '  *
    '  *  These types are used for the GetALTCalendarInfoW NLS API routine.
    '  */
    Public Const MAX_DEFAULTCHAR As Short = 2
    Public Const CAL_ICALINTVALUE As Short = &H1S '  calendar type
    Public Const CAL_SCALNAME As Short = &H2S '  native name of calendar
    Public Const CAL_IYEAROFFSETRANGE As Short = &H3S '  starting years of eras
    Public Const CAL_SERASTRING As Short = &H4S '  era name for IYearOffsetRanges
    Public Const CAL_SSHORTDATE As Short = &H5S '  Integer date format string
    Public Const CAL_SLONGDATE As Short = &H6S '  long date format string
    Public Const CAL_SDAYNAME1 As Short = &H7S '  native name for Monday
    Public Const CAL_SDAYNAME2 As Short = &H8S '  native name for Tuesday
    Public Const CAL_SDAYNAME3 As Short = &H9S '  native name for Wednesday
    Public Const CAL_SDAYNAME4 As Short = &HAS '  native name for Thursday
    Public Const CAL_SDAYNAME5 As Short = &HBS '  native name for Friday
    Public Const CAL_SDAYNAME6 As Short = &HCS '  native name for Saturday
    Public Const CAL_SDAYNAME7 As Short = &HDS '  native name for Sunday
    Public Const CAL_SABBREVDAYNAME1 As Short = &HES '  abbreviated name for Monday
    Public Const CAL_SABBREVDAYNAME2 As Short = &HFS '  abbreviated name for Tuesday
    Public Const CAL_SABBREVDAYNAME3 As Short = &H10S '  abbreviated name for Wednesday
    Public Const CAL_SABBREVDAYNAME4 As Short = &H11S '  abbreviated name for Thursday
    Public Const CAL_SABBREVDAYNAME5 As Short = &H12S '  abbreviated name for Friday
    Public Const CAL_SABBREVDAYNAME6 As Short = &H13S '  abbreviated name for Saturday
    Public Const CAL_SABBREVDAYNAME7 As Short = &H14S '  abbreviated name for Sunday
    Public Const CAL_SMONTHNAME1 As Short = &H15S '  native name for January
    Public Const CAL_SMONTHNAME2 As Short = &H16S '  native name for February
    Public Const CAL_SMONTHNAME3 As Short = &H17S '  native name for March
    Public Const CAL_SMONTHNAME4 As Short = &H18S '  native name for April
    Public Const CAL_SMONTHNAME5 As Short = &H19S '  native name for May
    Public Const CAL_SMONTHNAME6 As Short = &H1AS '  native name for June
    Public Const CAL_SMONTHNAME7 As Short = &H1BS '  native name for July
    Public Const CAL_SMONTHNAME8 As Short = &H1CS '  native name for August
    Public Const CAL_SMONTHNAME9 As Short = &H1DS '  native name for September
    Public Const CAL_SMONTHNAME10 As Short = &H1ES '  native name for October
    Public Const CAL_SMONTHNAME11 As Short = &H1FS '  native name for November
    Public Const CAL_SMONTHNAME12 As Short = &H20S '  native name for December
    Public Const CAL_SMONTHNAME13 As Short = &H21S '  native name for 13th month (if any)
    Public Const CAL_SABBREVMONTHNAME1 As Short = &H22S '  abbreviated name for January
    Public Const CAL_SABBREVMONTHNAME2 As Short = &H23S '  abbreviated name for February
    Public Const CAL_SABBREVMONTHNAME3 As Short = &H24S '  abbreviated name for March
    Public Const CAL_SABBREVMONTHNAME4 As Short = &H25S '  abbreviated name for April
    Public Const CAL_SABBREVMONTHNAME5 As Short = &H26S '  abbreviated name for May
    Public Const CAL_SABBREVMONTHNAME6 As Short = &H27S '  abbreviated name for June
    Public Const CAL_SABBREVMONTHNAME7 As Short = &H28S '  abbreviated name for July
    Public Const CAL_SABBREVMONTHNAME8 As Short = &H29S '  abbreviated name for August
    Public Const CAL_SABBREVMONTHNAME9 As Short = &H2AS '  abbreviated name for September
    Public Const CAL_SABBREVMONTHNAME10 As Short = &H2BS '  abbreviated name for October
    Public Const CAL_SABBREVMONTHNAME11 As Short = &H2CS '  abbreviated name for November
    Public Const CAL_SABBREVMONTHNAME12 As Short = &H2DS '  abbreviated name for December
    Public Const CAL_SABBREVMONTHNAME13 As Short = &H2ES '  abbreviated name for 13th month (if any)

    '
    '  *  Calendar Enumeration Value.
    '  */
    Public Const ENUM_ALL_CALENDARS As Short = &HFFFFS '  enumerate all calendars
    '
    '  *  Calendar ID Values.
    '  */
    Public Const CAL_GREGORIAN As Short = 1 '  Gregorian (localized) calendar
    Public Const CAL_GREGORIAN_US As Short = 2 '  Gregorian (U.S.) calendar
    Public Const CAL_JAPAN As Short = 3 '  Japanese Emperor Era calendar
    Public Const CAL_TAIWAN As Short = 4 '  Republic of China Era calendar
    Public Const CAL_KOREA As Short = 5 '  Korean Tangun Era calendar

    ' *************************************************************************** Typedefs
    ' *
    ' * Define all types for the NLS component here.
    ' \***************************************************************************/
    '
    '  *  CP Info.
    '  */

    Public Structure CPINFO
        Dim MaxCharSize As Integer '  max length (Byte) of a char
        <VBFixedArray(MAX_DEFAULTCHAR)> Dim DefaultChar() As Byte '  default character
        <VBFixedArray(MAX_LEADBYTES)> Dim LeadByte() As Byte '  lead byte ranges

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim DefaultChar(MAX_DEFAULTCHAR)
            ReDim LeadByte(MAX_LEADBYTES)
        End Sub
    End Structure

    Public Structure NUMBERFMT
        Dim NumDigits As Integer '  number of decimal digits
        Dim LeadingZero As Integer '  if leading zero in decimal fields
        Dim Grouping As Integer '  group size left of decimal
        Dim lpDecimalSep As String '  ptr to decimal separator string
        Dim lpThousandSep As String '  ptr to thousand separator string
        Dim NegativeOrder As Integer '  negative number ordering
    End Structure
    '
    '  *  Currency format.
    '  */

    Public Structure CURRENCYFMT
        Dim NumDigits As Integer '  number of decimal digits
        Dim LeadingZero As Integer '  if leading zero in decimal fields
        Dim Grouping As Integer '  group size left of decimal
        Dim lpDecimalSep As String '  ptr to decimal separator string
        Dim lpThousandSep As String '  ptr to thousand separator string
        Dim NegativeOrder As Integer '  negative currency ordering
        Dim PositiveOrder As Integer '  positive currency ordering
        Dim lpCurrencySymbol As String '  ptr to currency symbol string
    End Structure

    Public Declare Function EnumTimeFormats Lib "kernel32" (lpTimeFmtEnumProc As Integer, Locale As Integer, dwFlags As Integer) As Integer
    Public Declare Function EnumDateFormats Lib "kernel32" (lpDateFmtEnumProc As Integer, Locale As Integer, dwFlags As Integer) As Integer
    Public Declare Function IsValidLocale Lib "kernel32" (Locale As Integer, dwFlags As Integer) As Integer
    Public Declare Function ConvertDefaultLocale Lib "kernel32" (Locale As Integer) As Integer
    Public Declare Function GetThreadLocale Lib "kernel32" () As Integer
    Public Declare Function EnumSystemLocales Lib "kernel32" (lpLocaleEnumProc As Integer, dwFlags As Integer) As Integer
    Public Declare Function EnumSystemCodePages Lib "kernel32" (lpCodePageEnumProc As Integer, dwFlags As Integer) As Integer

    ' The following section contains the public data structures, data types,
    ' and procedures exported by the NT console subsystem.

    Public Structure COORD
        Dim X As Short
        Dim Y As Short
    End Structure

    Public Structure SMALL_RECT
        'UPGRADE_NOTE: Left ???? Left_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
        Dim Left_Renamed As Short
        Dim Top As Short
        'UPGRADE_NOTE: Right ???? Right_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
        Dim Right_Renamed As Short
        Dim Bottom As Short
    End Structure

    Public Structure KEY_EVENT_RECORD
        Dim bKeyDown As Integer
        Dim wRepeatCount As Short
        Dim wVirtualKeyCode As Short
        Dim wVirtualScanCode As Short
        Dim uChar As Short
        Dim dwControlKeyState As Integer
    End Structure

    '  ControlKeyState flags
    Public Const RIGHT_ALT_PRESSED As Short = &H1S '  the right alt key is pressed.
    Public Const LEFT_ALT_PRESSED As Short = &H2S '  the left alt key is pressed.
    Public Const RIGHT_CTRL_PRESSED As Short = &H4S '  the right ctrl key is pressed.
    Public Const LEFT_CTRL_PRESSED As Short = &H8S '  the left ctrl key is pressed.
    Public Const SHIFT_PRESSED As Short = &H10S '  the shift key is pressed.
    Public Const NUMLOCK_ON As Short = &H20S '  the numlock light is on.
    Public Const SCROLLLOCK_ON As Short = &H40S '  the scrolllock light is on.
    Public Const CAPSLOCK_ON As Short = &H80S '  the capslock light is on.
    Public Const ENHANCED_KEY As Short = &H100S '  the key is enhanced.

    Public Structure MOUSE_EVENT_RECORD
        Dim dwMousePosition As COORD
        Dim dwButtonState As Integer
        Dim dwControlKeyState As Integer
        Dim dwEventFlags As Integer
    End Structure

    '  ButtonState flags
    Public Const FROM_LEFT_1ST_BUTTON_PRESSED As Short = &H1S
    Public Const RIGHTMOST_BUTTON_PRESSED As Short = &H2S
    Public Const FROM_LEFT_2ND_BUTTON_PRESSED As Short = &H4S
    Public Const FROM_LEFT_3RD_BUTTON_PRESSED As Short = &H8S
    Public Const FROM_LEFT_4TH_BUTTON_PRESSED As Short = &H10S

    '  EventFlags
    Public Const MOUSE_MOVED As Short = &H1S
    Public Const DOUBLE_CLICK As Short = &H2S

    Public Structure WINDOW_BUFFER_SIZE_RECORD
        Dim dwSize As COORD
    End Structure

    Public Structure MENU_EVENT_RECORD
        Dim dwCommandId As Integer
    End Structure

    Public Structure FOCUS_EVENT_RECORD
        Dim bSetFocus As Integer
    End Structure

    '   EventType flags:
    Public Const KEY_EVENT As Short = &H1S '  Event contains key event record
    Public Const mouse_eventC As Short = &H2S '  Event contains mouse event record
    Public Const WINDOW_BUFFER_SIZE_EVENT As Short = &H4S '  Event contains window change event record
    Public Const MENU_EVENT As Short = &H8S '  Event contains menu event record
    Public Const FOCUS_EVENT As Short = &H10S '  event contains focus change

    Public Structure CHAR_INFO
        'UPGRADE_NOTE: Char ???? Char_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
        Dim Char_Renamed As Short
        Dim Attributes As Short
    End Structure

    '  Attributes flags:
    Public Const FOREGROUND_BLUE As Short = &H1S '  text color contains blue.
    Public Const FOREGROUND_GREEN As Short = &H2S '  text color contains green.
    Public Const FOREGROUND_RED As Short = &H4S '  text color contains red.
    Public Const FOREGROUND_INTENSITY As Short = &H8S '  text color is intensified.
    Public Const BACKGROUND_BLUE As Short = &H10S '  background color contains blue.
    Public Const BACKGROUND_GREEN As Short = &H20S '  background color contains green.
    Public Const BACKGROUND_RED As Short = &H40S '  background color contains red.
    Public Const BACKGROUND_INTENSITY As Short = &H80S '  background color is intensified.

    Public Structure CONSOLE_SCREEN_BUFFER_INFO
        Dim dwSize As COORD
        Dim dwCursorPosition As COORD
        Dim wAttributes As Short
        Dim srWindow As SMALL_RECT
        Dim dwMaximumWindowSize As COORD
    End Structure

    Public Structure CONSOLE_CURSOR_INFO
        Dim dwSize As Integer
        Dim bVisible As Integer
    End Structure

    Public Const CTRL_C_EVENT As Short = 0
    Public Const CTRL_BREAK_EVENT As Short = 1
    Public Const CTRL_CLOSE_EVENT As Short = 2
    '  3 is reserved!
    '  4 is reserved!
    Public Const CTRL_LOGOFF_EVENT As Short = 5
    Public Const CTRL_SHUTDOWN_EVENT As Short = 6

    ' Input Mode flags:
    Public Const ENABLE_PROCESSED_INPUT As Short = &H1S
    Public Const ENABLE_LINE_INPUT As Short = &H2S
    Public Const ENABLE_ECHO_INPUT As Short = &H4S
    Public Const ENABLE_WINDOW_INPUT As Short = &H8S
    Public Const ENABLE_MOUSE_INPUT As Short = &H10S

    ' Output Mode flags:
    Public Const ENABLE_PROCESSED_OUTPUT As Short = &H1S
    Public Const ENABLE_WRAP_AT_EOL_OUTPUT As Short = &H2S

    'UPGRADE_WARNING: ?? SMALL_RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? COORD ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? COORD ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? CHAR_INFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ReadConsoleOutput Lib "kernel32" Alias "ReadConsoleOutputA" (hConsoleOutput As Integer, ByRef lpBuffer As CHAR_INFO, ByRef dwBufferSize As COORD, ByRef dwBufferCoord As COORD, ByRef lpReadRegion As SMALL_RECT) As Integer
    'UPGRADE_WARNING: ?? SMALL_RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? COORD ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? COORD ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? CHAR_INFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function WriteConsoleOutput Lib "kernel32" Alias "WriteConsoleOutputA" (hConsoleOutput As Integer, ByRef lpBuffer As CHAR_INFO, ByRef dwBufferSize As COORD, ByRef dwBufferCoord As COORD, ByRef lpWriteRegion As SMALL_RECT) As Integer
    'UPGRADE_WARNING: ?? COORD ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ReadConsoleOutputCharacter Lib "kernel32" Alias "ReadConsoleOutputCharacterA" (hConsoleOutput As Integer, lpCharacter As String, nLength As Integer, ByRef dwReadCoord As COORD, ByRef lpNumberOfCharsRead As Integer) As Integer
    'UPGRADE_WARNING: ?? COORD ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ReadConsoleOutputAttribute Lib "kernel32" (hConsoleOutput As Integer, ByRef lpAttribute As Integer, nLength As Integer, ByRef dwReadCoord As COORD, ByRef lpNumberOfAttrsRead As Integer) As Integer
    'UPGRADE_WARNING: ?? COORD ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function WriteConsoleOutputCharacter Lib "kernel32" Alias "WriteConsoleOutputCharacterA" (hConsoleOutput As Integer, lpCharacter As String, nLength As Integer, ByRef dwWriteCoord As COORD, ByRef lpNumberOfCharsWritten As Integer) As Integer

    'UPGRADE_WARNING: ?? COORD ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function WriteConsoleOutputAttribute Lib "kernel32" (hConsoleOutput As Integer, ByRef lpAttribute As Short, nLength As Integer, ByRef dwWriteCoord As COORD, ByRef lpNumberOfAttrsWritten As Integer) As Integer
    'UPGRADE_WARNING: ?? COORD ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function FillConsoleOutputCharacter Lib "kernel32" Alias "FillConsoleOutputCharacterA" (hConsoleOutput As Integer, cCharacter As Byte, nLength As Integer, ByRef dwWriteCoord As COORD, ByRef lpNumberOfCharsWritten As Integer) As Integer
    'UPGRADE_WARNING: ?? COORD ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function FillConsoleOutputAttribute Lib "kernel32" (hConsoleOutput As Integer, wAttribute As Integer, nLength As Integer, ByRef dwWriteCoord As COORD, ByRef lpNumberOfAttrsWritten As Integer) As Integer
    Public Declare Function GetConsoleMode Lib "kernel32" (hConsoleHandle As Integer, ByRef lpMode As Integer) As Integer
    Public Declare Function GetNumberOfConsoleInputEvents Lib "kernel32" (hConsoleInput As Integer, ByRef lpNumberOfEvents As Integer) As Integer
    'UPGRADE_WARNING: ?? CONSOLE_SCREEN_BUFFER_INFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetConsoleScreenBufferInfo Lib "kernel32" (hConsoleOutput As Integer, ByRef lpConsoleScreenBufferInfo As CONSOLE_SCREEN_BUFFER_INFO) As Integer
    Public Declare Function GetLargestConsoleWindowSize Lib "kernel32" (hConsoleOutput As Integer) As COORD
    'UPGRADE_WARNING: ?? CONSOLE_CURSOR_INFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetConsoleCursorInfo Lib "kernel32" (hConsoleOutput As Integer, ByRef lpConsoleCursorInfo As CONSOLE_CURSOR_INFO) As Integer
    Public Declare Function GetNumberOfConsoleMouseButtons Lib "kernel32" (ByRef lpNumberOfMouseButtons As Integer) As Integer
    Public Declare Function SetConsoleMode Lib "kernel32" (hConsoleHandle As Integer, dwMode As Integer) As Integer
    Public Declare Function SetConsoleActiveScreenBuffer Lib "kernel32" (hConsoleOutput As Integer) As Integer
    Public Declare Function FlushConsoleInputBuffer Lib "kernel32" (hConsoleInput As Integer) As Integer
    'UPGRADE_WARNING: ?? COORD ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetConsoleScreenBufferSize Lib "kernel32" (hConsoleOutput As Integer, ByRef dwSize As COORD) As Integer
    'UPGRADE_WARNING: ?? COORD ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetConsoleCursorPosition Lib "kernel32" (hConsoleOutput As Integer, ByRef dwCursorPosition As COORD) As Integer
    'UPGRADE_WARNING: ?? CONSOLE_CURSOR_INFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetConsoleCursorInfo Lib "kernel32" (hConsoleOutput As Integer, ByRef lpConsoleCursorInfo As CONSOLE_CURSOR_INFO) As Integer
    'UPGRADE_WARNING: ?? CHAR_INFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? COORD ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? SMALL_RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? SMALL_RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ScrollConsoleScreenBuffer Lib "kernel32" Alias "ScrollConsoleScreenBufferA" (hConsoleOutput As Integer, ByRef lpScrollRectangle As SMALL_RECT, ByRef lpClipRectangle As SMALL_RECT, ByRef dwDestinationOrigin As COORD, ByRef lpFill As CHAR_INFO) As Integer
    'UPGRADE_WARNING: ?? SMALL_RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetConsoleWindowInfo Lib "kernel32" (hConsoleOutput As Integer, bAbsolute As Integer, ByRef lpConsoleWindow As SMALL_RECT) As Integer
    Public Declare Function SetConsoleTextAttribute Lib "kernel32" (hConsoleOutput As Integer, wAttributes As Integer) As Integer
    Public Declare Function SetConsoleCtrlHandler Lib "kernel32" (HandlerRoutine As Integer, Add As Integer) As Integer
    Public Declare Function GenerateConsoleCtrlEvent Lib "kernel32" (dwCtrlEvent As Integer, dwProcessGroupId As Integer) As Integer
    Public Declare Function AllocConsole Lib "kernel32" () As Integer
    Public Declare Function FreeConsole Lib "kernel32" () As Integer
    Public Declare Function GetConsoleTitle Lib "kernel32" Alias "GetConsoleTitleA" (lpConsoleTitle As String, nSize As Integer) As Integer
    Public Declare Function SetConsoleTitle Lib "kernel32" Alias "SetConsoleTitleA" (lpConsoleTitle As String) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function ReadConsole Lib "kernel32" Alias "ReadConsoleA" (hConsoleInput As Integer, ByRef lpBuffer As Object, nNumberOfCharsToRead As Integer, ByRef lpNumberOfCharsRead As Integer, ByRef lpReserved As Object) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function WriteConsole Lib "kernel32" Alias "WriteConsoleA" (hConsoleOutput As Integer, ByRef lpBuffer As Object, nNumberOfCharsToWrite As Integer, ByRef lpNumberOfCharsWritten As Integer, ByRef lpReserved As Object) As Integer

    Public Const CONSOLE_TEXTMODE_BUFFER As Short = 1

    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_WARNING: ?? SECURITY_ATTRIBUTES ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreateConsoleScreenBuffer Lib "kernel32" (dwDesiredAccess As Integer, dwShareMode As Integer, ByRef lpSecurityAttributes As SECURITY_ATTRIBUTES, dwFlags As Integer, ByRef lpScreenBufferData As Object) As Integer
    Public Declare Function GetConsoleCP Lib "kernel32" () As Integer
    Public Declare Function SetConsoleCP Lib "kernel32" (wCodePageID As Integer) As Integer
    Public Declare Function GetConsoleOutputCP Lib "kernel32" () As Integer
    Public Declare Function SetConsoleOutputCP Lib "kernel32" (wCodePageID As Integer) As Integer


    ' -------------
    '  GDI Section
    ' -------------

    ' Binary raster ops
    Public Const R2_BLACK As Short = 1 '   0
    Public Const R2_NOTMERGEPEN As Short = 2 '  DPon
    Public Const R2_MASKNOTPEN As Short = 3 '  DPna
    Public Const R2_NOTCOPYPEN As Short = 4 '  PN
    Public Const R2_MASKPENNOT As Short = 5 '  PDna
    Public Const R2_NOT As Short = 6 '  Dn
    Public Const R2_XORPEN As Short = 7 '  DPx
    Public Const R2_NOTMASKPEN As Short = 8 '  DPan
    Public Const R2_MASKPEN As Short = 9 '  DPa
    Public Const R2_NOTXORPEN As Short = 10 '  DPxn
    Public Const R2_NOP As Short = 11 '  D
    Public Const R2_MERGENOTPEN As Short = 12 '  DPno
    Public Const R2_COPYPEN As Short = 13 '  P
    Public Const R2_MERGEPENNOT As Short = 14 '  PDno
    Public Const R2_MERGEPEN As Short = 15 '  DPo
    Public Const R2_WHITE As Short = 16 '   1
    Public Const R2_LAST As Short = 16

    '  Ternary raster operations
    Public Const SRCCOPY As Integer = &HCC0020 ' (DWORD) dest = source
    Public Const SRCPAINT As Integer = &HEE0086 ' (DWORD) dest = source OR dest
    Public Const SRCAND As Integer = &H8800C6 ' (DWORD) dest = source AND dest
    Public Const SRCINVERT As Integer = &H660046 ' (DWORD) dest = source XOR dest
    Public Const SRCERASE As Integer = &H440328 ' (DWORD) dest = source AND (NOT dest )
    Public Const NOTSRCCOPY As Integer = &H330008 ' (DWORD) dest = (NOT source)
    Public Const NOTSRCERASE As Integer = &H1100A6 ' (DWORD) dest = (NOT src) AND (NOT dest)
    Public Const MERGECOPY As Integer = &HC000CA ' (DWORD) dest = (source AND pattern)
    Public Const MERGEPAINT As Integer = &HBB0226 ' (DWORD) dest = (NOT source) OR dest
    Public Const PATCOPY As Integer = &HF00021 ' (DWORD) dest = pattern
    Public Const PATPAINT As Integer = &HFB0A09 ' (DWORD) dest = DPSnoo
    Public Const PATINVERT As Integer = &H5A0049 ' (DWORD) dest = pattern XOR dest
    Public Const DSTINVERT As Integer = &H550009 ' (DWORD) dest = (NOT dest)
    Public Const BLACKNESS As Short = &H42S ' (DWORD) dest = BLACK
    Public Const WHITENESS As Integer = &HFF0062 ' (DWORD) dest = WHITE

    Public Const GDI_ERROR As Short = &HFFFFS
    Public Const HGDI_ERROR As Short = &HFFFFS

    ' Region Flags
    Public Const ERRORAPI As Short = 0
    Public Const NULLREGION As Short = 1
    Public Const SIMPLEREGION As Short = 2
    Public Const COMPLEXREGION As Short = 3

    ' CombineRgn() Styles
    Public Const RGN_AND As Short = 1
    Public Const RGN_OR As Short = 2
    Public Const RGN_XOR As Short = 3
    Public Const RGN_DIFF As Short = 4
    Public Const RGN_COPY As Short = 5
    Public Const RGN_MIN As Short = RGN_AND
    Public Const RGN_MAX As Short = RGN_COPY

    ' StretchBlt() Modes
    Public Const BLACKONWHITE As Short = 1
    Public Const WHITEONBLACK As Short = 2
    Public Const COLORONCOLOR As Short = 3
    Public Const HALFTONE As Short = 4
    Public Const MAXSTRETCHBLTMODE As Short = 4

    ' PolyFill() Modes
    Public Const ALTERNATE As Short = 1
    Public Const WINDING As Short = 2
    Public Const POLYFILL_LAST As Short = 2

    ' Text Alignment Options
    Public Const TA_NOUPDATECP As Short = 0
    Public Const TA_UPDATECP As Short = 1

    Public Const TA_LEFT As Short = 0
    Public Const TA_RIGHT As Short = 2
    Public Const TA_CENTER As Short = 6

    Public Const TA_TOP As Short = 0
    Public Const TA_BOTTOM As Short = 8
    Public Const TA_BASELINE As Short = 24
    Public Const TA_MASK As Decimal = (TA_BASELINE + TA_CENTER + TA_UPDATECP)

    Public Const VTA_BASELINE As Short = TA_BASELINE
    Public Const VTA_LEFT As Short = TA_BOTTOM
    Public Const VTA_RIGHT As Short = TA_TOP
    Public Const VTA_CENTER As Short = TA_CENTER
    Public Const VTA_BOTTOM As Short = TA_RIGHT
    Public Const VTA_TOP As Short = TA_LEFT

    Public Const ETO_GRAYED As Short = 1
    Public Const ETO_OPAQUE As Short = 2
    Public Const ETO_CLIPPED As Short = 4

    Public Const ASPECT_FILTERING As Short = &H1S

    Public Const DCB_RESET As Short = &H1S
    Public Const DCB_ACCUMULATE As Short = &H2S
    Public Const DCB_DIRTY As Short = DCB_ACCUMULATE
    Public Const DCB_SET As Boolean = (DCB_RESET Or DCB_ACCUMULATE)
    Public Const DCB_ENABLE As Short = &H4S
    Public Const DCB_DISABLE As Short = &H8S

    ' Metafile Functions
    Public Const META_SETBKCOLOR As Short = &H201S
    Public Const META_SETBKMODE As Short = &H102S
    Public Const META_SETMAPMODE As Short = &H103S
    Public Const META_SETROP2 As Short = &H104S
    Public Const META_SETRELABS As Short = &H105S
    Public Const META_SETPOLYFILLMODE As Short = &H106S
    Public Const META_SETSTRETCHBLTMODE As Short = &H107S
    Public Const META_SETTEXTCHAREXTRA As Short = &H108S
    Public Const META_SETTEXTCOLOR As Short = &H209S
    Public Const META_SETTEXTJUSTIFICATION As Short = &H20AS
    Public Const META_SETWINDOWORG As Short = &H20BS
    Public Const META_SETWINDOWEXT As Short = &H20CS
    Public Const META_SETVIEWPORTORG As Short = &H20DS
    Public Const META_SETVIEWPORTEXT As Short = &H20ES
    Public Const META_OFFSETWINDOWORG As Short = &H20FS
    Public Const META_SCALEWINDOWEXT As Short = &H410S
    Public Const META_OFFSETVIEWPORTORG As Short = &H211S
    Public Const META_SCALEVIEWPORTEXT As Short = &H412S
    Public Const META_LINETO As Short = &H213S
    Public Const META_MOVETO As Short = &H214S
    Public Const META_EXCLUDECLIPRECT As Short = &H415S
    Public Const META_INTERSECTCLIPRECT As Short = &H416S
    Public Const META_ARC As Short = &H817S
    Public Const META_ELLIPSE As Short = &H418S
    Public Const META_FLOODFILL As Short = &H419S
    Public Const META_PIE As Short = &H81AS
    Public Const META_RECTANGLE As Short = &H41BS
    Public Const META_ROUNDRECT As Short = &H61CS
    Public Const META_PATBLT As Short = &H61DS
    Public Const META_SAVEDC As Short = &H1ES
    Public Const META_SETPIXEL As Short = &H41FS
    Public Const META_OFFSETCLIPRGN As Short = &H220S
    Public Const META_TEXTOUT As Short = &H521S
    Public Const META_BITBLT As Short = &H922S
    Public Const META_STRETCHBLT As Short = &HB23S
    Public Const META_POLYGON As Short = &H324S
    Public Const META_POLYLINE As Short = &H325S
    Public Const META_ESCAPE As Short = &H626S
    Public Const META_RESTOREDC As Short = &H127S
    Public Const META_FILLREGION As Short = &H228S
    Public Const META_FRAMEREGION As Short = &H429S
    Public Const META_INVERTREGION As Short = &H12AS
    Public Const META_PAINTREGION As Short = &H12BS
    Public Const META_SELECTCLIPREGION As Short = &H12CS
    Public Const META_SELECTOBJECT As Short = &H12DS
    Public Const META_SETTEXTALIGN As Short = &H12ES
    Public Const META_CHORD As Short = &H830S
    Public Const META_SETMAPPERFLAGS As Short = &H231S
    Public Const META_EXTTEXTOUT As Short = &HA32S
    Public Const META_SETDIBTODEV As Short = &HD33S
    Public Const META_SELECTPALETTE As Short = &H234S
    Public Const META_REALIZEPALETTE As Short = &H35S
    Public Const META_ANIMATEPALETTE As Short = &H436S
    Public Const META_SETPALENTRIES As Short = &H37S
    Public Const META_POLYPOLYGON As Short = &H538S
    Public Const META_RESIZEPALETTE As Short = &H139S
    Public Const META_DIBBITBLT As Short = &H940S
    Public Const META_DIBSTRETCHBLT As Short = &HB41S
    Public Const META_DIBCREATEPATTERNBRUSH As Short = &H142S
    Public Const META_STRETCHDIB As Short = &HF43S
    Public Const META_EXTFLOODFILL As Short = &H548S
    Public Const META_DELETEOBJECT As Short = &H1F0S
    Public Const META_CREATEPALETTE As Short = &HF7S
    Public Const META_CREATEPATTERNBRUSH As Short = &H1F9S
    Public Const META_CREATEPENINDIRECT As Short = &H2FAS
    Public Const META_CREATEFONTINDIRECT As Short = &H2FBS
    Public Const META_CREATEBRUSHINDIRECT As Short = &H2FCS
    Public Const META_CREATEREGION As Short = &H6FFS


    ' GDI Escapes
    Public Const NEWFRAME As Short = 1
    Public Const AbortDocC As Short = 2
    Public Const NEXTBAND As Short = 3
    Public Const SETCOLORTABLE As Short = 4
    Public Const GETCOLORTABLE As Short = 5
    Public Const FLUSHOUTPUT As Short = 6
    Public Const DRAFTMODE As Short = 7
    Public Const QUERYESCSUPPORT As Short = 8
    Public Const SetAbortProc As Short = 9
    Public Const StartDocC As Short = 10
    Public Const EndDocC As Short = 11
    Public Const GETPHYSPAGESIZE As Short = 12
    Public Const GETPRINTINGOFFSET As Short = 13
    Public Const GETSCALINGFACTOR As Short = 14
    Public Const MFCOMMENT As Short = 15
    Public Const GETPENWIDTH As Short = 16
    Public Const SETCOPYCOUNT As Short = 17
    Public Const SELECTPAPERSOURCE As Short = 18
    Public Const DEVICEDATA As Short = 19
    Public Const PASSTHROUGH As Short = 19
    Public Const GETTECHNOLGY As Short = 20
    Public Const GETTECHNOLOGY As Short = 20
    Public Const SETLINECAP As Short = 21
    Public Const SETLINEJOIN As Short = 22
    Public Const SetMiterLimitC As Short = 23
    Public Const BANDINFO As Short = 24
    Public Const DRAWPATTERNRECT As Short = 25
    Public Const GETVECTORPENSIZE As Short = 26
    Public Const GETVECTORBRUSHSIZE As Short = 27
    Public Const ENABLEDUPLEX As Short = 28
    Public Const GETSETPAPERBINS As Short = 29
    Public Const GETSETPRINTORIENT As Short = 30
    Public Const ENUMPAPERBINS As Short = 31
    Public Const SETDIBSCALING As Short = 32
    Public Const EPSPRINTING As Short = 33
    Public Const ENUMPAPERMETRICS As Short = 34
    Public Const GETSETPAPERMETRICS As Short = 35
    Public Const POSTSCRIPT_DATA As Short = 37
    Public Const POSTSCRIPT_IGNORE As Short = 38
    Public Const MOUSETRAILS As Short = 39
    Public Const GETDEVICEUNITS As Short = 42

    Public Const GETEXTENDEDTEXTMETRICS As Short = 256
    Public Const GETEXTENTTABLE As Short = 257
    Public Const GETPAIRKERNTABLE As Short = 258
    Public Const GETTRACKKERNTABLE As Short = 259
    Public Const ExtTextOutC As Short = 512
    Public Const GETFACENAME As Short = 513
    Public Const DOWNLOADFACE As Short = 514
    Public Const ENABLERELATIVEWIDTHS As Short = 768
    Public Const ENABLEPAIRKERNING As Short = 769
    Public Const SETKERNTRACK As Short = 770
    Public Const SETALLJUSTVALUES As Short = 771
    Public Const SETCHARSET As Short = 772

    Public Const StretchBltC As Short = 2048
    Public Const GETSETSCREENPARAMS As Short = 3072
    Public Const BEGIN_PATH As Short = 4096
    Public Const CLIP_TO_PATH As Short = 4097
    Public Const END_PATH As Short = 4098
    Public Const EXT_DEVICE_CAPS As Short = 4099
    Public Const RESTORE_CTM As Short = 4100
    Public Const SAVE_CTM As Short = 4101
    Public Const SET_ARC_DIRECTION As Short = 4102
    Public Const SET_BACKGROUND_COLOR As Short = 4103
    Public Const SET_POLY_MODE As Short = 4104
    Public Const SET_SCREEN_ANGLE As Short = 4105
    Public Const SET_SPREAD As Short = 4106
    Public Const TRANSFORM_CTM As Short = 4107
    Public Const SET_CLIP_BOX As Short = 4108
    Public Const SET_BOUNDS As Short = 4109
    Public Const SET_MIRROR_MODE As Short = 4110
    Public Const OPENCHANNEL As Short = 4110
    Public Const DOWNLOADHEADER As Short = 4111
    Public Const CLOSECHANNEL As Short = 4112
    Public Const POSTSCRIPT_PASSTHROUGH As Short = 4115
    Public Const ENCAPSULATED_POSTSCRIPT As Short = 4116

    ' Spooler Error Codes
    Public Const SP_NOTREPORTED As Short = &H4000S
    Public Const SP_ERROR As Short = (-1)
    Public Const SP_APPABORT As Short = (-2)
    Public Const SP_USERABORT As Short = (-3)
    Public Const SP_OUTOFDISK As Short = (-4)
    Public Const SP_OUTOFMEMORY As Short = (-5)

    Public Const PR_JOBSTATUS As Short = &H0S

    '  Object Definitions for EnumObjects()
    Public Const OBJ_PEN As Short = 1
    Public Const OBJ_BRUSH As Short = 2
    Public Const OBJ_DC As Short = 3
    Public Const OBJ_METADC As Short = 4
    Public Const OBJ_PAL As Short = 5
    Public Const OBJ_FONT As Short = 6
    Public Const OBJ_BITMAP As Short = 7
    Public Const OBJ_REGION As Short = 8
    Public Const OBJ_METAFILE As Short = 9
    Public Const OBJ_MEMDC As Short = 10
    Public Const OBJ_EXTPEN As Short = 11
    Public Const OBJ_ENHMETADC As Short = 12
    Public Const OBJ_ENHMETAFILE As Short = 13

    '  xform stuff
    Public Const MWT_IDENTITY As Short = 1
    Public Const MWT_LEFTMULTIPLY As Short = 2
    Public Const MWT_RIGHTMULTIPLY As Short = 3

    Public Const MWT_MIN As Short = MWT_IDENTITY
    Public Const MWT_MAX As Short = MWT_RIGHTMULTIPLY

    Public Structure xform
        Dim eM11 As Double
        Dim eM12 As Double
        Dim eM21 As Double
        Dim eM22 As Double
        Dim eDx As Double
        Dim eDy As Double
    End Structure

    ' Bitmap Header Definition
    Public Structure BITMAP '14 bytes
        Dim bmType As Integer
        Dim bmWidth As Integer
        Dim bmHeight As Integer
        Dim bmWidthBytes As Integer
        Dim bmPlanes As Short
        Dim bmBitsPixel As Short
        Dim bmBits As Integer
    End Structure

    Public Structure RGBTRIPLE
        Dim rgbtBlue As Byte
        Dim rgbtGreen As Byte
        Dim rgbtRed As Byte
    End Structure

    Public Structure RGBQUAD
        Dim rgbBlue As Byte
        Dim rgbGreen As Byte
        Dim rgbRed As Byte
        Dim rgbReserved As Byte
    End Structure

    ' structures for defining DIBs
    Public Structure BITMAPCOREHEADER '12 bytes
        Dim bcSize As Integer
        Dim bcWidth As Short
        Dim bcHeight As Short
        Dim bcPlanes As Short
        Dim bcBitCount As Short
    End Structure

    Public Structure BITMAPINFOHEADER '40 bytes
        Dim biSize As Integer
        Dim biWidth As Integer
        Dim biHeight As Integer
        Dim biPlanes As Short
        Dim biBitCount As Short
        Dim biCompression As Integer
        Dim biSizeImage As Integer
        Dim biXPelsPerMeter As Integer
        Dim biYPelsPerMeter As Integer
        Dim biClrUsed As Integer
        Dim biClrImportant As Integer
    End Structure

    ' constants for the biCompression field
    Public Const BI_RGB As Short = 0
    Public Const BI_RLE8 As Short = 1
    Public Const BI_RLE4 As Short = 2
    Public Const BI_bitfields As Short = 3

    Public Structure BITMAPINFO
        Dim bmiHeader As BITMAPINFOHEADER
        Dim bmiColors As RGBQUAD
    End Structure

    Public Structure BITMAPCOREINFO
        Dim bmciHeader As BITMAPCOREHEADER
        Dim bmciColors As RGBTRIPLE
    End Structure

    Public Structure BITMAPFILEHEADER
        Dim bfType As Short
        Dim bfSize As Integer
        Dim bfReserved1 As Short
        Dim bfReserved2 As Short
        Dim bfOffBits As Integer
    End Structure


    ' Clipboard Metafile Picture Structure
    Public Structure HANDLETABLE
        <VBFixedArray(1)> Dim objectHandle() As Integer

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim objectHandle(1)
        End Sub
    End Structure

    Public Structure METARECORD
        Dim rdSize As Integer
        Dim rdFunction As Short
        <VBFixedArray(1)> Dim rdParm() As Short

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim rdParm(1)
        End Sub
    End Structure


    Public Structure METAFILEPICT
        Dim mm As Integer
        Dim xExt As Integer
        Dim yExt As Integer
        Dim hMF As Integer
    End Structure

    Public Structure METAHEADER
        Dim mtType As Short
        Dim mtHeaderSize As Short
        Dim mtVersion As Short
        Dim mtSize As Integer
        Dim mtNoObjects As Short
        Dim mtMaxRecord As Integer
        Dim mtNoParameters As Short
    End Structure

    Public Structure ENHMETARECORD
        Dim iType As Integer
        Dim nSize As Integer
        <VBFixedArray(1)> Dim dParm() As Integer

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim dParm(1)
        End Sub
    End Structure

    Public Structure SIZEL
        Dim cx As Integer
        Dim cy As Integer
    End Structure

    Public Structure ENHMETAHEADER
        Dim iType As Integer
        Dim nSize As Integer
        Dim rclBounds As RECTL
        Dim rclFrame As RECTL
        Dim dSignature As Integer
        Dim nVersion As Integer
        Dim nBytes As Integer
        Dim nRecords As Integer
        Dim nHandles As Short
        Dim sReserved As Short
        Dim nDescription As Integer
        Dim offDescription As Integer
        Dim nPalEntries As Integer
        Dim szlDevice As SIZEL
        Dim szlMillimeters As SIZEL
    End Structure

    Public Structure TEXTMETRIC
        Dim tmHeight As Integer
        Dim tmAscent As Integer
        Dim tmDescent As Integer
        Dim tmInternalLeading As Integer
        Dim tmExternalLeading As Integer
        Dim tmAveCharWidth As Integer
        Dim tmMaxCharWidth As Integer
        Dim tmWeight As Integer
        Dim tmOverhang As Integer
        Dim tmDigitizedAspectX As Integer
        Dim tmDigitizedAspectY As Integer
        Dim tmFirstChar As Byte
        Dim tmLastChar As Byte
        Dim tmDefaultChar As Byte
        Dim tmBreakChar As Byte
        Dim tmItalic As Byte
        Dim tmUnderlined As Byte
        Dim tmStruckOut As Byte
        Dim tmPitchAndFamily As Byte
        Dim tmCharSet As Byte
    End Structure

    ' ntmFlags field flags
    Public Const NTM_REGULAR As Integer = &H40
    Public Const NTM_BOLD As Integer = &H20
    Public Const NTM_ITALIC As Integer = &H1

    ' Public Structure passed to FONTENUMPROC
    ' NOTE: NEWTEXTMETRIC is the same as TEXTMETRIC plus 4 new fields
    Public Structure NEWTEXTMETRIC
        Dim tmHeight As Integer
        Dim tmAscent As Integer
        Dim tmDescent As Integer
        Dim tmInternalLeading As Integer
        Dim tmExternalLeading As Integer
        Dim tmAveCharWidth As Integer
        Dim tmMaxCharWidth As Integer
        Dim tmWeight As Integer
        Dim tmOverhang As Integer
        Dim tmDigitizedAspectX As Integer
        Dim tmDigitizedAspectY As Integer
        Dim tmFirstChar As Byte
        Dim tmLastChar As Byte
        Dim tmDefaultChar As Byte
        Dim tmBreakChar As Byte
        Dim tmItalic As Byte
        Dim tmUnderlined As Byte
        Dim tmStruckOut As Byte
        Dim tmPitchAndFamily As Byte
        Dim tmCharSet As Byte
        Dim ntmFlags As Integer
        Dim ntmSizeEM As Integer
        Dim ntmCellHeight As Integer
        Dim ntmAveWidth As Integer
    End Structure

    '  tmPitchAndFamily flags
    Public Const TMPF_FIXED_PITCH As Short = &H1S
    Public Const TMPF_VECTOR As Short = &H2S
    Public Const TMPF_DEVICE As Short = &H8S
    Public Const TMPF_TRUETYPE As Short = &H4S


    ' GDI Logical Objects:

    Public Structure PELARRAY
        Dim paXCount As Integer
        Dim paYCount As Integer
        Dim paXExt As Integer
        Dim paYExt As Integer
        Dim paRGBs As Short
    End Structure

    ' Logical Brush (or Pattern)
    Public Structure LOGBRUSH
        Dim lbStyle As Integer
        Dim lbColor As Integer
        Dim lbHatch As Integer
    End Structure

    ' Logical Pen
    Public Structure LOGPEN
        Dim lopnStyle As Integer
        Dim lopnWidth As POINTAPI
        Dim lopnColor As Integer
    End Structure

    Public Structure EXTLOGPEN
        Dim elpPenStyle As Integer
        Dim elpWidth As Integer
        Dim elpBrushStyle As Integer
        Dim elpColor As Integer
        Dim elpHatch As Integer
        Dim elpNumEntries As Integer
        <VBFixedArray(1)> Dim elpStyleEntry() As Integer

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim elpStyleEntry(1)
        End Sub
    End Structure

    Public Structure PALETTEENTRY
        Dim peRed As Byte
        Dim peGreen As Byte
        Dim peBlue As Byte
        Dim peFlags As Byte
    End Structure

    ' Logical Palette
    Public Structure LOGPALETTE
        Dim palVersion As Short
        Dim palNumEntries As Short
        <VBFixedArray(1)> Dim palPalEntry() As PALETTEENTRY

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim palPalEntry(1)
        End Sub
    End Structure

    ' Logical Font
    Public Const LF_FACESIZE As Short = 32
    Public Const LF_FULLFACESIZE As Short = 64

    Public Structure LOGFONT
        Dim lfHeight As Integer
        Dim lfWidth As Integer
        Dim lfEscapement As Integer
        Dim lfOrientation As Integer
        Dim lfWeight As Integer
        Dim lfItalic As Byte
        Dim lfUnderline As Byte
        Dim lfStrikeOut As Byte
        Dim lfCharSet As Byte
        Dim lfOutPrecision As Byte
        Dim lfClipPrecision As Byte
        Dim lfQuality As Byte
        Dim lfPitchAndFamily As Byte
        <VBFixedArray(LF_FACESIZE)> Dim lfFaceName() As Byte

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim lfFaceName(LF_FACESIZE)
        End Sub
    End Structure

    Public Structure NONCLIENTMETRICS
        Dim cbSize As Integer
        Dim iBorderWidth As Integer
        Dim iScrollWidth As Integer
        Dim iScrollHeight As Integer
        Dim iCaptionWidth As Integer
        Dim iCaptionHeight As Integer
        'UPGRADE_WARNING: ?? lfCaptionFont ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"”
        Dim lfCaptionFont As LOGFONT
        Dim iSMCaptionWidth As Integer
        Dim iSMCaptionHeight As Integer
        'UPGRADE_WARNING: ?? lfSMCaptionFont ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"”
        Dim lfSMCaptionFont As LOGFONT
        Dim iMenuWidth As Integer
        Dim iMenuHeight As Integer
        'UPGRADE_WARNING: ?? lfMenuFont ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"”
        Dim lfMenuFont As LOGFONT
        'UPGRADE_WARNING: ?? lfStatusFont ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"”
        Dim lfStatusFont As LOGFONT
        'UPGRADE_WARNING: ?? lfMessageFont ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"”
        Dim lfMessageFont As LOGFONT

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            lfCaptionFont.Initialize()
            lfSMCaptionFont.Initialize()
            lfMenuFont.Initialize()
            lfStatusFont.Initialize()
            lfMessageFont.Initialize()
        End Sub
    End Structure

    Public Structure ENUMLOGFONT
        'UPGRADE_WARNING: ?? elfLogFont ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"”
        Dim elfLogFont As LOGFONT
        <VBFixedArray(LF_FULLFACESIZE)> Dim elfFullName() As Byte
        <VBFixedArray(LF_FACESIZE)> Dim elfStyle() As Byte

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            elfLogFont.Initialize()
            ReDim elfFullName(LF_FULLFACESIZE)
            ReDim elfStyle(LF_FACESIZE)
        End Sub
    End Structure

    Public Const OUT_DEFAULT_PRECIS As Short = 0
    Public Const OUT_STRING_PRECIS As Short = 1
    Public Const OUT_CHARACTER_PRECIS As Short = 2
    Public Const OUT_STROKE_PRECIS As Short = 3
    Public Const OUT_TT_PRECIS As Short = 4
    Public Const OUT_DEVICE_PRECIS As Short = 5
    Public Const OUT_RASTER_PRECIS As Short = 6
    Public Const OUT_TT_ONLY_PRECIS As Short = 7
    Public Const OUT_OUTLINE_PRECIS As Short = 8

    Public Const CLIP_DEFAULT_PRECIS As Short = 0
    Public Const CLIP_CHARACTER_PRECIS As Short = 1
    Public Const CLIP_STROKE_PRECIS As Short = 2
    Public Const CLIP_MASK As Short = &HFS
    Public Const CLIP_LH_ANGLES As Short = 16
    Public Const CLIP_TT_ALWAYS As Short = 32
    Public Const CLIP_EMBEDDED As Short = 128

    Public Const DEFAULT_QUALITY As Short = 0
    Public Const DRAFT_QUALITY As Short = 1
    Public Const PROOF_QUALITY As Short = 2

    Public Const DEFAULT_PITCH As Short = 0
    Public Const FIXED_PITCH As Short = 1
    Public Const VARIABLE_PITCH As Short = 2

    Public Const ANSI_CHARSET As Short = 0
    Public Const DEFAULT_CHARSET As Short = 1
    Public Const SYMBOL_CHARSET As Short = 2
    Public Const SHIFTJIS_CHARSET As Short = 128
    Public Const HANGEUL_CHARSET As Short = 129
    Public Const CHINESEBIG5_CHARSET As Short = 136
    Public Const OEM_CHARSET As Short = 255

    ' Font Families
    '
    Public Const FF_DONTCARE As Short = 0 '  Don't care or don't know.
    Public Const FF_ROMAN As Short = 16 '  Variable stroke width, serifed.

    ' Times Roman, Century Schoolbook, etc.
    Public Const FF_SWISS As Short = 32 '  Variable stroke width, sans-serifed.

    ' Helvetica, Swiss, etc.
    Public Const FF_MODERN As Short = 48 '  Constant stroke width, serifed or sans-serifed.

    ' Pica, Elite, Courier, etc.
    Public Const FF_SCRIPT As Short = 64 '  Cursive, etc.
    Public Const FF_DECORATIVE As Short = 80 '  Old English, etc.

    ' Font Weights
    Public Const FW_DONTCARE As Short = 0
    Public Const FW_THIN As Short = 100
    Public Const FW_EXTRALIGHT As Short = 200
    Public Const FW_LIGHT As Short = 300
    Public Const FW_NORMAL As Short = 400
    Public Const FW_MEDIUM As Short = 500
    Public Const FW_SEMIBOLD As Short = 600
    Public Const FW_BOLD As Short = 700
    Public Const FW_EXTRABOLD As Short = 800
    Public Const FW_HEAVY As Short = 900

    Public Const FW_ULTRALIGHT As Short = FW_EXTRALIGHT
    Public Const FW_REGULAR As Short = FW_NORMAL
    Public Const FW_DEMIBOLD As Short = FW_SEMIBOLD
    Public Const FW_ULTRABOLD As Short = FW_EXTRABOLD
    Public Const FW_BLACK As Short = FW_HEAVY

    Public Const PANOSE_COUNT As Short = 10
    Public Const PAN_FAMILYTYPE_INDEX As Short = 0
    Public Const PAN_SERIFSTYLE_INDEX As Short = 1
    Public Const PAN_WEIGHT_INDEX As Short = 2
    Public Const PAN_PROPORTION_INDEX As Short = 3
    Public Const PAN_CONTRAST_INDEX As Short = 4
    Public Const PAN_STROKEVARIATION_INDEX As Short = 5
    Public Const PAN_ARMSTYLE_INDEX As Short = 6
    Public Const PAN_LETTERFORM_INDEX As Short = 7
    Public Const PAN_MIDLINE_INDEX As Short = 8
    Public Const PAN_XHEIGHT_INDEX As Short = 9

    Public Const PAN_CULTURE_LATIN As Short = 0

    Public Structure PANOSE
        Dim ulculture As Integer
        Dim bFamilyType As Byte
        Dim bSerifStyle As Byte
        Dim bWeight As Byte
        Dim bProportion As Byte
        Dim bContrast As Byte
        Dim bStrokeVariation As Byte
        Dim bArmStyle As Byte
        Dim bLetterform As Byte
        Dim bMidline As Byte
        Dim bXHeight As Byte
    End Structure

    Public Const PAN_ANY As Short = 0 '  Any
    Public Const PAN_NO_FIT As Short = 1 '  No Fit

    Public Const PAN_FAMILY_TEXT_DISPLAY As Short = 2 '  Text and Display
    Public Const PAN_FAMILY_SCRIPT As Short = 3 '  Script
    Public Const PAN_FAMILY_DECORATIVE As Short = 4 '  Decorative
    Public Const PAN_FAMILY_PICTORIAL As Short = 5 '  Pictorial

    Public Const PAN_SERIF_COVE As Short = 2 '  Cove
    Public Const PAN_SERIF_OBTUSE_COVE As Short = 3 '  Obtuse Cove
    Public Const PAN_SERIF_SQUARE_COVE As Short = 4 '  Square Cove
    Public Const PAN_SERIF_OBTUSE_SQUARE_COVE As Short = 5 '  Obtuse Square Cove
    Public Const PAN_SERIF_SQUARE As Short = 6 '  Square
    Public Const PAN_SERIF_THIN As Short = 7 '  Thin
    Public Const PAN_SERIF_BONE As Short = 8 '  Bone
    Public Const PAN_SERIF_EXAGGERATED As Short = 9 '  Exaggerated
    Public Const PAN_SERIF_TRIANGLE As Short = 10 '  Triangle
    Public Const PAN_SERIF_NORMAL_SANS As Short = 11 '  Normal Sans
    Public Const PAN_SERIF_OBTUSE_SANS As Short = 12 '  Obtuse Sans
    Public Const PAN_SERIF_PERP_SANS As Short = 13 '  Prep Sans
    Public Const PAN_SERIF_FLARED As Short = 14 '  Flared
    Public Const PAN_SERIF_ROUNDED As Short = 15 '  Rounded

    Public Const PAN_WEIGHT_VERY_LIGHT As Short = 2 '  Very Light
    Public Const PAN_WEIGHT_LIGHT As Short = 3 '  Light
    Public Const PAN_WEIGHT_THIN As Short = 4 '  Thin
    Public Const PAN_WEIGHT_BOOK As Short = 5 '  Book
    Public Const PAN_WEIGHT_MEDIUM As Short = 6 '  Medium
    Public Const PAN_WEIGHT_DEMI As Short = 7 '  Demi
    Public Const PAN_WEIGHT_BOLD As Short = 8 '  Bold
    Public Const PAN_WEIGHT_HEAVY As Short = 9 '  Heavy
    Public Const PAN_WEIGHT_BLACK As Short = 10 '  Black
    Public Const PAN_WEIGHT_NORD As Short = 11 '  Nord

    Public Const PAN_PROP_OLD_STYLE As Short = 2 '  Old Style
    Public Const PAN_PROP_MODERN As Short = 3 '  Modern
    Public Const PAN_PROP_EVEN_WIDTH As Short = 4 '  Even Width
    Public Const PAN_PROP_EXPANDED As Short = 5 '  Expanded
    Public Const PAN_PROP_CONDENSED As Short = 6 '  Condensed
    Public Const PAN_PROP_VERY_EXPANDED As Short = 7 '  Very Expanded
    Public Const PAN_PROP_VERY_CONDENSED As Short = 8 '  Very Condensed
    Public Const PAN_PROP_MONOSPACED As Short = 9 '  Monospaced

    Public Const PAN_CONTRAST_NONE As Short = 2 '  None
    Public Const PAN_CONTRAST_VERY_LOW As Short = 3 '  Very Low
    Public Const PAN_CONTRAST_LOW As Short = 4 '  Low
    Public Const PAN_CONTRAST_MEDIUM_LOW As Short = 5 '  Medium Low
    Public Const PAN_CONTRAST_MEDIUM As Short = 6 '  Medium
    Public Const PAN_CONTRAST_MEDIUM_HIGH As Short = 7 '  Mediim High
    Public Const PAN_CONTRAST_HIGH As Short = 8 '  High
    Public Const PAN_CONTRAST_VERY_HIGH As Short = 9 '  Very High

    Public Const PAN_STROKE_GRADUAL_DIAG As Short = 2 '  Gradual/Diagonal
    Public Const PAN_STROKE_GRADUAL_TRAN As Short = 3 '  Gradual/Transitional
    Public Const PAN_STROKE_GRADUAL_VERT As Short = 4 '  Gradual/Vertical
    Public Const PAN_STROKE_GRADUAL_HORZ As Short = 5 '  Gradual/Horizontal
    Public Const PAN_STROKE_RAPID_VERT As Short = 6 '  Rapid/Vertical
    Public Const PAN_STROKE_RAPID_HORZ As Short = 7 '  Rapid/Horizontal
    Public Const PAN_STROKE_INSTANT_VERT As Short = 8 '  Instant/Vertical

    Public Const PAN_STRAIGHT_ARMS_HORZ As Short = 2 '  Straight Arms/Horizontal
    Public Const PAN_STRAIGHT_ARMS_WEDGE As Short = 3 '  Straight Arms/Wedge
    Public Const PAN_STRAIGHT_ARMS_VERT As Short = 4 '  Straight Arms/Vertical
    Public Const PAN_STRAIGHT_ARMS_SINGLE_SERIF As Short = 5 '  Straight Arms/Single-Serif
    Public Const PAN_STRAIGHT_ARMS_DOUBLE_SERIF As Short = 6 '  Straight Arms/Double-Serif
    Public Const PAN_BENT_ARMS_HORZ As Short = 7 '  Non-Straight Arms/Horizontal
    Public Const PAN_BENT_ARMS_WEDGE As Short = 8 '  Non-Straight Arms/Wedge
    Public Const PAN_BENT_ARMS_VERT As Short = 9 '  Non-Straight Arms/Vertical
    Public Const PAN_BENT_ARMS_SINGLE_SERIF As Short = 10 '  Non-Straight Arms/Single-Serif
    Public Const PAN_BENT_ARMS_DOUBLE_SERIF As Short = 11 '  Non-Straight Arms/Double-Serif

    Public Const PAN_LETT_NORMAL_CONTACT As Short = 2 '  Normal/Contact
    Public Const PAN_LETT_NORMAL_WEIGHTED As Short = 3 '  Normal/Weighted
    Public Const PAN_LETT_NORMAL_BOXED As Short = 4 '  Normal/Boxed
    Public Const PAN_LETT_NORMAL_FLATTENED As Short = 5 '  Normal/Flattened
    Public Const PAN_LETT_NORMAL_ROUNDED As Short = 6 '  Normal/Rounded
    Public Const PAN_LETT_NORMAL_OFF_CENTER As Short = 7 '  Normal/Off Center
    Public Const PAN_LETT_NORMAL_SQUARE As Short = 8 '  Normal/Square
    Public Const PAN_LETT_OBLIQUE_CONTACT As Short = 9 '  Oblique/Contact
    Public Const PAN_LETT_OBLIQUE_WEIGHTED As Short = 10 '  Oblique/Weighted
    Public Const PAN_LETT_OBLIQUE_BOXED As Short = 11 '  Oblique/Boxed
    Public Const PAN_LETT_OBLIQUE_FLATTENED As Short = 12 '  Oblique/Flattened
    Public Const PAN_LETT_OBLIQUE_ROUNDED As Short = 13 '  Oblique/Rounded
    Public Const PAN_LETT_OBLIQUE_OFF_CENTER As Short = 14 '  Oblique/Off Center
    Public Const PAN_LETT_OBLIQUE_SQUARE As Short = 15 '  Oblique/Square

    Public Const PAN_MIDLINE_STANDARD_TRIMMED As Short = 2 '  Standard/Trimmed
    Public Const PAN_MIDLINE_STANDARD_POINTED As Short = 3 '  Standard/Pointed
    Public Const PAN_MIDLINE_STANDARD_SERIFED As Short = 4 '  Standard/Serifed
    Public Const PAN_MIDLINE_HIGH_TRIMMED As Short = 5 '  High/Trimmed
    Public Const PAN_MIDLINE_HIGH_POINTED As Short = 6 '  High/Pointed
    Public Const PAN_MIDLINE_HIGH_SERIFED As Short = 7 '  High/Serifed
    Public Const PAN_MIDLINE_CONSTANT_TRIMMED As Short = 8 '  Constant/Trimmed
    Public Const PAN_MIDLINE_CONSTANT_POINTED As Short = 9 '  Constant/Pointed
    Public Const PAN_MIDLINE_CONSTANT_SERIFED As Short = 10 '  Constant/Serifed
    Public Const PAN_MIDLINE_LOW_TRIMMED As Short = 11 '  Low/Trimmed
    Public Const PAN_MIDLINE_LOW_POINTED As Short = 12 '  Low/Pointed
    Public Const PAN_MIDLINE_LOW_SERIFED As Short = 13 '  Low/Serifed

    Public Const PAN_XHEIGHT_CONSTANT_SMALL As Short = 2 '  Constant/Small
    Public Const PAN_XHEIGHT_CONSTANT_STD As Short = 3 '  Constant/Standard
    Public Const PAN_XHEIGHT_CONSTANT_LARGE As Short = 4 '  Constant/Large
    Public Const PAN_XHEIGHT_DUCKING_SMALL As Short = 5 '  Ducking/Small
    Public Const PAN_XHEIGHT_DUCKING_STD As Short = 6 '  Ducking/Standard
    Public Const PAN_XHEIGHT_DUCKING_LARGE As Short = 7 '  Ducking/Large

    Public Const ELF_VENDOR_SIZE As Short = 4

    Public Structure EXTLOGFONT
        'UPGRADE_WARNING: ?? elfLogFont ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"”
        Dim elfLogFont As LOGFONT
        <VBFixedArray(LF_FULLFACESIZE)> Dim elfFullName() As Byte
        <VBFixedArray(LF_FACESIZE)> Dim elfStyle() As Byte
        Dim elfVersion As Integer
        Dim elfStyleSize As Integer
        Dim elfMatch As Integer
        Dim elfReserved As Integer
        <VBFixedArray(ELF_VENDOR_SIZE)> Dim elfVendorId() As Byte
        Dim elfCulture As Integer
        Dim elfPanose As PANOSE

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            elfLogFont.Initialize()
            ReDim elfFullName(LF_FULLFACESIZE)
            ReDim elfStyle(LF_FACESIZE)
            ReDim elfVendorId(ELF_VENDOR_SIZE)
        End Sub
    End Structure

    Public Const ELF_VERSION As Short = 0
    Public Const ELF_CULTURE_LATIN As Short = 0

    '  EnumFonts Masks
    Public Const RASTER_FONTTYPE As Short = &H1S
    Public Const DEVICE_FONTTYPE As Short = &H2S
    Public Const TRUETYPE_FONTTYPE As Short = &H4S

    ' palette entry flags
    Public Const PC_RESERVED As Short = &H1S '  palette index used for animation
    Public Const PC_EXPLICIT As Short = &H2S '  palette index is explicit to device
    Public Const PC_NOCOLLAPSE As Short = &H4S '  do not match color to system palette

    ' Background Modes
    Public Const TRANSPARENT As Short = 1
    Public Const OPAQUE As Short = 2
    Public Const BKMODE_LAST As Short = 2

    '  Graphics Modes
    Public Const GM_COMPATIBLE As Short = 1
    Public Const GM_ADVANCED As Short = 2
    Public Const GM_LAST As Short = 2

    '  PolyDraw and GetPath point types
    Public Const PT_CLOSEFIGURE As Short = &H1S
    Public Const PT_LINETO As Short = &H2S
    Public Const PT_BEZIERTO As Short = &H4S
    Public Const PT_MOVETO As Short = &H6S

    '  Mapping Modes
    Public Const MM_TEXT As Short = 1
    Public Const MM_LOMETRIC As Short = 2
    Public Const MM_HIMETRIC As Short = 3
    Public Const MM_LOENGLISH As Short = 4
    Public Const MM_HIENGLISH As Short = 5
    Public Const MM_TWIPS As Short = 6
    Public Const MM_ISOTROPIC As Short = 7
    Public Const MM_ANISOTROPIC As Short = 8

    '  Min and Max Mapping Mode values
    Public Const MM_MIN As Short = MM_TEXT
    Public Const MM_MAX As Short = MM_ANISOTROPIC
    Public Const MM_MAX_FIXEDSCALE As Short = MM_TWIPS

    ' Coordinate Modes
    Public Const ABSOLUTE As Short = 1
    Public Const RELATIVE As Short = 2

    ' Stock Logical Objects
    Public Const WHITE_BRUSH As Short = 0
    Public Const LTGRAY_BRUSH As Short = 1
    Public Const GRAY_BRUSH As Short = 2
    Public Const DKGRAY_BRUSH As Short = 3
    Public Const BLACK_BRUSH As Short = 4
    Public Const NULL_BRUSH As Short = 5
    Public Const HOLLOW_BRUSH As Short = NULL_BRUSH
    Public Const WHITE_PEN As Short = 6
    Public Const BLACK_PEN As Short = 7
    Public Const NULL_PEN As Short = 8
    Public Const OEM_FIXED_FONT As Short = 10
    Public Const ANSI_FIXED_FONT As Short = 11
    Public Const ANSI_VAR_FONT As Short = 12
    Public Const SYSTEM_FONT As Short = 13
    Public Const DEVICE_DEFAULT_FONT As Short = 14
    Public Const DEFAULT_PALETTE As Short = 15
    Public Const SYSTEM_FIXED_FONT As Short = 16
    Public Const STOCK_LAST As Short = 16

    Public Const CLR_INVALID As Short = &HFFFFS

    ' Brush Styles
    Public Const BS_SOLID As Short = 0
    Public Const BS_NULL As Short = 1
    Public Const BS_HOLLOW As Short = BS_NULL
    Public Const BS_HATCHED As Short = 2
    Public Const BS_PATTERN As Short = 3
    Public Const BS_INDEXED As Short = 4
    Public Const BS_DIBPATTERN As Short = 5
    Public Const BS_DIBPATTERNPT As Short = 6
    Public Const BS_PATTERN8X8 As Short = 7
    Public Const BS_DIBPATTERN8X8 As Short = 8

    '  Hatch Styles
    Public Const HS_HORIZONTAL As Short = 0 '  -----
    Public Const HS_VERTICAL As Short = 1 '  |||||
    Public Const HS_FDIAGONAL As Short = 2 '  \\\\\
    Public Const HS_BDIAGONAL As Short = 3 '  /////
    Public Const HS_CROSS As Short = 4 '  +++++
    Public Const HS_DIAGCROSS As Short = 5 '  xxxxx
    Public Const HS_FDIAGONAL1 As Short = 6
    Public Const HS_BDIAGONAL1 As Short = 7
    Public Const HS_SOLID As Short = 8
    Public Const HS_DENSE1 As Short = 9
    Public Const HS_DENSE2 As Short = 10
    Public Const HS_DENSE3 As Short = 11
    Public Const HS_DENSE4 As Short = 12
    Public Const HS_DENSE5 As Short = 13
    Public Const HS_DENSE6 As Short = 14
    Public Const HS_DENSE7 As Short = 15
    Public Const HS_DENSE8 As Short = 16
    Public Const HS_NOSHADE As Short = 17
    Public Const HS_HALFTONE As Short = 18
    Public Const HS_SOLIDCLR As Short = 19
    Public Const HS_DITHEREDCLR As Short = 20
    Public Const HS_SOLIDTEXTCLR As Short = 21
    Public Const HS_DITHEREDTEXTCLR As Short = 22
    Public Const HS_SOLIDBKCLR As Short = 23
    Public Const HS_DITHEREDBKCLR As Short = 24
    Public Const HS_API_MAX As Short = 25

    '  Pen Styles
    Public Const PS_SOLID As Short = 0
    Public Const PS_DASH As Short = 1 '  -------
    Public Const PS_DOT As Short = 2 '  .......
    Public Const PS_DASHDOT As Short = 3 '  _._._._
    Public Const PS_DASHDOTDOT As Short = 4 '  _.._.._
    Public Const PS_NULL As Short = 5
    Public Const PS_INSIDEFRAME As Short = 6
    Public Const PS_USERSTYLE As Short = 7
    Public Const PS_ALTERNATE As Short = 8
    Public Const PS_STYLE_MASK As Short = &HFS

    Public Const PS_ENDCAP_ROUND As Short = &H0S
    Public Const PS_ENDCAP_SQUARE As Short = &H100S
    Public Const PS_ENDCAP_FLAT As Short = &H200S
    Public Const PS_ENDCAP_MASK As Short = &HF00S

    Public Const PS_JOIN_ROUND As Short = &H0S
    Public Const PS_JOIN_BEVEL As Short = &H1000S
    Public Const PS_JOIN_MITER As Short = &H2000S
    Public Const PS_JOIN_MASK As Short = &HF000S

    Public Const PS_COSMETIC As Short = &H0S
    Public Const PS_GEOMETRIC As Integer = &H10000
    Public Const PS_TYPE_MASK As Integer = &HF0000

    Public Const AD_COUNTERCLOCKWISE As Short = 1
    Public Const AD_CLOCKWISE As Short = 2

    '  Device Parameters for GetDeviceCaps()
    Public Const DRIVERVERSION As Short = 0 '  Device driver version
    Public Const TECHNOLOGY As Short = 2 '  Device classification
    Public Const HORZSIZE As Short = 4 '  Horizontal size in millimeters
    Public Const VERTSIZE As Short = 6 '  Vertical size in millimeters
    Public Const HORZRES As Short = 8 '  Horizontal width in pixels
    Public Const VERTRES As Short = 10 '  Vertical width in pixels
    Public Const BITSPIXEL As Short = 12 '  Number of bits per pixel
    Public Const PLANES As Short = 14 '  Number of planes
    Public Const NUMBRUSHES As Short = 16 '  Number of brushes the device has
    Public Const NUMPENS As Short = 18 '  Number of pens the device has
    Public Const NUMMARKERS As Short = 20 '  Number of markers the device has
    Public Const NUMFONTS As Short = 22 '  Number of fonts the device has
    Public Const NUMCOLORS As Short = 24 '  Number of colors the device supports
    Public Const PDEVICESIZE As Short = 26 '  Size required for device descriptor
    Public Const CURVECAPS As Short = 28 '  Curve capabilities
    Public Const LINECAPS As Short = 30 '  Line capabilities
    Public Const POLYGONALCAPS As Short = 32 '  Polygonal capabilities
    Public Const TEXTCAPS As Short = 34 '  Text capabilities
    Public Const CLIPCAPS As Short = 36 '  Clipping capabilities
    Public Const RASTERCAPS As Short = 38 '  Bitblt capabilities
    Public Const ASPECTX As Short = 40 '  Length of the X leg
    Public Const ASPECTY As Short = 42 '  Length of the Y leg
    Public Const ASPECTXY As Short = 44 '  Length of the hypotenuse

    Public Const LOGPIXELSX As Short = 88 '  Logical pixels/inch in X
    Public Const LOGPIXELSY As Short = 90 '  Logical pixels/inch in Y

    Public Const SIZEPALETTE As Short = 104 '  Number of entries in physical palette
    Public Const NUMRESERVED As Short = 106 '  Number of reserved entries in palette
    Public Const COLORRES As Short = 108 '  Actual color resolution

    '  Printing related DeviceCaps. These replace the appropriate Escapes
    Public Const PHYSICALWIDTH As Short = 110 '  Physical Width in device units
    Public Const PHYSICALHEIGHT As Short = 111 '  Physical Height in device units
    Public Const PHYSICALOFFSETX As Short = 112 '  Physical Printable Area x margin
    Public Const PHYSICALOFFSETY As Short = 113 '  Physical Printable Area y margin
    Public Const SCALINGFACTORX As Short = 114 '  Scaling factor x
    Public Const SCALINGFACTORY As Short = 115 '  Scaling factor y

    '  Device Capability Masks:

    '  Device Technologies
    Public Const DT_PLOTTER As Short = 0 '  Vector plotter
    Public Const DT_RASDISPLAY As Short = 1 '  Raster display
    Public Const DT_RASPRINTER As Short = 2 '  Raster printer
    Public Const DT_RASCAMERA As Short = 3 '  Raster camera
    Public Const DT_CHARSTREAM As Short = 4 '  Character-stream, PLP
    Public Const DT_METAFILE As Short = 5 '  Metafile, VDM
    Public Const DT_DISPFILE As Short = 6 '  Display-file

    '  Curve Capabilities
    Public Const CC_NONE As Short = 0 '  Curves not supported
    Public Const CC_CIRCLES As Short = 1 '  Can do circles
    Public Const CC_PIE As Short = 2 '  Can do pie wedges
    Public Const CC_CHORD As Short = 4 '  Can do chord arcs
    Public Const CC_ELLIPSES As Short = 8 '  Can do ellipese
    Public Const CC_WIDE As Short = 16 '  Can do wide lines
    Public Const CC_STYLED As Short = 32 '  Can do styled lines
    Public Const CC_WIDESTYLED As Short = 64 '  Can do wide styled lines
    Public Const CC_INTERIORS As Short = 128 '  Can do interiors
    Public Const CC_ROUNDRECT As Short = 256 '

    '  Line Capabilities
    Public Const LC_NONE As Short = 0 '  Lines not supported
    Public Const LC_POLYLINE As Short = 2 '  Can do polylines
    Public Const LC_MARKER As Short = 4 '  Can do markers
    Public Const LC_POLYMARKER As Short = 8 '  Can do polymarkers
    Public Const LC_WIDE As Short = 16 '  Can do wide lines
    Public Const LC_STYLED As Short = 32 '  Can do styled lines
    Public Const LC_WIDESTYLED As Short = 64 '  Can do wide styled lines
    Public Const LC_INTERIORS As Short = 128 '  Can do interiors

    '  Polygonal Capabilities
    Public Const PC_NONE As Short = 0 '  Polygonals not supported
    Public Const PC_POLYGON As Short = 1 '  Can do polygons
    Public Const PC_RECTANGLE As Short = 2 '  Can do rectangles
    Public Const PC_WINDPOLYGON As Short = 4 '  Can do winding polygons
    Public Const PC_TRAPEZOID As Short = 4 '  Can do trapezoids
    Public Const PC_SCANLINE As Short = 8 '  Can do scanlines
    Public Const PC_WIDE As Short = 16 '  Can do wide borders
    Public Const PC_STYLED As Short = 32 '  Can do styled borders
    Public Const PC_WIDESTYLED As Short = 64 '  Can do wide styled borders
    Public Const PC_INTERIORS As Short = 128 '  Can do interiors

    '  Polygonal Capabilities
    Public Const CP_NONE As Short = 0 '  No clipping of output
    Public Const CP_RECTANGLE As Short = 1 '  Output clipped to rects
    Public Const CP_REGION As Short = 2 '

    '  Text Capabilities
    Public Const TC_OP_CHARACTER As Short = &H1S '  Can do OutputPrecision   CHARACTER
    Public Const TC_OP_STROKE As Short = &H2S '  Can do OutputPrecision   STROKE
    Public Const TC_CP_STROKE As Short = &H4S '  Can do ClipPrecision     STROKE
    Public Const TC_CR_90 As Short = &H8S '  Can do CharRotAbility    90
    Public Const TC_CR_ANY As Short = &H10S '  Can do CharRotAbility    ANY
    Public Const TC_SF_X_YINDEP As Short = &H20S '  Can do ScaleFreedom      X_YINDEPENDENT
    Public Const TC_SA_DOUBLE As Short = &H40S '  Can do ScaleAbility      DOUBLE
    Public Const TC_SA_INTEGER As Short = &H80S '  Can do ScaleAbility      INTEGER
    Public Const TC_SA_CONTIN As Short = &H100S '  Can do ScaleAbility      CONTINUOUS
    Public Const TC_EA_DOUBLE As Short = &H200S '  Can do EmboldenAbility   DOUBLE
    Public Const TC_IA_ABLE As Short = &H400S '  Can do ItalisizeAbility  ABLE
    Public Const TC_UA_ABLE As Short = &H800S '  Can do UnderlineAbility  ABLE
    Public Const TC_SO_ABLE As Short = &H1000S '  Can do StrikeOutAbility  ABLE
    Public Const TC_RA_ABLE As Short = &H2000S '  Can do RasterFontAble    ABLE
    Public Const TC_VA_ABLE As Short = &H4000S '  Can do VectorFontAble    ABLE
    Public Const TC_RESERVED As Short = &H8000S
    Public Const TC_SCROLLBLT As Integer = &H10000 '  do text scroll with blt

    '  Raster Capabilities
    Public Const RC_NONE As Short = 0
    Public Const RC_BITBLT As Short = 1 '  Can do standard BLT.
    Public Const RC_BANDING As Short = 2 '  Device requires banding support
    Public Const RC_SCALING As Short = 4 '  Device requires scaling support
    Public Const RC_BITMAP64 As Short = 8 '  Device can support >64K bitmap
    Public Const RC_GDI20_OUTPUT As Short = &H10S '  has 2.0 output calls
    Public Const RC_GDI20_STATE As Short = &H20S
    Public Const RC_SAVEBITMAP As Short = &H40S
    Public Const RC_DI_BITMAP As Short = &H80S '  supports DIB to memory
    Public Const RC_PALETTE As Short = &H100S '  supports a palette
    Public Const RC_DIBTODEV As Short = &H200S '  supports DIBitsToDevice
    Public Const RC_BIGFONT As Short = &H400S '  supports >64K fonts
    Public Const RC_STRETCHBLT As Short = &H800S '  supports StretchBlt
    Public Const RC_FLOODFILL As Short = &H1000S '  supports FloodFill
    Public Const RC_STRETCHDIB As Short = &H2000S '  supports StretchDIBits
    Public Const RC_OP_DX_OUTPUT As Short = &H4000S
    Public Const RC_DEVBITS As Short = &H8000S

    ' DIB color table identifiers
    Public Const DIB_RGB_COLORS As Short = 0 '  color table in RGBs
    Public Const DIB_PAL_COLORS As Short = 1 '  color table in palette indices
    Public Const DIB_PAL_INDICES As Short = 2 '  No color table indices into surf palette
    Public Const DIB_PAL_PHYSINDICES As Short = 2 '  No color table indices into surf palette
    Public Const DIB_PAL_LOGINDICES As Short = 4 '  No color table indices into DC palette

    ' constants for Get/SetSystemPaletteUse()
    Public Const SYSPAL_ERROR As Short = 0
    Public Const SYSPAL_STATIC As Short = 1
    Public Const SYSPAL_NOSTATIC As Short = 2

    ' constants for CreateDIBitmap
    Public Const CBM_CREATEDIB As Short = &H2S '  create DIB bitmap
    Public Const CBM_INIT As Short = &H4S '  initialize bitmap

    ' ExtFloodFill style flags
    Public Const FLOODFILLBORDER As Short = 0
    Public Const FLOODFILLSURFACE As Short = 1

    '  size of a device name string
    Public Const CCHDEVICENAME As Short = 32

    '  size of a form name string
    Public Const CCHFORMNAME As Short = 32

    Public Structure DEVMODE
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(CCHDEVICENAME), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=CCHDEVICENAME)> Public dmDeviceName() As Char
        Dim dmSpecVersion As Short
        Dim dmDriverVersion As Short
        Dim dmSize As Short
        Dim dmDriverExtra As Short
        Dim dmFields As Integer
        Dim dmOrientation As Short
        Dim dmPaperSize As Short
        Dim dmPaperLength As Short
        Dim dmPaperWidth As Short
        Dim dmScale As Short
        Dim dmCopies As Short
        Dim dmDefaultSource As Short
        Dim dmPrintQuality As Short
        Dim dmColor As Short
        Dim dmDuplex As Short
        Dim dmYResolution As Short
        Dim dmTTOption As Short
        Dim dmCollate As Short
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(CCHFORMNAME), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=CCHFORMNAME)> Public dmFormName() As Char
        Dim dmUnusedPadding As Short
        Dim dmBitsPerPel As Short
        Dim dmPelsWidth As Integer
        Dim dmPelsHeight As Integer
        Dim dmDisplayFlags As Integer
        Dim dmDisplayFrequency As Integer
    End Structure

    ' current version of specification
    Public Const DM_SPECVERSION As Short = &H320S

    ' field selection bits
    Public Const DM_ORIENTATION As Integer = &H1
    Public Const DM_PAPERSIZE As Integer = &H2
    Public Const DM_PAPERLENGTH As Integer = &H4
    Public Const DM_PAPERWIDTH As Integer = &H8
    Public Const DM_SCALE As Integer = &H10
    Public Const DM_COPIES As Integer = &H100
    Public Const DM_DEFAULTSOURCE As Integer = &H200
    Public Const DM_PRINTQUALITY As Integer = &H400
    Public Const DM_COLOR As Integer = &H800
    Public Const DM_DUPLEX As Integer = &H1000
    Public Const DM_YRESOLUTION As Integer = &H2000
    Public Const DM_TTOPTION As Integer = &H4000
    Public Const DM_COLLATE As Integer = &H8000S
    Public Const DM_FORMNAME As Integer = &H10000

    '  orientation selections
    Public Const DMORIENT_PORTRAIT As Short = 1
    Public Const DMORIENT_LANDSCAPE As Short = 2

    '  paper selections
    Public Const DMPAPER_LETTER As Short = 1
    Public Const DMPAPER_FIRST As Short = DMPAPER_LETTER
    '  Letter 8 1/2 x 11 in
    Public Const DMPAPER_LETTERSMALL As Short = 2 '  Letter Small 8 1/2 x 11 in
    Public Const DMPAPER_TABLOID As Short = 3 '  Tabloid 11 x 17 in
    Public Const DMPAPER_LEDGER As Short = 4 '  Ledger 17 x 11 in
    Public Const DMPAPER_LEGAL As Short = 5 '  Legal 8 1/2 x 14 in
    Public Const DMPAPER_STATEMENT As Short = 6 '  Statement 5 1/2 x 8 1/2 in
    Public Const DMPAPER_EXECUTIVE As Short = 7 '  Executive 7 1/4 x 10 1/2 in
    Public Const DMPAPER_A3 As Short = 8 '  A3 297 x 420 mm
    Public Const DMPAPER_A4 As Short = 9 '  A4 210 x 297 mm
    Public Const DMPAPER_A4SMALL As Short = 10 '  A4 Small 210 x 297 mm
    Public Const DMPAPER_A5 As Short = 11 '  A5 148 x 210 mm
    Public Const DMPAPER_B4 As Short = 12 '  B4 250 x 354
    Public Const DMPAPER_B5 As Short = 13 '  B5 182 x 257 mm
    Public Const DMPAPER_FOLIO As Short = 14 '  Folio 8 1/2 x 13 in
    Public Const DMPAPER_QUARTO As Short = 15 '  Quarto 215 x 275 mm
    Public Const DMPAPER_10X14 As Short = 16 '  10x14 in
    Public Const DMPAPER_11X17 As Short = 17 '  11x17 in
    Public Const DMPAPER_NOTE As Short = 18 '  Note 8 1/2 x 11 in
    Public Const DMPAPER_ENV_9 As Short = 19 '  Envelope #9 3 7/8 x 8 7/8
    Public Const DMPAPER_ENV_10 As Short = 20 '  Envelope #10 4 1/8 x 9 1/2
    Public Const DMPAPER_ENV_11 As Short = 21 '  Envelope #11 4 1/2 x 10 3/8
    Public Const DMPAPER_ENV_12 As Short = 22 '  Envelope #12 4 \276 x 11
    Public Const DMPAPER_ENV_14 As Short = 23 '  Envelope #14 5 x 11 1/2
    Public Const DMPAPER_CSHEET As Short = 24 '  C size sheet
    Public Const DMPAPER_DSHEET As Short = 25 '  D size sheet
    Public Const DMPAPER_ESHEET As Short = 26 '  E size sheet
    Public Const DMPAPER_ENV_DL As Short = 27 '  Envelope DL 110 x 220mm
    Public Const DMPAPER_ENV_C5 As Short = 28 '  Envelope C5 162 x 229 mm
    Public Const DMPAPER_ENV_C3 As Short = 29 '  Envelope C3  324 x 458 mm
    Public Const DMPAPER_ENV_C4 As Short = 30 '  Envelope C4  229 x 324 mm
    Public Const DMPAPER_ENV_C6 As Short = 31 '  Envelope C6  114 x 162 mm
    Public Const DMPAPER_ENV_C65 As Short = 32 '  Envelope C65 114 x 229 mm
    Public Const DMPAPER_ENV_B4 As Short = 33 '  Envelope B4  250 x 353 mm
    Public Const DMPAPER_ENV_B5 As Short = 34 '  Envelope B5  176 x 250 mm
    Public Const DMPAPER_ENV_B6 As Short = 35 '  Envelope B6  176 x 125 mm
    Public Const DMPAPER_ENV_ITALY As Short = 36 '  Envelope 110 x 230 mm
    Public Const DMPAPER_ENV_MONARCH As Short = 37 '  Envelope Monarch 3.875 x 7.5 in
    Public Const DMPAPER_ENV_PERSONAL As Short = 38 '  6 3/4 Envelope 3 5/8 x 6 1/2 in
    Public Const DMPAPER_FANFOLD_US As Short = 39 '  US Std Fanfold 14 7/8 x 11 in
    Public Const DMPAPER_FANFOLD_STD_GERMAN As Short = 40 '  German Std Fanfold 8 1/2 x 12 in
    Public Const DMPAPER_FANFOLD_LGL_GERMAN As Short = 41 '  German Legal Fanfold 8 1/2 x 13 in

    Public Const DMPAPER_LAST As Short = DMPAPER_FANFOLD_LGL_GERMAN

    Public Const DMPAPER_USER As Short = 256

    '  bin selections
    Public Const DMBIN_UPPER As Short = 1
    Public Const DMBIN_FIRST As Short = DMBIN_UPPER

    Public Const DMBIN_ONLYONE As Short = 1
    Public Const DMBIN_LOWER As Short = 2
    Public Const DMBIN_MIDDLE As Short = 3
    Public Const DMBIN_MANUAL As Short = 4
    Public Const DMBIN_ENVELOPE As Short = 5
    Public Const DMBIN_ENVMANUAL As Short = 6
    Public Const DMBIN_AUTO As Short = 7
    Public Const DMBIN_TRACTOR As Short = 8
    Public Const DMBIN_SMALLFMT As Short = 9
    Public Const DMBIN_LARGEFMT As Short = 10
    Public Const DMBIN_LARGECAPACITY As Short = 11
    Public Const DMBIN_CASSETTE As Short = 14
    Public Const DMBIN_LAST As Short = DMBIN_CASSETTE

    Public Const DMBIN_USER As Short = 256 '  device specific bins start here

    '  print qualities
    Public Const DMRES_DRAFT As Short = (-1)
    Public Const DMRES_LOW As Short = (-2)
    Public Const DMRES_MEDIUM As Short = (-3)
    Public Const DMRES_HIGH As Short = (-4)

    '  color enable/disable for color printers
    Public Const DMCOLOR_MONOCHROME As Short = 1
    Public Const DMCOLOR_COLOR As Short = 2

    '  duplex enable
    Public Const DMDUP_SIMPLEX As Short = 1
    Public Const DMDUP_VERTICAL As Short = 2
    Public Const DMDUP_HORIZONTAL As Short = 3

    '  TrueType options
    Public Const DMTT_BITMAP As Short = 1 '  print TT fonts as graphics
    Public Const DMTT_DOWNLOAD As Short = 2 '  download TT fonts as soft fonts
    Public Const DMTT_SUBDEV As Short = 3 '  substitute device fonts for TT fonts

    '  Collation selections
    Public Const DMCOLLATE_FALSE As Short = 0
    Public Const DMCOLLATE_TRUE As Short = 1

    '  DEVMODE dmDisplayFlags flags

    Public Const DM_GRAYSCALE As Short = &H1S
    Public Const DM_INTERLACED As Short = &H2S

    '  GetRegionData/ExtCreateRegion

    Public Const RDH_RECTANGLES As Short = 1

    Public Structure RGNDATAHEADER
        Dim dwSize As Integer
        Dim iType As Integer
        Dim nCount As Integer
        Dim nRgnSize As Integer
        Dim rcBound As RECT
    End Structure

    Public Structure RgnData
        Dim rdh As RGNDATAHEADER
        Dim Buffer As Byte
    End Structure

    Public Structure ABC
        Dim abcA As Integer
        Dim abcB As Integer
        Dim abcC As Integer
    End Structure

    Public Structure ABCFLOAT
        Dim abcfA As Double
        Dim abcfB As Double
        Dim abcfC As Double
    End Structure

    Public Structure OUTLINETEXTMETRIC
        Dim otmSize As Integer
        Dim otmTextMetrics As TEXTMETRIC
        Dim otmFiller As Byte
        Dim otmPanoseNumber As PANOSE
        Dim otmfsSelection As Integer
        Dim otmfsType As Integer
        Dim otmsCharSlopeRise As Integer
        Dim otmsCharSlopeRun As Integer
        Dim otmItalicAngle As Integer
        Dim otmEMSquare As Integer
        Dim otmAscent As Integer
        Dim otmDescent As Integer
        Dim otmLineGap As Integer
        Dim otmsCapEmHeight As Integer
        Dim otmsXHeight As Integer
        Dim otmrcFontBox As RECT
        Dim otmMacAscent As Integer
        Dim otmMacDescent As Integer
        Dim otmMacLineGap As Integer
        Dim otmusMinimumPPEM As Integer
        Dim otmptSubscriptSize As POINTAPI
        Dim otmptSubscriptOffset As POINTAPI
        Dim otmptSuperscriptSize As POINTAPI
        Dim otmptSuperscriptOffset As POINTAPI
        Dim otmsStrikeoutSize As Integer
        Dim otmsStrikeoutPosition As Integer
        Dim otmsUnderscorePosition As Integer
        Dim otmsUnderscoreSize As Integer
        Dim otmpFamilyName As String
        Dim otmpFaceName As String
        Dim otmpStyleName As String
        Dim otmpFullName As String
    End Structure

    Public Structure POLYTEXT
        Dim X As Integer
        Dim Y As Integer
        Dim n As Integer
        Dim lpStr As String
        Dim uiFlags As Integer
        Dim rcl As RECT
        Dim pdx As Integer
    End Structure

    Public Structure FIXED
        Dim fract As Short
        Dim Value As Short
    End Structure

    Public Structure MAT2
        Dim eM11 As FIXED
        Dim eM12 As FIXED
        Dim eM21 As FIXED
        Dim eM22 As FIXED
    End Structure

    Public Structure GLYPHMETRICS
        Dim gmBlackBoxX As Integer
        Dim gmBlackBoxY As Integer
        Dim gmptGlyphOrigin As POINTAPI
        Dim gmCellIncX As Short
        Dim gmCellIncY As Short
    End Structure


    ' GetGlyphOutline constants
    Public Const GGO_METRICS As Short = 0
    Public Const GGO_BITMAP As Short = 1
    Public Const GGO_NATIVE As Short = 2

    Public Const TT_POLYGON_TYPE As Short = 24

    Public Const TT_PRIM_LINE As Short = 1
    Public Const TT_PRIM_QSPLINE As Short = 2

    Public Structure POINTFX
        Dim X As FIXED
        Dim Y As FIXED
    End Structure

    Public Structure TTPOLYCURVE
        Dim wType As Short
        Dim cpfx As Short
        Dim apfx As POINTFX
    End Structure

    Public Structure TTPOLYGONHEADER
        Dim cb As Integer
        Dim dwType As Integer
        Dim pfxStart As POINTFX
    End Structure

    Public Structure RASTERIZER_STATUS
        Dim nSize As Short
        Dim wFlags As Short
        Dim nLanguageID As Short
    End Structure

    ' bits defined in wFlags of RASTERIZER_STATUS
    Public Const TT_AVAILABLE As Short = &H1S
    Public Const TT_ENABLED As Short = &H2S

    Public Declare Function AddFontResource Lib "gdi32" Alias "AddFontResourceA" (lpFileName As String) As Integer

    'UPGRADE_WARNING: ?? PALETTEENTRY ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function AnimatePalette Lib "gdi32" Alias "AnimatePaletteA" (hPalette As Integer, wStartIndex As Integer, wNumEntries As Integer, ByRef lpPaletteColors As PALETTEENTRY) As Integer
    Public Declare Function Arc Lib "gdi32" (hdc As Integer, X1 As Integer, Y1 As Integer, X2 As Integer, Y2 As Integer, X3 As Integer, Y3 As Integer, X4 As Integer, Y4 As Integer) As Integer
    Public Declare Function BitBlt Lib "gdi32" (hDestDC As Integer, X As Integer, Y As Integer, nWidth As Integer, nHeight As Integer, hSrcDC As Integer, xSrc As Integer, ySrc As Integer, dwRop As Integer) As Integer
    Public Declare Function CancelDC Lib "gdi32" (hdc As Integer) As Integer
    Public Declare Function Chord Lib "gdi32" (hdc As Integer, X1 As Integer, Y1 As Integer, X2 As Integer, Y2 As Integer, X3 As Integer, Y3 As Integer, X4 As Integer, Y4 As Integer) As Integer
    Public Declare Function CloseMetaFile Lib "gdi32" (hMF As Integer) As Integer
    Public Declare Function CombineRgn Lib "gdi32" (hDestRgn As Integer, hSrcRgn1 As Integer, hSrcRgn2 As Integer, nCombineMode As Integer) As Integer
    Public Declare Function CopyMetaFile Lib "gdi32" Alias "CopyMetaFileA" (hMF As Integer, lpFileName As String) As Integer

    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function CreateBitmap Lib "gdi32" (nWidth As Integer, nHeight As Integer, nPlanes As Integer, nBitCount As Integer, ByRef lpBits As Object) As Integer
    'UPGRADE_WARNING: ?? BITMAP ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreateBitmapIndirect Lib "gdi32" (ByRef lpBitmap As BITMAP) As Integer
    'UPGRADE_WARNING: ?? LOGBRUSH ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreateBrushIndirect Lib "gdi32" (ByRef lpLogBrush As LOGBRUSH) As Integer
    Public Declare Function CreateCompatibleBitmap Lib "gdi32" (hdc As Integer, nWidth As Integer, nHeight As Integer) As Integer
    Public Declare Function CreateDiscardableBitmap Lib "gdi32" (hdc As Integer, nWidth As Integer, nHeight As Integer) As Integer

    Public Declare Function CreateCompatibleDC Lib "gdi32" (hdc As Integer) As Integer
    'UPGRADE_WARNING: ?? DEVMODE ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreateDC Lib "gdi32" Alias "CreateDCA" (lpDriverName As String, lpDeviceName As String, lpOutput As String, ByRef lpInitData As DEVMODE) As Integer

    'UPGRADE_WARNING: ?? BITMAPINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_WARNING: ?? BITMAPINFOHEADER ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreateDIBitmap Lib "gdi32" (hdc As Integer, ByRef lpInfoHeader As BITMAPINFOHEADER, dwUsage As Integer, ByRef lpInitBits As Object, ByRef lpInitInfo As BITMAPINFO, wUsage As Integer) As Integer
    Public Declare Function CreateDIBPatternBrush Lib "gdi32" (hPackedDIB As Integer, wUsage As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function CreateDIBPatternBrushPt Lib "gdi32" (ByRef lpPackedDIB As Object, iUsage As Integer) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreateEllipticRgnIndirect Lib "gdi32" (ByRef lpRect As RECT) As Integer
    Public Declare Function CreateEllipticRgn Lib "gdi32" (X1 As Integer, Y1 As Integer, X2 As Integer, Y2 As Integer) As Integer

    'UPGRADE_WARNING: ?? LOGFONT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreateFontIndirect Lib "gdi32" Alias "CreateFontIndirectA" (ByRef lpLogFont As LOGFONT) As Integer
    Public Declare Function CreateFont Lib "gdi32" Alias "CreateFontA" (H As Integer, W As Integer, E As Integer, O As Integer, W As Integer, I As Integer, u As Integer, S As Integer, C As Integer, OP As Integer, CP As Integer, Q As Integer, PAF As Integer, F As String) As Integer

    Public Declare Function CreateHatchBrush Lib "gdi32" (nIndex As Integer, crColor As Integer) As Integer
    'UPGRADE_WARNING: ?? DEVMODE ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreateIC Lib "gdi32" Alias "CreateICA" (lpDriverName As String, lpDeviceName As String, lpOutput As String, ByRef lpInitData As DEVMODE) As Integer

    Public Declare Function CreateMetaFile Lib "gdi32" Alias "CreateMetaFileA" (lpString As String) As Integer

    'UPGRADE_WARNING: ?? LOGPALETTE ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreatePalette Lib "gdi32" (ByRef lpLogPalette As LOGPALETTE) As Integer
    Public Declare Function CreatePen Lib "gdi32" (nPenStyle As Integer, nWidth As Integer, crColor As Integer) As Integer
    'UPGRADE_WARNING: ?? LOGPEN ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreatePenIndirect Lib "gdi32" (ByRef lpLogPen As LOGPEN) As Integer
    Public Declare Function CreateRectRgn Lib "gdi32" (X1 As Integer, Y1 As Integer, X2 As Integer, Y2 As Integer) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreateRectRgnIndirect Lib "gdi32" (ByRef lpRect As RECT) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreatePolyPolygonRgn Lib "gdi32" (ByRef lpPoint As POINTAPI, ByRef lpPolyCounts As Integer, nCount As Integer, nPolyFillMode As Integer) As Integer
    Public Declare Function CreatePatternBrush Lib "gdi32" (hBitmap As Integer) As Integer
    Public Declare Function CreateRoundRectRgn Lib "gdi32" (X1 As Integer, Y1 As Integer, X2 As Integer, Y2 As Integer, X3 As Integer, Y3 As Integer) As Integer
    Public Declare Function CreateScalableFontResource Lib "gdi32" Alias "CreateScalableFontResourceA" (fHidden As Integer, lpszResourceFile As String, lpszFontFile As String, lpszCurrentPath As String) As Integer
    Public Declare Function CreateSolidBrush Lib "gdi32" (crColor As Integer) As Integer

    Public Declare Function DeleteDC Lib "gdi32" (hdc As Integer) As Integer
    Public Declare Function DeleteObject Lib "gdi32" (hObject As Integer) As Integer
    Public Declare Function DeleteMetaFile Lib "gdi32" (hMF As Integer) As Integer

    '  mode selections for the device mode function
    Public Const DM_UPDATE As Short = 1
    Public Const DM_COPY As Short = 2
    Public Const DM_PROMPT As Short = 4
    Public Const DM_MODIFY As Short = 8

    Public Const DM_IN_BUFFER As Short = DM_MODIFY
    Public Const DM_IN_PROMPT As Short = DM_PROMPT
    Public Const DM_OUT_BUFFER As Short = DM_COPY
    Public Const DM_OUT_DEFAULT As Short = DM_UPDATE

    '  device capabilities indices
    Public Const DC_FIELDS As Short = 1
    Public Const DC_PAPERS As Short = 2
    Public Const DC_PAPERSIZE As Short = 3
    Public Const DC_MINEXTENT As Short = 4
    Public Const DC_MAXEXTENT As Short = 5
    Public Const DC_BINS As Short = 6
    Public Const DC_DUPLEX As Short = 7
    Public Const DC_SIZE As Short = 8
    Public Const DC_EXTRA As Short = 9
    Public Const DC_VERSION As Short = 10
    Public Const DC_DRIVER As Short = 11
    Public Const DC_BINNAMES As Short = 12
    Public Const DC_ENUMRESOLUTIONS As Short = 13
    Public Const DC_FILEDEPENDENCIES As Short = 14
    Public Const DC_TRUETYPE As Short = 15
    Public Const DC_PAPERNAMES As Short = 16
    Public Const DC_ORIENTATION As Short = 17
    Public Const DC_COPIES As Short = 18

    '  bit fields of the return value (DWORD) for DC_TRUETYPE
    Public Const DCTT_BITMAP As Integer = &H1
    Public Const DCTT_DOWNLOAD As Integer = &H2
    Public Const DCTT_SUBDEV As Integer = &H4



    '  Flags value for COLORADJUSTMENT
    Public Const CA_NEGATIVE As Short = &H1S
    Public Const CA_LOG_FILTER As Short = &H2S

    '  IlluminantIndex values
    Public Const ILLUMINANT_DEVICE_DEFAULT As Short = 0
    Public Const ILLUMINANT_A As Short = 1
    Public Const ILLUMINANT_B As Short = 2
    Public Const ILLUMINANT_C As Short = 3
    Public Const ILLUMINANT_D50 As Short = 4
    Public Const ILLUMINANT_D55 As Short = 5
    Public Const ILLUMINANT_D65 As Short = 6
    Public Const ILLUMINANT_D75 As Short = 7
    Public Const ILLUMINANT_F2 As Short = 8
    Public Const ILLUMINANT_MAX_INDEX As Short = ILLUMINANT_F2

    Public Const ILLUMINANT_TUNGSTEN As Short = ILLUMINANT_A
    Public Const ILLUMINANT_DAYLIGHT As Short = ILLUMINANT_C
    Public Const ILLUMINANT_FLUORESCENT As Short = ILLUMINANT_F2
    Public Const ILLUMINANT_NTSC As Short = ILLUMINANT_C

    '  Min and max for RedGamma, GreenGamma, BlueGamma
    Public Const RGB_GAMMA_MIN As Short = 2500 'words
    Public Const RGB_GAMMA_MAX As Integer = 65000

    '  Min and max for ReferenceBlack and ReferenceWhite
    Public Const REFERENCE_WHITE_MIN As Short = 6000 'words
    Public Const REFERENCE_WHITE_MAX As Short = 10000
    Public Const REFERENCE_BLACK_MIN As Short = 0
    Public Const REFERENCE_BLACK_MAX As Short = 4000

    '  Min and max for Contrast, Brightness, Colorfulness, RedGreenTint
    Public Const COLOR_ADJ_MIN As Short = -100 'shorts
    Public Const COLOR_ADJ_MAX As Short = 100

    Public Structure ColorAdjustment
        Dim caSize As Short
        Dim caFlags As Short
        Dim caIlluminantIndex As Short
        Dim caRedGamma As Short
        Dim caGreenGamma As Short
        Dim caBlueGamma As Short
        Dim caReferenceBlack As Short
        Dim caReferenceWhite As Short
        Dim caContrast As Short
        Dim caBrightness As Short
        Dim caColorfulness As Short
        Dim caRedGreenTint As Short
    End Structure

    Public Structure DOCINFO
        Dim cbSize As Integer
        Dim lpszDocName As String
        Dim lpszOutput As String
    End Structure


    Public Const FONTMAPPER_MAX As Short = 10

    Public Structure KERNINGPAIR
        Dim wFirst As Short
        Dim wSecond As Short
        Dim iKernAmount As Integer
    End Structure


    ' Enhanced metafile constants

    Public Const ENHMETA_SIGNATURE As Integer = &H464D4520

    '  Stock object flag used in the object handle
    ' index in the enhanced metafile records.
    '  E.g. The object handle index (META_STOCK_OBJECT Or BLACK_BRUSH)
    '  represents the stock object BLACK_BRUSH.

    Public Const ENHMETA_STOCK_OBJECT As Integer = &H80000000

    '  Enhanced metafile record types.

    Public Const EMR_HEADER As Short = 1
    Public Const EMR_POLYBEZIER As Short = 2
    Public Const EMR_POLYGON As Short = 3
    Public Const EMR_POLYLINE As Short = 4
    Public Const EMR_POLYBEZIERTO As Short = 5
    Public Const EMR_POLYLINETO As Short = 6
    Public Const EMR_POLYPOLYLINE As Short = 7
    Public Const EMR_POLYPOLYGON As Short = 8
    Public Const EMR_SETWINDOWEXTEX As Short = 9
    Public Const EMR_SETWINDOWORGEX As Short = 10
    Public Const EMR_SETVIEWPORTEXTEX As Short = 11
    Public Const EMR_SETVIEWPORTORGEX As Short = 12
    Public Const EMR_SETBRUSHORGEX As Short = 13
    Public Const EMR_EOF As Short = 14
    Public Const EMR_SETPIXELV As Short = 15
    Public Const EMR_SETMAPPERFLAGS As Short = 16
    Public Const EMR_SETMAPMODE As Short = 17
    Public Const EMR_SETBKMODE As Short = 18
    Public Const EMR_SETPOLYFILLMODE As Short = 19
    Public Const EMR_SETROP2 As Short = 20
    Public Const EMR_SETSTRETCHBLTMODE As Short = 21
    Public Const EMR_SETTEXTALIGN As Short = 22
    Public Const EMR_SETCOLORADJUSTMENT As Short = 23
    Public Const EMR_SETTEXTCOLOR As Short = 24
    Public Const EMR_SETBKCOLOR As Short = 25
    Public Const EMR_OFFSETCLIPRGN As Short = 26
    Public Const EMR_MOVETOEX As Short = 27
    Public Const EMR_SETMETARGN As Short = 28
    Public Const EMR_EXCLUDECLIPRECT As Short = 29
    Public Const EMR_INTERSECTCLIPRECT As Short = 30
    Public Const EMR_SCALEVIEWPORTEXTEX As Short = 31
    Public Const EMR_SCALEWINDOWEXTEX As Short = 32
    Public Const EMR_SAVEDC As Short = 33
    Public Const EMR_RESTOREDC As Short = 34
    Public Const EMR_SETWORLDTRANSFORM As Short = 35
    Public Const EMR_MODIFYWORLDTRANSFORM As Short = 36
    Public Const EMR_SELECTOBJECT As Short = 37
    Public Const EMR_CREATEPEN As Short = 38
    Public Const EMR_CREATEBRUSHINDIRECT As Short = 39
    Public Const EMR_DELETEOBJECT As Short = 40
    Public Const EMR_ANGLEARC As Short = 41
    Public Const EMR_ELLIPSE As Short = 42
    Public Const EMR_RECTANGLE As Short = 43
    Public Const EMR_ROUNDRECT As Short = 44
    Public Const EMR_ARC As Short = 45
    Public Const EMR_CHORD As Short = 46
    Public Const EMR_PIE As Short = 47
    Public Const EMR_SELECTPALETTE As Short = 48
    Public Const EMR_CREATEPALETTE As Short = 49
    Public Const EMR_SETPALETTEENTRIES As Short = 50
    Public Const EMR_RESIZEPALETTE As Short = 51
    Public Const EMR_REALIZEPALETTE As Short = 52
    Public Const EMR_EXTFLOODFILL As Short = 53
    Public Const EMR_LINETO As Short = 54
    Public Const EMR_ARCTO As Short = 55
    Public Const EMR_POLYDRAW As Short = 56
    Public Const EMR_SETARCDIRECTION As Short = 57
    Public Const EMR_SETMITERLIMIT As Short = 58
    Public Const EMR_BEGINPATH As Short = 59
    Public Const EMR_ENDPATH As Short = 60
    Public Const EMR_CLOSEFIGURE As Short = 61
    Public Const EMR_FILLPATH As Short = 62
    Public Const EMR_STROKEANDFILLPATH As Short = 63
    Public Const EMR_STROKEPATH As Short = 64
    Public Const EMR_FLATTENPATH As Short = 65
    Public Const EMR_WIDENPATH As Short = 66
    Public Const EMR_SELECTCLIPPATH As Short = 67
    Public Const EMR_ABORTPATH As Short = 68

    Public Const EMR_GDICOMMENT As Short = 70
    Public Const EMR_FILLRGN As Short = 71
    Public Const EMR_FRAMERGN As Short = 72
    Public Const EMR_INVERTRGN As Short = 73
    Public Const EMR_PAINTRGN As Short = 74
    Public Const EMR_EXTSELECTCLIPRGN As Short = 75
    Public Const EMR_BITBLT As Short = 76
    Public Const EMR_STRETCHBLT As Short = 77
    Public Const EMR_MASKBLT As Short = 78
    Public Const EMR_PLGBLT As Short = 79
    Public Const EMR_SETDIBITSTODEVICE As Short = 80
    Public Const EMR_STRETCHDIBITS As Short = 81
    Public Const EMR_EXTCREATEFONTINDIRECTW As Short = 82
    Public Const EMR_EXTTEXTOUTA As Short = 83
    Public Const EMR_EXTTEXTOUTW As Short = 84
    Public Const EMR_POLYBEZIER16 As Short = 85
    Public Const EMR_POLYGON16 As Short = 86
    Public Const EMR_POLYLINE16 As Short = 87
    Public Const EMR_POLYBEZIERTO16 As Short = 88
    Public Const EMR_POLYLINETO16 As Short = 89
    Public Const EMR_POLYPOLYLINE16 As Short = 90
    Public Const EMR_POLYPOLYGON16 As Short = 91
    Public Const EMR_POLYDRAW16 As Short = 92
    Public Const EMR_CREATEMONOBRUSH As Short = 93
    Public Const EMR_CREATEDIBPATTERNBRUSHPT As Short = 94
    Public Const EMR_EXTCREATEPEN As Short = 95
    Public Const EMR_POLYTEXTOUTA As Short = 96
    Public Const EMR_POLYTEXTOUTW As Short = 97

    Public Const EMR_MIN As Short = 1
    Public Const EMR_MAX As Short = 97

    Public Structure emr
        Dim iType As Integer
        Dim nSize As Integer
    End Structure

    Public Structure emrtext
        Dim ptlReference As POINTL
        Dim nchars As Integer
        Dim offString As Integer
        Dim fOptions As Integer
        Dim rcl As RECTL
        Dim offDx As Integer
    End Structure

    Public Structure EMRABORTPATH
        Dim pEmr As emr
    End Structure

    Public Structure EMRBEGINPATH
        Dim pEmr As emr
    End Structure

    Public Structure EMRENDPATH
        Dim pEmr As emr
    End Structure

    Public Structure EMRCLOSEFIGURE
        Dim pEmr As emr
    End Structure

    Public Structure EMRFLATTENPATH
        Dim pEmr As emr
    End Structure

    Public Structure EMRWIDENPATH
        Dim pEmr As emr
    End Structure

    Public Structure EMRSETMETARGN
        Dim pEmr As emr
    End Structure

    Public Structure EMREMRSAVEDC
        Dim pEmr As emr
    End Structure

    Public Structure EMRREALIZEPALETTE
        Dim pEmr As emr
    End Structure

    Public Structure EMRSELECTCLIPPATH
        Dim pEmr As emr
        Dim iMode As Integer
    End Structure

    Public Structure EMRSETBKMODE
        Dim pEmr As emr
        Dim iMode As Integer
    End Structure

    Public Structure EMRSETMAPMODE
        Dim pEmr As emr
        Dim iMode As Integer
    End Structure

    Public Structure EMRSETPOLYFILLMODE
        Dim pEmr As emr
        Dim iMode As Integer
    End Structure

    Public Structure EMRSETROP2
        Dim pEmr As emr
        Dim iMode As Integer
    End Structure

    Public Structure EMRSETSTRETCHBLTMODE
        Dim pEmr As emr
        Dim iMode As Integer
    End Structure

    Public Structure EMRSETTEXTALIGN
        Dim pEmr As emr
        Dim iMode As Integer
    End Structure

    Public Structure EMRSETMITERLIMIT
        Dim pEmr As emr
        Dim eMiterLimit As Double
    End Structure

    Public Structure EMRRESTOREDC
        Dim pEmr As emr
        Dim iRelative As Integer
    End Structure

    Public Structure EMRSETARCDIRECTION
        Dim pEmr As emr
        Dim iArcDirection As Integer
    End Structure

    Public Structure EMRSETMAPPERFLAGS
        Dim pEmr As emr
        Dim dwFlags As Integer
    End Structure

    Public Structure EMRSETTEXTCOLOR
        Dim pEmr As emr
        Dim crColor As Integer
    End Structure

    Public Structure EMRSETBKCOLOR
        Dim pEmr As emr
        Dim crColor As Integer
    End Structure

    Public Structure EMRSELECTOBJECT
        Dim pEmr As emr
        Dim ihObject As Integer
    End Structure

    Public Structure EMRDELETEOBJECT
        Dim pEmr As emr
        Dim ihObject As Integer
    End Structure

    Public Structure EMRSELECTPALETTE
        Dim pEmr As emr
        Dim ihPal As Integer
    End Structure

    Public Structure EMRRESIZEPALETTE
        Dim pEmr As emr
        Dim ihPal As Integer
        Dim cEntries As Integer
    End Structure

    Public Structure EMRSETPALETTEENTRIES
        Dim pEmr As emr
        Dim ihPal As Integer
        Dim iStart As Integer
        Dim cEntries As Integer
        <VBFixedArray(1)> Dim aPalEntries() As PALETTEENTRY

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim aPalEntries(1)
        End Sub
    End Structure

    Public Structure EMRSETCOLORADJUSTMENT
        Dim pEmr As emr
        'UPGRADE_NOTE: ColorAdjustment ???? ColorAdjustment_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
        Dim ColorAdjustment_Renamed As ColorAdjustment
    End Structure

    Public Structure EMRGDICOMMENT
        Dim pEmr As emr
        Dim cbData As Integer
        <VBFixedArray(1)> Dim Data() As Short

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim Data(1)
        End Sub
    End Structure

    Public Structure EMREOF
        Dim pEmr As emr
        Dim nPalEntries As Integer
        Dim offPalEntries As Integer
        Dim nSizeLast As Integer
    End Structure

    Public Structure EMRLINETO
        Dim pEmr As emr
        Dim ptl As POINTL
    End Structure

    Public Structure EMRMOVETOEX
        Dim pEmr As emr
        Dim ptl As POINTL
    End Structure

    Public Structure EMROFFSETCLIPRGN
        Dim pEmr As emr
        Dim ptlOffset As POINTL
    End Structure

    Public Structure EMRFILLPATH
        Dim pEmr As emr
        Dim rclBounds As RECTL
    End Structure

    Public Structure EMRSTROKEANDFILLPATH
        Dim pEmr As emr
        Dim rclBounds As RECTL
    End Structure

    Public Structure EMRSTROKEPATH
        Dim pEmr As emr
        Dim rclBounds As RECTL
    End Structure

    Public Structure EMREXCLUDECLIPRECT
        Dim pEmr As emr
        Dim rclClip As RECTL
    End Structure

    Public Structure EMRINTERSECTCLIPRECT
        Dim pEmr As emr
        Dim rclClip As RECTL
    End Structure

    Public Structure EMRSETVIEWPORTORGEX
        Dim pEmr As emr
        Dim ptlOrigin As POINTL
    End Structure

    Public Structure EMRSETWINDOWORGEX
        Dim pEmr As emr
        Dim ptlOrigin As POINTL
    End Structure

    Public Structure EMRSETBRUSHORGEX
        Dim pEmr As emr
        Dim ptlOrigin As POINTL
    End Structure

    Public Structure EMRSETVIEWPORTEXTEX
        Dim pEmr As emr
        Dim szlExtent As SIZEL
    End Structure

    Public Structure EMRSETWINDOWEXTEX
        Dim pEmr As emr
        Dim szlExtent As SIZEL
    End Structure

    Public Structure EMRSCALEVIEWPORTEXTEX
        Dim pEmr As emr
        Dim xNum As Integer
        Dim xDenom As Integer
        Dim yNum As Integer
        Dim yDemon As Integer
    End Structure

    Public Structure EMRSCALEWINDOWEXTEX
        Dim pEmr As emr
        Dim xNum As Integer
        Dim xDenom As Integer
        Dim yNum As Integer
        Dim yDemon As Integer
    End Structure

    Public Structure EMRSETWORLDTRANSFORM
        Dim pEmr As emr
        'UPGRADE_NOTE: xform ???? xform_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
        Dim xform_Renamed As xform
    End Structure

    Public Structure EMRMODIFYWORLDTRANSFORM
        Dim pEmr As emr
        'UPGRADE_NOTE: xform ???? xform_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
        Dim xform_Renamed As xform
        Dim iMode As Integer
    End Structure

    Public Structure EMRSETPIXELV
        Dim pEmr As emr
        Dim ptlPixel As POINTL
        Dim crColor As Integer
    End Structure

    Public Structure EMREXTFLOODFILL
        Dim pEmr As emr
        Dim ptlStart As POINTL
        Dim crColor As Integer
        Dim iMode As Integer
    End Structure

    Public Structure EMRELLIPSE
        Dim pEmr As emr
        Dim rclBox As RECTL
    End Structure

    Public Structure EMRRECTANGLE
        Dim pEmr As emr
        Dim rclBox As RECTL
    End Structure

    Public Structure EMRROUNDRECT
        Dim pEmr As emr
        Dim rclBox As RECTL
        Dim szlCorner As SIZEL
    End Structure

    Public Structure EMRARC
        Dim pEmr As emr
        Dim rclBox As RECTL
        Dim ptlStart As POINTL
        Dim ptlEnd As POINTL
    End Structure

    Public Structure EMRARCTO
        Dim pEmr As emr
        Dim rclBox As RECTL
        Dim ptlStart As POINTL
        Dim ptlEnd As POINTL
    End Structure

    Public Structure EMRCHORD
        Dim pEmr As emr
        Dim rclBox As RECTL
        Dim ptlStart As POINTL
        Dim ptlEnd As POINTL
    End Structure

    Public Structure EMRPIE
        Dim pEmr As emr
        Dim rclBox As RECTL
        Dim ptlStart As POINTL
        Dim ptlEnd As POINTL
    End Structure

    Public Structure EMRANGLEARC
        Dim pEmr As emr
        Dim ptlCenter As POINTL
        Dim nRadius As Integer
        Dim eStartAngle As Double
        Dim eSweepAngle As Double
    End Structure

    Public Structure EMRPOLYLINE
        Dim pEmr As emr
        Dim rclBounds As RECTL
        Dim cptl As Integer
        <VBFixedArray(1)> Dim aptl() As POINTL

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim aptl(1)
        End Sub
    End Structure

    Public Structure EMRPOLYBEZIER
        Dim pEmr As emr
        Dim rclBounds As RECTL
        Dim cptl As Integer
        <VBFixedArray(1)> Dim aptl() As POINTL

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim aptl(1)
        End Sub
    End Structure

    Public Structure EMRPOLYGON
        Dim pEmr As emr
        Dim rclBounds As RECTL
        Dim cptl As Integer
        <VBFixedArray(1)> Dim aptl() As POINTL

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim aptl(1)
        End Sub
    End Structure

    Public Structure EMRPOLYBEZIERTO
        Dim pEmr As emr
        Dim rclBounds As RECTL
        Dim cptl As Integer
        <VBFixedArray(1)> Dim aptl() As POINTL

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim aptl(1)
        End Sub
    End Structure

    Public Structure EMRPOLYLINE16
        Dim pEmr As emr
        Dim rclBounds As RECTL
        Dim cpts As Integer
        <VBFixedArray(1)> Dim apts() As POINTS

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim apts(1)
        End Sub
    End Structure

    Public Structure EMRPOLYBEZIER16
        Dim pEmr As emr
        Dim rclBounds As RECTL
        Dim cpts As Integer
        <VBFixedArray(1)> Dim apts() As POINTS

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim apts(1)
        End Sub
    End Structure

    Public Structure EMRPOLYGON16
        Dim pEmr As emr
        Dim rclBounds As RECTL
        Dim cpts As Integer
        <VBFixedArray(1)> Dim apts() As POINTS

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim apts(1)
        End Sub
    End Structure

    Public Structure EMRPLOYBEZIERTO16
        Dim pEmr As emr
        Dim rclBounds As RECTL
        Dim cpts As Integer
        <VBFixedArray(1)> Dim apts() As POINTS

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim apts(1)
        End Sub
    End Structure

    Public Structure EMRPOLYLINETO16
        Dim pEmr As emr
        Dim rclBounds As RECTL
        Dim cpts As Integer
        <VBFixedArray(1)> Dim apts() As POINTS

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim apts(1)
        End Sub
    End Structure

    Public Structure EMRPOLYDRAW
        Dim pEmr As emr
        Dim rclBounds As RECTL
        Dim cptl As Integer
        <VBFixedArray(1)> Dim aptl() As POINTL
        <VBFixedArray(1)> Dim abTypes() As Short

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim aptl(1)
            ReDim abTypes(1)
        End Sub
    End Structure

    Public Structure EMRPOLYDRAW16
        Dim pEmr As emr
        Dim rclBounds As RECTL
        Dim cpts As Integer
        <VBFixedArray(1)> Dim apts() As POINTS
        <VBFixedArray(1)> Dim abTypes() As Short

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim apts(1)
            ReDim abTypes(1)
        End Sub
    End Structure

    Public Structure EMRPOLYPOLYLINE
        Dim pEmr As emr
        Dim rclBounds As RECTL
        Dim nPolys As Integer
        Dim cptl As Integer
        <VBFixedArray(1)> Dim aPolyCounts() As Integer
        <VBFixedArray(1)> Dim aptl() As POINTL

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim aPolyCounts(1)
            ReDim aptl(1)
        End Sub
    End Structure

    Public Structure EMRPOLYPOLYGON
        Dim pEmr As emr
        Dim rclBounds As RECTL
        Dim nPolys As Integer
        Dim cptl As Integer
        <VBFixedArray(1)> Dim aPolyCounts() As Integer
        <VBFixedArray(1)> Dim aptl() As POINTL

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim aPolyCounts(1)
            ReDim aptl(1)
        End Sub
    End Structure

    Public Structure EMRPOLYPOLYLINE16
        Dim pEmr As emr
        Dim rclBounds As RECTL
        Dim nPolys As Integer
        Dim cpts As Integer
        <VBFixedArray(1)> Dim aPolyCounts() As Integer
        <VBFixedArray(1)> Dim apts() As POINTS

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim aPolyCounts(1)
            ReDim apts(1)
        End Sub
    End Structure

    Public Structure EMRPOLYPOLYGON16
        Dim pEmr As emr
        Dim rclBounds As RECTL
        Dim nPolys As Integer
        Dim cpts As Integer
        <VBFixedArray(1)> Dim aPolyCounts() As Integer
        <VBFixedArray(1)> Dim apts() As POINTS

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim aPolyCounts(1)
            ReDim apts(1)
        End Sub
    End Structure

    Public Structure EMRINVERTRGN
        Dim pEmr As emr
        Dim rclBounds As RECTL
        Dim cbRgnData As Integer
        'UPGRADE_NOTE: RgnData ???? RgnData_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
        <VBFixedArray(1)> Dim RgnData_Renamed() As Short

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            'UPGRADE_NOTE: RgnData ???? RgnData_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
            ReDim RgnData_Renamed(1)
        End Sub
    End Structure

    Public Structure EMRPAINTRGN
        Dim pEmr As emr
        Dim rclBounds As RECTL
        Dim cbRgnData As Integer
        'UPGRADE_NOTE: RgnData ???? RgnData_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
        <VBFixedArray(1)> Dim RgnData_Renamed() As Short

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            'UPGRADE_NOTE: RgnData ???? RgnData_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
            ReDim RgnData_Renamed(1)
        End Sub
    End Structure

    Public Structure EMRFILLRGN
        Dim pEmr As emr
        Dim rclBounds As RECTL
        Dim cbRgnData As Integer
        Dim ihBrush As Integer
        'UPGRADE_NOTE: RgnData ???? RgnData_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
        <VBFixedArray(1)> Dim RgnData_Renamed() As Short

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            'UPGRADE_NOTE: RgnData ???? RgnData_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
            ReDim RgnData_Renamed(1)
        End Sub
    End Structure

    Public Structure EMRFRAMERGN
        Dim pEmr As emr
        Dim rclBounds As RECTL
        Dim cbRgnData As Integer
        Dim ihBrush As Integer
        Dim szlStroke As SIZEL
        'UPGRADE_NOTE: RgnData ???? RgnData_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
        <VBFixedArray(1)> Dim RgnData_Renamed() As Short

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            'UPGRADE_NOTE: RgnData ???? RgnData_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
            ReDim RgnData_Renamed(1)
        End Sub
    End Structure

    Public Structure EMREXTSELECTCLIPRGN
        Dim pEmr As emr
        Dim cbRgnData As Integer
        Dim iMode As Integer
        'UPGRADE_NOTE: RgnData ???? RgnData_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
        <VBFixedArray(1)> Dim RgnData_Renamed() As Short

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            'UPGRADE_NOTE: RgnData ???? RgnData_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
            ReDim RgnData_Renamed(1)
        End Sub
    End Structure

    Public Structure EMREXTTEXTOUT
        Dim pEmr As emr
        Dim rclBounds As RECTL
        Dim iGraphicsMode As Integer
        Dim exScale As Double
        Dim eyScale As Double
        'UPGRADE_NOTE: emrtext ???? emrtext_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
        Dim emrtext_Renamed As emrtext
    End Structure

    Public Structure EMRBITBLT
        Dim pEmr As emr
        Dim rclBounds As RECTL
        Dim xDest As Integer
        Dim yDest As Integer
        Dim cxDest As Integer
        Dim cyDest As Integer
        Dim dwRop As Integer
        Dim xSrc As Integer
        Dim ySrc As Integer
        Dim xformSrc As xform
        Dim crBkColorSrc As Integer
        Dim iUsageSrc As Integer
        Dim offBmiSrc As Integer
        Dim cbBmiSrc As Integer
        Dim offBitsSrc As Integer
        Dim cbBitsSrc As Integer
    End Structure

    Public Structure EMRSTRETCHBLT
        Dim pEmr As emr
        Dim rclBounds As RECTL
        Dim xDest As Integer
        Dim yDest As Integer
        Dim cxDest As Integer
        Dim cyDest As Integer
        Dim dwRop As Integer
        Dim xSrc As Integer
        Dim ySrc As Integer
        Dim xformSrc As xform
        Dim crBkColorSrc As Integer
        Dim iUsageSrc As Integer
        Dim offBmiSrc As Integer
        Dim cbBmiSrc As Integer
        Dim offBitsSrc As Integer
        Dim cbBitsSrc As Integer
        Dim cxSrc As Integer
        Dim cySrc As Integer
    End Structure

    Public Structure EMRMASKBLT
        Dim pEmr As emr
        Dim rclBounds As RECTL
        Dim xDest As Integer
        Dim yDest As Integer
        Dim cxDest As Integer
        Dim cyDest As Integer
        Dim dwRop As Integer
        Dim xSrc2 As Integer
        Dim cyDest2 As Integer
        Dim dwRop2 As Integer
        Dim xSrc As Integer
        Dim ySrc As Integer
        Dim xformSrc As xform
        Dim crBkColorSrc As Integer
        Dim iUsageSrc As Integer
        Dim offBmiSrc As Integer
        Dim cbBmiSrc As Integer
        Dim offBitsSrc As Integer
        Dim cbBitsSrc As Integer
        Dim xMask As Integer
        Dim yMask As Integer
        Dim iUsageMask As Integer
        Dim offBmiMask As Integer
        Dim cbBmiMask As Integer
        Dim offBitsMask As Integer
        Dim cbBitsMask As Integer
    End Structure

    Public Structure EMRPLGBLT
        Dim pEmr As emr
        Dim rclBounds As RECTL
        <VBFixedArray(3)> Dim aptlDest() As POINTL
        Dim xSrc As Integer
        Dim ySrc As Integer
        Dim cxSrc As Integer
        Dim cySrc As Integer
        Dim xformSrc As xform
        Dim crBkColorSrc As Integer
        Dim iUsageSrc As Integer
        Dim offBmiSrc As Integer
        Dim cbBmiSrc As Integer
        Dim offBitsSrc As Integer
        Dim cbBitsSrc As Integer
        Dim xMask As Integer
        Dim yMask As Integer
        Dim iUsageMask As Integer
        Dim offBmiMask As Integer
        Dim cbBmiMask As Integer
        Dim offBitsMask As Integer
        Dim cbBitsMask As Integer

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim aptlDest(3)
        End Sub
    End Structure

    Public Structure EMRSETDIBITSTODEVICE
        Dim pEmr As emr
        Dim rclBounds As RECTL
        Dim xDest As Integer
        Dim yDest As Integer
        Dim xSrc As Integer
        Dim ySrc As Integer
        Dim cxSrc As Integer
        Dim cySrc As Integer
        Dim offBmiSrc As Integer
        Dim cbBmiSrc As Integer
        Dim offBitsSrc As Integer
        Dim cbBitsSrc As Integer
        Dim iUsageSrc As Integer
        Dim iStartScan As Integer
        Dim cScans As Integer
    End Structure

    Public Structure EMRSTRETCHDIBITS
        Dim pEmr As emr
        Dim rclBounds As RECTL
        Dim xDest As Integer
        Dim yDest As Integer
        Dim xSrc As Integer
        Dim ySrc As Integer
        Dim cxSrc As Integer
        Dim cySrc As Integer
        Dim offBmiSrc As Integer
        Dim cbBmiSrc As Integer
        Dim offBitsSrc As Integer
        Dim cbBitsSrc As Integer
        Dim iUsageSrc As Integer
        Dim dwRop As Integer
        Dim cxDest As Integer
        Dim cyDest As Integer
    End Structure

    Public Structure EMREXTCREATEFONTINDIRECT
        Dim pEmr As emr
        Dim ihFont As Integer
        'UPGRADE_WARNING: ?? elfw ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"”
        Dim elfw As EXTLOGFONT

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            elfw.Initialize()
        End Sub
    End Structure

    Public Structure EMRCREATEPALETTE
        Dim pEmr As emr
        Dim ihPal As Integer
        'UPGRADE_WARNING: ?? lgpl ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"”
        Dim lgpl As LOGPALETTE

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            lgpl.Initialize()
        End Sub
    End Structure

    Public Structure EMRCREATEPEN
        Dim pEmr As emr
        Dim ihPen As Integer
        Dim lopn As LOGPEN
    End Structure

    Public Structure EMREXTCREATEPEN
        Dim pEmr As emr
        Dim ihPen As Integer
        Dim offBmi As Integer
        Dim cbBmi As Integer
        Dim offBits As Integer
        Dim cbBits As Integer
        'UPGRADE_WARNING: ?? elp ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"”
        Dim elp As EXTLOGPEN

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            elp.Initialize()
        End Sub
    End Structure

    Public Structure EMRCREATEBRUSHINDIRECT
        Dim pEmr As emr
        Dim ihBrush As Integer
        Dim lb As LOGBRUSH
    End Structure

    Public Structure EMRCREATEMONOBRUSH
        Dim pEmr As emr
        Dim ihBrush As Integer
        Dim iUsage As Integer
        Dim offBmi As Integer
        Dim cbBmi As Integer
        Dim offBits As Integer
        Dim cbBits As Integer
    End Structure

    Public Structure EMRCREATEDIBPATTERNBRUSHPT
        Dim pEmr As emr
        Dim ihBursh As Integer
        Dim iUsage As Integer
        Dim offBmi As Integer
        Dim cbBmi As Integer
        Dim offBits As Integer
        Dim cbBits As Integer
    End Structure

    ' new wingdi
    ' *************************************************************************
    ' *                                                                         *
    ' * wingdi.h -- GDI procedure declarations, constant definitions and macros *
    ' *                                                                         *
    ' * Copyright (c) 1985-1995, Microsoft Corp. All rights reserved.           *
    ' *                                                                         *
    ' **************************************************************************/

    '  StretchBlt() Modes
    Public Const STRETCH_ANDSCANS As Short = 1
    Public Const STRETCH_ORSCANS As Short = 2
    Public Const STRETCH_DELETESCANS As Short = 3
    Public Const STRETCH_HALFTONE As Short = 4

    Public Structure BITMAPV4HEADER
        Dim bV4Size As Integer
        Dim bV4Width As Integer
        Dim bV4Height As Integer
        Dim bV4Planes As Short
        Dim bV4BitCount As Short
        Dim bV4V4Compression As Integer
        Dim bV4SizeImage As Integer
        Dim bV4XPelsPerMeter As Integer
        Dim bV4YPelsPerMeter As Integer
        Dim bV4ClrUsed As Integer
        Dim bV4ClrImportant As Integer
        Dim bV4RedMask As Integer
        Dim bV4GreenMask As Integer
        Dim bV4BlueMask As Integer
        Dim bV4AlphaMask As Integer
        Dim bV4CSType As Integer
        Dim bV4Endpoints As Integer
        Dim bV4GammaRed As Integer
        Dim bV4GammaGreen As Integer
        Dim bV4GammaBlue As Integer
    End Structure

    Public Structure FONTSIGNATURE
        <VBFixedArray(4)> Dim fsUsb() As Integer
        <VBFixedArray(2)> Dim fsCsb() As Integer

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim fsUsb(4)
            ReDim fsCsb(2)
        End Sub
    End Structure

    Public Structure CHARSETINFO
        Dim ciCharset As Integer
        Dim ciACP As Integer
        'UPGRADE_WARNING: ?? fs ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"”
        Dim fs As FONTSIGNATURE

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            fs.Initialize()
        End Sub
    End Structure

    Public Const TCI_SRCCHARSET As Short = 1
    Public Const TCI_SRCCODEPAGE As Short = 2
    Public Const TCI_SRCFONTSIG As Short = 3

    Public Structure LOCALESIGNATURE
        <VBFixedArray(4)> Dim lsUsb() As Integer
        <VBFixedArray(2)> Dim lsCsbDefault() As Integer
        <VBFixedArray(2)> Dim lsCsbSupported() As Integer

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim lsUsb(4)
            ReDim lsCsbDefault(2)
            ReDim lsCsbSupported(2)
        End Sub
    End Structure

    Public Structure NEWTEXTMETRICEX
        Dim ntmTm As NEWTEXTMETRIC
        'UPGRADE_WARNING: ?? ntmFontSig ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"”
        Dim ntmFontSig As FONTSIGNATURE

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ntmFontSig.Initialize()
        End Sub
    End Structure

    Public Structure ENUMLOGFONTEX
        'UPGRADE_WARNING: ?? elfLogFont ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"”
        Dim elfLogFont As LOGFONT
        <VBFixedArray(LF_FULLFACESIZE)> Dim elfFullName() As Byte
        <VBFixedArray(LF_FACESIZE)> Dim elfStyle() As Byte
        <VBFixedArray(LF_FACESIZE)> Dim elfScript() As Byte

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            elfLogFont.Initialize()
            ReDim elfFullName(LF_FULLFACESIZE)
            ReDim elfStyle(LF_FACESIZE)
            ReDim elfScript(LF_FACESIZE)
        End Sub
    End Structure

    Public Const MONO_FONT As Short = 8
    Public Const JOHAB_CHARSET As Short = 130
    Public Const HEBREW_CHARSET As Short = 177
    Public Const ARABIC_CHARSET As Short = 178
    Public Const GREEK_CHARSET As Short = 161
    Public Const TURKISH_CHARSET As Short = 162
    Public Const THAI_CHARSET As Short = 222
    Public Const EASTEUROPE_CHARSET As Short = 238
    Public Const RUSSIAN_CHARSET As Short = 204

    Public Const MAC_CHARSET As Short = 77
    Public Const BALTIC_CHARSET As Short = 186

    Public Const FS_LATIN1 As Integer = &H1
    Public Const FS_LATIN2 As Integer = &H2
    Public Const FS_CYRILLIC As Integer = &H4
    Public Const FS_GREEK As Integer = &H8
    Public Const FS_TURKISH As Integer = &H10
    Public Const FS_HEBREW As Integer = &H20
    Public Const FS_ARABIC As Integer = &H40
    Public Const FS_BALTIC As Integer = &H80
    Public Const FS_THAI As Integer = &H10000
    Public Const FS_JISJAPAN As Integer = &H20000
    Public Const FS_CHINESESIMP As Integer = &H40000
    Public Const FS_WANSUNG As Integer = &H80000
    Public Const FS_CHINESETRAD As Integer = &H100000
    Public Const FS_JOHAB As Integer = &H200000
    Public Const FS_SYMBOL As Integer = &H80000000
    Public Const DEFAULT_GUI_FONT As Short = 17
    '  current version of specification
    Public Const DM_RESERVED1 As Integer = &H800000
    Public Const DM_RESERVED2 As Integer = &H1000000
    Public Const DM_ICMMETHOD As Integer = &H2000000
    Public Const DM_ICMINTENT As Integer = &H4000000
    Public Const DM_MEDIATYPE As Integer = &H8000000
    Public Const DM_DITHERTYPE As Integer = &H10000000
    Public Const DMPAPER_ISO_B4 As Short = 42 '  B4 (ISO) 250 x 353 mm
    Public Const DMPAPER_JAPANESE_POSTCARD As Short = 43 '  Japanese Postcard 100 x 148 mm
    Public Const DMPAPER_9X11 As Short = 44 '  9 x 11 in
    Public Const DMPAPER_10X11 As Short = 45 '  10 x 11 in
    Public Const DMPAPER_15X11 As Short = 46 '  15 x 11 in
    Public Const DMPAPER_ENV_INVITE As Short = 47 '  Envelope Invite 220 x 220 mm
    Public Const DMPAPER_RESERVED_48 As Short = 48 '  RESERVED--DO NOT USE
    Public Const DMPAPER_RESERVED_49 As Short = 49 '  RESERVED--DO NOT USE
    Public Const DMPAPER_LETTER_EXTRA As Short = 50 '  Letter Extra 9 \275 x 12 in
    Public Const DMPAPER_LEGAL_EXTRA As Short = 51 '  Legal Extra 9 \275 x 15 in
    Public Const DMPAPER_TABLOID_EXTRA As Short = 52 '  Tabloid Extra 11.69 x 18 in
    Public Const DMPAPER_A4_EXTRA As Short = 53 '  A4 Extra 9.27 x 12.69 in
    Public Const DMPAPER_LETTER_TRANSVERSE As Short = 54 '  Letter Transverse 8 \275 x 11 in
    Public Const DMPAPER_A4_TRANSVERSE As Short = 55 '  A4 Transverse 210 x 297 mm
    Public Const DMPAPER_LETTER_EXTRA_TRANSVERSE As Short = 56 '  Letter Extra Transverse 9\275 x 12 in
    Public Const DMPAPER_A_PLUS As Short = 57 '  SuperA/SuperA/A4 227 x 356 mm
    Public Const DMPAPER_B_PLUS As Short = 58 '  SuperB/SuperB/A3 305 x 487 mm
    Public Const DMPAPER_LETTER_PLUS As Short = 59 '  Letter Plus 8.5 x 12.69 in
    Public Const DMPAPER_A4_PLUS As Short = 60 '  A4 Plus 210 x 330 mm
    Public Const DMPAPER_A5_TRANSVERSE As Short = 61 '  A5 Transverse 148 x 210 mm
    Public Const DMPAPER_B5_TRANSVERSE As Short = 62 '  B5 (JIS) Transverse 182 x 257 mm
    Public Const DMPAPER_A3_EXTRA As Short = 63 '  A3 Extra 322 x 445 mm
    Public Const DMPAPER_A5_EXTRA As Short = 64 '  A5 Extra 174 x 235 mm
    Public Const DMPAPER_B5_EXTRA As Short = 65 '  B5 (ISO) Extra 201 x 276 mm
    Public Const DMPAPER_A2 As Short = 66 '  A2 420 x 594 mm
    Public Const DMPAPER_A3_TRANSVERSE As Short = 67 '  A3 Transverse 297 x 420 mm
    Public Const DMPAPER_A3_EXTRA_TRANSVERSE As Short = 68 '  A3 Extra Transverse 322 x 445 mm
    Public Const DMTT_DOWNLOAD_OUTLINE As Short = 4 '  download TT fonts as outline soft fonts

    '  ICM methods
    Public Const DMICMMETHOD_NONE As Short = 1 '  ICM disabled
    Public Const DMICMMETHOD_SYSTEM As Short = 2 '  ICM handled by system
    Public Const DMICMMETHOD_DRIVER As Short = 3 '  ICM handled by driver
    Public Const DMICMMETHOD_DEVICE As Short = 4 '  ICM handled by device
    Public Const DMICMMETHOD_USER As Short = 256 '  Device-specific methods start here

    '  ICM Intents
    Public Const DMICM_SATURATE As Short = 1 '  Maximize color saturation
    Public Const DMICM_CONTRAST As Short = 2 '  Maximize color contrast
    Public Const DMICM_COLORMETRIC As Short = 3 '  Use specific color metric
    Public Const DMICM_USER As Short = 256 '  Device-specific intents start here

    '  Media types
    Public Const DMMEDIA_STANDARD As Short = 1 '  Standard paper
    Public Const DMMEDIA_GLOSSY As Short = 2 '  Glossy paper
    Public Const DMMEDIA_TRANSPARENCY As Short = 3 '  Transparency

    Public Const DMMEDIA_USER As Short = 256 '  Device-specific media start here

    '  Dither types
    Public Const DMDITHER_NONE As Short = 1 '  No dithering
    Public Const DMDITHER_COARSE As Short = 2 '  Dither with a coarse brush
    Public Const DMDITHER_FINE As Short = 3 '  Dither with a fine brush
    Public Const DMDITHER_LINEART As Short = 4 '  LineArt dithering
    Public Const DMDITHER_GRAYSCALE As Short = 5 '  Device does grayscaling
    Public Const DMDITHER_USER As Short = 256 '  Device-specific dithers start here
    Public Const GGO_GRAY2_BITMAP As Short = 4
    Public Const GGO_GRAY4_BITMAP As Short = 5
    Public Const GGO_GRAY8_BITMAP As Short = 6
    Public Const GGO_GLYPH_INDEX As Short = &H80S
    Public Const GCP_DBCS As Short = &H1S
    Public Const GCP_REORDER As Short = &H2S
    Public Const GCP_USEKERNING As Short = &H8S
    Public Const GCP_GLYPHSHAPE As Short = &H10S
    Public Const GCP_LIGATE As Short = &H20S
    Public Const GCP_DIACRITIC As Short = &H100S
    Public Const GCP_KASHIDA As Short = &H400S
    Public Const GCP_ERROR As Short = &H8000S
    Public Const FLI_MASK As Short = &H103BS
    Public Const GCP_JUSTIFY As Integer = &H10000
    Public Const GCP_NODIACRITICS As Integer = &H20000
    Public Const FLI_GLYPHS As Integer = &H40000
    Public Const GCP_CLASSIN As Integer = &H80000
    Public Const GCP_MAXEXTENT As Integer = &H100000
    Public Const GCP_JUSTIFYIN As Integer = &H200000
    Public Const GCP_DISPLAYZWG As Integer = &H400000
    Public Const GCP_SYMSWAPOFF As Integer = &H800000
    Public Const GCP_NUMERICOVERRIDE As Integer = &H1000000
    Public Const GCP_NEUTRALOVERRIDE As Integer = &H2000000
    Public Const GCP_NUMERICSLATIN As Integer = &H4000000
    Public Const GCP_NUMERICSLOCAL As Integer = &H8000000
    Public Const GCPCLASS_LATIN As Short = 1
    Public Const GCPCLASS_HEBREW As Short = 2
    Public Const GCPCLASS_ARABIC As Short = 2
    Public Const GCPCLASS_NEUTRAL As Short = 3
    Public Const GCPCLASS_LOCALNUMBER As Short = 4
    Public Const GCPCLASS_LATINNUMBER As Short = 5
    Public Const GCPCLASS_LATINNUMERICTERMINATOR As Short = 6
    Public Const GCPCLASS_LATINNUMERICSEPARATOR As Short = 7
    Public Const GCPCLASS_NUMERICSEPARATOR As Short = 8
    Public Const GCPCLASS_PREBOUNDRTL As Short = &H80S
    Public Const GCPCLASS_PREBOUNDLTR As Short = &H40S



    Public Structure GCP_RESULTS
        Dim lStructSize As Integer
        Dim lpOutString As String
        Dim lpOrder As Integer
        Dim lpDX As Integer
        Dim lpCaretPos As Integer
        Dim lpClass As String
        Dim lpGlyphs As String
        Dim nGlyphs As Integer
        Dim nMaxFit As Integer
    End Structure

    Public Const DC_BINADJUST As Short = 19
    Public Const DC_EMF_COMPLIANT As Short = 20
    Public Const DC_DATATYPE_PRODUCED As Short = 21
    Public Const DC_COLLATE As Short = 22

    Public Const DCTT_DOWNLOAD_OUTLINE As Integer = &H8

    '  return values for DC_BINADJUST
    Public Const DCBA_FACEUPNONE As Short = &H0S
    Public Const DCBA_FACEUPCENTER As Short = &H1S
    Public Const DCBA_FACEUPLEFT As Short = &H2S
    Public Const DCBA_FACEUPRIGHT As Short = &H3S
    Public Const DCBA_FACEDOWNNONE As Short = &H100S
    Public Const DCBA_FACEDOWNCENTER As Short = &H101S
    Public Const DCBA_FACEDOWNLEFT As Short = &H102S
    Public Const DCBA_FACEDOWNRIGHT As Short = &H103S

    Public Declare Function EnumFontFamilies Lib "gdi32" Alias "EnumFontFamiliesA" (hdc As Integer, lpszFamily As String, lpEnumFontFamProc As Integer, lParam As Integer) As Integer
    'UPGRADE_WARNING: ?? LOGFONT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function EnumFontFamiliesEx Lib "gdi32" Alias "EnumFontFamiliesExA" (hdc As Integer, ByRef lpLogFont As LOGFONT, lpEnumFontProc As Integer, lParam As Integer, dw As Integer) As Integer
    Public Declare Function GetTextCharset Lib "gdi32" (hdc As Integer) As Integer
    'UPGRADE_WARNING: ?? FONTSIGNATURE ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetTextCharsetInfo Lib "gdi32" (hdc As Integer, ByRef lpSig As FONTSIGNATURE, dwFlags As Integer) As Integer

    'UPGRADE_WARNING: ?? CHARSETINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function TranslateCharsetInfo Lib "gdi32" (ByRef lpSrc As Integer, ByRef lpcs As CHARSETINFO, dwFlags As Integer) As Integer
    Public Declare Function GetFontLanguageInfo Lib "gdi32" (hdc As Integer) As Integer
    'UPGRADE_WARNING: ?? GCP_RESULTS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetCharacterPlacement Lib "gdi32" Alias " GetCharacterPlacementA" (hdc As Integer, lpsz As String, n1 As Integer, n2 As Integer, ByRef lpGcpResults As GCP_RESULTS, dw As Integer) As Integer

    Public Const ICM_OFF As Short = 1
    Public Const ICM_ON As Short = 2
    Public Const ICM_QUERY As Short = 3

    Public Structure CIEXYZ
        Dim ciexyzX As Integer
        Dim ciexyzY As Integer
        Dim ciexyzZ As Integer
    End Structure

    Public Structure CIEXYZTRIPLE
        Dim ciexyzRed As CIEXYZ
        Dim ciexyzGreen As CIEXYZ
        Dim ciexyBlue As CIEXYZ
    End Structure

    Public Structure LOGCOLORSPACE
        Dim lcsSignature As Integer
        Dim lcsVersion As Integer
        Dim lcsSize As Integer
        Dim lcsCSType As Integer
        Dim lcsIntent As Integer
        Dim lcsEndPoints As CIEXYZTRIPLE
        Dim lcsGammaRed As Integer
        Dim lcsGammaGreen As Integer
        Dim lcsGammaBlue As Integer
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(MAX_PATH), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=MAX_PATH)> Public lcsFileName() As Char
    End Structure

    Public Declare Function SetICMMode Lib "gdi32" (hdc As Integer, n As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function CheckColorsInGamut Lib "gdi32" (hdc As Integer, ByRef lpv As Object, ByRef lpv2 As Object, dw As Integer) As Integer
    'UPGRADE_WARNING: ?? LOGCOLORSPACE ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetLogColorSpace Lib "gdi32" Alias "GetLogColorSpaceA" (hcolorspace As Integer, lplogcolorspace As LOGCOLORSPACE, dw As Integer) As Integer
    Public Declare Function GetColorSpace Lib "gdi32" (hdc As Integer) As Integer

    'UPGRADE_WARNING: ?? LOGCOLORSPACE ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreateColorSpace Lib "gdi32" Alias "CreateColorSpaceA" (ByRef lplogcolorspace As LOGCOLORSPACE) As Integer

    Public Declare Function SetColorSpace Lib "gdi32" (hdc As Integer, hcolorspace As Integer) As Integer
    Public Declare Function DeleteColorSpace Lib "gdi32" (hcolorspace As Integer) As Integer
    Public Declare Function GetICMProfile Lib "gdi32" Alias "GetICMProfileA" (hdc As Integer, dw As Integer, lpStr As String) As Integer
    Public Declare Function SetICMProfile Lib "gdi32" Alias "SetICMProfileA" (hdc As Integer, lpStr As String) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function GetDeviceGammaRamp Lib "gdi32" (hdc As Integer, ByRef lpv As Object) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function SetDeviceGammaRamp Lib "gdi32" (hdc As Integer, ByRef lpv As Object) As Integer
    Public Declare Function ColorMatchToTarget Lib "gdi32" (hdc As Integer, hdc2 As Integer, dw As Integer) As Integer

    Public Declare Function EnumICMProfiles Lib "gdi32" Alias "EnumICMProfilesA" (hdc As Integer, icmEnumProc As Integer, lParam As Integer) As Integer

    Public Const EMR_SETICMMODE As Short = 98
    Public Const EMR_CREATECOLORSPACE As Short = 99
    Public Const EMR_SETCOLORSPACE As Short = 100
    Public Const EMR_DELETECOLORSPACE As Short = 101

    Public Structure EMRSELECTCOLORSPACE
        Dim pEmr As emr
        Dim ihCS As Integer '  ColorSpace handle index
    End Structure

    Public Structure EMRCREATECOLORSPACE
        Dim pEmr As emr
        Dim ihCS As Integer '  ColorSpace handle index
        Dim lcs As LOGCOLORSPACE
    End Structure


    ' --------------
    '  USER Section
    ' --------------

    ' Scroll Bar Constants
    Public Const SB_HORZ As Short = 0
    Public Const SB_VERT As Short = 1
    Public Const SB_CTL As Short = 2
    Public Const SB_BOTH As Short = 3

    ' Scroll Bar Commands
    Public Const SB_LINEUP As Short = 0
    Public Const SB_LINELEFT As Short = 0
    Public Const SB_LINEDOWN As Short = 1
    Public Const SB_LINERIGHT As Short = 1
    Public Const SB_PAGEUP As Short = 2
    Public Const SB_PAGELEFT As Short = 2
    Public Const SB_PAGEDOWN As Short = 3
    Public Const SB_PAGERIGHT As Short = 3
    Public Const SB_THUMBPOSITION As Short = 4
    Public Const SB_THUMBTRACK As Short = 5
    Public Const SB_TOP As Short = 6
    Public Const SB_LEFT As Short = 6
    Public Const SB_BOTTOM As Short = 7
    Public Const SB_RIGHT As Short = 7
    Public Const SB_ENDSCROLL As Short = 8

    ' ShowWindow() Commands
    Public Const SW_HIDE As Short = 0
    Public Const SW_SHOWNORMAL As Short = 1
    Public Const SW_NORMAL As Short = 1
    Public Const SW_SHOWMINIMIZED As Short = 2
    Public Const SW_SHOWMAXIMIZED As Short = 3
    Public Const SW_MAXIMIZE As Short = 3
    Public Const SW_SHOWNOACTIVATE As Short = 4
    Public Const SW_SHOW As Short = 5
    Public Const SW_MINIMIZE As Short = 6
    Public Const SW_SHOWMINNOACTIVE As Short = 7
    Public Const SW_SHOWNA As Short = 8
    Public Const SW_RESTORE As Short = 9
    Public Const SW_SHOWDEFAULT As Short = 10
    Public Const SW_MAX As Short = 10

    ' Old ShowWindow() Commands
    Public Const HIDE_WINDOW As Short = 0
    Public Const SHOW_OPENWINDOW As Short = 1
    Public Const SHOW_ICONWINDOW As Short = 2
    Public Const SHOW_FULLSCREEN As Short = 3
    Public Const SHOW_OPENNOACTIVATE As Short = 4

    ' Identifiers for the WM_SHOWWINDOW message
    Public Const SW_PARENTCLOSING As Short = 1
    Public Const SW_OTHERZOOM As Short = 2
    Public Const SW_PARENTOPENING As Short = 3
    Public Const SW_OTHERUNZOOM As Short = 4

    ' WM_KEYUP/DOWN/CHAR HIWORD(lParam) flags
    Public Const KF_EXTENDED As Short = &H100S
    Public Const KF_DLGMODE As Short = &H800S
    Public Const KF_MENUMODE As Short = &H1000S
    Public Const KF_ALTDOWN As Short = &H2000S
    Public Const KF_REPEAT As Short = &H4000S
    Public Const KF_UP As Short = &H8000S

    ' Virtual Keys, Standard Set
    Public Const VK_LBUTTON As Short = &H1S
    Public Const VK_RBUTTON As Short = &H2S
    Public Const VK_CANCEL As Short = &H3S
    Public Const VK_MBUTTON As Short = &H4S '  NOT contiguous with L RBUTTON

    Public Const VK_BACK As Short = &H8S
    Public Const VK_TAB As Short = &H9S

    Public Const VK_CLEAR As Short = &HCS
    Public Const VK_RETURN As Short = &HDS

    Public Const VK_SHIFT As Short = &H10S
    Public Const VK_CONTROL As Short = &H11S
    Public Const VK_MENU As Short = &H12S
    Public Const VK_PAUSE As Short = &H13S
    Public Const VK_CAPITAL As Short = &H14S

    Public Const VK_ESCAPE As Short = &H1BS

    Public Const VK_SPACE As Short = &H20S
    Public Const VK_PRIOR As Short = &H21S
    Public Const VK_NEXT As Short = &H22S
    Public Const VK_END As Short = &H23S
    Public Const VK_HOME As Short = &H24S
    Public Const VK_LEFT As Short = &H25S
    Public Const VK_UP As Short = &H26S
    Public Const VK_RIGHT As Short = &H27S
    Public Const VK_DOWN As Short = &H28S
    Public Const VK_SELECT As Short = &H29S
    Public Const VK_PRINT As Short = &H2AS
    Public Const VK_EXECUTE As Short = &H2BS
    Public Const VK_SNAPSHOT As Short = &H2CS
    Public Const VK_INSERT As Short = &H2DS
    Public Const VK_DELETE As Short = &H2ES
    Public Const VK_HELP As Short = &H2FS

    ' VK_A thru VK_Z are the same as their ASCII equivalents: 'A' thru 'Z'
    ' VK_0 thru VK_9 are the same as their ASCII equivalents: '0' thru '9'

    Public Const VK_NUMPAD0 As Short = &H60S
    Public Const VK_NUMPAD1 As Short = &H61S
    Public Const VK_NUMPAD2 As Short = &H62S
    Public Const VK_NUMPAD3 As Short = &H63S
    Public Const VK_NUMPAD4 As Short = &H64S
    Public Const VK_NUMPAD5 As Short = &H65S
    Public Const VK_NUMPAD6 As Short = &H66S
    Public Const VK_NUMPAD7 As Short = &H67S
    Public Const VK_NUMPAD8 As Short = &H68S
    Public Const VK_NUMPAD9 As Short = &H69S
    Public Const VK_MULTIPLY As Short = &H6AS
    Public Const VK_ADD As Short = &H6BS
    Public Const VK_SEPARATOR As Short = &H6CS
    Public Const VK_SUBTRACT As Short = &H6DS
    Public Const VK_DECIMAL As Short = &H6ES
    Public Const VK_DIVIDE As Short = &H6FS
    Public Const VK_F1 As Short = &H70S
    Public Const VK_F2 As Short = &H71S
    Public Const VK_F3 As Short = &H72S
    Public Const VK_F4 As Short = &H73S
    Public Const VK_F5 As Short = &H74S
    Public Const VK_F6 As Short = &H75S
    Public Const VK_F7 As Short = &H76S
    Public Const VK_F8 As Short = &H77S
    Public Const VK_F9 As Short = &H78S
    Public Const VK_F10 As Short = &H79S
    Public Const VK_F11 As Short = &H7AS
    Public Const VK_F12 As Short = &H7BS
    Public Const VK_F13 As Short = &H7CS
    Public Const VK_F14 As Short = &H7DS
    Public Const VK_F15 As Short = &H7ES
    Public Const VK_F16 As Short = &H7FS
    Public Const VK_F17 As Short = &H80S
    Public Const VK_F18 As Short = &H81S
    Public Const VK_F19 As Short = &H82S
    Public Const VK_F20 As Short = &H83S
    Public Const VK_F21 As Short = &H84S
    Public Const VK_F22 As Short = &H85S
    Public Const VK_F23 As Short = &H86S
    Public Const VK_F24 As Short = &H87S

    Public Const VK_NUMLOCK As Short = &H90S
    Public Const VK_SCROLL As Short = &H91S

    '
    '   VK_L VK_R - left and right Alt, Ctrl and Shift virtual keys.
    '   Used only as parameters to GetAsyncKeyState() and GetKeyState().
    '   No other API or message will distinguish left and right keys in this way.
    '  /
    Public Const VK_LSHIFT As Short = &HA0S
    Public Const VK_RSHIFT As Short = &HA1S
    Public Const VK_LCONTROL As Short = &HA2S
    Public Const VK_RCONTROL As Short = &HA3S
    Public Const VK_LMENU As Short = &HA4S
    Public Const VK_RMENU As Short = &HA5S

    Public Const VK_ATTN As Short = &HF6S
    Public Const VK_CRSEL As Short = &HF7S
    Public Const VK_EXSEL As Short = &HF8S
    Public Const VK_EREOF As Short = &HF9S
    Public Const VK_PLAY As Short = &HFAS
    Public Const VK_ZOOM As Short = &HFBS
    Public Const VK_NONAME As Short = &HFCS
    Public Const VK_PA1 As Short = &HFDS
    Public Const VK_OEM_CLEAR As Short = &HFES

    ' SetWindowsHook() codes
    Public Const WH_MIN As Short = (-1)
    Public Const WH_MSGFILTER As Short = (-1)
    Public Const WH_JOURNALRECORD As Short = 0
    Public Const WH_JOURNALPLAYBACK As Short = 1
    Public Const WH_KEYBOARD As Short = 2
    Public Const WH_GETMESSAGE As Short = 3
    Public Const WH_CALLWNDPROC As Short = 4
    Public Const WH_CBT As Short = 5
    Public Const WH_SYSMSGFILTER As Short = 6
    Public Const WH_MOUSE As Short = 7
    Public Const WH_HARDWARE As Short = 8
    Public Const WH_DEBUG As Short = 9
    Public Const WH_SHELL As Short = 10
    Public Const WH_FOREGROUNDIDLE As Short = 11
    Public Const WH_MAX As Short = 11

    ' Hook Codes
    Public Const HC_ACTION As Short = 0
    Public Const HC_GETNEXT As Short = 1
    Public Const HC_SKIP As Short = 2
    Public Const HC_NOREMOVE As Short = 3
    Public Const HC_NOREM As Short = HC_NOREMOVE
    Public Const HC_SYSMODALON As Short = 4
    Public Const HC_SYSMODALOFF As Short = 5

    ' CBT Hook Codes
    Public Const HCBT_MOVESIZE As Short = 0
    Public Const HCBT_MINMAX As Short = 1
    Public Const HCBT_QS As Short = 2
    Public Const HCBT_CREATEWND As Short = 3
    Public Const HCBT_DESTROYWND As Short = 4
    Public Const HCBT_ACTIVATE As Short = 5
    Public Const HCBT_CLICKSKIPPED As Short = 6
    Public Const HCBT_KEYSKIPPED As Short = 7
    Public Const HCBT_SYSCOMMAND As Short = 8
    Public Const HCBT_SETFOCUS As Short = 9

    ' HCBT_ACTIVATE Public Structure pointed to by lParam
    Public Structure CBTACTIVATESTRUCT
        Dim fMouse As Integer
        Dim hWndActive As Integer
    End Structure

    ' WH_MSGFILTER Filter Proc Codes
    Public Const MSGF_DIALOGBOX As Short = 0
    Public Const MSGF_MESSAGEBOX As Short = 1
    Public Const MSGF_MENU As Short = 2
    Public Const MSGF_MOVE As Short = 3
    Public Const MSGF_SIZE As Short = 4
    Public Const MSGF_SCROLLBAR As Short = 5
    Public Const MSGF_NEXTWINDOW As Short = 6
    Public Const MSGF_MAINLOOP As Short = 8
    Public Const MSGF_MAX As Short = 8
    Public Const MSGF_USER As Short = 4096

    Public Const HSHELL_WINDOWCREATED As Short = 1
    Public Const HSHELL_WINDOWDESTROYED As Short = 2
    Public Const HSHELL_ACTIVATESHELLWINDOW As Short = 3

    ' Message Public Structure used in Journaling
    Public Structure EVENTMSG
        Dim message As Integer
        Dim paramL As Integer
        Dim paramH As Integer
        Dim time As Integer
        Dim hWnd As Integer
    End Structure

    Public Structure CWPSTRUCT
        Dim lParam As Integer
        Dim wParam As Integer
        Dim message As Integer
        Dim hWnd As Integer
    End Structure

    Public Structure DEBUGHOOKINFO
        Dim hModuleHook As Integer
        Dim Reserved As Integer
        Dim lParam As Integer
        Dim wParam As Integer
        Dim code As Integer
    End Structure


    Public Structure MOUSEHOOKSTRUCT
        Dim pt As POINTAPI
        Dim hWnd As Integer
        Dim wHitTestCode As Integer
        Dim dwExtraInfo As Integer
    End Structure

    ' Keyboard Layout API
    Public Const HKL_PREV As Short = 0
    Public Const HKL_NEXT As Short = 1

    Public Const KLF_ACTIVATE As Short = &H1S
    Public Const KLF_SUBSTITUTE_OK As Short = &H2S
    Public Const KLF_UNLOADPREVIOUS As Short = &H4S
    Public Const KLF_REORDER As Short = &H8S

    ' Size of KeyboardLayoutName (number of characters), including nul terminator
    Public Const KL_NAMELENGTH As Short = 9

    Public Declare Function LoadKeyboardLayout Lib "user32" Alias "LoadKeyboardLayoutA" (pwszKLID As String, Flags As Integer) As Integer
    Public Declare Function ActivateKeyboardLayout Lib "user32" (hkl As Integer, Flags As Integer) As Integer
    Public Declare Function UnloadKeyboardLayout Lib "user32" (hkl As Integer) As Integer
    Public Declare Function GetKeyboardLayoutName Lib "user32" Alias "GetKeyboardLayoutNameA" (pwszKLID As String) As Integer

    ' Desktop-specific access flags
    Public Const DESKTOP_READOBJECTS As Integer = &H1
    Public Const DESKTOP_CREATEWINDOW As Integer = &H2
    Public Const DESKTOP_CREATEMENU As Integer = &H4
    Public Const DESKTOP_HOOKCONTROL As Integer = &H8
    Public Const DESKTOP_JOURNALRECORD As Integer = &H10
    Public Const DESKTOP_JOURNALPLAYBACK As Integer = &H20
    Public Const DESKTOP_ENUMERATE As Integer = &H40
    Public Const DESKTOP_WRITEOBJECTS As Integer = &H80

    Public Declare Function GetThreadDesktop Lib "user32" (dwThread As Integer) As Integer

    ' Windowstation-specific access flags
    Public Const WINSTA_ENUMDESKTOPS As Integer = &H1
    Public Const WINSTA_READATTRIBUTES As Integer = &H2
    Public Const WINSTA_ACCESSCLIPBOARD As Integer = &H4
    Public Const WINSTA_CREATEDESKTOP As Integer = &H8
    Public Const WINSTA_WRITEATTRIBUTES As Integer = &H10
    Public Const WINSTA_ACCESSPUBLICATOMS As Integer = &H20
    Public Const WINSTA_EXITWINDOWS As Integer = &H40
    Public Const WINSTA_ENUMERATE As Integer = &H100
    Public Const WINSTA_READSCREEN As Integer = &H200

    Public Declare Function GetProcessWindowStation Lib "user32" () As Integer
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetUserObjectSecurity Lib "user32" (hObj As Integer, ByRef pSIRequested As Integer, ByRef pSd As SECURITY_DESCRIPTOR) As Integer
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetUserObjectSecurity Lib "user32" (hObj As Integer, ByRef pSIRequested As Integer, ByRef pSd As SECURITY_DESCRIPTOR, nLength As Integer, ByRef lpnLengthNeeded As Integer) As Integer

    ' Message structure

    ' Window field offsets for GetWindowLong() and GetWindowWord()
    Public Const GWL_WNDPROC As Short = (-4)
    Public Const GWL_HINSTANCE As Short = (-6)
    Public Const GWL_HWNDPARENT As Short = (-8)
    Public Const GWL_STYLE As Short = (-16)
    Public Const GWL_EXSTYLE As Short = (-20)
    Public Const GWL_USERDATA As Short = (-21)
    Public Const GWL_ID As Short = (-12)

    ' Class field offsets for GetClassLong() and GetClassWord()
    Public Const GCL_MENUNAME As Short = (-8)
    Public Const GCL_HBRBACKGROUND As Short = (-10)
    Public Const GCL_HCURSOR As Short = (-12)
    Public Const GCL_HICON As Short = (-14)
    Public Const GCL_HMODULE As Short = (-16)
    Public Const GCL_CBWNDEXTRA As Short = (-18)
    Public Const GCL_CBCLSEXTRA As Short = (-20)
    Public Const GCL_WNDPROC As Short = (-24)
    Public Const GCL_STYLE As Short = (-26)
    Public Const GCW_ATOM As Short = (-32)

    ' Window Messages
    Public Const WM_NULL As Short = &H0S
    Public Const WM_CREATE As Short = &H1S
    Public Const WM_DESTROY As Short = &H2S
    Public Const WM_MOVE As Short = &H3S
    Public Const WM_SIZE As Short = &H5S

    Public Const WM_ACTIVATE As Short = &H6S
    '
    '  WM_ACTIVATE state values

    Public Const WA_INACTIVE As Short = 0
    Public Const WA_ACTIVE As Short = 1
    Public Const WA_CLICKACTIVE As Short = 2

    Public Const WM_SETFOCUS As Short = &H7S
    Public Const WM_KILLFOCUS As Short = &H8S
    Public Const WM_ENABLE As Short = &HAS
    Public Const WM_SETREDRAW As Short = &HBS
    Public Const WM_SETTEXT As Short = &HCS
    Public Const WM_GETTEXT As Short = &HDS
    Public Const WM_GETTEXTLENGTH As Short = &HES
    Public Const WM_PAINT As Short = &HFS
    Public Const WM_CLOSE As Short = &H10S
    Public Const WM_QUERYENDSESSION As Short = &H11S
    Public Const WM_QUIT As Short = &H12S
    Public Const WM_QUERYOPEN As Short = &H13S
    Public Const WM_ERASEBKGND As Short = &H14S
    Public Const WM_SYSCOLORCHANGE As Short = &H15S
    Public Const WM_ENDSESSION As Short = &H16S
    Public Const WM_SHOWWINDOW As Short = &H18S
    Public Const WM_WININICHANGE As Short = &H1AS
    Public Const WM_DEVMODECHANGE As Short = &H1BS
    Public Const WM_ACTIVATEAPP As Short = &H1CS
    Public Const WM_FONTCHANGE As Short = &H1DS
    Public Const WM_TIMECHANGE As Short = &H1ES
    Public Const WM_CANCELMODE As Short = &H1FS
    Public Const WM_SETCURSOR As Short = &H20S
    Public Const WM_MOUSEACTIVATE As Short = &H21S
    Public Const WM_CHILDACTIVATE As Short = &H22S
    Public Const WM_QUEUESYNC As Short = &H23S

    Public Const WM_GETMINMAXINFO As Short = &H24S

    Public Structure MINMAXINFO
        Dim ptReserved As POINTAPI
        Dim ptMaxSize As POINTAPI
        Dim ptMaxPosition As POINTAPI
        Dim ptMinTrackSize As POINTAPI
        Dim ptMaxTrackSize As POINTAPI
    End Structure

    Public Const WM_PAINTICON As Short = &H26S
    Public Const WM_ICONERASEBKGND As Short = &H27S
    Public Const WM_NEXTDLGCTL As Short = &H28S
    Public Const WM_SPOOLERSTATUS As Short = &H2AS
    Public Const WM_DRAWITEM As Short = &H2BS
    Public Const WM_MEASUREITEM As Short = &H2CS
    Public Const WM_DELETEITEM As Short = &H2DS
    Public Const WM_VKEYTOITEM As Short = &H2ES
    Public Const WM_CHARTOITEM As Short = &H2FS
    Public Const WM_SETFONT As Short = &H30S
    Public Const WM_GETFONT As Short = &H31S
    Public Const WM_SETHOTKEY As Short = &H32S
    Public Const WM_GETHOTKEY As Short = &H33S
    Public Const WM_QUERYDRAGICON As Short = &H37S
    Public Const WM_COMPAREITEM As Short = &H39S
    Public Const WM_COMPACTING As Short = &H41S
    Public Const WM_OTHERWINDOWCREATED As Short = &H42S '  no longer suported
    Public Const WM_OTHERWINDOWDESTROYED As Short = &H43S '  no longer suported
    Public Const WM_COMMNOTIFY As Short = &H44S '  no longer suported

    ' notifications passed in low word of lParam on WM_COMMNOTIFY messages
    Public Const CN_RECEIVE As Short = &H1S
    Public Const CN_TRANSMIT As Short = &H2S
    Public Const CN_EVENT As Short = &H4S

    Public Const WM_WINDOWPOSCHANGING As Short = &H46S
    Public Const WM_WINDOWPOSCHANGED As Short = &H47S

    Public Const WM_POWER As Short = &H48S
    '
    '  wParam for WM_POWER window message and DRV_POWER driver notification

    Public Const PWR_OK As Short = 1
    Public Const PWR_FAIL As Short = (-1)
    Public Const PWR_SUSPENDREQUEST As Short = 1
    Public Const PWR_SUSPENDRESUME As Short = 2
    Public Const PWR_CRITICALRESUME As Short = 3

    Public Const WM_COPYDATA As Short = &H4AS
    Public Const WM_CANCELJOURNAL As Short = &H4BS

    Public Structure COPYDATASTRUCT
        Dim dwData As Integer
        Dim cbData As Integer
        Dim lpData As Integer
    End Structure

    Public Const WM_NCCREATE As Short = &H81S
    Public Const WM_NCDESTROY As Short = &H82S
    Public Const WM_NCCALCSIZE As Short = &H83S
    Public Const WM_NCHITTEST As Short = &H84S
    Public Const WM_NCPAINT As Short = &H85S
    Public Const WM_NCACTIVATE As Short = &H86S
    Public Const WM_GETDLGCODE As Short = &H87S
    Public Const WM_NCMOUSEMOVE As Short = &HA0S
    Public Const WM_NCLBUTTONDOWN As Short = &HA1S
    Public Const WM_NCLBUTTONUP As Short = &HA2S
    Public Const WM_NCLBUTTONDBLCLK As Short = &HA3S
    Public Const WM_NCRBUTTONDOWN As Short = &HA4S
    Public Const WM_NCRBUTTONUP As Short = &HA5S
    Public Const WM_NCRBUTTONDBLCLK As Short = &HA6S
    Public Const WM_NCMBUTTONDOWN As Short = &HA7S
    Public Const WM_NCMBUTTONUP As Short = &HA8S
    Public Const WM_NCMBUTTONDBLCLK As Short = &HA9S

    Public Const WM_KEYFIRST As Short = &H100S
    Public Const WM_KEYDOWN As Short = &H100S
    Public Const WM_KEYUP As Short = &H101S
    Public Const WM_CHAR As Short = &H102S
    Public Const WM_DEADCHAR As Short = &H103S
    Public Const WM_SYSKEYDOWN As Short = &H104S
    Public Const WM_SYSKEYUP As Short = &H105S
    Public Const WM_SYSCHAR As Short = &H106S
    Public Const WM_SYSDEADCHAR As Short = &H107S
    Public Const WM_KEYLAST As Short = &H108S
    Public Const WM_INITDIALOG As Short = &H110S
    Public Const WM_COMMAND As Short = &H111S
    Public Const WM_SYSCOMMAND As Short = &H112S
    Public Const WM_TIMER As Short = &H113S
    Public Const WM_HSCROLL As Short = &H114S
    Public Const WM_VSCROLL As Short = &H115S
    Public Const WM_INITMENU As Short = &H116S
    Public Const WM_INITMENUPOPUP As Short = &H117S
    Public Const WM_MENUSELECT As Short = &H11FS
    Public Const WM_MENUCHAR As Short = &H120S
    Public Const WM_ENTERIDLE As Short = &H121S

    Public Const WM_CTLCOLORMSGBOX As Short = &H132S
    Public Const WM_CTLCOLOREDIT As Short = &H133S
    Public Const WM_CTLCOLORLISTBOX As Short = &H134S
    Public Const WM_CTLCOLORBTN As Short = &H135S
    Public Const WM_CTLCOLORDLG As Short = &H136S
    Public Const WM_CTLCOLORSCROLLBAR As Short = &H137S
    Public Const WM_CTLCOLORSTATIC As Short = &H138S

    Public Const WM_MOUSEFIRST As Short = &H200S
    Public Const WM_MOUSEMOVE As Short = &H200S
    Public Const WM_LBUTTONDOWN As Short = &H201S
    Public Const WM_LBUTTONUP As Short = &H202S
    Public Const WM_LBUTTONDBLCLK As Short = &H203S
    Public Const WM_RBUTTONDOWN As Short = &H204S
    Public Const WM_RBUTTONUP As Short = &H205S
    Public Const WM_RBUTTONDBLCLK As Short = &H206S
    Public Const WM_MBUTTONDOWN As Short = &H207S
    Public Const WM_MBUTTONUP As Short = &H208S
    Public Const WM_MBUTTONDBLCLK As Short = &H209S
    Public Const WM_MOUSELAST As Short = &H209S

    Public Const WM_PARENTNOTIFY As Short = &H210S
    Public Const WM_ENTERMENULOOP As Short = &H211S
    Public Const WM_EXITMENULOOP As Short = &H212S
    Public Const WM_MDICREATE As Short = &H220S
    Public Const WM_MDIDESTROY As Short = &H221S
    Public Const WM_MDIACTIVATE As Short = &H222S
    Public Const WM_MDIRESTORE As Short = &H223S
    Public Const WM_MDINEXT As Short = &H224S
    Public Const WM_MDIMAXIMIZE As Short = &H225S
    Public Const WM_MDITILE As Short = &H226S
    Public Const WM_MDICASCADE As Short = &H227S
    Public Const WM_MDIICONARRANGE As Short = &H228S
    Public Const WM_MDIGETACTIVE As Short = &H229S
    Public Const WM_MDISETMENU As Short = &H230S
    Public Const WM_DROPFILES As Short = &H233S
    Public Const WM_MDIREFRESHMENU As Short = &H234S


    Public Const WM_CUT As Short = &H300S
    Public Const WM_COPY As Short = &H301S
    Public Const WM_PASTE As Short = &H302S
    Public Const WM_CLEAR As Short = &H303S
    Public Const WM_UNDO As Short = &H304S
    Public Const WM_RENDERFORMAT As Short = &H305S
    Public Const WM_RENDERALLFORMATS As Short = &H306S
    Public Const WM_DESTROYCLIPBOARD As Short = &H307S
    Public Const WM_DRAWCLIPBOARD As Short = &H308S
    Public Const WM_PAINTCLIPBOARD As Short = &H309S
    Public Const WM_VSCROLLCLIPBOARD As Short = &H30AS
    Public Const WM_SIZECLIPBOARD As Short = &H30BS
    Public Const WM_ASKCBFORMATNAME As Short = &H30CS
    Public Const WM_CHANGECBCHAIN As Short = &H30DS
    Public Const WM_HSCROLLCLIPBOARD As Short = &H30ES
    Public Const WM_QUERYNEWPALETTE As Short = &H30FS
    Public Const WM_PALETTEISCHANGING As Short = &H310S
    Public Const WM_PALETTECHANGED As Short = &H311S
    Public Const WM_HOTKEY As Short = &H312S

    Public Const WM_PENWINFIRST As Short = &H380S
    Public Const WM_PENWINLAST As Short = &H38FS

    ' NOTE: All Message Numbers below 0x0400 are RESERVED.

    ' Private Window Messages Start Here:
    Public Const WM_USER As Short = &H400S

    ' WM_SYNCTASK Commands
    Public Const ST_BEGINSWP As Short = 0
    Public Const ST_ENDSWP As Short = 1

    ' WM_NCHITTEST and MOUSEHOOKSTRUCT Mouse Position Codes
    Public Const HTERROR As Short = (-2)
    Public Const HTTRANSPARENT As Short = (-1)
    Public Const HTNOWHERE As Short = 0
    Public Const HTCLIENT As Short = 1
    Public Const HTCAPTION As Short = 2
    Public Const HTSYSMENU As Short = 3
    Public Const HTGROWBOX As Short = 4
    Public Const HTSIZE As Short = HTGROWBOX
    Public Const HTMENU As Short = 5
    Public Const HTHSCROLL As Short = 6
    Public Const HTVSCROLL As Short = 7
    Public Const HTMINBUTTON As Short = 8
    Public Const HTMAXBUTTON As Short = 9
    Public Const HTLEFT As Short = 10
    Public Const HTRIGHT As Short = 11
    Public Const HTTOP As Short = 12
    Public Const HTTOPLEFT As Short = 13
    Public Const HTTOPRIGHT As Short = 14
    Public Const HTBOTTOM As Short = 15
    Public Const HTBOTTOMLEFT As Short = 16
    Public Const HTBOTTOMRIGHT As Short = 17
    Public Const HTBORDER As Short = 18
    Public Const HTREDUCE As Short = HTMINBUTTON
    Public Const HTZOOM As Short = HTMAXBUTTON
    Public Const HTSIZEFIRST As Short = HTLEFT
    Public Const HTSIZELAST As Short = HTBOTTOMRIGHT

    '  SendMessageTimeout values
    Public Const SMTO_NORMAL As Short = &H0S
    Public Const SMTO_BLOCK As Short = &H1S
    Public Const SMTO_ABORTIFHUNG As Short = &H2S

    ' WM_MOUSEACTIVATE Return Codes
    Public Const MA_ACTIVATE As Short = 1
    Public Const MA_ACTIVATEANDEAT As Short = 2
    Public Const MA_NOACTIVATE As Short = 3
    Public Const MA_NOACTIVATEANDEAT As Short = 4

    Public Declare Function RegisterWindowMessage Lib "user32" Alias "RegisterWindowMessageA" (lpString As String) As Integer

    ' WM_SIZE message wParam values
    Public Const SIZE_RESTORED As Short = 0
    Public Const SIZE_MINIMIZED As Short = 1
    Public Const SIZE_MAXIMIZED As Short = 2
    Public Const SIZE_MAXSHOW As Short = 3
    Public Const SIZE_MAXHIDE As Short = 4

    ' Obsolete constant names
    Public Const SIZENORMAL As Short = SIZE_RESTORED
    Public Const SIZEICONIC As Short = SIZE_MINIMIZED
    Public Const SIZEFULLSCREEN As Short = SIZE_MAXIMIZED
    Public Const SIZEZOOMSHOW As Short = SIZE_MAXSHOW
    Public Const SIZEZOOMHIDE As Short = SIZE_MAXHIDE

    ' WM_WINDOWPOSCHANGING/CHANGED struct pointed to by lParam
    Public Structure WINDOWPOS
        Dim hWnd As Integer
        Dim hWndInsertAfter As Integer
        Dim X As Integer
        Dim Y As Integer
        Dim cx As Integer
        Dim cy As Integer
        Dim Flags As Integer
    End Structure

    ' WM_NCCALCSIZE return flags
    Public Const WVR_ALIGNTOP As Short = &H10S
    Public Const WVR_ALIGNLEFT As Short = &H20S
    Public Const WVR_ALIGNBOTTOM As Short = &H40S
    Public Const WVR_ALIGNRIGHT As Short = &H80S
    Public Const WVR_HREDRAW As Short = &H100S
    Public Const WVR_VREDRAW As Short = &H200S
    Public Const WVR_REDRAW As Boolean = (WVR_HREDRAW Or WVR_VREDRAW)
    Public Const WVR_VALIDRECTS As Short = &H400S

    ' Key State Masks for Mouse Messages
    Public Const MK_LBUTTON As Short = &H1S
    Public Const MK_RBUTTON As Short = &H2S
    Public Const MK_SHIFT As Short = &H4S
    Public Const MK_CONTROL As Short = &H8S
    Public Const MK_MBUTTON As Short = &H10S

    ' Window Styles
    Public Const WS_OVERLAPPED As Integer = &H0
    Public Const WS_POPUP As Integer = &H80000000
    Public Const WS_CHILD As Integer = &H40000000
    Public Const WS_MINIMIZE As Integer = &H20000000
    Public Const WS_VISIBLE As Integer = &H10000000
    Public Const WS_DISABLED As Integer = &H8000000
    Public Const WS_CLIPSIBLINGS As Integer = &H4000000
    Public Const WS_CLIPCHILDREN As Integer = &H2000000
    Public Const WS_MAXIMIZE As Integer = &H1000000
    Public Const WS_CAPTION As Integer = &HC00000 '  WS_BORDER Or WS_DLGFRAME
    Public Const WS_BORDER As Integer = &H800000
    Public Const WS_DLGFRAME As Integer = &H400000
    Public Const WS_VSCROLL As Integer = &H200000
    Public Const WS_HSCROLL As Integer = &H100000
    Public Const WS_SYSMENU As Integer = &H80000
    Public Const WS_THICKFRAME As Integer = &H40000
    Public Const WS_GROUP As Integer = &H20000
    Public Const WS_TABSTOP As Integer = &H10000

    Public Const WS_MINIMIZEBOX As Integer = &H20000
    Public Const WS_MAXIMIZEBOX As Integer = &H10000

    Public Const WS_TILED As Integer = WS_OVERLAPPED
    Public Const WS_ICONIC As Integer = WS_MINIMIZE
    Public Const WS_SIZEBOX As Integer = WS_THICKFRAME
    Public Const WS_OVERLAPPEDWINDOW As Boolean = (WS_OVERLAPPED Or WS_CAPTION Or WS_SYSMENU Or WS_THICKFRAME Or WS_MINIMIZEBOX Or WS_MAXIMIZEBOX)
    Public Const WS_TILEDWINDOW As Boolean = WS_OVERLAPPEDWINDOW

    '
    '   Common Window Styles
    '  /


    Public Const WS_POPUPWINDOW As Boolean = (WS_POPUP Or WS_BORDER Or WS_SYSMENU)

    Public Const WS_CHILDWINDOW As Integer = (WS_CHILD)

    ' Extended Window Styles
    Public Const WS_EX_DLGMODALFRAME As Integer = &H1
    Public Const WS_EX_NOPARENTNOTIFY As Integer = &H4
    Public Const WS_EX_TOPMOST As Integer = &H8
    Public Const WS_EX_ACCEPTFILES As Integer = &H10
    Public Const WS_EX_TRANSPARENT As Integer = &H20

    ' Class styles
    Public Const CS_VREDRAW As Short = &H1S
    Public Const CS_HREDRAW As Short = &H2S
    Public Const CS_KEYCVTWINDOW As Short = &H4S
    Public Const CS_DBLCLKS As Short = &H8S
    Public Const CS_OWNDC As Short = &H20S
    Public Const CS_CLASSDC As Short = &H40S
    Public Const CS_PARENTDC As Short = &H80S
    Public Const CS_NOKEYCVT As Short = &H100S
    Public Const CS_NOCLOSE As Short = &H200S
    Public Const CS_SAVEBITS As Short = &H800S
    Public Const CS_BYTEALIGNCLIENT As Short = &H1000S
    Public Const CS_BYTEALIGNWINDOW As Short = &H2000S
    Public Const CS_PUBLICCLASS As Short = &H4000S

    ' Predefined Clipboard Formats
    Public Const CF_TEXT As Short = 1
    Public Const CF_BITMAP As Short = 2
    Public Const CF_METAFILEPICT As Short = 3
    Public Const CF_SYLK As Short = 4
    Public Const CF_DIF As Short = 5
    Public Const CF_TIFF As Short = 6
    Public Const CF_OEMTEXT As Short = 7
    Public Const CF_DIB As Short = 8
    Public Const CF_PALETTE As Short = 9
    Public Const CF_PENDATA As Short = 10
    Public Const CF_RIFF As Short = 11
    Public Const CF_WAVE As Short = 12
    Public Const CF_UNICODETEXT As Short = 13
    Public Const CF_ENHMETAFILE As Short = 14

    Public Const CF_OWNERDISPLAY As Short = &H80S
    Public Const CF_DSPTEXT As Short = &H81S
    Public Const CF_DSPBITMAP As Short = &H82S
    Public Const CF_DSPMETAFILEPICT As Short = &H83S
    Public Const CF_DSPENHMETAFILE As Short = &H8ES

    ' "Private" formats don't get GlobalFree()'d
    Public Const CF_PRIVATEFIRST As Short = &H200S
    Public Const CF_PRIVATELAST As Short = &H2FFS

    ' "GDIOBJ" formats do get DeleteObject()'d
    Public Const CF_GDIOBJFIRST As Short = &H300S
    Public Const CF_GDIOBJLAST As Short = &H3FFS

    '  Defines for the fVirt field of the Accelerator table structure.
    Public Const FVIRTKEY As Boolean = True '  Assumed to be == TRUE
    Public Const FNOINVERT As Short = &H2S
    Public Const FSHIFT As Short = &H4S
    Public Const FCONTROL As Short = &H8S
    Public Const FALT As Short = &H10S

    Public Structure ACCEL
        Dim fVirt As Byte
        Dim key As Short
        Dim cmd As Short
    End Structure

    Public Structure PAINTSTRUCT
        Dim hdc As Integer
        Dim fErase As Integer
        Dim rcPaint As RECT
        Dim fRestore As Integer
        Dim fIncUpdate As Integer
        Dim rgbReserved As Byte
    End Structure

    Public Structure CREATESTRUCT
        Dim lpCreateParams As Integer
        Dim hInstance As Integer
        Dim hMenu As Integer
        Dim hwndParent As Integer
        Dim cy As Integer
        Dim cx As Integer
        Dim Y As Integer
        Dim X As Integer
        Dim style As Integer
        Dim lpszName As String
        Dim lpszClass As String
        Dim ExStyle As Integer
    End Structure

    ' HCBT_CREATEWND parameters pointed to by lParam
    Public Structure CBT_CREATEWND
        Dim lpcs As CREATESTRUCT
        Dim hWndInsertAfter As Integer
    End Structure

    Public Structure WINDOWPLACEMENT
        Dim Length As Integer
        Dim Flags As Integer
        Dim showCmd As Integer
        Dim ptMinPosition As POINTAPI
        Dim ptMaxPosition As POINTAPI
        Dim rcNormalPosition As RECT
    End Structure

    Public Const WPF_SETMINPOSITION As Short = &H1S
    Public Const WPF_RESTORETOMAXIMIZED As Short = &H2S

    ' Owner draw control types
    Public Const ODT_MENU As Short = 1
    Public Const ODT_LISTBOX As Short = 2
    Public Const ODT_COMBOBOX As Short = 3
    Public Const ODT_BUTTON As Short = 4

    ' Owner draw actions
    Public Const ODA_DRAWENTIRE As Short = &H1S
    Public Const ODA_SELECT As Short = &H2S
    Public Const ODA_FOCUS As Short = &H4S

    ' Owner draw state
    Public Const ODS_SELECTED As Short = &H1S
    Public Const ODS_GRAYED As Short = &H2S
    Public Const ODS_DISABLED As Short = &H4S
    Public Const ODS_CHECKED As Short = &H8S
    Public Const ODS_FOCUS As Short = &H10S

    ' MEASUREITEMSTRUCT for ownerdraw
    Public Structure MEASUREITEMSTRUCT
        Dim CtlType As Integer
        Dim CtlID As Integer
        Dim itemID As Integer
        Dim itemWidth As Integer
        Dim itemHeight As Integer
        Dim itemData As Integer
    End Structure

    ' DRAWITEMSTRUCT for ownerdraw
    Public Structure DRAWITEMSTRUCT
        Dim CtlType As Integer
        Dim CtlID As Integer
        Dim itemID As Integer
        Dim itemAction As Integer
        Dim itemState As Integer
        Dim hwndItem As Integer
        Dim hdc As Integer
        Dim rcItem As RECT
        Dim itemData As Integer
    End Structure

    ' DELETEITEMSTRUCT for ownerdraw
    Public Structure DELETEITEMSTRUCT
        Dim CtlType As Integer
        Dim CtlID As Integer
        Dim itemID As Integer
        Dim hwndItem As Integer
        Dim itemData As Integer
    End Structure

    ' COMPAREITEMSTRUCT for ownerdraw sorting
    Public Structure COMPAREITEMSTRUCT
        Dim CtlType As Integer
        Dim CtlID As Integer
        Dim hwndItem As Integer
        Dim itemID1 As Integer
        Dim itemData1 As Integer
        Dim itemID2 As Integer
        Dim itemData2 As Integer
    End Structure

    ' Message Function Templates
    'UPGRADE_WARNING: ?? Msg ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetMessage Lib "user32" Alias "GetMessageA" (ByRef lpMsg As Msg, hWnd As Integer, wMsgFilterMin As Integer, wMsgFilterMax As Integer) As Integer
    'UPGRADE_WARNING: ?? Msg ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function TranslateMessage Lib "user32" (ByRef lpMsg As Msg) As Integer
    'UPGRADE_WARNING: ?? Msg ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function DispatchMessage Lib "user32" Alias "DispatchMessageA" (ByRef lpMsg As Msg) As Integer
    'UPGRADE_WARNING: ?? Msg ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function PeekMessage Lib "user32" Alias "PeekMessageA" (ByRef lpMsg As Msg, hWnd As Integer, wMsgFilterMin As Integer, wMsgFilterMax As Integer, wRemoveMsg As Integer) As Integer

    ' PeekMessage() Options
    Public Const PM_NOREMOVE As Short = &H0S
    Public Const PM_REMOVE As Short = &H1S
    Public Const PM_NOYIELD As Short = &H2S

    Public Declare Function RegisterHotKey Lib "user32" (hWnd As Integer, id As Integer, fsModifiers As Integer, vk As Integer) As Integer
    Public Declare Function UnregisterHotKey Lib "user32" (hWnd As Integer, id As Integer) As Integer

    Public Const MOD_ALT As Short = &H1S
    Public Const MOD_CONTROL As Short = &H2S
    Public Const MOD_SHIFT As Short = &H4S

    Public Const IDHOT_SNAPWINDOW As Short = (-1) '  SHIFT-PRINTSCRN
    Public Const IDHOT_SNAPDESKTOP As Short = (-2) '  PRINTSCRN

    Public Const EWX_LOGOFF As Short = 0
    Public Const EWX_SHUTDOWN As Short = 1
    Public Const EWX_REBOOT As Short = 2
    Public Const EWX_FORCE As Short = 4

    Public Const READAPI As Short = 0 '  Flags for _lopen
    Public Const WRITEAPI As Short = 1
    Public Const READ_WRITE As Short = 2

    Public Declare Function ExitWindows Lib "user32" (dwReserved As Integer, uReturnCode As Integer) As Integer
    Public Declare Function ExitWindowsEx Lib "user32" (uFlags As Integer, dwReserved As Integer) As Integer

    Public Declare Function SwapMouseButton Lib "user32" (bSwap As Integer) As Integer
    Public Declare Function GetMessagePos Lib "user32" () As Integer
    Public Declare Function GetMessageTime Lib "user32" () As Integer
    Public Declare Function GetMessageExtraInfo Lib "user32" () As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function SendMessage Lib "user32" Alias "SendMessageA" (hWnd As Integer, wMsg As Integer, wParam As Integer, ByRef lParam As Object) As Integer
    Public Declare Function SendMessageTimeout Lib "user32" Alias "SendMessageTimeoutA" (hWnd As Integer, Msg As Integer, wParam As Integer, lParam As Integer, fuFlags As Integer, uTimeout As Integer, ByRef lpdwResult As Integer) As Integer
    Public Declare Function SendNotifyMessage Lib "user32" Alias "SendNotifyMessageA" (hWnd As Integer, Msg As Integer, wParam As Integer, lParam As Integer) As Integer
    Public Declare Function SendMessageCallback Lib "user32" Alias "SendMessageCallbackA" (hWnd As Integer, Msg As Integer, wParam As Integer, lParam As Integer, lpResultCallBack As Integer, dwData As Integer) As Integer
    Public Declare Function PostMessage Lib "user32" Alias "PostMessageA" (hWnd As Integer, wMsg As Integer, wParam As Integer, lParam As Integer) As Integer
    Public Declare Function PostThreadMessage Lib "user32" Alias "PostThreadMessageA" (idThread As Integer, Msg As Integer, wParam As Integer, lParam As Integer) As Integer

    ' Special HWND value for use with PostMessage and SendMessage
    Public Const HWND_BROADCAST As Integer = &HFFFF


    Public Structure WNDCLASS
        Dim style As Integer
        Dim lpfnWndProc As Integer
        Dim cbClsExtra As Integer
        Dim cbWndExtra2 As Integer
        Dim hInstance As Integer
        Dim hIcon As Integer
        Dim hCursor As Integer
        Dim hbrBackground As Integer
        Dim lpszMenuName As String
        Dim lpszClassName As String
    End Structure

    Public Declare Function AttachThreadInput Lib "user32" (idAttach As Integer, idAttachTo As Integer, fAttach As Integer) As Integer
    Public Declare Function ReplyMessage Lib "user32" (lReply As Integer) As Integer
    Public Declare Function WaitMessage Lib "user32" () As Integer
    Public Declare Function WaitForInputIdle Lib "user32" (hProcess As Integer, dwMilliseconds As Integer) As Integer
    Public Declare Function DefWindowProc Lib "user32" Alias "DefWindowProcA" (hWnd As Integer, wMsg As Integer, wParam As Integer, lParam As Integer) As Integer
    Public Declare Sub PostQuitMessage Lib "user32" (nExitCode As Integer)
    Public Declare Function InSendMessage Lib "user32" () As Integer

    Public Declare Function GetDoubleClickTime Lib "user32" () As Integer
    Public Declare Function SetDoubleClickTime Lib "user32" (wCount As Integer) As Integer
    'UPGRADE_NOTE: Class ???? Class_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
    'UPGRADE_WARNING: ?? WNDCLASS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function RegisterClass Lib "user32" (ByRef Class_Renamed As WNDCLASS) As Integer
    Public Declare Function UnregisterClass Lib "user32" Alias "UnregisterClassA" (lpClassName As String, hInstance As Integer) As Integer
    'UPGRADE_WARNING: ?? WNDCLASS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetClassInfo Lib "user32" Alias "GetClassInfoA" (hInstance As Integer, lpClassName As String, ByRef lpWndClass As WNDCLASS) As Integer

    Public Const CW_USEDEFAULT As Integer = &H80000000
    Public Const HWND_DESKTOP As Short = 0

    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function CreateWindowEx Lib "user32" Alias "CreateWindowExA" (dwExStyle As Integer, lpClassName As String, lpWindowName As String, dwStyle As Integer, X As Integer, Y As Integer, nWidth As Integer, nHeight As Integer, hwndParent As Integer, hMenu As Integer, hInstance As Integer, ByRef lpParam As Object) As Integer
    Public Declare Function IsWindow Lib "user32" (hWnd As Integer) As Integer
    Public Declare Function IsMenu Lib "user32" (hMenu As Integer) As Integer
    Public Declare Function IsChild Lib "user32" (hwndParent As Integer, hWnd As Integer) As Integer
    Public Declare Function DestroyWindow Lib "user32" (hWnd As Integer) As Integer

    Public Declare Function ShowWindow Lib "user32" (hWnd As Integer, nCmdShow As Integer) As Integer
    Public Declare Function FlashWindow Lib "user32" (hWnd As Integer, bInvert As Integer) As Integer
    Public Declare Function ShowOwnedPopups Lib "user32" (hWnd As Integer, fShow As Integer) As Integer

    Public Declare Function OpenIcon Lib "user32" (hWnd As Integer) As Integer
    Public Declare Function CloseWindow Lib "user32" (hWnd As Integer) As Integer
    Public Declare Function MoveWindow Lib "user32" (hWnd As Integer, X As Integer, Y As Integer, nWidth As Integer, nHeight As Integer, bRepaint As Integer) As Integer
    Public Declare Function SetWindowPos Lib "user32" (hWnd As Integer, hWndInsertAfter As Integer, X As Integer, Y As Integer, cx As Integer, cy As Integer, wFlags As Integer) As Integer
    'UPGRADE_WARNING: ?? WINDOWPLACEMENT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetWindowPlacement Lib "user32" (hWnd As Integer, ByRef lpwndpl As WINDOWPLACEMENT) As Integer
    'UPGRADE_WARNING: ?? WINDOWPLACEMENT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetWindowPlacement Lib "user32" (hWnd As Integer, ByRef lpwndpl As WINDOWPLACEMENT) As Integer

    Public Declare Function BeginDeferWindowPos Lib "user32" (nNumWindows As Integer) As Integer
    Public Declare Function DeferWindowPos Lib "user32" (hWinPosInfo As Integer, hWnd As Integer, hWndInsertAfter As Integer, X As Integer, Y As Integer, cx As Integer, cy As Integer, wFlags As Integer) As Integer
    Public Declare Function EndDeferWindowPos Lib "user32" (hWinPosInfo As Integer) As Integer

    Public Declare Function IsWindowVisible Lib "user32" (hWnd As Integer) As Integer
    Public Declare Function IsIconic Lib "user32" (hWnd As Integer) As Integer
    Public Declare Function AnyPopup Lib "user32" () As Integer
    Public Declare Function BringWindowToTop Lib "user32" (hWnd As Integer) As Integer
    Public Declare Function IsZoomed Lib "user32" (hWnd As Integer) As Integer

    ' SetWindowPos Flags
    Public Const SWP_NOSIZE As Short = &H1S
    Public Const SWP_NOMOVE As Short = &H2S
    Public Const SWP_NOZORDER As Short = &H4S
    Public Const SWP_NOREDRAW As Short = &H8S
    Public Const SWP_NOACTIVATE As Short = &H10S
    Public Const SWP_FRAMECHANGED As Short = &H20S '  The frame changed: send WM_NCCALCSIZE
    Public Const SWP_SHOWWINDOW As Short = &H40S
    Public Const SWP_HIDEWINDOW As Short = &H80S
    Public Const SWP_NOCOPYBITS As Short = &H100S
    Public Const SWP_NOOWNERZORDER As Short = &H200S '  Don't do owner Z ordering

    Public Const SWP_DRAWFRAME As Short = SWP_FRAMECHANGED
    Public Const SWP_NOREPOSITION As Short = SWP_NOOWNERZORDER

    ' SetWindowPos() hwndInsertAfter values
    Public Const HWND_TOP As Short = 0
    Public Const HWND_BOTTOM As Short = 1
    Public Const HWND_TOPMOST As Short = -1
    Public Const HWND_NOTOPMOST As Short = -2

    Public Structure DLGTEMPLATE
        Dim style As Integer
        Dim dwExtendedStyle As Integer
        Dim cdit As Short
        Dim X As Short
        Dim Y As Short
        Dim cx As Short
        Dim cy As Short
    End Structure

    Public Structure DLGITEMTEMPLATE
        Dim style As Integer
        Dim dwExtendedStyle As Integer
        Dim X As Short
        Dim Y As Short
        Dim cx As Short
        Dim cy As Short
        Dim id As Short
    End Structure


    Public Const KEYEVENTF_EXTENDEDKEY As Short = &H1S
    Public Const KEYEVENTF_KEYUP As Short = &H2S

    Public Declare Sub keybd_event Lib "user32" (bVk As Byte, bScan As Byte, dwFlags As Integer, dwExtraInfo As Integer)

    Public Const MOUSEEVENTF_MOVE As Short = &H1S '  mouse move
    Public Const MOUSEEVENTF_LEFTDOWN As Short = &H2S '  left button down
    Public Const MOUSEEVENTF_LEFTUP As Short = &H4S '  left button up
    Public Const MOUSEEVENTF_RIGHTDOWN As Short = &H8S '  right button down
    Public Const MOUSEEVENTF_RIGHTUP As Short = &H10S '  right button up
    Public Const MOUSEEVENTF_MIDDLEDOWN As Short = &H20S '  middle button down
    Public Const MOUSEEVENTF_MIDDLEUP As Short = &H40S '  middle button up
    Public Const MOUSEEVENTF_ABSOLUTE As Short = &H8000S '  absolute move

    ' GetQueueStatus flags
    Public Const QS_KEY As Short = &H1S
    Public Const QS_MOUSEMOVE As Short = &H2S
    Public Const QS_MOUSEBUTTON As Short = &H4S
    Public Const QS_POSTMESSAGE As Short = &H8S
    Public Const QS_TIMER As Short = &H10S
    Public Const QS_PAINT As Short = &H20S
    Public Const QS_SENDMESSAGE As Short = &H40S
    Public Const QS_HOTKEY As Short = &H80S

    Public Const QS_MOUSE As Boolean = (QS_MOUSEMOVE Or QS_MOUSEBUTTON)

    Public Const QS_INPUT As Boolean = (QS_MOUSE Or QS_KEY)

    Public Const QS_ALLEVENTS As Boolean = (QS_INPUT Or QS_POSTMESSAGE Or QS_TIMER Or QS_PAINT Or QS_HOTKEY)

    Public Const QS_ALLINPUT As Boolean = (QS_SENDMESSAGE Or QS_PAINT Or QS_TIMER Or QS_POSTMESSAGE Or QS_MOUSEBUTTON Or QS_MOUSEMOVE Or QS_HOTKEY Or QS_KEY)

    ' GetSystemMetrics() codes
    Public Const SM_CXSCREEN As Short = 0
    Public Const SM_CYSCREEN As Short = 1
    Public Const SM_CXVSCROLL As Short = 2
    Public Const SM_CYHSCROLL As Short = 3
    Public Const SM_CYCAPTION As Short = 4
    Public Const SM_CXBORDER As Short = 5
    Public Const SM_CYBORDER As Short = 6
    Public Const SM_CXDLGFRAME As Short = 7
    Public Const SM_CYDLGFRAME As Short = 8
    Public Const SM_CYVTHUMB As Short = 9
    Public Const SM_CXHTHUMB As Short = 10
    Public Const SM_CXICON As Short = 11
    Public Const SM_CYICON As Short = 12
    Public Const SM_CXCURSOR As Short = 13
    Public Const SM_CYCURSOR As Short = 14
    Public Const SM_CYMENU As Short = 15
    Public Const SM_CXFULLSCREEN As Short = 16
    Public Const SM_CYFULLSCREEN As Short = 17
    Public Const SM_CYKANJIWINDOW As Short = 18
    Public Const SM_MOUSEPRESENT As Short = 19
    Public Const SM_CYVSCROLL As Short = 20
    Public Const SM_CXHSCROLL As Short = 21
    Public Const SM_DEBUG As Short = 22
    Public Const SM_SWAPBUTTON As Short = 23
    Public Const SM_RESERVED1 As Short = 24
    Public Const SM_RESERVED2 As Short = 25
    Public Const SM_RESERVED3 As Short = 26
    Public Const SM_RESERVED4 As Short = 27
    Public Const SM_CXMIN As Short = 28
    Public Const SM_CYMIN As Short = 29
    Public Const SM_CXSIZE As Short = 30
    Public Const SM_CYSIZE As Short = 31
    Public Const SM_CXFRAME As Short = 32
    Public Const SM_CYFRAME As Short = 33
    Public Const SM_CXMINTRACK As Short = 34
    Public Const SM_CYMINTRACK As Short = 35
    Public Const SM_CXDOUBLECLK As Short = 36
    Public Const SM_CYDOUBLECLK As Short = 37
    Public Const SM_CXICONSPACING As Short = 38
    Public Const SM_CYICONSPACING As Short = 39
    Public Const SM_MENUDROPALIGNMENT As Short = 40
    Public Const SM_PENWINDOWS As Short = 41
    Public Const SM_DBCSENABLED As Short = 42
    Public Const SM_CMOUSEBUTTONS As Short = 43
    Public Const SM_CMETRICS As Short = 44
    Public Const SM_CXSIZEFRAME As Short = SM_CXFRAME
    Public Const SM_CYSIZEFRAME As Short = SM_CYFRAME
    Public Const SM_CXFIXEDFRAME As Short = SM_CXDLGFRAME
    Public Const SM_CYFIXEDFRAME As Short = SM_CYDLGFRAME

    ' Flags for TrackPopupMenu
    Public Const TPM_LEFTBUTTON As Integer = &H0
    Public Const TPM_RIGHTBUTTON As Integer = &H2
    Public Const TPM_LEFTALIGN As Integer = &H0
    Public Const TPM_CENTERALIGN As Integer = &H4
    Public Const TPM_RIGHTALIGN As Integer = &H8

    Public Declare Function DrawIcon Lib "user32" (hdc As Integer, X As Integer, Y As Integer, hIcon As Integer) As Integer

    ' DrawText() Format Flags
    Public Const DT_TOP As Short = &H0S
    Public Const DT_LEFT As Short = &H0S
    Public Const DT_CENTER As Short = &H1S
    Public Const DT_RIGHT As Short = &H2S
    Public Const DT_VCENTER As Short = &H4S
    Public Const DT_BOTTOM As Short = &H8S
    Public Const DT_WORDBREAK As Short = &H10S
    Public Const DT_SINGLELINE As Short = &H20S
    Public Const DT_EXPANDTABS As Short = &H40S
    Public Const DT_TABSTOP As Short = &H80S
    Public Const DT_NOCLIP As Short = &H100S
    Public Const DT_EXTERNALLEADING As Short = &H200S
    Public Const DT_CALCRECT As Short = &H400S
    Public Const DT_NOPREFIX As Short = &H800S
    Public Const DT_INTERNAL As Short = &H1000S

    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function DrawText Lib "user32" Alias "DrawTextA" (hdc As Integer, lpStr As String, nCount As Integer, ByRef lpRect As RECT, wFormat As Integer) As Integer
    Public Declare Function TabbedTextOut Lib "user32" Alias "TabbedTextOutA" (hdc As Integer, X As Integer, Y As Integer, lpString As String, nCount As Integer, nTabPositions As Integer, ByRef lpnTabStopPositions As Integer, nTabOrigin As Integer) As Integer
    Public Declare Function GetTabbedTextExtent Lib "user32" Alias "GetTabbedTextExtentA" (hdc As Integer, lpString As String, nCount As Integer, nTabPositions As Integer, ByRef lpnTabStopPositions As Integer) As Integer

    Public Declare Function UpdateWindow Lib "user32" (hWnd As Integer) As Integer
    Public Declare Function SetActiveWindow Lib "user32" (hWnd As Integer) As Integer
    Public Declare Function GetForegroundWindow Lib "user32" () As Integer
    Public Declare Function SetForegroundWindow Lib "user32" (hWnd As Integer) As Integer
    Public Declare Function WindowFromDC Lib "user32" (hdc As Integer) As Integer

    Public Declare Function GetDC Lib "user32" (hWnd As Integer) As Integer
    Public Declare Function GetDCEx Lib "user32" (hWnd As Integer, hrgnclip As Integer, fdwOptions As Integer) As Integer

    Public Const DCX_WINDOW As Integer = &H1
    Public Const DCX_CACHE As Integer = &H2
    Public Const DCX_NORESETATTRS As Integer = &H4
    Public Const DCX_CLIPCHILDREN As Integer = &H8
    Public Const DCX_CLIPSIBLINGS As Integer = &H10
    Public Const DCX_PARENTCLIP As Integer = &H20

    Public Const DCX_EXCLUDERGN As Integer = &H40
    Public Const DCX_INTERSECTRGN As Integer = &H80

    Public Const DCX_EXCLUDEUPDATE As Integer = &H100
    Public Const DCX_INTERSECTUPDATE As Integer = &H200

    Public Const DCX_LOCKWINDOWUPDATE As Integer = &H400

    Public Const DCX_NORECOMPUTE As Integer = &H100000
    Public Const DCX_VALIDATE As Integer = &H200000

    Public Declare Function GetWindowDC Lib "user32" (hWnd As Integer) As Integer
    Public Declare Function ReleaseDC Lib "user32" (hWnd As Integer, hdc As Integer) As Integer

    'UPGRADE_WARNING: ?? PAINTSTRUCT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function BeginPaint Lib "user32" (hWnd As Integer, ByRef lpPaint As PAINTSTRUCT) As Integer
    'UPGRADE_WARNING: ?? PAINTSTRUCT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function EndPaint Lib "user32" (hWnd As Integer, ByRef lpPaint As PAINTSTRUCT) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetUpdateRect Lib "user32" (hWnd As Integer, ByRef lpRect As RECT, bErase As Integer) As Integer
    Public Declare Function GetUpdateRgn Lib "user32" (hWnd As Integer, hRgn As Integer, fErase As Integer) As Integer
    Public Declare Function ExcludeUpdateRgn Lib "user32" (hdc As Integer, hWnd As Integer) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function InvalidateRect Lib "user32" (hWnd As Integer, ByRef lpRect As RECT, bErase As Integer) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ValidateRect Lib "user32" (hWnd As Integer, ByRef lpRect As RECT) As Integer
    Public Declare Function InvalidateRgn Lib "user32" (hWnd As Integer, hRgn As Integer, bErase As Integer) As Integer
    Public Declare Function ValidateRgn Lib "user32" (hWnd As Integer, hRgn As Integer) As Integer

    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function RedrawWindow Lib "user32" (hWnd As Integer, ByRef lprcUpdate As RECT, hrgnUpdate As Integer, fuRedraw As Integer) As Integer

    Public Const RDW_INVALIDATE As Short = &H1S
    Public Const RDW_INTERNALPAINT As Short = &H2S
    Public Const RDW_ERASE As Short = &H4S

    Public Const RDW_VALIDATE As Short = &H8S
    Public Const RDW_NOINTERNALPAINT As Short = &H10S
    Public Const RDW_NOERASE As Short = &H20S

    Public Const RDW_NOCHILDREN As Short = &H40S
    Public Const RDW_ALLCHILDREN As Short = &H80S

    Public Const RDW_UPDATENOW As Short = &H100S
    Public Const RDW_ERASENOW As Short = &H200S

    Public Const RDW_FRAME As Short = &H400S
    Public Const RDW_NOFRAME As Short = &H800S

    Public Declare Function LockWindowUpdate Lib "user32" (hwndLock As Integer) As Integer

    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ScrollWindow Lib "user32" (hWnd As Integer, XAmount As Integer, YAmount As Integer, ByRef lpRect As RECT, ByRef lpClipRect As RECT) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ScrollDC Lib "user32" (hdc As Integer, dx As Integer, dy As Integer, ByRef lprcScroll As RECT, ByRef lprcClip As RECT, hrgnUpdate As Integer, ByRef lprcUpdate As RECT) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ScrollWindowEx Lib "user32" (hWnd As Integer, dx As Integer, dy As Integer, ByRef lprcScroll As RECT, ByRef lprcClip As RECT, hrgnUpdate As Integer, ByRef lprcUpdate As RECT, fuScroll As Integer) As Integer

    Public Const SW_SCROLLCHILDREN As Short = &H1S
    Public Const SW_INVALIDATE As Short = &H2S
    Public Const SW_ERASE As Short = &H4S

    Public Declare Function SetScrollPos Lib "user32" (hWnd As Integer, nBar As Integer, nPos As Integer, bRedraw As Integer) As Integer
    Public Declare Function GetScrollPos Lib "user32" (hWnd As Integer, nBar As Integer) As Integer
    Public Declare Function SetScrollRange Lib "user32" (hWnd As Integer, nBar As Integer, nMinPos As Integer, nMaxPos As Integer, bRedraw As Integer) As Integer
    Public Declare Function GetScrollRange Lib "user32" (hWnd As Integer, nBar As Integer, ByRef lpMinPos As Integer, ByRef lpMaxPos As Integer) As Integer
    Public Declare Function ShowScrollBar Lib "user32" (hWnd As Integer, wBar As Integer, bShow As Integer) As Integer
    Public Declare Function EnableScrollBar Lib "user32" (hWnd As Integer, wSBflags As Integer, wArrows As Integer) As Integer

    ' EnableScrollBar() flags
    Public Const ESB_ENABLE_BOTH As Short = &H0S
    Public Const ESB_DISABLE_BOTH As Short = &H3S

    Public Const ESB_DISABLE_LEFT As Short = &H1S
    Public Const ESB_DISABLE_RIGHT As Short = &H2S

    Public Const ESB_DISABLE_UP As Short = &H1S
    Public Const ESB_DISABLE_DOWN As Short = &H2S

    Public Const ESB_DISABLE_LTUP As Short = ESB_DISABLE_LEFT
    Public Const ESB_DISABLE_RTDN As Short = ESB_DISABLE_RIGHT

    Public Declare Function SetProp Lib "user32" Alias "SetPropA" (hWnd As Integer, lpString As String, hData As Integer) As Integer
    Public Declare Function GetProp Lib "user32" Alias "GetPropA" (hWnd As Integer, lpString As String) As Integer
    Public Declare Function RemoveProp Lib "user32" Alias "RemovePropA" (hWnd As Integer, lpString As String) As Integer

    Public Declare Function SetWindowText Lib "user32" Alias "SetWindowTextA" (hWnd As Integer, lpString As String) As Integer
    Public Declare Function GetWindowText Lib "user32" Alias "GetWindowTextA" (hWnd As Integer, lpString As String, cch As Integer) As Integer
    Public Declare Function GetWindowTextLength Lib "user32" Alias "GetWindowTextLengthA" (hWnd As Integer) As Integer

    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetClientRect Lib "user32" (hWnd As Integer, ByRef lpRect As RECT) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetWindowRect Lib "user32" (hWnd As Integer, ByRef lpRect As RECT) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function AdjustWindowRect Lib "user32" (ByRef lpRect As RECT, dwStyle As Integer, bMenu As Integer) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function AdjustWindowRectEx Lib "user32" (ByRef lpRect As RECT, dsStyle As Integer, bMenu As Integer, dwEsStyle As Integer) As Integer

    ' MessageBox() Flags
    Public Const MB_OK As Integer = &H0
    Public Const MB_OKCANCEL As Integer = &H1
    Public Const MB_ABORTRETRYIGNORE As Integer = &H2
    Public Const MB_YESNOCANCEL As Integer = &H3
    Public Const MB_YESNO As Integer = &H4
    Public Const MB_RETRYCANCEL As Integer = &H5

    Public Const MB_ICONHAND As Integer = &H10
    Public Const MB_ICONQUESTION As Integer = &H20
    Public Const MB_ICONEXCLAMATION As Integer = &H30
    Public Const MB_ICONASTERISK As Integer = &H40

    Public Const MB_ICONINFORMATION As Integer = MB_ICONASTERISK
    Public Const MB_ICONSTOP As Integer = MB_ICONHAND

    Public Const MB_DEFBUTTON1 As Integer = &H0
    Public Const MB_DEFBUTTON2 As Integer = &H100
    Public Const MB_DEFBUTTON3 As Integer = &H200

    Public Const MB_APPLMODAL As Integer = &H0
    Public Const MB_SYSTEMMODAL As Integer = &H1000
    Public Const MB_TASKMODAL As Integer = &H2000

    Public Const MB_NOFOCUS As Integer = &H8000
    Public Const MB_SETFOREGROUND As Integer = &H10000
    Public Const MB_DEFAULT_DESKTOP_ONLY As Integer = &H20000

    Public Const MB_TYPEMASK As Integer = &HF
    Public Const MB_ICONMASK As Integer = &HF0
    Public Const MB_DEFMASK As Integer = &HF00
    Public Const MB_MODEMASK As Integer = &H3000
    Public Const MB_MISCMASK As Integer = &HC000

    Public Declare Function MessageBox Lib "user32" Alias "MessageBoxA" (hWnd As Integer, lpText As String, lpCaption As String, wType As Integer) As Integer
    Public Declare Function MessageBoxEx Lib "user32" Alias "MessageBoxExA" (hWnd As Integer, lpText As String, lpCaption As String, uType As Integer, wLanguageId As Integer) As Integer
    Public Declare Function MessageBeep Lib "user32" (wType As Integer) As Integer

    Public Declare Function ShowCursor Lib "user32" (bShow As Integer) As Integer
    Public Declare Function SetCursorPos Lib "user32" (X As Integer, Y As Integer) As Integer
    Public Declare Function SetCursor Lib "user32" (hCursor As Integer) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetCursorPos Lib "user32" (ByRef lpPoint As POINTAPI) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function ClipCursor Lib "user32" (ByRef lpRect As Object) As Integer
    Public Declare Function GetCursor Lib "user32" () As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetClipCursor Lib "user32" (ByRef lprc As RECT) As Integer

    Public Declare Function CreateCaret Lib "user32" (hWnd As Integer, hBitmap As Integer, nWidth As Integer, nHeight As Integer) As Integer
    Public Declare Function GetCaretBlinkTime Lib "user32" () As Integer
    Public Declare Function SetCaretBlinkTime Lib "user32" (wMSeconds As Integer) As Integer
    Public Declare Function DestroyCaret Lib "user32" () As Integer
    Public Declare Function HideCaret Lib "user32" (hWnd As Integer) As Integer
    Public Declare Function ShowCaret Lib "user32" (hWnd As Integer) As Integer
    Public Declare Function SetCaretPos Lib "user32" (X As Integer, Y As Integer) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetCaretPos Lib "user32" (ByRef lpPoint As POINTAPI) As Integer

    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ClientToScreen Lib "user32" (hWnd As Integer, ByRef lpPoint As POINTAPI) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ScreenToClient Lib "user32" (hWnd As Integer, ByRef lpPoint As POINTAPI) As Integer

    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function MapWindowPoints Lib "user32" (hwndFrom As Integer, hwndTo As Integer, ByRef lppt As Object, cPoints As Integer) As Integer
    Public Declare Function WindowFromPoint Lib "user32" (xPoint As Integer, yPoint As Integer) As Integer
    Public Declare Function ChildWindowFromPoint Lib "user32" (hWnd As Integer, xPoint As Integer, yPoint As Integer) As Integer

    ' Color Types
    Public Const CTLCOLOR_MSGBOX As Short = 0
    Public Const CTLCOLOR_EDIT As Short = 1
    Public Const CTLCOLOR_LISTBOX As Short = 2
    Public Const CTLCOLOR_BTN As Short = 3
    Public Const CTLCOLOR_DLG As Short = 4
    Public Const CTLCOLOR_SCROLLBAR As Short = 5
    Public Const CTLCOLOR_STATIC As Short = 6
    Public Const CTLCOLOR_MAX As Short = 8 '  three bits max

    Public Const COLOR_SCROLLBAR As Short = 0
    Public Const COLOR_BACKGROUND As Short = 1
    Public Const COLOR_ACTIVECAPTION As Short = 2
    Public Const COLOR_INACTIVECAPTION As Short = 3
    Public Const COLOR_MENU As Short = 4
    Public Const COLOR_WINDOW As Short = 5
    Public Const COLOR_WINDOWFRAME As Short = 6
    Public Const COLOR_MENUTEXT As Short = 7
    Public Const COLOR_WINDOWTEXT As Short = 8
    Public Const COLOR_CAPTIONTEXT As Short = 9
    Public Const COLOR_ACTIVEBORDER As Short = 10
    Public Const COLOR_INACTIVEBORDER As Short = 11
    Public Const COLOR_APPWORKSPACE As Short = 12
    Public Const COLOR_HIGHLIGHT As Short = 13
    Public Const COLOR_HIGHLIGHTTEXT As Short = 14
    Public Const COLOR_BTNFACE As Short = 15
    Public Const COLOR_BTNSHADOW As Short = 16
    Public Const COLOR_GRAYTEXT As Short = 17
    Public Const COLOR_BTNTEXT As Short = 18
    Public Const COLOR_INACTIVECAPTIONTEXT As Short = 19
    Public Const COLOR_BTNHIGHLIGHT As Short = 20

    Public Declare Function GetSysColor Lib "user32" (nIndex As Integer) As Integer
    Public Declare Function SetSysColors Lib "user32" (nChanges As Integer, ByRef lpSysColor As Integer, ByRef lpColorValues As Integer) As Integer

    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function DrawFocusRect Lib "user32" (hdc As Integer, ByRef lpRect As RECT) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function FillRect Lib "user32" (hdc As Integer, ByRef lpRect As RECT, hBrush As Integer) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function FrameRect Lib "user32" (hdc As Integer, ByRef lpRect As RECT, hBrush As Integer) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function InvertRect Lib "user32" (hdc As Integer, ByRef lpRect As RECT) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetRect Lib "user32" (ByRef lpRect As RECT, X1 As Integer, Y1 As Integer, X2 As Integer, Y2 As Integer) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetRectEmpty Lib "user32" (ByRef lpRect As RECT) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CopyRect Lib "user32" (ByRef lpDestRect As RECT, ByRef lpSourceRect As RECT) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function InflateRect Lib "user32" (ByRef lpRect As RECT, X As Integer, Y As Integer) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function IntersectRect Lib "user32" (ByRef lpDestRect As RECT, ByRef lpSrc1Rect As RECT, ByRef lpSrc2Rect As RECT) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function UnionRect Lib "user32" (ByRef lpDestRect As RECT, ByRef lpSrc1Rect As RECT, ByRef lpSrc2Rect As RECT) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SubtractRect Lib "user32" (ByRef lprcDst As RECT, ByRef lprcSrc1 As RECT, ByRef lprcSrc2 As RECT) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function OffsetRect Lib "user32" (ByRef lpRect As RECT, X As Integer, Y As Integer) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function IsRectEmpty Lib "user32" (ByRef lpRect As RECT) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function EqualRect Lib "user32" (ByRef lpRect1 As RECT, ByRef lpRect2 As RECT) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function PtInRect Lib "user32" (ByRef lpRect As RECT, ByRef pt As POINTAPI) As Integer

    Public Declare Function GetWindowWord Lib "user32" (hWnd As Integer, nIndex As Integer) As Short
    Public Declare Function SetWindowWord Lib "user32" (hWnd As Integer, nIndex As Integer, wNewWord As Integer) As Integer
    Public Declare Function GetWindowLong Lib "user32" Alias "GetWindowLongA" (hWnd As Integer, nIndex As Integer) As Integer
    Public Declare Function SetWindowLong Lib "user32" Alias "SetWindowLongA" (hWnd As Integer, nIndex As Integer, dwNewLong As Integer) As Integer
    Public Declare Function GetClassWord Lib "user32" (hWnd As Integer, nIndex As Integer) As Integer
    Public Declare Function SetClassWord Lib "user32" (hWnd As Integer, nIndex As Integer, wNewWord As Integer) As Integer
    Public Declare Function GetClassLong Lib "user32" Alias "GetClassLongA" (hWnd As Integer, nIndex As Integer) As Integer
    Public Declare Function SetClassLong Lib "user32" Alias "SetClassLongA" (hWnd As Integer, nIndex As Integer, dwNewLong As Integer) As Integer
    Public Declare Function GetDesktopWindow Lib "user32" () As Integer

    Public Declare Function GetParent Lib "user32" (hWnd As Integer) As Integer
    Public Declare Function SetParent Lib "user32" (hWndChild As Integer, hWndNewParent As Integer) As Integer
    Public Declare Function FindWindow Lib "user32" Alias "FindWindowA" (lpClassName As String, lpWindowName As String) As Integer

    Public Declare Function GetClassName Lib "user32" Alias "GetClassNameA" (hWnd As Integer, lpClassName As String, nMaxCount As Integer) As Integer
    Public Declare Function GetTopWindow Lib "user32" (hWnd As Integer) As Integer
    Public Declare Function GetNextWindow Lib "user32" Alias "GetWindow" (hWnd As Integer, wFlag As Integer) As Integer

    Public Declare Function GetWindowThreadProcessId Lib "user32" (hWnd As Integer, ByRef lpdwProcessId As Integer) As Integer

    Public Declare Function GetLastActivePopup Lib "user32" (hwndOwnder As Integer) As Integer

    ' GetWindow() Constants
    Public Const GW_HWNDFIRST As Short = 0
    Public Const GW_HWNDLAST As Short = 1
    Public Const GW_HWNDNEXT As Short = 2
    Public Const GW_HWNDPREV As Short = 3
    Public Const GW_OWNER As Short = 4
    Public Const GW_CHILD As Short = 5
    Public Const GW_MAX As Short = 5

    Public Declare Function GetWindow Lib "user32" (hWnd As Integer, wCmd As Integer) As Integer
    Public Declare Function UnhookWindowsHookEx Lib "user32" (hHook As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function CallNextHookEx Lib "user32" (hHook As Integer, nCode As Integer, wParam As Integer, ByRef lParam As Object) As Integer

    ' Menu flags for Add/Check/EnableMenuItem()
    Public Const MF_INSERT As Integer = &H0
    Public Const MF_CHANGE As Integer = &H80
    Public Const MF_APPEND As Integer = &H100
    Public Const MF_DELETE As Integer = &H200
    Public Const MF_REMOVE As Integer = &H1000

    Public Const MF_BYCOMMAND As Integer = &H0
    Public Const MF_BYPOSITION As Integer = &H400

    Public Const MF_SEPARATOR As Integer = &H800

    Public Const MF_ENABLED As Integer = &H0
    Public Const MF_GRAYED As Integer = &H1
    Public Const MF_DISABLED As Integer = &H2

    Public Const MF_UNCHECKED As Integer = &H0
    Public Const MF_CHECKED As Integer = &H8
    Public Const MF_USECHECKBITMAPS As Integer = &H200

    Public Const MF_STRING As Integer = &H0
    Public Const MF_BITMAP As Integer = &H4
    Public Const MF_OWNERDRAW As Integer = &H100

    Public Const MF_POPUP As Integer = &H10
    Public Const MF_MENUBARBREAK As Integer = &H20
    Public Const MF_MENUBREAK As Integer = &H40

    Public Const MF_UNHILITE As Integer = &H0
    Public Const MF_HILITE As Integer = &H80

    Public Const MF_SYSMENU As Integer = &H2000
    Public Const MF_HELP As Integer = &H4000
    Public Const MF_MOUSESELECT As Integer = &H8000

    ' Menu item resource format
    Public Structure MENUITEMTEMPLATEHEADER
        Dim versionNumber As Short
        Dim offset As Short
    End Structure

    Public Structure MENUITEMTEMPLATE
        Dim mtOption As Short
        Dim mtID As Short
        Dim mtString As Byte
    End Structure

    Public Const MF_END As Short = &H80S

    ' System Menu Command Values
    Public Const SC_SIZE As Short = &HF000S
    Public Const SC_MOVE As Short = &HF010S
    Public Const SC_MINIMIZE As Short = &HF020S
    Public Const SC_MAXIMIZE As Short = &HF030S
    Public Const SC_NEXTWINDOW As Short = &HF040S
    Public Const SC_PREVWINDOW As Short = &HF050S
    Public Const SC_CLOSE As Short = &HF060S
    Public Const SC_VSCROLL As Short = &HF070S
    Public Const SC_HSCROLL As Short = &HF080S
    Public Const SC_MOUSEMENU As Short = &HF090S
    Public Const SC_KEYMENU As Short = &HF100S
    Public Const SC_ARRANGE As Short = &HF110S
    Public Const SC_RESTORE As Short = &HF120S
    Public Const SC_TASKLIST As Short = &HF130S
    Public Const SC_SCREENSAVE As Short = &HF140S
    Public Const SC_HOTKEY As Short = &HF150S

    ' Obsolete names
    Public Const SC_ICON As Short = SC_MINIMIZE
    Public Const SC_ZOOM As Short = SC_MAXIMIZE

    ' Resource Loading Routines
    Public Declare Function LoadBitmap Lib "user32" Alias "LoadBitmapA" (hInstance As Integer, lpBitmapName As String) As Integer
    Public Declare Function LoadCursor Lib "user32" Alias "LoadCursorA" (hInstance As Integer, lpCursorName As String) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function CreateCursor Lib "user32" (hInstance As Integer, nXhotspot As Integer, nYhotspot As Integer, nWidth As Integer, nHeight As Integer, ByRef lpANDbitPlane As Object, ByRef lpXORbitPlane As Object) As Integer
    Public Declare Function DestroyCursor Lib "user32" (hCursor As Integer) As Integer
    Public Declare Function CopyCursor Lib "user32" (hcur As Integer) As Integer

    ' Standard Cursor IDs
    Public Const IDC_ARROW As Short = 32512
    Public Const IDC_IBEAM As Short = 32513
    Public Const IDC_WAIT As Short = 32514
    Public Const IDC_CROSS As Short = 32515
    Public Const IDC_UPARROW As Short = 32516
    Public Const IDC_SIZE As Short = 32640
    Public Const IDC_ICON As Short = 32641
    Public Const IDC_SIZENWSE As Short = 32642
    Public Const IDC_SIZENESW As Short = 32643
    Public Const IDC_SIZEWE As Short = 32644
    Public Const IDC_SIZENS As Short = 32645
    Public Const IDC_SIZEALL As Short = 32646
    Public Const IDC_NO As Short = 32648
    Public Const IDC_APPSTARTING As Short = 32650

    Public Structure ICONINFO
        Dim fIcon As Integer
        Dim xHotspot As Integer
        Dim yHotspot As Integer
        Dim hbmMask As Integer
        Dim hbmColor As Integer
    End Structure

    Public Declare Function LoadIcon Lib "user32" Alias "LoadIconA" (hInstance As Integer, lpIconName As String) As Integer
    Public Declare Function CreateIcon Lib "user32" (hInstance As Integer, nWidth As Integer, nHeight As Integer, nPlanes As Byte, nBitsPixel As Byte, ByRef lpANDbits As Byte, ByRef lpXORbits As Byte) As Integer
    Public Declare Function DestroyIcon Lib "user32" (hIcon As Integer) As Integer
    Public Declare Function LookupIconIdFromDirectory Lib "user32" (ByRef presbits As Byte, fIcon As Integer) As Integer
    Public Declare Function CreateIconFromResource Lib "user32" (ByRef presbits As Byte, dwResSize As Integer, fIcon As Integer, dwVer As Integer) As Integer
    'UPGRADE_WARNING: ?? ICONINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreateIconIndirect Lib "user32" (ByRef piconinfo As ICONINFO) As Integer
    Public Declare Function CopyIcon Lib "user32" (hIcon As Integer) As Integer
    'UPGRADE_WARNING: ?? ICONINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetIconInfo Lib "user32" (hIcon As Integer, ByRef piconinfo As ICONINFO) As Integer

    ' OEM Resource Ordinal Numbers
    Public Const OBM_CLOSE As Short = 32754
    Public Const OBM_UPARROW As Short = 32753
    Public Const OBM_DNARROW As Short = 32752
    Public Const OBM_RGARROW As Short = 32751
    Public Const OBM_LFARROW As Short = 32750
    Public Const OBM_REDUCE As Short = 32749
    Public Const OBM_ZOOM As Short = 32748
    Public Const OBM_RESTORE As Short = 32747
    Public Const OBM_REDUCED As Short = 32746
    Public Const OBM_ZOOMD As Short = 32745
    Public Const OBM_RESTORED As Short = 32744
    Public Const OBM_UPARROWD As Short = 32743
    Public Const OBM_DNARROWD As Short = 32742
    Public Const OBM_RGARROWD As Short = 32741
    Public Const OBM_LFARROWD As Short = 32740
    Public Const OBM_MNARROW As Short = 32739
    Public Const OBM_COMBO As Short = 32738
    Public Const OBM_UPARROWI As Short = 32737
    Public Const OBM_DNARROWI As Short = 32736
    Public Const OBM_RGARROWI As Short = 32735
    Public Const OBM_LFARROWI As Short = 32734

    Public Const OBM_OLD_CLOSE As Short = 32767
    Public Const OBM_SIZE As Short = 32766
    Public Const OBM_OLD_UPARROW As Short = 32765
    Public Const OBM_OLD_DNARROW As Short = 32764
    Public Const OBM_OLD_RGARROW As Short = 32763
    Public Const OBM_OLD_LFARROW As Short = 32762
    Public Const OBM_BTSIZE As Short = 32761
    Public Const OBM_CHECK As Short = 32760
    Public Const OBM_CHECKBOXES As Short = 32759
    Public Const OBM_BTNCORNERS As Short = 32758
    Public Const OBM_OLD_REDUCE As Short = 32757
    Public Const OBM_OLD_ZOOM As Short = 32756
    Public Const OBM_OLD_RESTORE As Short = 32755

    Public Const OCR_NORMAL As Short = 32512
    Public Const OCR_IBEAM As Short = 32513
    Public Const OCR_WAIT As Short = 32514
    Public Const OCR_CROSS As Short = 32515
    Public Const OCR_UP As Short = 32516
    Public Const OCR_SIZE As Short = 32640
    Public Const OCR_ICON As Short = 32641
    Public Const OCR_SIZENWSE As Short = 32642
    Public Const OCR_SIZENESW As Short = 32643
    Public Const OCR_SIZEWE As Short = 32644
    Public Const OCR_SIZENS As Short = 32645
    Public Const OCR_SIZEALL As Short = 32646
    Public Const OCR_ICOCUR As Short = 32647
    Public Const OCR_NO As Short = 32648 ' not in win3.1

    Public Const OIC_SAMPLE As Short = 32512
    Public Const OIC_HAND As Short = 32513
    Public Const OIC_QUES As Short = 32514
    Public Const OIC_BANG As Short = 32515
    Public Const OIC_NOTE As Short = 32516

    Public Const ORD_LANGDRIVER As Short = 1 '  The ordinal number for the entry point of
    '  language drivers.

    ' Standard Icon IDs
    Public Const IDI_APPLICATION As Short = 32512
    Public Const IDI_HAND As Short = 32513
    Public Const IDI_QUESTION As Short = 32514
    Public Const IDI_EXCLAMATION As Short = 32515
    Public Const IDI_ASTERISK As Short = 32516

    Public Declare Function LoadString Lib "user32" Alias "LoadStringA" (hInstance As Integer, wID As Integer, lpBuffer As String, nBufferMax As Integer) As Integer

    ' Dialog Box Command IDs
    Public Const IDOK As Short = 1
    Public Const IDCANCEL As Short = 2
    Public Const IDABORT As Short = 3
    Public Const IDRETRY As Short = 4
    Public Const IDIGNORE As Short = 5
    Public Const IDYES As Short = 6
    Public Const IDNO As Short = 7

    ' Control Manager Structures and Definitions

    ' Edit Control Styles
    Public Const ES_LEFT As Integer = &H0
    Public Const ES_CENTER As Integer = &H1
    Public Const ES_RIGHT As Integer = &H2
    Public Const ES_MULTILINE As Integer = &H4
    Public Const ES_UPPERCASE As Integer = &H8
    Public Const ES_LOWERCASE As Integer = &H10
    Public Const ES_PASSWORD As Integer = &H20
    Public Const ES_AUTOVSCROLL As Integer = &H40
    Public Const ES_AUTOHSCROLL As Integer = &H80
    Public Const ES_NOHIDESEL As Integer = &H100
    Public Const ES_OEMCONVERT As Integer = &H400
    Public Const ES_READONLY As Integer = &H800
    Public Const ES_WANTRETURN As Integer = &H1000

    ' Edit Control Notification Codes
    Public Const EN_SETFOCUS As Short = &H100S
    Public Const EN_KILLFOCUS As Short = &H200S
    Public Const EN_CHANGE As Short = &H300S
    Public Const EN_UPDATE As Short = &H400S
    Public Const EN_ERRSPACE As Short = &H500S
    Public Const EN_MAXTEXT As Short = &H501S
    Public Const EN_HSCROLL As Short = &H601S
    Public Const EN_VSCROLL As Short = &H602S

    ' Edit Control Messages
    Public Const EM_GETSEL As Short = &HB0S
    Public Const EM_SETSEL As Short = &HB1S
    Public Const EM_GETRECT As Short = &HB2S
    Public Const EM_SETRECT As Short = &HB3S
    Public Const EM_SETRECTNP As Short = &HB4S
    Public Const EM_SCROLL As Short = &HB5S
    Public Const EM_LINESCROLL As Short = &HB6S
    Public Const EM_SCROLLCARET As Short = &HB7S
    Public Const EM_GETMODIFY As Short = &HB8S
    Public Const EM_SETMODIFY As Short = &HB9S
    Public Const EM_GETLINECOUNT As Short = &HBAS
    Public Const EM_LINEINDEX As Short = &HBBS
    Public Const EM_SETHANDLE As Short = &HBCS
    Public Const EM_GETHANDLE As Short = &HBDS
    Public Const EM_GETTHUMB As Short = &HBES
    Public Const EM_LINELENGTH As Short = &HC1S
    Public Const EM_REPLACESEL As Short = &HC2S
    Public Const EM_GETLINE As Short = &HC4S
    Public Const EM_LIMITTEXT As Short = &HC5S
    Public Const EM_CANUNDO As Short = &HC6S
    Public Const EM_UNDO As Short = &HC7S
    Public Const EM_FMTLINES As Short = &HC8S
    Public Const EM_LINEFROMCHAR As Short = &HC9S
    Public Const EM_SETTABSTOPS As Short = &HCBS
    Public Const EM_SETPASSWORDCHAR As Short = &HCCS
    Public Const EM_EMPTYUNDOBUFFER As Short = &HCDS
    Public Const EM_GETFIRSTVISIBLELINE As Short = &HCES
    Public Const EM_SETREADONLY As Short = &HCFS
    Public Const EM_SETWORDBREAKPROC As Short = &HD0S
    Public Const EM_GETWORDBREAKPROC As Short = &HD1S
    Public Const EM_GETPASSWORDCHAR As Short = &HD2S

    ' EDITWORDBREAKPROC code values
    Public Const WB_LEFT As Short = 0
    Public Const WB_RIGHT As Short = 1
    Public Const WB_ISDELIMITER As Short = 2

    ' Button Control Styles
    Public Const BS_PUSHBUTTON As Integer = &H0
    Public Const BS_DEFPUSHBUTTON As Integer = &H1
    Public Const BS_CHECKBOX As Integer = &H2
    Public Const BS_AUTOCHECKBOX As Integer = &H3
    Public Const BS_RADIOBUTTON As Integer = &H4
    Public Const BS_3STATE As Integer = &H5
    Public Const BS_AUTO3STATE As Integer = &H6
    Public Const BS_GROUPBOX As Integer = &H7
    Public Const BS_USERBUTTON As Integer = &H8
    Public Const BS_AUTORADIOBUTTON As Integer = &H9
    Public Const BS_OWNERDRAW As Integer = &HB
    Public Const BS_LEFTTEXT As Integer = &H20

    ' User Button Notification Codes
    Public Const BN_CLICKED As Short = 0
    Public Const BN_PAINT As Short = 1
    Public Const BN_HILITE As Short = 2
    Public Const BN_UNHILITE As Short = 3
    Public Const BN_DISABLE As Short = 4
    Public Const BN_DOUBLECLICKED As Short = 5

    ' Button Control Messages
    Public Const BM_GETCHECK As Short = &HF0S
    Public Const BM_SETCHECK As Short = &HF1S
    Public Const BM_GETSTATE As Short = &HF2S
    Public Const BM_SETSTATE As Short = &HF3S
    Public Const BM_SETSTYLE As Short = &HF4S

    ' Static Control Constants
    Public Const SS_LEFT As Integer = &H0
    Public Const SS_CENTER As Integer = &H1
    Public Const SS_RIGHT As Integer = &H2
    Public Const SS_ICON As Integer = &H3
    Public Const SS_BLACKRECT As Integer = &H4
    Public Const SS_GRAYRECT As Integer = &H5
    Public Const SS_WHITERECT As Integer = &H6
    Public Const SS_BLACKFRAME As Integer = &H7
    Public Const SS_GRAYFRAME As Integer = &H8
    Public Const SS_WHITEFRAME As Integer = &H9
    Public Const SS_USERITEM As Integer = &HA
    Public Const SS_SIMPLE As Integer = &HB
    Public Const SS_LEFTNOWORDWRAP As Integer = &HC
    Public Const SS_NOPREFIX As Short = &H80S '  Don't do "&" character translation

    ' Static Control Mesages
    Public Const STM_SETICON As Short = &H170S
    Public Const STM_GETICON As Short = &H171S
    Public Const STM_MSGMAX As Short = &H172S

    Public Const WC_DIALOG As Short = 8002

    '  Get/SetWindowWord/Long offsets for use with WC_DIALOG windows
    Public Const DWL_MSGRESULT As Short = 0
    Public Const DWL_DLGPROC As Short = 4
    Public Const DWL_USER As Short = 8

    ' Dialog Manager Routines
    'UPGRADE_WARNING: ?? Msg ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function IsDialogMessage Lib "user32" Alias "IsDialogMessageA" (hDlg As Integer, ByRef lpMsg As Msg) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function MapDialogRect Lib "user32" (hDlg As Integer, ByRef lpRect As RECT) As Integer
    Public Declare Function DlgDirList Lib "user32" Alias "DlgDirListA" (hDlg As Integer, lpPathSpec As String, nIDListBox As Integer, nIDStaticPath As Integer, wFileType As Integer) As Integer

    ' DlgDirList, DlgDirListComboBox flags values
    Public Const DDL_READWRITE As Short = &H0S
    Public Const DDL_READONLY As Short = &H1S
    Public Const DDL_HIDDEN As Short = &H2S
    Public Const DDL_SYSTEM As Short = &H4S
    Public Const DDL_DIRECTORY As Short = &H10S
    Public Const DDL_ARCHIVE As Short = &H20S

    Public Const DDL_POSTMSGS As Short = &H2000S
    Public Const DDL_DRIVES As Short = &H4000S
    Public Const DDL_EXCLUSIVE As Short = &H8000S

    Public Declare Function DlgDirSelectEx Lib "user32" Alias "DlgDirSelectExA" (hWndDlg As Integer, lpszPath As String, cbPath As Integer, idListBox As Integer) As Integer
    Public Declare Function DlgDirListComboBox Lib "user32" Alias "DlgDirListComboBoxA" (hDlg As Integer, lpPathSpec As String, nIDComboBox As Integer, nIDStaticPath As Integer, wFileType As Integer) As Integer
    Public Declare Function DlgDirSelectComboBoxEx Lib "user32" Alias "DlgDirSelectComboBoxExA" (hWndDlg As Integer, lpszPath As String, cbPath As Integer, idComboBox As Integer) As Integer

    ' Dialog Styles
    Public Const DS_ABSALIGN As Integer = &H1
    Public Const DS_SYSMODAL As Integer = &H2
    Public Const DS_LOCALEDIT As Short = &H20S '  Edit items get Local storage.
    Public Const DS_SETFONT As Short = &H40S '  User specified font for Dlg controls
    Public Const DS_MODALFRAME As Short = &H80S '  Can be combined with WS_CAPTION
    Public Const DS_NOIDLEMSG As Short = &H100S '  WM_ENTERIDLE message will not be sent
    Public Const DS_SETFOREGROUND As Short = &H200S '  not in win3.1

    Public Const DM_GETDEFID As Integer = WM_USER + 0
    Public Const DM_SETDEFID As Integer = WM_USER + 1
    Public Const DC_HASDEFID As Short = &H534S '0x534B

    ' Dialog Codes
    Public Const DLGC_WANTARROWS As Short = &H1S '  Control wants arrow keys
    Public Const DLGC_WANTTAB As Short = &H2S '  Control wants tab keys
    Public Const DLGC_WANTALLKEYS As Short = &H4S '  Control wants all keys
    Public Const DLGC_WANTMESSAGE As Short = &H4S '  Pass message to control
    Public Const DLGC_HASSETSEL As Short = &H8S '  Understands EM_SETSEL message
    Public Const DLGC_DEFPUSHBUTTON As Short = &H10S '  Default pushbutton
    Public Const DLGC_UNDEFPUSHBUTTON As Short = &H20S '  Non-default pushbutton
    Public Const DLGC_RADIOBUTTON As Short = &H40S '  Radio button
    Public Const DLGC_WANTCHARS As Short = &H80S '  Want WM_CHAR messages
    Public Const DLGC_STATIC As Short = &H100S '  Static item: don't include
    Public Const DLGC_BUTTON As Short = &H2000S '  Button item: can be checked

    Public Const LB_CTLCODE As Short = 0

    ' Listbox Return Values
    Public Const LB_OKAY As Short = 0
    Public Const LB_ERR As Short = (-1)
    Public Const LB_ERRSPACE As Short = (-2)

    ' The idStaticPath parameter to DlgDirList can have the following values
    ' ORed if the list box should show other details of the files along with
    ' the name of the files;
    ' all other details also will be returned

    ' Listbox Notification Codes
    Public Const LBN_ERRSPACE As Short = (-2)
    Public Const LBN_SELCHANGE As Short = 1
    Public Const LBN_DBLCLK As Short = 2
    Public Const LBN_SELCANCEL As Short = 3
    Public Const LBN_SETFOCUS As Short = 4
    Public Const LBN_KILLFOCUS As Short = 5

    ' Listbox messages
    Public Const LB_ADDSTRING As Short = &H180S
    Public Const LB_INSERTSTRING As Short = &H181S
    Public Const LB_DELETESTRING As Short = &H182S
    Public Const LB_SELITEMRANGEEX As Short = &H183S
    Public Const LB_RESETCONTENT As Short = &H184S
    Public Const LB_SETSEL As Short = &H185S
    Public Const LB_SETCURSEL As Short = &H186S
    Public Const LB_GETSEL As Short = &H187S
    Public Const LB_GETCURSEL As Short = &H188S
    Public Const LB_GETTEXT As Short = &H189S
    Public Const LB_GETTEXTLEN As Short = &H18AS
    Public Const LB_GETCOUNT As Short = &H18BS
    Public Const LB_SELECTSTRING As Short = &H18CS
    Public Const LB_DIR As Short = &H18DS
    Public Const LB_GETTOPINDEX As Short = &H18ES
    Public Const LB_FINDSTRING As Short = &H18FS
    Public Const LB_GETSELCOUNT As Short = &H190S
    Public Const LB_GETSELITEMS As Short = &H191S
    Public Const LB_SETTABSTOPS As Short = &H192S
    Public Const LB_GETHORIZONTALEXTENT As Short = &H193S
    Public Const LB_SETHORIZONTALEXTENT As Short = &H194S
    Public Const LB_SETCOLUMNWIDTH As Short = &H195S
    Public Const LB_ADDFILE As Short = &H196S
    Public Const LB_SETTOPINDEX As Short = &H197S
    Public Const LB_GETITEMRECT As Short = &H198S
    Public Const LB_GETITEMDATA As Short = &H199S
    Public Const LB_SETITEMDATA As Short = &H19AS
    Public Const LB_SELITEMRANGE As Short = &H19BS
    Public Const LB_SETANCHORINDEX As Short = &H19CS
    Public Const LB_GETANCHORINDEX As Short = &H19DS
    Public Const LB_SETCARETINDEX As Short = &H19ES
    Public Const LB_GETCARETINDEX As Short = &H19FS
    Public Const LB_SETITEMHEIGHT As Short = &H1A0S
    Public Const LB_GETITEMHEIGHT As Short = &H1A1S
    Public Const LB_FINDSTRINGEXACT As Short = &H1A2S
    Public Const LB_SETLOCALE As Short = &H1A5S
    Public Const LB_GETLOCALE As Short = &H1A6S
    Public Const LB_SETCOUNT As Short = &H1A7S
    Public Const LB_MSGMAX As Short = &H1A8S

    ' Listbox Styles
    Public Const LBS_NOTIFY As Integer = &H1
    Public Const LBS_SORT As Integer = &H2
    Public Const LBS_NOREDRAW As Integer = &H4
    Public Const LBS_MULTIPLESEL As Integer = &H8
    Public Const LBS_OWNERDRAWFIXED As Integer = &H10
    Public Const LBS_OWNERDRAWVARIABLE As Integer = &H20
    Public Const LBS_HASSTRINGS As Integer = &H40
    Public Const LBS_USETABSTOPS As Integer = &H80
    Public Const LBS_NOINTEGRALHEIGHT As Integer = &H100
    Public Const LBS_MULTICOLUMN As Integer = &H200
    Public Const LBS_WANTKEYBOARDINPUT As Integer = &H400
    Public Const LBS_EXTENDEDSEL As Integer = &H800
    Public Const LBS_DISABLENOSCROLL As Integer = &H1000
    Public Const LBS_NODATA As Integer = &H2000
    Public Const LBS_STANDARD As Boolean = (LBS_NOTIFY Or LBS_SORT Or WS_VSCROLL Or WS_BORDER)

    ' Combo Box return Values
    Public Const CB_OKAY As Short = 0
    Public Const CB_ERR As Short = (-1)
    Public Const CB_ERRSPACE As Short = (-2)

    ' Combo Box Notification Codes
    Public Const CBN_ERRSPACE As Short = (-1)
    Public Const CBN_SELCHANGE As Short = 1
    Public Const CBN_DBLCLK As Short = 2
    Public Const CBN_SETFOCUS As Short = 3
    Public Const CBN_KILLFOCUS As Short = 4
    Public Const CBN_EDITCHANGE As Short = 5
    Public Const CBN_EDITUPDATE As Short = 6
    Public Const CBN_DROPDOWN As Short = 7
    Public Const CBN_CLOSEUP As Short = 8
    Public Const CBN_SELENDOK As Short = 9
    Public Const CBN_SELENDCANCEL As Short = 10

    ' Combo Box styles
    Public Const CBS_SIMPLE As Integer = &H1
    Public Const CBS_DROPDOWN As Integer = &H2
    Public Const CBS_DROPDOWNLIST As Integer = &H3
    Public Const CBS_OWNERDRAWFIXED As Integer = &H10
    Public Const CBS_OWNERDRAWVARIABLE As Integer = &H20
    Public Const CBS_AUTOHSCROLL As Integer = &H40
    Public Const CBS_OEMCONVERT As Integer = &H80
    Public Const CBS_SORT As Integer = &H100
    Public Const CBS_HASSTRINGS As Integer = &H200
    Public Const CBS_NOINTEGRALHEIGHT As Integer = &H400
    Public Const CBS_DISABLENOSCROLL As Integer = &H800

    ' Combo Box messages
    Public Const CB_GETEDITSEL As Short = &H140S
    Public Const CB_LIMITTEXT As Short = &H141S
    Public Const CB_SETEDITSEL As Short = &H142S
    Public Const CB_ADDSTRING As Short = &H143S
    Public Const CB_DELETESTRING As Short = &H144S
    Public Const CB_DIR As Short = &H145S
    Public Const CB_GETCOUNT As Short = &H146S
    Public Const CB_GETCURSEL As Short = &H147S
    Public Const CB_GETLBTEXT As Short = &H148S
    Public Const CB_GETLBTEXTLEN As Short = &H149S
    Public Const CB_INSERTSTRING As Short = &H14AS
    Public Const CB_RESETCONTENT As Short = &H14BS
    Public Const CB_FINDSTRING As Short = &H14CS
    Public Const CB_SELECTSTRING As Short = &H14DS
    Public Const CB_SETCURSEL As Short = &H14ES
    Public Const CB_SHOWDROPDOWN As Short = &H14FS
    Public Const CB_GETITEMDATA As Short = &H150S
    Public Const CB_SETITEMDATA As Short = &H151S
    Public Const CB_GETDROPPEDCONTROLRECT As Short = &H152S
    Public Const CB_SETITEMHEIGHT As Short = &H153S
    Public Const CB_GETITEMHEIGHT As Short = &H154S
    Public Const CB_SETEXTENDEDUI As Short = &H155S
    Public Const CB_GETEXTENDEDUI As Short = &H156S
    Public Const CB_GETDROPPEDSTATE As Short = &H157S
    Public Const CB_FINDSTRINGEXACT As Short = &H158S
    Public Const CB_SETLOCALE As Short = &H159S
    Public Const CB_GETLOCALE As Short = &H15AS
    Public Const CB_MSGMAX As Short = &H15BS

    ' Scroll Bar Styles
    Public Const SBS_HORZ As Integer = &H0
    Public Const SBS_VERT As Integer = &H1
    Public Const SBS_TOPALIGN As Integer = &H2
    Public Const SBS_LEFTALIGN As Integer = &H2
    Public Const SBS_BOTTOMALIGN As Integer = &H4
    Public Const SBS_RIGHTALIGN As Integer = &H4
    Public Const SBS_SIZEBOXTOPLEFTALIGN As Integer = &H2
    Public Const SBS_SIZEBOXBOTTOMRIGHTALIGN As Integer = &H4
    Public Const SBS_SIZEBOX As Integer = &H8

    '  Scroll bar messages
    Public Const SBM_SETPOS As Short = &HE0S ' not in win3.1
    Public Const SBM_GETPOS As Short = &HE1S ' not in win3.1
    Public Const SBM_SETRANGE As Short = &HE2S ' not in win3.1
    Public Const SBM_SETRANGEREDRAW As Short = &HE6S ' not in win3.1
    Public Const SBM_GETRANGE As Short = &HE3S ' not in win3.1
    Public Const SBM_ENABLE_ARROWS As Short = &HE4S ' not in win3.1

    Public Const MDIS_ALLCHILDSTYLES As Short = &H1S

    ' wParam values for WM_MDITILE and WM_MDICASCADE messages.
    Public Const MDITILE_VERTICAL As Short = &H0S
    Public Const MDITILE_HORIZONTAL As Short = &H1S
    Public Const MDITILE_SKIPDISABLED As Short = &H2S

    Public Structure MDICREATESTRUCT
        Dim szClass As String
        Dim szTitle As String
        Dim hOwner As Integer
        Dim X As Integer
        Dim Y As Integer
        Dim cx As Integer
        Dim cy As Integer
        Dim style As Integer
        Dim lParam As Integer
    End Structure

    Public Structure CLIENTCREATESTRUCT
        Dim hWindowMenu As Integer
        Dim idFirstChild As Integer
    End Structure

    Public Declare Function DefFrameProc Lib "user32" Alias "DefFrameProcA" (hWnd As Integer, hWndMDIClient As Integer, wMsg As Integer, wParam As Integer, lParam As Integer) As Integer
    Public Declare Function DefMDIChildProc Lib "user32" Alias "DefMDIChildProcA" (hWnd As Integer, wMsg As Integer, wParam As Integer, lParam As Integer) As Integer

    'UPGRADE_WARNING: ?? Msg ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function TranslateMDISysAccel Lib "user32" (hWndClient As Integer, ByRef lpMsg As Msg) As Integer

    Public Declare Function ArrangeIconicWindows Lib "user32" (hWnd As Integer) As Integer

    Public Declare Function CreateMDIWindow Lib "user32" Alias "CreateMDIWindowA" (lpClassName As String, lpWindowName As String, dwStyle As Integer, X As Integer, Y As Integer, nWidth As Integer, nHeight As Integer, hwndParent As Integer, hInstance As Integer, lParam As Integer) As Integer

    '  Help engine section.

    Public Structure MULTIKEYHELP
        Dim mkSize As Integer
        Dim mkKeylist As Byte
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(253), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=253)> Public szKeyphrase() As Char ' Array length is arbitrary; may be changed
    End Structure

    Public Structure HELPWININFO
        Dim wStructSize As Integer
        Dim X As Integer
        Dim Y As Integer
        Dim dx As Integer
        Dim dy As Integer
        Dim wMax As Integer
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(2), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=2)> Public rgchMember() As Char
    End Structure

    ' Commands to pass WinHelp()
    Public Const HELP_CONTEXT As Short = &H1S '  Display topic in ulTopic
    Public Const HELP_QUIT As Short = &H2S '  Terminate help
    Public Const HELP_INDEX As Short = &H3S '  Display index
    Public Const HELP_CONTENTS As Integer = &H3
    Public Const HELP_HELPONHELP As Short = &H4S '  Display help on using help
    Public Const HELP_SETINDEX As Short = &H5S '  Set current Index for multi index help
    Public Const HELP_SETCONTENTS As Integer = &H5
    Public Const HELP_CONTEXTPOPUP As Integer = &H8
    Public Const HELP_FORCEFILE As Integer = &H9
    Public Const HELP_KEY As Short = &H101S '  Display topic for keyword in offabData
    Public Const HELP_COMMAND As Integer = &H102
    Public Const HELP_PARTIALKEY As Integer = &H105
    Public Const HELP_MULTIKEY As Integer = &H201
    Public Const HELP_SETWINPOS As Integer = &H203

    Public Declare Function WinHelp Lib "user32" Alias "WinHelpA" (hWnd As Integer, lpHelpFile As String, wCommand As Integer, dwData As Integer) As Integer

    ' Parameter for SystemParametersInfo()
    Public Const SPI_GETBEEP As Short = 1
    Public Const SPI_SETBEEP As Short = 2
    Public Const SPI_GETMOUSE As Short = 3
    Public Const SPI_SETMOUSE As Short = 4
    Public Const SPI_GETBORDER As Short = 5
    Public Const SPI_SETBORDER As Short = 6
    Public Const SPI_GETKEYBOARDSPEED As Short = 10
    Public Const SPI_SETKEYBOARDSPEED As Short = 11
    Public Const SPI_LANGDRIVER As Short = 12
    Public Const SPI_ICONHORIZONTALSPACING As Short = 13
    Public Const SPI_GETSCREENSAVETIMEOUT As Short = 14
    Public Const SPI_SETSCREENSAVETIMEOUT As Short = 15
    Public Const SPI_GETSCREENSAVEACTIVE As Short = 16
    Public Const SPI_SETSCREENSAVEACTIVE As Short = 17
    Public Const SPI_GETGRIDGRANULARITY As Short = 18
    Public Const SPI_SETGRIDGRANULARITY As Short = 19
    Public Const SPI_SETDESKWALLPAPER As Short = 20
    Public Const SPI_SETDESKPATTERN As Short = 21
    Public Const SPI_GETKEYBOARDDELAY As Short = 22
    Public Const SPI_SETKEYBOARDDELAY As Short = 23
    Public Const SPI_ICONVERTICALSPACING As Short = 24
    Public Const SPI_GETICONTITLEWRAP As Short = 25
    Public Const SPI_SETICONTITLEWRAP As Short = 26
    Public Const SPI_GETMENUDROPALIGNMENT As Short = 27
    Public Const SPI_SETMENUDROPALIGNMENT As Short = 28
    Public Const SPI_SETDOUBLECLKWIDTH As Short = 29
    Public Const SPI_SETDOUBLECLKHEIGHT As Short = 30
    Public Const SPI_GETICONTITLELOGFONT As Short = 31
    Public Const SPI_SETDOUBLECLICKTIME As Short = 32
    Public Const SPI_SETMOUSEBUTTONSWAP As Short = 33
    Public Const SPI_SETICONTITLELOGFONT As Short = 34
    Public Const SPI_GETFASTTASKSWITCH As Short = 35
    Public Const SPI_SETFASTTASKSWITCH As Short = 36
    Public Const SPI_SETDRAGFULLWINDOWS As Short = 37
    Public Const SPI_GETDRAGFULLWINDOWS As Short = 38
    Public Const SPI_GETNONCLIENTMETRICS As Short = 41
    Public Const SPI_SETNONCLIENTMETRICS As Short = 42
    Public Const SPI_GETMINIMIZEDMETRICS As Short = 43
    Public Const SPI_SETMINIMIZEDMETRICS As Short = 44
    Public Const SPI_GETICONMETRICS As Short = 45
    Public Const SPI_SETICONMETRICS As Short = 46
    Public Const SPI_SETWORKAREA As Short = 47
    Public Const SPI_GETWORKAREA As Short = 48
    Public Const SPI_SETPENWINDOWS As Short = 49
    Public Const SPI_GETFILTERKEYS As Short = 50
    Public Const SPI_SETFILTERKEYS As Short = 51
    Public Const SPI_GETTOGGLEKEYS As Short = 52
    Public Const SPI_SETTOGGLEKEYS As Short = 53
    Public Const SPI_GETMOUSEKEYS As Short = 54
    Public Const SPI_SETMOUSEKEYS As Short = 55
    Public Const SPI_GETSHOWSOUNDS As Short = 56
    Public Const SPI_SETSHOWSOUNDS As Short = 57
    Public Const SPI_GETSTICKYKEYS As Short = 58
    Public Const SPI_SETSTICKYKEYS As Short = 59
    Public Const SPI_GETACCESSTIMEOUT As Short = 60
    Public Const SPI_SETACCESSTIMEOUT As Short = 61
    Public Const SPI_GETSERIALKEYS As Short = 62
    Public Const SPI_SETSERIALKEYS As Short = 63
    Public Const SPI_GETSOUNDSENTRY As Short = 64
    Public Const SPI_SETSOUNDSENTRY As Short = 65
    Public Const SPI_GETHIGHCONTRAST As Short = 66
    Public Const SPI_SETHIGHCONTRAST As Short = 67
    Public Const SPI_GETKEYBOARDPREF As Short = 68
    Public Const SPI_SETKEYBOARDPREF As Short = 69
    Public Const SPI_GETSCREENREADER As Short = 70
    Public Const SPI_SETSCREENREADER As Short = 71
    Public Const SPI_GETANIMATION As Short = 72
    Public Const SPI_SETANIMATION As Short = 73
    Public Const SPI_GETFONTSMOOTHING As Short = 74
    Public Const SPI_SETFONTSMOOTHING As Short = 75
    Public Const SPI_SETDRAGWIDTH As Short = 76
    Public Const SPI_SETDRAGHEIGHT As Short = 77
    Public Const SPI_SETHANDHELD As Short = 78
    Public Const SPI_GETLOWPOWERTIMEOUT As Short = 79
    Public Const SPI_GETPOWEROFFTIMEOUT As Short = 80
    Public Const SPI_SETLOWPOWERTIMEOUT As Short = 81
    Public Const SPI_SETPOWEROFFTIMEOUT As Short = 82
    Public Const SPI_GETLOWPOWERACTIVE As Short = 83
    Public Const SPI_GETPOWEROFFACTIVE As Short = 84
    Public Const SPI_SETLOWPOWERACTIVE As Short = 85
    Public Const SPI_SETPOWEROFFACTIVE As Short = 86
    Public Const SPI_SETCURSORS As Short = 87
    Public Const SPI_SETICONS As Short = 88
    Public Const SPI_GETDEFAULTINPUTLANG As Short = 89
    Public Const SPI_SETDEFAULTINPUTLANG As Short = 90
    Public Const SPI_SETLANGTOGGLE As Short = 91
    Public Const SPI_GETWINDOWSEXTENSION As Short = 92
    Public Const SPI_SETMOUSETRAILS As Short = 93
    Public Const SPI_GETMOUSETRAILS As Short = 94
    Public Const SPI_SCREENSAVERRUNNING As Short = 97

    ' SystemParametersInfo flags
    Public Const SPIF_UPDATEINIFILE As Short = &H1S
    Public Const SPIF_SENDWININICHANGE As Short = &H2S

    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function SystemParametersInfo Lib "user32" Alias "SystemParametersInfoA" (uAction As Integer, uParam As Integer, lpvParam As Object, fuWinIni As Integer) As Integer

    '  DDE window messages
    Public Const WM_DDE_FIRST As Short = &H3E0S
    Public Const WM_DDE_INITIATE As Short = (WM_DDE_FIRST)
    Public Const WM_DDE_TERMINATE As Integer = (WM_DDE_FIRST + 1)
    Public Const WM_DDE_ADVISE As Integer = (WM_DDE_FIRST + 2)
    Public Const WM_DDE_UNADVISE As Integer = (WM_DDE_FIRST + 3)
    Public Const WM_DDE_ACK As Integer = (WM_DDE_FIRST + 4)
    Public Const WM_DDE_DATA As Integer = (WM_DDE_FIRST + 5)
    Public Const WM_DDE_REQUEST As Integer = (WM_DDE_FIRST + 6)
    Public Const WM_DDE_POKE As Integer = (WM_DDE_FIRST + 7)
    Public Const WM_DDE_EXECUTE As Integer = (WM_DDE_FIRST + 8)
    Public Const WM_DDE_LAST As Integer = (WM_DDE_FIRST + 8)

    ' *****************************************************************************                                                                             *
    ' * dde.h -       Dynamic Data Exchange structures and definitions              *
    ' *                                                                             *
    ' * Copyright (c) 1993-1995, Microsoft Corp.        All rights reserved              *
    ' *                                                                             *
    ' \*****************************************************************************/


    ' ----------------------------------------------------------------------------
    '        DDEACK structure
    '
    '         Public Structure of wStatus (LOWORD(lParam)) in WM_DDE_ACK message
    '        sent in response to a WM_DDE_DATA, WM_DDE_REQUEST, WM_DDE_POKE,
    '        WM_DDE_ADVISE, or WM_DDE_UNADVISE message.
    '
    ' ----------------------------------------------------------------------------*/

    Public Structure DDEACK
        Dim bAppReturnCode As Short
        Dim Reserved As Short
        Dim fbusy As Short
        Dim fAck As Short
    End Structure

    ' ----------------------------------------------------------------------------
    '        DDEADVISE structure
    '
    '         WM_DDE_ADVISE parameter Public Structure for hOptions (LOWORD(lParam))
    '
    ' ----------------------------------------------------------------------------*/

    Public Structure DDEADVISE
        Dim Reserved As Short
        Dim fDeferUpd As Short
        Dim fAckReq As Short
        Dim cfFormat As Short
    End Structure


    ' ----------------------------------------------------------------------------
    '        DDEDATA structure
    '
    '        WM_DDE_DATA parameter Public Structure for hData (LOWORD(lParam)).
    '        The actual size of this Public Structure depends on the size of
    '        the Value array.
    '
    ' ----------------------------------------------------------------------------*/

    Public Structure DDEDATA
        Dim unused As Short
        Dim fresponse As Short
        Dim fRelease As Short
        Dim Reserved As Short
        Dim fAckReq As Short
        Dim cfFormat As Short
        <VBFixedArray(1)> Dim Value() As Byte

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim Value(1)
        End Sub
    End Structure


    ' ----------------------------------------------------------------------------
    '         DDEPOKE structure
    '
    '         WM_DDE_POKE parameter Public Structure for hData (LOWORD(lParam)).
    '        The actual size of this Public Structure depends on the size of
    '        the Value array.
    '
    ' ----------------------------------------------------------------------------*/

    Public Structure DDEPOKE
        Dim unused As Short
        Dim fRelease As Short
        Dim fReserved As Short
        Dim cfFormat As Short
        <VBFixedArray(1)> Dim Value() As Byte

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim Value(1)
        End Sub
    End Structure

    ' ----------------------------------------------------------------------------
    ' The following typedef's were used in previous versions of the Windows SDK.
    ' They are still valid.  The above typedef's define exactly the same structures
    ' as those below.  The above typedef names are recommended, however, as they
    ' are more meaningful.

    ' Note that the DDEPOKE Public Structure typedef'ed in earlier versions of DDE.H did
    ' not correctly define the bit positions.
    ' ----------------------------------------------------------------------------*/

    Public Structure DDELN
        Dim unused As Short
        Dim fRelease As Short
        Dim fDeferUpd As Short
        Dim fAckReq As Short
        Dim cfFormat As Short
    End Structure

    Public Structure DDEUP
        Dim unused As Short
        Dim fAck As Short
        Dim fRelease As Short
        Dim fReserved As Short
        Dim fAckReq As Short
        Dim cfFormat As Short
        'UPGRADE_NOTE: rgb ???? rgb_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
        <VBFixedArray(1)> Dim rgb_Renamed() As Byte
    End Structure

    'UPGRADE_WARNING: ?? SECURITY_QUALITY_OF_SERVICE ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? SECURITY_QUALITY_OF_SERVICE ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function DdeSetQualityOfService Lib "user32" (hWndClient As Integer, ByRef pqosNew As SECURITY_QUALITY_OF_SERVICE, ByRef pqosPrev As SECURITY_QUALITY_OF_SERVICE) As Integer
    Public Declare Function ImpersonateDdeClientWindow Lib "user32" (hWndClient As Integer, hWndServer As Integer) As Integer
    Public Declare Function PackDDElParam Lib "user32" (Msg As Integer, uiLo As Integer, uiHi As Integer) As Integer
    Public Declare Function UnpackDDElParam Lib "user32" (Msg As Integer, lParam As Integer, ByRef puiLo As Integer, ByRef puiHi As Integer) As Integer
    Public Declare Function FreeDDElParam Lib "user32" (Msg As Integer, lParam As Integer) As Integer
    Public Declare Function ReuseDDElParam Lib "user32" (lParam As Integer, msgIn As Integer, msgOut As Integer, uiLo As Integer, uiHi As Integer) As Integer

    Public Structure HSZPAIR
        Dim hszSvc As Integer
        Dim hszTopic As Integer
    End Structure

    '//
    '// Quality Of Service
    '//

    Public Structure SECURITY_QUALITY_OF_SERVICE
        Dim Length As Integer
        Dim Impersonationlevel As Short
        Dim ContextTrackingMode As Short
        Dim EffectiveOnly As Integer
    End Structure

    Public Structure CONVCONTEXT
        Dim cb As Integer
        Dim wFlags As Integer
        Dim wCountryID As Integer
        Dim iCodePage As Integer
        Dim dwLangID As Integer
        Dim dwSecurity As Integer
        Dim qos As SECURITY_QUALITY_OF_SERVICE
    End Structure

    Public Structure CONVINFO
        Dim cb As Integer
        Dim hUser As Integer
        Dim hConvPartner As Integer
        Dim hszSvcPartner As Integer
        Dim hszServiceReq As Integer
        Dim hszTopic As Integer
        Dim hszItem As Integer
        Dim wFmt As Integer
        Dim wType As Integer
        Dim wStatus As Integer
        Dim wConvst As Integer
        Dim wLastError As Integer
        Dim hConvList As Integer
        Dim ConvCtxt As CONVCONTEXT
        Dim hWnd As Integer
        Dim hwndPartner As Integer
    End Structure

    '  conversation states (usState)
    Public Const XST_NULL As Short = 0 '  quiescent states
    Public Const XST_INCOMPLETE As Short = 1
    Public Const XST_CONNECTED As Short = 2
    Public Const XST_INIT1 As Short = 3 '  mid-initiation states
    Public Const XST_INIT2 As Short = 4
    Public Const XST_REQSENT As Short = 5 '  active conversation states
    Public Const XST_DATARCVD As Short = 6
    Public Const XST_POKESENT As Short = 7
    Public Const XST_POKEACKRCVD As Short = 8
    Public Const XST_EXECSENT As Short = 9
    Public Const XST_EXECACKRCVD As Short = 10
    Public Const XST_ADVSENT As Short = 11
    Public Const XST_UNADVSENT As Short = 12
    Public Const XST_ADVACKRCVD As Short = 13
    Public Const XST_UNADVACKRCVD As Short = 14
    Public Const XST_ADVDATASENT As Short = 15
    Public Const XST_ADVDATAACKRCVD As Short = 16

    '  used in LOWORD(dwData1) of XTYP_ADVREQ callbacks...
    Public Const CADV_LATEACK As Short = &HFFFFS

    '  conversation status bits (fsStatus)
    Public Const ST_CONNECTED As Short = &H1S
    Public Const ST_ADVISE As Short = &H2S
    Public Const ST_ISLOCAL As Short = &H4S
    Public Const ST_BLOCKED As Short = &H8S
    Public Const ST_CLIENT As Short = &H10S
    Public Const ST_TERMINATED As Short = &H20S
    Public Const ST_INLIST As Short = &H40S
    Public Const ST_BLOCKNEXT As Short = &H80S
    Public Const ST_ISSELF As Short = &H100S

    '  DDE constants for wStatus field
    Public Const DDE_FACK As Short = &H8000S
    Public Const DDE_FBUSY As Short = &H4000S
    Public Const DDE_FDEFERUPD As Short = &H4000S
    Public Const DDE_FACKREQ As Short = &H8000S
    Public Const DDE_FRELEASE As Short = &H2000S
    Public Const DDE_FREQUESTED As Short = &H1000S
    Public Const DDE_FAPPSTATUS As Short = &HFFS
    Public Const DDE_FNOTPROCESSED As Short = &H0S

    Public Const DDE_FACKRESERVED As Boolean = (Not (DDE_FACK Or DDE_FBUSY Or DDE_FAPPSTATUS))
    Public Const DDE_FADVRESERVED As Boolean = (Not (DDE_FACKREQ Or DDE_FDEFERUPD))
    Public Const DDE_FDATRESERVED As Boolean = (Not (DDE_FACKREQ Or DDE_FRELEASE Or DDE_FREQUESTED))
    Public Const DDE_FPOKRESERVED As Boolean = (Not (DDE_FRELEASE))

    '  message filter hook types
    Public Const MSGF_DDEMGR As Short = &H8001S

    '  codepage constants
    Public Const CP_WINANSI As Short = 1004 '  default codepage for windows old DDE convs.
    Public Const CP_WINUNICODE As Short = 1200

    '  transaction types
    Public Const XTYPF_NOBLOCK As Short = &H2S '  CBR_BLOCK will not work
    Public Const XTYPF_NODATA As Short = &H4S '  DDE_FDEFERUPD
    Public Const XTYPF_ACKREQ As Short = &H8S '  DDE_FACKREQ

    Public Const XCLASS_MASK As Short = &HFC00S
    Public Const XCLASS_BOOL As Short = &H1000S
    Public Const XCLASS_DATA As Short = &H2000S
    Public Const XCLASS_FLAGS As Short = &H4000S
    Public Const XCLASS_NOTIFICATION As Short = &H8000S

    Public Const XTYP_ERROR As Boolean = (&H0S Or XCLASS_NOTIFICATION Or XTYPF_NOBLOCK)
    Public Const XTYP_ADVDATA As Boolean = (&H10S Or XCLASS_FLAGS)
    Public Const XTYP_ADVREQ As Boolean = (&H20S Or XCLASS_DATA Or XTYPF_NOBLOCK)
    Public Const XTYP_ADVSTART As Boolean = (&H30S Or XCLASS_BOOL)
    Public Const XTYP_ADVSTOP As Boolean = (&H40S Or XCLASS_NOTIFICATION)
    Public Const XTYP_EXECUTE As Boolean = (&H50S Or XCLASS_FLAGS)
    Public Const XTYP_CONNECT As Boolean = (&H60S Or XCLASS_BOOL Or XTYPF_NOBLOCK)
    Public Const XTYP_CONNECT_CONFIRM As Boolean = (&H70S Or XCLASS_NOTIFICATION Or XTYPF_NOBLOCK)
    Public Const XTYP_XACT_COMPLETE As Boolean = (&H80S Or XCLASS_NOTIFICATION)
    Public Const XTYP_POKE As Boolean = (&H90S Or XCLASS_FLAGS)
    Public Const XTYP_REGISTER As Boolean = (&HA0S Or XCLASS_NOTIFICATION Or XTYPF_NOBLOCK)
    Public Const XTYP_REQUEST As Boolean = (&HB0S Or XCLASS_DATA)
    Public Const XTYP_DISCONNECT As Boolean = (&HC0S Or XCLASS_NOTIFICATION Or XTYPF_NOBLOCK)
    Public Const XTYP_UNREGISTER As Boolean = (&HD0S Or XCLASS_NOTIFICATION Or XTYPF_NOBLOCK)
    Public Const XTYP_WILDCONNECT As Boolean = (&HE0S Or XCLASS_DATA Or XTYPF_NOBLOCK)

    Public Const XTYP_MASK As Short = &HF0S
    Public Const XTYP_SHIFT As Short = 4 '  shift to turn XTYP_ into an index

    '  Timeout constants
    Public Const TIMEOUT_ASYNC As Short = &HFFFFS

    '  Transaction ID constants
    Public Const QID_SYNC As Short = &HFFFFS

    '  public strings used in DDE
    Public Const SZDDESYS_TOPIC As String = "System"
    Public Const SZDDESYS_ITEM_TOPICS As String = "Topics"
    Public Const SZDDESYS_ITEM_SYSITEMS As String = "SysItems"
    Public Const SZDDESYS_ITEM_RTNMSG As String = "ReturnMessage"
    Public Const SZDDESYS_ITEM_STATUS As String = "Status"
    Public Const SZDDESYS_ITEM_FORMATS As String = "Formats"
    Public Const SZDDESYS_ITEM_HELP As String = "Help"
    Public Const SZDDE_ITEM_ITEMLIST As String = "TopicItemList"

    Public Const CBR_BLOCK As Short = &HFFFFS

    ' Callback filter flags for use with standard apps.
    Public Const CBF_FAIL_SELFCONNECTIONS As Short = &H1000S
    Public Const CBF_FAIL_CONNECTIONS As Short = &H2000S
    Public Const CBF_FAIL_ADVISES As Short = &H4000S
    Public Const CBF_FAIL_EXECUTES As Short = &H8000S
    Public Const CBF_FAIL_POKES As Integer = &H10000
    Public Const CBF_FAIL_REQUESTS As Integer = &H20000
    Public Const CBF_FAIL_ALLSVRXACTIONS As Integer = &H3F000

    Public Const CBF_SKIP_CONNECT_CONFIRMS As Integer = &H40000
    Public Const CBF_SKIP_REGISTRATIONS As Integer = &H80000
    Public Const CBF_SKIP_UNREGISTRATIONS As Integer = &H100000
    Public Const CBF_SKIP_DISCONNECTS As Integer = &H200000
    Public Const CBF_SKIP_ALLNOTIFICATIONS As Integer = &H3C0000

    ' Application command flags
    Public Const APPCMD_CLIENTONLY As Integer = &H10
    Public Const APPCMD_FILTERINITS As Integer = &H20
    Public Const APPCMD_MASK As Integer = &HFF0

    ' Application classification flags
    Public Const APPCLASS_STANDARD As Integer = &H0
    Public Const APPCLASS_MASK As Integer = &HF

    Public Declare Function DdeUninitialize Lib "user32" (idInst As Integer) As Integer

    ' conversation enumeration functions
    'UPGRADE_WARNING: ?? CONVCONTEXT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function DdeConnectList Lib "user32" (idInst As Integer, hszService As Integer, hszTopic As Integer, hConvList As Integer, ByRef pCC As CONVCONTEXT) As Integer
    Public Declare Function DdeQueryNextServer Lib "user32" (hConvList As Integer, hConvPrev As Integer) As Integer
    Public Declare Function DdeDisconnectList Lib "user32" (hConvList As Integer) As Integer

    ' conversation control functions
    'UPGRADE_WARNING: ?? CONVCONTEXT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function DdeConnect Lib "user32" (idInst As Integer, hszService As Integer, hszTopic As Integer, ByRef pCC As CONVCONTEXT) As Integer
    Public Declare Function DdeDisconnect Lib "user32" (hConv As Integer) As Integer
    Public Declare Function DdeReconnect Lib "user32" (hConv As Integer) As Integer
    'UPGRADE_WARNING: ?? CONVINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function DdeQueryConvInfo Lib "user32" (hConv As Integer, idTransaction As Integer, ByRef pConvInfo As CONVINFO) As Integer
    Public Declare Function DdeSetUserHandle Lib "user32" (hConv As Integer, id As Integer, hUser As Integer) As Integer
    Public Declare Function DdeAbandonTransaction Lib "user32" (idInst As Integer, hConv As Integer, idTransaction As Integer) As Integer

    ' app server interface functions
    Public Declare Function DdePostAdvise Lib "user32" (idInst As Integer, hszTopic As Integer, hszItem As Integer) As Integer
    Public Declare Function DdeEnableCallback Lib "user32" (idInst As Integer, hConv As Integer, wCmd As Integer) As Integer
    Public Declare Function DdeImpersonateClient Lib "user32" (hConv As Integer) As Integer

    Public Const EC_ENABLEALL As Short = 0
    Public Const EC_ENABLEONE As Short = ST_BLOCKNEXT
    Public Const EC_DISABLE As Short = ST_BLOCKED
    Public Const EC_QUERYWAITING As Short = 2

    Public Declare Function DdeNameService Lib "user32" (idInst As Integer, hsz1 As Integer, hsz2 As Integer, afCmd As Integer) As Integer

    Public Const DNS_REGISTER As Short = &H1S
    Public Const DNS_UNREGISTER As Short = &H2S
    Public Const DNS_FILTERON As Short = &H4S
    Public Const DNS_FILTEROFF As Short = &H8S

    ' app client interface functions
    Public Declare Function DdeClientTransaction Lib "user32" (ByRef pData As Byte, cbData As Integer, hConv As Integer, hszItem As Integer, wFmt As Integer, wType As Integer, dwTimeout As Integer, ByRef pdwResult As Integer) As Integer

    ' data transfer functions
    Public Declare Function DdeCreateDataHandle Lib "user32" (idInst As Integer, ByRef pSrc As Byte, cb As Integer, cbOff As Integer, hszItem As Integer, wFmt As Integer, afCmd As Integer) As Integer
    Public Declare Function DdeAddData Lib "user32" Alias "DdeAddDataA" (hData As Integer, ByRef pSrc As Byte, cb As Integer, cbOff As Integer) As Integer
    Public Declare Function DdeGetData Lib "user32" Alias "DdeGetDataA" (hData As Integer, ByRef pDst As Byte, cbMax As Integer, cbOff As Integer) As Integer
    Public Declare Function DdeAccessData Lib "user32" Alias "DdeAccessDataA" (hData As Integer, ByRef pcbDataSize As Integer) As Integer
    Public Declare Function DdeUnaccessData Lib "user32" Alias "DdeUnaccessDataA" (hData As Integer) As Integer
    Public Declare Function DdeFreeDataHandle Lib "user32" (hData As Integer) As Integer

    Public Const HDATA_APPOWNED As Short = &H1S

    Public Declare Function DdeGetLastError Lib "user32" (idInst As Integer) As Integer

    Public Const DMLERR_NO_ERROR As Short = 0 '  must be 0

    Public Const DMLERR_FIRST As Short = &H4000S

    Public Const DMLERR_ADVACKTIMEOUT As Short = &H4000S
    Public Const DMLERR_BUSY As Short = &H4001S
    Public Const DMLERR_DATAACKTIMEOUT As Short = &H4002S
    Public Const DMLERR_DLL_NOT_INITIALIZED As Short = &H4003S
    Public Const DMLERR_DLL_USAGE As Short = &H4004S
    Public Const DMLERR_EXECACKTIMEOUT As Short = &H4005S
    Public Const DMLERR_INVALIDPARAMETER As Short = &H4006S
    Public Const DMLERR_LOW_MEMORY As Short = &H4007S
    Public Const DMLERR_MEMORY_ERROR As Short = &H4008S
    Public Const DMLERR_NOTPROCESSED As Short = &H4009S
    Public Const DMLERR_NO_CONV_ESTABLISHED As Short = &H400AS
    Public Const DMLERR_POKEACKTIMEOUT As Short = &H400BS
    Public Const DMLERR_POSTMSG_FAILED As Short = &H400CS
    Public Const DMLERR_REENTRANCY As Short = &H400DS
    Public Const DMLERR_SERVER_DIED As Short = &H400ES
    Public Const DMLERR_SYS_ERROR As Short = &H400FS
    Public Const DMLERR_UNADVACKTIMEOUT As Short = &H4010S
    Public Const DMLERR_UNFOUND_QUEUE_ID As Short = &H4011S

    Public Const DMLERR_LAST As Short = &H4011S

    Public Declare Function DdeCreateStringHandle Lib "user32" Alias "DdeCreateStringHandleA" (idInst As Integer, psz As String, iCodePage As Integer) As Integer

    Public Declare Function DdeQueryString Lib "user32" Alias "DdeQueryStringA" (idInst As Integer, hsz As Integer, psz As String, cchMax As Integer, iCodePage As Integer) As Integer

    Public Declare Function DdeFreeStringHandle Lib "user32" (idInst As Integer, hsz As Integer) As Integer
    Public Declare Function DdeKeepStringHandle Lib "user32" (idInst As Integer, hsz As Integer) As Integer
    Public Declare Function DdeCmpStringHandles Lib "user32" (hsz1 As Integer, hsz2 As Integer) As Integer

    Public Structure DDEML_MSG_HOOK_DATA '  new for NT
        Dim uiLo As Integer '  unpacked lo and hi parts of lParam
        Dim uiHi As Integer
        Dim cbData As Integer '  amount of data in message, if any. May be > than 32 bytes.
        <VBFixedArray(8)> Dim Data() As Integer '  data peeking by DDESPY is limited to 32 bytes.

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim Data(8)
        End Sub
    End Structure

    Public Structure MONMSGSTRUCT
        Dim cb As Integer
        Dim hwndTo As Integer
        Dim dwTime As Integer
        Dim htask As Integer
        Dim wMsg As Integer
        Dim wParam As Integer
        Dim lParam As Integer
        'UPGRADE_WARNING: ?? dmhd ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"”
        Dim dmhd As DDEML_MSG_HOOK_DATA '  new for NT

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            dmhd.Initialize()
        End Sub
    End Structure

    Public Structure MONCBSTRUCT
        Dim cb As Integer
        Dim dwTime As Integer
        Dim htask As Integer
        Dim dwRet As Integer
        Dim wType As Integer
        Dim wFmt As Integer
        Dim hConv As Integer
        Dim hsz1 As Integer
        Dim hsz2 As Integer
        Dim hData As Integer
        Dim dwData1 As Integer
        Dim dwData2 As Integer
        Dim cc As CONVCONTEXT '  new for NT for XTYP_CONNECT callbacks
        Dim cbData As Integer '  new for NT for data peeking
        <VBFixedArray(8)> Dim Data() As Integer '  new for NT for data peeking

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim Data(8)
        End Sub
    End Structure

    Public Structure MONHSZSTRUCT
        Dim cb As Integer
        Dim fsAction As Integer '  MH_ value
        Dim dwTime As Integer
        Dim hsz As Integer
        Dim htask As Integer
        'UPGRADE_NOTE: str ???? str_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
        Dim str_Renamed As Byte
    End Structure

    Public Const MH_CREATE As Short = 1
    Public Const MH_KEEP As Short = 2
    Public Const MH_DELETE As Short = 3
    Public Const MH_CLEANUP As Short = 4

    Public Structure MONERRSTRUCT
        Dim cb As Integer
        Dim wLastError As Integer
        Dim dwTime As Integer
        Dim htask As Integer
    End Structure

    Public Structure MONLINKSTRUCT
        Dim cb As Integer
        Dim dwTime As Integer
        Dim htask As Integer
        Dim fEstablished As Integer
        Dim fNoData As Integer
        Dim hszSvc As Integer
        Dim hszTopic As Integer
        Dim hszItem As Integer
        Dim wFmt As Integer
        Dim fServer As Integer
        Dim hConvServer As Integer
        Dim hConvClient As Integer
    End Structure

    Public Structure MONCONVSTRUCT
        Dim cb As Integer
        Dim fConnect As Integer
        Dim dwTime As Integer
        Dim htask As Integer
        Dim hszSvc As Integer
        Dim hszTopic As Integer
        Dim hConvClient As Integer '  Globally unique value != apps local hConv
        Dim hConvServer As Integer '  Globally unique value != apps local hConv
    End Structure

    Public Const MAX_MONITORS As Short = 4
    Public Const APPCLASS_MONITOR As Integer = &H1
    Public Const XTYP_MONITOR As Boolean = (&HF0S Or XCLASS_NOTIFICATION Or XTYPF_NOBLOCK)

    ' Callback filter flags for use with MONITOR apps - 0 implies no monitor callbacks
    Public Const MF_HSZ_INFO As Integer = &H1000000
    Public Const MF_SENDMSGS As Integer = &H2000000
    Public Const MF_POSTMSGS As Integer = &H4000000
    Public Const MF_CALLBACKS As Integer = &H8000000
    Public Const MF_ERRORS As Integer = &H10000000
    Public Const MF_LINKS As Integer = &H20000000
    Public Const MF_CONV As Integer = &H40000000

    Public Const MF_MASK As Integer = &HFF000000

    ' -------------------------
    '  MMSystem Section
    ' -------------------------

    ' This section defines all the support for Multimedia applications

    '  general constants
    Public Const MAXPNAMELEN As Short = 32 '  max product name length (including NULL)
    Public Const MAXERRORLENGTH As Short = 128 '  max error text length (including final NULL)

    Public Structure smpte
        'UPGRADE_NOTE: hour ???? hour_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
        Dim hour_Renamed As Byte
        Dim min As Byte
        Dim sec As Byte
        Dim frame As Byte
        Dim fps As Byte
        Dim dummy As Byte
        <VBFixedArray(2)> Dim pad() As Byte

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim pad(2)
        End Sub
    End Structure

    Public Structure midi
        Dim songptrpos As Integer
    End Structure

    Public Structure MMTIME
        Dim wType As Integer
        Dim u As Integer
    End Structure

    '  values for wType field in MMTIME struct
    Public Const TIME_MS As Short = &H1S '  time in Milliseconds
    Public Const TIME_SAMPLES As Short = &H2S '  number of wave samples
    Public Const TIME_BYTES As Short = &H4S '  current byte offset
    Public Const TIME_SMPTE As Short = &H8S '  SMPTE time
    Public Const TIME_MIDI As Short = &H10S '  MIDI time

    '  Multimedia Window Messages
    Public Const MM_JOY1MOVE As Short = &H3A0S '  joystick
    Public Const MM_JOY2MOVE As Short = &H3A1S
    Public Const MM_JOY1ZMOVE As Short = &H3A2S
    Public Const MM_JOY2ZMOVE As Short = &H3A3S
    Public Const MM_JOY1BUTTONDOWN As Short = &H3B5S
    Public Const MM_JOY2BUTTONDOWN As Short = &H3B6S
    Public Const MM_JOY1BUTTONUP As Short = &H3B7S
    Public Const MM_JOY2BUTTONUP As Short = &H3B8S

    Public Const MM_MCINOTIFY As Short = &H3B9S '  MCI
    Public Const MM_MCISYSTEM_STRING As Short = &H3CAS

    Public Const MM_WOM_OPEN As Short = &H3BBS '  waveform output
    Public Const MM_WOM_CLOSE As Short = &H3BCS
    Public Const MM_WOM_DONE As Short = &H3BDS

    Public Const MM_WIM_OPEN As Short = &H3BES '  waveform input
    Public Const MM_WIM_CLOSE As Short = &H3BFS
    Public Const MM_WIM_DATA As Short = &H3C0S

    Public Const MM_MIM_OPEN As Short = &H3C1S '  MIDI input
    Public Const MM_MIM_CLOSE As Short = &H3C2S
    Public Const MM_MIM_DATA As Short = &H3C3S
    Public Const MM_MIM_LONGDATA As Short = &H3C4S
    Public Const MM_MIM_ERROR As Short = &H3C5S
    Public Const MM_MIM_LONGERROR As Short = &H3C6S

    Public Const MM_MOM_OPEN As Short = &H3C7S '  MIDI output
    Public Const MM_MOM_CLOSE As Short = &H3C8S
    Public Const MM_MOM_DONE As Short = &H3C9S

    ' String resource number bases (internal use)

    Public Const MMSYSERR_BASE As Short = 0
    Public Const WAVERR_BASE As Short = 32
    Public Const MIDIERR_BASE As Short = 64
    Public Const TIMERR_BASE As Short = 96 '  was 128, changed to match Win 31 Sonic
    Public Const JOYERR_BASE As Short = 160
    Public Const MCIERR_BASE As Short = 256

    Public Const MCI_STRING_OFFSET As Short = 512 '  if this number is changed you MUST
    '  alter the MCI_DEVTYPE_... list below
    Public Const MCI_VD_OFFSET As Short = 1024
    Public Const MCI_CD_OFFSET As Short = 1088
    Public Const MCI_WAVE_OFFSET As Short = 1152
    Public Const MCI_SEQ_OFFSET As Short = 1216

    ' General error return values
    Public Const MMSYSERR_NOERROR As Short = 0 '  no error
    Public Const MMSYSERR_ERROR As Integer = (MMSYSERR_BASE + 1) '  unspecified error
    Public Const MMSYSERR_BADDEVICEID As Integer = (MMSYSERR_BASE + 2) '  device ID out of range
    Public Const MMSYSERR_NOTENABLED As Integer = (MMSYSERR_BASE + 3) '  driver failed enable
    Public Const MMSYSERR_ALLOCATED As Integer = (MMSYSERR_BASE + 4) '  device already allocated
    Public Const MMSYSERR_INVALHANDLE As Integer = (MMSYSERR_BASE + 5) '  device handle is invalid
    Public Const MMSYSERR_NODRIVER As Integer = (MMSYSERR_BASE + 6) '  no device driver present
    Public Const MMSYSERR_NOMEM As Integer = (MMSYSERR_BASE + 7) '  memory allocation error
    Public Const MMSYSERR_NOTSUPPORTED As Integer = (MMSYSERR_BASE + 8) '  function isn't supported
    Public Const MMSYSERR_BADERRNUM As Integer = (MMSYSERR_BASE + 9) '  error value out of range
    Public Const MMSYSERR_INVALFLAG As Integer = (MMSYSERR_BASE + 10) '  invalid flag passed
    Public Const MMSYSERR_INVALPARAM As Integer = (MMSYSERR_BASE + 11) '  invalid parameter passed
    Public Const MMSYSERR_HANDLEBUSY As Integer = (MMSYSERR_BASE + 12) '  handle being used
    '  simultaneously on another
    '  thread (eg callback)
    Public Const MMSYSERR_INVALIDALIAS As Integer = (MMSYSERR_BASE + 13) '  "Specified alias not found in WIN.INI
    Public Const MMSYSERR_LASTERROR As Integer = (MMSYSERR_BASE + 13) '  last error in range
    Public Const MM_MOM_POSITIONCB As Short = &H3CAS '  Callback for MEVT_POSITIONCB
    Public Const MM_MCISIGNAL As Short = &H3CBS
    Public Const MM_MIM_MOREDATA As Short = &H3CCS '  MIM_DONE w/ pending events
    Public Const MIDICAPS_STREAM As Short = &H8S '  driver supports midiStreamOut directly


    Public Structure MIDIEVENT
        Dim dwDeltaTime As Integer '  Ticks since last event
        Dim dwStreamID As Integer '  Reserved; must be zero
        Dim dwEvent As Integer '  Event type and parameters
        <VBFixedArray(1)> Dim dwParms() As Integer '  Parameters if this is a long event

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim dwParms(1)
        End Sub
    End Structure

    Public Structure MIDISTRMBUFFVER
        Dim dwVersion As Integer '  Stream buffer format version
        Dim dwMid As Integer '  Manufacturer ID as defined in MMREG.H
        Dim dwOEMVersion As Integer '  Manufacturer version for custom ext
    End Structure

    '  Type codes which go in the high byte of the event DWORD of a stream buffer

    '  Type codes 00-7F contain parameters within the low 24 bits
    '  Type codes 80-FF contain a length of their parameter in the low 24
    '  bits, followed by their parameter data in the buffer. The event
    '  DWORD contains the exact byte length; the parm data itself must be
    '  padded to be an even multiple of 4 Byte long.
    '

    Public Const MEVT_F_SHORT As Integer = &H0
    Public Const MEVT_F_LONG As Integer = &H80000000
    Public Const MEVT_F_CALLBACK As Integer = &H40000000
    Public Const MIDISTRM_ERROR As Short = -2

    '
    '  Structures and defines for midiStreamProperty
    '
    Public Const MIDIPROP_SET As Integer = &H80000000
    Public Const MIDIPROP_GET As Integer = &H40000000

    '  These are intentionally both non-zero so the app cannot accidentally
    '  leave the operation off and happen to appear to work due to default
    '  action.

    Public Const MIDIPROP_TIMEDIV As Integer = &H1
    Public Const MIDIPROP_TEMPO As Integer = &H2

    Public Structure MIDIPROPTIMEDIV
        Dim cbStruct As Integer
        Dim dwTimeDiv As Integer
    End Structure

    Public Structure MIDIPROPTEMPO
        Dim cbStruct As Integer
        Dim dwTempo As Integer
    End Structure


    '  MIDI function prototypes *

    ' ***************************************************************************

    '                             Mixer Support

    ' **************************************************************************

    Public Const MIXER_SHORT_NAME_CHARS As Short = 16
    Public Const MIXER_LONG_NAME_CHARS As Short = 64

    '
    '   MMRESULT error return values specific to the mixer API
    '
    '
    Public Const MIXERR_BASE As Short = 1024
    Public Const MIXERR_INVALLINE As Integer = (MIXERR_BASE + 0)
    Public Const MIXERR_INVALCONTROL As Integer = (MIXERR_BASE + 1)
    Public Const MIXERR_INVALVALUE As Integer = (MIXERR_BASE + 2)
    Public Const MIXERR_LASTERROR As Integer = (MIXERR_BASE + 2)


    Public Const MIXER_OBJECTF_HANDLE As Integer = &H80000000
    Public Const MIXER_OBJECTF_MIXER As Integer = &H0
    Public Const MIXER_OBJECTF_HMIXER As Boolean = (MIXER_OBJECTF_HANDLE Or MIXER_OBJECTF_MIXER)
    Public Const MIXER_OBJECTF_WAVEOUT As Integer = &H10000000
    Public Const MIXER_OBJECTF_HWAVEOUT As Boolean = (MIXER_OBJECTF_HANDLE Or MIXER_OBJECTF_WAVEOUT)
    Public Const MIXER_OBJECTF_WAVEIN As Integer = &H20000000
    Public Const MIXER_OBJECTF_HWAVEIN As Boolean = (MIXER_OBJECTF_HANDLE Or MIXER_OBJECTF_WAVEIN)
    Public Const MIXER_OBJECTF_MIDIOUT As Integer = &H30000000
    Public Const MIXER_OBJECTF_HMIDIOUT As Boolean = (MIXER_OBJECTF_HANDLE Or MIXER_OBJECTF_MIDIOUT)
    Public Const MIXER_OBJECTF_MIDIIN As Integer = &H40000000
    Public Const MIXER_OBJECTF_HMIDIIN As Boolean = (MIXER_OBJECTF_HANDLE Or MIXER_OBJECTF_MIDIIN)
    Public Const MIXER_OBJECTF_AUX As Integer = &H50000000

    Public Declare Function mixerGetNumDevs Lib "winmm.dll" () As Integer

    Public Structure MIXERCAPS
        Dim wMid As Short '  manufacturer id
        Dim wPid As Short '  product id
        Dim vDriverVersion As Integer '  version of the driver
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(MAXPNAMELEN), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=MAXPNAMELEN)> Public szPname() As Char '  product name
        Dim fdwSupport As Integer '  misc. support bits
        Dim cDestinations As Integer '  count of destinations
    End Structure

    'UPGRADE_WARNING: ?? MIXERCAPS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function mixerGetDevCaps Lib "winmm.dll" Alias "mixerGetDevCapsA" (uMxId As Integer, pmxcaps As MIXERCAPS, cbmxcaps As Integer) As Integer
    Public Declare Function mixerOpen Lib "winmm.dll" (ByRef phmx As Integer, uMxId As Integer, dwCallback As Integer, dwInstance As Integer, fdwOpen As Integer) As Integer
    Public Declare Function mixerClose Lib "winmm.dll" (hmx As Integer) As Integer
    Public Declare Function mixerMessage Lib "winmm.dll" (hmx As Integer, uMsg As Integer, dwParam1 As Integer, dwParam2 As Integer) As Integer

    Public Structure Target ' for use in MIXERLINE and others (embedded structure)
        Dim dwType As Integer '  MIXERLINE_TARGETTYPE_xxxx
        Dim dwDeviceID As Integer '  target device ID of device type
        Dim wMid As Short '  of target device
        Dim wPid As Short '       "
        Dim vDriverVersion As Integer '       "
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(MAXPNAMELEN), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=MAXPNAMELEN)> Public szPname() As Char
    End Structure

    Public Structure MIXERLINE
        Dim cbStruct As Integer '  size of MIXERLINE structure
        Dim dwDestination As Integer '  zero based destination index
        Dim dwSource As Integer '  zero based source index (if source)
        Dim dwLineID As Integer '  unique line id for mixer device
        Dim fdwLine As Integer '  state/information about line
        Dim dwUser As Integer '  driver specific information
        Dim dwComponentType As Integer '  component type line connects to
        Dim cChannels As Integer '  number of channels line supports
        Dim cConnections As Integer '  number of connections (possible)
        Dim cControls As Integer '  number of controls at this line
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(MIXER_SHORT_NAME_CHARS), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=MIXER_SHORT_NAME_CHARS)> Public szShortName() As Char
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(MIXER_LONG_NAME_CHARS), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=MIXER_LONG_NAME_CHARS)> Public szName() As Char
        Dim lpTarget As Target
    End Structure

    '   MIXERLINE.fdwLine

    Public Const MIXERLINE_LINEF_ACTIVE As Integer = &H1
    Public Const MIXERLINE_LINEF_DISCONNECTED As Integer = &H8000
    Public Const MIXERLINE_LINEF_SOURCE As Integer = &H80000000

    '   MIXERLINE.dwComponentType

    '   component types for destinations and sources

    Public Const MIXERLINE_COMPONENTTYPE_DST_FIRST As Integer = &H0
    Public Const MIXERLINE_COMPONENTTYPE_DST_UNDEFINED As Decimal = (MIXERLINE_COMPONENTTYPE_DST_FIRST + 0)
    Public Const MIXERLINE_COMPONENTTYPE_DST_DIGITAL As Decimal = (MIXERLINE_COMPONENTTYPE_DST_FIRST + 1)
    Public Const MIXERLINE_COMPONENTTYPE_DST_LINE As Decimal = (MIXERLINE_COMPONENTTYPE_DST_FIRST + 2)
    Public Const MIXERLINE_COMPONENTTYPE_DST_MONITOR As Decimal = (MIXERLINE_COMPONENTTYPE_DST_FIRST + 3)
    Public Const MIXERLINE_COMPONENTTYPE_DST_SPEAKERS As Decimal = (MIXERLINE_COMPONENTTYPE_DST_FIRST + 4)
    Public Const MIXERLINE_COMPONENTTYPE_DST_HEADPHONES As Decimal = (MIXERLINE_COMPONENTTYPE_DST_FIRST + 5)
    Public Const MIXERLINE_COMPONENTTYPE_DST_TELEPHONE As Decimal = (MIXERLINE_COMPONENTTYPE_DST_FIRST + 6)
    Public Const MIXERLINE_COMPONENTTYPE_DST_WAVEIN As Decimal = (MIXERLINE_COMPONENTTYPE_DST_FIRST + 7)
    Public Const MIXERLINE_COMPONENTTYPE_DST_VOICEIN As Decimal = (MIXERLINE_COMPONENTTYPE_DST_FIRST + 8)
    Public Const MIXERLINE_COMPONENTTYPE_DST_LAST As Decimal = (MIXERLINE_COMPONENTTYPE_DST_FIRST + 8)

    Public Const MIXERLINE_COMPONENTTYPE_SRC_FIRST As Integer = &H1000
    Public Const MIXERLINE_COMPONENTTYPE_SRC_UNDEFINED As Decimal = (MIXERLINE_COMPONENTTYPE_SRC_FIRST + 0)
    Public Const MIXERLINE_COMPONENTTYPE_SRC_DIGITAL As Decimal = (MIXERLINE_COMPONENTTYPE_SRC_FIRST + 1)
    Public Const MIXERLINE_COMPONENTTYPE_SRC_LINE As Decimal = (MIXERLINE_COMPONENTTYPE_SRC_FIRST + 2)
    Public Const MIXERLINE_COMPONENTTYPE_SRC_MICROPHONE As Decimal = (MIXERLINE_COMPONENTTYPE_SRC_FIRST + 3)
    Public Const MIXERLINE_COMPONENTTYPE_SRC_SYNTHESIZER As Decimal = (MIXERLINE_COMPONENTTYPE_SRC_FIRST + 4)
    Public Const MIXERLINE_COMPONENTTYPE_SRC_COMPACTDISC As Decimal = (MIXERLINE_COMPONENTTYPE_SRC_FIRST + 5)
    Public Const MIXERLINE_COMPONENTTYPE_SRC_TELEPHONE As Decimal = (MIXERLINE_COMPONENTTYPE_SRC_FIRST + 6)
    Public Const MIXERLINE_COMPONENTTYPE_SRC_PCSPEAKER As Decimal = (MIXERLINE_COMPONENTTYPE_SRC_FIRST + 7)
    Public Const MIXERLINE_COMPONENTTYPE_SRC_WAVEOUT As Decimal = (MIXERLINE_COMPONENTTYPE_SRC_FIRST + 8)
    Public Const MIXERLINE_COMPONENTTYPE_SRC_AUXILIARY As Decimal = (MIXERLINE_COMPONENTTYPE_SRC_FIRST + 9)
    Public Const MIXERLINE_COMPONENTTYPE_SRC_ANALOG As Decimal = (MIXERLINE_COMPONENTTYPE_SRC_FIRST + 10)
    Public Const MIXERLINE_COMPONENTTYPE_SRC_LAST As Decimal = (MIXERLINE_COMPONENTTYPE_SRC_FIRST + 10)


    '
    '   MIXERLINE.Target.dwType
    '
    '
    Public Const MIXERLINE_TARGETTYPE_UNDEFINED As Short = 0
    Public Const MIXERLINE_TARGETTYPE_WAVEOUT As Short = 1
    Public Const MIXERLINE_TARGETTYPE_WAVEIN As Short = 2
    Public Const MIXERLINE_TARGETTYPE_MIDIOUT As Short = 3
    Public Const MIXERLINE_TARGETTYPE_MIDIIN As Short = 4
    Public Const MIXERLINE_TARGETTYPE_AUX As Short = 5

    'UPGRADE_WARNING: ?? MIXERLINE ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function mixerGetLineInfo Lib "winmm.dll" Alias "mixerGetLineInfoA" (hmxobj As Integer, ByRef pmxl As MIXERLINE, fdwInfo As Integer) As Integer
    Public Const MIXER_GETLINEINFOF_DESTINATION As Integer = &H0
    Public Const MIXER_GETLINEINFOF_SOURCE As Integer = &H1
    Public Const MIXER_GETLINEINFOF_LINEID As Integer = &H2
    Public Const MIXER_GETLINEINFOF_COMPONENTTYPE As Integer = &H3
    Public Const MIXER_GETLINEINFOF_TARGETTYPE As Integer = &H4
    Public Const MIXER_GETLINEINFOF_QUERYMASK As Integer = &HF

    Public Declare Function mixerGetID Lib "winmm.dll" (hmxobj As Integer, ByRef pumxID As Integer, fdwId As Integer) As Integer

    '   MIXERCONTROL

    Public Structure MIXERCONTROL
        Dim cbStruct As Integer '  size in Byte of MIXERCONTROL
        Dim dwControlID As Integer '  unique control id for mixer device
        Dim dwControlType As Integer '  MIXERCONTROL_CONTROLTYPE_xxx
        Dim fdwControl As Integer '  MIXERCONTROL_CONTROLF_xxx
        Dim cMultipleItems As Integer '  if MIXERCONTROL_CONTROLF_MULTIPLE set
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(MIXER_SHORT_NAME_CHARS), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=MIXER_SHORT_NAME_CHARS)> Public szShortName() As Char
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(MIXER_LONG_NAME_CHARS), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=MIXER_LONG_NAME_CHARS)> Public szName() As Char
        Dim Bounds As Double
        Dim Metrics As Integer
    End Structure
    '
    '   MIXERCONTROL.fdwControl

    Public Const MIXERCONTROL_CONTROLF_UNIFORM As Integer = &H1
    Public Const MIXERCONTROL_CONTROLF_MULTIPLE As Integer = &H2
    Public Const MIXERCONTROL_CONTROLF_DISABLED As Integer = &H80000000

    '   MIXERCONTROL_CONTROLTYPE_xxx building block defines

    Public Const MIXERCONTROL_CT_CLASS_MASK As Integer = &HF0000000
    Public Const MIXERCONTROL_CT_CLASS_CUSTOM As Integer = &H0
    Public Const MIXERCONTROL_CT_CLASS_METER As Integer = &H10000000
    Public Const MIXERCONTROL_CT_CLASS_SWITCH As Integer = &H20000000
    Public Const MIXERCONTROL_CT_CLASS_NUMBER As Integer = &H30000000
    Public Const MIXERCONTROL_CT_CLASS_SLIDER As Integer = &H40000000
    Public Const MIXERCONTROL_CT_CLASS_FADER As Integer = &H50000000
    Public Const MIXERCONTROL_CT_CLASS_TIME As Integer = &H60000000
    Public Const MIXERCONTROL_CT_CLASS_LIST As Integer = &H70000000
    Public Const MIXERCONTROL_CT_SUBCLASS_MASK As Integer = &HF000000
    Public Const MIXERCONTROL_CT_SC_SWITCH_BOOLEAN As Integer = &H0
    Public Const MIXERCONTROL_CT_SC_SWITCH_BUTTON As Integer = &H1000000
    Public Const MIXERCONTROL_CT_SC_METER_POLLED As Integer = &H0
    Public Const MIXERCONTROL_CT_SC_TIME_MICROSECS As Integer = &H0
    Public Const MIXERCONTROL_CT_SC_TIME_MILLISECS As Integer = &H1000000
    Public Const MIXERCONTROL_CT_SC_LIST_SINGLE As Integer = &H0
    Public Const MIXERCONTROL_CT_SC_LIST_MULTIPLE As Integer = &H1000000
    Public Const MIXERCONTROL_CT_UNITS_MASK As Integer = &HFF0000
    Public Const MIXERCONTROL_CT_UNITS_CUSTOM As Integer = &H0
    Public Const MIXERCONTROL_CT_UNITS_BOOLEAN As Integer = &H10000
    Public Const MIXERCONTROL_CT_UNITS_SIGNED As Integer = &H20000
    Public Const MIXERCONTROL_CT_UNITS_UNSIGNED As Integer = &H30000
    Public Const MIXERCONTROL_CT_UNITS_DECIBELS As Integer = &H40000 '  in 10ths
    Public Const MIXERCONTROL_CT_UNITS_PERCENT As Integer = &H50000 '  in 10ths
    '
    '   Commonly used control types for specifying MIXERCONTROL.dwControlType
    '
    Public Const MIXERCONTROL_CONTROLTYPE_CUSTOM As Boolean = (MIXERCONTROL_CT_CLASS_CUSTOM Or MIXERCONTROL_CT_UNITS_CUSTOM)
    Public Const MIXERCONTROL_CONTROLTYPE_BOOLEANMETER As Boolean = (MIXERCONTROL_CT_CLASS_METER Or MIXERCONTROL_CT_SC_METER_POLLED Or MIXERCONTROL_CT_UNITS_BOOLEAN)
    Public Const MIXERCONTROL_CONTROLTYPE_SIGNEDMETER As Boolean = (MIXERCONTROL_CT_CLASS_METER Or MIXERCONTROL_CT_SC_METER_POLLED Or MIXERCONTROL_CT_UNITS_SIGNED)
    Public Const MIXERCONTROL_CONTROLTYPE_PEAKMETER As Boolean = (CShort(MIXERCONTROL_CONTROLTYPE_SIGNEDMETER) + 1)
    Public Const MIXERCONTROL_CONTROLTYPE_UNSIGNEDMETER As Boolean = (MIXERCONTROL_CT_CLASS_METER Or MIXERCONTROL_CT_SC_METER_POLLED Or MIXERCONTROL_CT_UNITS_UNSIGNED)
    Public Const MIXERCONTROL_CONTROLTYPE_BOOLEAN As Boolean = (MIXERCONTROL_CT_CLASS_SWITCH Or MIXERCONTROL_CT_SC_SWITCH_BOOLEAN Or MIXERCONTROL_CT_UNITS_BOOLEAN)
    Public Const MIXERCONTROL_CONTROLTYPE_ONOFF As Boolean = (CShort(MIXERCONTROL_CONTROLTYPE_BOOLEAN) + 1)
    Public Const MIXERCONTROL_CONTROLTYPE_MUTE As Boolean = (CShort(MIXERCONTROL_CONTROLTYPE_BOOLEAN) + 2)
    Public Const MIXERCONTROL_CONTROLTYPE_MONO As Boolean = (CShort(MIXERCONTROL_CONTROLTYPE_BOOLEAN) + 3)
    Public Const MIXERCONTROL_CONTROLTYPE_LOUDNESS As Boolean = (CShort(MIXERCONTROL_CONTROLTYPE_BOOLEAN) + 4)
    Public Const MIXERCONTROL_CONTROLTYPE_STEREOENH As Boolean = (CShort(MIXERCONTROL_CONTROLTYPE_BOOLEAN) + 5)
    Public Const MIXERCONTROL_CONTROLTYPE_BUTTON As Boolean = (MIXERCONTROL_CT_CLASS_SWITCH Or MIXERCONTROL_CT_SC_SWITCH_BUTTON Or MIXERCONTROL_CT_UNITS_BOOLEAN)
    Public Const MIXERCONTROL_CONTROLTYPE_DECIBELS As Boolean = (MIXERCONTROL_CT_CLASS_NUMBER Or MIXERCONTROL_CT_UNITS_DECIBELS)
    Public Const MIXERCONTROL_CONTROLTYPE_SIGNED As Boolean = (MIXERCONTROL_CT_CLASS_NUMBER Or MIXERCONTROL_CT_UNITS_SIGNED)
    Public Const MIXERCONTROL_CONTROLTYPE_UNSIGNED As Boolean = (MIXERCONTROL_CT_CLASS_NUMBER Or MIXERCONTROL_CT_UNITS_UNSIGNED)
    Public Const MIXERCONTROL_CONTROLTYPE_PERCENT As Boolean = (MIXERCONTROL_CT_CLASS_NUMBER Or MIXERCONTROL_CT_UNITS_PERCENT)
    Public Const MIXERCONTROL_CONTROLTYPE_SLIDER As Boolean = (MIXERCONTROL_CT_CLASS_SLIDER Or MIXERCONTROL_CT_UNITS_SIGNED)
    Public Const MIXERCONTROL_CONTROLTYPE_PAN As Boolean = (CShort(MIXERCONTROL_CONTROLTYPE_SLIDER) + 1)
    Public Const MIXERCONTROL_CONTROLTYPE_QSOUNDPAN As Boolean = (CShort(MIXERCONTROL_CONTROLTYPE_SLIDER) + 2)
    Public Const MIXERCONTROL_CONTROLTYPE_FADER As Boolean = (MIXERCONTROL_CT_CLASS_FADER Or MIXERCONTROL_CT_UNITS_UNSIGNED)
    Public Const MIXERCONTROL_CONTROLTYPE_VOLUME As Boolean = (CShort(MIXERCONTROL_CONTROLTYPE_FADER) + 1)
    Public Const MIXERCONTROL_CONTROLTYPE_BASS As Boolean = (CShort(MIXERCONTROL_CONTROLTYPE_FADER) + 2)
    Public Const MIXERCONTROL_CONTROLTYPE_TREBLE As Boolean = (CShort(MIXERCONTROL_CONTROLTYPE_FADER) + 3)
    Public Const MIXERCONTROL_CONTROLTYPE_EQUALIZER As Boolean = (CShort(MIXERCONTROL_CONTROLTYPE_FADER) + 4)
    Public Const MIXERCONTROL_CONTROLTYPE_SINGLESELECT As Boolean = (MIXERCONTROL_CT_CLASS_LIST Or MIXERCONTROL_CT_SC_LIST_SINGLE Or MIXERCONTROL_CT_UNITS_BOOLEAN)
    Public Const MIXERCONTROL_CONTROLTYPE_MUX As Boolean = (CShort(MIXERCONTROL_CONTROLTYPE_SINGLESELECT) + 1)
    Public Const MIXERCONTROL_CONTROLTYPE_MULTIPLESELECT As Boolean = (MIXERCONTROL_CT_CLASS_LIST Or MIXERCONTROL_CT_SC_LIST_MULTIPLE Or MIXERCONTROL_CT_UNITS_BOOLEAN)
    Public Const MIXERCONTROL_CONTROLTYPE_MIXER As Boolean = (CShort(MIXERCONTROL_CONTROLTYPE_MULTIPLESELECT) + 1)
    Public Const MIXERCONTROL_CONTROLTYPE_MICROTIME As Boolean = (MIXERCONTROL_CT_CLASS_TIME Or MIXERCONTROL_CT_SC_TIME_MICROSECS Or MIXERCONTROL_CT_UNITS_UNSIGNED)
    Public Const MIXERCONTROL_CONTROLTYPE_MILLITIME As Boolean = (MIXERCONTROL_CT_CLASS_TIME Or MIXERCONTROL_CT_SC_TIME_MILLISECS Or MIXERCONTROL_CT_UNITS_UNSIGNED)
    '
    '   MIXERLINECONTROLS
    '
    Public Structure MIXERLINECONTROLS
        Dim cbStruct As Integer '  size in Byte of MIXERLINECONTROLS
        Dim dwLineID As Integer '  line id (from MIXERLINE.dwLineID)
        '  MIXER_GETLINECONTROLSF_ONEBYID or
        Dim dwControl As Integer '  MIXER_GETLINECONTROLSF_ONEBYTYPE
        Dim cControls As Integer '  count of controls pmxctrl points to
        Dim cbmxctrl As Integer '  size in Byte of _one_ MIXERCONTROL
        Dim pamxctrl As MIXERCONTROL '  pointer to first MIXERCONTROL array
    End Structure

    'UPGRADE_WARNING: ?? MIXERLINECONTROLS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function mixerGetLineControls Lib "winmm.dll" Alias "mixerGetLineControlsA" (hmxobj As Integer, ByRef pmxlc As MIXERLINECONTROLS, fdwControls As Integer) As Integer

    Public Const MIXER_GETLINECONTROLSF_ALL As Integer = &H0
    Public Const MIXER_GETLINECONTROLSF_ONEBYID As Integer = &H1
    Public Const MIXER_GETLINECONTROLSF_ONEBYTYPE As Integer = &H2
    Public Const MIXER_GETLINECONTROLSF_QUERYMASK As Integer = &HF

    Public Structure MIXERCONTROLDETAILS
        Dim cbStruct As Integer '  size in Byte of MIXERCONTROLDETAILS
        Dim dwControlID As Integer '  control id to get/set details on
        Dim cChannels As Integer '  number of channels in paDetails array
        Dim item As Integer ' hwndOwner or cMultipleItems
        Dim cbDetails As Integer '  size of _one_ details_XX struct
        Dim paDetails As Integer '  pointer to array of details_XX structs
    End Structure

    '   MIXER_GETCONTROLDETAILSF_LISTTEXT

    Public Structure MIXERCONTROLDETAILS_LISTTEXT
        Dim dwParam1 As Integer
        Dim dwParam2 As Integer
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(MIXER_LONG_NAME_CHARS), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=MIXER_LONG_NAME_CHARS)> Public szName() As Char
    End Structure

    '   MIXER_GETCONTROLDETAILSF_VALUE

    Public Structure MIXERCONTROLDETAILS_BOOLEAN
        Dim fValue As Integer
    End Structure

    Public Structure MIXERCONTROLDETAILS_SIGNED
        Dim lValue As Integer
    End Structure

    Public Structure MIXERCONTROLDETAILS_UNSIGNED
        Dim dwValue As Integer
    End Structure

    'UPGRADE_WARNING: ?? MIXERCONTROLDETAILS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function mixerGetControlDetails Lib "winmm.dll" Alias "mixerGetControlDetailsA" (hmxobj As Integer, ByRef pmxcd As MIXERCONTROLDETAILS, fdwDetails As Integer) As Integer

    Public Const MIXER_GETCONTROLDETAILSF_VALUE As Integer = &H0
    Public Const MIXER_GETCONTROLDETAILSF_LISTTEXT As Integer = &H1
    Public Const MIXER_GETCONTROLDETAILSF_QUERYMASK As Integer = &HF

    'UPGRADE_WARNING: ?? MIXERCONTROLDETAILS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function mixerSetControlDetails Lib "winmm.dll" (hmxobj As Integer, ByRef pmxcd As MIXERCONTROLDETAILS, fdwDetails As Integer) As Integer

    Public Const MIXER_SETCONTROLDETAILSF_VALUE As Integer = &H0
    Public Const MIXER_SETCONTROLDETAILSF_CUSTOM As Integer = &H1
    Public Const MIXER_SETCONTROLDETAILSF_QUERYMASK As Integer = &HF

    '  constants used with JOYINFOEX
    Public Const JOY_BUTTON5 As Integer = &H10
    Public Const JOY_BUTTON6 As Integer = &H20
    Public Const JOY_BUTTON7 As Integer = &H40
    Public Const JOY_BUTTON8 As Integer = &H80
    Public Const JOY_BUTTON9 As Integer = &H100
    Public Const JOY_BUTTON10 As Integer = &H200
    Public Const JOY_BUTTON11 As Integer = &H400
    Public Const JOY_BUTTON12 As Integer = &H800
    Public Const JOY_BUTTON13 As Integer = &H1000
    Public Const JOY_BUTTON14 As Integer = &H2000
    Public Const JOY_BUTTON15 As Integer = &H4000
    Public Const JOY_BUTTON16 As Integer = &H8000
    Public Const JOY_BUTTON17 As Integer = &H10000
    Public Const JOY_BUTTON18 As Integer = &H20000
    Public Const JOY_BUTTON19 As Integer = &H40000
    Public Const JOY_BUTTON20 As Integer = &H80000
    Public Const JOY_BUTTON21 As Integer = &H100000
    Public Const JOY_BUTTON22 As Integer = &H200000
    Public Const JOY_BUTTON23 As Integer = &H400000
    Public Const JOY_BUTTON24 As Integer = &H800000
    Public Const JOY_BUTTON25 As Integer = &H1000000
    Public Const JOY_BUTTON26 As Integer = &H2000000
    Public Const JOY_BUTTON27 As Integer = &H4000000
    Public Const JOY_BUTTON28 As Integer = &H8000000
    Public Const JOY_BUTTON29 As Integer = &H10000000
    Public Const JOY_BUTTON30 As Integer = &H20000000
    Public Const JOY_BUTTON31 As Integer = &H40000000
    Public Const JOY_BUTTON32 As Integer = &H80000000

    '  constants used with JOYINFOEX structure
    Public Const JOY_POVCENTERED As Short = -1
    Public Const JOY_POVFORWARD As Short = 0
    Public Const JOY_POVRIGHT As Short = 9000
    Public Const JOY_POVBACKWARD As Short = 18000
    Public Const JOY_POVLEFT As Short = 27000
    Public Const JOY_RETURNX As Integer = &H1
    Public Const JOY_RETURNY As Integer = &H2
    Public Const JOY_RETURNZ As Integer = &H4
    Public Const JOY_RETURNR As Integer = &H8
    Public Const JOY_RETURNU As Short = &H10S '  axis 5
    Public Const JOY_RETURNV As Short = &H20S '  axis 6
    Public Const JOY_RETURNPOV As Integer = &H40
    Public Const JOY_RETURNBUTTONS As Integer = &H80
    Public Const JOY_RETURNRAWDATA As Integer = &H100
    Public Const JOY_RETURNPOVCTS As Integer = &H200
    Public Const JOY_RETURNCENTERED As Integer = &H400
    Public Const JOY_USEDEADZONE As Integer = &H800
    Public Const JOY_RETURNALL As Boolean = (JOY_RETURNX Or JOY_RETURNY Or JOY_RETURNZ Or JOY_RETURNR Or JOY_RETURNU Or JOY_RETURNV Or JOY_RETURNPOV Or JOY_RETURNBUTTONS)
    Public Const JOY_CAL_READALWAYS As Integer = &H10000
    Public Const JOY_CAL_READXYONLY As Integer = &H20000
    Public Const JOY_CAL_READ3 As Integer = &H40000
    Public Const JOY_CAL_READ4 As Integer = &H80000
    Public Const JOY_CAL_READXONLY As Integer = &H100000
    Public Const JOY_CAL_READYONLY As Integer = &H200000
    Public Const JOY_CAL_READ5 As Integer = &H400000
    Public Const JOY_CAL_READ6 As Integer = &H800000
    Public Const JOY_CAL_READZONLY As Integer = &H1000000
    Public Const JOY_CAL_READRONLY As Integer = &H2000000
    Public Const JOY_CAL_READUONLY As Integer = &H4000000
    Public Const JOY_CAL_READVONLY As Integer = &H8000000

    'UPGRADE_WARNING: ?? JOYINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function joyGetPos Lib "winmm.dll" (uJoyID As Integer, ByRef pji As JOYINFO) As Integer
    'UPGRADE_WARNING: ?? JOYINFOEX ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function joyGetPosEx Lib "winmm.dll" (uJoyID As Integer, ByRef pji As JOYINFOEX) As Integer
    Public Const WAVE_FORMAT_QUERY As Short = &H1S
    Public Const SND_PURGE As Short = &H40S '  purge non-static events for task
    Public Const SND_APPLICATION As Short = &H80S '  look for application specific association
    Public Const WAVE_MAPPED As Short = &H4S
    Public Const WAVE_FORMAT_DIRECT As Short = &H8S
    Public Const WAVE_FORMAT_DIRECT_QUERY As Boolean = (WAVE_FORMAT_QUERY Or WAVE_FORMAT_DIRECT)
    Public Const MIM_MOREDATA As Short = MM_MIM_MOREDATA
    Public Const MOM_POSITIONCB As Short = MM_MOM_POSITIONCB

    '  flags for dwFlags parm of midiInOpen()
    Public Const MIDI_IO_STATUS As Integer = &H20

    Public Declare Function midiStreamOpen Lib "winmm.dll" (ByRef phms As Integer, ByRef puDeviceID As Integer, cMidi As Integer, dwCallback As Integer, dwInstance As Integer, fdwOpen As Integer) As Integer
    Public Declare Function midiStreamClose Lib "winmm.dll" (hms As Integer) As Integer

    Public Declare Function midiStreamProperty Lib "winmm.dll" (hms As Integer, ByRef lppropdata As Byte, dwProperty As Integer) As Integer
    'UPGRADE_WARNING: ?? MMTIME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function midiStreamPosition Lib "winmm.dll" (hms As Integer, ByRef lpmmt As MMTIME, cbmmt As Integer) As Integer
    'UPGRADE_WARNING: ?? MIDIHDR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function midiStreamOut Lib "winmm.dll" (hms As Integer, ByRef pmh As MIDIHDR, cbmh As Integer) As Integer
    Public Declare Function midiStreamPause Lib "winmm.dll" (hms As Integer) As Integer
    Public Declare Function midiStreamRestart Lib "winmm.dll" (hms As Integer) As Integer
    Public Declare Function midiStreamStop Lib "winmm.dll" (hms As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function midiConnect Lib "winmm.dll" (hmi As Integer, hmo As Integer, ByRef pReserved As Object) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function midiDisconnect Lib "winmm.dll" (hmi As Integer, hmo As Integer, ByRef pReserved As Object) As Integer

    Public Structure JOYINFOEX
        Dim dwSize As Integer '  size of structure
        Dim dwFlags As Integer '  flags to indicate what to return
        Dim dwXpos As Integer '  x position
        Dim dwYpos As Integer '  y position
        Dim dwZpos As Integer '  z position
        Dim dwRpos As Integer '  rudder/4th axis position
        Dim dwUpos As Integer '  5th axis position
        Dim dwVpos As Integer '  6th axis position
        Dim dwButtons As Integer '  button states
        Dim dwButtonNumber As Integer '  current button number pressed
        Dim dwPOV As Integer '  point of view state
        Dim dwReserved1 As Integer '  reserved for communication between winmm driver
        Dim dwReserved2 As Integer '  reserved for future expansion
    End Structure
    ' Installable driver support

    ' Driver messages
    Public Const DRV_LOAD As Short = &H1S
    Public Const DRV_ENABLE As Short = &H2S
    Public Const DRV_OPEN As Short = &H3S
    Public Const DRV_CLOSE As Short = &H4S
    Public Const DRV_DISABLE As Short = &H5S
    Public Const DRV_FREE As Short = &H6S
    Public Const DRV_CONFIGURE As Short = &H7S
    Public Const DRV_QUERYCONFIGURE As Short = &H8S
    Public Const DRV_INSTALL As Short = &H9S
    Public Const DRV_REMOVE As Short = &HAS
    Public Const DRV_EXITSESSION As Short = &HBS
    Public Const DRV_POWER As Short = &HFS
    Public Const DRV_RESERVED As Short = &H800S
    Public Const DRV_USER As Short = &H4000S

    Public Structure DRVCONFIGINFO
        Dim dwDCISize As Integer
        Dim lpszDCISectionName As String
        Dim lpszDCIAliasName As String
        Dim dnDevNode As Integer
    End Structure

    ' Supported return values for DRV_CONFIGURE message
    Public Const DRVCNF_CANCEL As Short = &H0S
    Public Const DRVCNF_OK As Short = &H1S
    Public Const DRVCNF_RESTART As Short = &H2S

    '  return values from DriverProc() function
    Public Const DRV_CANCEL As Short = DRVCNF_CANCEL
    Public Const DRV_OK As Short = DRVCNF_OK
    Public Const DRV_RESTART As Short = DRVCNF_RESTART

    Public Declare Function CloseDriver Lib "winmm.dll" (hDriver As Integer, lParam1 As Integer, lParam2 As Integer) As Integer
    Public Declare Function OpenDriver Lib "winmm.dll" (szDriverName As String, szSectionName As String, lParam2 As Integer) As Integer
    Public Declare Function SendDriverMessage Lib "winmm.dll" (hDriver As Integer, message As Integer, lParam1 As Integer, lParam2 As Integer) As Integer
    Public Declare Function DrvGetModuleHandle Lib "winmm.dll" (hDriver As Integer) As Integer
    Public Declare Function GetDriverModuleHandle Lib "winmm.dll" (hDriver As Integer) As Integer
    Public Declare Function DefDriverProc Lib "winmm.dll" (dwDriverIdentifier As Integer, hdrvr As Integer, uMsg As Integer, lParam1 As Integer, lParam2 As Integer) As Integer

    Public Const DRV_MCI_FIRST As Short = DRV_RESERVED
    Public Const DRV_MCI_LAST As Integer = DRV_RESERVED + &HFFFS

    ' Driver callback support

    '  flags used with waveOutOpen(), waveInOpen(), midiInOpen(), and
    '  midiOutOpen() to specify the type of the dwCallback parameter.
    Public Const CALLBACK_TYPEMASK As Integer = &H70000 '  callback type mask
    Public Const CALLBACK_NULL As Short = &H0S '  no callback
    Public Const CALLBACK_WINDOW As Integer = &H10000 '  dwCallback is a HWND
    Public Const CALLBACK_TASK As Integer = &H20000 '  dwCallback is a HTASK
    Public Const CALLBACK_FUNCTION As Integer = &H30000 '  dwCallback is a FARPROC

    '  manufacturer IDs
    Public Const MM_MICROSOFT As Short = 1 '  Microsoft Corp.

    '  product IDs
    Public Const MM_MIDI_MAPPER As Short = 1 '  MIDI Mapper
    Public Const MM_WAVE_MAPPER As Short = 2 '  Wave Mapper

    Public Const MM_SNDBLST_MIDIOUT As Short = 3 '  Sound Blaster MIDI output port
    Public Const MM_SNDBLST_MIDIIN As Short = 4 '  Sound Blaster MIDI input port
    Public Const MM_SNDBLST_SYNTH As Short = 5 '  Sound Blaster internal synthesizer
    Public Const MM_SNDBLST_WAVEOUT As Short = 6 '  Sound Blaster waveform output
    Public Const MM_SNDBLST_WAVEIN As Short = 7 '  Sound Blaster waveform input

    Public Const MM_ADLIB As Short = 9 '  Ad Lib-compatible synthesizer

    Public Const MM_MPU401_MIDIOUT As Short = 10 '  MPU401-compatible MIDI output port
    Public Const MM_MPU401_MIDIIN As Short = 11 '  MPU401-compatible MIDI input port

    Public Const MM_PC_JOYSTICK As Short = 12 '  Joystick adapter

    Public Declare Function mmsystemGetVersion Lib "winmm.dll" () As Integer
    Public Declare Sub OutputDebugStr Lib "winmm.dll" (lpszOutputString As String)

    Public Declare Function sndPlaySound Lib "winmm.dll" Alias "sndPlaySoundA" (lpszSoundName As String, uFlags As Integer) As Integer

    '  flag values for uFlags parameter
    Public Const SND_SYNC As Short = &H0S '  play synchronously (default)
    Public Const SND_ASYNC As Short = &H1S '  play asynchronously

    Public Const SND_NODEFAULT As Short = &H2S '  silence not default, if sound not found

    Public Const SND_MEMORY As Short = &H4S '  lpszSoundName points to a memory file
    Public Const SND_ALIAS As Integer = &H10000 '  name is a WIN.INI [sounds] entry
    Public Const SND_FILENAME As Integer = &H20000 '  name is a file name
    Public Const SND_RESOURCE As Integer = &H40004 '  name is a resource name or atom
    Public Const SND_ALIAS_ID As Integer = &H110000 '  name is a WIN.INI [sounds] entry identifier

    Public Const SND_ALIAS_START As Short = 0 '  must be > 4096 to keep strings in same section of resource file

    Public Const SND_LOOP As Short = &H8S '  loop the sound until next sndPlaySound
    Public Const SND_NOSTOP As Short = &H10S '  don't stop any currently playing sound
    Public Const SND_VALID As Short = &H1FS '  valid flags          / ;Internal /

    Public Const SND_NOWAIT As Short = &H2000S '  don't wait if the driver is busy

    Public Const SND_VALIDFLAGS As Integer = &H17201F '  Set of valid flag bits.  Anything outside
    '  this range will raise an error
    Public Const SND_RESERVED As Integer = &HFF000000 '  In particular these flags are reserved

    Public Const SND_TYPE_MASK As Integer = &H170007

    Public Declare Function PlaySound Lib "winmm.dll" Alias "PlaySoundA" (lpszName As String, hModule As Integer, dwFlags As Integer) As Integer

    '  waveform audio error return values
    Public Const WAVERR_BADFORMAT As Integer = (WAVERR_BASE + 0) '  unsupported wave format
    Public Const WAVERR_STILLPLAYING As Integer = (WAVERR_BASE + 1) '  still something playing
    Public Const WAVERR_UNPREPARED As Integer = (WAVERR_BASE + 2) '  header not prepared
    Public Const WAVERR_SYNC As Integer = (WAVERR_BASE + 3) '  device is synchronous
    Public Const WAVERR_LASTERROR As Integer = (WAVERR_BASE + 3) '  last error in range

    '  wave callback messages
    Public Const WOM_OPEN As Short = MM_WOM_OPEN
    Public Const WOM_CLOSE As Short = MM_WOM_CLOSE
    Public Const WOM_DONE As Short = MM_WOM_DONE
    Public Const WIM_OPEN As Short = MM_WIM_OPEN
    Public Const WIM_CLOSE As Short = MM_WIM_CLOSE
    Public Const WIM_DATA As Short = MM_WIM_DATA

    '  device ID for wave device mapper
    Public Const WAVE_MAPPER As Short = -1

    '  flags for dwFlags parameter in waveOutOpen() and waveInOpen()

    Public Const WAVE_ALLOWSYNC As Short = &H2S
    Public Const WAVE_VALID As Short = &H3S '  ;Internal

    Public Structure WAVEHDR
        Dim lpData As String
        Dim dwBufferLength As Integer
        Dim dwBytesRecorded As Integer
        Dim dwUser As Integer
        Dim dwFlags As Integer
        Dim dwLoops As Integer
        Dim lpNext As Integer
        Dim Reserved As Integer
    End Structure

    '  flags for dwFlags field of WAVEHDR
    Public Const WHDR_DONE As Short = &H1S '  done bit
    Public Const WHDR_PREPARED As Short = &H2S '  set if this header has been prepared
    Public Const WHDR_BEGINLOOP As Short = &H4S '  loop start block
    Public Const WHDR_ENDLOOP As Short = &H8S '  loop end block
    Public Const WHDR_INQUEUE As Short = &H10S '  reserved for driver
    Public Const WHDR_VALID As Short = &H1FS '  valid flags      / ;Internal /

    Public Structure WAVEOUTCAPS
        Dim wMid As Short
        Dim wPid As Short
        Dim vDriverVersion As Integer
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(MAXPNAMELEN), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=MAXPNAMELEN)> Public szPname() As Char
        Dim dwFormats As Integer
        Dim wChannels As Short
        Dim dwSupport As Integer
    End Structure

    '  flags for dwSupport field of WAVEOUTCAPS
    Public Const WAVECAPS_PITCH As Short = &H1S '  supports pitch control
    Public Const WAVECAPS_PLAYBACKRATE As Short = &H2S '  supports playback rate control
    Public Const WAVECAPS_VOLUME As Short = &H4S '  supports volume control
    Public Const WAVECAPS_LRVOLUME As Short = &H8S '  separate left-right volume control
    Public Const WAVECAPS_SYNC As Short = &H10S

    Public Structure WAVEINCAPS
        Dim wMid As Short
        Dim wPid As Short
        Dim vDriverVersion As Integer
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(MAXPNAMELEN), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=MAXPNAMELEN)> Public szPname() As Char
        Dim dwFormats As Integer
        Dim wChannels As Short
    End Structure

    '  defines for dwFormat field of WAVEINCAPS and WAVEOUTCAPS
    Public Const WAVE_INVALIDFORMAT As Short = &H0S '  invalid format
    Public Const WAVE_FORMAT_1M08 As Short = &H1S '  11.025 kHz, Mono,   8-bit
    Public Const WAVE_FORMAT_1S08 As Short = &H2S '  11.025 kHz, Stereo, 8-bit
    Public Const WAVE_FORMAT_1M16 As Short = &H4S '  11.025 kHz, Mono,   16-bit
    Public Const WAVE_FORMAT_1S16 As Short = &H8S '  11.025 kHz, Stereo, 16-bit
    Public Const WAVE_FORMAT_2M08 As Short = &H10S '  22.05  kHz, Mono,   8-bit
    Public Const WAVE_FORMAT_2S08 As Short = &H20S '  22.05  kHz, Stereo, 8-bit
    Public Const WAVE_FORMAT_2M16 As Short = &H40S '  22.05  kHz, Mono,   16-bit
    Public Const WAVE_FORMAT_2S16 As Short = &H80S '  22.05  kHz, Stereo, 16-bit
    Public Const WAVE_FORMAT_4M08 As Short = &H100S '  44.1   kHz, Mono,   8-bit
    Public Const WAVE_FORMAT_4S08 As Short = &H200S '  44.1   kHz, Stereo, 8-bit
    Public Const WAVE_FORMAT_4M16 As Short = &H400S '  44.1   kHz, Mono,   16-bit
    Public Const WAVE_FORMAT_4S16 As Short = &H800S '  44.1   kHz, Stereo, 16-bit

    '  flags for wFormatTag field of WAVEFORMAT
    Public Const WAVE_FORMAT_PCM As Short = 1 '  Needed in resource files so outside #ifndef RC_INVOKED

    Public Structure WAVEFORMAT
        Dim wFormatTag As Short
        Dim nChannels As Short
        Dim nSamplesPerSec As Integer
        Dim nAvgBytesPerSec As Integer
        Dim nBlockAlign As Short
    End Structure

    Public Structure PCMWAVEFORMAT
        Dim wf As WAVEFORMAT
        Dim wBitsPerSample As Short
    End Structure

    Public Declare Function waveOutGetNumDevs Lib "winmm.dll" () As Integer
    'UPGRADE_WARNING: ?? WAVEOUTCAPS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function waveOutGetDevCaps Lib "winmm.dll" Alias "waveOutGetDevCapsA" (uDeviceID As Integer, ByRef lpCaps As WAVEOUTCAPS, uSize As Integer) As Integer

    Public Declare Function waveOutGetVolume Lib "winmm.dll" (uDeviceID As Integer, ByRef lpdwVolume As Integer) As Integer
    Public Declare Function waveOutSetVolume Lib "winmm.dll" (uDeviceID As Integer, dwVolume As Integer) As Integer

    'UPGRADE_NOTE: err ???? err_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
    Public Declare Function waveOutGetErrorText Lib "winmm.dll" Alias "waveOutGetErrorTextA" (err_Renamed As Integer, lpText As String, uSize As Integer) As Integer

    'UPGRADE_WARNING: ?? WAVEFORMAT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function waveOutOpen Lib "winmm.dll" (ByRef lphWaveOut As Integer, uDeviceID As Integer, ByRef lpFormat As WAVEFORMAT, dwCallback As Integer, dwInstance As Integer, dwFlags As Integer) As Integer
    Public Declare Function waveOutClose Lib "winmm.dll" (hWaveOut As Integer) As Integer
    'UPGRADE_WARNING: ?? WAVEHDR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function waveOutPrepareHeader Lib "winmm.dll" (hWaveOut As Integer, ByRef lpWaveOutHdr As WAVEHDR, uSize As Integer) As Integer
    'UPGRADE_WARNING: ?? WAVEHDR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function waveOutUnprepareHeader Lib "winmm.dll" (hWaveOut As Integer, ByRef lpWaveOutHdr As WAVEHDR, uSize As Integer) As Integer
    'UPGRADE_WARNING: ?? WAVEHDR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function waveOutWrite Lib "winmm.dll" (hWaveOut As Integer, ByRef lpWaveOutHdr As WAVEHDR, uSize As Integer) As Integer
    Public Declare Function waveOutPause Lib "winmm.dll" (hWaveOut As Integer) As Integer
    Public Declare Function waveOutRestart Lib "winmm.dll" (hWaveOut As Integer) As Integer
    Public Declare Function waveOutReset Lib "winmm.dll" (hWaveOut As Integer) As Integer
    Public Declare Function waveOutBreakLoop Lib "winmm.dll" (hWaveOut As Integer) As Integer
    'UPGRADE_WARNING: ?? MMTIME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function waveOutGetPosition Lib "winmm.dll" (hWaveOut As Integer, ByRef lpInfo As MMTIME, uSize As Integer) As Integer
    Public Declare Function waveOutGetPitch Lib "winmm.dll" (hWaveOut As Integer, ByRef lpdwPitch As Integer) As Integer
    Public Declare Function waveOutSetPitch Lib "winmm.dll" (hWaveOut As Integer, dwPitch As Integer) As Integer
    Public Declare Function waveOutGetPlaybackRate Lib "winmm.dll" (hWaveOut As Integer, ByRef lpdwRate As Integer) As Integer
    Public Declare Function waveOutSetPlaybackRate Lib "winmm.dll" (hWaveOut As Integer, dwRate As Integer) As Integer
    Public Declare Function waveOutGetID Lib "winmm.dll" (hWaveOut As Integer, ByRef lpuDeviceID As Integer) As Integer
    Public Declare Function waveOutMessage Lib "winmm.dll" (hWaveOut As Integer, Msg As Integer, dw1 As Integer, dw2 As Integer) As Integer
    Public Declare Function waveInGetNumDevs Lib "winmm.dll" () As Integer

    'UPGRADE_WARNING: ?? WAVEINCAPS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function waveInGetDevCaps Lib "winmm.dll" Alias "waveInGetDevCapsA" (uDeviceID As Integer, ByRef lpCaps As WAVEINCAPS, uSize As Integer) As Integer

    'UPGRADE_NOTE: err ???? err_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
    Public Declare Function waveInGetErrorText Lib "winmm.dll" Alias "waveInGetErrorTextA" (err_Renamed As Integer, lpText As String, uSize As Integer) As Integer

    'UPGRADE_WARNING: ?? WAVEFORMAT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function waveInOpen Lib "winmm.dll" (ByRef lphWaveIn As Integer, uDeviceID As Integer, ByRef lpFormat As WAVEFORMAT, dwCallback As Integer, dwInstance As Integer, dwFlags As Integer) As Integer
    Public Declare Function waveInClose Lib "winmm.dll" (hWaveIn As Integer) As Integer
    'UPGRADE_WARNING: ?? WAVEHDR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function waveInPrepareHeader Lib "winmm.dll" (hWaveIn As Integer, ByRef lpWaveInHdr As WAVEHDR, uSize As Integer) As Integer
    'UPGRADE_WARNING: ?? WAVEHDR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function waveInUnprepareHeader Lib "winmm.dll" (hWaveIn As Integer, ByRef lpWaveInHdr As WAVEHDR, uSize As Integer) As Integer
    'UPGRADE_WARNING: ?? WAVEHDR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function waveInAddBuffer Lib "winmm.dll" (hWaveIn As Integer, ByRef lpWaveInHdr As WAVEHDR, uSize As Integer) As Integer
    Public Declare Function waveInStart Lib "winmm.dll" (hWaveIn As Integer) As Integer
    Public Declare Function waveInStop Lib "winmm.dll" (hWaveIn As Integer) As Integer
    Public Declare Function waveInReset Lib "winmm.dll" (hWaveIn As Integer) As Integer
    'UPGRADE_WARNING: ?? MMTIME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function waveInGetPosition Lib "winmm.dll" (hWaveIn As Integer, ByRef lpInfo As MMTIME, uSize As Integer) As Integer
    Public Declare Function waveInGetID Lib "winmm.dll" (hWaveIn As Integer, ByRef lpuDeviceID As Integer) As Integer
    Public Declare Function waveInMessage Lib "winmm.dll" (hWaveIn As Integer, Msg As Integer, dw1 As Integer, dw2 As Integer) As Integer

    '  MIDI error return values
    Public Const MIDIERR_UNPREPARED As Integer = (MIDIERR_BASE + 0) '  header not prepared
    Public Const MIDIERR_STILLPLAYING As Integer = (MIDIERR_BASE + 1) '  still something playing
    Public Const MIDIERR_NOMAP As Integer = (MIDIERR_BASE + 2) '  no current map
    Public Const MIDIERR_NOTREADY As Integer = (MIDIERR_BASE + 3) '  hardware is still busy
    Public Const MIDIERR_NODEVICE As Integer = (MIDIERR_BASE + 4) '  port no longer connected
    Public Const MIDIERR_INVALIDSETUP As Integer = (MIDIERR_BASE + 5) '  invalid setup
    Public Const MIDIERR_LASTERROR As Integer = (MIDIERR_BASE + 5) '  last error in range

    '  MIDI callback messages
    Public Const MIM_OPEN As Short = MM_MIM_OPEN
    Public Const MIM_CLOSE As Short = MM_MIM_CLOSE
    Public Const MIM_DATA As Short = MM_MIM_DATA
    Public Const MIM_LONGDATA As Short = MM_MIM_LONGDATA
    Public Const MIM_ERROR As Short = MM_MIM_ERROR
    Public Const MIM_LONGERROR As Short = MM_MIM_LONGERROR
    Public Const MOM_OPEN As Short = MM_MOM_OPEN
    Public Const MOM_CLOSE As Short = MM_MOM_CLOSE
    Public Const MOM_DONE As Short = MM_MOM_DONE

    '  device ID for MIDI mapper
    Public Const MIDIMAPPER As Short = (-1) '  Cannot be cast to DWORD as RC complains
    Public Const MIDI_MAPPER As Short = -1

    '  flags for wFlags parm of midiOutCachePatches(), midiOutCacheDrumPatches()
    Public Const MIDI_CACHE_ALL As Short = 1
    Public Const MIDI_CACHE_BESTFIT As Short = 2
    Public Const MIDI_CACHE_QUERY As Short = 3
    Public Const MIDI_UNCACHE As Short = 4
    Public Const MIDI_CACHE_VALID As Boolean = (MIDI_CACHE_ALL Or MIDI_CACHE_BESTFIT Or MIDI_CACHE_QUERY Or MIDI_UNCACHE) '  ;Internal

    Public Structure MIDIOUTCAPS
        Dim wMid As Short
        Dim wPid As Short
        Dim vDriverVersion As Integer
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(MAXPNAMELEN), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=MAXPNAMELEN)> Public szPname() As Char
        Dim wTechnology As Short
        Dim wVoices As Short
        Dim wNotes As Short
        Dim wChannelMask As Short
        Dim dwSupport As Integer
    End Structure

    '  flags for wTechnology field of MIDIOUTCAPS structure
    Public Const MOD_MIDIPORT As Short = 1 '  output port
    Public Const MOD_SYNTH As Short = 2 '  generic internal synth
    Public Const MOD_SQSYNTH As Short = 3 '  square wave internal synth
    Public Const MOD_FMSYNTH As Short = 4 '  FM internal synth
    Public Const MOD_MAPPER As Short = 5 '  MIDI mapper

    '  flags for dwSupport field of MIDIOUTCAPS
    Public Const MIDICAPS_VOLUME As Short = &H1S '  supports volume control
    Public Const MIDICAPS_LRVOLUME As Short = &H2S '  separate left-right volume control
    Public Const MIDICAPS_CACHE As Short = &H4S

    Public Structure MIDIINCAPS
        Dim wMid As Short
        Dim wPid As Short
        Dim vDriverVersion As Integer
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(MAXPNAMELEN), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=MAXPNAMELEN)> Public szPname() As Char
    End Structure

    Public Structure MIDIHDR
        Dim lpData As String
        Dim dwBufferLength As Integer
        Dim dwBytesRecorded As Integer
        Dim dwUser As Integer
        Dim dwFlags As Integer
        Dim lpNext As Integer
        Dim Reserved As Integer
    End Structure

    '  flags for dwFlags field of MIDIHDR structure
    Public Const MHDR_DONE As Short = &H1S '  done bit
    Public Const MHDR_PREPARED As Short = &H2S '  set if header prepared
    Public Const MHDR_INQUEUE As Short = &H4S '  reserved for driver
    Public Const MHDR_VALID As Short = &H7S '  valid flags / ;Internal /

    'UPGRADE_WARNING: ?? MIDIOUTCAPS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function midiOutGetDevCaps Lib "winmm.dll" Alias "midiOutGetDevCapsA" (uDeviceID As Integer, ByRef lpCaps As MIDIOUTCAPS, uSize As Integer) As Integer

    Public Declare Function midiOutGetVolume Lib "winmm.dll" (uDeviceID As Integer, ByRef lpdwVolume As Integer) As Integer
    Public Declare Function midiOutSetVolume Lib "winmm.dll" (uDeviceID As Integer, dwVolume As Integer) As Integer

    'UPGRADE_NOTE: err ???? err_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
    Public Declare Function midiOutGetErrorText Lib "winmm.dll" Alias "midiOutGetErrorTextA" (err_Renamed As Integer, lpText As String, uSize As Integer) As Integer

    Public Declare Function midiOutOpen Lib "winmm.dll" (ByRef lphMidiOut As Integer, uDeviceID As Integer, dwCallback As Integer, dwInstance As Integer, dwFlags As Integer) As Integer
    Public Declare Function midiOutClose Lib "winmm.dll" (hMidiOut As Integer) As Integer
    'UPGRADE_WARNING: ?? MIDIHDR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function midiOutPrepareHeader Lib "winmm.dll" (hMidiOut As Integer, ByRef lpMidiOutHdr As MIDIHDR, uSize As Integer) As Integer
    'UPGRADE_WARNING: ?? MIDIHDR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function midiOutUnprepareHeader Lib "winmm.dll" (hMidiOut As Integer, ByRef lpMidiOutHdr As MIDIHDR, uSize As Integer) As Integer
    Public Declare Function midiOutShortMsg Lib "winmm.dll" (hMidiOut As Integer, dwMsg As Integer) As Integer
    'UPGRADE_WARNING: ?? MIDIHDR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function midiOutLongMsg Lib "winmm.dll" (hMidiOut As Integer, ByRef lpMidiOutHdr As MIDIHDR, uSize As Integer) As Integer
    Public Declare Function midiOutReset Lib "winmm.dll" (hMidiOut As Integer) As Integer
    Public Declare Function midiOutCachePatches Lib "winmm.dll" (hMidiOut As Integer, uBank As Integer, ByRef lpPatchArray As Integer, uFlags As Integer) As Integer
    Public Declare Function midiOutCacheDrumPatches Lib "winmm.dll" (hMidiOut As Integer, uPatch As Integer, ByRef lpKeyArray As Integer, uFlags As Integer) As Integer
    Public Declare Function midiOutGetID Lib "winmm.dll" (hMidiOut As Integer, ByRef lpuDeviceID As Integer) As Integer
    Public Declare Function midiOutMessage Lib "winmm.dll" (hMidiOut As Integer, Msg As Integer, dw1 As Integer, dw2 As Integer) As Integer
    Public Declare Function midiInGetNumDevs Lib "winmm.dll" () As Integer

    'UPGRADE_WARNING: ?? MIDIINCAPS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function midiInGetDevCaps Lib "winmm.dll" Alias "midiInGetDevCapsA" (uDeviceID As Integer, ByRef lpCaps As MIDIINCAPS, uSize As Integer) As Integer

    'UPGRADE_NOTE: err ???? err_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
    Public Declare Function midiInGetErrorText Lib "winmm.dll" Alias "midiInGetErrorTextA" (err_Renamed As Integer, lpText As String, uSize As Integer) As Integer

    Public Declare Function midiInOpen Lib "winmm.dll" (ByRef lphMidiIn As Integer, uDeviceID As Integer, dwCallback As Integer, dwInstance As Integer, dwFlags As Integer) As Integer
    Public Declare Function midiInClose Lib "winmm.dll" (hMidiIn As Integer) As Integer
    'UPGRADE_WARNING: ?? MIDIHDR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function midiInPrepareHeader Lib "winmm.dll" (hMidiIn As Integer, ByRef lpMidiInHdr As MIDIHDR, uSize As Integer) As Integer
    'UPGRADE_WARNING: ?? MIDIHDR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function midiInUnprepareHeader Lib "winmm.dll" (hMidiIn As Integer, ByRef lpMidiInHdr As MIDIHDR, uSize As Integer) As Integer
    'UPGRADE_WARNING: ?? MIDIHDR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function midiInAddBuffer Lib "winmm.dll" (hMidiIn As Integer, ByRef lpMidiInHdr As MIDIHDR, uSize As Integer) As Integer
    Public Declare Function midiInStart Lib "winmm.dll" (hMidiIn As Integer) As Integer
    Public Declare Function midiInStop Lib "winmm.dll" (hMidiIn As Integer) As Integer
    Public Declare Function midiInReset Lib "winmm.dll" (hMidiIn As Integer) As Integer
    Public Declare Function midiInGetID Lib "winmm.dll" (hMidiIn As Integer, ByRef lpuDeviceID As Integer) As Integer
    Public Declare Function midiInMessage Lib "winmm.dll" (hMidiIn As Integer, Msg As Integer, dw1 As Integer, dw2 As Integer) As Integer

    '  device ID for aux device mapper
    Public Const AUX_MAPPER As Short = -1

    Public Structure AUXCAPS
        Dim wMid As Short
        Dim wPid As Short
        Dim vDriverVersion As Integer
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(MAXPNAMELEN), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=MAXPNAMELEN)> Public szPname() As Char
        Dim wTechnology As Short
        Dim dwSupport As Integer
    End Structure

    '  flags for wTechnology field in AUXCAPS structure
    Public Const AUXCAPS_CDAUDIO As Short = 1 '  audio from internal CD-ROM drive
    Public Const AUXCAPS_AUXIN As Short = 2 '  audio from auxiliary input jacks

    '  flags for dwSupport field in AUXCAPS structure
    Public Const AUXCAPS_VOLUME As Short = &H1S '  supports volume control
    Public Const AUXCAPS_LRVOLUME As Short = &H2S '  separate left-right volume control

    Public Declare Function auxGetNumDevs Lib "winmm.dll" () As Integer
    'UPGRADE_WARNING: ?? AUXCAPS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function auxGetDevCaps Lib "winmm.dll" Alias "auxGetDevCapsA" (uDeviceID As Integer, ByRef lpCaps As AUXCAPS, uSize As Integer) As Integer

    Public Declare Function auxSetVolume Lib "winmm.dll" (uDeviceID As Integer, dwVolume As Integer) As Integer
    Public Declare Function auxGetVolume Lib "winmm.dll" (uDeviceID As Integer, ByRef lpdwVolume As Integer) As Integer
    Public Declare Function auxOutMessage Lib "winmm.dll" (uDeviceID As Integer, Msg As Integer, dw1 As Integer, dw2 As Integer) As Integer

    '  timer error return values
    Public Const TIMERR_NOERROR As Short = (0) '  no error
    Public Const TIMERR_NOCANDO As Integer = (TIMERR_BASE + 1) '  request not completed
    Public Const TIMERR_STRUCT As Integer = (TIMERR_BASE + 33) '  time struct size

    '  flags for wFlags parameter of timeSetEvent() function
    Public Const TIME_ONESHOT As Short = 0 '  program timer for single event
    Public Const TIME_PERIODIC As Short = 1 '  program for continuous periodic event

    Public Structure TIMECAPS
        Dim wPeriodMin As Integer
        Dim wPeriodMax As Integer
    End Structure

    'UPGRADE_WARNING: ?? MMTIME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function timeGetSystemTime Lib "winmm.dll" (ByRef lpTime As MMTIME, uSize As Integer) As Integer
    Public Declare Function timeGetTime Lib "winmm.dll" () As Integer
    Public Declare Function timeSetEvent Lib "winmm.dll" (uDelay As Integer, uResolution As Integer, lpFunction As Integer, dwUser As Integer, uFlags As Integer) As Integer
    Public Declare Function timeKillEvent Lib "winmm.dll" (uID As Integer) As Integer
    'UPGRADE_WARNING: ?? TIMECAPS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function timeGetDevCaps Lib "winmm.dll" (ByRef lpTimeCaps As TIMECAPS, uSize As Integer) As Integer
    Public Declare Function timeBeginPeriod Lib "winmm.dll" (uPeriod As Integer) As Integer
    Public Declare Function timeEndPeriod Lib "winmm.dll" (uPeriod As Integer) As Integer

    '  joystick error return values
    Public Const JOYERR_NOERROR As Short = (0) '  no error
    Public Const JOYERR_PARMS As Integer = (JOYERR_BASE + 5) '  bad parameters
    Public Const JOYERR_NOCANDO As Integer = (JOYERR_BASE + 6) '  request not completed
    Public Const JOYERR_UNPLUGGED As Integer = (JOYERR_BASE + 7) '  joystick is unplugged

    '  constants used with JOYINFO Public Structure and MM_JOY messages
    Public Const JOY_BUTTON1 As Short = &H1S
    Public Const JOY_BUTTON2 As Short = &H2S
    Public Const JOY_BUTTON3 As Short = &H4S
    Public Const JOY_BUTTON4 As Short = &H8S
    Public Const JOY_BUTTON1CHG As Short = &H100S
    Public Const JOY_BUTTON2CHG As Short = &H200S
    Public Const JOY_BUTTON3CHG As Short = &H400S
    Public Const JOY_BUTTON4CHG As Short = &H800S

    '  joystick ID constants
    Public Const JOYSTICKID1 As Short = 0
    Public Const JOYSTICKID2 As Short = 1

    Public Structure JOYCAPS
        Dim wMid As Short
        Dim wPid As Short
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(MAXPNAMELEN), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=MAXPNAMELEN)> Public szPname() As Char
        Dim wXmin As Short
        Dim wXmax As Short
        Dim wYmin As Short
        Dim wYmax As Short
        Dim wZmin As Short
        Dim wZmax As Short
        Dim wNumButtons As Short
        Dim wPeriodMin As Short
        Dim wPeriodMax As Short
    End Structure

    Public Structure JOYINFO
        Dim wXpos As Short
        Dim wYpos As Short
        Dim wZpos As Short
        Dim wButtons As Short
    End Structure

    'UPGRADE_WARNING: ?? JOYCAPS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function joyGetDevCaps Lib "winmm.dll" Alias "joyGetDevCapsA" (id As Integer, ByRef lpCaps As JOYCAPS, uSize As Integer) As Integer

    Public Declare Function joyGetNumDevs Lib "winmm.dll" Alias "joyGetNumDev" () As Integer
    Public Declare Function joyGetThreshold Lib "winmm.dll" (id As Integer, ByRef lpuThreshold As Integer) As Integer
    Public Declare Function joyReleaseCapture Lib "winmm.dll" (id As Integer) As Integer
    Public Declare Function joySetCapture Lib "winmm.dll" (hWnd As Integer, uID As Integer, uPeriod As Integer, bChanged As Integer) As Integer
    Public Declare Function joySetThreshold Lib "winmm.dll" (id As Integer, uThreshold As Integer) As Integer

    '  MMIO error return values
    Public Const MMIOERR_BASE As Short = 256
    Public Const MMIOERR_FILENOTFOUND As Integer = (MMIOERR_BASE + 1) '  file not found
    Public Const MMIOERR_OUTOFMEMORY As Integer = (MMIOERR_BASE + 2) '  out of memory
    Public Const MMIOERR_CANNOTOPEN As Integer = (MMIOERR_BASE + 3) '  cannot open
    Public Const MMIOERR_CANNOTCLOSE As Integer = (MMIOERR_BASE + 4) '  cannot close
    Public Const MMIOERR_CANNOTREAD As Integer = (MMIOERR_BASE + 5) '  cannot read
    Public Const MMIOERR_CANNOTWRITE As Integer = (MMIOERR_BASE + 6) '  cannot write
    Public Const MMIOERR_CANNOTSEEK As Integer = (MMIOERR_BASE + 7) '  cannot seek
    Public Const MMIOERR_CANNOTEXPAND As Integer = (MMIOERR_BASE + 8) '  cannot expand file
    Public Const MMIOERR_CHUNKNOTFOUND As Integer = (MMIOERR_BASE + 9) '  chunk not found
    Public Const MMIOERR_UNBUFFERED As Integer = (MMIOERR_BASE + 10) '  file is unbuffered

    '  MMIO constants
    Public Const CFSEPCHAR As String = "+" '  compound file name separator char.

    Public Structure MMIOINFO
        Dim dwFlags As Integer
        Dim fccIOProc As Integer
        Dim pIOProc As Integer
        Dim wErrorRet As Integer
        Dim htask As Integer
        Dim cchBuffer As Integer
        Dim pchBuffer As String
        Dim pchNext As String
        Dim pchEndRead As String
        Dim pchEndWrite As String
        Dim lBufOffset As Integer
        Dim lDiskOffset As Integer
        <VBFixedArray(4)> Dim adwInfo() As Integer
        Dim dwReserved1 As Integer
        Dim dwReserved2 As Integer
        Dim hmmio As Integer

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim adwInfo(4)
        End Sub
    End Structure

    Public Const MMIO_RWMODE As Short = &H3S '  mask to get bits used for opening
    '  file for reading/writing/both
    Public Const MMIO_SHAREMODE As Short = &H70S '  file sharing mode number

    '  constants for dwFlags field of MMIOINFO
    Public Const MMIO_CREATE As Short = &H1000S '  create new file (or truncate file)
    Public Const MMIO_PARSE As Short = &H100S '  parse new file returning path
    Public Const MMIO_DELETE As Short = &H200S '  create new file (or truncate file)
    Public Const MMIO_EXIST As Short = &H4000S '  checks for existence of file
    Public Const MMIO_ALLOCBUF As Integer = &H10000 '  mmioOpen() should allocate a buffer
    Public Const MMIO_GETTEMP As Integer = &H20000 '  mmioOpen() should retrieve temp name

    Public Const MMIO_DIRTY As Integer = &H10000000 '  I/O buffer is dirty
    '  MMIO_DIRTY is also used in the <dwFlags> field of MMCKINFO structure

    Public Const MMIO_OPEN_VALID As Integer = &H3FFFF '  valid flags for mmioOpen / ;Internal /

    '  read/write mode numbers (bit field MMIO_RWMODE)
    Public Const MMIO_READ As Short = &H0S '  open file for reading only
    Public Const MMIO_WRITE As Short = &H1S '  open file for writing only
    Public Const MMIO_READWRITE As Short = &H2S '  open file for reading and writing

    '  share mode numbers (bit field MMIO_SHAREMODE)
    Public Const MMIO_COMPAT As Short = &H0S '  compatibility mode
    Public Const MMIO_EXCLUSIVE As Short = &H10S '  exclusive-access mode
    Public Const MMIO_DENYWRITE As Short = &H20S '  deny writing to other processes
    Public Const MMIO_DENYREAD As Short = &H30S '  deny reading to other processes
    Public Const MMIO_DENYNONE As Short = &H40S '  deny nothing to other processes

    '  flags for other functions
    Public Const MMIO_FHOPEN As Short = &H10S '  mmioClose(): keep file handle open
    Public Const MMIO_EMPTYBUF As Short = &H10S '  mmioFlush(): empty the I/O buffer
    Public Const MMIO_TOUPPER As Short = &H10S '  mmioStringToFOURCC(): cvt. to u-case
    Public Const MMIO_INSTALLPROC As Integer = &H10000 '  mmioInstallIOProc(): install MMIOProc
    Public Const MMIO_PUBLICPROC As Integer = &H10000000 '  mmioInstallIOProc: install Globally
    Public Const MMIO_UNICODEPROC As Integer = &H1000000 '  mmioInstallIOProc(): Unicode MMIOProc
    Public Const MMIO_REMOVEPROC As Integer = &H20000 '  mmioInstallIOProc(): remove MMIOProc
    Public Const MMIO_FINDPROC As Integer = &H40000 '  mmioInstallIOProc(): find an MMIOProc
    Public Const MMIO_FINDCHUNK As Short = &H10S '  mmioDescend(): find a chunk by ID
    Public Const MMIO_FINDRIFF As Short = &H20S '  mmioDescend(): find a LIST chunk
    Public Const MMIO_FINDLIST As Short = &H40S '  mmioDescend(): find a RIFF chunk
    Public Const MMIO_CREATERIFF As Short = &H20S '  mmioCreateChunk(): make a LIST chunk
    Public Const MMIO_CREATELIST As Short = &H40S '  mmioCreateChunk(): make a RIFF chunk

    Public Const MMIO_VALIDPROC As Integer = &H11070000 '  valid for mmioInstallIOProc / ;Internal /

    '  message numbers for MMIOPROC I/O procedure functions
    Public Const MMIOM_READ As Short = MMIO_READ '  read (must equal MMIO_READ!)
    Public Const MMIOM_WRITE As Short = MMIO_WRITE '  write (must equal MMIO_WRITE!)
    Public Const MMIOM_SEEK As Short = 2 '  seek to a new position in file
    Public Const MMIOM_OPEN As Short = 3 '  open file
    Public Const MMIOM_CLOSE As Short = 4 '  close file
    Public Const MMIOM_WRITEFLUSH As Short = 5 '  write and flush
    Public Const MMIOM_RENAME As Short = 6 '  rename specified file
    Public Const MMIOM_USER As Short = &H8000S '  beginning of user-defined messages

    '  flags for mmioSeek()
    Public Const SEEK_SET As Short = 0 '  seek to an absolute position
    Public Const SEEK_CUR As Short = 1 '  seek relative to current position
    Public Const SEEK_END As Short = 2 '  seek relative to end of file

    '  other constants
    Public Const MMIO_DEFAULTBUFFER As Short = 8192 '  default buffer size

    Public Structure MMCKINFO
        Dim ckid As Integer
        Dim ckSize As Integer
        Dim fccType As Integer
        Dim dwDataOffset As Integer
        Dim dwFlags As Integer
    End Structure

    '   MCI error return values
    Public Const MCIERR_INVALID_DEVICE_ID As Integer = (MCIERR_BASE + 1)
    Public Const MCIERR_UNRECOGNIZED_KEYWORD As Integer = (MCIERR_BASE + 3)
    Public Const MCIERR_UNRECOGNIZED_COMMAND As Integer = (MCIERR_BASE + 5)
    Public Const MCIERR_HARDWARE As Integer = (MCIERR_BASE + 6)
    Public Const MCIERR_INVALID_DEVICE_NAME As Integer = (MCIERR_BASE + 7)
    Public Const MCIERR_OUT_OF_MEMORY As Integer = (MCIERR_BASE + 8)
    Public Const MCIERR_DEVICE_OPEN As Integer = (MCIERR_BASE + 9)
    Public Const MCIERR_CANNOT_LOAD_DRIVER As Integer = (MCIERR_BASE + 10)
    Public Const MCIERR_MISSING_COMMAND_STRING As Integer = (MCIERR_BASE + 11)
    Public Const MCIERR_PARAM_OVERFLOW As Integer = (MCIERR_BASE + 12)
    Public Const MCIERR_MISSING_STRING_ARGUMENT As Integer = (MCIERR_BASE + 13)
    Public Const MCIERR_BAD_INTEGER As Integer = (MCIERR_BASE + 14)
    Public Const MCIERR_PARSER_INTERNAL As Integer = (MCIERR_BASE + 15)
    Public Const MCIERR_DRIVER_INTERNAL As Integer = (MCIERR_BASE + 16)
    Public Const MCIERR_MISSING_PARAMETER As Integer = (MCIERR_BASE + 17)
    Public Const MCIERR_UNSUPPORTED_FUNCTION As Integer = (MCIERR_BASE + 18)
    Public Const MCIERR_FILE_NOT_FOUND As Integer = (MCIERR_BASE + 19)
    Public Const MCIERR_DEVICE_NOT_READY As Integer = (MCIERR_BASE + 20)
    Public Const MCIERR_INTERNAL As Integer = (MCIERR_BASE + 21)
    Public Const MCIERR_DRIVER As Integer = (MCIERR_BASE + 22)
    Public Const MCIERR_CANNOT_USE_ALL As Integer = (MCIERR_BASE + 23)
    Public Const MCIERR_MULTIPLE As Integer = (MCIERR_BASE + 24)
    Public Const MCIERR_EXTENSION_NOT_FOUND As Integer = (MCIERR_BASE + 25)
    Public Const MCIERR_OUTOFRANGE As Integer = (MCIERR_BASE + 26)
    Public Const MCIERR_FLAGS_NOT_COMPATIBLE As Integer = (MCIERR_BASE + 28)
    Public Const MCIERR_FILE_NOT_SAVED As Integer = (MCIERR_BASE + 30)
    Public Const MCIERR_DEVICE_TYPE_REQUIRED As Integer = (MCIERR_BASE + 31)
    Public Const MCIERR_DEVICE_LOCKED As Integer = (MCIERR_BASE + 32)
    Public Const MCIERR_DUPLICATE_ALIAS As Integer = (MCIERR_BASE + 33)
    Public Const MCIERR_BAD_CONSTANT As Integer = (MCIERR_BASE + 34)
    Public Const MCIERR_MUST_USE_SHAREABLE As Integer = (MCIERR_BASE + 35)
    Public Const MCIERR_MISSING_DEVICE_NAME As Integer = (MCIERR_BASE + 36)
    Public Const MCIERR_BAD_TIME_FORMAT As Integer = (MCIERR_BASE + 37)
    Public Const MCIERR_NO_CLOSING_QUOTE As Integer = (MCIERR_BASE + 38)
    Public Const MCIERR_DUPLICATE_FLAGS As Integer = (MCIERR_BASE + 39)
    Public Const MCIERR_INVALID_FILE As Integer = (MCIERR_BASE + 40)
    Public Const MCIERR_NULL_PARAMETER_BLOCK As Integer = (MCIERR_BASE + 41)
    Public Const MCIERR_UNNAMED_RESOURCE As Integer = (MCIERR_BASE + 42)
    Public Const MCIERR_NEW_REQUIRES_ALIAS As Integer = (MCIERR_BASE + 43)
    Public Const MCIERR_NOTIFY_ON_AUTO_OPEN As Integer = (MCIERR_BASE + 44)
    Public Const MCIERR_NO_ELEMENT_ALLOWED As Integer = (MCIERR_BASE + 45)
    Public Const MCIERR_NONAPPLICABLE_FUNCTION As Integer = (MCIERR_BASE + 46)
    Public Const MCIERR_ILLEGAL_FOR_AUTO_OPEN As Integer = (MCIERR_BASE + 47)
    Public Const MCIERR_FILENAME_REQUIRED As Integer = (MCIERR_BASE + 48)
    Public Const MCIERR_EXTRA_CHARACTERS As Integer = (MCIERR_BASE + 49)
    Public Const MCIERR_DEVICE_NOT_INSTALLED As Integer = (MCIERR_BASE + 50)
    Public Const MCIERR_GET_CD As Integer = (MCIERR_BASE + 51)
    Public Const MCIERR_SET_CD As Integer = (MCIERR_BASE + 52)
    Public Const MCIERR_SET_DRIVE As Integer = (MCIERR_BASE + 53)
    Public Const MCIERR_DEVICE_LENGTH As Integer = (MCIERR_BASE + 54)
    Public Const MCIERR_DEVICE_ORD_LENGTH As Integer = (MCIERR_BASE + 55)
    Public Const MCIERR_NO_INTEGER As Integer = (MCIERR_BASE + 56)

    Public Const MCIERR_WAVE_OUTPUTSINUSE As Integer = (MCIERR_BASE + 64)
    Public Const MCIERR_WAVE_SETOUTPUTINUSE As Integer = (MCIERR_BASE + 65)
    Public Const MCIERR_WAVE_INPUTSINUSE As Integer = (MCIERR_BASE + 66)
    Public Const MCIERR_WAVE_SETINPUTINUSE As Integer = (MCIERR_BASE + 67)
    Public Const MCIERR_WAVE_OUTPUTUNSPECIFIED As Integer = (MCIERR_BASE + 68)
    Public Const MCIERR_WAVE_INPUTUNSPECIFIED As Integer = (MCIERR_BASE + 69)
    Public Const MCIERR_WAVE_OUTPUTSUNSUITABLE As Integer = (MCIERR_BASE + 70)
    Public Const MCIERR_WAVE_SETOUTPUTUNSUITABLE As Integer = (MCIERR_BASE + 71)
    Public Const MCIERR_WAVE_INPUTSUNSUITABLE As Integer = (MCIERR_BASE + 72)
    Public Const MCIERR_WAVE_SETINPUTUNSUITABLE As Integer = (MCIERR_BASE + 73)

    Public Const MCIERR_SEQ_DIV_INCOMPATIBLE As Integer = (MCIERR_BASE + 80)
    Public Const MCIERR_SEQ_PORT_INUSE As Integer = (MCIERR_BASE + 81)
    Public Const MCIERR_SEQ_PORT_NONEXISTENT As Integer = (MCIERR_BASE + 82)
    Public Const MCIERR_SEQ_PORT_MAPNODEVICE As Integer = (MCIERR_BASE + 83)
    Public Const MCIERR_SEQ_PORT_MISCERROR As Integer = (MCIERR_BASE + 84)
    Public Const MCIERR_SEQ_TIMER As Integer = (MCIERR_BASE + 85)
    Public Const MCIERR_SEQ_PORTUNSPECIFIED As Integer = (MCIERR_BASE + 86)
    Public Const MCIERR_SEQ_NOMIDIPRESENT As Integer = (MCIERR_BASE + 87)

    Public Const MCIERR_NO_WINDOW As Integer = (MCIERR_BASE + 90)
    Public Const MCIERR_CREATEWINDOW As Integer = (MCIERR_BASE + 91)
    Public Const MCIERR_FILE_READ As Integer = (MCIERR_BASE + 92)
    Public Const MCIERR_FILE_WRITE As Integer = (MCIERR_BASE + 93)

    '  All custom device driver errors must be >= this value
    Public Const MCIERR_CUSTOM_DRIVER_BASE As Integer = (MCIERR_BASE + 256)

    '  Message numbers must be in the range between MCI_FIRST and MCI_LAST

    Public Const MCI_FIRST As Short = &H800S
    '  Messages 0x801 and 0x802 are reserved
    Public Const MCI_OPEN As Short = &H803S
    Public Const MCI_CLOSE As Short = &H804S
    Public Const MCI_ESCAPE As Short = &H805S
    Public Const MCI_PLAY As Short = &H806S
    Public Const MCI_SEEK As Short = &H807S
    Public Const MCI_STOP As Short = &H808S
    Public Const MCI_PAUSE As Short = &H809S
    Public Const MCI_INFO As Short = &H80AS
    Public Const MCI_GETDEVCAPS As Short = &H80BS
    Public Const MCI_SPIN As Short = &H80CS
    Public Const MCI_SET As Short = &H80DS
    Public Const MCI_STEP As Short = &H80ES
    Public Const MCI_RECORD As Short = &H80FS
    Public Const MCI_SYSINFO As Short = &H810S
    Public Const MCI_BREAK As Short = &H811S
    Public Const MCI_SOUND As Short = &H812S
    Public Const MCI_SAVE As Short = &H813S
    Public Const MCI_STATUS As Short = &H814S

    Public Const MCI_CUE As Short = &H830S

    Public Const MCI_REALIZE As Short = &H840S
    Public Const MCI_WINDOW As Short = &H841S
    Public Const MCI_PUT As Short = &H842S
    Public Const MCI_WHERE As Short = &H843S
    Public Const MCI_FREEZE As Short = &H844S
    Public Const MCI_UNFREEZE As Short = &H845S

    Public Const MCI_LOAD As Short = &H850S
    Public Const MCI_CUT As Short = &H851S
    Public Const MCI_COPY As Short = &H852S
    Public Const MCI_PASTE As Short = &H853S
    Public Const MCI_UPDATE As Short = &H854S
    Public Const MCI_RESUME As Short = &H855S
    Public Const MCI_DELETE As Short = &H856S

    Public Const MCI_LAST As Short = &HFFFS

    '  the next 0x400 message ID's are reserved for custom drivers
    '  all custom MCI command messages must be >= than this value
    Public Const MCI_USER_MESSAGES As Integer = (&H400S + MCI_FIRST)
    Public Const MCI_ALL_DEVICE_ID As Short = -1 '  Matches all MCI devices

    '  constants for predefined MCI device types
    Public Const MCI_DEVTYPE_VCR As Short = 513
    Public Const MCI_DEVTYPE_VIDEODISC As Short = 514
    Public Const MCI_DEVTYPE_OVERLAY As Short = 515
    Public Const MCI_DEVTYPE_CD_AUDIO As Short = 516
    Public Const MCI_DEVTYPE_DAT As Short = 517
    Public Const MCI_DEVTYPE_SCANNER As Short = 518
    Public Const MCI_DEVTYPE_ANIMATION As Short = 519
    Public Const MCI_DEVTYPE_DIGITAL_VIDEO As Short = 520
    Public Const MCI_DEVTYPE_OTHER As Short = 521
    Public Const MCI_DEVTYPE_WAVEFORM_AUDIO As Short = 522
    Public Const MCI_DEVTYPE_SEQUENCER As Short = 523

    Public Const MCI_DEVTYPE_FIRST As Short = MCI_DEVTYPE_VCR
    Public Const MCI_DEVTYPE_LAST As Short = MCI_DEVTYPE_SEQUENCER

    Public Const MCI_DEVTYPE_FIRST_USER As Short = &H1000S

    '  return values for 'status mode' command
    Public Const MCI_MODE_NOT_READY As Integer = (MCI_STRING_OFFSET + 12)
    Public Const MCI_MODE_STOP As Integer = (MCI_STRING_OFFSET + 13)
    Public Const MCI_MODE_PLAY As Integer = (MCI_STRING_OFFSET + 14)
    Public Const MCI_MODE_RECORD As Integer = (MCI_STRING_OFFSET + 15)
    Public Const MCI_MODE_SEEK As Integer = (MCI_STRING_OFFSET + 16)
    Public Const MCI_MODE_PAUSE As Integer = (MCI_STRING_OFFSET + 17)
    Public Const MCI_MODE_OPEN As Integer = (MCI_STRING_OFFSET + 18)

    '  constants used in 'set time format' and 'status time format' commands
    Public Const MCI_FORMAT_MILLISECONDS As Short = 0
    Public Const MCI_FORMAT_HMS As Short = 1
    Public Const MCI_FORMAT_MSF As Short = 2
    Public Const MCI_FORMAT_FRAMES As Short = 3
    Public Const MCI_FORMAT_SMPTE_24 As Short = 4
    Public Const MCI_FORMAT_SMPTE_25 As Short = 5
    Public Const MCI_FORMAT_SMPTE_30 As Short = 6
    Public Const MCI_FORMAT_SMPTE_30DROP As Short = 7
    Public Const MCI_FORMAT_BYTES As Short = 8
    Public Const MCI_FORMAT_SAMPLES As Short = 9
    Public Const MCI_FORMAT_TMSF As Short = 10

    '  Flags for wParam of the MM_MCINOTIFY message
    Public Const MCI_NOTIFY_SUCCESSFUL As Short = &H1S
    Public Const MCI_NOTIFY_SUPERSEDED As Short = &H2S
    Public Const MCI_NOTIFY_ABORTED As Short = &H4S
    Public Const MCI_NOTIFY_FAILURE As Short = &H8S

    '  common flags for dwFlags parameter of MCI command messages
    Public Const MCI_NOTIFY As Integer = &H1
    Public Const MCI_WAIT As Integer = &H2
    Public Const MCI_FROM As Integer = &H4
    Public Const MCI_TO As Integer = &H8
    Public Const MCI_TRACK As Integer = &H10

    '  flags for dwFlags parameter of MCI_OPEN command message
    Public Const MCI_OPEN_SHAREABLE As Integer = &H100
    Public Const MCI_OPEN_ELEMENT As Integer = &H200
    Public Const MCI_OPEN_ALIAS As Integer = &H400
    Public Const MCI_OPEN_ELEMENT_ID As Integer = &H800
    Public Const MCI_OPEN_TYPE_ID As Integer = &H1000
    Public Const MCI_OPEN_TYPE As Integer = &H2000

    '  flags for dwFlags parameter of MCI_SEEK command message
    Public Const MCI_SEEK_TO_START As Integer = &H100
    Public Const MCI_SEEK_TO_END As Integer = &H200

    '  flags for dwFlags parameter of MCI_STATUS command message
    Public Const MCI_STATUS_ITEM As Integer = &H100
    Public Const MCI_STATUS_START As Integer = &H200

    '  flags for dwItem field of the MCI_STATUS_PARMS parameter block
    Public Const MCI_STATUS_LENGTH As Integer = &H1
    Public Const MCI_STATUS_POSITION As Integer = &H2
    Public Const MCI_STATUS_NUMBER_OF_TRACKS As Integer = &H3
    Public Const MCI_STATUS_MODE As Integer = &H4
    Public Const MCI_STATUS_MEDIA_PRESENT As Integer = &H5
    Public Const MCI_STATUS_TIME_FORMAT As Integer = &H6
    Public Const MCI_STATUS_READY As Integer = &H7
    Public Const MCI_STATUS_CURRENT_TRACK As Integer = &H8

    '  flags for dwFlags parameter of MCI_INFO command message
    Public Const MCI_INFO_PRODUCT As Integer = &H100
    Public Const MCI_INFO_FILE As Integer = &H200

    '  flags for dwFlags parameter of MCI_GETDEVCAPS command message
    Public Const MCI_GETDEVCAPS_ITEM As Integer = &H100

    '  flags for dwItem field of the MCI_GETDEVCAPS_PARMS parameter block
    Public Const MCI_GETDEVCAPS_CAN_RECORD As Integer = &H1
    Public Const MCI_GETDEVCAPS_HAS_AUDIO As Integer = &H2
    Public Const MCI_GETDEVCAPS_HAS_VIDEO As Integer = &H3
    Public Const MCI_GETDEVCAPS_DEVICE_TYPE As Integer = &H4
    Public Const MCI_GETDEVCAPS_USES_FILES As Integer = &H5
    Public Const MCI_GETDEVCAPS_COMPOUND_DEVICE As Integer = &H6
    Public Const MCI_GETDEVCAPS_CAN_EJECT As Integer = &H7
    Public Const MCI_GETDEVCAPS_CAN_PLAY As Integer = &H8
    Public Const MCI_GETDEVCAPS_CAN_SAVE As Integer = &H9

    '  flags for dwFlags parameter of MCI_SYSINFO command message
    Public Const MCI_SYSINFO_QUANTITY As Integer = &H100
    Public Const MCI_SYSINFO_OPEN As Integer = &H200
    Public Const MCI_SYSINFO_NAME As Integer = &H400
    Public Const MCI_SYSINFO_INSTALLNAME As Integer = &H800

    '  flags for dwFlags parameter of MCI_SET command message
    Public Const MCI_SET_DOOR_OPEN As Integer = &H100
    Public Const MCI_SET_DOOR_CLOSED As Integer = &H200
    Public Const MCI_SET_TIME_FORMAT As Integer = &H400
    Public Const MCI_SET_AUDIO As Integer = &H800
    Public Const MCI_SET_VIDEO As Integer = &H1000
    Public Const MCI_SET_ON As Integer = &H2000
    Public Const MCI_SET_OFF As Integer = &H4000

    '  flags for dwAudio field of MCI_SET_PARMS or MCI_SEQ_SET_PARMS
    Public Const MCI_SET_AUDIO_ALL As Integer = &H4001
    Public Const MCI_SET_AUDIO_LEFT As Integer = &H4002
    Public Const MCI_SET_AUDIO_RIGHT As Integer = &H4003

    '  flags for dwFlags parameter of MCI_BREAK command message
    Public Const MCI_BREAK_KEY As Integer = &H100
    Public Const MCI_BREAK_HWND As Integer = &H200
    Public Const MCI_BREAK_OFF As Integer = &H400

    '  flags for dwFlags parameter of MCI_RECORD command message
    Public Const MCI_RECORD_INSERT As Integer = &H100
    Public Const MCI_RECORD_OVERWRITE As Integer = &H200

    '  flags for dwFlags parameter of MCI_SOUND command message
    Public Const MCI_SOUND_NAME As Integer = &H100

    '  flags for dwFlags parameter of MCI_SAVE command message
    Public Const MCI_SAVE_FILE As Integer = &H100

    '  flags for dwFlags parameter of MCI_LOAD command message
    Public Const MCI_LOAD_FILE As Integer = &H100

    Public Structure MCI_GENERIC_PARMS
        Dim dwCallback As Integer
    End Structure

    Public Structure MCI_OPEN_PARMS
        Dim dwCallback As Integer
        Dim wDeviceID As Integer
        Dim lpstrDeviceType As String
        Dim lpstrElementName As String
        Dim lpstrAlias As String
    End Structure

    Public Structure MCI_PLAY_PARMS
        Dim dwCallback As Integer
        Dim dwFrom As Integer
        Dim dwTo As Integer
    End Structure

    Public Structure MCI_SEEK_PARMS
        Dim dwCallback As Integer
        Dim dwTo As Integer
    End Structure

    Public Structure MCI_STATUS_PARMS
        Dim dwCallback As Integer
        Dim dwReturn As Integer
        Dim dwItem As Integer
        Dim dwTrack As Short
    End Structure

    Public Structure MCI_INFO_PARMS
        Dim dwCallback As Integer
        Dim lpstrReturn As String
        Dim dwRetSize As Integer
    End Structure

    Public Structure MCI_GETDEVCAPS_PARMS
        Dim dwCallback As Integer
        Dim dwReturn As Integer
        Dim dwIten As Integer
    End Structure

    Public Structure MCI_SYSINFO_PARMS
        Dim dwCallback As Integer
        Dim lpstrReturn As String
        Dim dwRetSize As Integer
        Dim dwNumber As Integer
        Dim wDeviceType As Integer
    End Structure

    Public Structure MCI_SET_PARMS
        Dim dwCallback As Integer
        Dim dwTimeFormat As Integer
        Dim dwAudio As Integer
    End Structure

    Public Structure MCI_BREAK_PARMS
        Dim dwCallback As Integer
        Dim nVirtKey As Integer
        Dim hwndBreak As Integer
    End Structure

    Public Structure MCI_SOUND_PARMS
        Dim dwCallback As Integer
        Dim lpstrSoundName As String
    End Structure

    Public Structure MCI_SAVE_PARMS
        Dim dwCallback As Integer
        Dim lpFileName As String
    End Structure

    Public Structure MCI_LOAD_PARMS
        Dim dwCallback As Integer
        Dim lpFileName As String
    End Structure

    Public Structure MCI_RECORD_PARMS
        Dim dwCallback As Integer
        Dim dwFrom As Integer
        Dim dwTo As Integer
    End Structure

    Public Const MCI_VD_MODE_PARK As Integer = (MCI_VD_OFFSET + 1)

    '  return ID's for videodisc MCI_GETDEVCAPS command

    '  flag for dwReturn field of MCI_STATUS_PARMS
    '  MCI_STATUS command, (dwItem == MCI_VD_STATUS_MEDIA_TYPE)
    Public Const MCI_VD_MEDIA_CLV As Integer = (MCI_VD_OFFSET + 2)
    Public Const MCI_VD_MEDIA_CAV As Integer = (MCI_VD_OFFSET + 3)
    Public Const MCI_VD_MEDIA_OTHER As Integer = (MCI_VD_OFFSET + 4)

    Public Const MCI_VD_FORMAT_TRACK As Short = &H4001S

    '  flags for dwFlags parameter of MCI_PLAY command message
    Public Const MCI_VD_PLAY_REVERSE As Integer = &H10000
    Public Const MCI_VD_PLAY_FAST As Integer = &H20000
    Public Const MCI_VD_PLAY_SPEED As Integer = &H40000
    Public Const MCI_VD_PLAY_SCAN As Integer = &H80000
    Public Const MCI_VD_PLAY_SLOW As Integer = &H100000

    '  flag for dwFlags parameter of MCI_SEEK command message
    Public Const MCI_VD_SEEK_REVERSE As Integer = &H10000

    '  flags for dwItem field of MCI_STATUS_PARMS parameter block
    Public Const MCI_VD_STATUS_SPEED As Integer = &H4002
    Public Const MCI_VD_STATUS_FORWARD As Integer = &H4003
    Public Const MCI_VD_STATUS_MEDIA_TYPE As Integer = &H4004
    Public Const MCI_VD_STATUS_SIDE As Integer = &H4005
    Public Const MCI_VD_STATUS_DISC_SIZE As Integer = &H4006

    '  flags for dwFlags parameter of MCI_GETDEVCAPS command message
    Public Const MCI_VD_GETDEVCAPS_CLV As Integer = &H10000
    Public Const MCI_VD_GETDEVCAPS_CAV As Integer = &H20000

    Public Const MCI_VD_SPIN_UP As Integer = &H10000
    Public Const MCI_VD_SPIN_DOWN As Integer = &H20000

    '  flags for dwItem field of MCI_GETDEVCAPS_PARMS parameter block
    Public Const MCI_VD_GETDEVCAPS_CAN_REVERSE As Integer = &H4002
    Public Const MCI_VD_GETDEVCAPS_FAST_RATE As Integer = &H4003
    Public Const MCI_VD_GETDEVCAPS_SLOW_RATE As Integer = &H4004
    Public Const MCI_VD_GETDEVCAPS_NORMAL_RATE As Integer = &H4005

    '  flags for the dwFlags parameter of MCI_STEP command message
    Public Const MCI_VD_STEP_FRAMES As Integer = &H10000
    Public Const MCI_VD_STEP_REVERSE As Integer = &H20000

    '  flag for the MCI_ESCAPE command message
    Public Const MCI_VD_ESCAPE_STRING As Integer = &H100

    Public Structure MCI_VD_PLAY_PARMS
        Dim dwCallback As Integer
        Dim dwFrom As Integer
        Dim dwTo As Integer
        Dim dwSpeed As Integer
    End Structure

    Public Structure MCI_VD_STEP_PARMS
        Dim dwCallback As Integer
        Dim dwFrames As Integer
    End Structure

    Public Structure MCI_VD_ESCAPE_PARMS
        Dim dwCallback As Integer
        Dim lpstrCommand As String
    End Structure

    Public Const MCI_WAVE_PCM As Integer = (MCI_WAVE_OFFSET + 0)
    Public Const MCI_WAVE_MAPPER As Integer = (MCI_WAVE_OFFSET + 1)

    '  flags for the dwFlags parameter of MCI_OPEN command message
    Public Const MCI_WAVE_OPEN_BUFFER As Integer = &H10000

    '  flags for the dwFlags parameter of MCI_SET command message
    Public Const MCI_WAVE_SET_FORMATTAG As Integer = &H10000
    Public Const MCI_WAVE_SET_CHANNELS As Integer = &H20000
    Public Const MCI_WAVE_SET_SAMPLESPERSEC As Integer = &H40000
    Public Const MCI_WAVE_SET_AVGBYTESPERSEC As Integer = &H80000
    Public Const MCI_WAVE_SET_BLOCKALIGN As Integer = &H100000
    Public Const MCI_WAVE_SET_BITSPERSAMPLE As Integer = &H200000

    '  flags for the dwFlags parameter of MCI_STATUS, MCI_SET command messages
    Public Const MCI_WAVE_INPUT As Integer = &H400000
    Public Const MCI_WAVE_OUTPUT As Integer = &H800000

    '  flags for the dwItem field of MCI_STATUS_PARMS parameter block
    Public Const MCI_WAVE_STATUS_FORMATTAG As Integer = &H4001
    Public Const MCI_WAVE_STATUS_CHANNELS As Integer = &H4002
    Public Const MCI_WAVE_STATUS_SAMPLESPERSEC As Integer = &H4003
    Public Const MCI_WAVE_STATUS_AVGBYTESPERSEC As Integer = &H4004
    Public Const MCI_WAVE_STATUS_BLOCKALIGN As Integer = &H4005
    Public Const MCI_WAVE_STATUS_BITSPERSAMPLE As Integer = &H4006
    Public Const MCI_WAVE_STATUS_LEVEL As Integer = &H4007

    '  flags for the dwFlags parameter of MCI_SET command message
    Public Const MCI_WAVE_SET_ANYINPUT As Integer = &H4000000
    Public Const MCI_WAVE_SET_ANYOUTPUT As Integer = &H8000000

    '  flags for the dwFlags parameter of MCI_GETDEVCAPS command message
    Public Const MCI_WAVE_GETDEVCAPS_INPUTS As Integer = &H4001
    Public Const MCI_WAVE_GETDEVCAPS_OUTPUTS As Integer = &H4002

    Public Structure MCI_WAVE_OPEN_PARMS
        Dim dwCallback As Integer
        Dim wDeviceID As Integer
        Dim lpstrDeviceType As String
        Dim lpstrElementName As String
        Dim lpstrAlias As String
        Dim dwBufferSeconds As Integer
    End Structure

    Public Structure MCI_WAVE_DELETE_PARMS
        Dim dwCallback As Integer
        Dim dwFrom As Integer
        Dim dwTo As Integer
    End Structure

    Public Structure MCI_WAVE_SET_PARMS
        Dim dwCallback As Integer
        Dim dwTimeFormat As Integer
        Dim dwAudio As Integer
        Dim wInput As Integer
        Dim wOutput As Integer
        Dim wFormatTag As Short
        Dim wReserved2 As Short
        Dim nChannels As Short
        Dim wReserved3 As Short
        Dim nSamplesPerSec As Integer
        Dim nAvgBytesPerSec As Integer
        Dim nBlockAlign As Short
        Dim wReserved4 As Short
        Dim wBitsPerSample As Short
        Dim wReserved5 As Short
    End Structure

    '  flags for the dwReturn field of MCI_STATUS_PARMS parameter block
    '  MCI_STATUS command, (dwItem == MCI_SEQ_STATUS_DIVTYPE)
    Public Const MCI_SEQ_DIV_PPQN As Short = (0 + MCI_SEQ_OFFSET)
    Public Const MCI_SEQ_DIV_SMPTE_24 As Short = (1 + MCI_SEQ_OFFSET)
    Public Const MCI_SEQ_DIV_SMPTE_25 As Short = (2 + MCI_SEQ_OFFSET)
    Public Const MCI_SEQ_DIV_SMPTE_30DROP As Short = (3 + MCI_SEQ_OFFSET)
    Public Const MCI_SEQ_DIV_SMPTE_30 As Short = (4 + MCI_SEQ_OFFSET)

    '  flags for the dwMaster field of MCI_SEQ_SET_PARMS parameter block
    '  MCI_SET command, (dwFlags == MCI_SEQ_SET_MASTER)
    Public Const MCI_SEQ_FORMAT_SONGPTR As Short = &H4001S
    Public Const MCI_SEQ_FILE As Short = &H4002S
    Public Const MCI_SEQ_MIDI As Short = &H4003S
    Public Const MCI_SEQ_SMPTE As Short = &H4004S
    Public Const MCI_SEQ_NONE As Integer = 65533

    Public Const MCI_SEQ_MAPPER As Integer = 65535

    '  flags for the dwItem field of MCI_STATUS_PARMS parameter block
    Public Const MCI_SEQ_STATUS_TEMPO As Integer = &H4002
    Public Const MCI_SEQ_STATUS_PORT As Integer = &H4003
    Public Const MCI_SEQ_STATUS_SLAVE As Integer = &H4007
    Public Const MCI_SEQ_STATUS_MASTER As Integer = &H4008
    Public Const MCI_SEQ_STATUS_OFFSET As Integer = &H4009
    Public Const MCI_SEQ_STATUS_DIVTYPE As Integer = &H400A

    '  flags for the dwFlags parameter of MCI_SET command message
    Public Const MCI_SEQ_SET_TEMPO As Integer = &H10000
    Public Const MCI_SEQ_SET_PORT As Integer = &H20000
    Public Const MCI_SEQ_SET_SLAVE As Integer = &H40000
    Public Const MCI_SEQ_SET_MASTER As Integer = &H80000
    Public Const MCI_SEQ_SET_OFFSET As Integer = &H1000000

    Public Structure MCI_SEQ_SET_PARMS
        Dim dwCallback As Integer
        Dim dwTimeFormat As Integer
        Dim dwAudio As Integer
        Dim dwTempo As Integer
        Dim dwPort As Integer
        Dim dwSlave As Integer
        Dim dwMaster As Integer
        Dim dwOffset As Integer
    End Structure

    '  flags for dwFlags parameter of MCI_OPEN command message
    Public Const MCI_ANIM_OPEN_WS As Integer = &H10000
    Public Const MCI_ANIM_OPEN_PARENT As Integer = &H20000
    Public Const MCI_ANIM_OPEN_NOSTATIC As Integer = &H40000

    '  flags for dwFlags parameter of MCI_PLAY command message
    Public Const MCI_ANIM_PLAY_SPEED As Integer = &H10000
    Public Const MCI_ANIM_PLAY_REVERSE As Integer = &H20000
    Public Const MCI_ANIM_PLAY_FAST As Integer = &H40000
    Public Const MCI_ANIM_PLAY_SLOW As Integer = &H80000
    Public Const MCI_ANIM_PLAY_SCAN As Integer = &H100000

    '  flags for dwFlags parameter of MCI_STEP command message
    Public Const MCI_ANIM_STEP_REVERSE As Integer = &H10000
    Public Const MCI_ANIM_STEP_FRAMES As Integer = &H20000

    '  flags for dwItem field of MCI_STATUS_PARMS parameter block
    Public Const MCI_ANIM_STATUS_SPEED As Integer = &H4001
    Public Const MCI_ANIM_STATUS_FORWARD As Integer = &H4002
    Public Const MCI_ANIM_STATUS_HWND As Integer = &H4003
    Public Const MCI_ANIM_STATUS_HPAL As Integer = &H4004
    Public Const MCI_ANIM_STATUS_STRETCH As Integer = &H4005

    '  flags for the dwFlags parameter of MCI_INFO command message
    Public Const MCI_ANIM_INFO_TEXT As Integer = &H10000

    '  flags for dwItem field of MCI_GETDEVCAPS_PARMS parameter block
    Public Const MCI_ANIM_GETDEVCAPS_CAN_REVERSE As Integer = &H4001
    Public Const MCI_ANIM_GETDEVCAPS_FAST_RATE As Integer = &H4002
    Public Const MCI_ANIM_GETDEVCAPS_SLOW_RATE As Integer = &H4003
    Public Const MCI_ANIM_GETDEVCAPS_NORMAL_RATE As Integer = &H4004
    Public Const MCI_ANIM_GETDEVCAPS_PALETTES As Integer = &H4006
    Public Const MCI_ANIM_GETDEVCAPS_CAN_STRETCH As Integer = &H4007
    Public Const MCI_ANIM_GETDEVCAPS_MAX_WINDOWS As Integer = &H4008

    '  flags for the MCI_REALIZE command message
    Public Const MCI_ANIM_REALIZE_NORM As Integer = &H10000
    Public Const MCI_ANIM_REALIZE_BKGD As Integer = &H20000

    '  flags for dwFlags parameter of MCI_WINDOW command message
    Public Const MCI_ANIM_WINDOW_HWND As Integer = &H10000
    Public Const MCI_ANIM_WINDOW_STATE As Integer = &H40000
    Public Const MCI_ANIM_WINDOW_TEXT As Integer = &H80000
    Public Const MCI_ANIM_WINDOW_ENABLE_STRETCH As Integer = &H100000
    Public Const MCI_ANIM_WINDOW_DISABLE_STRETCH As Integer = &H200000

    '  flags for hWnd field of MCI_ANIM_WINDOW_PARMS parameter block
    '  MCI_WINDOW command message, (dwFlags == MCI_ANIM_WINDOW_HWND)
    Public Const MCI_ANIM_WINDOW_DEFAULT As Integer = &H0

    '  flags for dwFlags parameter of MCI_PUT command message
    Public Const MCI_ANIM_RECT As Integer = &H10000
    Public Const MCI_ANIM_PUT_SOURCE As Integer = &H20000 '  also  MCI_WHERE
    Public Const MCI_ANIM_PUT_DESTINATION As Integer = &H40000 '  also  MCI_WHERE

    '  flags for dwFlags parameter of MCI_WHERE command message
    Public Const MCI_ANIM_WHERE_SOURCE As Integer = &H20000
    Public Const MCI_ANIM_WHERE_DESTINATION As Integer = &H40000

    '  flags for dwFlags parameter of MCI_UPDATE command message
    Public Const MCI_ANIM_UPDATE_HDC As Integer = &H20000

    Public Structure MCI_ANIM_OPEN_PARMS
        Dim dwCallback As Integer
        Dim wDeviceID As Integer
        Dim lpstrDeviceType As String
        Dim lpstrElementName As String
        Dim lpstrAlias As String
        Dim dwStyle As Integer
        Dim hwndParent As Integer
    End Structure

    Public Structure MCI_ANIM_PLAY_PARMS
        Dim dwCallback As Integer
        Dim dwFrom As Integer
        Dim dwTo As Integer
        Dim dwSpeed As Integer
    End Structure

    Public Structure MCI_ANIM_STEP_PARMS
        Dim dwCallback As Integer
        Dim dwFrames As Integer
    End Structure

    Public Structure MCI_ANIM_WINDOW_PARMS
        Dim dwCallback As Integer
        Dim hWnd As Integer
        Dim nCmdShow As Integer
        Dim lpstrText As String
    End Structure

    Public Structure MCI_ANIM_RECT_PARMS
        Dim dwCallback As Integer
        Dim rc As RECT
    End Structure

    Public Structure MCI_ANIM_UPDATE_PARMS
        Dim dwCallback As Integer
        Dim rc As RECT
        Dim hdc As Integer
    End Structure

    '  flags for dwFlags parameter of MCI_OPEN command message
    Public Const MCI_OVLY_OPEN_WS As Integer = &H10000
    Public Const MCI_OVLY_OPEN_PARENT As Integer = &H20000

    '  flags for dwFlags parameter of MCI_STATUS command message
    Public Const MCI_OVLY_STATUS_HWND As Integer = &H4001
    Public Const MCI_OVLY_STATUS_STRETCH As Integer = &H4002

    '  flags for dwFlags parameter of MCI_INFO command message
    Public Const MCI_OVLY_INFO_TEXT As Integer = &H10000

    '  flags for dwItem field of MCI_GETDEVCAPS_PARMS parameter block
    Public Const MCI_OVLY_GETDEVCAPS_CAN_STRETCH As Integer = &H4001
    Public Const MCI_OVLY_GETDEVCAPS_CAN_FREEZE As Integer = &H4002
    Public Const MCI_OVLY_GETDEVCAPS_MAX_WINDOWS As Integer = &H4003

    '  flags for dwFlags parameter of MCI_WINDOW command message
    Public Const MCI_OVLY_WINDOW_HWND As Integer = &H10000
    Public Const MCI_OVLY_WINDOW_STATE As Integer = &H40000
    Public Const MCI_OVLY_WINDOW_TEXT As Integer = &H80000
    Public Const MCI_OVLY_WINDOW_ENABLE_STRETCH As Integer = &H100000
    Public Const MCI_OVLY_WINDOW_DISABLE_STRETCH As Integer = &H200000

    '  flags for hWnd parameter of MCI_OVLY_WINDOW_PARMS parameter block
    Public Const MCI_OVLY_WINDOW_DEFAULT As Integer = &H0

    '  flags for dwFlags parameter of MCI_PUT command message
    Public Const MCI_OVLY_RECT As Integer = &H10000
    Public Const MCI_OVLY_PUT_SOURCE As Integer = &H20000
    Public Const MCI_OVLY_PUT_DESTINATION As Integer = &H40000
    Public Const MCI_OVLY_PUT_FRAME As Integer = &H80000
    Public Const MCI_OVLY_PUT_VIDEO As Integer = &H100000

    '  flags for dwFlags parameter of MCI_WHERE command message
    Public Const MCI_OVLY_WHERE_SOURCE As Integer = &H20000
    Public Const MCI_OVLY_WHERE_DESTINATION As Integer = &H40000
    Public Const MCI_OVLY_WHERE_FRAME As Integer = &H80000
    Public Const MCI_OVLY_WHERE_VIDEO As Integer = &H100000

    Public Structure MCI_OVLY_OPEN_PARMS
        Dim dwCallback As Integer
        Dim wDeviceID As Integer
        Dim lpstrDeviceType As String
        Dim lpstrElementName As String
        Dim lpstrAlias As String
        Dim dwStyle As Integer
        Dim hwndParent As Integer
    End Structure

    Public Structure MCI_OVLY_WINDOW_PARMS
        Dim dwCallback As Integer
        Dim hWnd As Integer
        Dim nCmdShow As Integer
        Dim lpstrText As String
    End Structure

    Public Structure MCI_OVLY_RECT_PARMS
        Dim dwCallback As Integer
        Dim rc As RECT
    End Structure

    Public Structure MCI_OVLY_SAVE_PARMS
        Dim dwCallback As Integer
        Dim lpFileName As String
        Dim rc As RECT
    End Structure

    Public Structure MCI_OVLY_LOAD_PARMS
        Dim dwCallback As Integer
        Dim lpFileName As String
        Dim rc As RECT
    End Structure

    Public Const CAPS1 As Short = 94 '  other caps
    Public Const C1_TRANSPARENT As Short = &H1S '  new raster cap
    Public Const NEWTRANSPARENT As Short = 3 '  use with SetBkMode()

    Public Const QUERYROPSUPPORT As Short = 40 '  use to determine ROP support

    Public Const SELECTDIB As Short = 41 '  DIB.DRV select dib escape

    ' ----------------
    ' shell association database management functions
    ' -----------------

    '  error values for ShellExecute() beyond the regular WinExec() codes
    Public Const SE_ERR_SHARE As Short = 26
    Public Const SE_ERR_ASSOCINCOMPLETE As Short = 27
    Public Const SE_ERR_DDETIMEOUT As Short = 28
    Public Const SE_ERR_DDEFAIL As Short = 29
    Public Const SE_ERR_DDEBUSY As Short = 30
    Public Const SE_ERR_NOASSOC As Short = 31

    ' -------------
    ' Print APIs
    ' -------------

    Public Structure PRINTER_INFO_1
        Dim Flags As Integer
        Dim pDescription As String
        Dim pName As String
        Dim pComment As String
    End Structure

    Public Structure PRINTER_INFO_2
        Dim pServerName As String
        Dim pPrinterName As String
        Dim pShareName As String
        Dim pPortName As String
        Dim pDriverName As String
        Dim pComment As String
        Dim pLocation As String
        Dim pDevmode As DEVMODE
        Dim pSepFile As String
        Dim pPrintProcessor As String
        Dim pDatatype As String
        Dim pParameters As String
        Dim pSecurityDescriptor As SECURITY_DESCRIPTOR
        Dim Attributes As Integer
        Dim Priority As Integer
        Dim DefaultPriority As Integer
        Dim StartTime As Integer
        Dim UntilTime As Integer
        Dim Status As Integer
        Dim cJobs As Integer
        Dim AveragePPM As Integer
    End Structure

    Public Structure PRINTER_INFO_3
        Dim pSecurityDescriptor As SECURITY_DESCRIPTOR
    End Structure

    Public Const PRINTER_CONTROL_PAUSE As Short = 1
    Public Const PRINTER_CONTROL_RESUME As Short = 2
    Public Const PRINTER_CONTROL_PURGE As Short = 3

    Public Const PRINTER_STATUS_PAUSED As Short = &H1S
    Public Const PRINTER_STATUS_ERROR As Short = &H2S
    Public Const PRINTER_STATUS_PENDING_DELETION As Short = &H4S
    Public Const PRINTER_STATUS_PAPER_JAM As Short = &H8S
    Public Const PRINTER_STATUS_PAPER_OUT As Short = &H10S
    Public Const PRINTER_STATUS_MANUAL_FEED As Short = &H20S
    Public Const PRINTER_STATUS_PAPER_PROBLEM As Short = &H40S
    Public Const PRINTER_STATUS_OFFLINE As Short = &H80S
    Public Const PRINTER_STATUS_IO_ACTIVE As Short = &H100S
    Public Const PRINTER_STATUS_BUSY As Short = &H200S
    Public Const PRINTER_STATUS_PRINTING As Short = &H400S
    Public Const PRINTER_STATUS_OUTPUT_BIN_FULL As Short = &H800S
    Public Const PRINTER_STATUS_NOT_AVAILABLE As Short = &H1000S
    Public Const PRINTER_STATUS_WAITING As Short = &H2000S
    Public Const PRINTER_STATUS_PROCESSING As Short = &H4000S
    Public Const PRINTER_STATUS_INITIALIZING As Short = &H8000S
    Public Const PRINTER_STATUS_WARMING_UP As Integer = &H10000
    Public Const PRINTER_STATUS_TONER_LOW As Integer = &H20000
    Public Const PRINTER_STATUS_NO_TONER As Integer = &H40000
    Public Const PRINTER_STATUS_PAGE_PUNT As Integer = &H80000
    Public Const PRINTER_STATUS_USER_INTERVENTION As Integer = &H100000
    Public Const PRINTER_STATUS_OUT_OF_MEMORY As Integer = &H200000
    Public Const PRINTER_STATUS_DOOR_OPEN As Integer = &H400000

    Public Const PRINTER_ATTRIBUTE_QUEUED As Short = &H1S
    Public Const PRINTER_ATTRIBUTE_DIRECT As Short = &H2S
    Public Const PRINTER_ATTRIBUTE_DEFAULT As Short = &H4S
    Public Const PRINTER_ATTRIBUTE_SHARED As Short = &H8S
    Public Const PRINTER_ATTRIBUTE_NETWORK As Short = &H10S
    Public Const PRINTER_ATTRIBUTE_HIDDEN As Short = &H20S
    Public Const PRINTER_ATTRIBUTE_LOCAL As Short = &H40S

    Public Const NO_PRIORITY As Short = 0
    Public Const MAX_PRIORITY As Short = 99
    Public Const MIN_PRIORITY As Short = 1
    Public Const DEF_PRIORITY As Short = 1

    Public Structure JOB_INFO_1
        Dim JobId As Integer
        Dim pPrinterName As String
        Dim pMachineName As String
        Dim pUserName As String
        Dim pDocument As String
        Dim pDatatype As String
        Dim pStatus As String
        Dim Status As Integer
        Dim Priority As Integer
        Dim Position As Integer
        Dim TotalPages As Integer
        Dim PagesPrinted As Integer
        Dim Submitted As SystemTime
    End Structure

    Public Structure JOB_INFO_2
        Dim JobId As Integer
        Dim pPrinterName As String
        Dim pMachineName As String
        Dim pUserName As String
        Dim pDocument As String
        Dim pNotifyName As String
        Dim pDatatype As String
        Dim pPrintProcessor As String
        Dim pParameters As String
        Dim pDriverName As String
        Dim pDevmode As DEVMODE
        Dim pStatus As String
        Dim pSecurityDescriptor As SECURITY_DESCRIPTOR
        Dim Status As Integer
        Dim Priority As Integer
        Dim Position As Integer
        Dim StartTime As Integer
        Dim UntilTime As Integer
        Dim TotalPages As Integer
        'UPGRADE_NOTE: Size ???? Size_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
        Dim Size_Renamed As Integer
        Dim Submitted As SystemTime
        Dim time As Integer
        Dim PagesPrinted As Integer
    End Structure

    Public Const JOB_CONTROL_PAUSE As Short = 1
    Public Const JOB_CONTROL_RESUME As Short = 2
    Public Const JOB_CONTROL_CANCEL As Short = 3
    Public Const JOB_CONTROL_RESTART As Short = 4

    Public Const JOB_STATUS_PAUSED As Short = &H1S
    Public Const JOB_STATUS_ERROR As Short = &H2S
    Public Const JOB_STATUS_DELETING As Short = &H4S
    Public Const JOB_STATUS_SPOOLING As Short = &H8S
    Public Const JOB_STATUS_PRINTING As Short = &H10S
    Public Const JOB_STATUS_OFFLINE As Short = &H20S
    Public Const JOB_STATUS_PAPEROUT As Short = &H40S
    Public Const JOB_STATUS_PRINTED As Short = &H80S

    Public Const JOB_POSITION_UNSPECIFIED As Short = 0

    Public Structure ADDJOB_INFO_1
        Dim Path As String
        Dim JobId As Integer
    End Structure

    Public Structure DRIVER_INFO_1
        Dim pName As String
    End Structure

    Public Structure DRIVER_INFO_2
        Dim cVersion As Integer
        Dim pName As String
        Dim pEnvironment As String
        Dim pDriverPath As String
        Dim pDataFile As String
        Dim pConfigFile As String
    End Structure

    Public Structure DOC_INFO_1
        Dim pDocName As String
        Dim pOutputFile As String
        Dim pDatatype As String
    End Structure

    Public Structure FORM_INFO_1
        Dim pName As String
        'UPGRADE_NOTE: Size ???? Size_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
        Dim Size_Renamed As SIZEL
        Dim ImageableArea As RECTL
    End Structure

    Public Const FORM_BUILTIN As Short = &H1S

    Public Structure PRINTPROCESSOR_INFO_1
        Dim pName As String
    End Structure

    Public Structure PORT_INFO_1
        Dim pName As String
    End Structure

    Public Structure MONITOR_INFO_1
        Dim pName As String
    End Structure

    Public Structure MONITOR_INFO_2
        Dim pName As String
        Dim pEnvironment As String
        Dim pDLLName As String
    End Structure

    Public Structure DATATYPES_INFO_1
        Dim pName As String
    End Structure

    Public Structure PRINTER_DEFAULTS
        Dim pDatatype As String
        Dim pDevmode As DEVMODE
        Dim DesiredAccess As Integer
    End Structure

    Public Structure PRINTER_INFO_4
        Dim pPrinterName As String
        Dim pServerName As String
        Dim Attributes As Integer
    End Structure

    Public Structure PRINTER_INFO_5
        Dim pPrinterName As String
        Dim pPortName As String
        Dim Attributes As Integer
        Dim DeviceNotSelectedTimeout As Integer
        Dim TransmissionRetryTimeout As Integer
    End Structure

    Public Const PRINTER_CONTROL_SET_STATUS As Short = 4
    Public Const PRINTER_ATTRIBUTE_WORK_OFFLINE As Short = &H400S
    Public Const PRINTER_ATTRIBUTE_ENABLE_BIDI As Short = &H800S
    Public Const JOB_CONTROL_DELETE As Short = 5
    Public Const JOB_STATUS_USER_INTERVENTION As Integer = &H10000

    Public Structure DRIVER_INFO_3
        Dim cVersion As Integer
        Dim pName As String '  QMS 810
        Dim pEnvironment As String '  Win32 x86
        Dim pDriverPath As String '  c:\drivers\pscript.dll
        Dim pDataFile As String '  c:\drivers\QMS810.PPD
        Dim pConfigFile As String '  c:\drivers\PSCRPTUI.DLL
        Dim pHelpFile As String '  c:\drivers\PSCRPTUI.HLP
        Dim pDependentFiles As String '
        Dim pMonitorName As String '  "PJL monitor"
        Dim pDefaultDataType As String '  "EMF"
    End Structure

    Public Structure DOC_INFO_2
        Dim pDocName As String
        Dim pOutputFile As String
        Dim pDatatype As String
        Dim dwMode As Integer
        Dim JobId As Integer
    End Structure

    Public Const DI_CHANNEL As Short = 1 '  start direct read/write channel,
    Public Const DI_READ_SPOOL_JOB As Short = 3

    Public Structure PORT_INFO_2
        Dim pPortName As String
        Dim pMonitorName As String
        Dim pDescription As String
        Dim fPortType As Integer
        Dim Reserved As Integer
    End Structure

    Public Const PORT_TYPE_WRITE As Short = &H1S
    Public Const PORT_TYPE_READ As Short = &H2S
    Public Const PORT_TYPE_REDIRECTED As Short = &H4S
    Public Const PORT_TYPE_NET_ATTACHED As Short = &H8S


    Public Const PRINTER_ENUM_DEFAULT As Short = &H1S
    Public Const PRINTER_ENUM_LOCAL As Short = &H2S
    Public Const PRINTER_ENUM_CONNECTIONS As Short = &H4S
    Public Const PRINTER_ENUM_FAVORITE As Short = &H4S
    Public Const PRINTER_ENUM_NAME As Short = &H8S
    Public Const PRINTER_ENUM_REMOTE As Short = &H10S
    Public Const PRINTER_ENUM_SHARED As Short = &H20S
    Public Const PRINTER_ENUM_NETWORK As Short = &H40S

    Public Const PRINTER_ENUM_EXPAND As Short = &H4000S
    Public Const PRINTER_ENUM_CONTAINER As Short = &H8000S

    Public Const PRINTER_ENUM_ICONMASK As Integer = &HFF0000
    Public Const PRINTER_ENUM_ICON1 As Integer = &H10000
    Public Const PRINTER_ENUM_ICON2 As Integer = &H20000
    Public Const PRINTER_ENUM_ICON3 As Integer = &H40000
    Public Const PRINTER_ENUM_ICON4 As Integer = &H80000
    Public Const PRINTER_ENUM_ICON5 As Integer = &H100000
    Public Const PRINTER_ENUM_ICON6 As Integer = &H200000
    Public Const PRINTER_ENUM_ICON7 As Integer = &H400000
    Public Const PRINTER_ENUM_ICON8 As Integer = &H800000


    Public Const PRINTER_CHANGE_ADD_PRINTER As Short = &H1S
    Public Const PRINTER_CHANGE_SET_PRINTER As Short = &H2S
    Public Const PRINTER_CHANGE_DELETE_PRINTER As Short = &H4S
    Public Const PRINTER_CHANGE_PRINTER As Short = &HFFS
    Public Const PRINTER_CHANGE_ADD_JOB As Short = &H100S
    Public Const PRINTER_CHANGE_SET_JOB As Short = &H200S
    Public Const PRINTER_CHANGE_DELETE_JOB As Short = &H400S
    Public Const PRINTER_CHANGE_WRITE_JOB As Short = &H800S
    Public Const PRINTER_CHANGE_JOB As Short = &HFF00S
    Public Const PRINTER_CHANGE_ADD_FORM As Integer = &H10000
    Public Const PRINTER_CHANGE_SET_FORM As Integer = &H20000
    Public Const PRINTER_CHANGE_DELETE_FORM As Integer = &H40000
    Public Const PRINTER_CHANGE_FORM As Integer = &H70000
    Public Const PRINTER_CHANGE_ADD_PORT As Integer = &H100000
    Public Const PRINTER_CHANGE_CONFIGURE_PORT As Integer = &H200000
    Public Const PRINTER_CHANGE_DELETE_PORT As Integer = &H400000
    Public Const PRINTER_CHANGE_PORT As Integer = &H700000
    Public Const PRINTER_CHANGE_ADD_PRINT_PROCESSOR As Integer = &H1000000
    Public Const PRINTER_CHANGE_DELETE_PRINT_PROCESSOR As Integer = &H4000000
    Public Const PRINTER_CHANGE_PRINT_PROCESSOR As Integer = &H7000000
    Public Const PRINTER_CHANGE_ADD_PRINTER_DRIVER As Integer = &H10000000
    Public Const PRINTER_CHANGE_DELETE_PRINTER_DRIVER As Integer = &H40000000
    Public Const PRINTER_CHANGE_PRINTER_DRIVER As Integer = &H70000000
    Public Const PRINTER_CHANGE_TIMEOUT As Integer = &H80000000
    Public Const PRINTER_CHANGE_ALL As Integer = &H7777FFFF

    Public Const PRINTER_ERROR_INFORMATION As Integer = &H80000000
    Public Const PRINTER_ERROR_WARNING As Integer = &H40000000
    Public Const PRINTER_ERROR_SEVERE As Integer = &H20000000

    Public Const PRINTER_ERROR_OUTOFPAPER As Short = &H1S
    Public Const PRINTER_ERROR_JAM As Short = &H2S
    Public Const PRINTER_ERROR_OUTOFTONER As Short = &H4S


    Public Structure PROVIDOR_INFO_1
        Dim pName As String
        Dim pEnvironment As String
        Dim pDLLName As String
    End Structure

    Public Const SERVER_ACCESS_ADMINISTER As Short = &H1S
    Public Const SERVER_ACCESS_ENUMERATE As Short = &H2S

    Public Const PRINTER_ACCESS_ADMINISTER As Short = &H4S
    Public Const PRINTER_ACCESS_USE As Short = &H8S

    Public Const JOB_ACCESS_ADMINISTER As Short = &H10S

    ' Access rights for print servers

    Public Const SERVER_ALL_ACCESS As Boolean = (STANDARD_RIGHTS_REQUIRED Or SERVER_ACCESS_ADMINISTER Or SERVER_ACCESS_ENUMERATE)
    Public Const SERVER_READ As Boolean = (STANDARD_RIGHTS_READ Or SERVER_ACCESS_ENUMERATE)
    Public Const SERVER_WRITE As Boolean = (STANDARD_RIGHTS_WRITE Or SERVER_ACCESS_ADMINISTER Or SERVER_ACCESS_ENUMERATE)
    Public Const SERVER_EXECUTE As Boolean = (STANDARD_RIGHTS_EXECUTE Or SERVER_ACCESS_ENUMERATE)

    ' Access rights for printers
    Public Const PRINTER_ALL_ACCESS As Boolean = (STANDARD_RIGHTS_REQUIRED Or PRINTER_ACCESS_ADMINISTER Or PRINTER_ACCESS_USE)
    Public Const PRINTER_READ As Boolean = (STANDARD_RIGHTS_READ Or PRINTER_ACCESS_USE)
    Public Const PRINTER_WRITE As Boolean = (STANDARD_RIGHTS_WRITE Or PRINTER_ACCESS_USE)
    Public Const PRINTER_EXECUTE As Boolean = (STANDARD_RIGHTS_EXECUTE Or PRINTER_ACCESS_USE)

    ' Access rights for jobs
    Public Const JOB_ALL_ACCESS As Boolean = (STANDARD_RIGHTS_REQUIRED Or JOB_ACCESS_ADMINISTER)
    Public Const JOB_READ As Boolean = (STANDARD_RIGHTS_READ Or JOB_ACCESS_ADMINISTER)
    Public Const JOB_WRITE As Boolean = (STANDARD_RIGHTS_WRITE Or JOB_ACCESS_ADMINISTER)
    Public Const JOB_EXECUTE As Boolean = (STANDARD_RIGHTS_EXECUTE Or JOB_ACCESS_ADMINISTER)

    '  Windows Network support

    '  RESOURCE ENUMERATION

    Public Const RESOURCE_CONNECTED As Short = &H1S
    Public Const RESOURCE_PUBLICNET As Short = &H2S
    Public Const RESOURCE_REMEMBERED As Short = &H3S

    Public Const RESOURCETYPE_ANY As Short = &H0S
    Public Const RESOURCETYPE_DISK As Short = &H1S
    Public Const RESOURCETYPE_PRINT As Short = &H2S
    Public Const RESOURCETYPE_UNKNOWN As Short = &HFFFFS

    Public Const RESOURCEUSAGE_CONNECTABLE As Short = &H1S
    Public Const RESOURCEUSAGE_CONTAINER As Short = &H2S
    Public Const RESOURCEUSAGE_RESERVED As Integer = &H80000000

    Public Const RESOURCEDISPLAYTYPE_GENERIC As Short = &H0S
    Public Const RESOURCEDISPLAYTYPE_DOMAIN As Short = &H1S
    Public Const RESOURCEDISPLAYTYPE_SERVER As Short = &H2S
    Public Const RESOURCEDISPLAYTYPE_SHARE As Short = &H3S
    Public Const RESOURCEDISPLAYTYPE_FILE As Short = &H4S
    Public Const RESOURCEDISPLAYTYPE_GROUP As Short = &H5S

    Public Structure NETRESOURCE
        Dim dwScope As Integer
        Dim dwType As Integer
        Dim dwDisplayType As Integer
        Dim dwUsage As Integer
        Dim lpLocalName As String
        Dim lpRemoteName As String
        Dim lpComment As String
        Dim lpProvider As String
    End Structure

    Public Const CONNECT_UPDATE_PROFILE As Short = &H1S

    Public Declare Function WNetAddConnection Lib "mpr.dll" Alias "WNetAddConnectionA" (lpszNetPath As String, lpszPassword As String, lpszLocalName As String) As Integer
    'UPGRADE_WARNING: ?? NETRESOURCE ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function WNetAddConnection2 Lib "mpr.dll" Alias "WNetAddConnection2A" (ByRef lpNetResource As NETRESOURCE, lpPassword As String, lpUserName As String, dwFlags As Integer) As Integer
    Public Declare Function WNetCancelConnection Lib "mpr.dll" Alias "WNetCancelConnectionA" (lpszName As String, bForce As Integer) As Integer
    Public Declare Function WNetCancelConnection2 Lib "mpr.dll" Alias "WNetCancelConnection2A" (lpName As String, dwFlags As Integer, fForce As Integer) As Integer
    Public Declare Function WNetGetConnection Lib "mpr.dll" Alias "WNetGetConnectionA" (lpszLocalName As String, lpszRemoteName As String, ByRef cbRemoteName As Integer) As Integer
    'UPGRADE_WARNING: ?? NETRESOURCE ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'Public Declare Function WNetOpenEnum Lib "mpr.dll" Alias "WNetOpenEnumA" (dwScope As Integer, dwType As Integer, dwUsage As Integer, ByRef lpNetResource As NETRESOURCE, ByRef lphEnum As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function WNetEnumResource Lib "mpr.dll" Alias "WNetEnumResourceA" (hEnum As Integer, ByRef lpcCount As Integer, ByRef lpBuffer As Object, ByRef lpBufferSize As Integer) As Integer
    Public Declare Function WNetCloseEnum Lib "mpr.dll" (hEnum As Integer) As Integer

    'Public Declare Function WNetGetUser Lib "mpr.dll" Alias "WNetGetUserA" (lpName As String, lpUserName As String, ByRef lpnLength As Integer) As Integer

    Public Declare Function WNetConnectionDialog Lib "mpr.dll" (hWnd As Integer, dwType As Integer) As Integer
    Public Declare Function WNetDisconnectDialog Lib "mpr.dll" (hWnd As Integer, dwType As Integer) As Integer

    Public Declare Function WNetGetLastError Lib "mpr.dll" Alias "WNetGetLastErrorA" (ByRef lpError As Integer, lpErrorBuf As String, nErrorBufSize As Integer, lpNameBuf As String, nNameBufSize As Integer) As Integer

    ' Status Codes

    ' This section is provided for backward compatibility.  Use of the ERROR_
    ' codes is preferred.  The WN_ error codes may not be available in future
    ' releases.

    ' General

    Public Const WN_SUCCESS As Short = NO_ERROR
    Public Const WN_NOT_SUPPORTED As Short = ERROR_NOT_SUPPORTED
    Public Const WN_NET_ERROR As Short = ERROR_UNEXP_NET_ERR
    Public Const WN_MORE_DATA As Short = ERROR_MORE_DATA
    Public Const WN_BAD_POINTER As Short = ERROR_INVALID_ADDRESS
    Public Const WN_BAD_VALUE As Short = ERROR_INVALID_PARAMETER
    Public Const WN_BAD_PASSWORD As Short = ERROR_INVALID_PASSWORD
    Public Const WN_ACCESS_DENIED As Short = ERROR_ACCESS_DENIED
    Public Const WN_FUNCTION_BUSY As Short = ERROR_BUSY
    Public Const WN_WINDOWS_ERROR As Short = ERROR_UNEXP_NET_ERR
    Public Const WN_BAD_USER As Short = ERROR_BAD_USERNAME
    Public Const WN_OUT_OF_MEMORY As Short = ERROR_NOT_ENOUGH_MEMORY
    Public Const WN_NO_NETWORK As Short = ERROR_NO_NETWORK
    Public Const WN_EXTENDED_ERROR As Short = ERROR_EXTENDED_ERROR

    ' Connection

    Public Const WN_NOT_CONNECTED As Short = ERROR_NOT_CONNECTED
    Public Const WN_OPEN_FILES As Short = ERROR_OPEN_FILES
    Public Const WN_DEVICE_IN_USE As Short = ERROR_DEVICE_IN_USE
    Public Const WN_BAD_NETNAME As Short = ERROR_BAD_NET_NAME
    Public Const WN_BAD_LOCALNAME As Short = ERROR_BAD_DEVICE
    Public Const WN_ALREADY_CONNECTED As Short = ERROR_ALREADY_ASSIGNED
    Public Const WN_DEVICE_ERROR As Short = ERROR_GEN_FAILURE
    Public Const WN_CONNECTION_CLOSED As Short = ERROR_CONNECTION_UNAVAIL
    Public Const WN_NO_NET_OR_BAD_PATH As Short = ERROR_NO_NET_OR_BAD_PATH
    Public Const WN_BAD_PROVIDER As Short = ERROR_BAD_PROVIDER
    Public Const WN_CANNOT_OPEN_PROFILE As Short = ERROR_CANNOT_OPEN_PROFILE
    Public Const WN_BAD_PROFILE As Short = ERROR_BAD_PROFILE

    ' Enumeration

    Public Const WN_BAD_HANDLE As Short = ERROR_INVALID_HANDLE
    Public Const WN_NO_MORE_ENTRIES As Short = ERROR_NO_MORE_ITEMS
    Public Const WN_NOT_CONTAINER As Short = ERROR_NOT_CONTAINER

    Public Const WN_NO_ERROR As Short = NO_ERROR

    ' This section contains the definitions
    ' for portable NetBIOS 3.0 support.

    Public Const NCBNAMSZ As Short = 16 '  absolute length of a net name
    Public Const MAX_LANA As Short = 254 '  lana's in range 0 to MAX_LANA

    Public Structure NCB
        Dim ncb_command As Short
        Dim ncb_retcode As Short
        Dim ncb_lsn As Short
        Dim ncb_num As Short
        Dim ncb_buffer As String
        Dim ncb_length As Short
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(NCBNAMSZ), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=NCBNAMSZ)> Public ncb_callname() As Char
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(NCBNAMSZ), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=NCBNAMSZ)> Public ncb_name() As Char
        Dim ncb_rto As Short
        Dim ncb_sto As Short
        Dim ncb_post As Integer
        Dim ncb_lana_num As Short
        Dim ncb_cmd_cplt As Short
        <VBFixedArray(10)> Dim ncb_reserve() As Byte ' Reserved, must be 0
        Dim ncb_event As Integer

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim ncb_reserve(10)
        End Sub
    End Structure

    Public Structure ADAPTER_STATUS
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(6), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=6)> Public adapter_address() As Char
        Dim rev_major As Short
        Dim reserved0 As Short
        Dim adapter_type As Short
        Dim rev_minor As Short
        Dim duration As Short
        Dim frmr_recv As Short
        Dim frmr_xmit As Short
        Dim iframe_recv_err As Short
        Dim xmit_aborts As Short
        Dim xmit_success As Integer
        Dim recv_success As Integer
        Dim iframe_xmit_err As Short
        Dim recv_buff_unavail As Short
        Dim t1_timeouts As Short
        Dim ti_timeouts As Short
        Dim Reserved1 As Integer
        Dim free_ncbs As Short
        Dim max_cfg_ncbs As Short
        Dim max_ncbs As Short
        Dim xmit_buf_unavail As Short
        Dim max_dgram_size As Short
        Dim pending_sess As Short
        Dim max_cfg_sess As Short
        Dim max_sess As Short
        Dim max_sess_pkt_size As Short
        Dim name_count As Short
    End Structure

    Public Structure NAME_BUFFER
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(NCBNAMSZ), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=NCBNAMSZ)> Public name() As Char
        Dim name_num As Short
        Dim name_flags As Short
    End Structure

    ' values for name_flags bits.

    Public Const NAME_FLAGS_MASK As Short = &H87S

    Public Const GROUP_NAME As Short = &H80S
    Public Const UNIQUE_NAME As Short = &H0S

    Public Const REGISTERING As Short = &H0S
    Public Const REGISTERED As Short = &H4S
    Public Const DEREGISTERED As Short = &H5S
    Public Const DUPLICATE As Short = &H6S
    Public Const DUPLICATE_DEREG As Short = &H7S

    Public Structure SESSION_HEADER
        Dim sess_name As Short
        Dim num_sess As Short
        Dim rcv_dg_outstanding As Short
        Dim rcv_any_outstanding As Short
    End Structure

    Public Structure SESSION_BUFFER
        Dim lsn As Short
        Dim State As Short
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(NCBNAMSZ), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=NCBNAMSZ)> Public local_name() As Char
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(NCBNAMSZ), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=NCBNAMSZ)> Public remote_name() As Char
        Dim rcvs_outstanding As Short
        Dim sends_outstanding As Short
    End Structure

    ' Values for state
    Public Const LISTEN_OUTSTANDING As Short = &H1S
    Public Const CALL_PENDING As Short = &H2S
    Public Const SESSION_ESTABLISHED As Short = &H3S
    Public Const HANGUP_PENDING As Short = &H4S
    Public Const HANGUP_COMPLETE As Short = &H5S
    Public Const SESSION_ABORTED As Short = &H6S

    Public Structure LANA_ENUM
        Dim Length As Short
        <VBFixedArray(MAX_LANA)> Dim lana() As Short

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim lana(MAX_LANA)
        End Sub
    End Structure

    Public Structure FIND_NAME_HEADER
        Dim node_count As Short
        Dim Reserved As Short
        Dim unique_group As Short
    End Structure

    Public Structure FIND_NAME_BUFFER
        Dim Length As Short
        Dim access_control As Short
        Dim frame_control As Short
        <VBFixedArray(6)> Dim destination_addr() As Short
        <VBFixedArray(6)> Dim source_addr() As Short
        <VBFixedArray(18)> Dim routing_info() As Short

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim destination_addr(6)
            ReDim source_addr(6)
            ReDim routing_info(18)
        End Sub
    End Structure

    Public Structure ACTION_HEADER
        Dim transport_id As Integer
        Dim action_code As Short
        Dim Reserved As Short
    End Structure

    ' Values for transport_id
    Public Const ALL_TRANSPORTS As String = "M\0\0\0"
    Public Const MS_NBF As String = "MNBF"

    ' NCB Command codes
    Public Const NCBCALL As Short = &H10S '  NCB CALL
    Public Const NCBLISTEN As Short = &H11S '  NCB LISTEN
    Public Const NCBHANGUP As Short = &H12S '  NCB HANG UP
    Public Const NCBSEND As Short = &H14S '  NCB SEND
    Public Const NCBRECV As Short = &H15S '  NCB RECEIVE
    Public Const NCBRECVANY As Short = &H16S '  NCB RECEIVE ANY
    Public Const NCBCHAINSEND As Short = &H17S '  NCB CHAIN SEND
    Public Const NCBDGSEND As Short = &H20S '  NCB SEND DATAGRAM
    Public Const NCBDGRECV As Short = &H21S '  NCB RECEIVE DATAGRAM
    Public Const NCBDGSENDBC As Short = &H22S '  NCB SEND BROADCAST DATAGRAM
    Public Const NCBDGRECVBC As Short = &H23S '  NCB RECEIVE BROADCAST DATAGRAM
    Public Const NCBADDNAME As Short = &H30S '  NCB ADD NAME
    Public Const NCBDELNAME As Short = &H31S '  NCB DELETE NAME
    Public Const NCBRESET As Short = &H32S '  NCB RESET
    Public Const NCBASTAT As Short = &H33S '  NCB ADAPTER STATUS
    Public Const NCBSSTAT As Short = &H34S '  NCB SESSION STATUS
    Public Const NCBCANCEL As Short = &H35S '  NCB CANCEL
    Public Const NCBADDGRNAME As Short = &H36S '  NCB ADD GROUP NAME
    Public Const NCBENUM As Short = &H37S '  NCB ENUMERATE LANA NUMBERS
    Public Const NCBUNLINK As Short = &H70S '  NCB UNLINK
    Public Const NCBSENDNA As Short = &H71S '  NCB SEND NO ACK
    Public Const NCBCHAINSENDNA As Short = &H72S '  NCB CHAIN SEND NO ACK
    Public Const NCBLANSTALERT As Short = &H73S '  NCB LAN STATUS ALERT
    Public Const NCBACTION As Short = &H77S '  NCB ACTION
    Public Const NCBFINDNAME As Short = &H78S '  NCB FIND NAME
    Public Const NCBTRACE As Short = &H79S '  NCB TRACE

    Public Const ASYNCH As Short = &H80S '  high bit set == asynchronous

    ' NCB Return codes
    Public Const NRC_GOODRET As Short = &H0S '  good return
    '  also returned when ASYNCH request accepted
    Public Const NRC_BUFLEN As Short = &H1S '  illegal buffer length
    Public Const NRC_ILLCMD As Short = &H3S '  illegal command
    Public Const NRC_CMDTMO As Short = &H5S '  command timed out
    Public Const NRC_INCOMP As Short = &H6S '  message incomplete, issue another command
    Public Const NRC_BADDR As Short = &H7S '  illegal buffer address
    Public Const NRC_SNUMOUT As Short = &H8S '  session number out of range
    Public Const NRC_NORES As Short = &H9S '  no resource available
    Public Const NRC_SCLOSED As Short = &HAS '  session closed
    Public Const NRC_CMDCAN As Short = &HBS '  command cancelled
    Public Const NRC_DUPNAME As Short = &HDS '  duplicate name
    Public Const NRC_NAMTFUL As Short = &HES '  name table full
    Public Const NRC_ACTSES As Short = &HFS '  no deletions, name has active sessions
    Public Const NRC_LOCTFUL As Short = &H11S '  local session table full
    Public Const NRC_REMTFUL As Short = &H12S '  remote session table full
    Public Const NRC_ILLNN As Short = &H13S '  illegal name number
    Public Const NRC_NOCALL As Short = &H14S '  no callname
    Public Const NRC_NOWILD As Short = &H15S '  cannot put  in NCB_NAME
    Public Const NRC_INUSE As Short = &H16S '  name in use on remote adapter
    Public Const NRC_NAMERR As Short = &H17S '  name deleted
    Public Const NRC_SABORT As Short = &H18S '  session ended abnormally
    Public Const NRC_NAMCONF As Short = &H19S '  name conflict detected
    Public Const NRC_IFBUSY As Short = &H21S '  interface busy, IRET before retrying
    Public Const NRC_TOOMANY As Short = &H22S '  too many commands outstanding, retry later
    Public Const NRC_BRIDGE As Short = &H23S '  ncb_lana_num field invalid
    Public Const NRC_CANOCCR As Short = &H24S '  command completed while cancel occurring
    Public Const NRC_CANCEL As Short = &H26S '  command not valid to cancel
    Public Const NRC_DUPENV As Short = &H30S '  name defined by anther local process
    Public Const NRC_ENVNOTDEF As Short = &H34S '  environment undefined. RESET required
    Public Const NRC_OSRESNOTAV As Short = &H35S '  required OS resources exhausted
    Public Const NRC_MAXAPPS As Short = &H36S '  max number of applications exceeded
    Public Const NRC_NOSAPS As Short = &H37S '  no saps available for netbios
    Public Const NRC_NORESOURCES As Short = &H38S '  requested resources are not available
    Public Const NRC_INVADDRESS As Short = &H39S '  invalid ncb address or length > segment
    Public Const NRC_INVDDID As Short = &H3BS '  invalid NCB DDID
    Public Const NRC_LOCKFAIL As Short = &H3CS '  lock of user area failed
    Public Const NRC_OPENERR As Short = &H3FS '  NETBIOS not loaded
    Public Const NRC_SYSTEM As Short = &H40S '  system error

    Public Const NRC_PENDING As Short = &HFFS '  asynchronous command is not yet finished

    'UPGRADE_WARNING: ?? NCB ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function Netbios Lib "netapi32.dll" (ByRef pncb As NCB) As Byte

    ' Legal values for expression in except().
    Public Const EXCEPTION_EXECUTE_HANDLER As Short = 1
    Public Const EXCEPTION_CONTINUE_SEARCH As Short = 0
    Public Const EXCEPTION_CONTINUE_EXECUTION As Short = -1

    ' UI dialog constants and types

    ' ----Constants--------------------------------------------------------------
    Public Const ctlFirst As Short = &H400S
    Public Const ctlLast As Short = &H4FFS
    '  Push buttons
    Public Const psh1 As Short = &H400S
    Public Const psh2 As Short = &H401S
    Public Const psh3 As Short = &H402S
    Public Const psh4 As Short = &H403S
    Public Const psh5 As Short = &H404S
    Public Const psh6 As Short = &H405S
    Public Const psh7 As Short = &H406S
    Public Const psh8 As Short = &H407S
    Public Const psh9 As Short = &H408S
    Public Const psh10 As Short = &H409S
    Public Const psh11 As Short = &H40AS
    Public Const psh12 As Short = &H40BS
    Public Const psh13 As Short = &H40CS
    Public Const psh14 As Short = &H40DS
    Public Const psh15 As Short = &H40ES
    Public Const pshHelp As Short = psh15
    Public Const psh16 As Short = &H40FS
    '  Checkboxes
    Public Const chx1 As Short = &H410S
    Public Const chx2 As Short = &H411S
    Public Const chx3 As Short = &H412S
    Public Const chx4 As Short = &H413S
    Public Const chx5 As Short = &H414S
    Public Const chx6 As Short = &H415S
    Public Const chx7 As Short = &H416S
    Public Const chx8 As Short = &H417S
    Public Const chx9 As Short = &H418S
    Public Const chx10 As Short = &H419S
    Public Const chx11 As Short = &H41AS
    Public Const chx12 As Short = &H41BS
    Public Const chx13 As Short = &H41CS
    Public Const chx14 As Short = &H41DS
    Public Const chx15 As Short = &H41ES
    Public Const chx16 As Short = &H41DS
    '  Radio buttons
    Public Const rad1 As Short = &H420S
    Public Const rad2 As Short = &H421S
    Public Const rad3 As Short = &H422S
    Public Const rad4 As Short = &H423S
    Public Const rad5 As Short = &H424S
    Public Const rad6 As Short = &H425S
    Public Const rad7 As Short = &H426S
    Public Const rad8 As Short = &H427S
    Public Const rad9 As Short = &H428S
    Public Const rad10 As Short = &H429S
    Public Const rad11 As Short = &H42AS
    Public Const rad12 As Short = &H42BS
    Public Const rad13 As Short = &H42CS
    Public Const rad14 As Short = &H42DS
    Public Const rad15 As Short = &H42ES
    Public Const rad16 As Short = &H42FS
    '  Groups, frames, rectangles, and icons
    Public Const grp1 As Short = &H430S
    Public Const grp2 As Short = &H431S
    Public Const grp3 As Short = &H432S
    Public Const grp4 As Short = &H433S
    Public Const frm1 As Short = &H434S
    Public Const frm2 As Short = &H435S
    Public Const frm3 As Short = &H436S
    Public Const frm4 As Short = &H437S
    Public Const rct1 As Short = &H438S
    Public Const rct2 As Short = &H439S
    Public Const rct3 As Short = &H43AS
    Public Const rct4 As Short = &H43BS
    Public Const ico1 As Short = &H43CS
    Public Const ico2 As Short = &H43DS
    Public Const ico3 As Short = &H43ES
    Public Const ico4 As Short = &H43FS
    '  Static text
    Public Const stc1 As Short = &H440S
    Public Const stc2 As Short = &H441S
    Public Const stc3 As Short = &H442S
    Public Const stc4 As Short = &H443S
    Public Const stc5 As Short = &H444S
    Public Const stc6 As Short = &H445S
    Public Const stc7 As Short = &H446S
    Public Const stc8 As Short = &H447S
    Public Const stc9 As Short = &H448S
    Public Const stc10 As Short = &H449S
    Public Const stc11 As Short = &H44AS
    Public Const stc12 As Short = &H44BS
    Public Const stc13 As Short = &H44CS
    Public Const stc14 As Short = &H44DS
    Public Const stc15 As Short = &H44ES
    Public Const stc16 As Short = &H44FS
    Public Const stc17 As Short = &H450S
    Public Const stc18 As Short = &H451S
    Public Const stc19 As Short = &H452S
    Public Const stc20 As Short = &H453S
    Public Const stc21 As Short = &H454S
    Public Const stc22 As Short = &H455S
    Public Const stc23 As Short = &H456S
    Public Const stc24 As Short = &H457S
    Public Const stc25 As Short = &H458S
    Public Const stc26 As Short = &H459S
    Public Const stc27 As Short = &H45AS
    Public Const stc28 As Short = &H45BS
    Public Const stc29 As Short = &H45CS
    Public Const stc30 As Short = &H45DS
    Public Const stc31 As Short = &H45ES
    Public Const stc32 As Short = &H45FS
    '  Listboxes
    Public Const lst1 As Short = &H460S
    Public Const lst2 As Short = &H461S
    Public Const lst3 As Short = &H462S
    Public Const lst4 As Short = &H463S
    Public Const lst5 As Short = &H464S
    Public Const lst6 As Short = &H465S
    Public Const lst7 As Short = &H466S
    Public Const lst8 As Short = &H467S
    Public Const lst9 As Short = &H468S
    Public Const lst10 As Short = &H469S
    Public Const lst11 As Short = &H46AS
    Public Const lst12 As Short = &H46BS
    Public Const lst13 As Short = &H46CS
    Public Const lst14 As Short = &H46DS
    Public Const lst15 As Short = &H46ES
    Public Const lst16 As Short = &H46FS
    '  Combo boxes
    Public Const cmb1 As Short = &H470S
    Public Const cmb2 As Short = &H471S
    Public Const cmb3 As Short = &H472S
    Public Const cmb4 As Short = &H473S
    Public Const cmb5 As Short = &H474S
    Public Const cmb6 As Short = &H475S
    Public Const cmb7 As Short = &H476S
    Public Const cmb8 As Short = &H477S
    Public Const cmb9 As Short = &H478S
    Public Const cmb10 As Short = &H479S
    Public Const cmb11 As Short = &H47AS
    Public Const cmb12 As Short = &H47BS
    Public Const cmb13 As Short = &H47CS
    Public Const cmb14 As Short = &H47DS
    Public Const cmb15 As Short = &H47ES
    Public Const cmb16 As Short = &H47FS
    '  Edit controls
    Public Const edt1 As Short = &H480S
    Public Const edt2 As Short = &H481S
    Public Const edt3 As Short = &H482S
    Public Const edt4 As Short = &H483S
    Public Const edt5 As Short = &H484S
    Public Const edt6 As Short = &H485S
    Public Const edt7 As Short = &H486S
    Public Const edt8 As Short = &H487S
    Public Const edt9 As Short = &H488S
    Public Const edt10 As Short = &H489S
    Public Const edt11 As Short = &H48AS
    Public Const edt12 As Short = &H48BS
    Public Const edt13 As Short = &H48CS
    Public Const edt14 As Short = &H48DS
    Public Const edt15 As Short = &H48ES
    Public Const edt16 As Short = &H48FS
    '  Scroll bars
    Public Const scr1 As Short = &H490S
    Public Const scr2 As Short = &H491S
    Public Const scr3 As Short = &H492S
    Public Const scr4 As Short = &H493S
    Public Const scr5 As Short = &H494S
    Public Const scr6 As Short = &H495S
    Public Const scr7 As Short = &H496S
    Public Const scr8 As Short = &H497S

    Public Const FILEOPENORD As Short = 1536
    Public Const MULTIFILEOPENORD As Short = 1537
    Public Const PRINTDLGORD As Short = 1538
    Public Const PRNSETUPDLGORD As Short = 1539
    Public Const FINDDLGORD As Short = 1540
    Public Const REPLACEDLGORD As Short = 1541
    Public Const FONTDLGORD As Short = 1542
    Public Const FORMATDLGORD31 As Short = 1543
    Public Const FORMATDLGORD30 As Short = 1544

    Public Structure CRGB
        Dim bRed As Byte
        Dim bGreen As Byte
        Dim bBlue As Byte
        Dim bExtra As Byte
    End Structure

    ' -----------------
    ' ADVAPI32
    ' -----------------

    ' function prototypes, constants, and type definitions
    ' for Windows 32-bit Registry API

    Public Const HKEY_CLASSES_ROOT As Integer = &H80000000
    Public Const HKEY_CURRENT_USER As Integer = &H80000001
    Public Const HKEY_LOCAL_MACHINE As Integer = &H80000002
    Public Const HKEY_USERS As Integer = &H80000003
    Public Const HKEY_PERFORMANCE_DATA As Integer = &H80000004
    Public Const HKEY_CURRENT_CONFIG As Integer = &H80000005
    Public Const HKEY_DYN_DATA As Integer = &H80000006


    ' Service database names
    Public Const SERVICES_ACTIVE_DATABASE As String = "ServicesActive"
    Public Const SERVICES_FAILED_DATABASE As String = "ServicesFailed"

    ' Value to indicate no change to an optional parameter
    Public Const SERVICE_NO_CHANGE As Short = &HFFFFS

    ' Service State -- for Enum Requests (Bit Mask)
    Public Const SERVICE_ACTIVE As Short = &H1S
    Public Const SERVICE_INACTIVE As Short = &H2S
    Public Const SERVICE_STATE_ALL As Boolean = (SERVICE_ACTIVE Or SERVICE_INACTIVE)

    ' Controls
    Public Const SERVICE_CONTROL_STOP As Short = &H1S
    Public Const SERVICE_CONTROL_PAUSE As Short = &H2S
    Public Const SERVICE_CONTROL_CONTINUE As Short = &H3S
    Public Const SERVICE_CONTROL_INTERROGATE As Short = &H4S
    Public Const SERVICE_CONTROL_SHUTDOWN As Short = &H5S

    ' Service State -- for CurrentState
    Public Const SERVICE_STOPPED As Short = &H1S
    Public Const SERVICE_START_PENDING As Short = &H2S
    Public Const SERVICE_STOP_PENDING As Short = &H3S
    Public Const SERVICE_RUNNING As Short = &H4S
    Public Const SERVICE_CONTINUE_PENDING As Short = &H5S
    Public Const SERVICE_PAUSE_PENDING As Short = &H6S
    Public Const SERVICE_PAUSED As Short = &H7S

    ' Controls Accepted  (Bit Mask)
    Public Const SERVICE_ACCEPT_STOP As Short = &H1S
    Public Const SERVICE_ACCEPT_PAUSE_CONTINUE As Short = &H2S
    Public Const SERVICE_ACCEPT_SHUTDOWN As Short = &H4S

    ' Service Control Manager object specific access types
    Public Const SC_MANAGER_CONNECT As Short = &H1S
    Public Const SC_MANAGER_CREATE_SERVICE As Short = &H2S
    Public Const SC_MANAGER_ENUMERATE_SERVICE As Short = &H4S
    Public Const SC_MANAGER_LOCK As Short = &H8S
    Public Const SC_MANAGER_QUERY_LOCK_STATUS As Short = &H10S
    Public Const SC_MANAGER_MODIFY_BOOT_CONFIG As Short = &H20S

    Public Const SC_MANAGER_ALL_ACCESS As Boolean = (STANDARD_RIGHTS_REQUIRED Or SC_MANAGER_CONNECT Or SC_MANAGER_CREATE_SERVICE Or SC_MANAGER_ENUMERATE_SERVICE Or SC_MANAGER_LOCK Or SC_MANAGER_QUERY_LOCK_STATUS Or SC_MANAGER_MODIFY_BOOT_CONFIG)

    ' Service object specific access type
    Public Const SERVICE_QUERY_CONFIG As Short = &H1S
    Public Const SERVICE_CHANGE_CONFIG As Short = &H2S
    Public Const SERVICE_QUERY_STATUS As Short = &H4S
    Public Const SERVICE_ENUMERATE_DEPENDENTS As Short = &H8S
    Public Const SERVICE_START As Short = &H10S
    Public Const SERVICE_STOP As Short = &H20S
    Public Const SERVICE_PAUSE_CONTINUE As Short = &H40S
    Public Const SERVICE_INTERROGATE As Short = &H80S
    Public Const SERVICE_USER_DEFINED_CONTROL As Short = &H100S

    Public Const SERVICE_ALL_ACCESS As Boolean = (STANDARD_RIGHTS_REQUIRED Or SERVICE_QUERY_CONFIG Or SERVICE_CHANGE_CONFIG Or SERVICE_QUERY_STATUS Or SERVICE_ENUMERATE_DEPENDENTS Or SERVICE_START Or SERVICE_STOP Or SERVICE_PAUSE_CONTINUE Or SERVICE_INTERROGATE Or SERVICE_USER_DEFINED_CONTROL)


    Public Structure SERVICE_STATUS
        Dim dwServiceType As Integer
        Dim dwCurrentState As Integer
        Dim dwControlsAccepted As Integer
        Dim dwWin32ExitCode As Integer
        Dim dwServiceSpecificExitCode As Integer
        Dim dwCheckPoint As Integer
        Dim dwWaitHint As Integer
    End Structure

    Public Structure ENUM_SERVICE_STATUS
        Dim lpServiceName As String
        Dim lpDisplayName As String
        Dim ServiceStatus As SERVICE_STATUS
    End Structure

    Public Structure QUERY_SERVICE_LOCK_STATUS
        Dim fIsLocked As Integer
        Dim lpLockOwner As String
        Dim dwLockDuration As Integer
    End Structure

    Public Structure QUERY_SERVICE_CONFIG
        Dim dwServiceType As Integer
        Dim dwStartType As Integer
        Dim dwErrorControl As Integer
        Dim lpBinaryPathName As String
        Dim lpLoadOrderGroup As String
        Dim dwTagId As Integer
        Dim lpDependencies As String
        Dim lpServiceStartName As String
        Dim lpDisplayName As String
    End Structure

    Public Structure SERVICE_TABLE_ENTRY
        Dim lpServiceName As String
        Dim lpServiceProc As Integer
    End Structure


    ' ++ BUILD Version: 0010    '  Increment this if a change has global effects
    ' Copyright (c) 1995  Microsoft Corporation
    ' Module Name:
    '     winsvc.h
    ' Abstract:
    '     Header file for the Service Control Manager
    ' Environment:
    '     User Mode - Win32
    ' --*/
    '
    '  Constants

    '  Character to designate that a name is a group
    '
    Public Const SC_GROUP_IDENTIFIER As String = "+"


    Public Structure LARGE_INTEGER
        Dim lowpart As Integer
        Dim highpart As Integer
    End Structure


    ' Section for Performance Monitor data

    Public Const PERF_DATA_VERSION As Short = 1
    Public Const PERF_DATA_REVISION As Short = 1

    Public Structure PERF_DATA_BLOCK
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(4), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=4)> Public Signature() As Char
        Dim LittleEndian As Integer
        Dim Version As Integer
        Dim Revision As Integer
        Dim TotalByteLength As Integer
        Dim HeaderLength As Integer
        Dim NumObjectTypes As Integer
        Dim DefaultObject As Integer
        'UPGRADE_NOTE: SystemTime ???? SystemTime_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
        Dim SystemTime_Renamed As SystemTime
        Dim PerfTime As LARGE_INTEGER
        Dim PerfFreq As LARGE_INTEGER
        Dim PerTime100nSec As LARGE_INTEGER
        Dim SystemNameLength As Integer
        Dim SystemNameOffset As Integer
    End Structure

    Public Structure PERF_OBJECT_TYPE
        Dim TotalByteLength As Integer
        Dim DefinitionLength As Integer
        Dim HeaderLength As Integer
        Dim ObjectNameTitleIndex As Integer
        Dim ObjectNameTitle As String
        Dim ObjectHelpTitleIndex As Integer
        Dim ObjectHelpTitle As String
        Dim DetailLevel As Integer
        Dim NumCounters As Integer
        Dim DefaultCounter As Integer
        Dim NumInstances As Integer
        Dim CodePage As Integer
        Dim PerfTime As LARGE_INTEGER
        Dim PerfFreq As LARGE_INTEGER
    End Structure

    Public Const PERF_NO_INSTANCES As Short = -1 '  no instances

    ' The counter type is the "or" of the following values as described below
    '
    ' select one of the following to indicate the counter's data size
    Public Const PERF_SIZE_DWORD As Short = &H0S
    Public Const PERF_SIZE_LARGE As Short = &H100S
    Public Const PERF_SIZE_ZERO As Short = &H200S '  for Zero Length fields
    Public Const PERF_SIZE_VARIABLE_LEN As Short = &H300S '  length is in CounterLength field of Counter Definition struct

    ' select one of the following values to indicate the counter field usage
    Public Const PERF_TYPE_NUMBER As Short = &H0S '  a number (not a counter)
    Public Const PERF_TYPE_COUNTER As Short = &H400S '  an increasing numeric value
    Public Const PERF_TYPE_TEXT As Short = &H800S '  a text field
    Public Const PERF_TYPE_ZERO As Short = &HC00S '  displays a zero

    ' If the PERF_TYPE_NUMBER field was selected, then select one of the
    ' following to describe the Number
    Public Const PERF_NUMBER_HEX As Short = &H0S '  display as HEX value
    Public Const PERF_NUMBER_DECIMAL As Integer = &H10000 '  display as a decimal integer
    Public Const PERF_NUMBER_DEC_1000 As Integer = &H20000 '  display as a decimal/1000
    '
    ' If the PERF_TYPE_COUNTER value was selected then select one of the
    ' following to indicate the type of counter
    Public Const PERF_COUNTER_VALUE As Short = &H0S '  display counter value
    Public Const PERF_COUNTER_RATE As Integer = &H10000 '  divide ctr / delta time
    Public Const PERF_COUNTER_FRACTION As Integer = &H20000 '  divide ctr / base
    Public Const PERF_COUNTER_BASE As Integer = &H30000 '  base value used in fractions
    Public Const PERF_COUNTER_ELAPSED As Integer = &H40000 '  subtract counter from current time
    Public Const PERF_COUNTER_QUEUELEN As Integer = &H50000 '  Use Queuelen processing func.
    Public Const PERF_COUNTER_HISTOGRAM As Integer = &H60000 '  Counter begins or ends a histogram

    ' If the PERF_TYPE_TEXT value was selected, then select one of the
    ' following to indicate the type of TEXT data.
    Public Const PERF_TEXT_UNICODE As Short = &H0S '  type of text in text field
    Public Const PERF_TEXT_ASCII As Integer = &H10000 '  ASCII using the CodePage field

    ' Timer SubTypes
    Public Const PERF_TIMER_TICK As Short = &H0S '  use system perf. freq for base
    Public Const PERF_TIMER_100NS As Integer = &H100000 '  use 100 NS timer time base units
    Public Const PERF_OBJECT_TIMER As Integer = &H200000 '  use the object timer freq

    ' Any types that have calculations performed can use one or more of
    ' the following calculation modification flags listed here
    Public Const PERF_DELTA_COUNTER As Integer = &H400000 '  compute difference first
    Public Const PERF_DELTA_BASE As Integer = &H800000 '  compute base diff as well
    Public Const PERF_INVERSE_COUNTER As Integer = &H1000000 '  show as 1.00-value (assumes:
    Public Const PERF_MULTI_COUNTER As Integer = &H2000000 '  sum of multiple instances

    ' Select one of the following values to indicate the display suffix (if any)
    Public Const PERF_DISPLAY_NO_SUFFIX As Short = &H0S '  no suffix
    Public Const PERF_DISPLAY_PER_SEC As Integer = &H10000000 '  "/sec"
    Public Const PERF_DISPLAY_PERCENT As Integer = &H20000000 '  "%"
    Public Const PERF_DISPLAY_SECONDS As Integer = &H30000000 '  "secs"
    Public Const PERF_DISPLAY_NOSHOW As Integer = &H40000000 '  value is not displayed

    ' Predefined counter types

    ' 32-bit Counter.  Divide delta by delta time.  Display suffix: "/sec"
    Public Const PERF_COUNTER_COUNTER As Boolean = (PERF_SIZE_DWORD Or PERF_TYPE_COUNTER Or PERF_COUNTER_RATE Or PERF_TIMER_TICK Or PERF_DELTA_COUNTER Or PERF_DISPLAY_PER_SEC)

    ' 64-bit Timer.  Divide delta by delta time.  Display suffix: "%"
    Public Const PERF_COUNTER_TIMER As Boolean = (PERF_SIZE_LARGE Or PERF_TYPE_COUNTER Or PERF_COUNTER_RATE Or PERF_TIMER_TICK Or PERF_DELTA_COUNTER Or PERF_DISPLAY_PERCENT)

    ' Queue Length Space-Time Product. Divide delta by delta time. No Display Suffix.
    Public Const PERF_COUNTER_QUEUELEN_TYPE As Boolean = (PERF_SIZE_DWORD Or PERF_TYPE_COUNTER Or PERF_COUNTER_QUEUELEN Or PERF_TIMER_TICK Or PERF_DELTA_COUNTER Or PERF_DISPLAY_NO_SUFFIX)

    ' 64-bit Counter.  Divide delta by delta time. Display Suffix: "/sec"
    Public Const PERF_COUNTER_BULK_COUNT As Boolean = (PERF_SIZE_LARGE Or PERF_TYPE_COUNTER Or PERF_COUNTER_RATE Or PERF_TIMER_TICK Or PERF_DELTA_COUNTER Or PERF_DISPLAY_PER_SEC)

    ' Indicates the counter is not a  counter but rather Unicode text Display as text.
    Public Const PERF_COUNTER_TEXT As Boolean = (PERF_SIZE_VARIABLE_LEN Or PERF_TYPE_TEXT Or PERF_TEXT_UNICODE Or PERF_DISPLAY_NO_SUFFIX)

    ' Indicates the data is a counter  which should not be
    ' time averaged on display (such as an error counter on a serial line)
    ' Display as is.  No Display Suffix.
    Public Const PERF_COUNTER_RAWCOUNT As Boolean = (PERF_SIZE_DWORD Or PERF_TYPE_NUMBER Or PERF_NUMBER_DECIMAL Or PERF_DISPLAY_NO_SUFFIX)

    ' A count which is either 1 or 0 on each sampling interrupt (% busy)
    ' Divide delta by delta base. Display Suffix: "%"
    Public Const PERF_SAMPLE_FRACTION As Boolean = (PERF_SIZE_DWORD Or PERF_TYPE_COUNTER Or PERF_COUNTER_FRACTION Or PERF_DELTA_COUNTER Or PERF_DELTA_BASE Or PERF_DISPLAY_PERCENT)

    ' A count which is sampled on each sampling interrupt (queue length)
    ' Divide delta by delta time. No Display Suffix.
    Public Const PERF_SAMPLE_COUNTER As Boolean = (PERF_SIZE_DWORD Or PERF_TYPE_COUNTER Or PERF_COUNTER_RATE Or PERF_TIMER_TICK Or PERF_DELTA_COUNTER Or PERF_DISPLAY_NO_SUFFIX)

    ' A label: no data is associated with this counter (it has 0 length)
    ' Do not display.
    Public Const PERF_COUNTER_NODATA As Boolean = (PERF_SIZE_ZERO Or PERF_DISPLAY_NOSHOW)

    ' 64-bit Timer inverse (e.g., idle is measured, but display busy  As Integer)
    ' Display 100 - delta divided by delta time.  Display suffix: "%"
    Public Const PERF_COUNTER_TIMER_INV As Boolean = (PERF_SIZE_LARGE Or PERF_TYPE_COUNTER Or PERF_COUNTER_RATE Or PERF_TIMER_TICK Or PERF_DELTA_COUNTER Or PERF_INVERSE_COUNTER Or PERF_DISPLAY_PERCENT)

    ' The divisor for a sample, used with the previous counter to form a
    ' sampled %.  You must check for >0 before dividing by this!  This
    ' counter will directly follow the  numerator counter.  It should not
    ' be displayed to the user.
    Public Const PERF_SAMPLE_BASE As Boolean = (PERF_SIZE_DWORD Or PERF_TYPE_COUNTER Or PERF_COUNTER_BASE Or PERF_DISPLAY_NOSHOW Or &H1S) '  for compatibility with pre-beta versions

    ' A timer which, when divided by an average base, produces a time
    ' in seconds which is the average time of some operation.  This
    ' timer times total operations, and  the base is the number of opera-
    ' tions.  Display Suffix: "sec"
    Public Const PERF_AVERAGE_TIMER As Boolean = (PERF_SIZE_DWORD Or PERF_TYPE_COUNTER Or PERF_COUNTER_FRACTION Or PERF_DISPLAY_SECONDS)

    ' Used as the denominator in the computation of time or count
    ' averages.  Must directly follow the numerator counter.  Not dis-
    ' played to the user.
    Public Const PERF_AVERAGE_BASE As Boolean = (PERF_SIZE_DWORD Or PERF_TYPE_COUNTER Or PERF_COUNTER_BASE Or PERF_DISPLAY_NOSHOW Or &H2S) '  for compatibility with pre-beta versions

    ' A bulk count which, when divided (typically) by the number of
    ' operations, gives (typically) the number of bytes per operation.
    ' No Display Suffix.
    Public Const PERF_AVERAGE_BULK As Boolean = (PERF_SIZE_LARGE Or PERF_TYPE_COUNTER Or PERF_COUNTER_FRACTION Or PERF_DISPLAY_NOSHOW)

    ' 64-bit Timer in 100 nsec units. Display delta divided by
    ' delta time.  Display suffix: "%"
    Public Const PERF_100NSEC_TIMER As Boolean = (PERF_SIZE_LARGE Or PERF_TYPE_COUNTER Or PERF_COUNTER_RATE Or PERF_TIMER_100NS Or PERF_DELTA_COUNTER Or PERF_DISPLAY_PERCENT)

    ' 64-bit Timer inverse (e.g., idle is measured, but display busy  As Integer)
    ' Display 100 - delta divided by delta time.  Display suffix: "%"
    Public Const PERF_100NSEC_TIMER_INV As Boolean = (PERF_SIZE_LARGE Or PERF_TYPE_COUNTER Or PERF_COUNTER_RATE Or PERF_TIMER_100NS Or PERF_DELTA_COUNTER Or PERF_INVERSE_COUNTER Or PERF_DISPLAY_PERCENT)

    ' 64-bit Timer.  Divide delta by delta time.  Display suffix: "%"
    ' Timer for multiple instances, so result can exceed 100%.
    Public Const PERF_COUNTER_MULTI_TIMER As Boolean = (PERF_SIZE_LARGE Or PERF_TYPE_COUNTER Or PERF_COUNTER_RATE Or PERF_DELTA_COUNTER Or PERF_TIMER_TICK Or PERF_MULTI_COUNTER Or PERF_DISPLAY_PERCENT)

    ' 64-bit Timer inverse (e.g., idle is measured, but display busy  As Integer)
    ' Display 100  _MULTI_BASE - delta divided by delta time.
    ' Display suffix: "%" Timer for multiple instances, so result
    ' can exceed 100%.  Followed by a counter of type _MULTI_BASE.
    Public Const PERF_COUNTER_MULTI_TIMER_INV As Boolean = (PERF_SIZE_LARGE Or PERF_TYPE_COUNTER Or PERF_COUNTER_RATE Or PERF_DELTA_COUNTER Or PERF_MULTI_COUNTER Or PERF_TIMER_TICK Or PERF_INVERSE_COUNTER Or PERF_DISPLAY_PERCENT)

    ' Number of instances to which the preceding _MULTI_..._INV counter
    ' applies.  Used as a factor to get the percentage.
    Public Const PERF_COUNTER_MULTI_BASE As Boolean = (PERF_SIZE_LARGE Or PERF_TYPE_COUNTER Or PERF_COUNTER_BASE Or PERF_MULTI_COUNTER Or PERF_DISPLAY_NOSHOW)

    ' 64-bit Timer in 100 nsec units. Display delta divided by delta time.
    ' Display suffix: "%" Timer for multiple instances, so result can exceed 100%.
    Public Const PERF_100NSEC_MULTI_TIMER As Boolean = (PERF_SIZE_LARGE Or PERF_TYPE_COUNTER Or PERF_DELTA_COUNTER Or PERF_COUNTER_RATE Or PERF_TIMER_100NS Or PERF_MULTI_COUNTER Or PERF_DISPLAY_PERCENT)

    ' 64-bit Timer inverse (e.g., idle is measured, but display busy  As Integer)
    ' Display 100  _MULTI_BASE - delta divided by delta time.
    ' Display suffix: "%" Timer for multiple instances, so result
    ' can exceed 100%.  Followed by a counter of type _MULTI_BASE.
    Public Const PERF_100NSEC_MULTI_TIMER_INV As Boolean = (PERF_SIZE_LARGE Or PERF_TYPE_COUNTER Or PERF_DELTA_COUNTER Or PERF_COUNTER_RATE Or PERF_TIMER_100NS Or PERF_MULTI_COUNTER Or PERF_INVERSE_COUNTER Or PERF_DISPLAY_PERCENT)

    ' Indicates the data is a fraction of the following counter  which
    ' should not be time averaged on display (such as free space over
    ' total space.) Display as is.  Display the quotient as "%".
    Public Const PERF_RAW_FRACTION As Boolean = (PERF_SIZE_DWORD Or PERF_TYPE_COUNTER Or PERF_COUNTER_FRACTION Or PERF_DISPLAY_PERCENT)

    ' Indicates the data is a base for the preceding counter which should
    ' not be time averaged on display (such as free space over total space.)
    Public Const PERF_RAW_BASE As Boolean = (PERF_SIZE_DWORD Or PERF_TYPE_COUNTER Or PERF_COUNTER_BASE Or PERF_DISPLAY_NOSHOW Or &H3S) '  for compatibility with pre-beta versions

    ' The data collected in this counter is actually the start time of the
    ' item being measured. For display, this data is subtracted from the
    ' sample time to yield the elapsed time as the difference between the two.
    ' In the definition below, the PerfTime field of the Object contains
    ' the sample time as indicated by the PERF_OBJECT_TIMER bit and the
    ' difference is scaled by the PerfFreq of the Object to convert the time
    ' units into seconds.
    Public Const PERF_ELAPSED_TIME As Boolean = (PERF_SIZE_LARGE Or PERF_TYPE_COUNTER Or PERF_COUNTER_ELAPSED Or PERF_OBJECT_TIMER Or PERF_DISPLAY_SECONDS)

    ' The following counter type can be used with the preceding types to
    ' define a range of values to be displayed in a histogram.
    Public Const PERF_COUNTER_HISTOGRAM_TYPE As Integer = &H80000000 ' Counter begins or ends a histogram

    ' The following are used to determine the level of detail associated
    ' with the counter.  The user will be setting the level of detail
    ' that should be displayed at any given time.
    Public Const PERF_DETAIL_NOVICE As Short = 100 '  The uninformed can understand it
    Public Const PERF_DETAIL_ADVANCED As Short = 200 '  For the advanced user
    Public Const PERF_DETAIL_EXPERT As Short = 300 '  For the expert user
    Public Const PERF_DETAIL_WIZARD As Short = 400 '  For the system designer

    Public Structure PERF_COUNTER_DEFINITION
        Dim ByteLength As Integer
        Dim CounterNameTitleIndex As Integer
        Dim CounterNameTitle As String
        Dim CounterHelpTitleIndex As Integer
        Dim CounterHelpTitle As String
        Dim DefaultScale As Integer
        Dim DetailLevel As Integer
        Dim CounterType As Integer
        Dim CounterSize As Integer
        Dim CounterOffset As Integer
    End Structure

    Public Const PERF_NO_UNIQUE_ID As Short = -1

    Public Structure PERF_INSTANCE_DEFINITION
        Dim ByteLength As Integer
        Dim ParentObjectTitleIndex As Integer
        Dim ParentObjectInstance As Integer
        Dim UniqueID As Integer
        Dim NameOffset As Integer
        Dim NameLength As Integer
    End Structure

    Public Structure PERF_COUNTER_BLOCK
        Dim ByteLength As Integer
    End Structure

    Public Const CDERR_DIALOGFAILURE As Short = &HFFFFS

    Public Const CDERR_GENERALCODES As Short = &H0S
    Public Const CDERR_STRUCTSIZE As Short = &H1S
    Public Const CDERR_INITIALIZATION As Short = &H2S
    Public Const CDERR_NOTEMPLATE As Short = &H3S
    Public Const CDERR_NOHINSTANCE As Short = &H4S
    Public Const CDERR_LOADSTRFAILURE As Short = &H5S
    Public Const CDERR_FINDRESFAILURE As Short = &H6S
    Public Const CDERR_LOADRESFAILURE As Short = &H7S
    Public Const CDERR_LOCKRESFAILURE As Short = &H8S
    Public Const CDERR_MEMALLOCFAILURE As Short = &H9S
    Public Const CDERR_MEMLOCKFAILURE As Short = &HAS
    Public Const CDERR_NOHOOK As Short = &HBS
    Public Const CDERR_REGISTERMSGFAIL As Short = &HCS

    Public Const PDERR_PRINTERCODES As Short = &H1000S
    Public Const PDERR_SETUPFAILURE As Short = &H1001S
    Public Const PDERR_PARSEFAILURE As Short = &H1002S
    Public Const PDERR_RETDEFFAILURE As Short = &H1003S
    Public Const PDERR_LOADDRVFAILURE As Short = &H1004S
    Public Const PDERR_GETDEVMODEFAIL As Short = &H1005S
    Public Const PDERR_INITFAILURE As Short = &H1006S
    Public Const PDERR_NODEVICES As Short = &H1007S
    Public Const PDERR_NODEFAULTPRN As Short = &H1008S
    Public Const PDERR_DNDMMISMATCH As Short = &H1009S
    Public Const PDERR_CREATEICFAILURE As Short = &H100AS
    Public Const PDERR_PRINTERNOTFOUND As Short = &H100BS
    Public Const PDERR_DEFAULTDIFFERENT As Short = &H100CS

    Public Const CFERR_CHOOSEFONTCODES As Short = &H2000S
    Public Const CFERR_NOFONTS As Short = &H2001S
    Public Const CFERR_MAXLESSTHANMIN As Short = &H2002S

    Public Const FNERR_FILENAMECODES As Short = &H3000S
    Public Const FNERR_SUBCLASSFAILURE As Short = &H3001S
    Public Const FNERR_INVALIDFILENAME As Short = &H3002S
    Public Const FNERR_BUFFERTOOSMALL As Short = &H3003S

    Public Const FRERR_FINDREPLACECODES As Short = &H4000S
    Public Const FRERR_BUFFERLENGTHZERO As Short = &H4001S

    Public Const CCERR_CHOOSECOLORCODES As Short = &H5000S


    ' public interface to LZEXP?.LIB

    '  LZEXPAND error return codes
    Public Const LZERROR_BADINHANDLE As Short = (-1) '  invalid input handle
    Public Const LZERROR_BADOUTHANDLE As Short = (-2) '  invalid output handle
    Public Const LZERROR_READ As Short = (-3) '  corrupt compressed file format
    Public Const LZERROR_WRITE As Short = (-4) '  out of space for output file
    Public Const LZERROR_PUBLICLOC As Short = (-5) '  insufficient memory for LZFile struct
    Public Const LZERROR_GLOBLOCK As Short = (-6) '  bad Global handle
    Public Const LZERROR_BADVALUE As Short = (-7) '  input parameter out of range
    Public Const LZERROR_UNKNOWNALG As Short = (-8) '  compression algorithm not recognized

    ' ********************************************************************
    '       IMM.H - Input Method Manager definitions
    '
    '       Copyright (c) 1993-1995  Microsoft Corporation
    ' ********************************************************************

    Public Const VK_PROCESSKEY As Short = &HE5S

    Public Structure COMPOSITIONFORM
        Dim dwStyle As Integer
        Dim ptCurrentPos As POINTAPI
        Dim rcArea As RECT
    End Structure

    Public Structure CANDIDATEFORM
        Dim dwIndex As Integer
        Dim dwStyle As Integer
        Dim ptCurrentPos As POINTAPI
        Dim rcArea As RECT
    End Structure

    Public Structure CANDIDATELIST
        Dim dwSize As Integer
        Dim dwStyle As Integer
        Dim dwCount As Integer
        Dim dwSelection As Integer
        Dim dwPageStart As Integer
        Dim dwPageSize As Integer
        <VBFixedArray(1)> Dim dwOffset() As Integer

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim dwOffset(1)
        End Sub
    End Structure

    Public Const STYLE_DESCRIPTION_SIZE As Short = 32

    Public Structure STYLEBUF
        Dim dwStyle As Integer
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(STYLE_DESCRIPTION_SIZE), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=STYLE_DESCRIPTION_SIZE)> Public szDescription() As Char
    End Structure
    '  the IME related messages
    Public Const WM_CONVERTREQUESTEX As Short = &H108S
    Public Const WM_IME_STARTCOMPOSITION As Short = &H10DS
    Public Const WM_IME_ENDCOMPOSITION As Short = &H10ES
    Public Const WM_IME_COMPOSITION As Short = &H10FS
    Public Const WM_IME_KEYLAST As Short = &H10FS

    Public Const WM_IME_SETCONTEXT As Short = &H281S
    Public Const WM_IME_NOTIFY As Short = &H282S
    Public Const WM_IME_CONTROL As Short = &H283S
    Public Const WM_IME_COMPOSITIONFULL As Short = &H284S
    Public Const WM_IME_SELECT As Short = &H285S
    Public Const WM_IME_CHAR As Short = &H286S

    Public Const WM_IME_KEYDOWN As Short = &H290S
    Public Const WM_IME_KEYUP As Short = &H291S

    '  wParam for WM_IME_CONTROL
    Public Const IMC_GETCANDIDATEPOS As Short = &H7S
    Public Const IMC_SETCANDIDATEPOS As Short = &H8S
    Public Const IMC_GETCOMPOSITIONFONT As Short = &H9S
    Public Const IMC_SETCOMPOSITIONFONT As Short = &HAS
    Public Const IMC_GETCOMPOSITIONWINDOW As Short = &HBS
    Public Const IMC_SETCOMPOSITIONWINDOW As Short = &HCS
    Public Const IMC_GETSTATUSWINDOWPOS As Short = &HFS
    Public Const IMC_SETSTATUSWINDOWPOS As Short = &H10S
    Public Const IMC_CLOSESTATUSWINDOW As Short = &H21S
    Public Const IMC_OPENSTATUSWINDOW As Short = &H22S


    '  wParam for WM_IME_CONTROL to the soft keyboard
    '  dwAction for ImmNotifyIME
    Public Const NI_OPENCANDIDATE As Short = &H10S
    Public Const NI_CLOSECANDIDATE As Short = &H11S
    Public Const NI_SELECTCANDIDATESTR As Short = &H12S
    Public Const NI_CHANGECANDIDATELIST As Short = &H13S
    Public Const NI_FINALIZECONVERSIONRESULT As Short = &H14S
    Public Const NI_COMPOSITIONSTR As Short = &H15S
    Public Const NI_SETCANDIDATE_PAGESTART As Short = &H16S
    Public Const NI_SETCANDIDATE_PAGESIZE As Short = &H17S

    '  lParam for WM_IME_SETCONTEXT
    Public Const ISC_SHOWUICANDIDATEWINDOW As Short = &H1S
    Public Const ISC_SHOWUICOMPOSITIONWINDOW As Integer = &H80000000
    Public Const ISC_SHOWUIGUIDELINE As Integer = &H40000000
    Public Const ISC_SHOWUIALLCANDIDATEWINDOW As Short = &HFS
    Public Const ISC_SHOWUIALL As Integer = &HC000000F

    '  dwIndex for ImmNotifyIME/NI_COMPOSITIONSTR
    Public Const CPS_COMPLETE As Short = &H1S
    Public Const CPS_CONVERT As Short = &H2S
    Public Const CPS_REVERT As Short = &H3S
    Public Const CPS_CANCEL As Short = &H4S

    '  Windows for Simplified Chinese Edition hot key ID from 0x10 - 0x2F
    Public Const IME_CHOTKEY_IME_NONIME_TOGGLE As Short = &H10S
    Public Const IME_CHOTKEY_SHAPE_TOGGLE As Short = &H11S
    Public Const IME_CHOTKEY_SYMBOL_TOGGLE As Short = &H12S

    '  Windows for Japanese Edition hot key ID from 0x30 - 0x4F
    Public Const IME_JHOTKEY_CLOSE_OPEN As Short = &H30S

    '  Windows for Korean Edition hot key ID from 0x50 - 0x6F
    Public Const IME_KHOTKEY_SHAPE_TOGGLE As Short = &H50S
    Public Const IME_KHOTKEY_HANJACONVERT As Short = &H51S
    Public Const IME_KHOTKEY_ENGLISH As Short = &H52S

    '  Windows for Tranditional Chinese Edition hot key ID from 0x70 - 0x8F
    Public Const IME_THOTKEY_IME_NONIME_TOGGLE As Short = &H70S
    Public Const IME_THOTKEY_SHAPE_TOGGLE As Short = &H71S
    Public Const IME_THOTKEY_SYMBOL_TOGGLE As Short = &H72S

    '  direct switch hot key ID from 0x100 - 0x11F
    Public Const IME_HOTKEY_DSWITCH_FIRST As Short = &H100S
    Public Const IME_HOTKEY_DSWITCH_LAST As Short = &H11FS

    '  IME private hot key from 0x200 - 0x21F
    Public Const IME_ITHOTKEY_RESEND_RESULTSTR As Short = &H200S
    Public Const IME_ITHOTKEY_PREVIOUS_COMPOSITION As Short = &H201S
    Public Const IME_ITHOTKEY_UISTYLE_TOGGLE As Short = &H202S

    '  parameter of ImmGetCompositionString
    Public Const GCS_COMPREADSTR As Short = &H1S
    Public Const GCS_COMPREADATTR As Short = &H2S
    Public Const GCS_COMPREADCLAUSE As Short = &H4S
    Public Const GCS_COMPSTR As Short = &H8S
    Public Const GCS_COMPATTR As Short = &H10S
    Public Const GCS_COMPCLAUSE As Short = &H20S
    Public Const GCS_CURSORPOS As Short = &H80S
    Public Const GCS_DELTASTART As Short = &H100S
    Public Const GCS_RESULTREADSTR As Short = &H200S
    Public Const GCS_RESULTREADCLAUSE As Short = &H400S
    Public Const GCS_RESULTSTR As Short = &H800S
    Public Const GCS_RESULTCLAUSE As Short = &H1000S

    '  style bit flags for WM_IME_COMPOSITION
    Public Const CS_INSERTCHAR As Short = &H2000S
    Public Const CS_NOMOVECARET As Short = &H4000S

    '  bits of fdwInit of INPUTCONTEXT
    '  IME property bits
    Public Const IME_PROP_AT_CARET As Integer = &H10000
    Public Const IME_PROP_SPECIAL_UI As Integer = &H20000
    Public Const IME_PROP_CANDLIST_START_FROM_1 As Integer = &H40000
    Public Const IME_PROP_UNICODE As Integer = &H80000

    '  IME UICapability bits
    Public Const UI_CAP_2700 As Short = &H1S
    Public Const UI_CAP_ROT90 As Short = &H2S
    Public Const UI_CAP_ROTANY As Short = &H4S

    '  ImmSetCompositionString Capability bits
    Public Const SCS_CAP_COMPSTR As Short = &H1S
    Public Const SCS_CAP_MAKEREAD As Short = &H2S

    '  IME WM_IME_SELECT inheritance Capability bits
    Public Const SELECT_CAP_CONVERSION As Short = &H1S
    Public Const SELECT_CAP_SENTENCE As Short = &H2S

    '  ID for deIndex of ImmGetGuideLine
    Public Const GGL_LEVEL As Short = &H1S
    Public Const GGL_INDEX As Short = &H2S
    Public Const GGL_STRING As Short = &H3S
    Public Const GGL_PRIVATE As Short = &H4S

    '  ID for dwLevel of GUIDELINE Structure
    Public Const GL_LEVEL_NOGUIDELINE As Short = &H0S
    Public Const GL_LEVEL_FATAL As Short = &H1S
    Public Const GL_LEVEL_ERROR As Short = &H2S
    Public Const GL_LEVEL_WARNING As Short = &H3S
    Public Const GL_LEVEL_INFORMATION As Short = &H4S

    '  ID for dwIndex of GUIDELINE Structure
    Public Const GL_ID_UNKNOWN As Short = &H0S
    Public Const GL_ID_NOMODULE As Short = &H1S
    Public Const GL_ID_NODICTIONARY As Short = &H10S
    Public Const GL_ID_CANNOTSAVE As Short = &H11S
    Public Const GL_ID_NOCONVERT As Short = &H20S
    Public Const GL_ID_TYPINGERROR As Short = &H21S
    Public Const GL_ID_TOOMANYSTROKE As Short = &H22S
    Public Const GL_ID_READINGCONFLICT As Short = &H23S
    Public Const GL_ID_INPUTREADING As Short = &H24S
    Public Const GL_ID_INPUTRADICAL As Short = &H25S
    Public Const GL_ID_INPUTCODE As Short = &H26S
    Public Const GL_ID_INPUTSYMBOL As Short = &H27S
    Public Const GL_ID_CHOOSECANDIDATE As Short = &H28S
    Public Const GL_ID_REVERSECONVERSION As Short = &H29S
    Public Const GL_ID_PRIVATE_FIRST As Short = &H8000S
    Public Const GL_ID_PRIVATE_LAST As Short = &HFFFFS

    '  ID for dwIndex of ImmGetProperty
    Public Const IGP_PROPERTY As Short = &H4S
    Public Const IGP_CONVERSION As Short = &H8S
    Public Const IGP_SENTENCE As Short = &HCS
    Public Const IGP_UI As Short = &H10S
    Public Const IGP_SETCOMPSTR As Short = &H14S
    Public Const IGP_SELECT As Short = &H18S

    '  dwIndex for ImmSetCompositionString API
    Public Const SCS_SETSTR As Boolean = (GCS_COMPREADSTR Or GCS_COMPSTR)
    Public Const SCS_CHANGEATTR As Boolean = (GCS_COMPREADATTR Or GCS_COMPATTR)
    Public Const SCS_CHANGECLAUSE As Boolean = (GCS_COMPREADCLAUSE Or GCS_COMPCLAUSE)

    '  attribute for COMPOSITIONSTRING Structure
    Public Const ATTR_INPUT As Short = &H0S
    Public Const ATTR_TARGET_CONVERTED As Short = &H1S
    Public Const ATTR_CONVERTED As Short = &H2S
    Public Const ATTR_TARGET_NOTCONVERTED As Short = &H3S
    Public Const ATTR_INPUT_ERROR As Short = &H4S

    '  bit field for IMC_SETCOMPOSITIONWINDOW, IMC_SETCANDIDATEWINDOW
    Public Const CFS_DEFAULT As Short = &H0S
    Public Const CFS_RECT As Short = &H1S
    Public Const CFS_POINT As Short = &H2S
    Public Const CFS_SCREEN As Short = &H4S
    Public Const CFS_FORCE_POSITION As Short = &H20S
    Public Const CFS_CANDIDATEPOS As Short = &H40S
    Public Const CFS_EXCLUDE As Short = &H80S

    '  conversion direction for ImmGetConversionList
    Public Const GCL_CONVERSION As Short = &H1S
    Public Const GCL_REVERSECONVERSION As Short = &H2S
    Public Const GCL_REVERSE_LENGTH As Short = &H3S

    '  bit field for conversion mode
    Public Const IME_CMODE_ALPHANUMERIC As Short = &H0S
    Public Const IME_CMODE_NATIVE As Short = &H1S
    Public Const IME_CMODE_CHINESE As Short = IME_CMODE_NATIVE
    Public Const IME_CMODE_HANGEUL As Short = IME_CMODE_NATIVE
    Public Const IME_CMODE_JAPANESE As Short = IME_CMODE_NATIVE
    Public Const IME_CMODE_KATAKANA As Short = &H2S '  only effect under IME_CMODE_NATIVE
    Public Const IME_CMODE_LANGUAGE As Short = &H3S
    Public Const IME_CMODE_FULLSHAPE As Short = &H8S
    Public Const IME_CMODE_ROMAN As Short = &H10S
    Public Const IME_CMODE_CHARCODE As Short = &H20S
    Public Const IME_CMODE_HANJACONVERT As Short = &H40S
    Public Const IME_CMODE_SOFTKBD As Short = &H80S
    Public Const IME_CMODE_NOCONVERSION As Short = &H100S
    Public Const IME_CMODE_EUDC As Short = &H200S
    Public Const IME_CMODE_SYMBOL As Short = &H400S

    Public Const IME_SMODE_NONE As Short = &H0S
    Public Const IME_SMODE_PLAURALCLAUSE As Short = &H1S
    Public Const IME_SMODE_SINGLECONVERT As Short = &H2S
    Public Const IME_SMODE_AUTOMATIC As Short = &H4S
    Public Const IME_SMODE_PHRASEPREDICT As Short = &H8S

    '  style of candidate
    Public Const IME_CAND_UNKNOWN As Short = &H0S
    Public Const IME_CAND_READ As Short = &H1S
    Public Const IME_CAND_CODE As Short = &H2S
    Public Const IME_CAND_MEANING As Short = &H3S
    Public Const IME_CAND_RADICAL As Short = &H4S
    Public Const IME_CAND_STROKE As Short = &H5S

    '  wParam of report message WM_IME_NOTIFY
    Public Const IMN_CLOSESTATUSWINDOW As Short = &H1S
    Public Const IMN_OPENSTATUSWINDOW As Short = &H2S
    Public Const IMN_CHANGECANDIDATE As Short = &H3S
    Public Const IMN_CLOSECANDIDATE As Short = &H4S
    Public Const IMN_OPENCANDIDATE As Short = &H5S
    Public Const IMN_SETCONVERSIONMODE As Short = &H6S
    Public Const IMN_SETSENTENCEMODE As Short = &H7S
    Public Const IMN_SETOPENSTATUS As Short = &H8S
    Public Const IMN_SETCANDIDATEPOS As Short = &H9S
    Public Const IMN_SETCOMPOSITIONFONT As Short = &HAS
    Public Const IMN_SETCOMPOSITIONWINDOW As Short = &HBS
    Public Const IMN_SETSTATUSWINDOWPOS As Short = &HCS
    Public Const IMN_GUIDELINE As Short = &HDS
    Public Const IMN_PRIVATE As Short = &HES

    '  error code of ImmGetCompositionString
    Public Const IMM_ERROR_NODATA As Short = (-1)
    Public Const IMM_ERROR_GENERAL As Short = (-2)

    '  dialog mode of ImmConfigureIME
    Public Const IME_CONFIG_GENERAL As Short = 1
    Public Const IME_CONFIG_REGISTERWORD As Short = 2
    Public Const IME_CONFIG_SELECTDICTIONARY As Short = 3

    '  dialog mode of ImmEscape
    Public Const IME_ESC_QUERY_SUPPORT As Short = &H3S
    Public Const IME_ESC_RESERVED_FIRST As Short = &H4S
    Public Const IME_ESC_RESERVED_LAST As Short = &H7FFS
    Public Const IME_ESC_PRIVATE_FIRST As Short = &H800S
    Public Const IME_ESC_PRIVATE_LAST As Short = &HFFFS
    Public Const IME_ESC_SEQUENCE_TO_INTERNAL As Short = &H1001S
    Public Const IME_ESC_GET_EUDC_DICTIONARY As Short = &H1003S
    Public Const IME_ESC_SET_EUDC_DICTIONARY As Short = &H1004S
    Public Const IME_ESC_MAX_KEY As Short = &H1005S
    Public Const IME_ESC_IME_NAME As Short = &H1006S
    Public Const IME_ESC_SYNC_HOTKEY As Short = &H1007S
    Public Const IME_ESC_HANJA_MODE As Short = &H1008S

    '  style of word registration
    Public Const IME_REGWORD_STYLE_EUDC As Short = &H1S
    Public Const IME_REGWORD_STYLE_USER_FIRST As Integer = &H80000000
    Public Const IME_REGWORD_STYLE_USER_LAST As Short = &HFFFFS

    '  type of soft keyboard
    '  for Windows Tranditional Chinese Edition
    Public Const SOFTKEYBOARD_TYPE_T1 As Short = &H1S
    '  for Windows Simplified Chinese Edition
    Public Const SOFTKEYBOARD_TYPE_C1 As Short = &H2S



    ' ***********************************************************************
    ' *                                                                       *
    ' *   mcx.h -- This module defines the 32-Bit Windows MCX APIs            *
    ' *                                                                       *
    ' *   Copyright (c) 1990-1995, Microsoft Corp. All rights reserved.       *
    ' *                                                                       *
    ' ************************************************************************/


    Public Structure MODEMDEVCAPS
        Dim dwActualSize As Integer
        Dim dwRequiredSize As Integer
        Dim dwDevSpecificOffset As Integer
        Dim dwDevSpecificSize As Integer
        '  product and version identification
        Dim dwModemProviderVersion As Integer
        Dim dwModemManufacturerOffset As Integer
        Dim dwModemManufacturerSize As Integer
        Dim dwModemModelOffset As Integer
        Dim dwModemModelSize As Integer
        Dim dwModemVersionOffset As Integer
        Dim dwModemVersionSize As Integer
        '  local option capabilities
        Dim dwDialOptions As Integer '  bitmap of supported values
        Dim dwCallSetupFailTimer As Integer '  maximum in seconds
        Dim dwInactivityTimeout As Integer '  maximum in seconds
        Dim dwSpeakerVolume As Integer '  bitmap of supported values
        Dim dwSpeakerMode As Integer '  bitmap of supported values
        Dim dwModemOptions As Integer '  bitmap of supported values
        Dim dwMaxDTERate As Integer '  maximum value in bit/s
        Dim dwMaxDCERate As Integer '  maximum value in bit/s
        '  Variable portion for proprietary expansion
        <VBFixedArray(1)> Dim abVariablePortion() As Byte

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim abVariablePortion(1)
        End Sub
    End Structure

    Public Structure MODEMSETTINGS
        Dim dwActualSize As Integer
        Dim dwRequiredSize As Integer
        Dim dwDevSpecificOffset As Integer
        Dim dwDevSpecificSize As Integer
        '  static local options (read/write)
        Dim dwCallSetupFailTimer As Integer '  seconds
        Dim dwInactivityTimeout As Integer '  seconds
        Dim dwSpeakerVolume As Integer '  level
        Dim dwSpeakerMode As Integer '  mode
        Dim dwPreferredModemOptions As Integer '  bitmap
        '  negotiated options (read only) for current or last call
        Dim dwNegotiatedModemOptions As Integer '  bitmap
        Dim dwNegotiatedDCERate As Integer '  bit/s
        '  Variable portion for proprietary expansion
        <VBFixedArray(1)> Dim abVariablePortion() As Byte

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            ReDim abVariablePortion(1)
        End Sub
    End Structure

    '  Dial Options
    Public Const DIALOPTION_BILLING As Short = &H40S '  Supports wait for bong "$"
    Public Const DIALOPTION_QUIET As Short = &H80S '  Supports wait for quiet "@"
    Public Const DIALOPTION_DIALTONE As Short = &H100S '  Supports wait for dial tone "W"

    '  SpeakerVolume for MODEMDEVCAPS
    Public Const MDMVOLFLAG_LOW As Short = &H1S
    Public Const MDMVOLFLAG_MEDIUM As Short = &H2S
    Public Const MDMVOLFLAG_HIGH As Short = &H4S

    '  SpeakerVolume for MODEMSETTINGS
    Public Const MDMVOL_LOW As Short = &H0S
    Public Const MDMVOL_MEDIUM As Short = &H1S
    Public Const MDMVOL_HIGH As Short = &H2S

    '  SpeakerMode for MODEMDEVCAPS
    Public Const MDMSPKRFLAG_OFF As Short = &H1S
    Public Const MDMSPKRFLAG_DIAL As Short = &H2S
    Public Const MDMSPKRFLAG_ON As Short = &H4S
    Public Const MDMSPKRFLAG_CALLSETUP As Short = &H8S

    '  SpeakerMode for MODEMSETTINGS
    Public Const MDMSPKR_OFF As Short = &H0S
    Public Const MDMSPKR_DIAL As Short = &H1S
    Public Const MDMSPKR_ON As Short = &H2S
    Public Const MDMSPKR_CALLSETUP As Short = &H3S

    '  Modem Options
    Public Const MDM_COMPRESSION As Short = &H1S
    Public Const MDM_ERROR_CONTROL As Short = &H2S
    Public Const MDM_FORCED_EC As Short = &H4S
    Public Const MDM_CELLULAR As Short = &H8S
    Public Const MDM_FLOWCONTROL_HARD As Short = &H10S
    Public Const MDM_FLOWCONTROL_SOFT As Short = &H20S
    Public Const MDM_CCITT_OVERRIDE As Short = &H40S
    Public Const MDM_SPEED_ADJUST As Short = &H80S
    Public Const MDM_TONE_DIAL As Short = &H100S
    Public Const MDM_BLIND_DIAL As Short = &H200S
    Public Const MDM_V23_OVERRIDE As Short = &H400S



    ' *****************************************************************************                                                                             *
    ' * shellapi.h -  SHELL.DLL functions, types, and definitions                   *
    ' *                                                                             *
    ' * Copyright (c) 1992-1995, Microsoft Corp.  All rights reserved               *
    ' *                                                                             *
    ' \*****************************************************************************/


    Public Structure DRAGINFO
        Dim uSize As Integer '  init with sizeof(DRAGINFO)
        Dim pt As POINTAPI
        Dim fNC As Integer
        Dim lpFileList As String
        Dim grfKeyState As Integer
    End Structure

    ' // AppBar stuff

    Public Const ABM_NEW As Short = &H0S
    Public Const ABM_REMOVE As Short = &H1S
    Public Const ABM_QUERYPOS As Short = &H2S
    Public Const ABM_SETPOS As Short = &H3S
    Public Const ABM_GETSTATE As Short = &H4S
    Public Const ABM_GETTASKBARPOS As Short = &H5S
    Public Const ABM_ACTIVATE As Short = &H6S '  lParam == TRUE/FALSE means activate/deactivate
    Public Const ABM_GETAUTOHIDEBAR As Short = &H7S
    Public Const ABM_SETAUTOHIDEBAR As Short = &H8S '  this can fail at any time.  MUST check the result
    '  lParam = TRUE/FALSE  Set/Unset
    '  uEdge = what edge
    Public Const ABM_WINDOWPOSCHANGED As Short = &H9S


    '  these are put in the wparam of callback messages
    Public Const ABN_STATECHANGE As Short = &H0S
    Public Const ABN_POSCHANGED As Short = &H1S
    Public Const ABN_FULLSCREENAPP As Short = &H2S
    Public Const ABN_WINDOWARRANGE As Short = &H3S '  lParam == TRUE means hide

    '  flags for get state

    Public Const ABS_AUTOHIDE As Short = &H1S
    Public Const ABS_ALWAYSONTOP As Short = &H2S
    Public Const ABE_LEFT As Short = 0
    Public Const ABE_TOP As Short = 1
    Public Const ABE_RIGHT As Short = 2
    Public Const ABE_BOTTOM As Short = 3

    Public Structure APPBARDATA
        Dim cbSize As Integer
        Dim hWnd As Integer
        Dim uCallbackMessage As Integer
        Dim uEdge As Integer
        Dim rc As RECT
        Dim lParam As Integer '  message specific
    End Structure


    Public Const EIRESID As Short = -1


    ' // Shell File Operations

    Public Const FO_MOVE As Short = &H1S
    Public Const FO_COPY As Short = &H2S
    Public Const FO_DELETE As Short = &H3S
    Public Const FO_RENAME As Short = &H4S
    Public Const FOF_MULTIDESTFILES As Short = &H1S
    Public Const FOF_CONFIRMMOUSE As Short = &H2S
    Public Const FOF_SILENT As Short = &H4S '  don't create progress/report
    Public Const FOF_RENAMEONCOLLISION As Short = &H8S
    Public Const FOF_NOCONFIRMATION As Short = &H10S '  Don't prompt the user.
    Public Const FOF_WANTMAPPINGHANDLE As Short = &H20S '  Fill in SHFILEOPSTRUCT.hNameMappings
    '  Must be freed using SHFreeNameMappings
    Public Const FOF_ALLOWUNDO As Short = &H40S
    Public Const FOF_FILESONLY As Short = &H80S '  on *.*, do only files
    Public Const FOF_SIMPLEPROGRESS As Short = &H100S '  means don't show names of files
    Public Const FOF_NOCONFIRMMKDIR As Short = &H200S '  don't confirm making any needed dirs

    Public Const PO_DELETE As Short = &H13S '  printer is being deleted
    Public Const PO_RENAME As Short = &H14S '  printer is being renamed
    Public Const PO_PORTCHANGE As Short = &H20S '  port this printer connected to is being changed
    '  if this id is set, the strings received by
    '  the copyhook are a doubly-null terminated
    '  list of strings.  The first is the printer
    '  name and the second is the printer port.
    Public Const PO_REN_PORT As Short = &H34S '  PO_RENAME and PO_PORTCHANGE at same time.

    '  no POF_ flags currently defined

    '  implicit parameters are:
    '       if pFrom or pTo are unqualified names the current directories are
    '       taken from the global current drive/directory settings managed
    '       by Get/SetCurrentDrive/Directory
    '
    '       the global confirmation settings

    Public Structure SHFILEOPSTRUCT
        Dim hWnd As Integer
        Dim wFunc As Integer
        Dim pFrom As String
        Dim pTo As String
        Dim fFlags As Short
        Dim fAnyOperationsAborted As Integer
        Dim hNameMappings As Integer
        Dim lpszProgressTitle As String '  only used if FOF_SIMPLEPROGRESS
    End Structure

    Public Structure SHNAMEMAPPING
        Dim pszOldPath As String
        Dim pszNewPath As String
        Dim cchOldPath As Integer
        Dim cchNewPath As Integer
    End Structure

    ' // End Shell File Operations

    ' //  Begin ShellExecuteEx and family

    '  ShellExecute() and ShellExecuteEx() error codes

    '  regular WinExec() codes
    Public Const SE_ERR_FNF As Short = 2 '  file not found
    Public Const SE_ERR_PNF As Short = 3 '  path not found
    Public Const SE_ERR_ACCESSDENIED As Short = 5 '  access denied
    Public Const SE_ERR_OOM As Short = 8 '  out of memory
    Public Const SE_ERR_DLLNOTFOUND As Short = 32


    '  Note CLASSKEY overrides CLASSNAME
    Public Const SEE_MASK_CLASSNAME As Short = &H1S
    Public Const SEE_MASK_CLASSKEY As Short = &H3S
    '  Note INVOKEIDLIST overrides IDLIST
    Public Const SEE_MASK_IDLIST As Short = &H4S
    Public Const SEE_MASK_INVOKEIDLIST As Short = &HCS
    Public Const SEE_MASK_ICON As Short = &H10S
    Public Const SEE_MASK_HOTKEY As Short = &H20S
    Public Const SEE_MASK_NOCLOSEPROCESS As Short = &H40S
    Public Const SEE_MASK_CONNECTNETDRV As Short = &H80S
    Public Const SEE_MASK_FLAG_DDEWAIT As Short = &H100S
    Public Const SEE_MASK_DOENVSUBST As Short = &H200S
    Public Const SEE_MASK_FLAG_NO_UI As Short = &H400S

    Public Structure SHELLEXECUTEINFO
        Dim cbSize As Integer
        Dim fMask As Integer
        Dim hWnd As Integer
        Dim lpVerb As String
        Dim lpFile As String
        Dim lpParameters As String
        Dim lpDirectory As String
        Dim nShow As Integer
        Dim hInstApp As Integer
        '  Optional fields
        Dim lpIDList As Integer
        Dim lpClass As String
        Dim hkeyClass As Integer
        Dim dwHotKey As Integer
        Dim hIcon As Integer
        Dim hProcess As Integer
    End Structure


    ' //  End ShellExecuteEx and family

    ' // Tray notification definitions

    Public Structure NOTIFYICONDATA
        Dim cbSize As Integer
        Dim hWnd As Integer
        Dim uID As Integer
        Dim uFlags As Integer
        Dim uCallbackMessage As Integer
        Dim hIcon As Integer
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(64), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=64)> Public szTip() As Char
    End Structure

    Public Const NIM_ADD As Short = &H0S
    Public Const NIM_MODIFY As Short = &H1S
    Public Const NIM_DELETE As Short = &H2S

    Public Const NIF_MESSAGE As Short = &H1S
    Public Const NIF_ICON As Short = &H2S
    Public Const NIF_TIP As Short = &H4S


    ' // End Tray Notification Icons

    ' // Begin SHGetFileInfo

    '  * The SHGetFileInfo API provides an easy way to get attributes
    '  * for a file given a pathname.
    '  *
    '  *   PARAMETERS
    '  *
    '  *     pszPath              file name to get info about
    '  *     dwFileAttributes     file attribs, only used with SHGFI_USEFILEATTRIBUTES
    '  *     psfi                 place to return file info
    '  *     cbFileInfo           size of structure
    '  *     uFlags               flags
    '  *
    '  *   RETURN
    '  *     TRUE if things worked
    '  */

    Public Structure SHFILEINFO
        Dim hIcon As Integer '  out: icon
        Dim iIcon As Integer '  out: icon index
        Dim dwAttributes As Integer '  out: SFGAO_ flags
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(MAX_PATH), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=MAX_PATH)> Public szDisplayName() As Char '  out: display name (or path)
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(80), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=80)> Public szTypeName() As Char '  out: type name
    End Structure

    Public Const SHGFI_ICON As Short = &H100S '  get icon
    Public Const SHGFI_DISPLAYNAME As Short = &H200S '  get display name
    Public Const SHGFI_TYPENAME As Short = &H400S '  get type name
    Public Const SHGFI_ATTRIBUTES As Short = &H800S '  get attributes
    Public Const SHGFI_ICONLOCATION As Short = &H1000S '  get icon location
    Public Const SHGFI_EXETYPE As Short = &H2000S '  return exe type
    Public Const SHGFI_SYSICONINDEX As Short = &H4000S '  get system icon index
    Public Const SHGFI_LINKOVERLAY As Short = &H8000S '  put a link overlay on icon
    Public Const SHGFI_SELECTED As Integer = &H10000 '  show icon in selected state
    Public Const SHGFI_LARGEICON As Short = &H0S '  get large icon
    Public Const SHGFI_SMALLICON As Short = &H1S '  get small icon
    Public Const SHGFI_OPENICON As Short = &H2S '  get open icon
    Public Const SHGFI_SHELLICONSIZE As Short = &H4S '  get shell size icon
    Public Const SHGFI_PIDL As Short = &H8S '  pszPath is a pidl
    Public Const SHGFI_USEFILEATTRIBUTES As Short = &H10S '  use passed dwFileAttribute


    Public Const SHGNLI_PIDL As Short = &H1S '  pszLinkTo is a pidl
    Public Const SHGNLI_PREFIXNAME As Short = &H2S '  Make name "Shortcut to xxx"

    ' // End SHGetFileInfo


    ' Copyright (C) 1993 - 1995 Microsoft Corporation

    ' Module Name:

    '     winperf.h

    ' Abstract:

    '     Header file for the Performance Monitor data.

    '     This file contains the definitions of the data structures returned
    '     by the Configuration Registry in response to a request for
    '     performance data.  This file is used by both the Configuration
    '     Registry and the Performance Monitor to define their interface.
    '     The complete interface is described here, except for the name
    '     of the node to query in the registry.  It is

    '                    HKEY_PERFORMANCE_DATA.

    '     By querying that node with a subkey of "Global" the caller will
    '     retrieve the structures described here.

    '     There is no need to RegOpenKey() the reserved handle HKEY_PERFORMANCE_DATA,
    '     but the caller should RegCloseKey() the handle so that network transports
    '     and drivers can be removed or installed (which cannot happen while
    '     they are open for monitoring.)  Remote requests must first
    '     RegConnectRegistry().

    ' --*/

    '   Data Public Structure definitions.

    '   In order for data to be returned through the Configuration Registry
    '   in a system-independent fashion, it must be self-describing.

    '   In the following, all offsets are in bytes.

    '
    '   Data is returned through the Configuration Registry in a
    '   a data block which begins with a _PERF_DATA_BLOCK structure.
    '

    '   The _PERF_DATA_BLOCK Public Structure is followed by NumObjectTypes of
    '   data sections, one for each type of object measured.  Each object
    '   type section begins with a _PERF_OBJECT_TYPE structure.


    ' *****************************************************************************                                                                             *
    ' * winver.h -    Version management functions, types, and definitions          *
    ' *                                                                             *
    ' *               Include file for VER.DLL.  This library is                    *
    ' *               designed to allow version stamping of Windows executable files*
    ' *               and of special .VER files for DOS executable files.           *
    ' *                                                                             *
    ' *               Copyright (c) 1993, Microsoft Corp.  All rights reserved      *
    ' *                                                                             *
    ' \*****************************************************************************/

    '  ----- Symbols -----
    Public Const VS_VERSION_INFO As Short = 1
    Public Const VS_USER_DEFINED As Short = 100

    '  ----- VS_VERSION.dwFileFlags -----
    Public Const VS_FFI_SIGNATURE As Integer = &HFEEF04BD
    Public Const VS_FFI_STRUCVERSION As Integer = &H10000
    Public Const VS_FFI_FILEFLAGSMASK As Integer = &H3F

    '  ----- VS_VERSION.dwFileFlags -----
    Public Const VS_FF_DEBUG As Integer = &H1
    Public Const VS_FF_PRERELEASE As Integer = &H2
    Public Const VS_FF_PATCHED As Integer = &H4
    Public Const VS_FF_PRIVATEBUILD As Integer = &H8
    Public Const VS_FF_INFOINFERRED As Integer = &H10
    Public Const VS_FF_SPECIALBUILD As Integer = &H20

    '  ----- VS_VERSION.dwFileOS -----
    Public Const VOS_UNKNOWN As Integer = &H0
    Public Const VOS_DOS As Integer = &H10000
    Public Const VOS_OS216 As Integer = &H20000
    Public Const VOS_OS232 As Integer = &H30000
    Public Const VOS_NT As Integer = &H40000

    Public Const VOS__BASE As Integer = &H0
    Public Const VOS__WINDOWS16 As Integer = &H1
    Public Const VOS__PM16 As Integer = &H2
    Public Const VOS__PM32 As Integer = &H3
    Public Const VOS__WINDOWS32 As Integer = &H4

    Public Const VOS_DOS_WINDOWS16 As Integer = &H10001
    Public Const VOS_DOS_WINDOWS32 As Integer = &H10004
    Public Const VOS_OS216_PM16 As Integer = &H20002
    Public Const VOS_OS232_PM32 As Integer = &H30003
    Public Const VOS_NT_WINDOWS32 As Integer = &H40004

    '  ----- VS_VERSION.dwFileType -----
    Public Const VFT_UNKNOWN As Integer = &H0
    Public Const VFT_APP As Integer = &H1
    Public Const VFT_DLL As Integer = &H2
    Public Const VFT_DRV As Integer = &H3
    Public Const VFT_FONT As Integer = &H4
    Public Const VFT_VXD As Integer = &H5
    Public Const VFT_STATIC_LIB As Integer = &H7

    '  ----- VS_VERSION.dwFileSubtype for VFT_WINDOWS_DRV -----
    Public Const VFT2_UNKNOWN As Integer = &H0
    Public Const VFT2_DRV_PRINTER As Integer = &H1
    Public Const VFT2_DRV_KEYBOARD As Integer = &H2
    Public Const VFT2_DRV_LANGUAGE As Integer = &H3
    Public Const VFT2_DRV_DISPLAY As Integer = &H4
    Public Const VFT2_DRV_MOUSE As Integer = &H5
    Public Const VFT2_DRV_NETWORK As Integer = &H6
    Public Const VFT2_DRV_SYSTEM As Integer = &H7
    Public Const VFT2_DRV_INSTALLABLE As Integer = &H8
    Public Const VFT2_DRV_SOUND As Integer = &H9
    Public Const VFT2_DRV_COMM As Integer = &HA
    Public Const VFT2_DRV_INPUTMETHOD As Integer = &HB

    '  ----- VS_VERSION.dwFileSubtype for VFT_WINDOWS_FONT -----
    Public Const VFT2_FONT_RASTER As Integer = &H1
    Public Const VFT2_FONT_VECTOR As Integer = &H2
    Public Const VFT2_FONT_TRUETYPE As Integer = &H3

    '  ----- VerFindFile() flags -----
    Public Const VFFF_ISSHAREDFILE As Short = &H1S

    Public Const VFF_CURNEDEST As Short = &H1S
    Public Const VFF_FILEINUSE As Short = &H2S
    Public Const VFF_BUFFTOOSMALL As Short = &H4S

    '  ----- VerInstallFile() flags -----
    Public Const VIFF_FORCEINSTALL As Short = &H1S
    Public Const VIFF_DONTDELETEOLD As Short = &H2S

    Public Const VIF_TEMPFILE As Integer = &H1
    Public Const VIF_MISMATCH As Integer = &H2
    Public Const VIF_SRCOLD As Integer = &H4

    Public Const VIF_DIFFLANG As Integer = &H8
    Public Const VIF_DIFFCODEPG As Integer = &H10
    Public Const VIF_DIFFTYPE As Integer = &H20

    Public Const VIF_WRITEPROT As Integer = &H40
    Public Const VIF_FILEINUSE As Integer = &H80
    Public Const VIF_OUTOFSPACE As Integer = &H100
    Public Const VIF_ACCESSVIOLATION As Integer = &H200
    Public Const VIF_SHARINGVIOLATION As Integer = &H400
    Public Const VIF_CANNOTCREATE As Integer = &H800
    Public Const VIF_CANNOTDELETE As Integer = &H1000
    Public Const VIF_CANNOTRENAME As Integer = &H2000
    Public Const VIF_CANNOTDELETECUR As Integer = &H4000
    Public Const VIF_OUTOFMEMORY As Integer = &H8000

    Public Const VIF_CANNOTREADSRC As Integer = &H10000
    Public Const VIF_CANNOTREADDST As Integer = &H20000
    Public Const VIF_BUFFTOOSMALL As Integer = &H40000

    '  ----- Types and structures -----

    Public Structure VS_FIXEDFILEINFO
        Dim dwSignature As Integer
        Dim dwStrucVersion As Integer '  e.g. 0x00000042 = "0.42"
        Dim dwFileVersionMS As Integer '  e.g. 0x00030075 = "3.75"
        Dim dwFileVersionLS As Integer '  e.g. 0x00000031 = "0.31"
        Dim dwProductVersionMS As Integer '  e.g. 0x00030010 = "3.10"
        Dim dwProductVersionLS As Integer '  e.g. 0x00000031 = "0.31"
        Dim dwFileFlagsMask As Integer '  = 0x3F for version "0.42"
        Dim dwFileFlags As Integer '  e.g. VFF_DEBUG Or VFF_PRERELEASE
        Dim dwFileOS As Integer '  e.g. VOS_DOS_WINDOWS16
        Dim dwFileType As Integer '  e.g. VFT_DRIVER
        Dim dwFileSubtype As Integer '  e.g. VFT2_DRV_KEYBOARD
        Dim dwFileDateMS As Integer '  e.g. 0
        Dim dwFileDateLS As Integer '  e.g. 0
    End Structure

    ' ***********************************************************************
    ' *                                                                       *
    ' *   winbase.h -- This module defines the 32-Bit Windows Base APIs       *
    ' *                                                                       *
    ' *   Copyright (c) 1990-1995, Microsoft Corp. All rights reserved.       *
    ' *                                                                       *
    ' ************************************************************************/
    Public Structure ICONMETRICS
        Dim cbSize As Integer
        Dim iHorzSpacing As Integer
        Dim iVertSpacing As Integer
        Dim iTitleWrap As Integer
        'UPGRADE_WARNING: ?? lfFont ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"”
        Dim lfFont As LOGFONT

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            lfFont.Initialize()
        End Sub
    End Structure

    Public Structure HELPINFO
        Dim cbSize As Integer
        Dim iContextType As Integer
        Dim iCtrlId As Integer
        Dim hItemHandle As Integer
        Dim dwContextId As Integer
        Dim MousePos As POINTAPI
    End Structure

    Public Structure ANIMATIONINFO
        Dim cbSize As Integer
        Dim iMinAnimate As Integer
    End Structure

    Public Structure MINIMIZEDMETRICS
        Dim cbSize As Integer
        Dim iWidth As Integer
        Dim iHorzGap As Integer
        Dim iVertGap As Integer
        Dim iArrange As Integer
        'UPGRADE_WARNING: ?? lfFont ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"”
        Dim lfFont As LOGFONT

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            lfFont.Initialize()
        End Sub
    End Structure

    Public Const PROCESS_HEAP_REGION As Short = &H1S
    Public Const PROCESS_HEAP_UNCOMMITTED_RANGE As Short = &H2S
    Public Const PROCESS_HEAP_ENTRY_BUSY As Short = &H4S
    Public Const PROCESS_HEAP_ENTRY_MOVEABLE As Short = &H10S
    Public Const PROCESS_HEAP_ENTRY_DDESHARE As Short = &H20S

    '  GetBinaryType return values.

    Public Const SCS_32BIT_BINARY As Short = 0
    Public Const SCS_DOS_BINARY As Short = 1
    Public Const SCS_WOW_BINARY As Short = 2
    Public Const SCS_PIF_BINARY As Short = 3
    Public Const SCS_POSIX_BINARY As Short = 4
    Public Const SCS_OS216_BINARY As Short = 5

    '  Logon Support APIs

    Public Const LOGON32_LOGON_INTERACTIVE As Short = 2
    Public Const LOGON32_LOGON_BATCH As Short = 4
    Public Const LOGON32_LOGON_SERVICE As Short = 5

    Public Const LOGON32_PROVIDER_DEFAULT As Short = 0
    Public Const LOGON32_PROVIDER_WINNT35 As Short = 1

    '  Performance counter API's

    Public Structure OSVERSIONINFO
        Dim dwOSVersionInfoSize As Integer
        Dim dwMajorVersion As Integer
        Dim dwMinorVersion As Integer
        Dim dwBuildNumber As Integer
        Dim dwPlatformId As Integer
        'UPGRADE_WARNING: ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="3C1E4426-0B80-443E-B943-0627CD55D48B"”
        <VBFixedString(128), System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=128)> Public szCSDVersion() As Char '  Maintenance string for PSS usage
    End Structure

    '  dwPlatformId defines:
    '
    Public Const VER_PLATFORM_WIN32s As Short = 0
    Public Const VER_PLATFORM_WIN32_WINDOWS As Short = 1
    Public Const VER_PLATFORM_WIN32_NT As Short = 2

    '  Power Management APIs

    Public Const AC_LINE_OFFLINE As Short = &H0S
    Public Const AC_LINE_ONLINE As Short = &H1S
    Public Const AC_LINE_BACKUP_POWER As Short = &H2S
    Public Const AC_LINE_UNKNOWN As Short = &HFFS
    Public Const BATTERY_FLAG_HIGH As Short = &H1S
    Public Const BATTERY_FLAG_LOW As Short = &H2S
    Public Const BATTERY_FLAG_CRITICAL As Short = &H4S
    Public Const BATTERY_FLAG_CHARGING As Short = &H8S
    Public Const BATTERY_FLAG_NO_BATTERY As Short = &H80S
    Public Const BATTERY_FLAG_UNKNOWN As Short = &HFFS
    Public Const BATTERY_PERCENTAGE_UNKNOWN As Short = &HFFS
    Public Const BATTERY_LIFE_UNKNOWN As Short = &HFFFFS

    Public Structure SYSTEM_POWER_STATUS
        Dim ACLineStatus As Byte
        Dim BatteryFlag As Byte
        Dim BatteryLifePercent As Byte
        Dim Reserved1 As Byte
        Dim BatteryLifeTime As Integer
        Dim BatteryFullLifeTime As Integer
    End Structure

    ' *   commdlg.h -- This module defines the 32-Bit Common Dialog APIs      *

    Public Structure OPENFILENAME
        Dim lStructSize As Integer
        Dim hwndOwner As Integer
        Dim hInstance As Integer
        Dim lpstrFilter As String
        Dim lpstrCustomFilter As String
        Dim nMaxCustFilter As Integer
        Dim nFilterIndex As Integer
        Dim lpstrFile As String
        Dim nMaxFile As Integer
        Dim lpstrFileTitle As String
        Dim nMaxFileTitle As Integer
        Dim lpstrInitialDir As String
        Dim lpstrTitle As String
        Dim Flags As Integer
        Dim nFileOffset As Short
        Dim nFileExtension As Short
        Dim lpstrDefExt As String
        Dim lCustData As Integer
        Dim lpfnHook As Integer
        Dim lpTemplateName As String
    End Structure


    Public Const OFN_READONLY As Short = &H1S
    Public Const OFN_OVERWRITEPROMPT As Short = &H2S
    Public Const OFN_HIDEREADONLY As Short = &H4S
    Public Const OFN_NOCHANGEDIR As Short = &H8S
    Public Const OFN_SHOWHELP As Short = &H10S
    Public Const OFN_ENABLEHOOK As Short = &H20S
    Public Const OFN_ENABLETEMPLATE As Short = &H40S
    Public Const OFN_ENABLETEMPLATEHANDLE As Short = &H80S
    Public Const OFN_NOVALIDATE As Short = &H100S
    Public Const OFN_ALLOWMULTISELECT As Short = &H200S
    Public Const OFN_EXTENSIONDIFFERENT As Short = &H400S
    Public Const OFN_PATHMUSTEXIST As Short = &H800S
    Public Const OFN_FILEMUSTEXIST As Short = &H1000S
    Public Const OFN_CREATEPROMPT As Short = &H2000S
    Public Const OFN_SHAREAWARE As Short = &H4000S
    Public Const OFN_NOREADONLYRETURN As Short = &H8000S
    Public Const OFN_NOTESTFILECREATE As Integer = &H10000
    Public Const OFN_NONETWORKBUTTON As Integer = &H20000
    Public Const OFN_NOLONGNAMES As Integer = &H40000 '  force no long names for 4.x modules
    Public Const OFN_EXPLORER As Integer = &H80000 '  new look commdlg
    Public Const OFN_NODEREFERENCELINKS As Integer = &H100000
    Public Const OFN_LONGNAMES As Integer = &H200000 '  force long names for 3.x modules

    Public Const OFN_SHAREFALLTHROUGH As Short = 2
    Public Const OFN_SHARENOWARN As Short = 1
    Public Const OFN_SHAREWARN As Short = 0

    Public Structure NMHDR
        Dim hwndFrom As Integer
        Dim idfrom As Integer
        Dim code As Integer
    End Structure

    Public Structure OFNOTIFY
        Dim hdr As NMHDR
        Dim lpOFN As OPENFILENAME
        Dim pszFile As String '  May be NULL
    End Structure

    Public Const CDM_FIRST As Integer = (WM_USER + 100)
    Public Const CDM_LAST As Integer = (WM_USER + 200)
    Public Const CDM_GETSPEC As Integer = (CDM_FIRST + &H0S)
    Public Const CDM_GETFILEPATH As Integer = (CDM_FIRST + &H1S)
    Public Const CDM_GETFOLDERPATH As Integer = (CDM_FIRST + &H2S)
    Public Const CDM_GETFOLDERIDLIST As Integer = (CDM_FIRST + &H3S)
    Public Const CDM_SETCONTROLTEXT As Integer = (CDM_FIRST + &H4S)
    Public Const CDM_HIDECONTROL As Integer = (CDM_FIRST + &H5S)
    Public Const CDM_SETDEFEXT As Integer = (CDM_FIRST + &H6S)

    Public Structure ChooseColor
        Dim lStructSize As Integer
        Dim hwndOwner As Integer
        Dim hInstance As Integer
        Dim rgbResult As Integer
        Dim lpCustColors As Integer
        Dim Flags As Integer
        Dim lCustData As Integer
        Dim lpfnHook As Integer
        Dim lpTemplateName As String
    End Structure


    Public Const CC_RGBINIT As Short = &H1S
    Public Const CC_FULLOPEN As Short = &H2S
    Public Const CC_PREVENTFULLOPEN As Short = &H4S
    Public Const CC_SHOWHELP As Short = &H8S
    Public Const CC_ENABLEHOOK As Short = &H10S
    Public Const CC_ENABLETEMPLATE As Short = &H20S
    Public Const CC_ENABLETEMPLATEHANDLE As Short = &H40S
    Public Const CC_SOLIDCOLOR As Short = &H80S
    Public Const CC_ANYCOLOR As Short = &H100S

    Public Structure FINDREPLACE
        Dim lStructSize As Integer '  size of this struct 0x20
        Dim hwndOwner As Integer '  handle to owner's window
        Dim hInstance As Integer '  instance handle of.EXE that
        '    contains cust. dlg. template
        Dim Flags As Integer '  one or more of the FR_??
        Dim lpstrFindWhat As String '  ptr. to search string
        Dim lpstrReplaceWith As String '  ptr. to replace string
        Dim wFindWhatLen As Short '  size of find buffer
        Dim wReplaceWithLen As Short '  size of replace buffer
        Dim lCustData As Integer '  data passed to hook fn.
        Dim lpfnHook As Integer '  ptr. to hook fn. or NULL
        Dim lpTemplateName As String '  custom template name
    End Structure

    Public Const FR_DOWN As Short = &H1S
    Public Const FR_WHOLEWORD As Short = &H2S
    Public Const FR_MATCHCASE As Short = &H4S
    Public Const FR_FINDNEXT As Short = &H8S
    Public Const FR_REPLACE As Short = &H10S
    Public Const FR_REPLACEALL As Short = &H20S
    Public Const FR_DIALOGTERM As Short = &H40S
    Public Const FR_SHOWHELP As Short = &H80S
    Public Const FR_ENABLEHOOK As Short = &H100S
    Public Const FR_ENABLETEMPLATE As Short = &H200S
    Public Const FR_NOUPDOWN As Short = &H400S
    Public Const FR_NOMATCHCASE As Short = &H800S
    Public Const FR_NOWHOLEWORD As Short = &H1000S
    Public Const FR_ENABLETEMPLATEHANDLE As Short = &H2000S
    Public Const FR_HIDEUPDOWN As Short = &H4000S
    Public Const FR_HIDEMATCHCASE As Short = &H8000S
    Public Const FR_HIDEWHOLEWORD As Integer = &H10000

    Public Structure ChooseFont
        Dim lStructSize As Integer
        Dim hwndOwner As Integer '  caller's window handle
        Dim hdc As Integer '  printer DC/IC or NULL
        'UPGRADE_WARNING: ?? lpLogFont ?????????????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"”
        Dim lpLogFont As LOGFONT '  ptr. to a LOGFONT struct
        Dim iPointSize As Integer '  10 * size in points of selected font
        Dim Flags As Integer '  enum. type flags
        Dim rgbColors As Integer '  returned text color
        Dim lCustData As Integer '  data passed to hook fn.
        Dim lpfnHook As Integer '  ptr. to hook function
        Dim lpTemplateName As String '  custom template name
        Dim hInstance As Integer '  instance handle of.EXE that
        '    contains cust. dlg. template
        Dim lpszStyle As String '  return the style field here
        '  must be LF_FACESIZE or bigger
        Dim nFontType As Short '  same value reported to the EnumFonts
        '    call back with the extra FONTTYPE_
        '    bits added
        Dim MISSING_ALIGNMENT As Short
        Dim nSizeMin As Integer '  minimum pt size allowed &
        Dim nSizeMax As Integer '  max pt size allowed if
        '    CF_LIMITSIZE is used

        'UPGRADE_TODO: ????“Initialize”??????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"”
        Public Sub Initialize()
            lpLogFont.Initialize()
        End Sub
    End Structure

    Public Const CF_SCREENFONTS As Short = &H1S
    Public Const CF_PRINTERFONTS As Short = &H2S
    Public Const CF_BOTH As Boolean = (CF_SCREENFONTS Or CF_PRINTERFONTS)
    Public Const CF_SHOWHELP As Integer = &H4
    Public Const CF_ENABLEHOOK As Integer = &H8
    Public Const CF_ENABLETEMPLATE As Integer = &H10
    Public Const CF_ENABLETEMPLATEHANDLE As Integer = &H20
    Public Const CF_INITTOLOGFONTSTRUCT As Integer = &H40
    Public Const CF_USESTYLE As Integer = &H80
    Public Const CF_EFFECTS As Integer = &H100
    Public Const CF_APPLY As Integer = &H200
    Public Const CF_ANSIONLY As Integer = &H400
    Public Const CF_SCRIPTSONLY As Integer = CF_ANSIONLY
    Public Const CF_NOVECTORFONTS As Integer = &H800
    Public Const CF_NOOEMFONTS As Integer = CF_NOVECTORFONTS
    Public Const CF_NOSIMULATIONS As Integer = &H1000
    Public Const CF_LIMITSIZE As Integer = &H2000
    Public Const CF_FIXEDPITCHONLY As Integer = &H4000
    Public Const CF_WYSIWYG As Short = &H8000S '  must also have CF_SCREENFONTS CF_PRINTERFONTS
    Public Const CF_FORCEFONTEXIST As Integer = &H10000
    Public Const CF_SCALABLEONLY As Integer = &H20000
    Public Const CF_TTONLY As Integer = &H40000
    Public Const CF_NOFACESEL As Integer = &H80000
    Public Const CF_NOSTYLESEL As Integer = &H100000
    Public Const CF_NOSIZESEL As Integer = &H200000
    Public Const CF_SELECTSCRIPT As Integer = &H400000
    Public Const CF_NOSCRIPTSEL As Integer = &H800000
    Public Const CF_NOVERTFONTS As Integer = &H1000000

    Public Const SIMULATED_FONTTYPE As Short = &H8000S
    Public Const PRINTER_FONTTYPE As Short = &H4000S
    Public Const SCREEN_FONTTYPE As Short = &H2000S
    Public Const BOLD_FONTTYPE As Short = &H100S
    Public Const ITALIC_FONTTYPE As Short = &H200S
    Public Const REGULAR_FONTTYPE As Short = &H400S

    Public Const WM_CHOOSEFONT_GETLOGFONT As Integer = (WM_USER + 1)
    Public Const WM_CHOOSEFONT_SETLOGFONT As Integer = (WM_USER + 101)
    Public Const WM_CHOOSEFONT_SETFLAGS As Integer = (WM_USER + 102)

    Public Const LBSELCHSTRING As String = "commdlg_LBSelChangedNotify"
    Public Const SHAREVISTRING As String = "commdlg_ShareViolation"
    Public Const FILEOKSTRING As String = "commdlg_FileNameOK"
    Public Const COLOROKSTRING As String = "commdlg_ColorOK"
    Public Const SETRGBSTRING As String = "commdlg_SetRGBColor"
    Public Const HELPMSGSTRING As String = "commdlg_help"
    Public Const FINDMSGSTRING As String = "commdlg_FindReplace"

    Public Const CD_LBSELNOITEMS As Short = -1
    Public Const CD_LBSELCHANGE As Short = 0
    Public Const CD_LBSELSUB As Short = 1
    Public Const CD_LBSELADD As Short = 2

    Public Structure PrintDlg
        Dim lStructSize As Integer
        Dim hwndOwner As Integer
        Dim hDevMode As Integer
        Dim hDevNames As Integer
        Dim hdc As Integer
        Dim Flags As Integer
        Dim nFromPage As Short
        Dim nToPage As Short
        Dim nMinPage As Short
        Dim nMaxPage As Short
        Dim nCopies As Short
        Dim hInstance As Integer
        Dim lCustData As Integer
        Dim lpfnPrintHook As Integer
        Dim lpfnSetupHook As Integer
        Dim lpPrintTemplateName As String
        Dim lpSetupTemplateName As String
        Dim hPrintTemplate As Integer
        Dim hSetupTemplate As Integer
    End Structure

    Public Const PD_ALLPAGES As Short = &H0S
    Public Const PD_SELECTION As Short = &H1S
    Public Const PD_PAGENUMS As Short = &H2S
    Public Const PD_NOSELECTION As Short = &H4S
    Public Const PD_NOPAGENUMS As Short = &H8S
    Public Const PD_COLLATE As Short = &H10S
    Public Const PD_PRINTTOFILE As Short = &H20S
    Public Const PD_PRINTSETUP As Short = &H40S
    Public Const PD_NOWARNING As Short = &H80S
    Public Const PD_RETURNDC As Short = &H100S
    Public Const PD_RETURNIC As Short = &H200S
    Public Const PD_RETURNDEFAULT As Short = &H400S
    Public Const PD_SHOWHELP As Short = &H800S
    Public Const PD_ENABLEPRINTHOOK As Short = &H1000S
    Public Const PD_ENABLESETUPHOOK As Short = &H2000S
    Public Const PD_ENABLEPRINTTEMPLATE As Short = &H4000S
    Public Const PD_ENABLESETUPTEMPLATE As Short = &H8000S
    Public Const PD_ENABLEPRINTTEMPLATEHANDLE As Integer = &H10000
    Public Const PD_ENABLESETUPTEMPLATEHANDLE As Integer = &H20000
    Public Const PD_USEDEVMODECOPIES As Integer = &H40000
    Public Const PD_USEDEVMODECOPIESANDCOLLATE As Integer = &H40000
    Public Const PD_DISABLEPRINTTOFILE As Integer = &H80000
    Public Const PD_HIDEPRINTTOFILE As Integer = &H100000
    Public Const PD_NONETWORKBUTTON As Integer = &H200000

    Public Structure DEVNAMES
        Dim wDriverOffset As Short
        Dim wDeviceOffset As Short
        Dim wOutputOffset As Short
        Dim wDefault As Short
    End Structure

    Public Const DN_DEFAULTPRN As Short = &H1S

    Public Declare Function CommDlgExtendedError Lib "comdlg32.dll" () As Integer

    Public Const WM_PSD_PAGESETUPDLG As Short = (WM_USER)
    Public Const WM_PSD_FULLPAGERECT As Integer = (WM_USER + 1)
    Public Const WM_PSD_MINMARGINRECT As Integer = (WM_USER + 2)
    Public Const WM_PSD_MARGINRECT As Integer = (WM_USER + 3)
    Public Const WM_PSD_GREEKTEXTRECT As Integer = (WM_USER + 4)
    Public Const WM_PSD_ENVSTAMPRECT As Integer = (WM_USER + 5)
    Public Const WM_PSD_YAFULLPAGERECT As Integer = (WM_USER + 6)

    Public Structure PageSetupDlg
        Dim lStructSize As Integer
        Dim hwndOwner As Integer
        Dim hDevMode As Integer
        Dim hDevNames As Integer
        Dim Flags As Integer
        Dim ptPaperSize As POINTAPI
        Dim rtMinMargin As RECT
        Dim rtMargin As RECT
        Dim hInstance As Integer
        Dim lCustData As Integer
        Dim lpfnPageSetupHook As Integer
        Dim lpfnPagePaintHook As Integer
        Dim lpPageSetupTemplateName As String
        Dim hPageSetupTemplate As Integer
    End Structure

    Public Const PSD_DEFAULTMINMARGINS As Short = &H0S '  default (printer's)
    Public Const PSD_INWININIINTLMEASURE As Short = &H0S '  1st of 4 possible

    Public Const PSD_MINMARGINS As Short = &H1S '  use caller's
    Public Const PSD_MARGINS As Short = &H2S '  use caller's
    Public Const PSD_INTHOUSANDTHSOFINCHES As Short = &H4S '  2nd of 4 possible
    Public Const PSD_INHUNDREDTHSOFMILLIMETERS As Short = &H8S '  3rd of 4 possible
    Public Const PSD_DISABLEMARGINS As Short = &H10S
    Public Const PSD_DISABLEPRINTER As Short = &H20S
    Public Const PSD_NOWARNING As Short = &H80S '  must be same as PD_*
    Public Const PSD_DISABLEORIENTATION As Short = &H100S
    Public Const PSD_RETURNDEFAULT As Short = &H400S '  must be same as PD_*
    Public Const PSD_DISABLEPAPER As Short = &H200S
    Public Const PSD_SHOWHELP As Short = &H800S '  must be same as PD_*
    Public Const PSD_ENABLEPAGESETUPHOOK As Short = &H2000S '  must be same as PD_*
    Public Const PSD_ENABLEPAGESETUPTEMPLATE As Short = &H8000S '  must be same as PD_*
    Public Const PSD_ENABLEPAGESETUPTEMPLATEHANDLE As Integer = &H20000 '  must be same as PD_*
    Public Const PSD_ENABLEPAGEPAINTHOOK As Integer = &H40000
    Public Const PSD_DISABLEPAGEPAINTING As Integer = &H80000

    Public Structure COMMCONFIG
        Dim dwSize As Integer
        Dim wVersion As Short
        Dim wReserved As Short
        Dim dcbx As DCB
        Dim dwProviderSubType As Integer
        Dim dwProviderOffset As Integer
        Dim dwProviderSize As Integer
        Dim wcProviderData As Byte
    End Structure

    Public Structure PIXELFORMATDESCRIPTOR
        Dim nSize As Short
        Dim nVersion As Short
        Dim dwFlags As Integer
        Dim iPixelType As Byte
        Dim cColorBits As Byte
        Dim cRedBits As Byte
        Dim cRedShift As Byte
        Dim cGreenBits As Byte
        Dim cGreenShift As Byte
        Dim cBlueBits As Byte
        Dim cBlueShift As Byte
        Dim cAlphaBits As Byte
        Dim cAlphaShift As Byte
        Dim cAccumBits As Byte
        Dim cAccumRedBits As Byte
        Dim cAccumGreenBits As Byte
        Dim cAccumBlueBits As Byte
        Dim cAccumAlphaBits As Byte
        Dim cDepthBits As Byte
        Dim cStencilBits As Byte
        Dim cAuxBuffers As Byte
        Dim iLayerType As Byte
        Dim bReserved As Byte
        Dim dwLayerMask As Integer
        Dim dwVisibleMask As Integer
        Dim dwDamageMask As Integer
    End Structure

    Public Structure DRAWTEXTPARAMS
        Dim cbSize As Integer
        Dim iTabLength As Integer
        Dim iLeftMargin As Integer
        Dim iRightMargin As Integer
        Dim uiLengthDrawn As Integer
    End Structure

    Public Structure MENUITEMINFO
        Dim cbSize As Integer
        Dim fMask As Integer
        Dim fType As Integer
        Dim fState As Integer
        Dim wID As Integer
        Dim hSubMenu As Integer
        Dim hbmpChecked As Integer
        Dim hbmpUnchecked As Integer
        Dim dwItemData As Integer
        Dim dwTypeData As String
        Dim cch As Integer
    End Structure

    Public Structure SCROLLINFO
        Dim cbSize As Integer
        Dim fMask As Integer
        Dim nMin As Integer
        Dim nMax As Integer
        Dim nPage As Integer
        Dim nPos As Integer
        Dim nTrackPos As Integer
    End Structure

    Public Structure MSGBOXPARAMS
        Dim cbSize As Integer
        Dim hwndOwner As Integer
        Dim hInstance As Integer
        Dim lpszText As String
        Dim lpszCaption As String
        Dim dwStyle As Integer
        Dim lpszIcon As String
        Dim dwContextHelpId As Integer
        Dim lpfnMsgBoxCallback As Integer
        Dim dwLanguageId As Integer
    End Structure

    Public Structure WNDCLASSEX
        Dim cbSize As Integer
        Dim style As Integer
        Dim lpfnWndProc As Integer
        Dim cbClsExtra As Integer
        Dim cbWndExtra As Integer
        Dim hInstance As Integer
        Dim hIcon As Integer
        Dim hCursor As Integer
        Dim hbrBackground As Integer
        Dim lpszMenuName As String
        Dim lpszClassName As String
        Dim hIconSm As Integer
    End Structure

    Public Structure TPMPARAMS
        Dim cbSize As Integer
        Dim rcExclude As RECT
    End Structure

    Public Const INVALID_HANDLE_VALUE As Short = -1

    'DrawEdge Constants
    Public Const BDR_RAISEDOUTER As Short = &H1S
    Public Const BDR_SUNKENOUTER As Short = &H2S
    Public Const BDR_RAISEDINNER As Short = &H4S
    Public Const BDR_SUNKENINNER As Short = &H8S

    Public Const BDR_OUTER As Short = &H3S
    Public Const BDR_INNER As Short = &HCS
    Public Const BDR_RAISED As Short = &H5S
    Public Const BDR_SUNKEN As Short = &HAS

    Public Const EDGE_RAISED As Boolean = (BDR_RAISEDOUTER Or BDR_RAISEDINNER)
    Public Const EDGE_SUNKEN As Boolean = (BDR_SUNKENOUTER Or BDR_SUNKENINNER)
    Public Const EDGE_ETCHED As Boolean = (BDR_SUNKENOUTER Or BDR_RAISEDINNER)
    Public Const EDGE_BUMP As Boolean = (BDR_RAISEDOUTER Or BDR_SUNKENINNER)

    Public Const BF_LEFT As Short = &H1S
    Public Const BF_TOP As Short = &H2S
    Public Const BF_RIGHT As Short = &H4S
    Public Const BF_BOTTOM As Short = &H8S

    Public Const BF_TOPLEFT As Boolean = (BF_TOP Or BF_LEFT)
    Public Const BF_TOPRIGHT As Boolean = (BF_TOP Or BF_RIGHT)
    Public Const BF_BOTTOMLEFT As Boolean = (BF_BOTTOM Or BF_LEFT)
    Public Const BF_BOTTOMRIGHT As Boolean = (BF_BOTTOM Or BF_RIGHT)
    Public Const BF_RECT As Boolean = (BF_LEFT Or BF_TOP Or BF_RIGHT Or BF_BOTTOM)

    Public Const BF_DIAGONAL As Short = &H10S

    ' For diagonal lines, the BF_RECT flags specify the end point of
    ' the vector bounded by the rectangle parameter.
    Public Const BF_DIAGONAL_ENDTOPRIGHT As Boolean = (BF_DIAGONAL Or BF_TOP Or BF_RIGHT)
    Public Const BF_DIAGONAL_ENDTOPLEFT As Boolean = (BF_DIAGONAL Or BF_TOP Or BF_LEFT)
    Public Const BF_DIAGONAL_ENDBOTTOMLEFT As Boolean = (BF_DIAGONAL Or BF_BOTTOM Or BF_LEFT)
    Public Const BF_DIAGONAL_ENDBOTTOMRIGHT As Boolean = (BF_DIAGONAL Or BF_BOTTOM Or BF_RIGHT)

    Public Const BF_MIDDLE As Short = &H800S ' Fill in the middle.
    Public Const BF_SOFT As Short = &H1000S ' Use for softer buttons.
    Public Const BF_ADJUST As Short = &H2000S ' Calculate the space left over.
    Public Const BF_FLAT As Short = &H4000S ' For flat rather than 3-D borders.
    Public Const BF_MONO As Short = &H8000S ' For monochrome borders.
End Module

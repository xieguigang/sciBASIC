#Region "Microsoft.VisualBasic::42a1601e13ce13437e93d4ffa4957a57, ..\VisualBasic_AppFramework\Win32API\WinMM.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Parallel.Tasks

<[PackageNamespace]("winmm.dll",
                    Description:="<pre>' ------------------------------------------------------------------------
'
'    WIN32API.TXT -- Win32 API Declarations for Visual Basic
'
'              Copyright (C) 1994 Microsoft Corporation
'
'
'  This file contains only the Const, Type,
' and Declare statements for  Win32 APIs.
'
'  You have a royalty-free right to use, modify, reproduce and distribute
'  this file (and/or any modified version) in any way you find useful,
'  provided that you agree that Microsoft has no warranty, obligation or
'  liability for its contents.  Refer to the Microsoft Windows Programmer's
'  Reference for further information.
'
' ------------------------------------------------------------------------</pre>",
                    Publisher:="Copyright (C) 2014 Microsoft Corporation",
                    Url:="http://www.microsoft.com/en-us/download/details.aspx?id=12427")>
Public Module WinMM

    '''' <summary>
    '''' DirectShow组件的抽象接口，整个播放器的核心部件
    '''' </summary>
    '''' <remarks></remarks>
    'Public Class DirectShow : Implements IDisposable

    '    Public ReadOnly Property MediaControl As IMediaControl
    '    Public ReadOnly Property MediaPosition As IMediaPosition
    '    Public ReadOnly Property BasicAudio As IBasicAudio
    '    Public ReadOnly Property url As String

    '    ''' <summary>
    '    '''
    '    ''' </summary>
    '    ''' <param name="position">当前的播放位置</param>
    '    Public Event Tick(position As Double)

    '    Dim _tickThread As New UpdateThread(1000, AddressOf __tick)

    '    Private Sub __tick()
    '        RaiseEvent Tick(MediaPosition.CurrentPosition)
    '    End Sub

    '    Public Sub Seek(position As Double)
    '        MediaPosition.CurrentPosition = position
    '    End Sub

    '    Public Function RenderFile(path As String) As DirectShow
    '        Try
    '            Call [Stop]()
    '            Call __renderFile(path)
    '        Catch ex As Exception
    '            Throw New Exception(path.ToFileURL, ex)
    '        End Try

    '        Return Me
    '    End Function

    '    Private Sub __renderFile(path As String)
    '        Me._MediaControl = New FilgraphManager
    '        Me._MediaControl.RenderFile(path)
    '        Me._BasicAudio = MediaControl
    '        Me._MediaPosition = MediaControl
    '        Me._url = path
    '    End Sub

    '    Public ReadOnly Property Duration As Double
    '        Get
    '            Return MediaPosition.Duration
    '        End Get
    '    End Property

    '    Public Sub Dispose() Implements System.IDisposable.Dispose
    '        On Error Resume Next

    '        Call [Stop]()

    '        Me._MediaPosition = Nothing
    '        Me._BasicAudio = Nothing
    '        Me._MediaControl = Nothing
    '    End Sub

    '    Public Overrides Function ToString() As String
    '        Return url.ToFileURL
    '    End Function

    '    Public Function IsNull() As Boolean
    '        Return (BasicAudio Is Nothing OrElse MediaControl Is Nothing OrElse MediaPosition Is Nothing)
    '    End Function

    '    Public Sub [Stop]()
    '        On Error Resume Next
    '        MediaControl.Stop()
    '        Call _tickThread.Stop()
    '    End Sub

    '    Public Sub Pause()
    '        On Error Resume Next
    '        MediaControl.Pause()
    '        Call _tickThread.Stop()
    '    End Sub

    '    Public Sub Play()
    '        On Error Resume Next
    '        MediaControl.Run()
    '        Call _tickThread.Start()
    '    End Sub

    '    Public ReadOnly Property State As Long
    '        Get
    '            Dim TimeOut As Long, s As Long = 0
    '            MediaControl.GetState(TimeOut, s)
    '            Return s
    '        End Get
    '    End Property
    'End Class

    ''' <summary>
    ''' 将数字转化为mm:ss的时间格式
    ''' </summary>
    ''' <param name="intTime"></param>
    ''' <returns></returns>
    Public Function Int2_strTime(intTime As Integer) As String
        Dim mm As Integer = Int(intTime \ 60)
        Dim ss As Integer = intTime Mod 60

        Return mm.ToString + ":" + Format(ss, "00").ToString
    End Function

    <ImportsConstant> Public Const SND_APPLICATION = &H80 ' look for application specific association
    <ImportsConstant> Public Const SND_ALIAS = &H10000 ' name is a WIN.INI [sounds] entry
    <ImportsConstant> Public Const SND_ALIAS_ID = &H110000 ' name is a WIN.INI [sounds] entry identifier
    <ImportsConstant> Public Const SND_ASYNC = &H1 ' play asynchronously
    <ImportsConstant> Public Const SND_FILENAME = &H20000 ' name is a file name
    <ImportsConstant> Public Const SND_LOOP = &H8 ' loop the sound until next sndPlaySound
    <ImportsConstant> Public Const SND_MEMORY = &H4 ' lpszSoundName points to a memory file
    <ImportsConstant> Public Const SND_NODEFAULT = &H2 ' silence not default, if sound not found
    <ImportsConstant> Public Const SND_NOSTOP = &H10 ' don't stop any currently playing sound
    <ImportsConstant> Public Const SND_NOWAIT = &H2000 ' don't wait if the driver is busy
    <ImportsConstant> Public Const SND_PURGE = &H40 ' purge non-static events for task
    <ImportsConstant> Public Const SND_RESOURCE = &H40004 ' name is a resource name or atom
    <ImportsConstant> Public Const SND_SYNC = &H0 ' play synchronously (default)

    <ExportAPI("PlaySoundA")>
    Public Declare Function PlaySound Lib "winmm.dll" Alias "PlaySoundA" (lpszName As String, hModule As Integer, dwFlags As Integer) As Integer

    '<ExportAPI("Invoke.DirectShow")>
    'Public Function InvokeDirectShow(<Parameter("media.url", "The file path of the media file on your file system.")> filename As String) As Double
    '    Dim Device As WinMM.DirectShow = New DirectShow
    '    Call Device.RenderFile(filename)
    '    Call Device.Play()
    '    Return 0
    'End Function

    Public Declare Function mciGetYieldProc Lib "winmm" (mciId As Integer, ByRef pdwYieldData As Integer) As Integer
    Public Declare Function mciSetYieldProc Lib "winmm" (mciId As Integer, fpYieldProc As Integer, dwYieldData As Integer) As Integer
    Public Declare Function midiOutGetNumDevs Lib "winmm" () As Short
    Public Declare Function mmioInstallIOProcA Lib "winmm" Alias "mmioInstallIOProcA" (fccIOProc As String, pIOProc As Long, dwFlags As Long) As Long

    Public Declare Function mmioStringToFOURCC Lib "winmm.dll" Alias "mmioStringToFOURCCA" (sz As String, uFlags As Integer) As Integer

    'UPGRADE_WARNING: ?? MMIOINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function mmioOpen Lib "winmm.dll" Alias "mmioOpenA" (szFileName As String, ByRef lpmmioinfo As MMIOINFO, dwOpenFlags As Integer) As Integer

    'UPGRADE_WARNING: ?? MMIOINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function mmioRename Lib "winmm.dll" Alias "mmioRenameA" (szFileName As String, SzNewFileName As String, ByRef lpmmioinfo As MMIOINFO, dwRenameFlags As Integer) As Integer

    Public Declare Function mmioClose Lib "winmm.dll" (hmmio As Integer, uFlags As Integer) As Integer
    Public Declare Function mmioRead Lib "winmm.dll" (hmmio As Integer, pch As String, cch As Integer) As Integer
    Public Declare Function mmioWrite Lib "winmm.dll" (hmmio As Integer, pch As String, cch As Integer) As Integer
    Public Declare Function mmioSeek Lib "winmm.dll" (hmmio As Integer, lOffset As Integer, iOrigin As Integer) As Integer
    'UPGRADE_WARNING: ?? MMIOINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function mmioGetInfo Lib "winmm.dll" (hmmio As Integer, ByRef lpmmioinfo As MMIOINFO, uFlags As Integer) As Integer
    'UPGRADE_WARNING: ?? MMIOINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function mmioSetInfo Lib "winmm.dll" (hmmio As Integer, ByRef lpmmioinfo As MMIOINFO, uFlags As Integer) As Integer
    Public Declare Function mmioSetBuffer Lib "winmm.dll" (hmmio As Integer, pchBuffer As String, cchBuffer As Integer, uFlags As Integer) As Integer
    Public Declare Function mmioFlush Lib "winmm.dll" (hmmio As Integer, uFlags As Integer) As Integer
    'UPGRADE_WARNING: ?? MMIOINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function mmioAdvance Lib "winmm.dll" (hmmio As Integer, ByRef lpmmioinfo As MMIOINFO, uFlags As Integer) As Integer
    Public Declare Function mmioSendMessage Lib "winmm.dll" (hmmio As Integer, uMsg As Integer, lParam1 As Integer, lParam2 As Integer) As Integer

    'UPGRADE_WARNING: ?? MMCKINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? MMCKINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function mmioDescend Lib "winmm.dll" (hmmio As Integer, ByRef lpck As MMCKINFO, ByRef lpckParent As MMCKINFO, uFlags As Integer) As Integer
    'UPGRADE_WARNING: ?? MMCKINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function mmioAscend Lib "winmm.dll" (hmmio As Integer, ByRef lpck As MMCKINFO, uFlags As Integer) As Integer
    'UPGRADE_WARNING: ?? MMCKINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function mmioCreateChunk Lib "winmm.dll" (hmmio As Integer, ByRef lpck As MMCKINFO, uFlags As Integer) As Integer

    ' MCI functions

    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function mciSendCommand Lib "winmm.dll" Alias "mciSendCommandA" (wDeviceID As Integer, uMessage As Integer, dwParam1 As Integer, dwParam2 As Object) As Integer

    Public Declare Function mciSendString Lib "winmm.dll" Alias "mciSendStringA" (lpstrCommand As String, lpstrReturnString As String, uReturnLength As Integer, hwndCallback As Integer) As Integer

    Public Declare Function mciGetCreatorTask Lib "winmm.dll" (wDeviceID As Integer) As Integer

    Public Declare Function mciGetDeviceID Lib "winmm.dll" Alias "mciGetDeviceIDA" (lpstrName As String) As Integer

    Public Declare Function mciGetDeviceIDFromElementID Lib "winmm.dll" Alias "mciGetDeviceIDFromElementIDA" (dwElementID As Integer, lpstrType As String) As Integer

    Public Declare Function mciGetErrorString Lib "winmm.dll" Alias "mciGetErrorStringA" (dwError As Integer, lpstrBuffer As String, uLength As Integer) As Integer

    Public Declare Function mciExecute Lib "winmm.dll" (lpstrCommand As String) As Integer

End Module

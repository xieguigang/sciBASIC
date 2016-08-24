#Region "Microsoft.VisualBasic::97d3c5c5a5248db3f485f3ec3355f33a, ..\visualbasic_App\UXFramework\Molk+\Molk+\Metro\API\Dwm.vb"

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

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.Runtime.InteropServices
Imports System.Drawing

Namespace Windows.Forms.API

    Public Class Dwm
        <StructLayout(LayoutKind.Explicit)>
        Public Structure RECT
            ' Fields
            <FieldOffset(12)>
            Public bottom As Integer
            <FieldOffset(0)>
            Public left As Integer
            <FieldOffset(8)>
            Public right As Integer
            <FieldOffset(4)>
            Public top As Integer

            ' Methods
            Public Sub New( rect As Rectangle)
                Me.left = rect.Left
                Me.top = rect.Top
                Me.right = rect.Right
                Me.bottom = rect.Bottom
            End Sub

            Public Sub New( left As Integer,  top As Integer,  right As Integer,  bottom As Integer)
                Me.left = left
                Me.top = top
                Me.right = right
                Me.bottom = bottom
            End Sub

            Public Sub [Set]()
                Me.left = InlineAssignHelper(Me.top, InlineAssignHelper(Me.right, InlineAssignHelper(Me.bottom, 0)))
            End Sub

            Public Sub [Set]( rect As Rectangle)
                Me.left = rect.Left
                Me.top = rect.Top
                Me.right = rect.Right
                Me.bottom = rect.Bottom
            End Sub

            Public Sub [Set]( left As Integer,  top As Integer,  right As Integer,  bottom As Integer)
                Me.left = left
                Me.top = top
                Me.right = right
                Me.bottom = bottom
            End Sub

            Public Function ToRectangle() As Rectangle
                Return New Rectangle(Me.left, Me.top, Me.right - Me.left, Me.bottom - Me.top)
            End Function

            ' Properties
            Public ReadOnly Property Height() As Integer
                Get
                    Return (Me.bottom - Me.top)
                End Get
            End Property

            Public ReadOnly Property Size() As Size
                Get
                    Return New Size(Me.Width, Me.Height)
                End Get
            End Property

            Public ReadOnly Property Width() As Integer
                Get
                    Return (Me.right - Me.left)
                End Get
            End Property
            Private Shared Function InlineAssignHelper(Of T)(ByRef target As T,  value As T) As T
                target = value
                Return value
            End Function
        End Structure



        ' Fields
        Public Const DWM_BB_BLURREGION As Integer = 2
        Public Const DWM_BB_ENABLE As Integer = 1
        Public Const DWM_BB_TRANSITIONONMAXIMIZED As Integer = 4
        Public Const DWM_COMPOSED_EVENT_BASE_NAME As String = "DwmComposedEvent_"
        Public Const DWM_COMPOSED_EVENT_NAME_FORMAT As String = "%s%d"
        Public Const DWM_COMPOSED_EVENT_NAME_MAX_LENGTH As Integer = &H40
        Public Const DWM_FRAME_DURATION_DEFAULT As Integer = -1
        Public Const DWM_TNP_OPACITY As Integer = 4
        Public Const DWM_TNP_RECTDESTINATION As Integer = 1
        Public Const DWM_TNP_RECTSOURCE As Integer = 2
        Public Const DWM_TNP_SOURCECLIENTAREAONLY As Integer = &H10
        Public Const DWM_TNP_VISIBLE As Integer = 8
        Public Shared ReadOnly DwmApiAvailable As Boolean = (Environment.OSVersion.Version.Major >= 6)
        Public Const WM_DWMCOMPOSITIONCHANGED As Integer = &H31E

        ' Methods
        <DllImport("dwmapi.dll")>
        Public Shared Function DwmDefWindowProc( hwnd As IntPtr,  msg As Integer,  wParam As IntPtr,  lParam As IntPtr, ByRef result As IntPtr) As Integer
        End Function
        <DllImport("dwmapi.dll")>
        Public Shared Function DwmEnableComposition( fEnable As Integer) As Integer
        End Function
        <DllImport("dwmapi.dll")>
        Public Shared Function DwmEnableMMCSS( fEnableMMCSS As Integer) As Integer
        End Function
        <DllImport("dwmapi.dll")>
        Public Shared Function DwmExtendFrameIntoClientArea( hdc As IntPtr, ByRef marInset As MARGINS) As Integer
        End Function
        <DllImport("dwmapi.dll")>
        Public Shared Function DwmGetColorizationColor(ByRef pcrColorization As Integer, ByRef pfOpaqueBlend As Integer) As Integer
        End Function
        <DllImport("dwmapi.dll")>
        Public Shared Function DwmGetCompositionTimingInfo( hwnd As IntPtr, ByRef pTimingInfo As DWM_TIMING_INFO) As Integer
        End Function
        <DllImport("dwmapi.dll")>
        Public Shared Function DwmGetWindowAttribute( hwnd As IntPtr,  dwAttribute As Integer,  pvAttribute As IntPtr,  cbAttribute As Integer) As Integer
        End Function
        <DllImport("dwmapi.dll")>
        Public Shared Function DwmIsCompositionEnabled(ByRef pfEnabled As Integer) As Integer
        End Function
        <DllImport("dwmapi.dll")>
        Public Shared Function DwmModifyPreviousDxFrameDuration( hwnd As IntPtr,  cRefreshes As Integer,  fRelative As Integer) As Integer
        End Function
        <DllImport("dwmapi.dll")>
        Public Shared Function DwmQueryThumbnailSourceSize( hThumbnail As IntPtr, ByRef pSize As Size) As Integer
        End Function
        <DllImport("dwmapi.dll")>
        Public Shared Function DwmRegisterThumbnail( hwndDestination As IntPtr,  hwndSource As IntPtr, ByRef pMinimizedSize As Size, ByRef phThumbnailId As IntPtr) As Integer
        End Function
        <DllImport("dwmapi.dll")>
        Public Shared Function DwmSetDxFrameDuration( hwnd As IntPtr,  cRefreshes As Integer) As Integer
        End Function
        <DllImport("dwmapi.dll")>
        Public Shared Function DwmSetPresentParameters( hwnd As IntPtr, ByRef pPresentParams As DWM_PRESENT_PARAMETERS) As Integer
        End Function
        <DllImport("dwmapi.dll")>
        Public Shared Function DwmSetWindowAttribute( hwnd As IntPtr,  dwAttribute As Integer,  pvAttribute As IntPtr,  cbAttribute As Integer) As Integer
        End Function
        <DllImport("dwmapi.dll")>
        Public Shared Function DwmUnregisterThumbnail( hThumbnailId As IntPtr) As Integer
        End Function
        <DllImport("dwmapi.dll")>
        Public Shared Function DwmUpdateThumbnailProperties( hThumbnailId As IntPtr, ByRef ptnProperties As DWM_THUMBNAIL_PROPERTIES) As Integer
        End Function

        ' Nested Types
        <StructLayout(LayoutKind.Sequential)>
        Public Structure DWM_BLURBEHIND
            Public dwFlags As Integer
            Public fEnable As Integer
            Public hRgnBlur As IntPtr
            Public fTransitionOnMaximized As Integer
        End Structure

        <StructLayout(LayoutKind.Sequential)>
        Public Structure DWM_PRESENT_PARAMETERS
            Public cbSize As Integer
            Public fQueue As Integer
            Public cRefreshStart As Long
            Public cBuffer As Integer
            Public fUseSourceRate As Integer
            Public rateSource As UNSIGNED_RATIO
            Public cRefreshesPerFrame As Integer
            Public eSampling As DWM_SOURCE_FRAME_SAMPLING
        End Structure

        Public Enum DWM_SOURCE_FRAME_SAMPLING
            DWM_SOURCE_FRAME_SAMPLING_POINT
            DWM_SOURCE_FRAME_SAMPLING_COVERAGE
            DWM_SOURCE_FRAME_SAMPLING_LAST
        End Enum

        <StructLayout(LayoutKind.Sequential)>
        Public Structure DWM_THUMBNAIL_PROPERTIES
            Public dwFlags As Integer
            Public rcDestination As RECT
            Public rcSource As RECT
            Public opacity As Byte
            Public fVisible As Integer
            Public fSourceClientAreaOnly As Integer
        End Structure

        <StructLayout(LayoutKind.Sequential)>
        Public Structure DWM_TIMING_INFO
            Public cbSize As Integer
            Public rateRefresh As UNSIGNED_RATIO
            Public rateCompose As UNSIGNED_RATIO
            Public qpcVBlank As Long
            Public cRefresh As Long
            Public qpcCompose As Long
            Public cFrame As Long
            Public cRefreshFrame As Long
            Public cRefreshConfirmed As Long
            Public cFlipsOutstanding As Integer
            Public cFrameCurrent As Long
            Public cFramesAvailable As Long
            Public cFrameCleared As Long
            Public cFramesReceived As Long
            Public cFramesDisplayed As Long
            Public cFramesDropped As Long
            Public cFramesMissed As Long
        End Structure

        Public Enum DWMNCRENDERINGPOLICY
            DWMNCRP_USEWINDOWSTYLE
            DWMNCRP_DISABLED
            DWMNCRP_ENABLED
        End Enum

        Public Enum DWMWINDOWATTRIBUTE
            DWMWA_ALLOW_NCPAINT = 4
            DWMWA_CAPTION_BUTTON_BOUNDS = 5
            DWMWA_FLIP3D_POLICY = 8
            DWMWA_FORCE_ICONIC_REPRESENTATION = 7
            DWMWA_LAST = 9
            DWMWA_NCRENDERING_ENABLED = 1
            DWMWA_NCRENDERING_POLICY = 2
            DWMWA_NONCLIENT_RTL_LAYOUT = 6
            DWMWA_TRANSITIONS_FORCEDISABLED = 3
        End Enum

        <StructLayout(LayoutKind.Sequential)>
        Public Structure UNSIGNED_RATIO
            Public uiNumerator As Integer
            Public uiDenominator As Integer
        End Structure



        <StructLayout(LayoutKind.Sequential)>
        Public Structure MARGINS
            Public cxLeftWidth As Integer
            Public cxRightWidth As Integer
            Public cyTopHeight As Integer
            Public cyBottomHeight As Integer
            Public Sub New( Left As Integer,  Right As Integer,  Top As Integer,  Bottom As Integer)
                Me.cxLeftWidth = Left
                Me.cxRightWidth = Right
                Me.cyTopHeight = Top
                Me.cyBottomHeight = Bottom
            End Sub
        End Structure


        ''' <summary>
        ''' Do Not Draw The Caption (Text)
        ''' </summary>
        Public Shared WTNCA_NODRAWCAPTION As UInteger = &H1
        ''' <summary>
        ''' Do Not Draw the Icon
        ''' </summary>
        Public Shared WTNCA_NODRAWICON As UInteger = &H2
        ''' <summary>
        ''' Do Not Show the System Menu
        ''' </summary>
        Public Shared WTNCA_NOSYSMENU As UInteger = &H4
        ''' <summary>
        ''' Do Not Mirror the Question mark Symbol
        ''' </summary>
        Public Shared WTNCA_NOMIRRORHELP As UInteger = &H8

        ''' <summary>
        ''' The Options of What Attributes to Add/Remove
        ''' </summary>
        <StructLayout(LayoutKind.Sequential)>
        Public Structure WTA_OPTIONS
            Public Flags As UInteger
            Public Mask As UInteger
        End Structure

        ''' <summary>
        ''' What Type of Attributes? (Only One is Currently Defined)
        ''' </summary>
        Public Enum WindowThemeAttributeType
            WTA_NONCLIENT = 1
        End Enum

        ''' <summary>
        ''' Set The Window's Theme Attributes
        ''' </summary>
        ''' <param name="hWnd">The Handle to the Window</param>
        ''' <param name="wtype">What Type of Attributes</param>
        ''' <param name="attributes">The Attributes to Add/Remove</param>
        ''' <param name="size">The Size of the Attributes Struct</param>
        ''' <returns>If The Call Was Successful or Not</returns>
        <DllImport("UxTheme.dll")>
        Public Shared Function SetWindowThemeAttribute( hWnd As IntPtr,  wtype As WindowThemeAttributeType, ByRef attributes As WTA_OPTIONS,  size As UInteger) As Integer
        End Function
    End Class
End Namespace

#Region "Microsoft.VisualBasic::4bb180cca46a978033e3eabfbdcbe73e, Microsoft.VisualBasic.Core\ApplicationServices\Tools\WinForm\MockForm.vb"

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

    '     Class MockForm
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: (+2 Overloads) Dispose, Resize
    ' 
    '     Class MockTerminal
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: getCmd
    ' 
    '         Sub: init
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Threading

Namespace Windows.Forms

    Public Class MockForm : Implements IDisposable

        Public Const SWP_SHOWWINDOW = &H40
        Public Const SWP_HIDEWINDOW = &H80
        Public Const WS_VSCROLL = &H200000
        Public Const WS_HSCROLL = &H100000
        Public Const WS_CAPTION = &HC00000
        Public Const WS_MINIMIZEBOX = &H20000
        Public Const WS_MAXIMIZEBOX = &H10000
        Public Const WS_OVERLAPPED = &H0&
        Public Const WS_SYSMENU = &H40000
        Public Const HWND_TOPMOST = -1

        Protected hwnd As IntPtr

        Public Declare Auto Function SetParent Lib "user32" Alias "SetParent" (hWndChild As IntPtr, hWndNewParent As IntPtr) As Integer
        Public Declare Auto Function DestroyWindow Lib "user32" (hwnd As Long) As Long
        Public Declare Auto Function FindWindow Lib "user32" Alias "FindWindowA" (lpClassName As String, lpWindowName As String) As IntPtr
        Public Declare Auto Function MoveWindow Lib "user32" Alias "MoveWindow" (hwnd As IntPtr, X As Integer, Y As Integer, cx As Integer, cy As Integer, Flags As Boolean) As Boolean
        Public Declare Auto Function SetWindowLong Lib "user32" Alias "SetWindowLongA" (hwnd As IntPtr, nlndex As Integer, wNewLong As Long) As Long
        Public Declare Auto Function GetWindowLong Lib "user32" Alias "GetWindowLongA" (hwnd As Long, nIndex As Long) As Long
        Public Declare Auto Function SetWindowPos Lib "user32" Alias "SetWindowPos" (hwnd As Long, hWndInsertAfter As Long, x As Long, y As Long, cx As Long, cy As Long, wFlags As Long) As Long

        ''' <summary>
        ''' Construct mock from <see cref="FindWindow"/>
        ''' </summary>
        ''' <param name="found"></param>
        Sub New(found As String)
            hwnd = FindWindow(vbNullString, found)
        End Sub

        Public Sub Resize(size As Size)
            Call MoveWindow(hwnd, 0, 0, size.Width, size.Height, True)
        End Sub

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    DestroyWindow(hwnd) ' 也不能正常调用,只能关闭自己
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class

    Public Class MockTerminal : Inherits MockForm

        Dim parent As Control

        Sub New(parent As Control)
            Call MyBase.New(getCmd)
            Call init(parent)
        End Sub

        ''' <summary>
        ''' Init by win32 API
        ''' </summary>
        Private Sub init(parent As Control)
            ' 直接嵌套到TabPage1内  
            SetParent(hwnd, parent.Handle)
            ' 设置窗体样式
            SetWindowLong(hwnd, (-16), GetWindowLong(hwnd, (-16)) And (Not WS_CAPTION) And (Not WS_VSCROLL) And (Not WS_HSCROLL))
            ' 改变子窗体位置
            SetWindowPos(parent.Handle, -1, 100, 100, 100, 100, SWP_SHOWWINDOW)

            Me.parent = parent
        End Sub

        ''' <summary>
        ''' Start a new cmd.exe instance for the terminal mock
        ''' </summary>
        ''' <returns></returns>
        Private Shared Function getCmd() As String
            Call Process.Start("cmd.exe")
            Call Thread.Sleep(18)   ' 过快下面的FindWindow有可能找不到窗体   
            Return "c:\windows\system32\cmd.exe"
        End Function
    End Class
End Namespace

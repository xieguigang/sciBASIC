#Region "Microsoft.VisualBasic::7e3b7e103b4668634dc99b2fde490ce0, Microsoft.VisualBasic.Core\ApplicationServices\Tools\WinForm\VistaSecurity.vb"

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

    '     Module VistaSecurity
    ' 
    '         Function: IsAdmin, IsVistaOrHigher, SendMessage
    ' 
    '         Sub: AddShieldToButton, RestartElevated
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Runtime.InteropServices
Imports System.Security.Principal

Namespace ApplicationServices.Windows.Forms

    Public Module VistaSecurity

        <DllImport("user32")>
        Public Function SendMessage(hWnd As IntPtr, msg As UInt32, wParam As UInt32, lParam As UInt32) As UInt32
        End Function

        Friend Const BCM_FIRST As Integer = &H1600
        Friend Const BCM_SETSHIELD As Integer = (BCM_FIRST + &HC)

        Public Function IsVistaOrHigher() As Boolean
            Return Environment.OSVersion.Version.Major < 6
        End Function

        ''' <summary>
        ''' Checks if the process is elevated
        ''' </summary>
        ''' <returns>If is elevated</returns>
        Public Function IsAdmin() As Boolean
            Dim id As WindowsIdentity = WindowsIdentity.GetCurrent()
            Dim p As New WindowsPrincipal(id)
            Return p.IsInRole(WindowsBuiltInRole.Administrator)
        End Function

        ''' <summary>
        ''' Add a shield icon to a button
        ''' </summary>
        ''' <param name="b">The button</param>
        Public Sub AddShieldToButton(b As Button)
            b.FlatStyle = FlatStyle.System
            SendMessage(b.Handle, BCM_SETSHIELD, 0, &HFFFFFFFFUI)
        End Sub

        ''' <summary>
        ''' Restart the current process with administrator credentials.(以管理员的身份重启本应用程序)
        ''' </summary>
        Public Sub RestartElevated(Optional args$ = "")
            Dim startInfo As New ProcessStartInfo()
            startInfo.UseShellExecute = True
            startInfo.WorkingDirectory = Environment.CurrentDirectory
            startInfo.FileName = Application.ExecutablePath
            startInfo.Arguments = args
            startInfo.Verb = "runas"
            Try
                Dim p As Process = Process.Start(startInfo)
            Catch ex As Win32Exception
                'If cancelled, do nothing
                Return
            End Try

            Call App.Exit()
        End Sub
    End Module
End Namespace

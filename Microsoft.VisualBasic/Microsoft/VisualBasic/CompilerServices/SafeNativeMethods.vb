Imports System
Imports System.ComponentModel
Imports System.Runtime.InteropServices
Imports System.Security

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never), SecurityCritical, ComVisible(False), SuppressUnmanagedCodeSecurity> _
    Friend NotInheritable Class SafeNativeMethods
        ' Methods
        Private Sub New()
        End Sub

        <DllImport("kernel32", CharSet:=CharSet.Ansi, SetLastError:=True, ExactSpelling:=True)>
        Friend Shared Sub GetLocalTime(systime As SystemTime)
        End Sub

        <DllImport("user32", CharSet:=CharSet.Ansi, SetLastError:=True, ExactSpelling:=True)>
        Friend Shared Function GetWindowThreadProcessId(hwnd As IntPtr, ByRef lpdwProcessId As Integer) As Integer
        End Function

        <DllImport("user32", CharSet:=CharSet.Ansi, SetLastError:=True, ExactSpelling:=True)>
        Friend Shared Function IsWindowEnabled(hwnd As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        <DllImport("user32", CharSet:=CharSet.Ansi, SetLastError:=True, ExactSpelling:=True)>
        Friend Shared Function IsWindowVisible(hwnd As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

    End Class
End Namespace


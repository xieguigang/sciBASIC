Imports System.Runtime.InteropServices
Imports System.Text
Imports Microsoft.VisualBasic.Language.UnixBash

Namespace ComponentModel.Settings.Inf

    ''' <summary>
    ''' Ini file I/O handler
    ''' </summary>
    Public Class IniFile

        Public ReadOnly Property path As String

        <DllImport("kernel32")>
        Private Shared Function WritePrivateProfileString(section As String, key As String, val As String, filePath As String) As Long
        End Function

        <DllImport("kernel32")>
        Private Shared Function GetPrivateProfileString(section As String, key As String, def As String, retVal As StringBuilder, size As Integer, filePath As String) As Integer
        End Function

        ''' <summary>
        ''' Open a ini file handle.
        ''' </summary>
        ''' <param name="INIPath"></param>
        Public Sub New(INIPath As String)
            path = PathMapper.GetMapPath(INIPath)
        End Sub

        Public Overrides Function ToString() As String
            Return path.ToFileURL
        End Function

        Public Sub WriteValue(Section As String, Key As String, Value As String)
            Call WritePrivateProfileString(Section, Key, Value, Me.path)
        End Sub

        Public Function ReadValue(Section As String, Key As String) As String
            Dim temp As New StringBuilder(255)
            Dim i As Integer = GetPrivateProfileString(Section, Key, "", temp, 255, Me.path)
            Return temp.ToString()
        End Function
    End Class
End Namespace
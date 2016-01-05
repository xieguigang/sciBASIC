Imports System.Text.RegularExpressions
Imports System.Text
Imports System.Runtime.InteropServices

Namespace ComponentModel.Settings.Inf

    Public Class Serialization : Inherits Microsoft.VisualBasic.ComponentModel.ITextFile

        <Xml.Serialization.XmlElement> Public Property Sections As Section()

        Public Shared Function Load(path As String) As Serialization
            Throw New NotImplementedException
        End Function

        Public Overrides Function Save(Optional FilePath As String = "", Optional Encoding As Encoding = Nothing) As Boolean
            Throw New NotImplementedException
        End Function

        Protected Overrides Function __getDefaultPath() As String
            Return FilePath
        End Function
    End Class

    Public Class IniFile

        Public ReadOnly Property path As String

        <DllImport("kernel32")>
        Private Shared Function WritePrivateProfileString(section As String, key As String, val As String, filePath As String) As Long
        End Function

        <DllImport("kernel32")>
        Private Shared Function GetPrivateProfileString(section As String, key As String, def As String, retVal As StringBuilder, size As Integer, filePath As String) As Integer
        End Function

        Public Sub New(INIPath As String)
            path = INIPath
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
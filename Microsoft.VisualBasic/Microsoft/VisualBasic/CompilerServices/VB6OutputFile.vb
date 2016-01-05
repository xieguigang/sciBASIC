Imports Microsoft.VisualBasic
Imports System
Imports System.ComponentModel
Imports System.IO
Imports System.Security

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Friend Class VB6OutputFile
        Inherits VB6File
        ' Methods
        Friend Sub New()
        End Sub

        Friend Sub New(FileName As String, share As OpenShare, fAppend As Boolean)
            MyBase.New(FileName, OpenAccess.Write, share, -1)
            MyBase.m_fAppend = fAppend
        End Sub

        Friend Overrides Function CanWrite() As Boolean
            Return True
        End Function

        Friend Overrides Function EOF() As Boolean
            Return True
        End Function

        Public Overrides Function GetMode() As OpenMode
            If MyBase.m_fAppend Then
                Return OpenMode.Append
            End If
            Return OpenMode.Output
        End Function

        Friend Overrides Function LOC() As Long
            Return ((MyBase.m_position + &H7F) / &H80)
        End Function

        Friend Overrides Sub OpenFile()
            Try
                If MyBase.m_fAppend Then
                    If File.Exists(MyBase.m_sFullPath) Then
                        MyBase.m_file = New FileStream(MyBase.m_sFullPath, FileMode.Open, DirectCast(MyBase.m_access, FileAccess), DirectCast(MyBase.m_share, FileShare))
                    Else
                        MyBase.m_file = New FileStream(MyBase.m_sFullPath, FileMode.Create, DirectCast(MyBase.m_access, FileAccess), DirectCast(MyBase.m_share, FileShare))
                    End If
                Else
                    MyBase.m_file = New FileStream(MyBase.m_sFullPath, FileMode.Create, DirectCast(MyBase.m_access, FileAccess), DirectCast(MyBase.m_share, FileShare))
                End If
            Catch exception As FileNotFoundException
                Throw ExceptionUtils.VbMakeException(exception, &H35)
            Catch exception2 As SecurityException
                Throw ExceptionUtils.VbMakeException(exception2, &H35)
            Catch exception3 As DirectoryNotFoundException
                Throw ExceptionUtils.VbMakeException(exception3, &H4C)
            Catch exception4 As IOException
                Throw ExceptionUtils.VbMakeException(exception4, &H4B)
            End Try
            MyBase.m_Encoding = Utils.GetFileIOEncoding
            MyBase.m_sw = New StreamWriter(MyBase.m_file, MyBase.m_Encoding)
            MyBase.m_sw.AutoFlush = True
            If MyBase.m_fAppend Then
                Dim length As Long = MyBase.m_file.Length
                MyBase.m_file.Position = length
                MyBase.m_position = length
            End If
        End Sub

        Friend Overrides Sub WriteLine(s As String)
            Dim numRef As Long
            If (s Is Nothing) Then
                MyBase.m_sw.WriteLine
                numRef = CLng(AddressOf Me.m_position) = (numRef + 2)
            Else
                If ((MyBase.m_bPrint AndAlso (MyBase.m_lWidth <> 0)) AndAlso (MyBase.m_lCurrentColumn >= MyBase.m_lWidth)) Then
                    MyBase.m_sw.WriteLine
                    numRef = CLng(AddressOf Me.m_position) = (numRef + 2)
                End If
                MyBase.m_sw.WriteLine(s)
                numRef = CLng(AddressOf Me.m_position) = (numRef + (MyBase.m_Encoding.GetByteCount(s) + 2))
            End If
            MyBase.m_lCurrentColumn = 0
        End Sub

        Friend Overrides Sub WriteString(s As String)
            If ((Not s Is Nothing) AndAlso (s.Length <> 0)) Then
                Dim numRef As Long
                Dim numRef2 As Integer
                If ((MyBase.m_bPrint AndAlso (MyBase.m_lWidth <> 0)) AndAlso ((MyBase.m_lCurrentColumn >= MyBase.m_lWidth) OrElse ((MyBase.m_lCurrentColumn <> 0) AndAlso ((MyBase.m_lCurrentColumn + s.Length) > MyBase.m_lWidth)))) Then
                    MyBase.m_sw.WriteLine
                    numRef = CLng(AddressOf Me.m_position) = (numRef + 2)
                    MyBase.m_lCurrentColumn = 0
                End If
                MyBase.m_sw.Write(s)
                Dim byteCount As Integer = MyBase.m_Encoding.GetByteCount(s)
                numRef = CLng(AddressOf Me.m_position) = (numRef + byteCount)
                numRef2 = CInt(AddressOf Me.m_lCurrentColumn) = (numRef2 + s.Length)
            End If
        End Sub

    End Class
End Namespace


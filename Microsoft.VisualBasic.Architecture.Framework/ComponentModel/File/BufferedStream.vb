Imports System.IO
Imports System.Text

Namespace ComponentModel

    Public Class BufferedStream

        Public ReadOnly Property FileName As String

        Protected __innerBuffer As String()
        Protected __innerStream As StreamReader

        Const maxBufferSize As Integer = 512 * 1024 * 1024

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="encoding"><see cref="System.Text.Encoding.Default"/>, if null</param>
        Sub New(path As String, Optional encoding As Encoding = Nothing, Optional maxBufferSize As Integer = BufferedStream.maxBufferSize)
            FileName = path
            encoding = encoding.Assertion

            Dim file As FileInfo = FileIO.FileSystem.GetFileInfo(path)
            If file.Length > maxBufferSize Then
                __innerStream = New StreamReader(path, encoding)
            Else
                __innerBuffer = IO.File.ReadAllLines(path, encoding)
            End If
        End Sub

        ''' <summary>
        ''' End of buffer read?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property EndRead As Boolean = False

        Public Function BufferProvider() As String()
            If EndRead Then
                Return Nothing
            Else
                If __innerBuffer Is Nothing Then

                Else
                    _EndRead = True
                    Return __innerBuffer
                End If
            End If
        End Function
    End Class
End Namespace
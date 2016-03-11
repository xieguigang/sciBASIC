Imports System.IO
Imports System.Text
Imports System.Security.Cryptography

Namespace SecurityString

    Public Class TripleDES : Inherits SecurityString.SecurityStringModel

        Private ReadOnly _des As New TripleDESCryptoServiceProvider
        Private ReadOnly _uni As New UnicodeEncoding
        Private ReadOnly _key() As Byte
        Private ReadOnly _iv() As Byte

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="key">24byte</param>
        ''' <param name="iv">8byte</param>
        ''' <remarks></remarks>
        Public Sub New(key() As Byte, iv() As Byte)
            _key = key
            _iv = iv
        End Sub

        Sub New(Optional password As String = "")
            If Not String.IsNullOrEmpty(password) Then
                strPassphrase = password
            End If

            Dim md5 = SecurityString.GetMd5Hash(strPassphrase)
            md5 = md5 & md5 & md5 & md5
            Dim bytes = _uni.GetBytes(md5)
            If bytes.Length < 24 Then
                bytes = {bytes, New Byte() {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24}}.MatrixToVector
            End If
            _key = bytes.Take(24).ToArray

            Dim n As Integer = bytes.Length / 3
            _iv = _key.Take(8).Reverse.ToArray
        End Sub

        Public Overrides Function ToString() As String
            Return strPassphrase
        End Function

        Public Overrides Function Encrypt(input() As Byte) As Byte()
            Return InternalCryptoTransform(input, _des.CreateEncryptor(_key, _iv))
        End Function

        Public Overrides Function EncryptData(text As String) As String
            Dim input() = _uni.GetBytes(text)
            Dim output() = InternalCryptoTransform(input, _des.CreateEncryptor(_key, _iv))
            Return Convert.ToBase64String(output)
        End Function

        Public Overrides Function Decrypt(input() As Byte) As Byte()
            Return InternalCryptoTransform(input, _des.CreateDecryptor(_key, _iv))
        End Function

        Public Overrides Function DecryptString(text As String) As String
            Dim input() = Convert.FromBase64String(text)
            Dim output() = InternalCryptoTransform(input, _des.CreateDecryptor(_key, _iv))
            Return _uni.GetString(output)
        End Function

        Private Function InternalCryptoTransform(input() As Byte, transform As ICryptoTransform) As Byte()
            Dim result As Byte() = Nothing

            Using ms As New MemoryStream
                Using cs As New CryptoStream(ms, transform, CryptoStreamMode.Write)
                    cs.Write(input, 0, input.Length)
                    Try
                        cs.FlushFinalBlock()
                    Catch ex As Exception
                        Call Console.WriteLine(ex.ToString)
                    End Try
                    ms.Position = 0
                    Try
                        result = ms.ToArray()
                    Catch ex As Exception
                        Call Console.WriteLine(ex.ToString)
                    End Try

                End Using
            End Using

            Return result
        End Function
    End Class
End Namespace
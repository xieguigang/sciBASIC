#Region "Microsoft.VisualBasic::e5cfa890b656e93e3a8988f76e1e286f, Microsoft.VisualBasic.Core\Extensions\Security\AES.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class AES
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Decrypt, DecryptString, Encrypt, EncryptData
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Security.Cryptography
Imports System.Text
Imports Microsoft.VisualBasic.Text

Namespace SecurityString

    Public Class AES : Inherits SecurityStringModel

        ReadOnly textEncoding As Encoding

        Sub New(pass$, Optional encoding As Encodings = Encodings.UTF8WithoutBOM)
            Me.strPassphrase = pass
            Me.textEncoding = encoding.CodePage
        End Sub

        Public Overrides Function Decrypt(input() As Byte) As Byte()
            Dim AES As New RijndaelManaged
            Dim hashAES As New MD5CryptoServiceProvider
            Dim hash(31) As Byte
            Dim temp As Byte() = hashAES.ComputeHash(ASCIIEncoding.ASCII.GetBytes(strPassphrase))
            Array.Copy(temp, 0, hash, 0, 16)
            Array.Copy(temp, 0, hash, 15, 16)
            AES.Key = hash
            AES.Mode = CipherMode.ECB
            Dim DESDecrypter As ICryptoTransform = AES.CreateDecryptor
            Dim Buffer As Byte() = input
            Buffer = DESDecrypter.TransformFinalBlock(Buffer, 0, Buffer.Length)
            Return Buffer
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="text">密文数据的base64字符串</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function DecryptString(text As String) As String
            Return textEncoding.GetString(Decrypt(Convert.FromBase64String(text)))
        End Function

        Public Overrides Function Encrypt(input() As Byte) As Byte()
            Dim AES As New RijndaelManaged
            Dim hashAES As New MD5CryptoServiceProvider
            Dim hash(31) As Byte
            Dim temp As Byte() = hashAES.ComputeHash(ASCIIEncoding.ASCII.GetBytes(strPassphrase))
            Array.Copy(temp, 0, hash, 0, 16)
            Array.Copy(temp, 0, hash, 15, 16)
            AES.Key = hash
            AES.Mode = CipherMode.ECB
            Dim DESEncrypter As ICryptoTransform = AES.CreateEncryptor
            Dim Buffer As Byte() = input
            Buffer = DESEncrypter.TransformFinalBlock(Buffer, 0, Buffer.Length)
            Return Buffer
        End Function

        ''' <summary>
        ''' 加密明文，然后使用base64字符串输出结果
        ''' </summary>
        ''' <param name="text"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function EncryptData(text As String) As String
            Return Convert.ToBase64String(Encrypt(textEncoding.GetBytes(text)))
        End Function
    End Class
End Namespace

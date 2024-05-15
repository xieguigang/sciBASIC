#Region "Microsoft.VisualBasic::9499f41f9f9aedb59fde536979175679, mime\application%pdf\PdfFileWriter\PDF\PdfEncryption.vb"

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


    ' Code Statistics:

    '   Total Lines: 428
    '    Code Lines: 198
    ' Comment Lines: 134
    '   Blank Lines: 96
    '     File Size: 14.41 KB


    ' Enum EncryptionType
    ' 
    '     Aes128, Standard128
    ' 
    '  
    ' 
    ' 
    ' 
    ' Enum Permission
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' Class PdfEncryption
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: CreateEncryptionKey, CreateOwnerKey, CreateUserKey, EncryptByteArray, ProcessPassword
    ' 
    '     Sub: CreateMasterKey, Dispose, EncryptRC4
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	PdfEncryption
'	Support for AES-128 encryption
'
'	Uzi Granot
'	Version: 1.0
'	Date: April 1, 2013
'	Copyright (C) 2013-2019 Uzi Granot. All Rights Reserved
'
'	PdfFileWriter C# class library and TestPdfFileWriter test/demo
'  application are free software.
'	They is distributed under the Code Project Open License (CPOL).
'	The document PdfFileWriterReadmeAndLicense.pdf contained within
'	the distribution specify the license agreement and other
'	conditions and notes. You must read this document and agree
'	with the conditions specified in order to use this software.
'
'	For version history please refer to PdfDocument.cs
'
'

Imports System
Imports System.IO
Imports System.Security.Cryptography
Imports stdNum = System.Math

''' <summary>
''' Encryption type enumeration
''' </summary>
Public Enum EncryptionType
    ''' <summary>
    ''' AES 128 bits
    ''' </summary>
    Aes128
    ''' <summary>
    ''' Standard 128 bits
    ''' </summary>
    Standard128
End Enum

''' <summary>
''' PDF reader permission flags enumeration
''' </summary>
''' <remarks>
''' PDF reference manual version 1.7 Table 3.20 
''' </remarks>
Public Enum Permission
    ''' <summary>
    ''' No permission flags
    ''' </summary>
    None = 0

    ''' <summary>
    ''' Low quality print (bit 3)
    ''' </summary>
    LowQalityPrint = 4      ' bit 3

    ''' <summary>
    ''' Modify contents (bit 4)
    ''' </summary>
    ModifyContents = 8      ' bit 4

    ''' <summary>
    ''' Extract contents (bit 5)
    ''' </summary>
    ExtractContents = &H10  ' bit 5

    ''' <summary>
    ''' Annotation (bit 6)
    ''' </summary>
    Annotation = &H20       ' bit 6

    ''' <summary>
    ''' Interactive (bit 9)
    ''' </summary>
    Interactive = &H100 ' bit 9

    ''' <summary>
    ''' Accessibility (bit 10)
    ''' </summary>
    Accessibility = &H200   ' bit 10

    ''' <summary>
    ''' Assemble document (bit 11)
    ''' </summary>
    AssembleDoc = &H400 ' bit 11

    ''' <summary>
    ''' Print (bit 12 plus bit 3)
    ''' </summary>
    Print = &H804           ' bit 12 + bit 3

    ''' <summary>
    ''' All permission bits
    ''' </summary>
    All = &HF3C         ' bits 3, 4, 5, 6, 9, 10, 11, 12
End Enum


''' <summary>
''' PDF encryption class
''' </summary>
''' <remarks>
''' <para>
''' For more information go to <a href="http://www.codeproject.com/Articles/570682/PDF-File-Writer-Csharp-Class-Library-Version#EncryptionSupport">2.6 Encryption Support</a>
''' </para>
''' </remarks>

Public Class PdfEncryption
    Inherits PdfObject
    Implements IDisposable


    ''' <summary>
    ''' 整个项目做编译的时候需要取消检查整数溢出
    ''' 因为源代码中有：
    ''' 
    ''' ```cs
    ''' private const int PermissionBase = unchecked((int) 0xfffff0c0);
    ''' ```
    ''' </summary>
    Public Const PermissionBase As Integer = &HFFFFF0C0

    Friend Permissions As Integer
    Friend EncryptionType As EncryptionType
    Friend MasterKey As Byte()
    Friend MD5 As MD5 = MD5.Create()
    Friend AES As AesCryptoServiceProvider = New AesCryptoServiceProvider()
    Private Shared ReadOnly PasswordPad As Byte() = {&H28, &HBF, &H4E, &H5E, &H4E, &H75, &H8A, &H41, &H64, &H0, &H4E, &H56, &HFF, &HFA, &H1, &H8, &H2E, &H2E, &H0, &HB6, &HD0, &H68, &H3E, &H80, &H2F, &HC, &HA9, &HFE, &H64, &H53, &H69, &H7A}
    Private Shared ReadOnly Salt As Byte() = {&H73, &H41, &H6C, &H54}

    
    ' Encryption Constructor
    

    Friend Sub New(Document As PdfDocument, UserPassword As String, OwnerPassword As String, UserPermissions As Permission, EncryptionType As EncryptionType)
        MyBase.New(Document)
        ' Notes:
        ' The PDF File Writer library supports AES 128 encryption and standard 128 encryption.
        ' The library does not strip leading or trailing white space. They are part of the password.
        ' EncriptMetadata is assumed to be true (this libraray does not use metadata).
        ' Embeded Files Only is assumed to be false (this library does not have embeded files).

        ' remove all unused bits and add all bits that must be one
        Permissions = UserPermissions And CInt(Permission.All) Or PdfEncryption.PermissionBase
        Dictionary.AddInteger("/P", Permissions)

        ' convert user string password to byte array
        Dim UserBinaryPassword = ProcessPassword(UserPassword)

        ' convert owner string password to byte array
        If String.IsNullOrEmpty(OwnerPassword) Then OwnerPassword = BitConverter.ToUInt64(PdfDocument.RandomByteArray(8), 0).ToString()
        Dim OwnerBinaryPassword = ProcessPassword(OwnerPassword)

        ' calculate owner key for crypto dictionary
        Dim OwnerKey = CreateOwnerKey(UserBinaryPassword, OwnerBinaryPassword)

        ' create master key and user key
        CreateMasterKey(UserBinaryPassword, OwnerKey)
        Dim UserKey As Byte() = CreateUserKey()

        ' build dictionary
        Dictionary.Add("/Filter", "/Standard")
        Dictionary.Add("/Length", "128")
        Dictionary.Add("/O", Document.ByteArrayToPdfHexString(OwnerKey))
        Dictionary.Add("/U", Document.ByteArrayToPdfHexString(UserKey))

        ' encryption type
        Me.EncryptionType = EncryptionType

        If EncryptionType = EncryptionType.Aes128 Then
            Dictionary.Add("/R", "4")
            Dictionary.Add("/V", "4")
            Dictionary.Add("/StrF", "/StdCF")
            Dictionary.Add("/StmF", "/StdCF")
            Dictionary.Add("/CF", "<</StdCF<</Length 16/AuthEvent/DocOpen/CFM/AESV2>>>>")
        Else
            Dictionary.Add("/R", "3")
            Dictionary.Add("/V", "2")
        End If

        ' add encryption to trailer dictionary
        Document.TrailerDict.AddIndirectReference("/Encrypt", Me)
        Return
    End Sub

    
    ' Encrypt byte array
    

    Friend Function EncryptByteArray(ObjectNumber As Integer, PlainText As Byte()) As Byte()
        ' create encryption key
        Dim EncryptionKey = CreateEncryptionKey(ObjectNumber)
        Dim CipherText As Byte()

        If EncryptionType = EncryptionType.Aes128 Then
            Dim OutputStream As MemoryStream = Nothing
            Dim CryptoStream As CryptoStream = Nothing

            ' generate new initialization vector IV 
            AES.GenerateIV()

            ' create cipher text buffer including initialization vector
            Dim CipherTextLen = (PlainText.Length And &H7FFFFFF0) + 16
            CipherText = New Byte(CipherTextLen + 16 - 1) {}
            Array.Copy(AES.IV, 0, CipherText, 0, 16)

            ' set encryption key and key length
            AES.Key = EncryptionKey

            ' Create the streams used for encryption.
            OutputStream = New MemoryStream()
            CryptoStream = New CryptoStream(OutputStream, AES.CreateEncryptor(), CryptoStreamMode.Write)

            ' write plain text byte array
            CryptoStream.Write(PlainText, 0, PlainText.Length)

            ' encrypt plain text to cipher text
            CryptoStream.FlushFinalBlock()

            ' get the result
            OutputStream.Seek(0, SeekOrigin.Begin)
            OutputStream.Read(CipherText, 16, CipherTextLen)

            ' release resources
            CryptoStream.Clear()
            OutputStream.Close()
        Else
            CipherText = CType(PlainText.Clone(), Byte())
            EncryptRC4(EncryptionKey, CipherText)
        End If

        ' return result
        Return CipherText
    End Function

    
    ' Process Password
    

    Private Function ProcessPassword(StringPassword As String) As Byte()
        ' no user password
        If String.IsNullOrEmpty(StringPassword) Then Return CType(PasswordPad.Clone(), Byte())

        ' convert password to byte array
        Dim BinaryPassword = New Byte(31) {}
        Dim IndexEnd = stdNum.Min(StringPassword.Length, 32)

        For Index = 0 To IndexEnd - 1
            Dim PWChar = AscW(StringPassword(Index))
            If PWChar > 255 Then Throw New ApplicationException("Owner or user Password has invalid character (allowed 0-255)")
            BinaryPassword(Index) = PWChar
        Next

        ' if user password is shorter than 32 bytes, add padding			
        If IndexEnd < 32 Then Array.Copy(PasswordPad, 0, BinaryPassword, IndexEnd, 32 - IndexEnd)

        ' return password
        Return BinaryPassword
    End Function

    
    ' Create owner key
    

    Private Function CreateOwnerKey(UserBinaryPassword As Byte(), OwnerBinaryPassword As Byte()) As Byte()
        ' create hash array for owner password
        Dim OwnerHash = MD5.ComputeHash(OwnerBinaryPassword)

        ' loop 50 times creating hash of a hash
        For Index = 0 To 50 - 1
            OwnerHash = MD5.ComputeHash(OwnerHash)
        Next

        Dim OwnerKey As Byte() = CType(UserBinaryPassword.Clone(), Byte())
        Dim TempKey = New Byte(15) {}

        For Index = 0 To 20 - 1

            For Tindex = 0 To 16 - 1
                TempKey(Tindex) = CByte(OwnerHash(Tindex) Xor Index)
            Next

            EncryptRC4(TempKey, OwnerKey)
        Next

        ' return encryption key
        Return OwnerKey
    End Function

    
    ' Create master key
    

    Private Sub CreateMasterKey(UserBinaryPassword As Byte(), OwnerKey As Byte())
        ' input byte array for MD5 hash function
        Dim HashInput = New Byte(UserBinaryPassword.Length + OwnerKey.Length + 4 + Document.DocumentID.Length - 1) {}
        Dim Ptr = 0
        Array.Copy(UserBinaryPassword, 0, HashInput, Ptr, UserBinaryPassword.Length)
        Ptr += UserBinaryPassword.Length
        Array.Copy(OwnerKey, 0, HashInput, Ptr, OwnerKey.Length)
        Ptr += OwnerKey.Length
        HashInput(stdNum.Min(Threading.Interlocked.Increment(Ptr), Ptr - 1)) = CByte(Permissions)
        HashInput(stdNum.Min(Threading.Interlocked.Increment(Ptr), Ptr - 1)) = CByte(Permissions >> 8)
        HashInput(stdNum.Min(Threading.Interlocked.Increment(Ptr), Ptr - 1)) = CByte(Permissions >> 16)
        HashInput(stdNum.Min(Threading.Interlocked.Increment(Ptr), Ptr - 1)) = CByte(Permissions >> 24)
        Array.Copy(Document.DocumentID, 0, HashInput, Ptr, Document.DocumentID.Length)
        MasterKey = MD5.ComputeHash(HashInput)

        ' loop 50 times creating hash of a hash
        For Index = 0 To 50 - 1
            MasterKey = MD5.ComputeHash(MasterKey)
        Next

        ' exit
        Return
    End Sub

    
    ' Create user key
    

    Private Function CreateUserKey() As Byte()
        ' input byte array for MD5 hash function
        Dim HashInput = New Byte(PasswordPad.Length + Document.DocumentID.Length - 1) {}
        Array.Copy(PasswordPad, 0, HashInput, 0, PasswordPad.Length)
        Array.Copy(Document.DocumentID, 0, HashInput, PasswordPad.Length, Document.DocumentID.Length)
        Dim UserKey = MD5.ComputeHash(HashInput)
        Dim TempKey = New Byte(15) {}

        For Index = 0 To 20 - 1

            For Tindex = 0 To 16 - 1
                TempKey(Tindex) = CByte(MasterKey(Tindex) Xor Index)
            Next

            EncryptRC4(TempKey, UserKey)
        Next

        Array.Resize(UserKey, 32)
        Return UserKey
    End Function

    
    ' Create encryption key
    

    Private Function CreateEncryptionKey(ObjectNumber As Integer) As Byte()
        Dim HashInput = New Byte(MasterKey.Length + 5 + If(EncryptionType = EncryptionType.Aes128, Salt.Length, 0) - 1) {}
        Dim Ptr = 0
        Array.Copy(MasterKey, 0, HashInput, Ptr, MasterKey.Length)
        Ptr += MasterKey.Length
        HashInput(stdNum.Min(Threading.Interlocked.Increment(Ptr), Ptr - 1)) = CByte(ObjectNumber)
        HashInput(stdNum.Min(Threading.Interlocked.Increment(Ptr), Ptr - 1)) = CByte(ObjectNumber >> 8)
        HashInput(stdNum.Min(Threading.Interlocked.Increment(Ptr), Ptr - 1)) = CByte(ObjectNumber >> 16)
        HashInput(stdNum.Min(Threading.Interlocked.Increment(Ptr), Ptr - 1)) = 0  ' Generation is always zero for this library
        HashInput(stdNum.Min(Threading.Interlocked.Increment(Ptr), Ptr - 1)) = 0  ' Generation is always zero for this library
        If EncryptionType = EncryptionType.Aes128 Then Array.Copy(Salt, 0, HashInput, Ptr, Salt.Length)
        Dim EncryptionKey = MD5.ComputeHash(HashInput)
        If EncryptionKey.Length > 16 Then Array.Resize(EncryptionKey, 16)
        Return EncryptionKey
    End Function

    
    ' RC4 Encryption
    

    Private Sub EncryptRC4(Key As Byte(), Data As Byte())
        Dim State = New Byte(255) {}

        For Index = 0 To 256 - 1
            State(Index) = CByte(Index)
        Next

        Dim Index1 = 0
        Dim Index2 = 0

        For Index = 0 To 256 - 1
            Index2 = Key(Index1) + State(Index) + Index2 And 255
            Dim tmp = State(Index)
            State(Index) = State(Index2)
            State(Index2) = tmp
            Index1 = (Index1 + 1) Mod Key.Length
        Next

        Dim x = 0
        Dim y = 0

        For Index = 0 To Data.Length - 1
            x = x + 1 And 255
            y = State(x) + y And 255
            Dim tmp = State(x)
            State(x) = State(y)
            State(y) = tmp
            Data(Index) = Data(Index) Xor State(State(x) + State(y) And 255)
        Next

        Return
    End Sub

    
    ''' <summary>
    ''' Dispose unmanaged resources
    ''' </summary>
    
    Public Sub Dispose() Implements IDisposable.Dispose
        If AES IsNot Nothing Then
            AES.Clear()
            ' NOTE: AES.Dispose() is valid for .NET 4.0 and later.
            ' In other words visual studio 2010 and later.
            ' If you compile this source with older versions of VS
            ' remove this call at your risk.
            AES.Dispose()
            AES = Nothing
        End If

        If MD5 IsNot Nothing Then
            MD5.Clear()
            MD5 = Nothing
        End If

        Return
    End Sub
End Class

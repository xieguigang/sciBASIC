#Region "Microsoft.VisualBasic::3afa453be23e34f3c2f688bd546bea2f, mime\application%pdf\PdfReader\Document\PdfDecryptStandard.vb"

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

    '     Class PdfDecryptStandard
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CompareArray, ComputeEncryptionKey, ComputeOwnerPasswordValue, ComputeUserPasswordValue, DecodeBytes
    '                   DecodeStream, DecodeStreamAsBytes, DecodeString, DecodeStringAsBytes
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Security
Imports System.Security.Cryptography
Imports System.Text
Imports stdNum = System.Math

Namespace PdfReader
    Public Class PdfDecryptStandard
        Inherits PdfDecrypt

        Private Shared PADDING_32 As Byte() = {&H28, &HBF, &H4E, &H5E, &H4E, &H75, &H8A, &H41, &H64, &H00, &H4E, &H56, &HFF, &HFA, &H01, &H08, &H2E, &H2E, &H00, &HB6, &HD0, &H68, &H3E, &H80, &H2F, &H0C, &HA9, &HFE, &H64, &H53, &H69, &H7A}
        Private _md5 As MD5 = Cryptography.MD5.Create()
        Private _encryptionKey As Byte()

        Public Sub New(ByVal parent As PdfObject, ByVal trailer As PdfDictionary, ByVal encrypt As PdfDictionary)
            MyBase.New(parent)
            ' Extract the first document identifier from the trailer
            Dim ids = trailer.MandatoryValue(Of PdfArray)("ID")
            Dim id0 = CType(ids.Objects(0), PdfString)

            ' Extract and check the mandatory fields
            Dim R = encrypt.MandatoryValue(Of PdfInteger)("R")
            Dim keyLength = encrypt.MandatoryValue(Of PdfInteger)("Length")
            Dim O = encrypt.MandatoryValue(Of PdfString)("O")
            Dim U = encrypt.MandatoryValue(Of PdfString)("U")
            Dim P = encrypt.MandatoryValue(Of PdfInteger)("P")
            If R.Value <> 3 Then Throw New ApplicationException("Cannot decrypt standard handler with revision other than 3.")
            If keyLength.Value < 40 OrElse keyLength.Value > 128 OrElse keyLength.Value Mod 8 <> 0 Then Throw New ApplicationException("Cannot decrypt with key length < 40 or > 128 or not a multiple of 8.")

            ' Setup by owner password
            Dim ownerPasswordValue = ComputeOwnerPasswordValue(keyLength.Value, O.ValueAsBytes)
            _encryptionKey = ComputeEncryptionKey(keyLength.Value, id0.ValueAsBytes, ownerPasswordValue, O.ValueAsBytes, P.Value)
            Dim userPasswordValue = ComputeUserPasswordValue(keyLength.Value, id0.ValueAsBytes, _encryptionKey)

            ' If the owner password does not match...
            Dim UBytes = U.ValueAsBytes

            If Not CompareArray(userPasswordValue, UBytes, 16) Then
                ' ...then try and use the user password instead...
                _encryptionKey = ComputeEncryptionKey(keyLength.Value, id0.ValueAsBytes, PADDING_32, O.ValueAsBytes, P.Value)
                userPasswordValue = ComputeUserPasswordValue(keyLength.Value, id0.ValueAsBytes, _encryptionKey)

                ' If the user password does not match either..
                If Not CompareArray(userPasswordValue, UBytes, 16) Then Throw New ApplicationException("Cannot decrypt using a blank owner or user password.")
            End If
        End Sub

        Public Overrides Function DecodeString(ByVal obj As PdfString) As String
            Return obj.ParseString.BytesToString(DecodeBytes(obj, obj.ParseString.ValueAsBytes))
        End Function

        Public Overrides Function DecodeStringAsBytes(ByVal obj As PdfString) As Byte()
            Return DecodeBytes(obj, obj.ParseString.ValueAsBytes)
        End Function

        Public Overrides Function DecodeStream(ByVal stream As PdfStream) As String
            Return Encoding.ASCII.GetString(stream.ParseStream.DecodeBytes(DecodeBytes(stream, stream.ParseStream.StreamBytes)))
        End Function

        Public Overrides Function DecodeStreamAsBytes(ByVal stream As PdfStream) As Byte()
            Return stream.ParseStream.DecodeBytes(DecodeBytes(stream, stream.ParseStream.StreamBytes))
        End Function

        Private Function DecodeBytes(ByVal obj As PdfObject, ByVal bytes As Byte()) As Byte()
            Dim indirectObject As PdfIndirectObject = obj.TypedParent(Of PdfIndirectObject)()
            If indirectObject Is Nothing Then Throw New ApplicationException($"Cannot decrypt a string that is not inside an indirect object.")

            ' Create bytes that need hashing by combining the encryption key with the indirect object numbers
            Dim key = New Byte(_encryptionKey.Length + 5 - 1) {}
            Array.Copy(_encryptionKey, 0, key, 0, _encryptionKey.Length)
            Dim index = _encryptionKey.Length
            Dim id = indirectObject.Id
            key(stdNum.Min(Threading.Interlocked.Increment(index), index - 1)) = CByte(id >> 0)
            key(stdNum.Min(Threading.Interlocked.Increment(index), index - 1)) = CByte(id >> 8)
            key(stdNum.Min(Threading.Interlocked.Increment(index), index - 1)) = CByte(id >> 16)
            Dim gen = indirectObject.Gen
            key(stdNum.Min(Threading.Interlocked.Increment(index), index - 1)) = CByte(gen >> 0)
            key(stdNum.Min(Threading.Interlocked.Increment(index), index - 1)) = CByte(gen >> 8)

            ' MD5 hash the bytes to get raw decrypt key
            key = _md5.ComputeHash(key)

            ' Limit check the decrypt key length
            Dim keyLength = _encryptionKey.Length + 5
            If keyLength > 16 Then keyLength = 16

            ' Create the RC4 key
            Dim rc4Key = New Byte(keyLength - 1) {}
            Array.Copy(key, 0, rc4Key, 0, keyLength)
            Return Transform(rc4Key, bytes)
        End Function

        Private Function ComputeEncryptionKey(ByVal keyLength As Integer, ByVal documentId As Byte(), ByVal ownerPasswordValue As Byte(), ByVal ownerKey As Byte(), ByVal permissions As Integer) As Byte()
            ' Algorithm 3.2, Computing an encryption key

            ' (1, 2, 3, 4, 5) Appends all required data that needs to be MD5 hashed
            Dim hash = New Byte(ownerPasswordValue.Length + ownerKey.Length + 4 + documentId.Length - 1) {}
            Array.Copy(ownerPasswordValue, 0, hash, 0, ownerPasswordValue.Length)
            Array.Copy(ownerKey, 0, hash, ownerPasswordValue.Length, ownerKey.Length)
            hash(ownerPasswordValue.Length + ownerKey.Length + 0) = CByte(permissions >> 0)
            hash(ownerPasswordValue.Length + ownerKey.Length + 1) = CByte(permissions >> 8)
            hash(ownerPasswordValue.Length + ownerKey.Length + 2) = CByte(permissions >> 16)
            hash(ownerPasswordValue.Length + ownerKey.Length + 3) = CByte(permissions >> 24)
            Array.Copy(documentId, 0, hash, ownerPasswordValue.Length + ownerKey.Length + 4, documentId.Length)

            ' (7) Hash using MD5
            hash = _md5.ComputeHash(hash)

            ' (8) Rehash 50 times
            Dim blockLength As Integer = keyLength / 8
            Dim block = New Byte(blockLength - 1) {}
            Array.Copy(hash, 0, block, 0, block.Length)

            For i = 0 To 50 - 1
                Array.Copy(_md5.ComputeHash(block), 0, block, 0, block.Length)
            Next

            ' (9) Return only the keyLength related number of bytes
            Return block
        End Function

        Private Function ComputeOwnerPasswordValue(ByVal keyLength As Integer, ByVal ownerKey As Byte()) As Byte()
            ' Algorithm 3.3, Computing the encryption dictionary's O (owner password) value

            ' (1) Pad the owner password (use the pad because we do not care about a users password)
            Dim ownerPasword = New Byte(31) {}
            Array.Copy(PADDING_32, ownerPasword, 32)

            ' (2) Initialize the MD5
            ownerPasword = _md5.ComputeHash(ownerPasword)

            ' (3) Rehash 50 times
            Dim blockLength As Integer = keyLength / 8
            Dim block = New Byte(blockLength - 1) {}
            Array.Copy(ownerPasword, 0, block, 0, block.Length)

            For i = 0 To 50 - 1
                Array.Copy(_md5.ComputeHash(block), 0, block, 0, block.Length)
            Next

            ' (4) RC4 key
            Dim rc4Key = New Byte(blockLength - 1) {}

            ' (5) Pad the user password
            Dim userPassword = New Byte(31) {}
            Array.Copy(PADDING_32, userPassword, 32)

            ' (6, 7) Iterate 20 times
            For i = 0 To 20 - 1
                Dim j = 0

                While j < blockLength
                    rc4Key(j) = CByte(block(j) Xor i)
                    Threading.Interlocked.Increment(j)
                End While

                ownerKey = Transform(rc4Key, ownerKey)
            Next

            Return ownerKey
        End Function

        Private Function ComputeUserPasswordValue(ByVal keyLength As Integer, ByVal documentId As Byte(), ByVal encryptionKey As Byte()) As Byte()
            ' Algorithm 3.5, Computing the encryption dictionary's U (user password) value

            ' (2, 3) Join the fixed padding and the document identifier
            Dim hash = New Byte(PADDING_32.Length + documentId.Length - 1) {}
            Array.Copy(PADDING_32, 0, hash, 0, PADDING_32.Length)
            Array.Copy(documentId, 0, hash, PADDING_32.Length, documentId.Length)

            ' (3) Hash using MD5
            hash = _md5.ComputeHash(hash)

            ' (4, 5) Iterate 20 times
            Dim rc4Key = New Byte(encryptionKey.Length - 1) {}

            For i = 0 To 20 - 1
                Dim j = 0

                While j < encryptionKey.Length
                    rc4Key(j) = CByte(encryptionKey(j) Xor i)
                    Threading.Interlocked.Increment(j)
                End While

                hash = Transform(rc4Key, hash)
            Next

            ' (6) First 16 bytes is the hash result and the remainder is zero's
            Dim userKey = New Byte(31) {}
            Array.Copy(hash, 0, userKey, 0, 16)
            Return userKey
        End Function

        Private Function CompareArray(ByVal l As Byte(), ByVal r As Byte(), ByVal length As Integer) As Boolean
            For i = 0 To length - 1
                If l(i) <> r(i) Then Return False
            Next

            Return True
        End Function
    End Class
End Namespace


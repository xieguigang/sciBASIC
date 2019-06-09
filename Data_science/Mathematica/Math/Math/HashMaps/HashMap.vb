#Region "Microsoft.VisualBasic::089529aa3a6de7169fa31baf0ec5d8f6, Data_science\Mathematica\Math\Math\HashMaps\HashMap.vb"

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

    '     Module HashMap
    ' 
    '         Function: (+2 Overloads) HashAP, (+2 Overloads) HashBKDR, (+2 Overloads) HashCMyMap, (+2 Overloads) HashDEK, (+2 Overloads) HashDJB
    '                   (+2 Overloads) HashELF, (+2 Overloads) HashJS, (+2 Overloads) HashPJW, (+2 Overloads) HashRS, (+2 Overloads) HashSDBM
    '                   (+2 Overloads) HashTimeMap
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Numerics
Imports System.Runtime.CompilerServices
Imports UncheckedUInt64 = System.Numerics.BigInteger

Namespace HashMaps

    ''' <summary>
    ''' VB.NET常用的哈希算法集.其中包括了著名的暴雪的哈希,T33哈希.......
    ''' 不同的哈希算法在分布式,布降过滤器,位图MAP等等应用得比较多...
    ''' </summary>
    ''' <remarks>
    ''' http://bbs.csdn.net/topics/391950537
    ''' </remarks>
    Public Module HashMap

#Region "哈希算法"

        ''' <summary>
        ''' 和 HashCMyMap 基本一样.
        ''' </summary>
        ''' <param name="Key"></param>
        ''' <returns></returns>
        Public Function HashDJB(Key As String) As ULong
            Dim hash As UncheckedUInt64 = 5381
            Dim L As Int32 = Key.Length - 1
            Dim KeyCharArr() As Char = Key.ToArray

            For i As Int32 = 0 To L
                hash = ((hash << 5) + hash) + Asc(KeyCharArr(i)) + 3
            Next

            Return hash
        End Function

        ''' <summary>
        ''' 和 HashCMyMap 基本一样.
        ''' </summary>
        ''' <param name="KeyByte"></param>
        ''' <returns></returns>
        Public Function HashDJB(KeyByte() As Byte) As ULong
            Dim hash As UncheckedUInt64 = 5381
            Dim L As Int32 = KeyByte.Length - 1

            For i As Int32 = 0 To L
                hash = ((hash << 5) + hash) + KeyByte(i) + 3
            Next

            Return hash
        End Function

        ''' <summary>
        ''' BKDR 哈希
        ''' </summary>
        ''' <param name="Key"></param>
        ''' <param name="seed">种子.最好是使用质数.</param>
        ''' <returns></returns>
        Public Function HashBKDR(Key As String, Optional seed As Long = 131) As ULong
            Dim hash As UncheckedUInt64 = 0
            Dim L As Int32 = Key.Length - 1
            Dim KeyCharArr() As Char = Key.ToArray
            For i As Int32 = 0 To L - 1
                hash = (hash * seed) + Asc(KeyCharArr(i)) + 3
            Next

            Return (hash And &H7FFFFFFF)
        End Function

        ''' <summary>
        ''' BKDR 哈希
        ''' </summary>
        ''' <param name="KeyByte"></param>
        ''' <param name="seed">种子数</param>
        ''' <returns></returns>
        Public Function HashBKDR(KeyByte() As Byte, Optional seed As Long = 131) As ULong
            Dim hash As UncheckedUInt64 = 0
            Dim L As Int32 = KeyByte.Length - 1
            For i As Int32 = 0 To L - 1
                hash = (hash * seed) + KeyByte(i) + 3
            Next

            Return (hash And &H7FFFFFFF)
        End Function

        Public Function HashRS(Key As String, Optional seed As Long = 131) As ULong
            Dim hash As UncheckedUInt64 = 0
            Dim b As ULong = 378551
            Dim a As ULong = 63689
            Dim L As Int32 = Key.Length - 1
            Dim KeyCharArr() As Char = Key.ToArray
            For i As Int32 = 0 To L - 1
                hash = (hash * a) + Asc(KeyCharArr(i))
                a = a * b
            Next

            Return (hash And &H7FFFFFFF)
        End Function

        Public Function HashRS(KeyByte() As Byte, Optional seed As Long = 131) As ULong
            Dim hash As UncheckedUInt64 = 0
            Dim b As ULong = 378551
            Dim a As ULong = 63689
            Dim L As Int32 = KeyByte.Length - 1

            For i As Int32 = 0 To L - 1
                hash = (hash * a) + KeyByte(i)
                a = a * b
            Next

            Return (hash And &H7FFFFFFF)
        End Function

        Public Function HashSDBM(Key As String) As ULong
            Dim hash As UncheckedUInt64 = 0
            Dim L As Int32 = Key.Length - 1
            Dim KeyCharArr() As Char = Key.ToArray
            For i As Int32 = 0 To L - 1
                hash = Asc(KeyCharArr(i)) + (hash << 6) + (hash << 16) - hash
            Next

            Return (hash And &H7FFFFFFF)
        End Function

        Public Function HashSDBM(KeyByte() As Byte) As ULong
            Dim hash As UncheckedUInt64 = 0
            Dim L As Int32 = KeyByte.Length - 1

            For i As Int32 = 0 To L - 1
                hash = KeyByte(i) + (hash << 6) + (hash << 16) - hash
            Next

            Return (hash And &H7FFFFFFF)
        End Function

        Public Function HashJS(Key As String) As ULong
            Dim hash As UncheckedUInt64 = 1315423911
            Dim L As Int32 = Key.Length - 1
            Dim KeyCharArr() As Char = Key.ToArray
            For i As Int32 = 0 To L - 1
                hash = hash Xor (((hash << 5) + Asc(KeyCharArr(i)) + (hash >> 2)))
            Next
            Return hash
        End Function

        Public Function HashJS(KeyByte() As Byte) As ULong
            Dim hash As UncheckedUInt64 = 1315423911
            Dim L As Int32 = KeyByte.Length - 1

            For i As Int32 = 0 To L - 1
                hash = hash Xor (((hash << 5) + KeyByte(i) + (hash >> 2)))
            Next
            Return hash
        End Function

        Public Function HashPJW(Key As String) As ULong
            Dim BitsInUnsignedInt As ULong = CLng(4 * 8)
            Dim ThreeQuarters As ULong = CLng((BitsInUnsignedInt * 3) / 4)
            Dim OneEighth As ULong = CLng(BitsInUnsignedInt / 8)
            Dim HighBits As ULong = CLng(&HFFFFFFFF) << (BitsInUnsignedInt - OneEighth)
            Dim hash As UncheckedUInt64 = 0
            Dim test As ULong = 0
            Dim L As Int32 = Key.Length - 1
            Dim KeyCharArr() As Char = Key.ToArray

            For I As Int32 = 0 To L
                hash = (hash << OneEighth) + Asc(KeyCharArr(I))

                If ((test = hash And HighBits) <> 0) Then
                    hash = ((hash Xor (test >> ThreeQuarters)) And (Not HighBits))
                End If
            Next

            Return hash
        End Function

        Public Function HashPJW(KeyByte() As Byte) As ULong
            Dim BitsInUnsignedInt As ULong = CLng(4 * 8)
            Dim ThreeQuarters As ULong = CLng((BitsInUnsignedInt * 3) / 4)
            Dim OneEighth As ULong = CLng(BitsInUnsignedInt / 8)
            Dim HighBits As ULong = CLng(&HFFFFFFFF) << (BitsInUnsignedInt - OneEighth)
            Dim hash As UncheckedUInt64 = 0
            Dim test As ULong = 0
            Dim L As Int32 = KeyByte.Length - 1

            For I As Int32 = 0 To L
                hash = (hash << OneEighth) + KeyByte(I)
                If ((test = hash And HighBits) <> 0) Then
                    hash = ((hash Xor (test >> ThreeQuarters)) And (Not HighBits))
                End If

            Next

            Return hash
        End Function

        ReadOnly __hashAP As Long = &HAAAAAAAA

        Public Function HashAP(Key As String) As ULong
            Dim hash As New UncheckedUInt64(__hashAP)
            Dim L As Int32 = Key.Length - 1
            Dim KeyCharArr() As Char = Key.ToArray

            For i As Int32 = 0 To L
                If ((i And 1) = 0) Then
                    hash = hash Xor (((hash << 7) Xor Asc(KeyCharArr(i)) * (hash >> 3)))
                Else
                    hash = hash Xor ((Not ((hash << 11) + Asc(KeyCharArr(i)) Xor (hash >> 5))))
                End If
            Next

            Return hash
        End Function

        Public Function HashAP(KeyByte() As Byte) As ULong
            Dim hash As New UncheckedUInt64(__hashAP)
            Dim L As Int32 = KeyByte.Length - 1

            For i As Int32 = 0 To L
                If ((i And 1) = 0) Then
                    hash = hash Xor (((hash << 7) Xor KeyByte(i) * (hash >> 3)))
                Else
                    hash = hash Xor ((Not ((hash << 11) + KeyByte(i) Xor (hash >> 5))))
                End If
            Next
            Return hash
        End Function

        Public Function HashDEK(Key As String) As ULong
            Dim L As Int32 = Key.Length - 1
            Dim KeyCharArr() As Char = Key.ToArray
            Dim hash As UncheckedUInt64 = L + 1
            For i As Int32 = 0 To L
                hash = ((hash << 5) Xor (hash >> 27)) Xor Asc(KeyCharArr(i))
            Next
            Return hash
        End Function

        Public Function HashDEK(KeyByte() As Byte) As ULong
            Dim L As Int32 = KeyByte.Length - 1
            Dim hash As UncheckedUInt64 = L + 1
            For i As Int32 = 0 To L
                hash = ((hash << 5) Xor (hash >> 27)) Xor KeyByte(i)
            Next
            Return hash
        End Function

        Public Function HashELF(key$) As ULong
            Dim L As Int32 = key.Length - 1
            Dim KeyCharArr() As Char = key.ToArray
            Dim hash As UncheckedUInt64 = 0
            Dim x As Long = 0
            For i As Int32 = 0 To L
                hash = (hash << 4) + Asc(KeyCharArr(i))
                x = hash And &HF0000000L
                If x <> 0 Then
                    hash = hash Xor (x >> 24)
                End If
                hash = hash And (Not x)
            Next

            Return hash
        End Function

        Public Function HashELF(KeyByte() As Byte) As ULong
            Dim L As Int32 = KeyByte.Length - 1
            Dim hash As UncheckedUInt64 = 0
            Dim x As Long = 0

            For i As Int32 = 0 To L
                hash = (hash << 4) + KeyByte(i)
                x = hash And &HF0000000L
                If x <> 0 Then
                    hash = hash Xor (x >> 24)
                End If
                hash = hash And (Not x)
            Next

            Return hash
        End Function

        ''' <summary>
        ''' 经典times33算法。简单高效。[这个使用移位代替*33]
        ''' 测试一千万。没有重复哈希值。
        ''' </summary>
        ''' <param name="Key"></param>
        ''' <returns></returns>
        Public Function HashCMyMap(key$) As ULong
            Dim nHash As UncheckedUInt64 = 0
            Dim L As Int32 = key.Length - 1
            Dim KeyCharArr() As Char = key.ToArray
            Dim I As Int32 = 0

            While (I < L)
                nHash = (nHash << 5) + nHash + Asc(KeyCharArr(I)) + 3
                I += 1
            End While

            Return nHash
        End Function

        ''' <summary>
        ''' 经典times33算法。简单高效。[这个使用移位代替*33]
        ''' 测试一千万。没有重复哈希值。
        ''' </summary>
        ''' <param name="KeyByte"></param>
        ''' <returns></returns>
        Public Function HashCMyMap(KeyByte() As Byte) As ULong
            Dim nHash As UncheckedUInt64 = 0
            Dim L As Int32 = KeyByte.Length - 1
            Dim I As Int32 = 0

            While (I < L)
                nHash = (nHash << 5) + nHash + KeyByte(I) + 3
                I += 1
            End While

            Return nHash
        End Function

        ''' <summary>
        ''' 经典的Time算法。简单，高效。
        ''' Ngix使用的是 time31，Tokyo Cabinet使用的是 time37
        ''' 小写英文词汇适合33, 大小写混合使用65。time33比较适合的是英文词汇的hash.
        ''' </summary>
        ''' <param name="Key"></param>
        ''' <param name="seed">种子数。 31，33，37 。。。</param>
        ''' <returns></returns>
        Public Function HashTimeMap(key$, seed As Int16) As ULong
            Dim nHash As UncheckedUInt64 = 0
            Dim L As Int32 = key.Length - 1
            Dim KeyCharArr() As Char = key.ToArray
            Dim I As Int32 = 0

            While (I < L)
                nHash = seed * nHash + nHash + Asc(KeyCharArr(I)) + 3
                I += 1
            End While

            Return nHash
        End Function

        ''' <summary>
        ''' 经典的Time算法。简单，高效。
        ''' Ngix使用的是 time31，Tokyo Cabinet使用的是 time37
        ''' 小写英文词汇适合33, 大小写混合使用65。time33比较适合的是英文词汇的hash.
        ''' </summary>
        ''' <param name="KeyByte"></param>
        ''' <param name="seed">种子质数。 31，33，37 。。。</param>
        ''' <returns></returns>
        Public Function HashTimeMap(KeyByte() As Byte, seed As UInt32) As ULong
            Dim nHash As UncheckedUInt64 = 0
            Dim L As Int32 = KeyByte.Length - 1
            Dim I As Int32 = 0

            While (I < L)
                nHash = seed * nHash + nHash + KeyByte(I) + 3
                I += 1
            End While

            Return nHash
        End Function
#End Region
    End Module
End Namespace

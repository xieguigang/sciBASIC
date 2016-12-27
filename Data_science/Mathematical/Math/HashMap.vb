#Region "Microsoft.VisualBasic::5e165e28360f0b42630a47b78cbf020b, ..\sciBASIC#\Data_science\Mathematical\Math\HashMap.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.InteropServices

''' <summary>
''' VB.NET常用的哈希算法集.其中包括了著名的暴雪的哈希,T33哈希.......
''' 不同的哈希算法在分布式,布降过滤器,位图MAP等等应用得比较多...
''' </summary>
''' <remarks>
''' http://bbs.csdn.net/topics/391950537
''' </remarks>
Public Module HashMap

    ''' <summary>
    ''' <see cref="UInt64"/>
    ''' </summary>
    <StructLayout(LayoutKind.Explicit)>
    Public Structure UncheckedUInt64

        <FieldOffset(0)>
        Private longValue As UInt64
        <FieldOffset(0)>
        Private intValueLo As UInt32
        <FieldOffset(4)>
        Private intValueHi As UInt32

        Sub New(newLongValue As UInt64)
            longValue = newLongValue
        End Sub

        Public Overrides Function ToString() As String
            Return longValue
        End Function

        Public Overloads Shared Widening Operator CType(value As UInt64) As UncheckedUInt64
            Return New UncheckedUInt64(value)
        End Operator

        Public Overloads Shared Widening Operator CType(value As UncheckedUInt64) As UInt64
            Return value.longValue
        End Operator

        Public Overloads Shared Operator *(x As UncheckedUInt64, y As UInt64) As UncheckedUInt64
            Return New UncheckedUInt64(x.longValue * y)
        End Operator

        Public Overloads Shared Operator /(x As UncheckedUInt64, y As UInt64) As UncheckedUInt64
            Return New UncheckedUInt64(x.longValue / y)
        End Operator

        Public Overloads Shared Operator ^(x As UncheckedUInt64, y As Double) As UncheckedUInt64
            Return New UncheckedUInt64(x.longValue ^ y)
        End Operator

        Public Overloads Shared Operator Xor(x As UncheckedUInt64, y As UInt64) As UncheckedUInt64
            Return New UncheckedUInt64(x.longValue Xor y)
        End Operator

        Public Overloads Shared Operator +(x As UncheckedUInt64, y As UInt64) As UncheckedUInt64
            Return New UncheckedUInt64(x.longValue + y)
        End Operator

        Public Overloads Shared Operator -(x As UncheckedUInt64, y As UInt64) As UncheckedUInt64
            Return New UncheckedUInt64(x.longValue - y)
        End Operator

        Public Overloads Shared Operator <<(x As UncheckedUInt64, y As Int32) As UncheckedUInt64
            Return New UncheckedUInt64(x.longValue << y)
        End Operator

        Public Overloads Shared Operator >>(x As UncheckedUInt64, y As Int32) As UncheckedUInt64
            Return New UncheckedUInt64(x.longValue >> y)
        End Operator

        Public Overloads Shared Operator And(x As UncheckedUInt64, y As UInt64) As UncheckedUInt64
            Return New UncheckedUInt64(x.longValue And y)
        End Operator

        Public Overloads Shared Operator =(x As UncheckedUInt64, y As UInt64) As UncheckedUInt64
            Return New UncheckedUInt64(x.longValue)
        End Operator

        Public Overloads Shared Operator <>(x As UncheckedUInt64, y As UInt64) As UncheckedUInt64
            Return New UncheckedUInt64(x.longValue <> y)
        End Operator

        Public Overloads Shared Operator Not(x As UncheckedUInt64) As UncheckedUInt64
            Return New UncheckedUInt64(Not x.longValue)
        End Operator
    End Structure

#Region "哈希算法"

    ''' <summary>
    ''' 和 HashCMyMap 基本一样.
    ''' </summary>
    ''' <param name="Key"></param>
    ''' <returns></returns>
    Public Function HashDJB(Key As String) As UInt64
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
    Public Function HashDJB(KeyByte() As Byte) As UInt64
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
    Public Function HashBKDR(Key As String, Optional seed As Long = 131) As UInt64
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
    Public Function HashBKDR(KeyByte() As Byte, Optional seed As Long = 131) As UInt64
        Dim hash As UncheckedUInt64 = 0
        Dim L As Int32 = KeyByte.Length - 1
        For i As Int32 = 0 To L - 1
            hash = (hash * seed) + KeyByte(i) + 3
        Next

        Return (hash And &H7FFFFFFF)
    End Function

    Public Function HashRS(Key As String, Optional seed As Long = 131) As UInt64
        Dim hash As UncheckedUInt64 = 0
        Dim b As UInt64 = 378551
        Dim a As UInt64 = 63689
        Dim L As Int32 = Key.Length - 1
        Dim KeyCharArr() As Char = Key.ToArray
        For i As Int32 = 0 To L - 1
            hash = (hash * a) + Asc(KeyCharArr(i))
            a = a * b
        Next

        Return (hash And &H7FFFFFFF)
    End Function

    Public Function HashRS(KeyByte() As Byte, Optional seed As Long = 131) As UInt64
        Dim hash As UncheckedUInt64 = 0
        Dim b As UInt64 = 378551
        Dim a As UInt64 = 63689
        Dim L As Int32 = KeyByte.Length - 1

        For i As Int32 = 0 To L - 1
            hash = (hash * a) + KeyByte(i)
            a = a * b
        Next

        Return (hash And &H7FFFFFFF)
    End Function

    Public Function HashSDBM(Key As String) As UInt64
        Dim hash As UncheckedUInt64 = 0
        Dim L As Int32 = Key.Length - 1
        Dim KeyCharArr() As Char = Key.ToArray
        For i As Int32 = 0 To L - 1
            hash = Asc(KeyCharArr(i)) + (hash << 6) + (hash << 16) - hash
        Next

        Return (hash And &H7FFFFFFF)
    End Function

    Public Function HashSDBM(KeyByte() As Byte) As UInt64
        Dim hash As UncheckedUInt64 = 0
        Dim L As Int32 = KeyByte.Length - 1

        For i As Int32 = 0 To L - 1
            hash = KeyByte(i) + (hash << 6) + (hash << 16) - hash
        Next

        Return (hash And &H7FFFFFFF)
    End Function

    Public Function HashJS(Key As String) As UInt64
        Dim hash As UncheckedUInt64 = 1315423911
        Dim L As Int32 = Key.Length - 1
        Dim KeyCharArr() As Char = Key.ToArray
        For i As Int32 = 0 To L - 1
            hash = hash Xor (((hash << 5) + Asc(KeyCharArr(i)) + (hash >> 2)))
        Next
        Return hash
    End Function

    Public Function HashJS(KeyByte() As Byte) As UInt64
        Dim hash As UncheckedUInt64 = 1315423911
        Dim L As Int32 = KeyByte.Length - 1

        For i As Int32 = 0 To L - 1
            hash = hash Xor (((hash << 5) + KeyByte(i) + (hash >> 2)))
        Next
        Return hash
    End Function

    Public Function HashPJW(Key As String) As UInt64
        Dim BitsInUnsignedInt As UInt64 = CLng(4 * 8)
        Dim ThreeQuarters As UInt64 = CLng((BitsInUnsignedInt * 3) / 4)
        Dim OneEighth As UInt64 = CLng(BitsInUnsignedInt / 8)
        Dim HighBits As UInt64 = CLng(&HFFFFFFFF) << (BitsInUnsignedInt - OneEighth)
        Dim hash As UncheckedUInt64 = 0
        Dim test As UInt64 = 0
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

    Public Function HashPJW(KeyByte() As Byte) As UInt64
        Dim BitsInUnsignedInt As UInt64 = CLng(4 * 8)
        Dim ThreeQuarters As UInt64 = CLng((BitsInUnsignedInt * 3) / 4)
        Dim OneEighth As UInt64 = CLng(BitsInUnsignedInt / 8)
        Dim HighBits As UInt64 = CLng(&HFFFFFFFF) << (BitsInUnsignedInt - OneEighth)
        Dim hash As UncheckedUInt64 = 0
        Dim test As UInt64 = 0
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

    Public Function HashAP(Key As String) As UInt64
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

    Public Function HashAP(KeyByte() As Byte) As UInt64
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

    Public Function HashDEK(Key As String) As UInt64
        Dim L As Int32 = Key.Length - 1
        Dim KeyCharArr() As Char = Key.ToArray
        Dim hash As UncheckedUInt64 = L + 1
        For i As Int32 = 0 To L
            hash = ((hash << 5) Xor (hash >> 27)) Xor Asc(KeyCharArr(i))
        Next
        Return hash
    End Function

    Public Function HashDEK(KeyByte() As Byte) As UInt64
        Dim L As Int32 = KeyByte.Length - 1
        Dim hash As UncheckedUInt64 = L + 1
        For i As Int32 = 0 To L
            hash = ((hash << 5) Xor (hash >> 27)) Xor KeyByte(i)
        Next
        Return hash
    End Function

    Public Function HashELF(key$) As UInt64
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

    Public Function HashELF(KeyByte() As Byte) As UInt64
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

    Dim cryptTable(&H100 * 5 - 1) As UInt64
    Dim IsInitcryptTable As Boolean = False

    Public Sub HashBlizzardInit()
        Dim seed As UInt64 = &H100001
        Dim index1 As UInt64 = 0
        Dim index2 As UInt64 = 0
        Dim I As UInt64
        Dim KKK As UInt64 = 0
        For index1 = 0 To &H100 - 1
            index2 = index1
            For I = 0 To 4
                Dim temp1, temp2 As UInt64
                seed = (seed * 125 + 3) Mod &H2AAAAB
                temp1 = (seed And &HFFFF) << &H10
                seed = (seed * 125 + 3) Mod &H2AAAAB
                temp2 = (seed And &HFFFF)
                cryptTable(index2) = (temp1 Or temp2) '//|
                index2 += &H100
            Next
        Next

        IsInitcryptTable = True
    End Sub

    ReadOnly __hashBlizzard_seed2 As Long = &HEEEEEEEE

    ''' <summary>
    ''' 暴雪公司出名的哈希码.
    ''' 测试了 二千万 GUID, 没有重复.但运算量比较大。
    ''' </summary>
    ''' <param name="key"></param>
    ''' <param name="HasType">HasType =0 ,1 ,2 </param>
    ''' <returns></returns>
    Public Function HashBlizzard(Key As String, Optional HasType As Long = 0) As UInt64
        If IsInitcryptTable = False Then
            Call HashBlizzardInit()
        End If

        Dim L As Int32 = Key.Length - 1
        Dim KeyCharArr() As Char = Key.ToArray
        Dim seed1 As UncheckedUInt64 = &H7FED7FED
        Dim seed2 As New UncheckedUInt64(__hashBlizzard_seed2)
        Dim LoopID As Int32 = 0
        While (LoopID < L)
            Dim ascCode As Int32 = Asc(KeyCharArr(LoopID))
            seed1 = cryptTable((HasType << 8) + ascCode) Xor (seed1 + seed2)
            seed2 = ascCode + seed1 + seed2 + (seed2 << 5) + 3
            LoopID += 1
        End While

        Return seed1
    End Function

    ''' <summary>
    ''' 暴雪公司著名的 HashMap .
    ''' 测试了 二千万 GUID, 没有重复.但运算量比较大。
    ''' </summary>
    ''' <param name="KeyByte"></param>
    ''' <param name="HasType">HasType =[0 ,1 ,2] </param>
    ''' <returns></returns>
    Public Function HashBlizzard(KeyByte() As Byte, Optional HasType As Long = 0) As UInt64
        If IsInitcryptTable = False Then
            Call HashBlizzardInit()
        End If

        Dim L As Int32 = KeyByte.Length - 1
        Dim seed1 As UncheckedUInt64 = &H7FED7FED
        Dim seed2 As New UncheckedUInt64(__hashBlizzard_seed2)
        Dim LoopID As Int32 = 0

        While (LoopID < L)
            Dim ascCode As Int32 = KeyByte(LoopID)
            seed1 = cryptTable((HasType << 8) + ascCode) Xor (seed1 + seed2)
            seed2 = ascCode + seed1 + seed2 + (seed2 << 5) + 3
            LoopID += 1
        End While

        Return seed1
    End Function

    ''' <summary>
    ''' 经典times33算法。简单高效。[这个使用移位代替*33]
    ''' 测试一千万。没有重复哈希值。
    ''' </summary>
    ''' <param name="Key"></param>
    ''' <returns></returns>
    Public Function HashCMyMap(key$) As UInt64
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
    Public Function HashCMyMap(KeyByte() As Byte) As UInt64
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
    Public Function HashTimeMap(key$, seed As Int16) As UInt64
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
    Public Function HashTimeMap(KeyByte() As Byte, seed As UInt32) As UInt64
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


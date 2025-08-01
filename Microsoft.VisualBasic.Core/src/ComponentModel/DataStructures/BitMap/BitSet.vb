#Region "Microsoft.VisualBasic::fc58dced97fb91b1899b7ac627c5805a, Microsoft.VisualBasic.Core\src\ComponentModel\DataStructures\BitMap\BitSet.vb"

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

    '   Total Lines: 672
    '    Code Lines: 513 (76.34%)
    ' Comment Lines: 54 (8.04%)
    '    - Xml Docs: 90.74%
    ' 
    '   Blank Lines: 105 (15.62%)
    '     File Size: 24.64 KB


    '     Class BitSet
    ' 
    '         Properties: Count, IsSynchronized, SyncRoot
    ' 
    '         Constructor: (+15 Overloads) Sub New
    ' 
    '         Function: [And], [Get], [Not], [Or], [Set]
    '                   [Xor], (+2 Overloads) Append, BinaryBitwiseOp, Clone, Concatenate
    '                   Duplicate, Equals, FromBinaryString, FromHexString, GetBits
    '                   GetEnumerator, GetHashCode, Repeat, RequiredSize, Reverse
    '                   SetAll, (+3 Overloads) SetBits, SplitEvery, ToArray, ToBinaryString
    '                   ToBytes, ToHexString, (+2 Overloads) ToInteger, ToString, ValueOf
    ' 
    '         Sub: CopyTo, Extend, (+2 Overloads) InitializeFrom
    ' 
    '         Operators: +, <<, <>, =, >>
    '                    (+2 Overloads) And, (+2 Overloads) Not, (+2 Overloads) Or, (+2 Overloads) Xor
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Globalization
Imports System.Numerics
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Threading
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports std = System.Math

Namespace ComponentModel

    ''' <summary>
    ''' A replacement for BitArray.(BitSet in java)
    ''' </summary>
    ''' <remarks>
    ''' https://stackoverflow.com/questions/14035687/what-is-the-c-sharp-equivalent-of-bitset-of-java#
    ''' </remarks>
    Public Class BitSet
        Implements IEnumerable
        Implements ICollection
        Implements ICloneable

        Shared ReadOnly ONE As UInt32 = CUInt(1) << 31
        Shared EndianFixer As Func(Of Byte(), Byte()) = Nothing

        Dim bits As UInt32() = Nothing
        Dim _length As Integer = 0
        Dim _syncRoot As Object

        Public Property Count() As Integer Implements ICollection.Count
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Me._length
            End Get
            Private Set
                If Value > Me._length Then
                    Extend(Value - Me._length)
                Else
                    Me._length = std.Max(0, Value)
                End If
            End Set
        End Property

        Public ReadOnly Property IsSynchronized() As Boolean Implements ICollection.IsSynchronized
            Get
                Return False
            End Get
        End Property

        Public ReadOnly Property SyncRoot() As Object Implements ICollection.SyncRoot
            Get
                If Me._syncRoot Is Nothing Then
                    Interlocked.CompareExchange(Of Object)(Me._syncRoot, New Object(), Nothing)
                End If
                Return _syncRoot
            End Get
        End Property

        Default Public Property Item(index As Integer) As Boolean
            Get
                If index >= _length Then
                    Throw New IndexOutOfRangeException()
                End If

                Dim byteIndex As Integer = index >> 5
                Dim bitIndex As Integer = index And &H1F
                Return ((bits(byteIndex) << bitIndex) And ONE) <> 0
            End Get
            Set
                If index >= _length Then
                    Throw New IndexOutOfRangeException()
                End If

                Dim byteIndex As Integer = index >> 5
                Dim bitIndex As Integer = index And &H1F

                If Value Then
                    bits(byteIndex) = bits(byteIndex) Or (ONE >> bitIndex)
                Else
                    bits(byteIndex) = bits(byteIndex) And Not (ONE >> bitIndex)
                End If
            End Set
        End Property

#Region "Constructors"

        Shared Sub New()
            If BitConverter.IsLittleEndian Then
                EndianFixer = Function(a) a.Reverse().ToArray()
            Else
                EndianFixer = Function(a) a
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(bits As BitSet)
            Me.InitializeFrom(bits.ToArray())
        End Sub

        Public Sub New(bits As BitArray)
            Me._length = bits.Count
            Me.bits = New UInt32(RequiredSize(Me._length) - 1) {}

            For i As Integer = 0 To bits.Count - 1
                Me(i) = bits(i)
            Next
        End Sub

        ''' <summary>
        ''' create bits vector data from a given 32 bits integer
        ''' </summary>
        ''' <param name="v"></param>
        Public Sub New(v As Integer)
            Dim bytes As ICollection(Of Byte) = EndianFixer(BitConverter.GetBytes(v)).ToList()

            Call InitializeFrom(bytes)
        End Sub

        Sub New(b As Byte)
            Dim bytes As ICollection(Of Byte) = EndianFixer({b}).ToList()
            Call InitializeFrom(bytes)
        End Sub

        Sub New(v As Short)
            Dim bytes As ICollection(Of Byte) = EndianFixer(BitConverter.GetBytes(v)).ToList()
            Call InitializeFrom(bytes)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(bits As ICollection(Of Boolean))
            Me.InitializeFrom(bits.ToArray())
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(bytes As ICollection(Of Byte))
            InitializeFrom(bytes)
        End Sub

        Public Sub New(data As ICollection(Of Short))
            Dim bytes As ICollection(Of Byte) = data.SelectMany(Function(v) EndianFixer(BitConverter.GetBytes(v))).ToList()
            InitializeFrom(bytes)
        End Sub

        Public Sub New(srcBits As ICollection(Of UShort))
            Dim bytes As ICollection(Of Byte) = srcBits.SelectMany(Function(v) EndianFixer(BitConverter.GetBytes(v))).ToList()
            InitializeFrom(bytes)
        End Sub

        Public Sub New(srcBits As ICollection(Of Integer))
            Dim bytes As ICollection(Of Byte) = srcBits.SelectMany(Function(v) EndianFixer(BitConverter.GetBytes(v))).ToList()
            InitializeFrom(bytes)
        End Sub

        Public Sub New(srcBits As ICollection(Of UInteger))
            Dim bytes As ICollection(Of Byte) = srcBits.SelectMany(Function(v) EndianFixer(BitConverter.GetBytes(v))).ToList()
            InitializeFrom(bytes)
        End Sub

        Public Sub New(srcBits As ICollection(Of Long))
            Dim bytes As ICollection(Of Byte) = srcBits.SelectMany(Function(v) EndianFixer(BitConverter.GetBytes(v))).ToList()
            InitializeFrom(bytes)
        End Sub

        Public Sub New(srcBits As ICollection(Of ULong))
            Dim bytes As ICollection(Of Byte) = srcBits.SelectMany(Function(v) EndianFixer(BitConverter.GetBytes(v))).ToList()
            InitializeFrom(bytes)
        End Sub

        Public Sub New(capacity As Integer, Optional defaultValue As Boolean = False)
            Me.bits = New UInt32(RequiredSize(capacity) - 1) {}
            Me._length = capacity

            ' Only need to do this if true, because default for all bits is false
            If defaultValue Then
                For i As Integer = 0 To Me._length - 1
                    Me(i) = True
                Next
            End If
        End Sub

        Private Sub InitializeFrom(bytes As ICollection(Of Byte))
            Me._length = bytes.Count * 8
            Me.bits = New UInt32(RequiredSize(Me._length) - 1) {}

            For i As Integer = 0 To bytes.Count - 1
                Dim bv As UInteger = bytes.Skip(i).Take(1).[Single]()

                For b As Integer = 0 To 7
                    Dim bitVal As Boolean = ((bv << b) And &H80) <> 0
                    Dim bi As Integer = 8 * i + b

                    Me(bi) = bitVal
                Next
            Next
        End Sub

        Private Sub InitializeFrom(bits As ICollection(Of Boolean))
            Dim index As i32 = 0

            Me._length = bits.Count
            Me.bits = New UInt32(RequiredSize(Me._length) - 1) {}

            For Each b As Boolean In bits
                Me(++index) = b
            Next
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function RequiredSize(bitCapacity As Integer) As Integer
            Return (bitCapacity + 31) >> 5
        End Function

#End Region

#Region "Interfaces implementation"
#Region "IEnumerable"
        Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            'for (int i = 0; i < _length; i++) yield return this[i];
            Return Me.ToArray().GetEnumerator()
        End Function
#End Region
#Region "ICollection"
        Public Sub CopyTo(array As Array, index As Integer) Implements ICollection.CopyTo
            If array Is Nothing Then
                Throw New ArgumentNullException("array")
            End If
            If index < 0 Then
                Throw New ArgumentOutOfRangeException("index")
            End If
            If array.Rank <> 1 Then
                Throw New ArgumentException("Multidimensional array not supported")
            End If
            If TypeOf array Is UInt32() Then
                Array.Copy(Me.bits, 0, array, index, (Me.Count + Marshal.SizeOf(GetType(UInt32)) - 1) \ Marshal.SizeOf(GetType(UInt32)))
            ElseIf TypeOf array Is Boolean() Then
                Array.Copy(Me.ToArray(), 0, array, index, Me.Count)
            Else
                Throw New ArgumentException("Array type not supported (UInt32[] or bool[] only)")
            End If
        End Sub
#End Region

#Region "ICloneable"

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Clone() As Object Implements ICloneable.Clone
            Return New BitSet(Me)
        End Function

        ''' <summary>
        ''' Not part of ICloneable, but better - returns a strongly-typed result
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Duplicate() As BitSet
            Return New BitSet(Me)
        End Function
#End Region

#End Region

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function ValueOf(ParamArray bytes As Byte()) As BitSet
            Return New BitSet(bytes)
        End Function

#Region "String Conversions"

        ''' <summary>
        ''' <see cref="ToBinaryString"/>
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return ToBinaryString()
        End Function

        Public Shared Function FromHexString(hex As String) As BitSet
            If hex Is Nothing Then
                Throw New ArgumentNullException("hex")
            End If

            Dim bits As New List(Of Boolean)()
            For i As Integer = 0 To hex.Length - 1
                Dim b As Integer = Byte.Parse(hex(i).ToString(), NumberStyles.HexNumber)
                bits.Add((b >> 3) = 1)
                bits.Add(((b And &H7) >> 2) = 1)
                bits.Add(((b And &H3) >> 1) = 1)
                bits.Add((b And &H1) = 1)
            Next
            Dim ba As New BitSet(bits.ToArray())
            Return ba
        End Function

        Public Function ToHexString(Optional bitSep8 As String = Nothing, Optional bitSep128 As String = Nothing) As String
            Dim s As String = String.Empty
            Dim b As Integer = 0
            Dim bbits As Boolean() = Me.ToArray()

            For i As Integer = 1 To bbits.Length
                b = (b << 1) Or (If(bbits(i - 1), 1, 0))
                If i Mod 4 = 0 Then
                    s = s & String.Format("{0:x}", b)
                    b = 0
                End If

                If i Mod (8 * 16) = 0 Then
                    s = s & bitSep128
                ElseIf i Mod 8 = 0 Then
                    s = s & bitSep8
                End If
            Next
            Dim ebits As Integer = bbits.Length Mod 4
            If ebits <> 0 Then
                b = b << (4 - ebits)
                s = s & String.Format("{0:x}", b)
            End If
            Return s
        End Function

        Shared ReadOnly defaultTrue As [Default](Of Char()) = {"1"c, "Y"c, "y"c, "T"c, "t"c}

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="bin"></param>
        ''' <param name="true">The chars in this collection means <see cref="Boolean.True"/></param>
        ''' <returns></returns>
        Public Shared Function FromBinaryString(bin As String, Optional [true] As Char() = Nothing) As BitSet
            If bin Is Nothing Then
                Throw New ArgumentNullException("bin")
            End If

            Dim trueChars As Index(Of Char) = [true] Or defaultTrue
            Dim ba As New BitSet(bin.Length)

            For i As Integer = 0 To bin.Length - 1
                ba(i) = bin(i) Like trueChars
            Next

            Return ba
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ToBinaryString(Optional setChar As Char = "1"c, Optional unsetChar As Char = "0"c) As String
            Return New String(Me.ToArray().[Select](Function(v) If(v, setChar, unsetChar)).ToArray())
        End Function

#End Region

#Region "Class Methods"
        Public Function ToArray() As Boolean()
            Dim vbits As Boolean() = New Boolean(Me._length - 1) {}
            For i As Integer = 0 To _length - 1
                vbits(i) = Me(i)
            Next
            Return vbits
        End Function

        Public Function Append(addBits As ICollection(Of Boolean)) As BitSet
            Dim startPos As Integer = Me._length
            Extend(addBits.Count)
            Dim bitArray As Boolean() = addBits.ToArray()
            For i As Integer = 0 To bitArray.Length - 1
                Me(i + startPos) = bitArray(i)
            Next
            Return Me
        End Function

        Public Function Append(addBits As BitSet) As BitSet
            Return Me.Append(addBits.ToArray())
        End Function

        Public Shared Function Concatenate(ParamArray bArrays As BitSet()) As BitSet
            Return New BitSet(bArrays.SelectMany(Function(ba) ba.ToArray()).ToArray())
        End Function

        Private Sub Extend(numBits As Integer)
            numBits += Me._length
            Dim reqBytes As Integer = RequiredSize(numBits)
            If reqBytes > Me.bits.Length Then
                Dim newBits As UInt32() = New UInt32(reqBytes - 1) {}
                Me.bits.CopyTo(newBits, 0)
                Me.bits = newBits
            End If
            Me._length = numBits
        End Sub

        Public Function [Get](index As Integer) As Boolean
            Return Me(index)
        End Function

        Public Function GetBits(Optional startBit As Integer = 0, Optional numBits As Integer = -1) As BitSet
            If numBits = -1 Then
                numBits = bits.Length
            End If
            Return New BitSet(Me.ToArray().Skip(startBit).Take(numBits).ToArray())
        End Function

        Public Function Repeat(numReps As Integer) As BitSet
            Dim oBits As Boolean() = Me.ToArray()
            Dim nBits As New List(Of Boolean)()
            For i As Integer = 0 To numReps - 1
                nBits.AddRange(oBits)
            Next
            Me.InitializeFrom(nBits)
            Return Me
        End Function

        Public Function Reverse() As BitSet
            Dim n As Integer = Me.Count
            For i As Integer = 0 To n \ 2 - 1
                Dim b1 As Boolean = Me(i)
                Me(i) = Me(n - i - 1)
                Me(n - i - 1) = b1
            Next
            Return Me
        End Function

        ''' <summary>
        ''' Set specific index bit to TRUE
        ''' </summary>
        ''' <param name="index"></param>
        ''' <param name="v"></param>
        ''' <returns></returns>
        Public Function [Set](index As Integer, Optional v As Boolean = True) As BitSet
            Me(index) = v
            Return Me
        End Function

        Public Function SetAll(v As Boolean) As BitSet
            For i As Integer = 0 To Me.Count - 1
                Me(i) = v
            Next
            Return Me
        End Function

        Public Function SetBits(b8 As Byte, Optional start As Integer = Scan0) As BitSet
            Dim bits8 As New BitSet(b8)
            Return SetBits(bits8.ToArray, start, Scan0)
        End Function

        Public Function SetBits(i16 As Short, Optional start As Integer = Scan0) As BitSet
            Dim bits16 As New BitSet(i16)
            Return SetBits(bits16.ToArray, start, Scan0)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="bits"></param>
        ''' <param name="destStartBit"></param>
        ''' <param name="srcStartBit"></param>
        ''' <param name="numBits">
        ''' negative value means set all <paramref name="bits"/>.
        ''' </param>
        ''' <param name="allowExtend"></param>
        ''' <returns></returns>
        Public Function SetBits(bits As ICollection(Of Boolean),
                            Optional destStartBit As Integer = 0,
                            Optional srcStartBit As Integer = 0,
                            Optional numBits As Integer = -1,
                            Optional allowExtend As Boolean = False) As BitSet

            If bits Is Nothing Then
                Throw New ArgumentNullException("setBits")
            End If
            If (destStartBit < 0) OrElse (destStartBit >= Me.Count) Then
                Throw New ArgumentOutOfRangeException("destStartBit")
            End If
            If (srcStartBit < 0) OrElse (srcStartBit >= bits.Count) Then
                Throw New ArgumentOutOfRangeException("srcStartBit")
            End If

            Dim sBits As Boolean()
            If TypeOf bits Is Boolean() Then
                sBits = DirectCast(bits, Boolean())
            Else
                sBits = bits.ToArray()
            End If

            If numBits = -1 Then
                numBits = bits.Count
            End If
            If numBits > (bits.Count - srcStartBit) Then
                numBits = bits.Count - srcStartBit
            End If

            Dim diffSize As Integer = numBits - (Me.Count - destStartBit)
            If diffSize > 0 Then
                If allowExtend Then
                    Extend(diffSize)
                Else
                    numBits = Me.Count - destStartBit
                End If
            End If
            For i As Integer = 0 To numBits - 1
                Me(destStartBit + i) = sBits(srcStartBit + i)
            Next
            Return Me
        End Function

        Public Function SplitEvery(numBits As Integer) As List(Of BitSet)
            Dim i As Integer = 0
            Dim bitSplits As New List(Of BitSet)()
            While i < Me.Count
                bitSplits.Add(Me.GetBits(i, numBits))
                i += numBits
            End While
            Return bitSplits
        End Function

        Public Function ToBytes(Optional startBit As Integer = 0, Optional numBits As Integer = -1) As Byte()
            If numBits = -1 Then
                numBits = Me._length - startBit
            End If
            Dim ba As BitSet = GetBits(startBit, numBits)
            Dim nb As Integer = (numBits + 7) \ 8
            Dim bb As Byte() = New Byte(nb - 1) {}
            For i As Integer = 0 To ba.Count - 1
                If Not ba(i) Then
                    Continue For
                End If
                Dim bp As Integer = 7 - (i Mod 8)
                bb(i \ 8) = CByte(CInt(bb(i \ 8)) Or (1 << bp))
            Next
            Return bb
        End Function

        Shared ReadOnly TWO As BigInteger = 2

        ''' <summary>
        ''' create a 32 bit integer
        ''' </summary>
        ''' <returns></returns>
        Public Function ToInteger() As Integer
            Return ToInteger(Scan0, 32)
        End Function

        ''' <summary>
        ''' bits to integer
        ''' </summary>
        ''' <param name="start"></param>
        ''' <param name="length"></param>
        ''' <returns></returns>
        Public Function ToInteger(start%, length%) As Integer
            If length <= 0 Then
                Throw New InvalidProgramException($"{NameOf(length)} must be > 0!")
            End If

            Dim result As BigInteger = 0

            For i As Integer = 0 To length - 1
                If Me.Get(start + i) Then
                    result = result + BigInteger.Pow(TWO, i)
                End If
            Next

            Return result
        End Function
#End Region

#Region "Logical Bitwise Operations"
        Public Function BinaryBitwiseOp(op As Func(Of Boolean, Boolean, Boolean), ba As BitSet, Optional start As Integer = 0) As BitSet
            For i As Integer = 0 To ba.Count - 1
                If start + i >= Me.Count Then
                    Exit For
                End If
                Me(start + i) = op(Me(start + i), ba(i))
            Next
            Return Me
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function [Xor](xor__1 As BitSet, Optional start As Integer = 0) As BitSet
            Return BinaryBitwiseOp(Function(a, b) (a Xor b), xor__1, start)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function [And](and__1 As BitSet, Optional start As Integer = 0) As BitSet
            Return BinaryBitwiseOp(Function(a, b) (a And b), and__1, start)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function [Or](or__1 As BitSet, Optional start As Integer = 0) As BitSet
            Return BinaryBitwiseOp(Function(a, b) (a Or b), or__1, start)
        End Function

        Public Function [Not](Optional start As Integer = 0, Optional len As Integer = -1) As BitSet
            For i As Integer = start To Me.Count - 1

                If Interlocked.Decrement(len) = -1 Then
                    Exit For
                End If

                Me(i) = Not Me(i)
            Next
            Return Me
        End Function
#End Region

#Region "Class Operators"

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator +(a As BitSet, b As BitSet) As BitSet
            Return a.Duplicate().Append(b)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator Or(a As BitSet, b As BitSet) As BitSet
            Return a.Duplicate().[Or](b)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator And(a As BitSet, b As BitSet) As BitSet
            Return a.Duplicate().[And](b)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator Xor(a As BitSet, b As BitSet) As BitSet
            Return a.Duplicate().[Xor](b)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator Not(a As BitSet) As BitSet
            Return a.Duplicate().[Not]()
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <<(a As BitSet, shift As Integer) As BitSet
            Return a.Duplicate().Append(New Boolean(shift - 1) {})
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator >>(a As BitSet, shift As Integer) As BitSet
            Return New BitSet(a.ToArray().Take(std.Max(0, a.Count - shift)).ToArray())
        End Operator

        Public Shared Operator =(a As BitSet, b As BitSet) As Boolean
            If a.Count <> b.Count Then
                Return False
            End If
            For i As Integer = 0 To a.Count - 1
                If a(i) <> b(i) Then
                    Return False
                End If
            Next
            Return True
        End Operator

        Public Overrides Function Equals(obj As Object) As Boolean
            If Not (TypeOf obj Is BitSet) Then
                Return False
            End If
            Return (Me Is DirectCast(obj, BitSet))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function GetHashCode() As Integer
            Return Me.ToHexString().GetHashCode()
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <>(a As BitSet, b As BitSet) As Boolean
            Return Not (a = b)
        End Operator

#End Region
    End Class
End Namespace

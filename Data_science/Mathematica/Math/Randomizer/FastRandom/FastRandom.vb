#Region "Microsoft.VisualBasic::d5fe11ea24a44bf758277e7a1c7668b8, Data_science\Mathematica\Math\Randomizer\FastRandom\FastRandom.vb"

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

    '   Total Lines: 362
    '    Code Lines: 181
    ' Comment Lines: 133
    '   Blank Lines: 48
    '     File Size: 13.80 KB


    ' Class FastRandom
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: (+3 Overloads) [Next], NextBool, NextDouble, NextFloat, NextInt
    '               NextUInt
    ' 
    '     Sub: NextBytes, NextFloats, Reinitialise
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language

''' <summary>
''' A fast random number generator for .NET, from https://www.codeproject.com/Articles/9187/A-fast-equivalent-for-System-Random
''' Colin Green, January 2005
'''
''' September 4th 2005
''' 	 Added NextBytesUnsafe() - commented out by default.
''' 	 Fixed bug in Reinitialise() - y,z and w variables were not being reset.
'''
''' Key points:
'''  1) Based on a simple and fast xor-shift pseudo random number generator (RNG) specified in:
'''  Marsaglia, George. (2003). Xorshift RNGs.
'''  http://www.jstatsoft.org/v08/i14/xorshift.pdf
'''
'''  This particular implementation of xorshift has a period of 2^128-1. See the above paper to see
'''  how this can be easily extened if you need a longer period. At the time of writing I could find no
'''  information on the period of System.Random for comparison.
'''
'''  2) Faster than System.Random. Up to 8x faster, depending on which methods are called.
'''
'''  3) Direct replacement for System.Random. This class implements all of the methods that System.Random
'''  does plus some additional methods. The like named methods are functionally equivalent.
'''
'''  4) Allows fast re-initialisation with a seed, unlike System.Random which accepts a seed at construction
'''  time which then executes a relatively expensive initialisation routine. This provides a vast speed improvement
'''  if you need to reset the pseudo-random number sequence many times, e.g. if you want to re-generate the same
'''  sequence many times. An alternative might be to cache random numbers in an array, but that approach is limited
'''  by memory capacity and the fact that you may also want a large number of different sequences cached. Each sequence
'''  can each be represented by a single seed value (int) when using FastRandom.
'''
'''  Notes.
'''  A further performance improvement can be obtained by declaring local variables as static, thus avoiding
'''  re-allocation of variables on each call. However care should be taken if multiple instances of
'''  FastRandom are in use or if being used in a multi-threaded environment.
'''
''' </summary>
Public Class FastRandom

    ' The +1 ensures NextDouble doesn't generate 1.0
    Const FLOAT_UNIT_INT As Single = 1.0F / (Integer.MaxValue + 1.0F)
    Const REAL_UNIT_INT As Double = 1.0 / (Integer.MaxValue + 1.0)
    Const REAL_UNIT_UINT As Double = 1.0 / (UInteger.MaxValue + 1.0)
    Const Y As UInteger = 842502087, Z As UInteger = 3579807591, W As UInteger = 273326509

    Dim x, yField, zField, wField As UInteger

    ''' <summary>
    ''' Initialises a new instance using time dependent seed.
    ''' </summary>
    Public Sub New()
        ' Initialise using the system tick count.
        Reinitialise(Environment.TickCount)
    End Sub

    ''' <summary>
    ''' Initialises a new instance using an int value as seed.
    ''' This constructor signature is provided to maintain compatibility with
    ''' System.Random
    ''' </summary>
    Public Sub New(seed As Integer)
        Reinitialise(seed)
    End Sub

    ''' <summary>
    ''' Reinitialises using an int value as a seed.
    ''' </summary>
    Public Sub Reinitialise(seed As Integer)
        ' The only stipulation stated for the xorshift RNG is that at least one of
        ' the seeds x,y,z,w is non-zero. We fulfill that requirement by only allowing
        ' resetting of the x seed
        x = CUInt(seed)
        yField = FastRandom.Y
        zField = FastRandom.Z
        wField = FastRandom.W
    End Sub

    ''' <summary>
    ''' Generates a random int over the range 0 to int.MaxValue-1.
    ''' MaxValue is not generated in order to remain functionally equivalent to System.Random.Next().
    ''' This does slightly eat into some of the performance gain over System.Random, but not much.
    ''' For better performance see:
    '''
    ''' Call NextInt() for an int over the range 0 to int.MaxValue.
    '''
    ''' Call NextUInt() and cast the result to an int to generate an int over the full Int32 value range
    ''' including negative values.
    ''' </summary>
    Public Function [Next]() As Integer
        Dim t = x Xor x << 11

        x = yField
        yField = zField
        zField = wField
        wField = wField Xor wField >> 19 Xor t Xor t >> 8

        ' Handle the special case where the value int.MaxValue is generated. This is outside of
        ' the range of permitted values, so we therefore call Next() to try again.
        Dim rtn As UInteger = wField And &H7FFFFFFF

        If rtn = &H7FFFFFFF Then
            Return [Next]()
        Else
            Return rtn
        End If
    End Function

    ''' <summary>
    ''' Generates a random int over the range 0 to upperBound-1, and not including upperBound.
    ''' </summary>
    Public Function [Next](upperBound As Integer) As Integer
        Dim t As UInteger

        If upperBound < 0 Then
            Throw New ArgumentOutOfRangeException("upperBound", upperBound, "upperBound must be >=0")
        Else
            t = x Xor x << 11
        End If

        x = yField
        yField = zField
        zField = wField

        ' The explicit int cast before the first multiplication gives better performance.
        ' See comments in NextDouble.
        wField = wField Xor wField >> 19 Xor t Xor t >> 8

        Return CInt(FastRandom.REAL_UNIT_INT * CInt(&H7FFFFFFF And wField) * upperBound)
    End Function

    ''' <summary>
    ''' Generates a random int over the range lowerBound to upperBound-1, and not including upperBound.
    ''' upperBound must be >= lowerBound. lowerBound may be negative.
    ''' </summary>
    Public Function [Next](lowerBound As Integer, upperBound As Integer) As Integer
        Dim t As UInteger

        If lowerBound > upperBound Then
            Throw New ArgumentOutOfRangeException("upperBound", upperBound, "upperBound must be >=lowerBound")
        Else
            t = x Xor x << 11
        End If

        x = yField
        yField = zField
        zField = wField

        ' The explicit int cast before the first multiplication gives better performance.
        ' See comments in NextDouble.
        Dim range = upperBound - lowerBound
        Dim rnd As Integer

        If range < 0 Then
            ' If range is <0 then an overflow has occured and must resort to using long integer arithmetic instead (slower).
            ' We also must use all 32 bits of precision, instead of the normal 31, which again is slower.
            wField = wField Xor wField >> 19 Xor t Xor t >> 8
            rnd = lowerBound + CInt(FastRandom.REAL_UNIT_UINT * CDbl(wField) * (upperBound - CLng(lowerBound)))
        Else
            ' 31 bits of precision will suffice if range<=int.MaxValue. This allows us to cast to an int and gain
            ' a little more performance.
            wField = wField Xor wField >> 19 Xor t Xor t >> 8
            rnd = lowerBound + CInt(FastRandom.REAL_UNIT_INT * CInt(&H7FFFFFFF And wField) * range)
        End If

        If rnd = upperBound Then
            Return upperBound - 1
        Else
            Return rnd
        End If
    End Function

    ''' <summary>
    ''' Generates a random double. Values returned are from 0.0 up to but not including 1.0.
    ''' </summary>
    Public Function NextDouble() As Double
        Dim t = x Xor x << 11

        x = yField
        yField = zField
        zField = wField

        ' Here we can gain a 2x speed improvement by generating a value that can be cast to
        ' an int instead of the more easily available uint. If we then explicitly cast to an
        ' int the compiler will then cast the int to a double to perform the multiplication,
        ' this final cast is a lot faster than casting from a uint to a double. The extra cast
        ' to an int is very fast (the allocated bits remain the same) and so the overall effect
        ' of the extra cast is a significant performance improvement.
        '
        ' Also note that the loss of one bit of precision is equivalent to what occurs within
        ' System.Random.
        wField = wField Xor wField >> 19 Xor t Xor t >> 8

        Return FastRandom.REAL_UNIT_INT * CInt(&H7FFFFFFF And wField)
    End Function

    ''' <summary>
    ''' Generates a random double. Values returned are from 0.0 up to but not including 1.0.
    ''' </summary>
    Public Function NextFloat() As Single
        Dim x = Me.x, y = yField, z = zField, w = wField
        Dim t = x Xor x << 11
        x = y
        y = z
        z = w
        w = w Xor w >> 19 Xor t Xor t >> 8
        Dim value = FastRandom.FLOAT_UNIT_INT * (&H7FFFFFFF And w)
        Me.x = x
        yField = y
        zField = z
        wField = w
        Return value
    End Function

    ''' <summary>
    ''' Fills the provided byte array with random floats.
    ''' </summary>
    Public Sub NextFloats(buffer As Double())
        Dim x = Me.x, y = yField, z = zField, w = wField
        Dim i As i32 = 0
        Dim t As UInteger
        Dim bound = buffer.Length

        While i < bound
            t = x Xor x << 11
            x = y
            y = z
            z = w
            w = w Xor w >> 19 Xor t Xor t >> 8
            buffer(++i) = FastRandom.FLOAT_UNIT_INT * (&H7FFFFFFF And w)
        End While

        Me.x = x
        yField = y
        zField = z
        wField = w
    End Sub


    ''' <summary>
    ''' Fills the provided byte array with random bytes.
    ''' This method is functionally equivalent to System.Random.NextBytes().
    ''' </summary>
    Public Sub NextBytes(buffer As Byte())
        ' Fill up the bulk of the buffer in chunks of 4 bytes at a time.
        Dim x = Me.x, y = yField, z = zField, w = wField
        Dim i As i32 = 0
        Dim t As UInteger
        Dim bound = buffer.Length - 3

        While i < bound
            ' Generate 4 bytes.
            ' Increased performance is achieved by generating 4 random bytes per loop.
            ' Also note that no mask needs to be applied to zero out the higher order bytes before
            ' casting because the cast ignores thos bytes. Thanks to Stefan Troschütz for pointing this out.
            t = x Xor x << 11
            x = y
            y = z
            z = w
            w = w Xor w >> 19 Xor t Xor t >> 8
            buffer(++i) = CByte(w)
            buffer(++i) = CByte(w >> 8)
            buffer(++i) = CByte(w >> 16)
            buffer(++i) = CByte(w >> 24)
        End While

        ' Fill up any remaining bytes in the buffer.
        If i < buffer.Length Then
            ' Generate 4 bytes.
            t = x Xor x << 11
            x = y
            y = z
            z = w
            w = w Xor w >> 19 Xor t Xor t >> 8
            buffer(++i) = CByte(w)

            If i < buffer.Length Then
                buffer(++i) = CByte(w >> 8)

                If i < buffer.Length Then
                    buffer(++i) = CByte(w >> 16)

                    If i < buffer.Length Then
                        buffer(i) = CByte(w >> 24)
                    End If
                End If
            End If
        End If

        Me.x = x
        yField = y
        zField = z
        wField = w
    End Sub

    ''' <summary>
    ''' Generates a uint. Values returned are over the full range of a uint,
    ''' uint.MinValue to uint.MaxValue, inclusive.
    '''
    ''' This is the fastest method for generating a single random number because the underlying
    ''' random number generator algorithm generates 32 random bits that can be cast directly to
    ''' a uint.
    ''' </summary>
    Public Function NextUInt() As UInteger
        Dim t = x Xor x << 11
        x = yField
        yField = zField
        zField = wField
        wField = wField Xor wField >> 19 Xor t Xor t >> 8

        Return wField
    End Function

    ''' <summary>
    ''' Generates a random int over the range 0 to int.MaxValue, inclusive.
    ''' This method differs from Next() only in that the range is 0 to int.MaxValue
    ''' and not 0 to int.MaxValue-1.
    '''
    ''' The slight difference in range means this method is slightly faster than Next()
    ''' but is not functionally equivalent to System.Random.Next().
    ''' </summary>
    Public Function NextInt() As Integer
        Dim t = x Xor x << 11
        x = yField
        yField = zField
        zField = wField
        wField = wField Xor wField >> 19 Xor t Xor t >> 8

        Return CInt(&H7FFFFFFF And wField)
    End Function


    ' Buffer 32 bits in bitBuffer, return 1 at a time, keep track of how many have been returned
    ' with bitBufferIdx.
    Private bitBuffer As UInteger
    Private bitMask As UInteger = 1

    ''' <summary>
    ''' Generates a single random bit.
    ''' This method's performance is improved by generating 32 bits in one operation and storing them
    ''' ready for future calls.
    ''' </summary>
    Public Function NextBool() As Boolean
        If bitMask = 1 Then
            ' Generate 32 more bits.
            Dim t = x Xor x << 11
            x = yField
            yField = zField
            zField = wField
            wField = wField Xor wField >> 19 Xor t Xor t >> 8

            bitBuffer = wField

            ' Reset the bitMask that tells us which bit to read next.
            bitMask = &H80000000
            Return (bitBuffer And bitMask) = 0
        End If

        bitMask = 1

        Return (bitBuffer And 1) = 0
    End Function
End Class

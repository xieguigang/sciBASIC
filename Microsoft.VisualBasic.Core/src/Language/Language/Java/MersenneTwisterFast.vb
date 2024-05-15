#Region "Microsoft.VisualBasic::fb7d384016451b2e147093e9fe1a3eff, Microsoft.VisualBasic.Core\src\Language\Language\Java\MersenneTwisterFast.vb"

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

    '   Total Lines: 917
    '    Code Lines: 625
    ' Comment Lines: 159
    '   Blank Lines: 133
    '     File Size: 41.24 KB


    '     Class MersenneTwisterFast
    ' 
    '         Properties: Seed
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: nextBoolean, nextByte, nextChar, nextDouble, nextFloat
    '                   nextGamma, nextGaussian, (+2 Overloads) nextInt, nextLong, nextShort
    '                   permuted, shuffled
    ' 
    '         Sub: nextBytes, permute, (+2 Overloads) shuffle
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports stdNum = System.Math

'
' * MersenneTwisterFast.java
' *
' * Copyright (C) 2002-2006 Alexei Drummond and Andrew Rambaut
' *
' * This file is part of BEAST.
' * See the NOTICE file distributed with this work for additional
' * information regarding copyright ownership and licensing.
' *
' * BEAST is free software; you can redistribute it and/or modify
' * it under the terms of the GNU Lesser General Public License as
' * published by the Free Software Foundation; either version 2
' * of the License, or (at your option) any later version.
' *
' *  BEAST is distributed in the hope that it will be useful,
' *  but WITHOUT ANY WARRANTY; without even the implied warranty of
' *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' *  GNU Lesser General Public License for more details.
' *
' * You should have received a copy of the GNU Lesser General Public
' * License along with BEAST; if not, write to the
' * Free Software Foundation, Inc., 51 Franklin St, Fifth Floor,
' * Boston, MA  02110-1301  USA
' 

Namespace Language.Java

    ''' <summary>
    ''' MersenneTwisterFast:
    '''  
    ''' A simulation quality fast random number generator (MT19937) with the same
    ''' public methods as java.util.Random.
    '''  
    '''  
    ''' About the Mersenne Twister. This is a Java version of the C-program for
    ''' MT19937: Integer version. next(32) generates one pseudorandom unsigned
    ''' integer (32bit) which is uniformly distributed among 0 to 2^32-1 for each
    ''' call. next(int bits) >>>'s by (32-bits) to get a value ranging between 0 and
    ''' 2^bits-1 long inclusive; hope that's correct. setSeed(seed) set initial
    ''' values to the working area of 624 words. For setSeed(seed), seed is any
    ''' 32-bit integer except for 0.
    '''  
    ''' Reference. M. Matsumoto and T. Nishimura, "Mersenne Twister: A
    ''' 623-Dimensionally Equidistributed Uniform Pseudo-Random Number Generator",
    ''' <i>ACM Transactions on Modeling and Computer Simulation,</i> Vol. 8, No. 1,
    ''' January 1998, pp 3--30.
    '''  
    '''  
    ''' Bug Fixes. This implementation implements the bug fixes made in Java 1.2's
    ''' version of Random, which means it can be used with earlier versions of Java.
    ''' See <a href=
    ''' "http://www.javasoft.com/products/jdk/1.2/docs/api/java/util/Random.html">
    ''' the JDK 1.2 java.util.Random documentation</a> for further documentation on
    ''' the random-number generation contracts made. Additionally, there's an
    ''' undocumented bug in the JDK java.util.Random.nextBytes() method, which this
    ''' code fixes.
    '''  
    '''  
    ''' Important Note. Just like java.util.Random, this generator accepts a long
    ''' seed but doesn't use all of it. java.util.Random uses 48 bits. The Mersenne
    ''' Twister instead uses 32 bits (int size). So it's best if your seed does not
    ''' exceed the int range.
    '''  
    '''  
    ''' <a href="http://www.cs.umd.edu/users/seanl/">Sean Luke's web page</a>
    '''  
    '''  
    ''' - added shuffling method (Alexei Drummond)
    '''  
    ''' - added gamma RV method (Marc Suchard)
    '''  
    ''' This is now package private - it should be accessed using the instance in
    ''' Random
    ''' </summary>
    <Serializable>
    Public Class MersenneTwisterFast
        ''' 
        Private Const serialVersionUID As Long = 6185086957226269797L
        ' Period parameters
        Private Const N As Integer = 624
        Private Const M As Integer = 397
        Private Const MATRIX_A As Integer = &H9908B0DF ' private static final *
        ' constant vector a
        Private Const UPPER_MASK As Integer = &H80000000 ' most significant w-r
        ' bits
        Private Const LOWER_MASK As Integer = &H7FFFFFFF ' least significant r
        ' bits

        ' Tempering parameters
        Private Const TEMPERING_MASK_B As Integer = &H9D2C5680
        Private Const TEMPERING_MASK_C As Integer = &HEFC60000

        ' #define TEMPERING_SHIFT_U(y) (y >>> 11)
        ' #define TEMPERING_SHIFT_S(y) (y << 7)
        ' #define TEMPERING_SHIFT_T(y) (y << 15)
        ' #define TEMPERING_SHIFT_L(y) (y >>> 18)

        Private mt As Integer() ' the array for the state vector
        Private mti As Integer ' mti==N+1 means mt[N] is not initialized
        Private mag01 As Integer()

        ' a good initial seed (of int size, though stored in a long)
        Private Const GOOD_SEED As Long = 4357

        Private nextNextGaussian As Double
        Private haveNextNextGaussian As Boolean

        ' The following can be accessed externally by the static accessor methods
        ' which
        ' inforce synchronization
        Public Shared ReadOnly DEFAULT_INSTANCE As New MersenneTwisterFast

        ' Added to curernt time in default constructor, and then adjust to allow
        ' for programs that construct
        ' multiple MersenneTwisterFast in a short amount of time.
        Private Shared seedAdditive_ As Long = 0

        Private initializationSeed As Long

        ''' <summary>
        ''' Constructor using the time of day as default seed.
        ''' </summary>
        Public Sub New()
            Me.New(Now.ToBinary + seedAdditive_)
            seedAdditive_ += nextInt()
        End Sub

        ''' <summary>
        ''' Constructor using a given seed. Though you pass this seed in as a long,
        ''' it's best to make sure it's actually an integer.
        ''' </summary>
        ''' <param name="seed">
        '''            generator starting number, often the time of day. </param>
        Private Sub New(seed As Long)
            If seed = 0 Then
                seed = GOOD_SEED
            Else
                seed = seed
            End If
        End Sub

        ''' <summary>
        ''' Initalize the pseudo random number generator. The Mersenne Twister only
        ''' uses an integer for its seed; It's best that you don't pass in a long
        ''' that's bigger than an int.
        ''' </summary>
        Public Property Seed As Long
            Set(seed As Long)
                If seed = 0 Then Throw New System.ArgumentException("Non zero random seed required.")
                initializationSeed = seed
                haveNextNextGaussian = False

                mt = New Integer(N - 1) {}

                ' setting initial seeds to mt[N] using
                ' the generator Line 25 of Table 1 in
                ' [KNUTH 1981, The Art of Computer Programming
                ' Vol. 2 (2nd Ed.), pp102]

                ' the 0xffffffff is commented out because in Java
                ' ints are always 32 bits; hence i & 0xffffffff == i

                mt(0) = (CInt(seed)) ' & 0xffffffff;

                For mti = 1 To N - 1
                    mt(mti) = (69069 * mt(mti - 1)) ' & 0xffffffff;
                Next mti

                ' mag01[x] = x * MATRIX_A for x=0,1
                mag01 = New Integer(1) {}
                mag01(0) = &H0
                mag01(1) = MATRIX_A
            End Set
            Get
                Return initializationSeed
            End Get
        End Property


        Public Function nextInt() As Integer
            Dim y As Integer

            If mti >= N Then ' generate N words at one time
                Dim kk As Integer

                For kk = 0 To N - M - 1
                    y = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                    mt(kk) = mt(kk + M) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)
                Next kk
                Do While kk < N - 1
                    y = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                    mt(kk) = mt(kk + (M - N)) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)
                    kk += 1
                Loop
                y = (mt(N - 1) And UPPER_MASK) Or (mt(0) And LOWER_MASK)
                mt(N - 1) = mt(M - 1) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)

                mti = 0
            End If

            y = mt(mti)
            mti += 1
            y = y Xor CInt(CUInt(y) >> 11) ' TEMPERING_SHIFT_U(y)
            y = y Xor (y << 7) And TEMPERING_MASK_B ' TEMPERING_SHIFT_S(y)
            y = y Xor (y << 15) And TEMPERING_MASK_C ' TEMPERING_SHIFT_T(y)
            y = y Xor (CInt(CUInt(y) >> 18)) ' TEMPERING_SHIFT_L(y)

            Return y
        End Function

        Public Function nextShort() As Short
            Dim y As Integer

            If mti >= N Then ' generate N words at one time
                Dim kk As Integer

                For kk = 0 To N - M - 1
                    y = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                    mt(kk) = mt(kk + M) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)
                Next kk
                Do While kk < N - 1
                    y = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                    mt(kk) = mt(kk + (M - N)) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)
                    kk += 1
                Loop
                y = (mt(N - 1) And UPPER_MASK) Or (mt(0) And LOWER_MASK)
                mt(N - 1) = mt(M - 1) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)

                mti = 0
            End If

            y = mt(mti)
            mti += 1
            y = y Xor CInt(CUInt(y) >> 11) ' TEMPERING_SHIFT_U(y)
            y = y Xor (y << 7) And TEMPERING_MASK_B ' TEMPERING_SHIFT_S(y)
            y = y Xor (y << 15) And TEMPERING_MASK_C ' TEMPERING_SHIFT_T(y)
            y = y Xor (CInt(CUInt(y) >> 18)) ' TEMPERING_SHIFT_L(y)

            Return CShort(CInt(CUInt(y) >> 16))
        End Function

        Public Function nextChar() As Char
            Dim y As Integer

            If mti >= N Then ' generate N words at one time
                Dim kk As Integer

                For kk = 0 To N - M - 1
                    y = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                    mt(kk) = mt(kk + M) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)
                Next kk
                Do While kk < N - 1
                    y = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                    mt(kk) = mt(kk + (M - N)) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)
                    kk += 1
                Loop
                y = (mt(N - 1) And UPPER_MASK) Or (mt(0) And LOWER_MASK)
                mt(N - 1) = mt(M - 1) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)

                mti = 0
            End If

            y = mt(mti)
            mti += 1
            y = y Xor CInt(CUInt(y) >> 11) ' TEMPERING_SHIFT_U(y)
            y = y Xor (y << 7) And TEMPERING_MASK_B ' TEMPERING_SHIFT_S(y)
            y = y Xor (y << 15) And TEMPERING_MASK_C ' TEMPERING_SHIFT_T(y)
            y = y Xor (CInt(CUInt(y) >> 18)) ' TEMPERING_SHIFT_L(y)

            Return ChrW(CInt(CUInt(y) >> 16))
        End Function

        Public Function nextBoolean() As Boolean
            Dim y As Integer

            If mti >= N Then ' generate N words at one time
                Dim kk As Integer

                For kk = 0 To N - M - 1
                    y = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                    mt(kk) = mt(kk + M) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)
                Next kk
                Do While kk < N - 1
                    y = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                    mt(kk) = mt(kk + (M - N)) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)
                    kk += 1
                Loop
                y = (mt(N - 1) And UPPER_MASK) Or (mt(0) And LOWER_MASK)
                mt(N - 1) = mt(M - 1) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)

                mti = 0
            End If

            y = mt(mti)
            mti += 1
            y = y Xor CInt(CUInt(y) >> 11) ' TEMPERING_SHIFT_U(y)
            y = y Xor (y << 7) And TEMPERING_MASK_B ' TEMPERING_SHIFT_S(y)
            y = y Xor (y << 15) And TEMPERING_MASK_C ' TEMPERING_SHIFT_T(y)
            y = y Xor (CInt(CUInt(y) >> 18)) ' TEMPERING_SHIFT_L(y)

            Return ((CInt(CUInt(y) >> 31)) <> 0)
        End Function

        Public Function nextByte() As SByte
            Dim y As Integer

            If mti >= N Then ' generate N words at one time
                Dim kk As Integer

                For kk = 0 To N - M - 1
                    y = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                    mt(kk) = mt(kk + M) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)
                Next kk
                Do While kk < N - 1
                    y = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                    mt(kk) = mt(kk + (M - N)) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)
                    kk += 1
                Loop
                y = (mt(N - 1) And UPPER_MASK) Or (mt(0) And LOWER_MASK)
                mt(N - 1) = mt(M - 1) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)

                mti = 0
            End If

            y = mt(mti)
            mti += 1
            y = y Xor CInt(CUInt(y) >> 11) ' TEMPERING_SHIFT_U(y)
            y = y Xor (y << 7) And TEMPERING_MASK_B ' TEMPERING_SHIFT_S(y)
            y = y Xor (y << 15) And TEMPERING_MASK_C ' TEMPERING_SHIFT_T(y)
            y = y Xor (CInt(CUInt(y) >> 18)) ' TEMPERING_SHIFT_L(y)

            Return CByte(CInt(CUInt(y) >> 24))
        End Function

        Public Sub nextBytes(bytes As SByte())
            Dim y As Integer

            For x As Integer = 0 To bytes.Length - 1
                If mti >= N Then ' generate N words at one time
                    Dim kk As Integer

                    For kk = 0 To N - M - 1
                        y = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                        mt(kk) = mt(kk + M) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)
                    Next kk
                    Do While kk < N - 1
                        y = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                        mt(kk) = mt(kk + (M - N)) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)
                        kk += 1
                    Loop
                    y = (mt(N - 1) And UPPER_MASK) Or (mt(0) And LOWER_MASK)
                    mt(N - 1) = mt(M - 1) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)

                    mti = 0
                End If

                y = mt(mti)
                mti += 1
                y = y Xor CInt(CUInt(y) >> 11) ' TEMPERING_SHIFT_U(y)
                y = y Xor (y << 7) And TEMPERING_MASK_B ' TEMPERING_SHIFT_S(y)
                y = y Xor (y << 15) And TEMPERING_MASK_C ' TEMPERING_SHIFT_T(y)
                y = y Xor (CInt(CUInt(y) >> 18)) ' TEMPERING_SHIFT_L(y)

                bytes(x) = CByte(CInt(CUInt(y) >> 24))
            Next x
        End Sub

        Public Function nextLong() As Long
            Dim y As Integer
            Dim z As Integer

            If mti >= N Then ' generate N words at one time
                Dim kk As Integer

                For kk = 0 To N - M - 1
                    y = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                    mt(kk) = mt(kk + M) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)
                Next kk
                Do While kk < N - 1
                    y = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                    mt(kk) = mt(kk + (M - N)) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)
                    kk += 1
                Loop
                y = (mt(N - 1) And UPPER_MASK) Or (mt(0) And LOWER_MASK)
                mt(N - 1) = mt(M - 1) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)

                mti = 0
            End If

            y = mt(mti)
            mti += 1
            y = y Xor CInt(CUInt(y) >> 11) ' TEMPERING_SHIFT_U(y)
            y = y Xor (y << 7) And TEMPERING_MASK_B ' TEMPERING_SHIFT_S(y)
            y = y Xor (y << 15) And TEMPERING_MASK_C ' TEMPERING_SHIFT_T(y)
            y = y Xor (CInt(CUInt(y) >> 18)) ' TEMPERING_SHIFT_L(y)

            If mti >= N Then ' generate N words at one time
                Dim kk As Integer

                For kk = 0 To N - M - 1
                    z = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                    mt(kk) = mt(kk + M) Xor (CInt(CUInt(z) >> 1)) Xor mag01(z And &H1)
                Next kk
                Do While kk < N - 1
                    z = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                    mt(kk) = mt(kk + (M - N)) Xor (CInt(CUInt(z) >> 1)) Xor mag01(z And &H1)
                    kk += 1
                Loop
                z = (mt(N - 1) And UPPER_MASK) Or (mt(0) And LOWER_MASK)
                mt(N - 1) = mt(M - 1) Xor (CInt(CUInt(z) >> 1)) Xor mag01(z And &H1)

                mti = 0
            End If

            z = mt(mti)
            mti += 1
            z = z Xor CInt(CUInt(z) >> 11) ' TEMPERING_SHIFT_U(z)
            z = z Xor (z << 7) And TEMPERING_MASK_B ' TEMPERING_SHIFT_S(z)
            z = z Xor (z << 15) And TEMPERING_MASK_C ' TEMPERING_SHIFT_T(z)
            z = z Xor (CInt(CUInt(z) >> 18)) ' TEMPERING_SHIFT_L(z)

            Return ((CLng(y)) << 32) + CLng(z)
        End Function

        Public Function nextDouble() As Double
            Dim y As Integer
            Dim z As Integer

            If mti >= N Then ' generate N words at one time
                Dim kk As Integer

                For kk = 0 To N - M - 1
                    y = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                    mt(kk) = mt(kk + M) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)
                Next kk
                Do While kk < N - 1
                    y = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                    mt(kk) = mt(kk + (M - N)) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)
                    kk += 1
                Loop
                y = (mt(N - 1) And UPPER_MASK) Or (mt(0) And LOWER_MASK)
                mt(N - 1) = mt(M - 1) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)

                mti = 0
            End If

            y = mt(mti)
            mti += 1
            y = y Xor CInt(CUInt(y) >> 11) ' TEMPERING_SHIFT_U(y)
            y = y Xor (y << 7) And TEMPERING_MASK_B ' TEMPERING_SHIFT_S(y)
            y = y Xor (y << 15) And TEMPERING_MASK_C ' TEMPERING_SHIFT_T(y)
            y = y Xor (CInt(CUInt(y) >> 18)) ' TEMPERING_SHIFT_L(y)

            If mti >= N Then ' generate N words at one time
                Dim kk As Integer

                For kk = 0 To N - M - 1
                    z = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                    mt(kk) = mt(kk + M) Xor (CInt(CUInt(z) >> 1)) Xor mag01(z And &H1)
                Next kk
                Do While kk < N - 1
                    z = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                    mt(kk) = mt(kk + (M - N)) Xor (CInt(CUInt(z) >> 1)) Xor mag01(z And &H1)
                    kk += 1
                Loop
                z = (mt(N - 1) And UPPER_MASK) Or (mt(0) And LOWER_MASK)
                mt(N - 1) = mt(M - 1) Xor (CInt(CUInt(z) >> 1)) Xor mag01(z And &H1)

                mti = 0
            End If

            z = mt(mti)
            mti += 1
            z = z Xor CInt(CUInt(z) >> 11) ' TEMPERING_SHIFT_U(z)
            z = z Xor (z << 7) And TEMPERING_MASK_B ' TEMPERING_SHIFT_S(z)
            z = z Xor (z << 15) And TEMPERING_MASK_C ' TEMPERING_SHIFT_T(z)
            z = z Xor (CInt(CUInt(z) >> 18)) ' TEMPERING_SHIFT_L(z)

            ' derived from nextDouble documentation in jdk 1.2 docs, see top 
            Return (((CLng(CInt(CUInt(y) >> 6))) << 27) + (CInt(CUInt(z) >> 5))) / CDbl(1L << 53)
        End Function

        Public Function nextGaussian() As Double
            If haveNextNextGaussian Then
                haveNextNextGaussian = False
                Return nextNextGaussian
            Else
                Dim v1, v2, s As Double
                Do
                    Dim y As Integer
                    Dim z As Integer
                    Dim a As Integer
                    Dim b As Integer

                    If mti >= N Then ' generate N words at one time
                        Dim kk As Integer

                        For kk = 0 To N - M - 1
                            y = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                            mt(kk) = mt(kk + M) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)
                        Next kk
                        Do While kk < N - 1
                            y = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                            mt(kk) = mt(kk + (M - N)) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)
                            kk += 1
                        Loop
                        y = (mt(N - 1) And UPPER_MASK) Or (mt(0) And LOWER_MASK)
                        mt(N - 1) = mt(M - 1) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)

                        mti = 0
                    End If

                    y = mt(mti)
                    mti += 1
                    y = y Xor CInt(CUInt(y) >> 11) ' TEMPERING_SHIFT_U(y)
                    y = y Xor (y << 7) And TEMPERING_MASK_B ' TEMPERING_SHIFT_S(y)
                    y = y Xor (y << 15) And TEMPERING_MASK_C ' TEMPERING_SHIFT_T(y)
                    y = y Xor (CInt(CUInt(y) >> 18)) ' TEMPERING_SHIFT_L(y)

                    If mti >= N Then ' generate N words at one time
                        Dim kk As Integer

                        For kk = 0 To N - M - 1
                            z = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                            mt(kk) = mt(kk + M) Xor (CInt(CUInt(z) >> 1)) Xor mag01(z And &H1)
                        Next kk
                        Do While kk < N - 1
                            z = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                            mt(kk) = mt(kk + (M - N)) Xor (CInt(CUInt(z) >> 1)) Xor mag01(z And &H1)
                            kk += 1
                        Loop
                        z = (mt(N - 1) And UPPER_MASK) Or (mt(0) And LOWER_MASK)
                        mt(N - 1) = mt(M - 1) Xor (CInt(CUInt(z) >> 1)) Xor mag01(z And &H1)

                        mti = 0
                    End If

                    z = mt(mti)
                    mti += 1
                    z = z Xor CInt(CUInt(z) >> 11) ' TEMPERING_SHIFT_U(z)
                    z = z Xor (z << 7) And TEMPERING_MASK_B ' TEMPERING_SHIFT_S(z)
                    z = z Xor (z << 15) And TEMPERING_MASK_C ' TEMPERING_SHIFT_T(z)
                    z = z Xor (CInt(CUInt(z) >> 18)) ' TEMPERING_SHIFT_L(z)

                    If mti >= N Then ' generate N words at one time
                        Dim kk As Integer

                        For kk = 0 To N - M - 1
                            a = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                            mt(kk) = mt(kk + M) Xor (CInt(CUInt(a) >> 1)) Xor mag01(a And &H1)
                        Next kk
                        Do While kk < N - 1
                            a = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                            mt(kk) = mt(kk + (M - N)) Xor (CInt(CUInt(a) >> 1)) Xor mag01(a And &H1)
                            kk += 1
                        Loop
                        a = (mt(N - 1) And UPPER_MASK) Or (mt(0) And LOWER_MASK)
                        mt(N - 1) = mt(M - 1) Xor (CInt(CUInt(a) >> 1)) Xor mag01(a And &H1)

                        mti = 0
                    End If

                    a = mt(mti)
                    mti += 1
                    a = a Xor CInt(CUInt(a) >> 11) ' TEMPERING_SHIFT_U(a)
                    a = a Xor (a << 7) And TEMPERING_MASK_B ' TEMPERING_SHIFT_S(a)
                    a = a Xor (a << 15) And TEMPERING_MASK_C ' TEMPERING_SHIFT_T(a)
                    a = a Xor (CInt(CUInt(a) >> 18)) ' TEMPERING_SHIFT_L(a)

                    If mti >= N Then ' generate N words at one time
                        Dim kk As Integer

                        For kk = 0 To N - M - 1
                            b = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                            mt(kk) = mt(kk + M) Xor (CInt(CUInt(b) >> 1)) Xor mag01(b And &H1)
                        Next kk
                        Do While kk < N - 1
                            b = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                            mt(kk) = mt(kk + (M - N)) Xor (CInt(CUInt(b) >> 1)) Xor mag01(b And &H1)
                            kk += 1
                        Loop
                        b = (mt(N - 1) And UPPER_MASK) Or (mt(0) And LOWER_MASK)
                        mt(N - 1) = mt(M - 1) Xor (CInt(CUInt(b) >> 1)) Xor mag01(b And &H1)

                        mti = 0
                    End If

                    b = mt(mti)
                    mti += 1
                    b = b Xor CInt(CUInt(b) >> 11) ' TEMPERING_SHIFT_U(b)
                    b = b Xor (b << 7) And TEMPERING_MASK_B ' TEMPERING_SHIFT_S(b)
                    b = b Xor (b << 15) And TEMPERING_MASK_C ' TEMPERING_SHIFT_T(b)
                    b = b Xor (CInt(CUInt(b) >> 18)) ' TEMPERING_SHIFT_L(b)

                    '				
                    '				 * derived from nextDouble documentation in jdk 1.2 docs, see
                    '				 * top
                    '				 
                    v1 = 2 * ((((CLng(CInt(CUInt(y) >> 6))) << 27) + (CInt(CUInt(z) >> 5))) / CDbl(1L << 53)) - 1
                    v2 = 2 * ((((CLng(CInt(CUInt(a) >> 6))) << 27) + (CInt(CUInt(b) >> 5))) / CDbl(1L << 53)) - 1
                    s = v1 * v1 + v2 * v2
                Loop While s >= 1
                Dim multiplier As Double = stdNum.Sqrt(-2 * stdNum.Log(s) / s)
                nextNextGaussian = v2 * multiplier
                haveNextNextGaussian = True
                Return v1 * multiplier
            End If
        End Function

        Public Function nextFloat() As Single
            Dim y As Integer

            If mti >= N Then ' generate N words at one time
                Dim kk As Integer

                For kk = 0 To N - M - 1
                    y = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                    mt(kk) = mt(kk + M) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)
                Next kk
                Do While kk < N - 1
                    y = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                    mt(kk) = mt(kk + (M - N)) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)
                    kk += 1
                Loop
                y = (mt(N - 1) And UPPER_MASK) Or (mt(0) And LOWER_MASK)
                mt(N - 1) = mt(M - 1) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)

                mti = 0
            End If

            y = mt(mti)
            mti += 1
            y = y Xor CInt(CUInt(y) >> 11) ' TEMPERING_SHIFT_U(y)
            y = y Xor (y << 7) And TEMPERING_MASK_B ' TEMPERING_SHIFT_S(y)
            y = y Xor (y << 15) And TEMPERING_MASK_C ' TEMPERING_SHIFT_T(y)
            y = y Xor (CInt(CUInt(y) >> 18)) ' TEMPERING_SHIFT_L(y)

            Return (CInt(CUInt(y) >> 8)) / (CSng(1 << 24))
        End Function

        ''' <summary>
        ''' Returns an integer drawn uniformly from 0 to n-1. Suffice it to say, n
        ''' must be > 0, or an IllegalArgumentException is raised.
        ''' </summary>
        Public Overridable Function nextInt(n As Integer) As Integer
            If n <= 0 Then Throw New System.ArgumentException("n must be positive")

            If (n And -n) = n Then ' i.e., n is a power of 2
                Dim y As Integer

                If mti >= MersenneTwisterFast.N Then ' generate N words at one time
                    Dim kk As Integer

                    For kk = 0 To MersenneTwisterFast.N - M - 1
                        y = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                        mt(kk) = mt(kk + M) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)
                    Next kk
                    Do While kk < MersenneTwisterFast.N - 1
                        y = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                        mt(kk) = mt(kk + (M - MersenneTwisterFast.N)) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)
                        kk += 1
                    Loop
                    y = (mt(MersenneTwisterFast.N - 1) And UPPER_MASK) Or (mt(0) And LOWER_MASK)
                    mt(MersenneTwisterFast.N - 1) = mt(M - 1) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)

                    mti = 0
                End If

                y = mt(mti)
                mti += 1
                y = y Xor CInt(CUInt(y) >> 11) ' TEMPERING_SHIFT_U(y)
                y = y Xor (y << 7) And TEMPERING_MASK_B ' TEMPERING_SHIFT_S(y)
                y = y Xor (y << 15) And TEMPERING_MASK_C ' TEMPERING_SHIFT_T(y)
                y = y Xor (CInt(CUInt(y) >> 18)) ' TEMPERING_SHIFT_L(y)

                Return CInt(Fix((n * CLng(CInt(CUInt(y) >> 1))) >> 31))
            End If

            Dim bits, val As Integer
            Do
                Dim y As Integer

                If mti >= MersenneTwisterFast.N Then ' generate N words at one time
                    Dim kk As Integer

                    For kk = 0 To MersenneTwisterFast.N - M - 1
                        y = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                        mt(kk) = mt(kk + M) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)
                    Next kk
                    Do While kk < MersenneTwisterFast.N - 1
                        y = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                        mt(kk) = mt(kk + (M - MersenneTwisterFast.N)) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)
                        kk += 1
                    Loop
                    y = (mt(MersenneTwisterFast.N - 1) And UPPER_MASK) Or (mt(0) And LOWER_MASK)
                    mt(MersenneTwisterFast.N - 1) = mt(M - 1) Xor (CInt(CUInt(y) >> 1)) Xor mag01(y And &H1)

                    mti = 0
                End If

                y = mt(mti)
                mti += 1
                y = y Xor CInt(CUInt(y) >> 11) ' TEMPERING_SHIFT_U(y)
                y = y Xor (y << 7) And TEMPERING_MASK_B ' TEMPERING_SHIFT_S(y)
                y = y Xor (y << 15) And TEMPERING_MASK_C ' TEMPERING_SHIFT_T(y)
                y = y Xor (CInt(CUInt(y) >> 18)) ' TEMPERING_SHIFT_L(y)

                bits = (CInt(CUInt(y) >> 1))
                val = bits Mod n
            Loop While bits - val + (n - 1) < 0
            Return val
        End Function

        ''' <summary>
        ''' Returns a uniform random permutation of int objects in array
        ''' </summary>
        Public Sub permute(array As Integer())
            Dim l As Integer = array.Length
            For i As Integer = 0 To l - 1
                Dim index As Integer = nextInt(l - i) + i
                Dim temp As Integer = array(index)
                array(index) = array(i)
                array(i) = temp
            Next i
        End Sub

        ''' <summary>
        ''' Shuffles an array.
        ''' </summary>
        Public Sub shuffle(array As Integer())
            Dim l As Integer = array.Length
            For i As Integer = 0 To l - 1
                Dim index As Integer = nextInt(l - i) + i
                Dim temp As Integer = array(index)
                array(index) = array(i)
                array(i) = temp
            Next i
        End Sub

        ''' <summary>
        ''' Shuffles an array. Shuffles numberOfShuffles times
        ''' </summary>
        Public Sub shuffle(array As Integer(), numberOfShuffles As Integer)
            Dim i As Integer, j As Integer, temp As Integer, l As Integer = array.Length
            For shuffle As Integer = 0 To numberOfShuffles - 1
                Do
                    i = nextInt(l)
                    j = nextInt(l)
                Loop While i <> j
                temp = array(j)
                array(j) = array(i)
                array(i) = temp
            Next shuffle
        End Sub

        ''' <summary>
        ''' Returns an array of shuffled indices of length l.
        ''' </summary>
        ''' <param name="l">
        '''            length of the array required. </param>
        Public Overridable Function shuffled(l As Integer) As Integer()

            Dim array As Integer() = New Integer(l - 1) {}

            ' initialize array
            For i As Integer = 0 To l - 1
                array(i) = i
            Next i
            shuffle(array)

            Return array
        End Function

        ''' <summary>
        ''' Returns a uniform random permutation of ints 0,...,l-1
        ''' </summary>
        ''' <param name="l">
        '''            length of the array required. </param>
        Public Overridable Function permuted(l As Integer) As Integer()

            Dim array As Integer() = New Integer(l - 1) {}

            ' initialize array
            For i As Integer = 0 To l - 1
                array(i) = i
            Next i
            permute(array)

            Return array
        End Function

        ''' <summary>
        '''****************************************************************
        ''' * Gamma Distribution - Acceptance Rejection combined with *
        ''' Acceptance Complement * *
        ''' ****************************************************************** 
        ''' * FUNCTION: - gds samples a random number from the standard * gamma
        ''' distribution with parameter a > 0. * Acceptance Rejection gs for a &lt;
        ''' 1 , * Acceptance Complement gd for a >= 1 . * REFERENCES: - J.H.
        ''' Ahrens, U. Dieter (1974): Computer methods * for sampling from gamma,
        ''' beta, Poisson and * binomial distributions, Computing 12, 223-246. *
        ''' - J.H. Ahrens, U. Dieter (1982): Generating gamma * variates by a
        ''' modified rejection technique, * Communications of the ACM 25, 47-54.
        ''' * SUBPROGRAMS: - drand(seed) ... (0,1)-Uniform generator with *
        ''' unsigned long integer *seed * - NORMAL(seed) ... Normal generator
        ''' N(0,1). * *
        ''' *****************************************************************
        ''' </summary>
        Public Overridable Function nextGamma(alpha As Double, lambda As Double) As Double
            Dim a As Double = alpha
            Dim aa As Double = -1.0, aaa As Double = -1.0, b As Double = 0.0, c As Double = 0.0, d As Double = 0.0, e As Double, r As Double, s As Double = 0.0, si As Double = 0.0, ss As Double = 0.0, q0 As Double = 0.0, q1 As Double = 0.0416666664, q2 As Double = 0.0208333723, q3 As Double = 0.0079849875, q4 As Double = 0.0015746717, q5 As Double = -0.0003349403, q6 As Double = 0.0003340332, q7 As Double = 0.0006053049, q8 As Double = -0.0004701849, q9 As Double = 0.000171032, a1 As Double = 0.333333333, a2 As Double = -0.249999949, a3 As Double = 0.199999867, a4 As Double = -0.166677482, a5 As Double = 0.142873973, a6 As Double = -0.124385581, a7 As Double = 0.11036831, a8 As Double = -0.112750886, a9 As Double = 0.104089866, e1 As Double = 1.0, e2 As Double = 0.499999994, e3 As Double = 0.166666848, e4 As Double = 0.041664508, e5 As Double = 0.008345522, e6 As Double = 0.001353826, e7 As Double = 0.000247453

            Dim gds, p, q, t, sign_u, u, v, w, x As Double
            Dim v1, v2, v12 As Double

            ' Check for invalid input values

            If a <= 0.0 Then Throw New System.ArgumentException
            If lambda <= 0.0 Then Dim TempIllegalArgumentException As System.ArgumentException = New System.ArgumentException

            If a < 1.0 Then ' CASE A: Acceptance rejection algorithm gs
                b = 1.0 + 0.36788794412 * a ' Step 1
                Do
                    p = b * nextDouble()
                    If p <= 1.0 Then ' Step 2. Case gds <= 1
                        gds = stdNum.Exp(stdNum.Log(p) / a)
                        If stdNum.Log(nextDouble()) <= -gds Then
                            Return (gds / lambda)
                        End If ' Step 3. Case gds > 1
                    Else
                        gds = -stdNum.Log((b - p) / a)
                        If stdNum.Log(nextDouble()) <= ((a - 1.0) * stdNum.Log(gds)) Then Return (gds / lambda)
                    End If
                Loop ' CASE B: Acceptance complement algorithm gd (gaussian
            Else
                ' distribution, box muller transformation)
                If a <> aa Then ' Step 1. Preparations
                    aa = a
                    ss = a - 0.5
                    s = stdNum.Sqrt(ss)
                    d = 5.656854249 - 12.0 * s
                End If
                ' Step 2. Normal deviate
                Do
                    v1 = 2.0 * nextDouble() - 1.0
                    v2 = 2.0 * nextDouble() - 1.0
                    v12 = v1 * v1 + v2 * v2
                Loop While v12 > 1.0
                t = v1 * stdNum.Sqrt(-2.0 * stdNum.Log(v12) / v12)
                x = s + 0.5 * t
                gds = x * x
                If t >= 0.0 Then Return (gds / lambda) ' Immediate acceptance

                u = nextDouble() ' Step 3. Uniform random number
                If d * u <= t * t * t Then Return (gds / lambda) ' Squeeze acceptance

                If a <> aaa Then ' Step 4. Set-up for hat case
                    aaa = a
                    r = 1.0 / a
                    q0 = ((((((((q9 * r + q8) * r + q7) * r + q6) * r + q5) * r + q4) * r + q3) * r + q2) * r + q1) * r
                    If a > 3.686 Then
                        If a > 13.022 Then
                            b = 1.77
                            si = 0.75
                            c = 0.1515 / s
                        Else
                            b = 1.654 + 0.0076 * ss
                            si = 1.68 / s + 0.275
                            c = 0.062 / s + 0.024
                        End If
                    Else
                        b = 0.463 + s - 0.178 * ss
                        si = 1.235
                        c = 0.195 / s - 0.079 + 0.016 * s
                    End If
                End If
                If x > 0.0 Then ' Step 5. Calculation of q
                    v = t / (s + s) ' Step 6.
                    If stdNum.Abs(v) > 0.25 Then
                        q = q0 - s * t + 0.25 * t * t + (ss + ss) * stdNum.Log(1.0 + v)
                    Else
                        q = q0 + 0.5 * t * t * ((((((((a9 * v + a8) * v + a7) * v + a6) * v + a5) * v + a4) * v + a3) * v + a2) * v + a1) * v
                    End If ' Step 7. Quotient acceptance
                    If stdNum.Log(1.0 - u) <= q Then Return (gds / lambda)
                End If

                Do ' Step 8. Double exponential deviate t
                    Do
                        e = -stdNum.Log(nextDouble())
                        u = nextDouble()
                        u = u + u - 1.0
                        sign_u = If(u > 0, 1.0, -1.0)
                        t = b + (e * si) * sign_u
                    Loop While t <= -0.71874483771719 ' Step 9. Rejection of t
                    v = t / (s + s) ' Step 10. New q(t)
                    If stdNum.Abs(v) > 0.25 Then
                        q = q0 - s * t + 0.25 * t * t + (ss + ss) * stdNum.Log(1.0 + v)
                    Else
                        q = q0 + 0.5 * t * t * ((((((((a9 * v + a8) * v + a7) * v + a6) * v + a5) * v + a4) * v + a3) * v + a2) * v + a1) * v
                    End If
                    If q <= 0.0 Then Continue Do ' Step 11.
                    If q > 0.5 Then
                        w = stdNum.Exp(q) - 1.0
                    Else
                        w = ((((((e7 * q + e6) * q + e5) * q + e4) * q + e3) * q + e2) * q + e1) * q
                    End If ' Step 12. Hat acceptance
                    If c * u * sign_u <= w * stdNum.Exp(e - 0.5 * t * t) Then
                        x = s + 0.5 * t
                        Return (x * x / lambda)
                    End If
                Loop
            End If
        End Function
    End Class
End Namespace

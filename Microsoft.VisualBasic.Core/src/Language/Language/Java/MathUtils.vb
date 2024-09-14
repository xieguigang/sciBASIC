#Region "Microsoft.VisualBasic::26683428474d996eab679c79699d72bc, Microsoft.VisualBasic.Core\src\Language\Language\Java\MathUtils.vb"

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

    '   Total Lines: 387
    '    Code Lines: 194 (50.13%)
    ' Comment Lines: 148 (38.24%)
    '    - Xml Docs: 72.30%
    ' 
    '   Blank Lines: 45 (11.63%)
    '     File Size: 14.13 KB


    '     Module MathUtils
    ' 
    '         Properties: Seed
    ' 
    '         Function: getNormalized, (+2 Overloads) getTotal, hypot, nextBoolean, nextByte
    '                   nextChar, nextDouble, nextExponential, nextFloat, nextGamma
    '                   nextGaussian, (+2 Overloads) nextInt, nextInverseGaussian, nextLong, nextShort
    '                   permuted, randomChoice, randomChoicePDF, randomLogDouble, sampleIndicesWithReplacement
    '                   shuffled, uniform
    ' 
    '         Sub: nextBytes, permute, (+2 Overloads) shuffle
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

'
' * MathUtils.java
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
    ''' Handy utility functions which have some Mathematical relavance.
    ''' 
    ''' @author Matthew Goode
    ''' @author Alexei Drummond
    ''' @author Gerton Lunter
    ''' @version $Id: MathUtils.java,v 1.13 2006/08/31 14:57:24 rambaut Exp $
    ''' </summary>
    Public Module MathUtils

        ''' <summary>
        ''' A random number generator that is initialized with the clock when this
        ''' class is loaded into the JVM. Use this for all random numbers. Note: This
        ''' method or getting random numbers in not thread-safe. Since
        ''' MersenneTwisterFast is currently (as of 9/01) not synchronized using this
        ''' function may cause concurrency issues. Use the static get methods of the
        ''' MersenneTwisterFast class for access to a single instance of the class,
        ''' that has synchronization.
        ''' </summary>
        ReadOnly random As MersenneTwisterFast = MersenneTwisterFast.DEFAULT_INSTANCE

        ''' <summary>
        ''' Chooses one category if a cumulative probability distribution is given 
        ''' </summary>
        ''' <param name="cf"></param>
        ''' <returns></returns>
        Public Function randomChoice(cf As Double()) As Integer
            Dim U As Double = random.nextDouble()
            Dim s As Integer

            If U <= cf(0) Then
                s = 0
            Else
                For s = 1 To cf.Length - 1
                    If U <= cf(s) AndAlso U > cf(s - 1) Then
                        Exit For
                    End If
                Next
            End If

            Return s
        End Function

        ''' <param name="pdf">
        '''            array of unnormalized probabilities </param>
        ''' <returns> a sample according to an unnormalized probability distribution </returns>
        Public Function randomChoicePDF(pdf As Double()) As Integer
            Dim U As Double = random.nextDouble() * getTotal(pdf)

            For i As Integer = 0 To pdf.Length - 1
                U -= pdf(i)

                If U < 0.0 Then
                    Return i
                End If
            Next

            For i As Integer = 0 To pdf.Length - 1
                Console.WriteLine(i & vbTab & pdf(i))
            Next

            Throw New Exception("randomChoiceUnnormalized falls through -- negative components in input distribution?")
        End Function

        ''' <param name="array">
        '''            to normalize </param>
        ''' <returns> a new double array where all the values sum to 1. Relative ratios
        '''         are preserved. </returns>
        Public Function getNormalized(array As Double()) As Double()
            Dim newArray As Double() = New Double(array.Length - 1) {}
            Dim ___total As Double = getTotal(array)
            For i As Integer = 0 To array.Length - 1
                newArray(i) = array(i) / ___total
            Next i
            Return newArray
        End Function

        ''' <param name="array">
        '''            entries to be summed </param>
        ''' <param name="start">
        '''            start position </param>
        ''' <param name="end">
        '''            the index of the element after the last one to be included </param>
        ''' <returns> the total of a the values in a range of an array </returns>
        Public Function getTotal(array As Double(), start As Integer, [end] As Integer) As Double
            Dim ___total As Double = 0.0
            For i As Integer = start To [end] - 1
                ___total += array(i)
            Next i
            Return ___total
        End Function

        ''' <param name="array">
        '''            to sum over </param>
        ''' <returns> the total of the values in an array </returns>
        Public Function getTotal(array As Double()) As Double
            Return getTotal(array, 0, array.Length)

        End Function

        ' ===================== (Synchronized) Static access methods to the private
        ' random instance ===========

        ''' <summary>
        ''' Access a default instance of this class, access is synchronized
        ''' </summary>
        Public Property Seed As Long
            Get
                SyncLock random
                    Return random.Seed
                End SyncLock
            End Get
            Set(seed As Long)
                SyncLock random
                    random.Seed = seed
                End SyncLock
            End Set
        End Property

        ''' <summary>
        ''' Access a default instance of this class, access is synchronized
        ''' </summary>
        Public Function nextByte() As SByte
            SyncLock random
                Return random.nextByte()
            End SyncLock
        End Function

        ''' <summary>
        ''' Access a default instance of this class, access is synchronized
        ''' </summary>
        Public Function nextBoolean() As Boolean
            SyncLock random
                Return random.nextBoolean()
            End SyncLock
        End Function

        ''' <summary>
        ''' Access a default instance of this class, access is synchronized
        ''' </summary>
        Public Sub nextBytes(bs As SByte())
            SyncLock random
                random.nextBytes(bs)
            End SyncLock
        End Sub

        ''' <summary>
        ''' Access a default instance of this class, access is synchronized
        ''' </summary>
        Public Function nextChar() As Char
            SyncLock random
                Return random.nextChar()
            End SyncLock
        End Function

        ''' <summary>
        ''' Access a default instance of this class, access is synchronized
        ''' </summary>
        Public Function nextGaussian() As Double
            SyncLock random
                Return random.nextGaussian()
            End SyncLock
        End Function

        ' Mean = alpha / lambda
        ' Variance = alpha / (lambda*lambda)

        Public Function nextGamma(alpha As Double, lambda As Double) As Double
            SyncLock random
                Return random.nextGamma(alpha, lambda)
            End SyncLock
        End Function

        ''' <summary>
        ''' Access a default instance of this class, access is synchronized
        ''' </summary>
        ''' <returns> a pseudo random double precision floating point number in [01) </returns>
        Public Function nextDouble() As Double
            SyncLock random
                Return random.nextDouble()
            End SyncLock
        End Function

        ''' <returns> log of random variable in [0,1] </returns>
        Public Function randomLogDouble() As Double
            Return std.Log(nextDouble())
        End Function

        ''' <summary>
        ''' Access a default instance of this class, access is synchronized
        ''' </summary>
        Public Function nextExponential(lambda As Double) As Double
            SyncLock random
                Return -1.0 * std.Log(1 - random.nextDouble()) / lambda
            End SyncLock
        End Function

        ''' <summary>
        ''' Access a default instance of this class, access is synchronized
        ''' </summary>
        Public Function nextInverseGaussian(mu As Double, lambda As Double) As Double
            SyncLock random
                '			
                '			 * CODE TAKEN FROM WIKIPEDIA. TESTING DONE WITH RESULTS GENERATED IN
                '			 * R AND LOOK COMPARABLE
                '			 
                Dim v As Double = random.nextGaussian() ' sample from a normal
                ' distribution with a mean of 0
                ' and 1 standard deviation
                Dim y As Double = v * v
                Dim x As Double = mu + (mu * mu * y) / (2 * lambda) - (mu / (2 * lambda)) * std.Sqrt(4 * mu * lambda * y + mu * mu * y * y)
                Dim test As Double = MathUtils.nextDouble() ' sample from a uniform
                ' distribution between 0
                ' and 1
                If test <= (mu) / (mu + x) Then
                    Return x
                Else
                    Return (mu * mu) / x
                End If
            End SyncLock
        End Function

        ''' <summary>
        ''' Access a default instance of this class, access is synchronized
        ''' </summary>
        Public Function nextFloat() As Single
            SyncLock random
                Return random.nextFloat()
            End SyncLock
        End Function

        ''' <summary>
        ''' Access a default instance of this class, access is synchronized
        ''' </summary>
        Public Function nextLong() As Long
            SyncLock random
                Return random.nextLong()
            End SyncLock
        End Function

        ''' <summary>
        ''' Access a default instance of this class, access is synchronized
        ''' </summary>
        Public Function nextShort() As Short
            SyncLock random
                Return random.nextShort()
            End SyncLock
        End Function

        ''' <summary>
        ''' Access a default instance of this class, access is synchronized
        ''' </summary>
        Public Function nextInt() As Integer
            SyncLock random
                Return random.nextInt()
            End SyncLock
        End Function

        ''' <summary>
        ''' Access a default instance of this class, access is synchronized
        ''' </summary>
        Public Function nextInt(n As Integer) As Integer
            SyncLock random
                Return random.nextInt(n)
            End SyncLock
        End Function

        ''' 
        ''' <param name="low"> </param>
        ''' <param name="high"> </param>
        ''' <returns> uniform between low and high </returns>
        Public Function uniform(low As Double, high As Double) As Double
            Return low + nextDouble() * (high - low)
        End Function

        ''' <summary>
        ''' Shuffles an array.
        ''' </summary>
        Public Sub shuffle(array As Integer())
            SyncLock random
                random.shuffle(array)
            End SyncLock
        End Sub

        ''' <summary>
        ''' Shuffles an array. Shuffles numberOfShuffles times
        ''' </summary>
        Public Sub shuffle(array As Integer(), numberOfShuffles As Integer)
            SyncLock random
                random.shuffle(array, numberOfShuffles)
            End SyncLock
        End Sub

        ''' <summary>
        ''' Returns an array of shuffled indices of length l.
        ''' </summary>
        ''' <param name="l">
        '''            length of the array required. </param>
        Public Function shuffled(l As Integer) As Integer()
            SyncLock random
                Return random.shuffled(l)
            End SyncLock
        End Function

        Public Function sampleIndicesWithReplacement(length As Integer) As Integer()
            SyncLock random
                Dim result As Integer() = New Integer(length - 1) {}
                For i As Integer = 0 To length - 1
                    result(i) = random.nextInt(length)
                Next i
                Return result
            End SyncLock
        End Function

        ''' <summary>
        ''' Permutes an array.
        ''' </summary>
        Public Sub permute(array As Integer())
            SyncLock random
                random.permute(array)
            End SyncLock
        End Sub

        ''' <summary>
        ''' Returns a uniform random permutation of 0,...,l-1
        ''' </summary>
        ''' <param name="l">
        '''            length of the array required. </param>
        Public Function permuted(l As Integer) As Integer()
            SyncLock random
                Return random.permuted(l)
            End SyncLock
        End Function

        'Public Function logHyperSphereVolume(dimension As Integer, radius As Double) As Double
        '    Return dimension * (0.57236494292470008 + sys.Log(radius)) + -jebl.math.GammaFunction.lnGamma(dimension / 2.0 + 1.0)
        'End Function

        ''' <summary>
        ''' Returns sqrt(a^2 + b^2) without under/overflow.
        ''' </summary>
        Public Function hypot(a As Double, b As Double) As Double
            Dim r As Double

            If std.Abs(a) > std.Abs(b) Then
                r = b / a
                r = std.Abs(a) * std.Sqrt(1 + r * r)
            ElseIf b <> 0 Then
                r = a / b
                r = std.Abs(b) * std.Sqrt(1 + r * r)
            Else
                r = 0.0
            End If

            Return r
        End Function
    End Module
End Namespace

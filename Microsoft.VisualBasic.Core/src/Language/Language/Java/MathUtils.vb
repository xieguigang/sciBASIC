#Region "Microsoft.VisualBasic::1699b021ddc41e1a57f764631ede0312, Microsoft.VisualBasic.Core\src\Language\Language\Java\MathUtils.vb"

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

    '   Total Lines: 291
    '    Code Lines: 141 (48.45%)
    ' Comment Lines: 115 (39.52%)
    '    - Xml Docs: 66.09%
    ' 
    '   Blank Lines: 35 (12.03%)
    '     File Size: 10.80 KB


    '     Module MathUtils
    ' 
    '         Properties: Seed
    ' 
    '         Function: getNormalized, (+2 Overloads) getTotal, hypot, nextBoolean, nextByte
    '                   nextExponential, nextGaussian, nextInverseGaussian, permuted, randomChoice
    '                   randomChoicePDF, randomLogDouble, sampleIndicesWithReplacement, shuffled, uniform
    ' 
    '         Sub: nextBytes, permute, (+2 Overloads) shuffle
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports Microsoft.VisualBasic.Linq

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
        ''' Chooses one category if a cumulative probability distribution is given 
        ''' </summary>
        ''' <param name="cf"></param>
        ''' <returns></returns>
        Public Function randomChoice(cf As Double()) As Integer
            Dim random As Random = randf.seeds
            Dim U As Double = random.NextDouble()
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
            Dim random As Random = randf.seeds
            Dim U As Double = random.NextDouble() * getTotal(pdf)

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
        Public Property Seed As Integer
            Get
                Return randf.Seed
            End Get
            Set
                randf.SetSeed(Seed)
            End Set
        End Property

        ''' <summary>
        ''' Access a default instance of this class, access is synchronized
        ''' </summary>
        Public Function nextByte() As SByte
            Return CSByte(randf.NextInteger(-128, 127))
        End Function

        ''' <summary>
        ''' Access a default instance of this class, access is synchronized
        ''' </summary>
        Public Function nextBoolean() As Boolean
            Return randf.NextBoolean
        End Function

        ''' <summary>
        ''' Access a default instance of this class, access is synchronized
        ''' </summary>
        Public Sub nextBytes(bs As SByte())
            For i As Integer = 0 To bs.Length - 1
                bs(i) = CSByte(randf.NextInteger(-128, 127))
            Next
        End Sub

        ''' <summary>
        ''' Access a default instance of this class, access is synchronized
        ''' </summary>
        Public Function nextGaussian() As Double
            Return randf.NextGaussian
        End Function

        ''' <returns> log of random variable in [0,1] </returns>
        Public Function randomLogDouble() As Double
            Return std.Log(randf.NextDouble())
        End Function

        ''' <summary>
        ''' Access a default instance of this class, access is synchronized
        ''' </summary>
        Public Function nextExponential(lambda As Double) As Double
            Return -1.0 * std.Log(1 - randf.NextDouble()) / lambda
        End Function

        ''' <summary>
        ''' Access a default instance of this class, access is synchronized
        ''' </summary>
        Public Function nextInverseGaussian(mu As Double, lambda As Double) As Double
            '			
            '			 * CODE TAKEN FROM WIKIPEDIA. TESTING DONE WITH RESULTS GENERATED IN
            '			 * R AND LOOK COMPARABLE
            '			 
            Dim v As Double = randf.NextGaussian() ' sample from a normal
            ' distribution with a mean of 0
            ' and 1 standard deviation
            Dim y As Double = v * v
            Dim x As Double = mu + (mu * mu * y) / (2 * lambda) - (mu / (2 * lambda)) * std.Sqrt(4 * mu * lambda * y + mu * mu * y * y)
            Dim test As Double = randf.NextDouble() ' sample from a uniform
            ' distribution between 0
            ' and 1
            If test <= (mu) / (mu + x) Then
                Return x
            Else
                Return (mu * mu) / x
            End If
        End Function

        ''' 
        ''' <param name="low"> </param>
        ''' <param name="high"> </param>
        ''' <returns> uniform between low and high </returns>
        Public Function uniform(low As Double, high As Double) As Double
            Return low + randf.NextDouble() * (high - low)
        End Function

        ''' <summary>
        ''' Shuffles an array.
        ''' </summary>
        Public Sub shuffle(array As Integer())
            randf.Shuffle(array)
        End Sub

        ''' <summary>
        ''' Shuffles an array. Shuffles numberOfShuffles times
        ''' </summary>
        Public Sub shuffle(array As Integer(), numberOfShuffles As Integer)
            randf.Shuffle(randf.seeds, array, numberOfShuffles)
        End Sub

        ''' <summary>
        ''' Returns an array of shuffled indices of length l.
        ''' </summary>
        ''' <param name="l">
        '''            length of the array required. </param>
        Public Function shuffled(l As Integer) As Integer()
            Dim index As Integer() = l.Sequence
            randf.Shuffle(index)
            Return index
        End Function

        Public Function sampleIndicesWithReplacement(length As Integer) As Integer()
            Dim result As Integer() = New Integer(length - 1) {}
            For i As Integer = 0 To length - 1
                result(i) = randf.NextInteger(length)
            Next i
            Return result
        End Function

        ''' <summary>
        ''' Permutes an array.
        ''' </summary>
        Public Sub permute(array As Integer())
            Dim l As Integer = array.Length
            For i As Integer = 0 To l - 1
                Dim index As Integer = randf.NextInteger(l - i) + i
                Dim temp As Integer = array(index)
                array(index) = array(i)
                array(i) = temp
            Next i
        End Sub

        ''' <summary>
        ''' Returns a uniform random permutation of 0,...,l-1
        ''' </summary>
        ''' <param name="l">
        '''            length of the array required. </param>
        Public Function permuted(l As Integer) As Integer()
            Dim index As Integer() = l.Sequence
            permute(index)
            Return index
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

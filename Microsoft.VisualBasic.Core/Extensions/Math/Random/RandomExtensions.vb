#Region "Microsoft.VisualBasic::4917fb0e089411880acc24cc5d2ed883, Microsoft.VisualBasic.Core\Extensions\Math\Random\RandomExtensions.vb"

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

    '     Delegate Function
    ' 
    ' 
    '     Delegate Function
    ' 
    ' 
    '     Module RandomExtensions
    ' 
    '         Function: (+2 Overloads) GetRandomValue, NextBoolean, (+2 Overloads) NextDouble, NextGaussian, NextTriangular
    '                   Permutation, randf, RandomSingle, Seed
    ' 
    '         Sub: (+2 Overloads) Shuffle
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports sys = System.Math

Namespace Math

    ''' <summary>
    ''' Generates a random number.
    ''' (事实上这个函数指针的功能仅仅是返回一个实数，所以这里不仅仅是局限于随机数，也可以是一个固定值的实数)
    ''' </summary>
    ''' <returns></returns>
    Public Delegate Function IValueProvider() As Double

    ''' <summary>
    ''' Tells the function how to generates a new random seed?
    ''' </summary>
    ''' <returns></returns>
    Public Delegate Function IRandomSeeds() As Random

    ''' <summary>
    ''' Some extension methods for <see cref="System.Random"/> for creating a few more kinds of random stuff.
    ''' </summary>
    ''' <remarks>Imports from https://github.com/rvs76/superbest-random.git </remarks>
    ''' 
    <Package("Random", Publisher:="rvs76", Description:="Some extension methods for Random for creating a few more kinds of random stuff.")>
    Public Module RandomExtensions

        ''' <summary>
        ''' A number used to calculate a starting value for the pseudo-random number sequence.
        ''' If a negative number is specified, the absolute value of the number is used.
        ''' </summary>
        ''' <returns></returns>
        Public Function Seed() As Integer
            Dim seeds& = CLng(Integer.MaxValue) * 2
            Randomize()
            seeds = (Rnd() * SecurityString.ToLong(SecurityString.GetMd5Hash(Now.ToString))) / seeds
            Return CInt(seeds)
        End Function

        Const RandfMultiply# = 10000

        ''' <summary>
        ''' 返回<paramref name="min"/>到<paramref name="max"/>区间之内的一个和实数
        ''' </summary>
        ''' <param name="min"></param>
        ''' <param name="max"></param>
        ''' <returns></returns>
        Public Function randf(min As Double, max As Double) As Double
            Dim minInteger& = CLng(sys.Truncate(min * RandfMultiply))
            Dim maxInteger& = CLng(sys.Truncate(max * RandfMultiply))
            Dim randInteger& = CLng(RandomNumbers.rand()) * CLng(RandomNumbers.rand())
            Dim diffInteger& = maxInteger - minInteger
            Dim resultInteger& = randInteger Mod diffInteger + minInteger

            Return resultInteger / RandfMultiply
        End Function

        ReadOnly seeds As New Random(Rnd() * 10000)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function RandomSingle() As Single
            Return seeds.NextDouble()
        End Function

        <Extension>
        Public Function GetRandomValue(rng As DoubleRange) As Double
            SyncLock seeds
                Return seeds.NextDouble(range:=rng)
            End SyncLock
        End Function

        ''' <summary>
        ''' Returns a random floating-point number that is greater than or equal to min of the range,
        ''' and less than the max of the range.
        ''' </summary>
        ''' <param name="rnd"></param>
        ''' <param name="range"></param>
        ''' <returns></returns>
        <Extension>
        Public Function NextDouble(rnd As Random, range As DoubleRange) As Double
            Return range.Length * rnd.NextDouble + range.Min
        End Function

        <Extension>
        Public Function NextDouble(rand As Random, min#, max#) As Double
            Return (max - min) * rand.NextDouble + min
        End Function

        <Extension>
        Public Function GetRandomValue(rng As IntRange) As Integer
            Return rng.Length * seeds.NextDouble + rng.Min
        End Function

        ''' <summary>
        ''' Generates normally distributed numbers. Each operation makes two Gaussians for the price of one, and apparently they can be cached or something for better performance, but who cares.
        ''' </summary>
        ''' <param name="r"></param>
        ''' <param name = "mu">Mean of the distribution</param>
        ''' <param name = "sigma">Standard deviation</param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("NextGaussian")>
        <Extension> Public Function NextGaussian(r As Random, Optional mu As Double = 0, Optional sigma As Double = 1) As Double
            Dim u1 As Double = r.NextDouble()
            Dim u2 As Double = r.NextDouble()

            Dim rand_std_normal = sys.Sqrt(-2.0 * sys.Log(u1)) * sys.Sin(2.0 * sys.PI * u2)
            Dim rand_normal = mu + sigma * rand_std_normal

            Return rand_normal
        End Function

        ''' <summary>
        ''' Generates values from a triangular distribution.
        ''' </summary>
        ''' <remarks>
        ''' See http://en.wikipedia.org/wiki/Triangular_distribution for a description of the triangular probability distribution and the algorithm for generating one.
        ''' </remarks>
        ''' <param name="r"></param>
        ''' <param name = "a">Minimum</param>
        ''' <param name = "b">Maximum</param>
        ''' <param name = "c">Mode (most frequent value)</param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("NextTriangular")>
        <Extension> Public Function NextTriangular(r As Random, a As Double, b As Double, c As Double) As Double
            Dim u As Double = r.NextDouble()
            Return If(u < (c - a) / (b - a), a + sys.Sqrt(u * (b - a) * (c - a)), b - sys.Sqrt((1 - u) * (b - a) * (b - c)))
        End Function

        ''' <summary>
        ''' Equally likely to return true or false. Uses <see cref="Random.Next(Integer)"/>.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' ```vbnet
        ''' 1 > 0 OR 0 > 0
        ''' ```
        ''' </remarks>
        <ExportAPI("NextBoolean")>
        <Extension> Public Function NextBoolean(r As Random) As Boolean
            Return r.[Next](2) > 0 ' 1 > 0 OR 0 > 0
        End Function

        ''' <summary>
        ''' Shuffles a list in O(n) time by using the Fisher-Yates/Knuth algorithm.
        ''' </summary>
        ''' <param name="r"></param>
        ''' <param name = "list"></param>
        <Extension> Public Sub Shuffle(Of T)(r As Random, ByRef list As List(Of T))
            For i As Integer = 0 To list.Count - 1
                Dim j As Integer = r.[Next](0, i + 1)
                Dim temp As T = list(j)
                list(j) = list(i)
                list(i) = temp
            Next
        End Sub

        ''' <summary>
        ''' Shuffles a list in O(n) time by using the Fisher-Yates/Knuth algorithm.
        ''' </summary>
        ''' <param name="r"></param>
        ''' <param name = "list"></param>
        ''' 
        <ExportAPI("Shuffle")>
        Public Sub Shuffle(r As Random, ByRef list As IList)
            For i As Integer = 0 To list.Count - 1
                Dim j As Integer = r.[Next](0, i + 1)
                Dim temp = list(j)
                list(j) = list(i)
                list(i) = temp
            Next
        End Sub

        ''' <summary>
        ''' Returns n unique random numbers in the range [1, n], inclusive. 
        ''' This is equivalent to getting the first n numbers of some random permutation of the sequential numbers from 1 to max. 
        ''' Runs in O(k^2) time.
        ''' </summary>
        ''' <param name="rand"></param>
        ''' <param name="n">Maximum number possible.(最大值)</param>
        ''' <param name="k">How many numbers to return.(返回的数据的数目)</param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("Permutation")>
        <Extension> Public Function Permutation(rand As Random, n As Integer, k As Integer) As Integer()
            Dim result As New List(Of Integer)()
            Dim sorted As New SortedSet(Of Integer)()

            For i As Integer = 0 To k - 1
                Dim r = rand.[Next](1, n + 1 - i)

                For Each q As Integer In sorted
                    If r >= q Then
                        r += 1
                    End If
                Next

                result.Add(r)
                sorted.Add(r)
            Next

            Return result.ToArray()
        End Function
    End Module
End Namespace

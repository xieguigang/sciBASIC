#Region "Microsoft.VisualBasic::76ce3b873414ecdca94aa5466611cf81, ..\Microsoft.VisualBasic.Architecture.Framework\Extensions\Math\RandomExtensions.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Imports System.Collections
Imports System.Collections.Generic
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

''' <summary>
''' Some extension methods for <see cref="Random"/> for creating a few more kinds of random stuff.
''' </summary>
''' <remarks>Imports from https://github.com/rvs76/superbest-random.git </remarks>
''' 
<PackageNamespace("Random", Publisher:="rvs76", Description:="Some extension methods for Random for creating a few more kinds of random stuff.")>
Public Module RandomExtensions

    ReadOnly __randomSeeds As New Random(RandomDouble() * 10000)

    Public Function RandomSingle() As Single
        Dim result = __randomSeeds.NextDouble()
        Return CSng(result)
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

        Dim rand_std_normal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2)
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
        Return If(u < (c - a) / (b - a), a + Math.Sqrt(u * (b - a) * (c - a)), b - Math.Sqrt((1 - u) * (b - a) * (b - c)))
    End Function

    ''' <summary>
    ''' Equally likely to return true or false. Uses <see cref="Random.Next()"/>.
    ''' </summary>
    ''' <returns></returns>
    ''' 
    <ExportAPI("NextBoolean")>
    <Extension> Public Function NextBoolean(r As Random) As Boolean
        Return r.[Next](2) > 0
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
    ''' <param name="n">Maximum number possible.</param>
    ''' <param name="k">How many numbers to return.</param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("Permutation")>
    <Extension> Public Function Permutation(rand As Random, n As Integer, k As Integer) As Integer()
        Dim result = New List(Of Integer)()
        Dim sorted = New SortedSet(Of Integer)()

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

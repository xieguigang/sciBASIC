#Region "Microsoft.VisualBasic::c6c72f312d8a957be241defab0dff01e, Data_science\Mathematica\Math\Randomizer\Randomizer.vb"

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

    '   Total Lines: 228
    '    Code Lines: 117
    ' Comment Lines: 74
    '   Blank Lines: 37
    '     File Size: 8.92 KB


    ' Class Randomizer
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: (+3 Overloads) [Next], getRandom, GetRandomInts, GetRandomNormalDeviates, GetRandomPercentages
    '               getRandoms, NextDouble, Sample
    ' 
    '     Sub: NextBytes
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' ##### Random generator based on the random table.(请注意，这个模块之中的所有函数都是线程不安全的)
''' 
''' ###### A Million Random Digits with 100,000 Normal Deviates
''' 
''' Not long after research began at RAND in 1946, the need arose for random numbers that 
''' could be used to solve problems of various kinds of experimental probability procedures. 
''' These applications, called Monte Carlo methods, required a large supply of random 
''' digits and normal deviates of high quality, and the tables presented here were produced 
''' to meet those requirements. This book was a product of RAND's pioneering work in computing, 
''' as well a testament to the patience and persistence of researchers in the early days of 
''' RAND. The tables of random numbers in this book have become a standard reference in 
''' engineering and econometrics textbooks and have been widely used in gaming and simulations 
''' that employ Monte Carlo trials. Still the largest published source of random digits and 
''' normal deviates, the work is routinely used by statisticians, physicists, polltakers, 
''' market analysts, lottery administrators, and quality control engineers. A 2001 article 
''' in the New York Times on the value of randomness featured the original edition of the book, 
''' published in 1955 by the Free Press. The rights have since reverted to RAND, and in this 
''' digital age, we thought it appropriate to reissue a new edition of the book in its original 
''' format, with a new foreword by Michael D. Rich, RAND's Executive Vice President.
''' 
''' > http://www.rand.org/pubs/monograph_reports/MR1418.html
''' </summary>
Public Class Randomizer : Inherits Random

    Shared ReadOnly deviates#()()
    Shared ReadOnly digits%()()

    Shared Sub New()
        Dim lines$() = My.Resources.deviates.LineTokens

        deviates# = LinqAPI.Exec(Of Double()) <=
            From line As String
            In lines.AsParallel  ' 并行化可能会进一步加深随机
            Let tokens As String() = Regex.Replace(Mid(line, 5).Trim, "\s{2,}", " ").Split
            Let n As Double() =
                tokens _
                .Select(Function(s) If(s.Last = "-"c, -Val(s), Val(s))) _
                .ToArray
            Select n

        lines$ = My.Resources.digits.LineTokens

        digits% = LinqAPI.Exec(Of Integer()) <=
            From line As String
            In lines.AsParallel  ' 并行化可能会进一步加深随机
            Let tokens As String() = Regex.Replace(Mid(line, 6).Trim, "\s{2,}", " ").Split
            Let n As Integer() =
                tokens _
                .Select(Function(s) CInt(Val(s))) _
                .ToArray
            Select n

        max = digits.IteratesALL.Max
        min = digits.IteratesALL.Min
        len = max - min
    End Sub

    ReadOnly _deviates As LoopArray(Of Double())
    ReadOnly _digits As LoopArray(Of Integer())
    ReadOnly rand As New Random(Now.Millisecond)

    ''' <summary>
    ''' <see cref="_digits"/> max integer
    ''' </summary>
    Shared ReadOnly max%, min%, len%

    Sub New()
        Dim rand As New Random

        SyncLock deviates
            Dim list As New List(Of Double())(deviates)

            rand.Shuffle(list)
            _deviates = New LoopArray(Of Double())(list)
        End SyncLock

        SyncLock digits
            Dim list As New List(Of Integer())(digits)

            rand.Shuffle(list)
            _digits = New LoopArray(Of Integer())(list)
        End SyncLock
    End Sub

    ''' <summary>
    ''' 每一行有10个随机数
    ''' </summary>
    Const DigitsRowLength% = 10

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetRandomInts(n As Integer) As Integer()
        Return getRandoms(n, _digits)
    End Function

    ''' <summary>
    ''' Returns a non-negative random integer.
    ''' </summary>
    ''' <returns>A 32-bit signed integer that is greater than or equal to 0 and less than System.Int32.MaxValue.</returns>
    Public Overrides Function [Next]() As Integer
        Return getRandom(_digits)
    End Function

    Private Function getRandom(Of T)(array As LoopArray(Of T())) As T
        Dim d As Integer = rand.NextBoolean
        Dim out As New List(Of T)
        Dim maxRange As Integer = (rand.NextDouble * 100) + 1

        Call array.Set(rand.Next(array.Length))

        Dim delta% = rand.Next(maxRange)
        Dim c% = rand.Next(DigitsRowLength)

        Return array.GET(delta)(c)
    End Function

    Private Function getRandoms(Of T)(n%, array As LoopArray(Of T())) As T()
        Dim rand As New Random(n * Now.Millisecond)
        Dim d As Integer = rand.NextBoolean
        Dim out As New List(Of T)
        Dim maxRange As Integer = n * rand.NextDouble + n

        Call array.Set(rand.Next(array.Length))

        For i As Integer = 0 To n - 1
            Dim delta% = rand.Next(maxRange)
            Dim c% = rand.Next(DigitsRowLength)

            Call out.Add(array.GET(delta)(c))
        Next

        Return out.ToArray
    End Function

    ''' <summary>
    ''' 返回随机的0-1之间的百分比数值
    ''' </summary>
    ''' <param name="n"></param>
    ''' <returns></returns>
    Public Function GetRandomPercentages(n As Integer) As Double()
        Dim ints%() = GetRandomInts(n)
        Dim ps#() = ints _
            .Select(Function(x) (x - min) / len) _
            .ToArray
        Return ps
    End Function

    ''' <summary>
    ''' Returns a random floating-point number that is greater than or equal to 0.0,
    ''' and less than 1.0.
    ''' </summary>
    ''' <returns>A double-precision floating point number that is greater than or equal to 0.0,
    ''' and less than 1.0.</returns>
    Public Overrides Function NextDouble() As Double
        Return (getRandom(_digits) - min) / len
    End Function

    ''' <summary>
    ''' Fills the elements of a specified array of bytes with random numbers.
    ''' </summary>
    ''' <param name="buffer">An array of bytes to contain random numbers.</param>
    Public Overrides Sub NextBytes(buffer() As Byte)
        Dim ints%() = GetRandomInts(buffer.Length)
        Dim lvs = ints.GenerateMapping(255, 0)

        For i As Integer = 0 To buffer.Length - 1
            buffer(i) = CByte(lvs(i))
        Next
    End Sub

    ''' <summary>
    ''' Returns a non-negative random integer that is less than the specified maximum.
    ''' </summary>
    ''' <param name="maxValue">The exclusive upper bound of the random number to be generated. maxValue must
    ''' be greater than or equal to 0.</param>
    ''' <returns>A 32-bit signed integer that is greater than or equal to 0, and less than maxValue;
    ''' that is, the range of return values ordinarily includes 0 but not maxValue. However,
    ''' if maxValue equals 0, maxValue is returned.</returns>
    Public Overrides Function [Next](maxValue As Integer) As Integer
        Return Next%(Scan0, maxValue)
    End Function

    ''' <summary>
    ''' Returns a random integer that is within a specified range.
    ''' </summary>
    ''' <param name="minValue">The inclusive lower bound of the random number returned.</param>
    ''' <param name="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater
    ''' than or equal to minValue.</param>
    ''' <returns>A 32-bit signed integer greater than or equal to minValue and less than maxValue;
    ''' that is, the range of return values includes minValue but not maxValue. If minValue
    ''' equals maxValue, minValue is returned.</returns>
    Public Overrides Function [Next](minValue As Integer, maxValue As Integer) As Integer
        Dim x As Integer = Integer.MaxValue

        If minValue = maxValue Then
            Return minValue
        End If

        Do While x < minValue OrElse maxValue <= x
            x = minValue + NextDouble() * (maxValue - minValue)
        Loop

        Return x
    End Function

    ''' <summary>
    ''' 返回一组符合标准正态分布的实数
    ''' </summary>
    ''' <param name="n"></param>
    ''' <returns></returns>
    Public Function GetRandomNormalDeviates(n As Integer) As Double()
        Return getRandoms(n, _deviates)
    End Function

    ''' <summary>
    ''' Normal deviates
    ''' </summary>
    ''' <returns></returns>
    Public Overloads Function Sample() As Double
        Return getRandom(_deviates)
    End Function
End Class

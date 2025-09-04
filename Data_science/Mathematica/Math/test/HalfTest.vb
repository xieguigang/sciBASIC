#Region "Microsoft.VisualBasic::488dfb38965d6d598471aea5b47118d4, sciBASIC#\Data_science\Mathematica\Math\test\HalfTest.vb"

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

    '   Total Lines: 82
    '    Code Lines: 51
    ' Comment Lines: 1
    '   Blank Lines: 30
    '     File Size: 2.41 KB


    ' Module HalfTest
    ' 
    '     Sub: Main, MathTest, memoryTest
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Numerics

Module HalfTest

    Sub Main()

        Call MathTest()
        Call memoryTest()

        Dim one As Half = 1.01!

        Console.WriteLine(one)

        Dim p100 As Half() = Replicate(CType(-100.31!, Half), 5).ToArray

        ' Console.WriteLine(p100)


        Dim dbl As Double() = Replicate(9999.009999, 5).ToArray

        Console.WriteLine(Half.MaxValue)
        Console.WriteLine(Half.MinValue)

        Console.WriteLine(HeapSizeOf.MeasureSize(one))
        Console.WriteLine(HeapSizeOf.MeasureSize(p100))
        Console.WriteLine(HeapSizeOf.MeasureSize(dbl))


        Pause()
    End Sub

    Sub MathTest()
        Dim raw As Double() = 3.0.Replicate(10).AsList + 0.000000000001.Replicate(5000000)
        Dim t1, t2, t3 As Long

        Dim vec As New Vector(raw)
        Dim half As New HalfVector(raw)
        Dim start = App.ElapsedMilliseconds

        Dim result = (Math.E ^ (-1 * (vec + 2))).Sum
        t1 = App.ElapsedMilliseconds - start

        start = App.ElapsedMilliseconds
        Dim resultHalf = (Math.E ^ (-1 * (half + 2))).Sum
        t2 = App.ElapsedMilliseconds - start


        Call Console.WriteLine($"result1={result}, {t1}ms")
        Call Console.WriteLine($"result2={resultHalf}, {t2}ms")
    End Sub

    Sub memoryTest()

        Dim raw As Double() = 200.0.Replicate(10).AsList + 0.000000000001.Replicate(5000000)
        Dim start = App.ElapsedMilliseconds
        Dim t1, t2, t3 As Long

        start = App.ElapsedMilliseconds
        Dim vector As New Vector(raw)

        t1 = App.ElapsedMilliseconds - start

        start = App.ElapsedMilliseconds
        Dim sparse As New SparseVector(raw)

        t2 = App.ElapsedMilliseconds - start

        start = App.ElapsedMilliseconds
        Dim halfs As New HalfVector(raw)

        t3 = App.ElapsedMilliseconds - start


        Call $"Takes {t1}ms to allocate {HeapSizeOf.MeasureSize(vector) / 1024 / 1024} MB".debug
        Call $"Takes {t2}ms to allocate {HeapSizeOf.MeasureSize(sparse) / 1024 / 1024} MB".debug
        Call $"Takes {t3}ms to allocate {HeapSizeOf.MeasureSize(halfs) / 1024 / 1024} MB".debug

        Pause()
    End Sub
End Module

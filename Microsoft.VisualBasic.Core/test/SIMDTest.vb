#Region "Microsoft.VisualBasic::2728d12e11a359df103243159fd911c5, sciBASIC#\Microsoft.VisualBasic.Core\test\SIMDTest.vb"

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

    '   Total Lines: 56
    '    Code Lines: 42
    ' Comment Lines: 0
    '   Blank Lines: 14
    '     File Size: 1.81 KB


    ' Module SIMDTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Module SIMDTest

    Sub Main()
        Dim nsize = 90000001
        Dim a As Double() = nsize.Sequence.Select(Function(i) randf.NextDouble).ToArray
        Dim b As Double() = nsize.Sequence.Select(Function(i) randf.NextDouble).ToArray
        Dim samplesLegacy As New List(Of Double)
        Dim samplesAvx As New List(Of Double)

        For i As Integer = 0 To 100
            SIMD.config = SIMDConfiguration.enable

            Dim t1 = App.NanoTime
            Dim c = SIMD.Add(a, b)
            Dim t2 = App.NanoTime

            SIMD.config = SIMDConfiguration.disable

            Dim t3 = App.NanoTime
            Dim d = SIMD.Add(a, b)
            Dim t4 = App.NanoTime

            SIMD.config = SIMDConfiguration.legacy

            Dim t5 = App.NanoTime
            Dim e = SIMD.Add(a, b)
            Dim t6 = App.NanoTime

            Call Console.WriteLine(c.SequenceEqual(d))
            Call Console.WriteLine(c.SequenceEqual(e))
            Call Console.WriteLine(t2 - t1)
            Call Console.WriteLine(t4 - t3)
            Call Console.WriteLine(t6 - t5)
            Call Console.WriteLine((t2 - t1) / (t4 - t3))
            Call Console.WriteLine((t6 - t5) / (t4 - t3))

            Call samplesAvx.Add((t2 - t1) / (t4 - t3))
            Call samplesLegacy.Add((t6 - t5) / (t4 - t3))

            Call Console.WriteLine("--------------")
        Next

        Call Console.WriteLine()
        Call Console.WriteLine()
        Call Console.WriteLine()

        Call Console.WriteLine(samplesAvx.Average)
        Call Console.WriteLine(samplesLegacy.Average)

        Pause()
    End Sub
End Module


#Region "Microsoft.VisualBasic::ce7276345c4d8aeb56ca0a82a26a8461, Data_science\Mathematica\SignalProcessing\SignalProcessing\Source\DumpsSignal.vb"

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

    '   Total Lines: 41
    '    Code Lines: 22
    ' Comment Lines: 12
    '   Blank Lines: 7
    '     File Size: 1.24 KB


    '     Module DumpsSignal
    ' 
    '         Function: bumps
    ' 
    '         Sub: bump
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports randf2 = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Namespace Source

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' Inspired by Lee Byron's test data generator.
    ''' </remarks>
    Public Module DumpsSignal

        ''' <summary>
        ''' Generate signal data for test
        ''' </summary>
        ''' <param name="length">信号的长度，点的数量</param>
        ''' <param name="m">信号的叠加次数</param>
        ''' <returns></returns>
        Public Function bumps(length%, Optional m% = 25) As Double()
            Dim a#() = New Double(length - 1) {}

            For i As Integer = 0 To m - 1
                Call bump(a, length, randf2.seeds)
            Next

            Return a
        End Function

        Private Sub bump(ByRef a#(), length%, seed As Random)
            Dim x = 1 / (0.1 + seed.NextDouble),
                y = 2 * seed.NextDouble - 0.5,
                Z = 10 / (0.1 + seed.NextDouble)

            For i As Integer = 0 To length - 1
                Dim w = (i / length - y) * Z
                a(i) += x * std.Exp(-w * w)
            Next
        End Sub
    End Module
End Namespace

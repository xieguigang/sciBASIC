#Region "Microsoft.VisualBasic::8a3a10235ab2be4167db9a26225baafc, gr\physics\test\Program.vb"

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

    '   Total Lines: 73
    '    Code Lines: 8 (10.96%)
    ' Comment Lines: 39 (53.42%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 26 (35.62%)
    '     File Size: 2.20 KB


    ' Module Program
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Math
Imports Microsoft.VisualBasic.Imaging.Physics
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Module Program



    Sub Main()

        'Call cl({210, 42}, {539, 72}) ' 265???
        'Call cl({0, 0}, {100, -5})
        'Call cl({100, -5}, {0, 0})

        'Call cl({100, 0}, {-100, 0})  ' X 轴， 0度
        'Call cl({0, 100}, {0, -100})   ' Y 轴，  90度
        'Call cl({100, 100}, {0, 0})  ' 45度

        'Call cl({-100, 100}, {0, 0}) '135
        'Call cl({-100, -100}, {0, 0}) '180+45

        'Call cl({100, -100}, {0, 0}) '360-45

        'Call cl({100, 100}, {-100, -100})
        'Call cl({-100, -100}, {100, 100})

        'Call g(100)

        'Call repl({0, 10}, {0, -10})   ' Y 轴，  90度
        'Call repl({100, 0}, {-100, 0})  ' X 轴  0度

        'Call reverse(New Force(100, 0))
        'Call reverse(New Force(100, PI / 3))


        'Call add(New Force(1000, 1 / 2 * PI / 2), New Force(100, PI + 1 / 2 * PI / 2))  ' 45  225  -> 45

        'Call add(New Force(100, 0), New Force(100, 1 / 2 * PI))  ' 45

        'Call add(New Force(100, 0), New Force(100, PI)) ' 0
        'Call add(New Force(100, 0), New Force(100, PI * 2)) ' 0

        'Call add(New Force(100, 0), New Force(100, PI + 1 / 2 * PI)) ' 270+45

        Pause()

    End Sub


    'Sub cl(a As Vector, b As Vector)
    '    Dim m1 As New MassPoint With {.Charge = 1, .Point = a}
    '    Dim m2 As New MassPoint With {.Charge = 1, .Point = b}
    '    Dim f = ForceMath.CoulombsLaw(m1, m2)

    '    Call $"Coulombs force between the {m1.Point.ToString} and {m2.Point.ToString} is {f.ToString}".debug
    'End Sub

    'Sub g(m#)
    '    Call $"gravity is {ForceMath.Gravity(New MassPoint With {.Mass = m}).ToString }".debug
    'End Sub

    'Sub repl(a As Vector, b As Vector)
    '    Call ForceMath.RepulsiveForce(100, a, b).debug
    'End Sub

    'Sub reverse(f As Force)
    '    Call $"ReverseOf({f.ToString}) is {-f}".debug
    'End Sub

    'Sub add(f1 As Force, f2 As Force)
    '    Call $"||{f1.ToString}|| + ||{f2.ToString}||  =>  ||{(f1 + f2).ToString}||".debug
    'End Sub
End Module

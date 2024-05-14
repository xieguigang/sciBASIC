#Region "Microsoft.VisualBasic::3a9a243c9ab5f920be8b3c46bcb91215, Data_science\Mathematica\Math\ODE\test\Module1.vb"

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

    '   Total Lines: 61
    '    Code Lines: 46
    ' Comment Lines: 0
    '   Blank Lines: 15
    '     File Size: 1.57 KB


    ' Module Module1
    ' 
    '     Sub: Main
    '     Class TestRefSin
    ' 
    '         Function: y0
    ' 
    '         Sub: func
    ' 
    '     Class TestSin
    ' 
    '         Function: y0
    ' 
    '         Sub: func
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics.Data
Imports stdNum = System.Math

Module Module1

    Sub Main()

        Dim s As New TestSin
        Dim result = s.Solve(1000, 0, 10)

        Call result.DataFrame.Save("x:\1.csv", Encodings.ASCII)

        Dim s2 As New TestRefSin With {
            .RefValues = New ValueVector With {
                .Y = result.y
            }
        }
        Dim result2 = s2.Solve(1000, 0, 10)

        Call result2.DataFrame.Save("x:\2.csv", Encodings.ASCII)

        Pause()
    End Sub

    Public Class TestRefSin : Inherits RefODEs

        Dim T As var

        Protected Overrides Sub func(dx As Double, ByRef dy As Vector, Y As ValueVector)
            dy(T) = stdNum.Sin(dx) + Y("a")
        End Sub

        Protected Overrides Function y0() As var()
            Return {
                T = 33
            }
        End Function
    End Class

    Public Class TestSin : Inherits ODEs

        Dim T As var
        Dim a As var

        Protected Overrides Sub func(dx As Double, ByRef dy As Vector)
            dy(T) = stdNum.Sin(dx) + a
            dy(a) = stdNum.Cos(dx)
        End Sub

        Protected Overrides Function y0() As var()
            Return {
                T = 33,
                a = -1
            }
        End Function
    End Class
End Module

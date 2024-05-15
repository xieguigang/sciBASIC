#Region "Microsoft.VisualBasic::0d1f0532dd0f53ee3c9099991f87a726, Data_science\Mathematica\Math\ODE\Dynamics\RefODEs.vb"

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

    '   Total Lines: 33
    '    Code Lines: 17
    ' Comment Lines: 8
    '   Blank Lines: 8
    '     File Size: 1.05 KB


    '     Class RefODEs
    ' 
    '         Properties: RefValues
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Calculus.Dynamics.Data
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Dynamics

    ''' <summary>
    ''' ``dy`` reference to the exists values.
    ''' </summary>
    Public MustInherit Class RefODEs : Inherits ODEs

        Public Property RefValues As ValueVector

        Dim RK%

        ''' <summary>
        ''' RK4每一次迭代会调用这个函数计算4次
        ''' </summary>
        ''' <param name="dx"></param>
        ''' <param name="dy"></param>
        Protected NotOverridable Overrides Sub func(dx As Double, ByRef dy As Vector)
            RK += 1
            func(dx, dy, RefValues)

            If RK = 4 Then
                RK = 0
                RefValues += 1  ' 最开始是0，假若这句代码被放在func调用的前面首先自增1的话，会在末尾出现越界的问题
            End If
        End Sub

        Protected MustOverride Overloads Sub func(dx#, ByRef dy As Vector, Y As ValueVector)

    End Class
End Namespace

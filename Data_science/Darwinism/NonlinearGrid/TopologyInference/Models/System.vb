#Region "Microsoft.VisualBasic::2f74a059e49c38d2d3b3415d2b20c354, Data_science\Darwinism\NonlinearGrid\TopologyInference\Models\System.vb"

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

    ' Class GridSystem
    ' 
    '     Properties: A, AC, C
    ' 
    '     Function: Clone, Evaluate
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization

''' <summary>
''' The Nonlinear Grid Dynamics System
''' </summary>
''' <remarks>
''' 理论上可以拟合任意一个系统
''' </remarks>
Public Class GridSystem : Implements ICloneable(Of GridSystem)

    ''' <summary>
    ''' 线性方程的常数项
    ''' </summary>
    ''' <returns></returns>
    Public Property AC As Double
    Public Property A As Vector
    Public Property C As Correlation()
    Public Property P As Vector

    ''' <summary>
    ''' Evaluate the system dynamics
    ''' 
    ''' ```
    ''' C + A * X ^ C
    ''' ```
    ''' </summary>
    ''' <param name="X"></param>
    ''' <returns></returns>
    Public Function Evaluate(X As Vector) As Double
        Dim C As Vector = Me.C.Select(Function(ci) ci.Evaluate(X)).AsVector
        Dim F As Vector = X ^ C
        Dim fx As Vector = A * F / (P + F)
        Dim result = AC + fx.Sum

        Return result
    End Function

    Public Function Clone() As GridSystem Implements ICloneable(Of GridSystem).Clone
        Return New GridSystem With {
            .A = New Vector(A.AsEnumerable),
            .AC = AC,
            .C = C _
                .Select(Function(ci) ci.Clone) _
                .ToArray,
            .P = New Vector(P.AsEnumerable)
        }
    End Function

    Public Overrides Function ToString() As String
        Return ""
    End Function
End Class

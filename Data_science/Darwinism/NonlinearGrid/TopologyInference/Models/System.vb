#Region "Microsoft.VisualBasic::1488bbd4ef31b2b83f06d96e3cc6850f, Data_science\Darwinism\NonlinearGrid\TopologyInference\Models\System.vb"

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
'     Properties: A, AC, Amplify, C, delay
' 
'     Function: Clone, Evaluate, ToString
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Public Interface IGrid(Of V As Vector, IC As ICorrelation(Of V))

    Property AC As Double
    Property A As V
    Property C As IC()

End Interface

Public Interface ICorrelation(Of V As Vector)
    Property B As V
    Property BC As Double
End Interface

''' <summary>
''' The Nonlinear Grid Dynamics System
''' </summary>
''' <remarks>
''' 理论上可以拟合任意一个系统
''' </remarks>
Public Class GridSystem : Implements IDynamicsComponent(Of GridSystem), IGrid(Of Vector, Correlation)

    ''' <summary>
    ''' 线性方程的常数项
    ''' </summary>
    ''' <returns></returns>
    Public Property AC As Double Implements IGrid(Of Vector, Correlation).AC
    Public Property A As Vector Implements IGrid(Of Vector, Correlation).A
    Public Property C As Correlation() Implements IGrid(Of Vector, Correlation).C

    Public ReadOnly Property Width As Integer Implements IDynamicsComponent(Of GridSystem).Width
        Get
            Return A.Dim
        End Get
    End Property

    ''' <summary>
    ''' Evaluate the system dynamics
    ''' 
    ''' ```
    ''' C + A * X ^ C
    ''' ```
    ''' </summary>
    ''' <param name="X"></param>
    ''' <returns></returns>
    Public Function Evaluate(X As Vector) As Double Implements IDynamicsComponent(Of GridSystem).Evaluate
        Dim C As Vector = Me.C.Select(Function(ci) ci.Evaluate(X)).AsVector
        ' 20190722 当X中存在负数的时候,假设对应的C相关因子为小数负数,则会出现NaN计算结果值
        Dim F As Vector = Math.E ^ C
        Dim fx As Vector = A * X * F
        Dim S = AC + fx.Sum

        Return S
    End Function

    Public Function Clone() As GridSystem Implements ICloneable(Of GridSystem).Clone
        Return New GridSystem With {
            .A = New Vector(A.AsEnumerable),
            .AC = AC,
            .C = C _
                .Select(Function(ci) ci.Clone) _
                .ToArray
        }
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function ToString() As String
        Return ToString(Me)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overloads Shared Function ToString(chromosome As GridSystem) As String
        Return chromosome.A.Length _
            .SeqIterator _
            .Select(Function(i)
                        Dim sign = chromosome.A(i)
                        Dim c = chromosome.C(i).B.Sum + chromosome.C(i).BC
                        Dim S = $"({chromosome.AC} + {sign} * {c})"

                        Return S
                    End Function) _
            .ToArray _
            .GetJson _
            .MD5
    End Function
End Class

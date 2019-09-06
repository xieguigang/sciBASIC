#Region "Microsoft.VisualBasic::42a2a2a7be3d81d5a86e45096b6b627a, Data_science\Darwinism\NonlinearGrid\TopologyInference\BigData\SparseSystem.vb"

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

    '     Class SparseGridSystem
    ' 
    '         Properties: A, AC, C, Width
    ' 
    '         Function: Clone, Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace BigData

    Public Class SparseGridSystem : Implements IDynamicsComponent(Of SparseGridSystem)
        Implements IGrid(Of HalfVector, SparseCorrelation)

        ''' <summary>
        ''' 线性方程的常数项
        ''' </summary>
        ''' <returns></returns>
        Public Property AC As Double Implements IGrid(Of HalfVector, SparseCorrelation).AC
        Public Property A As HalfVector Implements IGrid(Of HalfVector, SparseCorrelation).A
        Public Property C As SparseCorrelation() Implements IGrid(Of HalfVector, SparseCorrelation).C

        Public ReadOnly Property Width As Integer Implements IDynamicsComponent(Of SparseGridSystem).Width
            Get
                Return A.Length
            End Get
        End Property

        Public Function Evaluate(X As Vector) As Double Implements IDynamicsComponent(Of SparseGridSystem).Evaluate
            Dim C As Vector = Me.C.Select(Function(ci) ci.Evaluate(X)).AsVector
            ' 20190722 当X中存在负数的时候,假设对应的C相关因子为小数负数,则会出现NaN计算结果值
            Dim F As Vector = Math.E ^ C
            Dim fx As Vector = A * X * F
            Dim S = AC + fx.Sum

            Return S
        End Function

        Public Function Clone() As SparseGridSystem Implements ICloneable(Of SparseGridSystem).Clone
            Return New SparseGridSystem With {
                .A = New HalfVector(A),
                .AC = AC,
                .C = C _
                    .Select(Function(p) p.Clone) _
                    .ToArray
            }
        End Function

        Public Overrides Function ToString() As String
            Return A.Length _
                .SeqIterator _
                .Select(Function(i)
                            Dim sign = CSng(A(i))
                            Dim c = Me.C(i).B.Sum + Me.C(i).BC
                            Dim S = $"({AC} + {sign} * {c})"

                            Return S
                        End Function) _
                .ToArray _
                .GetJson _
                .MD5
        End Function
    End Class
End Namespace

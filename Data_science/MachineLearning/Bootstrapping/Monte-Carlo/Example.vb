#Region "Microsoft.VisualBasic::3b77316b6c4bc3ac1e35e4fe005b81fd, Data_science\MachineLearning\Bootstrapping\Monte-Carlo\Example.vb"

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

    '     Class Example
    ' 
    '         Function: eigenvector, params, yinit
    ' 
    '         Sub: func
    ' 
    '     Class TestObservation
    ' 
    '         Function: Compares, y0
    ' 
    '         Sub: func
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Calculus

Namespace MonteCarlo.Example

    ''' <summary>
    ''' 计算步骤
    ''' 
    ''' 1. 继承<see cref="Model"/>对象并实现具体的过程
    ''' 2. 设置好大概的参数的变化区间
    ''' 3. 设置好大概的函数初始值的变化区间
    ''' </summary>
    Public Class Example : Inherits Model

        Dim sin As var
        Dim a As Double
        Dim f As Double

        Protected Overrides Sub func(dx As Double, ByRef dy As Vector)
            dy(index:=sin) = a * Math.Sin(dx) + f
        End Sub

        Public Overrides Function params() As ValueRange()
            Return {
                New ValueRange(-1000, 1000) With {.Name = NameOf(a)},
                New ValueRange(-1000, 1000) With {.Name = NameOf(f)}
            }
        End Function

        Public Overrides Function yinit() As ValueRange()
            Return {
                New ValueRange(-1000, 1000) With {.Name = NameOf(sin)}
            }
        End Function

        Public Overrides Function eigenvector() As Dictionary(Of String, Eigenvector)
            Return Nothing
        End Function
    End Class

    Public Class TestObservation : Inherits ODEs

        Dim sin As var
        Dim a As Double = 10
        Dim f As Double = -9.3

        Dim compare As Boolean = False

        Protected Overrides Sub func(dx As Double, ByRef dy As Vector)
            dy(index:=sin) = a * Math.Sin(dx) + f
        End Sub

        Protected Overrides Function y0() As var()
            If compare Then
                Return {sin}
            Else
                Return {sin = f}
            End If
        End Function

        Public Shared Iterator Function Compares(n As Integer, a As Integer, b As Integer, parms As Dictionary(Of String, Double)) As IEnumerable(Of ODEsOut)
            Yield New TestObservation().Solve(n, a, b)
            Yield New TestObservation With {
                .a = parms(NameOf(a)),
                .compare = True,
                .f = parms(NameOf(f)),
                .sin = New var With {
                    .Name = NameOf(sin),
                    .value = parms(NameOf(sin))
                }
            }.Solve(n, a, b)
        End Function
    End Class
End Namespace

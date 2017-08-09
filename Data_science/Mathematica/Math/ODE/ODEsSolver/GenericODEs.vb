#Region "Microsoft.VisualBasic::a9a7d59c7ae6a32485e4e8fc081731c1, ..\sciBASIC#\Data_science\Mathematica\Math\ODE\ODEsSolver\GenericODEs.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Delegate Sub [Function](dx As Double, ByRef dy As Vector)

Public Class GenericODEs : Inherits ODEs

    ''' <summary>
    ''' df(dx As <see cref="Double"/>, ByRef dy As <see cref="Vector"/>)
    ''' </summary>
    ''' <returns></returns>
    Public Property df As [Function]

    Sub New(ParamArray vars As var())
        Me.vars = vars

        For Each x In vars.SeqIterator
            x.value.Index = x.i
        Next
    End Sub

    Sub New(vars As var(), df As [Function])
        Call Me.New(vars)
        Me.df = df
    End Sub

    Protected Overrides Sub func(dx As Double, ByRef dy As Vector)
        Call _df(dx, dy)
    End Sub

    Protected Overrides Function y0() As var()
        Return vars
    End Function
End Class

#Region "Microsoft.VisualBasic::22570f6c3b216ff908314d9752803f5f, Data_science\Mathematica\Math\ODE\Dynamics\GenericODEs.vb"

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

    '     Delegate Sub
    ' 
    ' 
    '     Class GenericODEs
    ' 
    '         Properties: df
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: y0
    ' 
    '         Sub: func
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Dynamics

    Public Delegate Sub [Function](dx As Double, ByRef dy As Vector)

    Public Class GenericODEs : Inherits ODEs

        ''' <summary>
        ''' df(dx As <see cref="Double"/>, ByRef dy As <see cref="Vector"/>)
        ''' </summary>
        ''' <returns></returns>
        Public Property df As [Function]

        Sub New(ParamArray vars As var())
            Me.vars = vars

            For Each x As SeqValue(Of var) In vars.SeqIterator
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
End Namespace

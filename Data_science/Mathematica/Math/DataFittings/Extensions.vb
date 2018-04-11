#Region "Microsoft.VisualBasic::17e1b49eeecbf623175271379a825201, Data_science\Mathematica\Math\DataFittings\Extensions.vb"

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

    ' Module Extensions
    ' 
    '     Function: X, Y, Yfit
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Module Extensions

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function X(fit As IFitted) As Vector
        Return fit.ErrorTest.Select(Function(point) point.X).AsVector
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function Y(fit As IFitted) As Vector
        Return fit.ErrorTest.Select(Function(point) point.Y).AsVector
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function Yfit(fit As IFitted) As Vector
        Return fit.ErrorTest.Select(Function(point) point.Yfit).AsVector
    End Function
End Module

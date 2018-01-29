#Region "Microsoft.VisualBasic::42d4c19d929cedb7cfe37a8abc7e2d49, ..\sciBASIC#\Data_science\Mathematica\Math\Math\Algebra\LP\OptimizationType.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
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

Imports System.ComponentModel

Namespace Algebra.LinearProgramming

    Public Enum OptimizationType

        ''' <summary>
        ''' Maximize
        ''' </summary>
        <Description("Maximize")> MAX
        ''' <summary>
        ''' Minimize
        ''' </summary>
        <Description("Minimize")> MIN
    End Enum
End Namespace

﻿#Region "Microsoft.VisualBasic::0406e6159606ed06fc7a9ca4c8f1c1c5, G:/GCModeller/src/runtime/sciBASIC#/Microsoft.VisualBasic.Core/src//ComponentModel/Algorithm/DynamicProgramming/DynamicProgramming.vb"

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

    '   Total Lines: 34
    '    Code Lines: 24
    ' Comment Lines: 0
    '   Blank Lines: 10
    '     File Size: 1.13 KB


    '     Class Cost
    ' 
    '         Function: DefaultCost, DefaultSubstituteCost
    ' 
    '     Delegate Function
    ' 
    ' 
    '     Delegate Function
    ' 
    ' 
    '     Interface IScore
    ' 
    '         Function: GetSimilarityScore
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language.Default

Namespace ComponentModel.Algorithm.DynamicProgramming

    Public Class Cost(Of T)

        Public insert As Func(Of T, Double)
        Public delete As Func(Of T, Double)
        Public substitute As Func(Of T, T, Double)
        Public transpose As Func(Of T, T, Double)

        Public Shared Function DefaultSubstituteCost(a As T, b As T, equals As GenericLambda(Of T).IEquals) As Double
            Return If(equals(a, b), 0, 1)
        End Function

        Public Shared Function DefaultCost(cost As Double) As [Default](Of Cost(Of T))
            Return New Cost(Of T) With {
                .insert = Function(x) cost,
                .delete = Function(x) cost,
                .substitute = Function(a, b) cost
            }
        End Function
    End Class

    Public Delegate Function ISimilarity(Of T)(x As T, y As T) As Double
    Public Delegate Function IEquals(Of T)(x As T, y As T) As Boolean

    Public Interface IScore(Of T)

        Function GetSimilarityScore(a As T, b As T) As Double

    End Interface

End Namespace

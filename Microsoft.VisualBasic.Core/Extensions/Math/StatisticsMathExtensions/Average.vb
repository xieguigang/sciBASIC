#Region "Microsoft.VisualBasic::4c8b3e8db9275b8503a5616f91739596, Microsoft.VisualBasic.Core\Extensions\Math\StatisticsMathExtensions\Average.vb"

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

    '     Class Average
    ' 
    '         Properties: Average
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    '         Operators: +
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Math.Statistics

    Public Class Average

        Public Sum#, N%

        Public ReadOnly Property Average As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Sum / N
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(data As IEnumerable(Of Double))
            With data.ToArray
                Sum = .Sum
                N = .Length
            End With
        End Sub

        Public Overrides Function ToString() As String
            Return $"Means of {N} samples = {Average}"
        End Function

        Public Shared Operator +(avg As Average, x#) As Average
            avg.Sum += x
            avg.N += 1
            Return avg
        End Operator

        Public Shared Widening Operator CType(avg As Double) As Average
            Return New Average() + avg
        End Operator
    End Class
End Namespace

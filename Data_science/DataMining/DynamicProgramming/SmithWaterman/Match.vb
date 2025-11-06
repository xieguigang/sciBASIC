#Region "Microsoft.VisualBasic::5d6c752f4b496aa2afd2493c3ee7ab95, Data_science\DataMining\DynamicProgramming\SmithWaterman\Match.vb"

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

    '   Total Lines: 92
    '    Code Lines: 48 (52.17%)
    ' Comment Lines: 30 (32.61%)
    '    - Xml Docs: 96.67%
    ' 
    '   Blank Lines: 14 (15.22%)
    '     File Size: 2.87 KB


    '     Class Match
    ' 
    '         Properties: fromA, fromB, score, toA, toB
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: isChainable, notOverlap, ToString
    '         Operators: -
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization

Namespace SmithWaterman

    ''' <summary>
    '''  Match class defintion
    ''' </summary>
    Public Class Match

        ''' <summary>
        ''' Returns the value of fromA.
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property fromA As Integer

        ''' <summary>
        ''' Returns the value of fromB.
        ''' </summary>
        <XmlAttribute> Public Property fromB As Integer

        ''' <summary>
        ''' Returns the value of toA.
        ''' </summary>
        <XmlAttribute> Public Property toA As Integer

        ''' <summary>
        ''' Returns the value of toB.
        ''' </summary>
        <XmlAttribute> Public Property toB As Integer

        ''' <summary>
        ''' Returns the value of score.
        ''' </summary>
        <XmlAttribute> Public Property score As Double

        Sub New()
        End Sub

        ''' <summary>
        ''' do match result data copy
        ''' </summary>
        ''' <param name="m"></param>
        Sub New(m As Match)
            fromA = m.fromA
            fromB = m.fromB
            toA = m.toA
            toB = m.toB
            score = m.score
        End Sub

        Sub New(fA As Integer, tA As Integer, fB As Integer, tB As Integer, s As Double)
            fromA = fA
            fromB = fB
            toA = tA
            toB = tB
            score = s
        End Sub

        ''' <summary>
        ''' Check whether this Match onecjt overlap with input Match m;
        ''' return true if two objects do not overlap
        ''' </summary>
        ''' <param name="m"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function notOverlap(m As Match) As Boolean
            Return (m.fromA > _toA OrElse _fromA > m.toA) AndAlso (m.fromB > _toB OrElse _fromB > m.toB)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function isChainable(m As Match) As Boolean
            Return (m.fromA > _toA AndAlso m.fromB > _toB)
        End Function

        Public Overrides Function ToString() As String
            Return $"[query: {{{fromA}, {toA}; len:{toA - fromA}}}, ref: {{{fromB}, {toB}; len:{toB - fromB}}}], score={score}"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator -(match As Match, offset%) As Match
            Return New Match With {
                .fromA = match.fromA - offset,
                .fromB = match.fromB - offset,
                .score = match.score,
                .toA = match.toA - offset,
                .toB = match.toB - offset
            }
        End Operator
    End Class
End Namespace

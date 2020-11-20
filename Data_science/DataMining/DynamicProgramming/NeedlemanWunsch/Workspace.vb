#Region "Microsoft.VisualBasic::f71e4e2f30a199253bb2ab8666196461, Data_science\DataMining\DynamicProgramming\NeedlemanWunsch\Workspace.vb"

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

    '     Class Workspace
    ' 
    '         Properties: NumberOfAlignments, Query, Score, Subject
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: getAligned1, getAligned2
    ' 
    '         Sub: AddAligned1, AddAligned2
    ' 
    '     Class ScoreMatrix
    ' 
    '         Properties: GapPenalty, MatchScore, MismatchScore
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: getMatchScore, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace NeedlemanWunsch

    ''' <summary>
    ''' Base class for the Needleman-Wunsch Algorithm
    ''' Bioinformatics 1, WS 15/16
    ''' Dr. Kay Nieselt and Alexander Seitz
    ''' </summary>
    Public Class Workspace(Of T)

        Dim aligned1 As New List(Of T())
        Dim aligned2 As New List(Of T())

        Protected ReadOnly __toChar As Func(Of T, Char)

        ''' <summary>
        ''' get numberOfAlignments </summary>
        ''' <returns> numberOfAlignments </returns>
        Public ReadOnly Property NumberOfAlignments As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return aligned1.Count
            End Get
        End Property

        Public ReadOnly Property Query As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New String(Sequence1.Select(Of Char)(__toChar).ToArray)
            End Get
        End Property

        Public ReadOnly Property Subject As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New String(Sequence2.Select(Of Char)(__toChar).ToArray)
            End Get
        End Property

        ''' <summary>
        ''' get sequence 1 </summary>
        ''' <return>  sequence 1 </return>
        Protected Sequence1 As T()

        ''' <summary>
        ''' get sequence 2cted int max (int a, int b, int c) {
        '''    return Math.max(a, Math.max(b, c)); </summary>
        ''' <return> sequence 2 </return>
        Protected Sequence2 As T()

        Protected scoreMatrix As ScoreMatrix(Of T)

        ''' <summary>
        ''' get aligned version of sequence 1 </summary>
        ''' <param name="i"> </param>
        ''' <returns>  aligned sequence 1 </returns>
        Public Function getAligned1(i As Integer) As T()
            Return aligned1(i)
        End Function

        ''' <summary>
        ''' set aligned sequence 1 </summary>
        ''' <param name="align"> </param>
        Protected Friend Sub AddAligned1(align As T())
            Me.aligned1.Add(align)
        End Sub

        ''' <summary>
        ''' get aligned version of sequence 2 </summary>
        ''' <param name="i"> </param>
        ''' <returns> aligned sequence 2 </returns>
        Public Function getAligned2(i As Integer) As T()
            Return aligned2(i)
        End Function

        ''' <summary>
        ''' set aligned sequence 2 </summary>
        ''' <param name="align"> </param>
        Protected Friend Sub AddAligned2(align As T())
            Me.aligned2.Add(align)
        End Sub

        ''' <summary>
        ''' get computed score </summary>
        ''' <returns> score </returns>
        Public Property Score As Integer

        Sub New(score As ScoreMatrix(Of T), toChar As Func(Of T, Char))
            __toChar = toChar
            scoreMatrix = score
        End Sub
    End Class

    Public Class ScoreMatrix(Of T)

        ''' <summary>
        ''' get gap open penalty </summary>
        ''' <returns> gap open penalty </returns>
        Public Property GapPenalty As Integer = 1

        ''' <summary>
        ''' get match score </summary>
        ''' <returns> match score </returns>
        Public Property MatchScore As Integer = 1

        ''' <summary>
        ''' get mismatch score </summary>
        ''' <returns> mismatch score </returns>
        Public Property MismatchScore As Integer = -1

        Friend ReadOnly __equals As IEquals(Of T)

        Sub New(match As IEquals(Of T))
            __equals = match
        End Sub

        ''' <summary>
        ''' if char a is equal to char b
        ''' return the match score
        ''' else return mismatch score
        ''' </summary>
        Public Overridable Function getMatchScore(a As T, b As T) As Integer
            If __equals(a, b) Then
                Return MatchScore
            Else
                Return MismatchScore
            End If
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

    End Class
End Namespace

#Region "Microsoft.VisualBasic::9aa52469ec6e12000c62c64b5a432f7c, ..\visualbasic_App\Data_science\Microsoft.VisualBasic.DataMining.Framework.DynamicProgramming\DP_lib\NeedlemanWunsch\NeedlemanWunschArguments.vb"

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
Imports Microsoft.VisualBasic

''' <summary>
''' Base class for the Needleman-Wunsch Algorithm
''' Bioinformatics 1, WS 15/16
''' Dr. Kay Nieselt and Alexander Seitz
''' </summary>
Public Class NeedlemanWunschArguments(Of T)

    Dim aligned1 As New List(Of T())
    Dim aligned2 As New List(Of T())

    ''' <summary>
    ''' get numberOfAlignments </summary>
    ''' <returns> numberOfAlignments </returns>
    Public ReadOnly Property NumberOfAlignments As Integer
        Get
            Return aligned1.Count
        End Get
    End Property

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

    Public ReadOnly Property Query As String
        Get
            Return New String(Sequence1.ToArray(Of Char)(__toChar))
        End Get
    End Property

    Public ReadOnly Property Subject As String
        Get
            Return New String(Sequence2.ToArray(Of Char)(__toChar))
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

    ''' <summary>
    ''' get aligned version of sequence 1 </summary>
    ''' <param name="i"> </param>
    ''' <returns>  aligned sequence 1 </returns>
    Public Function getAligned1(i As Integer) As T()
        Return aligned1(i)
    End Function

    ''' <summary>
    ''' set aligned sequence 1 </summary>
    ''' <param name="aligned1"> </param>
    Protected Friend Sub addAligned1(aligned1 As T())
        Me.aligned1.Add(aligned1)
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
    ''' <param name="aligned2"> </param>
    Protected Friend Sub addAligned2(aligned2 As T())
        Me.aligned2.Add(aligned2)
    End Sub

    ''' <summary>
    ''' get computed score </summary>
    ''' <returns> score </returns>
    Public Property Score As Integer

    Sub New(match As IEquals(Of T), toChar As Func(Of T, Char))
        __equals = match
        __toChar = toChar
    End Sub

    ReadOnly __toChar As Func(Of T, Char)
    ReadOnly __equals As IEquals(Of T)

    ''' <summary>
    ''' if char a is equal to char b
    ''' return the match score
    ''' else return mismatch score
    ''' </summary>
    Protected Function match(a As T, b As T) As Integer
        If __equals(a, b) Then Return MatchScore
        Return MismatchScore
    End Function
End Class

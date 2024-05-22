#Region "Microsoft.VisualBasic::ddfa3fea37aa96905cb59c064a20c1f4, Data_science\DataMining\DynamicProgramming\NeedlemanWunsch\Workspace.vb"

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

    '   Total Lines: 95
    '    Code Lines: 47 (49.47%)
    ' Comment Lines: 32 (33.68%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 16 (16.84%)
    '     File Size: 3.12 KB


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
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace NeedlemanWunsch

    ''' <summary>
    ''' Base class for the Needleman-Wunsch Algorithm
    ''' Bioinformatics 1, WS 15/16
    ''' Dr. Kay Nieselt and Alexander Seitz
    ''' </summary>
    Public Class Workspace(Of T)

        Dim aligned1 As New List(Of T())
        Dim aligned2 As New List(Of T())

        Protected Friend ReadOnly m_toChar As Func(Of T, Char)

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
                Return New String(Sequence1.Select(Of Char)(m_toChar).ToArray)
            End Get
        End Property

        Public ReadOnly Property Subject As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New String(Sequence2.Select(Of Char)(m_toChar).ToArray)
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
        ''' get computed score </summary>
        ''' <returns> score </returns>
        Public Property Score As Single

        Sub New(score As ScoreMatrix(Of T), toChar As Func(Of T, Char))
            m_toChar = toChar
            scoreMatrix = score
        End Sub

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
    End Class
End Namespace

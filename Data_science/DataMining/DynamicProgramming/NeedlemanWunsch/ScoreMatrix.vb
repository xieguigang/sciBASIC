#Region "Microsoft.VisualBasic::cff75b3ac5971175cdc42b28f0757abe, Data_science\DataMining\DynamicProgramming\NeedlemanWunsch\ScoreMatrix.vb"

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

    '   Total Lines: 51
    '    Code Lines: 26 (50.98%)
    ' Comment Lines: 14 (27.45%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (21.57%)
    '     File Size: 1.57 KB


    '     Class ScoreMatrix
    ' 
    '         Properties: GapPenalty, MatchScore, MismatchScore
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: getMatchScore, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace NeedlemanWunsch

    Public Class ScoreMatrix(Of T) : Implements IScore(Of T)

        ''' <summary>
        ''' get gap open penalty </summary>
        ''' <returns> gap open penalty </returns>
        Public Property GapPenalty As Double = 1

        ''' <summary>
        ''' get match score </summary>
        ''' <returns> match score </returns>
        Public Property MatchScore As Double = 1

        ''' <summary>
        ''' get mismatch score </summary>
        ''' <returns> mismatch score </returns>
        Public Property MismatchScore As Double = -1

        Friend ReadOnly m_equals As IEquals(Of T)

        Sub New(match As IEquals(Of T))
            m_equals = match
        End Sub

        Sub New(symbol As GenericSymbol(Of T))
            Call Me.New(symbol.getEquals)
        End Sub

        ''' <summary>
        ''' if char a is equal to char b
        ''' return the match score
        ''' else return mismatch score
        ''' </summary>
        Public Overridable Function getMatchScore(a As T, b As T) As Double Implements IScore(Of T).GetSimilarityScore
            If m_equals(a, b) Then
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

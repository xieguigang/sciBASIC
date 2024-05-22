#Region "Microsoft.VisualBasic::c9bbad5eb163605d003d738d3f7148ab, Data_science\DataMining\DynamicProgramming\NeedlemanWunsch\GlobalAlign.vb"

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

    '   Total Lines: 75
    '    Code Lines: 60 (80.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 15 (20.00%)
    '     File Size: 2.34 KB


    '     Class GlobalAlign
    ' 
    '         Properties: Length, query, score, subject
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Identities, (+2 Overloads) ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Data
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Text

Namespace NeedlemanWunsch

    Public Class GlobalAlign(Of T)

        Public Property score As Double
        Public Property query As T()
        Public Property subject As T()

        Private ReadOnly toChar As Func(Of T, Char)

        Public ReadOnly Property Length As Integer
            Get
                If query.Length <> subject.Length Then
                    Throw New InvalidConstraintException($"the size of query sequence({query.Length}) should be equals to the subject sequence({subject.Length})!")
                Else
                    Return query.Length
                End If
            End Get
        End Property

        Sub New(toChar As Func(Of T, Char))
            Me.toChar = toChar
        End Sub

        Public Function Identities(scoreMatrix As ScoreMatrix(Of T)) As Double
            Dim vq As New List(Of Double)
            Dim vs As New List(Of Double)

            For i As Integer = 0 To Length - 1
                Call vq.Add(1)

                If scoreMatrix.m_equals(query(i), subject(i)) Then
                    Call vs.Add(1)
                Else
                    Call vs.Add(0)
                End If
            Next

            Return Math.SSM(vq.AsVector, vs.AsVector)
        End Function

        Public Overloads Function ToString(toChar As Func(Of T, Char)) As String
            Dim q As New List(Of Char)
            Dim c As New List(Of Char)
            Dim s As New List(Of Char)

            For i As Integer = 0 To query.Length - 1
                q.Add(toChar(query(i)))
                s.Add(toChar(subject(i)))

                If q.Last = s.Last Then
                    c.Add("*"c)
                Else
                    c.Add(" "c)
                End If
            Next

            Return {
                q.CharString,
                c.CharString,
                s.CharString
            }.JoinBy(ASCII.LF)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return ToString(toChar)
        End Function
    End Class
End Namespace

#Region "Microsoft.VisualBasic::a969605793d3286ffe6d215ff85ea536, Data_science\DataMining\DynamicProgramming\NeedlemanWunsch\GlobalAlign.vb"

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

'     Structure GlobalAlign
' 
' 
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Text

Namespace NeedlemanWunsch

    Public Structure GlobalAlign(Of T)

        Dim Score#
        Dim query As T()
        Dim subject As T()

        Public ReadOnly Property Length As Integer
            Get
                If query.Length <> subject.Length Then
                    Throw New InvalidExpressionException("")
                Else
                    Return query.Length
                End If
            End Get
        End Property

        Public ReadOnly Property PossibleSimilarity As Double
            Get
                Return Score / Length
            End Get
        End Property

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
            Return ToString(Function(x) x.ToString.First)
        End Function
    End Structure
End Namespace

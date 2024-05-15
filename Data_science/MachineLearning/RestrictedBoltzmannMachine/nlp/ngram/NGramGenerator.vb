#Region "Microsoft.VisualBasic::9c5c47eac818c964cea76dbf6f21289e, Data_science\MachineLearning\RestrictedBoltzmannMachine\nlp\ngram\NGramGenerator.vb"

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

    '   Total Lines: 37
    '    Code Lines: 25
    ' Comment Lines: 3
    '   Blank Lines: 9
    '     File Size: 1.05 KB


    '     Class NGramGenerator
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: concat, generate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic
Imports System.Text

Namespace nlp.ngram

    ''' <summary>
    ''' Created by kenny on 5/23/14.
    ''' </summary>
    Public Class NGramGenerator

        Private ReadOnly n As Integer

        Public Sub New(n As Integer)
            Me.n = n
        End Sub

        Public Function generate(sentence As String) As IList(Of String)

            Dim ngrams As IList(Of String) = New List(Of String)()
            Dim words = sentence.StringSplit(" ", True)
            For i = 0 To words.Length - n + 1 - 1
                ngrams.Add(concat(words, i, i + n))
            Next
            Return ngrams
        End Function

        Public Function concat(words As String(), start As Integer, [end] As Integer) As String
            Dim sb As StringBuilder = New StringBuilder()
            For i = start To [end] - 1
                sb.Append(If(i > start, " ", "") & words(i))
            Next
            Return sb.ToString()
        End Function

    End Class

End Namespace

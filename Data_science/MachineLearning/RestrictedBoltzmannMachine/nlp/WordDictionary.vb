#Region "Microsoft.VisualBasic::b06c97ea1b2d0c858e580222bf436987, Data_science\MachineLearning\RestrictedBoltzmannMachine\nlp\WordDictionary.vb"

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

    '   Total Lines: 94
    '    Code Lines: 66
    ' Comment Lines: 7
    '   Blank Lines: 21
    '     File Size: 3.23 KB


    '     Class WordDictionary
    ' 
    '         Properties: WordVectors
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: buildSentence, contains, getClosestWord, getVector, readLines
    '                   size
    ' 
    '         Sub: add, load
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math.functions.distance
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.nlp.encode

Namespace nlp


    ''' <summary>
    ''' Created by kenny on 5/23/14.
    ''' </summary>
    Public Class WordDictionary

        ' a unique word vector for each word (storing a 1xN matrix)
        Private wordVectorsField As IDictionary(Of String, DenseMatrix) = New Dictionary(Of String, DenseMatrix)()

        Private wordEncoder As WordEncoder = New DiscreteRandomWordEncoder()

        Public Sub New()
        End Sub

        ' load a list of new line separated words
        Public Sub New(file As String)
            load(file)
        End Sub

        Private Sub load(file As String)
            Dim lines = readLines(file)
            For Each line In lines
                If line.StartsWith("#", StringComparison.Ordinal) Then
                    Continue For
                End If ' ignore comments
                add(line)
            Next
        End Sub

        Public Sub add(word As String)
            ' LOGGER.info("adding: " + word);
            If wordVectorsField.ContainsKey(word) Then
                Return
            End If

            wordVectorsField(word) = wordEncoder.encode(word)
        End Sub

        Public Function contains(word As String) As Boolean
            Return wordVectorsField.ContainsKey(word)
        End Function

        Public Function getVector(word As String) As DenseMatrix
            Return wordVectorsField(word)
        End Function

        Public ReadOnly Property WordVectors As IList(Of DenseMatrix)
            Get
                Return New List(Of DenseMatrix)(wordVectorsField.Values)
            End Get
        End Property

        Public Function buildSentence(wordVectors As IList(Of DenseMatrix)) As String
            Dim words As IList(Of String) = New List(Of String)()
            For Each wordVector In wordVectors
                words.Add(getClosestWord(wordVector))
            Next
            Return words.JoinBy("-")
        End Function

        ' TODO speed up
        Public Function getClosestWord(wordVector As DenseMatrix) As String

            Dim distanceFunction As DistanceFunction = New EuclideanDistanceFunction()

            Dim closest As String = Nothing
            Dim minDistance = Double.MaxValue
            For Each entry As KeyValuePair(Of String, DenseMatrix) In wordVectorsField
                Dim distance = distanceFunction.distance(entry.Value, wordVector)
                If distance < minDistance Then
                    minDistance = distance
                    closest = entry.Key
                End If
            Next
            Return closest
        End Function

        Public Function size() As Integer
            Return wordVectorsField.Count
        End Function

        Private Function readLines(file As String) As IList(Of String)
            Return file.IterateAllLines().ToList()
        End Function

    End Class

End Namespace

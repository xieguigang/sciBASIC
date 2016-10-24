' *****************************************************************************
' Copyright 2012 Yuriy Lagodiuk
' 
' Licensed under the Apache License, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
' 
'   http://www.apache.org/licenses/LICENSE-2.0
' 
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
' *****************************************************************************

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Java

Namespace GAF

    Public Class Population(Of C As Chromosome(Of C))
        Implements IEnumerable(Of C)

        Const DEFAULT_NUMBER_OF_CHROMOSOMES As Integer = 32

        Dim chromosomes As New List(Of C)(DEFAULT_NUMBER_OF_CHROMOSOMES)
        ReadOnly random As New Random()

        Public Overridable Sub addChromosome(chromosome As C)
            Me.chromosomes.Add(chromosome)
        End Sub

        Public Overridable ReadOnly Property Size As Integer
            Get
                Return Me.chromosomes.Count
            End Get
        End Property

        Public Overridable ReadOnly Property RandomChromosome As C
            Get
                Dim numOfChromosomes As Integer = Me.chromosomes.Count
                ' TODO improve random generator
                ' maybe use pattern strategy ?
                Dim indx As Integer = Me.random.Next(numOfChromosomes)
                Return Me.chromosomes(indx)
            End Get
        End Property

        Default Public ReadOnly Property Item(index%) As C
            Get
                Return chromosomes(index)
            End Get
        End Property

        Public Overridable Sub sortPopulationByFitness(chromosomesComparator As IComparer(Of C))
            Call Arrays.Shuffle(Me.chromosomes)
            Me.chromosomes.Sort(chromosomesComparator)
        End Sub

        ''' <summary>
        ''' shortening population till specific number
        ''' </summary>
        Public Overridable Sub trim(len As Integer)
            Me.chromosomes = Me.chromosomes.sublist(0, len)
        End Sub

        Public Iterator Function GetEnumerator() As IEnumerator(Of C) Implements IEnumerable(Of C).GetEnumerator
            For Each x As C In chromosomes
                Yield x
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class

End Namespace
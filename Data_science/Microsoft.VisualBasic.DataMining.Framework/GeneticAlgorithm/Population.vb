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

    Public Class Population(Of chr As Chromosome(Of chr))
        Implements IEnumerable(Of chr)

        Const DEFAULT_NUMBER_OF_CHROMOSOMES As Integer = 32

        Dim chromosomes As New List(Of chr)(DEFAULT_NUMBER_OF_CHROMOSOMES)
        ReadOnly _random As New Random()

        ''' <summary>
        ''' Add chromosome
        ''' </summary>
        ''' <param name="chromosome"></param>
        Public Sub Add(chromosome As chr)
            Call chromosomes.Add(chromosome)
        End Sub

        ''' <summary>
        ''' The number of chromosome elements in the inner list
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Size As Integer
            Get
                Return chromosomes.Count
            End Get
        End Property

        ''' <summary>
        ''' Gets random chromosome
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Random As chr
            Get
                Dim numOfChromosomes As Integer = chromosomes.Count
                ' TODO improve random generator
                ' maybe use pattern strategy ?
                Dim indx As Integer = _random.Next(numOfChromosomes)
                Return chromosomes(indx)
            End Get
        End Property

        ''' <summary>
        ''' Gets chromosome by index
        ''' </summary>
        ''' <param name="index%"></param>
        ''' <returns></returns>
        Default Public ReadOnly Property Item(index%) As chr
            Get
                Return chromosomes(index)
            End Get
        End Property

        Public Sub SortPopulationByFitness(comparator As IComparer(Of chr))
            Call Arrays.Shuffle(chromosomes)
            Call chromosomes.Sort(comparator)
        End Sub

        ''' <summary>
        ''' shortening population till specific number
        ''' </summary>
        Public Sub Trim(len As Integer)
            chromosomes = chromosomes.sublist(0, len)
        End Sub

        Public Iterator Function GetEnumerator() As IEnumerator(Of chr) Implements IEnumerable(Of chr).GetEnumerator
            For Each x As chr In chromosomes
                Yield x
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
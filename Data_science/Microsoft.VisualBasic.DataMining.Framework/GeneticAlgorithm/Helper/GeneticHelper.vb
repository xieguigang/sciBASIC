Imports System.Runtime.CompilerServices

Namespace GAF.Helper

    Public Module GeneticHelper

        ''' <summary>
        ''' Returns clone of current chromosome, which is mutated a bit
        ''' </summary>
        ''' <param name="v#"></param>
        ''' <param name="random"></param>
        <Extension> Public Sub Mutate(ByRef v#(), random As Random)
            ' just select random element of vector
            ' and increase or decrease it on small value
            Dim index As Integer = random.Next(v.Length)
            Dim mutationValue# =
                random.Next(v.Length) - (random.NextDouble * v.Length)

            v(index) += mutationValue
        End Sub

        ''' <summary>
        ''' Returns clone of current chromosome, which is mutated a bit
        ''' </summary>
        ''' <param name="v%"></param>
        ''' <param name="random"></param>
        <Extension> Public Sub Mutate(ByRef v%(), random As Random)
            ' just select random element of vector
            ' and increase or decrease it on small value
            Dim index As Integer = random.Next(v.Length)
            Dim mutationValue# =
                random.Next(v.Length) -
                random.Next(v.Length)

            v(index) += mutationValue
        End Sub

        ''' <summary>
        ''' Returns list of siblings 
        ''' Siblings are actually new chromosomes, 
        ''' created using any of crossover strategy
        ''' </summary>
        ''' <param name="random"></param>
        ''' <param name="v1#"></param>
        ''' <param name="v2#"></param>
        <Extension>
        Public Sub Crossover(Of T)(random As Random, ByRef v1 As T(), ByRef v2 As T())
            Dim index As Integer = random.Next(v1.Length - 1)
            Dim tmp As T

            ' one point crossover
            For i As Integer = index To v1.Length - 1
                tmp = v1(i)
                v1(i) = v2(i)
                v2(i) = tmp
            Next
        End Sub

        ''' <summary>
        ''' The simplest strategy for creating initial population <br/>
        ''' in real life it could be more complex
        ''' </summary>
        <Extension>
        Public Function InitialPopulation(Of T As Chromosome(Of T))(base As T, populationSize As Integer) As Population(Of T)
            Dim population As New Population(Of T)()

            For i As Integer = 0 To populationSize - 1
                ' each member of initial population
                ' is mutated clone of base chromosome
                Dim chr As T = base.Mutate()
                population.Add(chr)
            Next
            Return population
        End Function
    End Module
End Namespace
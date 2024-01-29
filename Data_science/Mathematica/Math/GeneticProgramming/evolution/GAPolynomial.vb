Imports Microsoft.VisualBasic.Math.Scripting
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.evolution.measure
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.model
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.model.impl

Namespace evolution

    Public Class GAPolynomial : Implements Individual

        Private Shared OBJECTIVEField As Objective = Nothing

        Public Shared WriteOnly Property Objective As Objective
            Set(value As Objective)
                OBJECTIVEField = value
            End Set
        End Property

        Private ReadOnly rootField As ExpressionWrapper

        Private ReadOnly parametersField As ISet(Of Number)

        Private orderField As Integer
        Private fitnessField As Double

        Public Sub New(root As ExpressionWrapper)
            rootField = root
            parametersField = GAPolynomialUtils.traverse(root)
            orderField = parametersField.Count - 1
            fitnessField = Double.NaN
        End Sub

        Protected Friend Overridable ReadOnly Property Root As ExpressionWrapper
            Get
                Return rootField
            End Get
        End Property

        Protected Friend Overridable ReadOnly Property Parameters As ISet(Of Number)
            Get
                Return parametersField
            End Get
        End Property

        Protected Friend Overridable Property Order As Integer
            Set(value As Integer)
                orderField = value
            End Set
            Get
                Return orderField
            End Get
        End Property


        Public Overridable ReadOnly Property Expression As Expression Implements Individual.Expression
            Get
                Return rootField.Expression
            End Get
        End Property

        Public Overridable ReadOnly Property Fitness As Double Implements Individual.Fitness
            Get
                Return fitnessField
            End Get
        End Property

        Public Overridable Function computeFitness(dataTuples As IList(Of DataPoint)) As Double Implements Individual.computeFitness

            Dim errors = New Double(dataTuples.Count - 1) {}
            Dim iterator As IEnumerator(Of DataPoint) = dataTuples.GetEnumerator()
            Dim i = 0

            While iterator.MoveNext()
                Dim tuple = iterator.Current
                Dim y = rootField.eval(tuple.x)
                errors(i) = OBJECTIVEField.getError(tuple.y, y)
                i += 1
            End While
            fitnessField = OBJECTIVEField.getOverallError(errors)
            fitnessField = If(Double.IsNaN(fitnessField), Double.PositiveInfinity, fitnessField)
            Return fitnessField
        End Function

        Public Overridable Function CompareTo(other As Individual) As Integer Implements IComparable(Of Individual).CompareTo
            If other Is Nothing OrElse Fitness < other.Fitness Then
                Return -1
            ElseIf Fitness > other.Fitness Then
                Return +1
            Else
                Return 0
            End If
        End Function

    End Class

End Namespace

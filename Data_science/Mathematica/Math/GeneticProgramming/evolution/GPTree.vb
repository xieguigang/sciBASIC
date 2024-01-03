
Namespace evolution

    Public Class GPTree
        Implements Individual

        Private Shared OBJECTIVEField As Objective = Nothing

        Public Shared Property Objective As Objective
            Set(value As Objective)
                OBJECTIVEField = value
            End Set
            Get
                Return OBJECTIVEField
            End Get
        End Property

        Private ReadOnly rootField As ExpressionWrapper

        Private ReadOnly terminalsField As ISet(Of ExpressionWrapper)
        Private ReadOnly nonTerminalsField As ISet(Of ExpressionWrapper)

        Private depthField As Integer
        Private fitnessField As Double

        Public Sub New(root As ExpressionWrapper)
            rootField = root
            Dim result = GPTreeUtils.traverse(root)
            depthField = result.depth
            terminalsField = result.terminals
            nonTerminalsField = result.nonTerminals
            fitnessField = Double.NaN
        End Sub

        Protected Friend Overridable ReadOnly Property Root As ExpressionWrapper
            Get
                Return rootField
            End Get
        End Property

        Protected Friend Overridable ReadOnly Property Terminals As ISet(Of ExpressionWrapper)
            Get
                Return terminalsField
            End Get
        End Property

        Protected Friend Overridable ReadOnly Property NonTerminals As ISet(Of ExpressionWrapper)
            Get
                Return nonTerminalsField
            End Get
        End Property

        Protected Friend Overridable Property Depth As Integer
            Set(value As Integer)
                depthField = value
            End Set
            Get
                Return depthField
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

        Public Overridable Function computeFitness(dataTuples As IList(Of Tuple)) As Double Implements Individual.computeFitness

            Dim errors = New Double(dataTuples.Count - 1) {}
            Dim iterator As IEnumerator(Of Tuple) = dataTuples.GetEnumerator()
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

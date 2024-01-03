Namespace model

    Public Interface BinaryExpression
        Inherits Expression

        Property LeftChild As Expression
        Property RightChild As Expression

        Function swapLeftChild(newLeftChild As Expression) As Expression

        Function swapRightChild(newRightChild As Expression) As Expression

    End Interface

End Namespace

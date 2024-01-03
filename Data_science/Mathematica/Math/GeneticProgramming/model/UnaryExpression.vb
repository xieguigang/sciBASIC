Namespace model
    Public Interface UnaryExpression
        Inherits Expression

        Property Child As Expression


        Function swapChild(newChild As Expression) As Expression

    End Interface

End Namespace

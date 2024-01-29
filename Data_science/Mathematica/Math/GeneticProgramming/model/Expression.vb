Namespace model

    ''' <summary>
    ''' Represents an expression that can be evaluated for
    ''' a variable <tt>x</tt>.
    ''' </summary>
    Public Interface Expression

        ''' <returns> <tt>true</tt> IFF the expression is a terminal </returns>
        ReadOnly Property Terminal As Boolean

        ''' <returns> depth of the expression tree </returns>
        ReadOnly Property Depth As Integer

        ''' <param name="x"> double variable <tt>x</tt> </param>
        ''' <returns> result of the expression for <tt>x</tt> </returns>
        Function eval(x As Double) As Double

        ''' <returns> string representation of the expression </returns>
        Function toStringExpression() As String

        ''' <returns> exact duplicate of the expression </returns>
        Function duplicate() As Expression

    End Interface

End Namespace

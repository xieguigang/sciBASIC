Namespace ShapleyValue

    ''' <summary>
    ''' facade of Shapley application
    ''' 
    ''' @author Franck Benault
    ''' 
    ''' @version	0.0.2
    ''' @since 0.0.2
    ''' 
    ''' </summary>
    Public Interface ShapleyApplication

        ''' <summary>
        ''' full calculation of the Shapley value
        ''' </summary>
        ''' <returns> partial result of the Shapley value </returns>
        Function calculate() As IDictionary(Of String, Double)

        ''' <summary>
        ''' complete the calculation of the Shapley value 
        ''' with the next set of coalitions choosen sequentially 
        ''' </summary>
        ''' <param name="nbCoalitions"> number of coalition taken in account for this calculation
        ''' </param>
        ''' <returns> partial result of the Shapley value
        ''' </returns>
        ''' <exception cref="Exception"> when different strategy are mixed
        '''  </exception>
        Function calculate(nbCoalitions As Long) As IDictionary(Of String, Double)

        ''' <summary>
        ''' complete the calculation of the Shapley value 
        ''' with the next set of coalitions which may be choosen randomly
        ''' </summary>
        ''' <param name="nbCoalitions"> number of coalition taken in account for this calculation </param>
        ''' <param name="strategy"> way to choose the next coalitions (sequential or random)  
        ''' </param>
        ''' <returns> partial result of the Shapley value
        ''' </returns>
        ''' <exception cref="Exception"> when different strategy are mixed
        '''  </exception>
        Function calculate(nbCoalitions As Long, strategy As CoalitionStrategy) As IDictionary(Of String, Double)

        ''' <summary>
        ''' check in the case of sequential strategy if the last coalition possible is reached
        ''' 
        ''' @return </summary>
        ''' <exception cref="Exception"> in the cas of non sequential strategy </exception>
        ReadOnly Property LastCoalitionReached As Boolean

    End Interface

End Namespace

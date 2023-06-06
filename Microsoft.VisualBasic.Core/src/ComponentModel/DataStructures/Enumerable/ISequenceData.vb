Namespace ComponentModel.Collection.Generic

    ''' <summary>
    ''' The sequence analysis data model, example as biological sequence, 
    ''' time signal, time sequence anaysis
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Interface ISequenceData(Of T, List As IEnumerable(Of T))

        ''' <summary>
        ''' the sequence data provider, example like time serial signals, 
        ''' logging data, biological sequence
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property SequenceData As List

    End Interface
End Namespace
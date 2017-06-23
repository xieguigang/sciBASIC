Namespace ComponentModel

    ''' <summary>
    ''' Driver abstract model
    ''' </summary>
    Public Interface ITaskDriver

        ''' <summary>
        ''' Start run this driver object.
        ''' </summary>
        ''' <returns></returns>
        Function Run() As Integer
    End Interface
End Namespace
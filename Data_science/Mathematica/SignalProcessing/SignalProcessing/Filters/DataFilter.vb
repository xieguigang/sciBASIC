Namespace mr.go.sgfilter
    ''' <summary>
    ''' This interface represents types which are able to filter data, for example:
    ''' eliminate redundant points.
    ''' 
    ''' @author Marcin Rzeźnicki </summary>
    ''' <seealsocref="SGFilter"/> </seealso>
    Public Interface DataFilter

        Function filter(data As Double()) As Double()
    End Interface

End Namespace

Namespace Filters
    ''' <summary>
    ''' This interface represents types which are able to filter data, for example:
    ''' eliminate redundant points.
    ''' 
    ''' @author Marcin Rzeźnicki </summary>
    ''' 
    Public Interface DataFilter

        Function filter(data As Double()) As Double()

    End Interface

End Namespace

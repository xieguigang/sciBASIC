Namespace Components

    ''' <summary>
    ''' 
    ''' </summary>
    Public Structure Dimension

        ''' <summary>
        ''' String with the name of the dimension
        ''' </summary>
        Dim name As String
        ''' <summary>
        ''' Number with the size of the dimension
        ''' </summary>
        Dim size As Integer

        Public Overrides Function ToString() As String
            Return $"{name}(size={size})"
        End Function
    End Structure

    Public Class DimensionList

        Public Property dimensions As Dimension()
        Public Property recordId As Integer?
        Public Property recordName As String

        Public Overrides Function ToString() As String
            Return $"[{recordId}] {recordName}"
        End Function
    End Class

    ''' <summary>
    ''' Metadata for the record dimension
    ''' </summary>
    Public Class recordDimension

        ''' <summary>
        ''' Number of elements in the record dimension
        ''' </summary>
        ''' <returns></returns>
        Public Property length As Integer
        ''' <summary>
        ''' Id number In the list Of dimensions For the record dimension
        ''' </summary>
        ''' <returns></returns>
        Public Property id As Integer
        ''' <summary>
        ''' String with the name of the record dimension
        ''' </summary>
        ''' <returns></returns>
        Public Property name As String
        ''' <summary>
        ''' Number with the record variables step size
        ''' </summary>
        ''' <returns></returns>
        Public Property recordStep As Integer

        Public Overrides Function ToString() As String
            Return $"[{id}] {name} ({recordStep}x{length})"
        End Function
    End Class

    Public Class attribute

        ''' <summary>
        ''' String with the name of the attribute
        ''' </summary>
        ''' <returns></returns>
        Public Property name As String
        ''' <summary>
        ''' String with the type of the attribute
        ''' </summary>
        ''' <returns></returns>
        Public Property type As String
        ''' <summary>
        ''' A number or string with the value of the attribute
        ''' </summary>
        ''' <returns></returns>
        Public Property value As String

        Public Overrides Function ToString() As String
            Return $"Dim {name} As {type} = {value}"
        End Function
    End Class

    Public Class variable

        ''' <summary>
        ''' String with the name of the variable
        ''' </summary>
        ''' <returns></returns>
        Public Property name As String
        ''' <summary>
        ''' Array with the dimension IDs of the variable
        ''' </summary>
        ''' <returns></returns>
        Public Property dimensions As Integer()
        ''' <summary>
        ''' Array with the attributes of the variable
        ''' </summary>
        ''' <returns></returns>
        Public Property attributes As attribute()
        ''' <summary>
        ''' String with the type of the variable
        ''' </summary>
        ''' <returns></returns>
        Public Property type As String
        ''' <summary>
        ''' Number with the size of the variable
        ''' </summary>
        ''' <returns></returns>
        Public Property size As Integer?
        ''' <summary>
        ''' Number with the offset where of the variable begins
        ''' </summary>
        ''' <returns></returns>
        Public Property offset As Long
        ''' <summary>
        ''' True if Is a record variable, false otherwise
        ''' </summary>
        ''' <returns></returns>
        Public Property record As Boolean

        Public Overrides Function ToString() As String
            Return $"Dim {name}[offset={offset}] As {type}"
        End Function
    End Class
End Namespace
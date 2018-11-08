Public Structure Dimension

    Dim name As String
    Dim size As Integer

End Structure

Public Class DimensionList
    Public Property dimensions As Dimension()
    Public Property recordId As Integer?
    Public Property recordName As String
End Class

Public Class recordDimension
    Public Property length As UInt32
    Public Property id As Integer
    Public Property name As String
    Public Property recordStep As Integer
End Class

Public Class attribute
    Public Property name As String
    Public Property type As String
    Public Property value As String
End Class

Public Class variable
    Public Property name As String
    Public Property dimensions As Integer()
    Public Property attributes As attribute()
    Public Property type As String
    Public Property size As Integer
    Public Property offset As Long
    Public Property record As Boolean
End Class
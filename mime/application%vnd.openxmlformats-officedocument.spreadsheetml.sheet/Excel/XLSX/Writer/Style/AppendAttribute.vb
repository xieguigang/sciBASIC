Namespace XLSX.Writer.Styling


    ''' <summary>
    ''' Attribute designated to control the copying of style properties
    ''' </summary>
    Public Class AppendAttribute
        Inherits Attribute
        ''' <summary>
        ''' Gets or sets a value indicating whether Ignore
        ''' Indicates whether the property annotated with the attribute is ignored during the copying of properties
        ''' </summary>
        Public Property Ignore As Boolean

        ''' <summary>
        ''' Gets or sets a value indicating whether NestedProperty
        ''' Indicates whether the property annotated with the attribute is a nested property. Nested properties are ignored during the copying of properties but can be broken down to its sub-properties
        ''' </summary>
        Public Property NestedProperty As Boolean

        ''' <summary>
        ''' Initializes a new instance of the <see cref="AppendAttribute"/> class
        ''' </summary>
        Public Sub New()
            Ignore = False
            NestedProperty = False
        End Sub
    End Class
End Namespace
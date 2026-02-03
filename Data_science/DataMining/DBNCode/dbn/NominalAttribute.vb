Imports DBNCode.utils

Namespace dbn

    Public Class NominalAttribute
        Implements Attribute

        Private nameField As String

        Private values As BidirectionalArray(Of String) = New BidirectionalArray(Of String)()

        Public Overridable ReadOnly Property Numeric As Boolean Implements Attribute.Numeric
            Get
                Return False
            End Get
        End Property

        Public Overridable ReadOnly Property Nominal As Boolean Implements Attribute.Nominal
            Get
                Return True
            End Get
        End Property

        Public Overridable Function size() As Integer Implements Attribute.size
            Return values.size()
        End Function

        Public Overridable Function add(value As String) As Boolean Implements Attribute.add
            Return values.add(value)
        End Function

        Public Overrides Function ToString() As String Implements Attribute.ToString
            Return "" & values.ToString()
        End Function

        Public Overridable Function getIndex(value As String) As Integer Implements Attribute.getIndex
            Return values.getIndex(value)
        End Function

        Public Overridable Function [get](index As Integer) As String Implements Attribute.get
            Return values.get(index)
        End Function

        Public Overridable Property Name As String Implements Attribute.Name
            Set(value As String)
                nameField = value
            End Set
            Get
                Return nameField
            End Get
        End Property


    End Class

End Namespace

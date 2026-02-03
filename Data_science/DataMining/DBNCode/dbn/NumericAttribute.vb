Imports Microsoft.VisualBasic.DataMining.DynamicBayesianNetwork.utils

Namespace dbn

    Public Class NumericAttribute
        Implements Attribute

        Private nameField As String

        Private values As New BidirectionalArray(Of Single)()

        Public Overridable ReadOnly Property Numeric As Boolean Implements Attribute.Numeric
            Get
                Return True
            End Get
        End Property

        Public Overridable ReadOnly Property Nominal As Boolean Implements Attribute.Nominal
            Get
                Return False
            End Get
        End Property

        Public Overridable Function size() As Integer Implements Attribute.size
            Return values.size()
        End Function

        Public Overridable Function add(value As String) As Boolean Implements Attribute.add
            Return values.add(Single.Parse(value))
        End Function

        Public Overrides Function ToString() As String Implements Attribute.ToString
            Return "" & values.ToString()
        End Function

        Public Overridable Function getIndex(value As String) As Integer Implements Attribute.getIndex
            Return values.getIndex(Single.Parse(value))
        End Function

        Public Overridable Function [get](index As Integer) As String Implements Attribute.get
            Return Convert.ToString(values.get(index))
        End Function

        Public Overridable Property Name As String Implements Attribute.Name
            Set(value As String)
                nameField = value
            End Set
            Get
                Return nameField
            End Get
        End Property


        Public Overrides Function GetHashCode() As Integer
            Const prime = 31
            Dim result = 1
            result = prime * result + (If(ReferenceEquals(nameField, Nothing), 0, nameField.GetHashCode()))
            Return result
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            If Me Is obj Then
                Return True
            End If
            If obj Is Nothing Then
                Return False
            End If
            If Not (TypeOf obj Is NumericAttribute) Then
                Return False
            End If
            Dim other = CType(obj, NumericAttribute)
            If ReferenceEquals(nameField, Nothing) Then
                If Not ReferenceEquals(other.nameField, Nothing) Then
                    Return False
                End If
            ElseIf Not nameField.Equals(other.nameField) Then
                Return False
            End If
            Return True
        End Function



    End Class

End Namespace

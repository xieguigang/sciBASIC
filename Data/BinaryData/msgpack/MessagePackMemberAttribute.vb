Namespace Serialization

    ''' <summary>
    ''' Mimic the full CLI namespace and naming so that this library can be used
    ''' as a drop-in replacement and/or linked file with both frameworks as needed.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class MessagePackMemberAttribute
        Inherits Attribute

        Private ReadOnly idField As Integer

        Public Sub New(id As Integer)
            idField = id
            NilImplication = NilImplication.MemberDefault
        End Sub

        Public ReadOnly Property Id As Integer
            Get
                Return idField
            End Get
        End Property

        Public Property NilImplication As NilImplication
    End Class
End Namespace

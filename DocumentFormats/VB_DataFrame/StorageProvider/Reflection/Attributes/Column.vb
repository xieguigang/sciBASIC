Imports System.Data.Linq.Mapping

Namespace StorageProvider.Reflection

    ''' <summary>
    ''' This is a column in the csv document. 
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property,
                    AllowMultiple:=False,
                    Inherited:=False)>
    Public Class ColumnAttribute : Inherits DataAttribute
        Implements Reflection.IAttributeComponent

        Public Overridable ReadOnly Property ProviderId As ProviderIds Implements IAttributeComponent.ProviderId
            Get
                Return ProviderIds.Column
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Name"></param>
        Sub New(Name As String)
            Me.Name = Name
            If String.IsNullOrEmpty(Name) Then
                Throw New DataException($"{NameOf(Name)} value can not be null!")
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return Name
        End Function

        Public Shared ReadOnly Property TypeInfo As System.Type = GetType(ColumnAttribute)

        Public Shared Narrowing Operator CType(attr As ColumnAttribute) As String
            Return attr.Name
        End Operator

        Public Shared Widening Operator CType(sName As String) As ColumnAttribute
            Return New ColumnAttribute(sName)
        End Operator
    End Class
End Namespace
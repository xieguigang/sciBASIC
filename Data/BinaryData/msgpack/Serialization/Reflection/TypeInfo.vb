Imports System.Collections
Imports Microsoft.VisualBasic.Emit.Delegates

Namespace Serialization.Reflection

    Public Class TypeInfo

        Public ReadOnly Property IsGenericList As Boolean
        Public ReadOnly Property IsGenericDictionary As Boolean
        Public ReadOnly Property Schema As Type

        Public ReadOnly Property IsSerializableGenericCollection As Boolean
            Get
                Return IsGenericList OrElse IsGenericDictionary
            End Get
        End Property

        Public Sub New(type As Type)
            IsGenericList = type.ImplementInterface(GetType(IList))
            IsGenericDictionary = type.ImplementInterface(GetType(IDictionary))
            Schema = type
        End Sub

        Public Overrides Function ToString() As String
            Return Schema.FullName
        End Function

    End Class
End Namespace
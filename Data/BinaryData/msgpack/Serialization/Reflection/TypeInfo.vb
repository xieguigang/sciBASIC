Namespace Serialization.Reflection

    Public Class TypeInfo

        Public Property IsGenericList As Boolean
        Public Property IsGenericDictionary As Boolean
        Public Property IsSerializableGenericCollection As Boolean

        Public Sub New(type As Type)
            IsGenericList = type.GetInterface("System.Collections.Generic.IList`1") IsNot Nothing
            IsGenericDictionary = type.GetInterface("System.Collections.Generic.IDictionary`2") IsNot Nothing
            IsSerializableGenericCollection = IsGenericList OrElse IsGenericDictionary
        End Sub

    End Class
End Namespace
Imports System.Text

Namespace StorageProvider.Reflection

    ''' <summary>
    ''' This property will not write into the csv document file.
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=False)>
    Public Class Ignored : Inherits Attribute

        Public Shared ReadOnly Property TypeInfo As Type = GetType(Ignored)

        Public Overrides Function ToString() As String
            Return "This property is ignored for the reflection operation..."
        End Function
    End Class

    Public Interface IAttributeComponent
        ReadOnly Property ProviderId As ProviderIds
    End Interface

    Public Enum ProviderIds As Integer
        NullMask = -100

        Column = 0
        CollectionColumn
        [Enum]
        MetaAttribute
        ''' <summary>
        ''' 在写入Csv文件之后是以键值对的形式出现的： Name:=value  (例如： GeneId:=XC_1184)
        ''' </summary>
        KeyValuePair
    End Enum
End Namespace
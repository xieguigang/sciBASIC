Namespace StorageProvider.Reflection

    ''' <summary>
    ''' This property is a array data type object.(并不建议使用本Csv属性来储存大量的文本字符串，极容易出错)
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=False)>
    Public Class CollectionAttribute : Inherits Csv.StorageProvider.Reflection.ColumnAttribute
        Implements Reflection.IAttributeComponent

        Public ReadOnly Property Delimiter As String

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <param name="Delimiter">由于受正则表达式的解析速度的影响，因为CSV文件是使用逗号进行分隔的，假若使用逗号的话，正则表达式的解析速度会比较低，故在这里优先考虑使用分号来作为分隔符</param>
        Sub New(Name As String, Optional Delimiter As String = "; ")
            Call MyBase.New(Name)
            Me._Delimiter = Delimiter
        End Sub

        Public Overrides Function ToString() As String
            Return Me.Name
        End Function

        Protected Friend Shared Shadows ReadOnly Property TypeInfo As System.Type = GetType(CollectionAttribute)

        Public Function CreateObject(cellData As String) As String()
            Dim Tokens As String() = Strings.Split(cellData, Me._Delimiter)
            Return Tokens
        End Function

        Public Overrides ReadOnly Property ProviderId As ProviderIds Implements IAttributeComponent.ProviderId
            Get
                Return ProviderIds.CollectionColumn
            End Get
        End Property

        Public Function CreateObject(Of T)(DataCollection As Generic.IEnumerable(Of T)) As String
            If DataCollection.IsNullOrEmpty Then
                Return ""
            End If

            Dim StringCollection As String() = (From item In DataCollection Where Not item Is Nothing Select _create = item.ToString).ToArray
            Return String.Join(_Delimiter, StringCollection)
        End Function

        Public Shared Function CreateObject(Of T)(DataCollection As Generic.IEnumerable(Of T), Delimiter As String) As String
            If DataCollection.IsNullOrEmpty Then
                Return ""
            End If

            Dim StringCollection As String() = (From item In DataCollection Select _create = item.ToString).ToArray
            Return String.Join(Delimiter, StringCollection)
        End Function
    End Class
End Namespace
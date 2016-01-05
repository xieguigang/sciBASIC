Namespace Serialization

    Public Class DynamicsConfiguration : Inherits Dynamic.DynamicObject

        ''' <summary>
        ''' 加载完数据之后返回其自身
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="path"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function LoadDocument(Of T)(path As String) As Object
            Dim TextChunk As String() = IO.File.ReadAllLines(path)

            Return Me
        End Function

#Region "Dynamics Support"

        Public Overrides Function GetDynamicMemberNames() As IEnumerable(Of String)
            Return MyBase.GetDynamicMemberNames()
        End Function

        Public Overrides Function TryGetMember(binder As Dynamic.GetMemberBinder, ByRef result As Object) As Boolean
            Return MyBase.TryGetMember(binder, result)
        End Function

        Public Overrides Function TrySetMember(binder As Dynamic.SetMemberBinder, value As Object) As Boolean
            Return MyBase.TrySetMember(binder, value)
        End Function

        Public Overrides Function TryConvert(binder As Dynamic.ConvertBinder, ByRef result As Object) As Boolean
            Return MyBase.TryConvert(binder, result)
        End Function
#End Region
    End Class
End Namespace
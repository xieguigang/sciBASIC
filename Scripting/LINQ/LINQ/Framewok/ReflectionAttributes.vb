Imports System.Text.RegularExpressions

Namespace Framework.Reflection

    ''' <summary>
    ''' LINQ entity type
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Struct, allowmultiple:=False, inherited:=True)>
    Public Class LINQEntity : Inherits System.Attribute

        Dim _EntityType As String

        Public Shared ReadOnly ILINQEntity As System.Type = GetType(LINQEntity)

        Sub New(EntityType As String)
            _EntityType = EntityType
        End Sub

        Public Overrides Function ToString() As String
            Return _EntityType
        End Function

        ''' <summary>
        ''' 获取目标类型上的自定义属性中的LINQEntity类型对象中的EntityType属性值
        ''' </summary>
        ''' <param name="Type"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetEntityType(Type As System.Type) As String
            Dim attr = Type.GetCustomAttributes(ILINQEntity, True)
            If attr Is Nothing OrElse attr.Count = 0 Then
                Return ""
            Else
                Dim LINQEnity = DirectCast(attr.First, Reflection.LINQEntity)
                Return LINQEnity._EntityType
            End If
        End Function
    End Class
End Namespace
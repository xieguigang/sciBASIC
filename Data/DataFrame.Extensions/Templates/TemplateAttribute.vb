''' <summary>
''' 用于标记当前的类型为csv输入文件的模板
''' </summary>
<AttributeUsage(AttributeTargets.Class Or AttributeTargets.Struct,
                AllowMultiple:=False,
                Inherited:=True)>
Public Class TemplateAttribute : Inherits Attribute

    Public ReadOnly Property Category As String
    Public ReadOnly Property AliasName As String

    ''' <summary>
    ''' 目标类型的模板会以csv文件的形式被保存，并且文件名为类型的名称
    ''' </summary>
    ''' <param name="category">分类信息，这个应该是一个文件夹的名称</param>
    Sub New(category$, Optional alias$ = Nothing)
        Me.Category = category
        Me.AliasName = [alias]
    End Sub

    Public Overrides Function ToString() As String
        Return Category
    End Function
End Class

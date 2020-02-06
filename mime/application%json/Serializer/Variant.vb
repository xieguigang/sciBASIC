Imports Microsoft.VisualBasic.MIME.application.json.Javascript

Public MustInherit Class [Variant]

    Protected ReadOnly schemaList As Type()

    Dim m_jsonValue As Object

    ''' <summary>
    ''' 反序列化得到的结果值
    ''' </summary>
    ''' <returns></returns>
    Public Property jsonValue As Object
        Get
            Return m_jsonValue
        End Get
        Friend Set(value As Object)
            m_jsonValue = value
        End Set
    End Property

    Sub New(ParamArray schemaList As Type())
        Me.schemaList = schemaList

        If schemaList.IsNullOrEmpty Then
            Throw New InvalidProgramException
        End If
    End Sub

    ''' <summary>
    ''' 在这里应该是根据一些json文档的关键特征选择合适的类型进行反序列
    ''' 选择的过程由用户代码进行控制
    ''' </summary>
    ''' <param name="json"></param>
    ''' <returns></returns>
    Protected Friend MustOverride Function which(json As JsonObject) As Type

    Public Overrides Function ToString() As String
        If m_jsonValue Is Nothing Then
            Return $"Variant[{schemaList.Select(Function(t) t.Name).JoinBy(", ")}]"
        Else
            Return jsonValue.ToString
        End If
    End Function

End Class
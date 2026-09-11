Namespace Data.Repository

    ' ============================================================================
    '  JsonlStore.vb —— 纯文本行存储引擎的兼容包装
    '  历史上本引擎只用于 JSONL（每行一个 JSON 对象），因此命名为 JsonlStore。
    '  引擎本身与行格式无关，现已泛化为 <see cref="TextLineStore"/>。
    '  这里保留 JsonlStore 作为 Thin Wrapper，以兼容既有调用方与测试代码。
    ' ============================================================================

    ''' <summary>
    ''' <see cref="TextLineStore"/> 的兼容别名。行为与泛化后的行存储引擎完全一致。
    ''' </summary>
    Public NotInheritable Class JsonlStore
        Inherits TextLineStore

        ''' <summary>打开（或创建）指定数据文件的存储引擎。</summary>
        Public Sub New(dataFilePath As String, Optional options As JsonlStoreOptions = Nothing)
            MyBase.New(dataFilePath, options)
        End Sub

    End Class

End Namespace

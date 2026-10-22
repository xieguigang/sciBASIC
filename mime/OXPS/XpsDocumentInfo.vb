' ============================================================================
' XpsDocumentInfo.vb - XPS/OXPS 文档属性模型
'
' 从包内 docProps/core.xml 读取的元数据，字段与 RtfDocumentInfo 保持同形，
' 便于在报告流水线中统一处理。
' ============================================================================

''' <summary>XPS / OXPS 文档属性（源自 docProps/core.xml）。</summary>
Public Class XpsDocumentInfo

    ''' <summary>文档标题。</summary>
    Public Property Title As String = ""

    ''' <summary>文档作者。</summary>
    Public Property Author As String = ""

    ''' <summary>文档主题。</summary>
    Public Property Subject As String = ""

    ''' <summary>文档关键字。</summary>
    Public Property Keywords As String = ""

    ''' <summary>文档描述。</summary>
    Public Property Description As String = ""

    ''' <summary>生成程序（core.xml 的 lastModifiedBy）。</summary>
    Public Property Creator As String = ""

End Class

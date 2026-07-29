Namespace JSONSchema

    ''' <summary>
    ''' 针对markdown格式有限的支持
    ''' </summary>
    Public Class Block

        ''' <summary>
        ''' 块级语法类型，统一小写。当前支持的完整集合：
        ''' heading(h) / paragraph(p) / code / list(li) / blockquote /
        ''' table / hr(horizontal-rule) / image(img) / html(raw) /
        ''' math / link / tasklist / footnote / deflist
        ''' </summary>
        ''' <returns></returns>
        Public Property type As String
        ''' <summary>
        ''' heading level if type = heading
        ''' </summary>
        ''' <returns></returns>
        Public Property level As Integer
        ''' <summary>
        ''' the text content of heading/paragraph/code/blockquote
        ''' </summary>
        ''' <returns></returns>
        Public Property content As String
        ''' <summary>
        ''' the language code if type = code, example as bash/r/vbnet/c-sharp/python/php
        ''' </summary>
        ''' <returns></returns>
        Public Property language As String
        ''' <summary>
        ''' is ordered list if type = list
        ''' </summary>
        ''' <returns></returns>
        Public Property ordered As Boolean
        ''' <summary>
        ''' the list items for type = list
        ''' </summary>
        ''' <returns></returns>
        Public Property items As String()
        ''' <summary>
        ''' the table headers for type = table
        ''' </summary>
        ''' <returns></returns>
        Public Property headers As String()
        ''' <summary>
        ''' the table header alignments, value could be left|right|center
        ''' </summary>
        ''' <returns></returns>
        Public Property alignments As String()
        ''' <summary>
        ''' the table rows for type = table, each block elements inside this array should be list type, list items will be used as table row cells
        ''' </summary>
        ''' <returns></returns>
        Public Property rows As String()()

        ''' <summary>
        ''' the image source url if type = image/img
        ''' </summary>
        ''' <returns></returns>
        Public Property url As String
        ''' <summary>
        ''' the alternative text if type = image/img
        ''' </summary>
        ''' <returns></returns>
        Public Property alt As String
        ''' <summary>
        ''' the optional title (hover tip) if type = image/img or link
        ''' </summary>
        ''' <returns></returns>
        Public Property title As String

        ''' <summary>
        ''' tasklist only: 与 <see cref="items"/> 平行排列，标记每一项是否被勾选。
        ''' 当该字段为 Nothing 时，渲染时默认所有项均未勾选。
        ''' </summary>
        ''' <returns></returns>
        Public Property checked As Boolean()

        ''' <summary>
        ''' footnote only: 脚注的唯一标识，例如 "1" / "note"。
        ''' 在 markdown 中表现为 [^id]: content，在 html 中表现为 id="fn-id"。
        ''' </summary>
        ''' <returns></returns>
        Public Property id As String

        ''' <summary>
        ''' deflist only: 术语列表（对应 html 的 &lt;dt&gt;）。
        ''' 与 <see cref="definitions"/> 平行排列。
        ''' </summary>
        ''' <returns></returns>
        Public Property terms As String()

        ''' <summary>
        ''' deflist only: 定义列表（对应 html 的 &lt;dd&gt;），与 <see cref="terms"/> 平行排列。
        ''' </summary>
        ''' <returns></returns>
        Public Property definitions As String()

    End Class
End Namespace
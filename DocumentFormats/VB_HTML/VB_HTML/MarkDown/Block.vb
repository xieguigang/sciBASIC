Namespace MarkDown

    ''' <summary>
    ''' Block-Level Grammar
    ''' </summary>
    Module Block

        Public Const newline = "^\n+"
        Public Const code = "^( {4}[^\n]+\n*)+"
        Public Const fences = "noop"
        Public Const hr = "^( *[-*_]){3,} *(?:\n+|$)"
        Public Const heading = "^ *(#{1,6}) *([^\n]+?) *#* *(?:\n+|$)"
        Public Const nptable = "noop"
        Public Const lheading = "^([^\n]+)\n *(=|-){2,} *(?\n+|$)"
        Public Const blockquote = "^( *>[^\n]+(\n(?!def)[^\n]+)*\n*)+"
        Public Const list = "^( *)(bull) [\s\S]+?(?hr|def|\n{2,}(?! )(?!\1bull )\n*|\s*$)"
        Public Const html = "^ *(?:comment *(?:\n|\s*$)|closed *(?:\n{2,}|\s*$)|closing *(?:\n{2,}|\s*$))"
        Public Const def = "^ *\[([^\]]+)\] *<?([^\s>]+)>?(?: +[""(]([^\n]+)["")])? *(?:\n+|$)"
        Public Const table = "noop"
        Public Const paragraph = "^((?:[^\n]+\n?(?!hr|heading|lheading|blockquote|tag|def))+)\n*"
        Public Const text = "^[^\n]+"

        Public Const bullet = "(?[*+-]|\d+\.)"
        Public Const item = "^( *)(bull) [^\n]*(?:\n(?!\1bull )[^\n]*)*"

        Public Const tag As String = "(?!(?:" &
            "a|em|strong|small|s|cite|q|dfn|abbr|data|time|code" &
            "|var|samp|kbd|sub|sup|i|b|u|mark|ruby|rt|rp|bdi|bdo" &
            "|span|br|wbr|ins|del|img)\\b)\\w+(?!:/|[^\\w\\s@]*@)\\b"

    End Module
End Namespace
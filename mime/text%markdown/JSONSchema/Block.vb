Namespace JSONSchema

    ''' <summary>
    ''' 针对markdown格式有限的支持
    ''' </summary>
    Public Class Block

        ''' <summary>
        ''' table/heading/paragraph/code/list/blockquote
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

    End Class
End Namespace
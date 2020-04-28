
Imports System.Text

Namespace Text

    ''' <summary>
    ''' The text document encodings constant for text file read and write
    ''' </summary>
    Public Enum Encodings As Byte

        ''' <summary>
        ''' <see cref="Encoding.Default"/>: Gets an encoding for the operating system's current ANSI code page.
        ''' </summary>
        [Default] = 0
        ASCII = 10
        ''' <summary>
        ''' Alias of the value <see cref="UTF16"/>.
        ''' (utf-16编码的别名？所以使用这个编码的效果是和<see cref="UTF16"/>的效果是一样的)
        ''' </summary>
        Unicode
        UTF7
        ''' <summary>
        ''' 在Linux平台上面是<see cref="TextEncodings.UTF8WithoutBOM"/>，而在Windows平台上面则是带有BOM的UTF8格式. 
        ''' (HTML的默认的编码格式，假若所保存的html文本出现乱码，请考虑是不是应该要选择这个编码才行？)
        ''' </summary>
        UTF8
        UTF8WithoutBOM
        ''' <summary>
        ''' VB.NET的XML文件的默认编码格式为utf-16
        ''' </summary>
        UTF16
        UTF32

        ''' <summary>
        ''' Text encoding for simplify Chinese.
        ''' </summary>
        GB2312
    End Enum
End Namespace
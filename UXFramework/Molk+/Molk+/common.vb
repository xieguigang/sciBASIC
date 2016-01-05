Module Common

    ''' <summary>
    ''' Microsoft YaHei font.(微软雅黑字体)
    ''' </summary>
    ''' <remarks></remarks>
    Public Const YaHei As String = "Microsoft YaHei"

    Public Function Format(s As Xml.Linq.XElement, ParamArray args As String())
        Return String.Format(s.Value, args)
    End Function
End Module

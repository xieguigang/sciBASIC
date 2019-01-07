Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Net.Http
Imports gdiImage = System.Drawing.Image

Namespace HTML.CSS.Parser

    Public Module UrlEvaluator

        ''' <summary>
        ''' 暂时还不支持SVG图像
        ''' </summary>
        ''' <param name="expression">
        ''' + url('filepath') 图片文件路径
        ''' + url('data:xxx') base64编码的data uri图像数据
        ''' </param>
        ''' <returns></returns>
        Public Function EvaluateAsImage(expression As String) As gdiImage
            Dim uri As String = expression.GetStackValue("(", ")").Trim("'"c)

            If DataURI.IsWellFormedUriString(uri) Then
                ' 是data uri base64字符串
                Return DataURI.URIParser(uri).ToStream.LoadImage
            Else
                If uri.IsURLPattern Then
                    ' 是网络文件
                    With App.GetAppSysTempFile
                        uri.DownloadFile(.ByRef)
                        Return .LoadImage
                    End With
                Else
                    ' 是本地文件
                    Return uri.LoadImage
                End If
            End If
        End Function
    End Module
End Namespace
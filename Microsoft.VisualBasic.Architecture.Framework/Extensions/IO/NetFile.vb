Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace FileIO

    <PackageNamespace("IO.NetFile")>
    Public Module NetFile

        ''' <summary>
        ''' 将网络文件映射为本地文件，这个可以同时兼容http或者本地文件路径
        ''' </summary>
        ''' <param name="url"></param>
        ''' <returns></returns>
        <ExportAPI("MapNetFile")>
        <Extension>
        Public Function MapNetFile(url As String) As String
            Dim path As String = url.GetMapPath

            If path.FileExists Then
                Return path
            Else  ' 下载数据然后缓存
                Call DownloadFile(url, path)
                Return path
            End If
        End Function

        <ExportAPI("NetFile.FileExists")>
        <Extension>
        Public Function NetFileExists(url As String) As Boolean
            Return url.GetMapPath.FileExists
        End Function

        ''' <summary>
        ''' 网络文件转换为本地文件路径
        ''' </summary>
        ''' <param name="url"></param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("Map.Path")>
        <Extension>
        Public Function GetMapPath(url As String) As String
            If InStr(url, "http://", CompareMethod.Text) +
                InStr(url, "https://", CompareMethod.Text) > 0 Then

                url = Strings.Split(url, "//").Last
                url = App.AppSystemTemp & "/" & url.NormalizePathString

                Dim folders As String = FileSystem.GetParentPath(url)
                Call FileSystem.CreateDirectory(folders)

                Return url
            Else
                If url.FileExists Then
                    Return url
                Else
                    Throw New Exception(url & " is a unrecognized url path!")
                End If
            End If
        End Function
    End Module
End Namespace
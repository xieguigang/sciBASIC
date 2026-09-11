Imports System.Text

Namespace Data.Repository

    ''' <summary>配置项。</summary>
    Public NotInheritable Class JsonlStoreOptions

        ''' <summary>数据文件编码。Nothing = UTF-8（推荐）。不支持 UTF-16。</summary>
        Public Property Encoding As Encoding
        ''' <summary>写入使用的换行符。Nothing = 自动检测，缺省 LF。</summary>
        Public Property NewLine As String
        ''' <summary>稀疏索引粒度（每多少行记一个字节偏移）。</summary>
        Public Property IndexGranularity As Integer = 1024
        ''' <summary>每次写操作后是否 fsync 日志（断电级安全；False 仅防进程崩溃）。</summary>
        Public Property FsyncEachWrite As Boolean = True
        ''' <summary>打开时截断数据文件尾部不完整行。注意：会删除“无结尾换行符”的最后一行。</summary>
        Public Property RepairTornTail As Boolean = True
        Public Property ReadBufferBytes As Integer = 1 << 20
        Public Property MergeBufferBytes As Integer = 1 << 20
        Public Property LogBufferBytes As Integer = 1 << 16
    End Class
End Namespace
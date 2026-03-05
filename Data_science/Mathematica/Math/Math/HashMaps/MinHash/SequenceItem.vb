Namespace HashMaps.MinHash

    ' 定义一个简单的数据结构来存储序列
    Public Class SequenceItem
        Public Property ID As Integer
        Public Property Content As String
        ' 最终生成的MinHash签名
        Public Property Signature As List(Of Integer)
    End Class

    ' 算法配置参数
    Public Class Config
        Public Const K_Shingle As Integer = 5      ' 切片长度
        Public Const Num_HashFunctions As Integer = 100 ' MinHash签名长度
        Public Const Num_Bands As Integer = 20     ' LSH波段数
        Public Const Rows_Per_Band As Integer = 5  ' 每个波段的行数 (100 / 20 = 5)
    End Class



End Namespace
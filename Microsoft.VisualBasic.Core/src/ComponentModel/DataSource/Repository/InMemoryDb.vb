Imports System.Runtime.CompilerServices
Imports System.Text

Namespace ComponentModel.DataSourceModel.Repository

    Public MustInherit Class InMemoryDb

        ''' <summary>
        ''' 枚举数据库中所有的键。
        ''' 此操作会遍历所有数据文件，可能比较耗时，建议在需要时调用。
        ''' </summary>
        ''' <returns>返回一个包含所有键的字符串集合。</returns>
        Public MustOverride Iterator Function EnumerateAllKeys() As IEnumerable(Of Byte())

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function [Get](key As String) As Byte()
            Return [Get](Encoding.UTF8.GetBytes(key))
        End Function

        Public MustOverride Function [Get](keydata As Byte()) As Byte()

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Put(key As String, data As Byte())
            Call Put(Encoding.UTF8.GetBytes(key), data)
        End Sub

        Public MustOverride Sub Put(keybuf As Byte(), data As Byte())

    End Class
End Namespace
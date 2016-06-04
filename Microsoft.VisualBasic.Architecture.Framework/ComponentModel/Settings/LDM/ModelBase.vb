
Namespace ComponentModel.Settings

    ''' <summary>
    ''' 具备有保存数据功能的可配置数据文件的基本定义
    ''' </summary>
    Public Interface IProfile

        Function Save(Optional FilePath As String = "", Optional Encoding As System.Text.Encoding = Nothing) As Boolean

        ''' <summary>
        ''' 本属性不能够被设置为只读属性是因为 Settings.Settings(Of IProfile).LoadFile 函数的需要
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property FilePath As String
    End Interface
End Namespace
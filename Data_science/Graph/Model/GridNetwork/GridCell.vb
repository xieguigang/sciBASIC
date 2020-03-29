Imports System.Drawing

Public Class GridCell(Of T As Class)

    ''' <summary>
    ''' 二维数组之中的索引 
    ''' </summary>
    ''' <returns></returns>
    Public Property index As Point

    Dim m_data As T

    Public Property data As T
        Get
            Return m_data
        End Get
        Protected Set(value As T)
            m_data = value
        End Set
    End Property

    Public Overrides Function ToString() As String
        Return $"[{index.X}, {index.Y}] {data.ToString}"
    End Function

End Class

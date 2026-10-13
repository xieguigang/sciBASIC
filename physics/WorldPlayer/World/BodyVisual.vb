Imports System.Drawing
Imports Microsoft.VisualBasic.Engine.Physics

''' <summary>
''' 一个"可被渲染的刚体"：物理刚体 + 渲染网格 + 外观。
''' 宿主通过它把物理世界映射到画面上，脚本侧只看到字符串 id。
''' </summary>
Public Class BodyVisual

    ''' <summary>脚本可见的标识。</summary>
    Public Property Id As String
    ''' <summary>可读名称（脚本可通过 label 检索）。</summary>
    Public Property Label As String
    ''' <summary>物理刚体。</summary>
    Public Property Body As RigidBody3D
    ''' <summary>局部渲染网格（与碰撞体共用同一份几何）。</summary>
    Public Property Mesh As Object
    ''' <summary>基础颜色。</summary>
    Public Property Color As Color
    ''' <summary>是否静态关卡几何。</summary>
    Public Property IsStaticGeometry As Boolean
    ''' <summary>存活时长（秒），用于生成时的高亮渐变。</summary>
    Public Property Age As Double

    Public Overrides Function ToString() As String
        Return $"{Id} [{Label}]"
    End Function
End Class

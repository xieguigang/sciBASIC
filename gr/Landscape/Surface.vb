Imports System.Drawing
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Imaging.Drawing2D

''' <summary>
''' 方便将3D模型保存与XML文件之中的数据模型对象
''' </summary>
''' <remarks>
''' 2017.1.30
''' 由于受到XML序列化的限制，这里就不再实现这个枚举接口了
''' </remarks>
Public Class Surface ': Implements IEnumerable(Of Point3D)

    ''' <summary>
    ''' 请注意，在这里面的点都是有先后顺序分别的
    ''' </summary>
    <XmlElement>
    Public Property vertices As Point3D()
    ''' <summary>
    ''' 这个是本``表面``对象所喷涂的纹理的定义，可以是颜色的名称或者表达式，也可以是图片的相对路径的引用
    ''' </summary>
    ''' <returns></returns>
    Public Property paint As String

    ''' <summary>
    ''' 将<see cref="paint"/>资源字符串转换为相对应的<see cref="Brush"/>画刷对象.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Brush As Brush
        Get
            Return paint.GetBrush
        End Get
    End Property

    Public Function CreateObject() As Drawing3D.Surface
        Return New Drawing3D.Surface With {
            .vertices = vertices,
            .brush = Brush
        }
    End Function

    Public Overrides Function ToString() As String
        Return paint
    End Function

    'Public Iterator Function GetEnumerator() As IEnumerator(Of Point3D) Implements IEnumerable(Of Point3D).GetEnumerator
    '    For Each x As Point3D In vertices
    '        Yield x
    '    Next
    'End Function

    'Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
    '    Yield GetEnumerator()
    'End Function
End Class

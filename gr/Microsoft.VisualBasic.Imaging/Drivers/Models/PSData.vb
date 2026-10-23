Imports System.Drawing
Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Imaging.PostScript
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Net.Http

Namespace Driver

    ''' <summary>
    ''' PostScript 格式的绘图数据：底层是 <see cref="PostScriptBuilder"/>
    ''' 所记录的绘制指令序列，<see cref="Save(Stream)"/> 时把它写成 ASCII PostScript 文本。
    ''' </summary>
    Public Class PostScriptData : Inherits GraphicsData
        Implements IVisualStudioPreviews

        ''' <summary>绘制指令的构建器（即绘图的上下文对象）</summary>
        ReadOnly ps As PostScriptBuilder

        Public Overrides ReadOnly Property Driver As Drivers
            Get
                Return Drivers.PostScript
            End Get
        End Property

        Public Overrides ReadOnly Property Previews As String Implements IVisualStudioPreviews.Previews
            Get
                Return "PostScript"
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="img">the postscript canvas context: <see cref="PostScriptBuilder"/></param>
        ''' <param name="size"></param>
        ''' <param name="padding"></param>
        Public Sub New(img As Object, size As Size, padding As Padding)
            MyBase.New(img, size, padding)

            Me.ps = DirectCast(img, PostScriptBuilder)
        End Sub

        ''' <summary>
        ''' 把 PostScript 脚本文本编码为 data uri
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function GetDataURI() As DataURI
            Dim buffer As New MemoryStream(Encoding.UTF8.GetBytes(ps.ToString))

            Return New DataURI(buffer, content_type)
        End Function

        ''' <summary>
        ''' 把 PostScript 脚本写入目标流
        ''' </summary>
        ''' <param name="out"></param>
        ''' <returns></returns>
        Public Overrides Function Save(out As Stream) As Boolean
            Dim bytes As Byte() = Encoding.UTF8.GetBytes(ps.ToString)

            Call out.Write(bytes, 0, bytes.Length)
            Call out.Flush()

            Return True
        End Function
    End Class
End Namespace

#If NET48 Then

Imports System.Drawing
Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary

Namespace Resource

    Public Module MimeData

        ''' <summary>
        ''' 将Bitmap对象序列化为Base64字符串
        ''' </summary>
        ''' <param name="bmp">要序列化的Bitmap对象</param>
        ''' <returns>Base64编码的字符串</returns>
        Public Function SerializeBitmapToBase64(bmp As Bitmap) As String
            Dim base64String As String = String.Empty
            ' 使用BinaryFormatter进行二进制序列化
            Dim binaryFormatter As New BinaryFormatter()

            ' 使用MemoryStream作为序列化的中间存储
            Using memoryStream As New MemoryStream()
                ' 将Bitmap对象序列化到内存流中
                binaryFormatter.Serialize(memoryStream, bmp)

                ' 将内存流中的二进制数据转换为Base64字符串
                base64String = Convert.ToBase64String(memoryStream.ToArray())
            End Using

            Return base64String
        End Function
    End Module
End Namespace

#End If
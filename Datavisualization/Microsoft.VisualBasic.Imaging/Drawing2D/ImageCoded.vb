Imports System.Drawing

''' <summary>
''' 
''' </summary>
''' <remarks>
''' # 
''' # res.0 (Image)
''' # ----------------------------
''' # ----------------------------
''' #
''' </remarks>
Public Module ImageCoded

    Dim Encoder As New SecurityString.SHA256("VectorScript", "12345678")

    Public Function Serialization(res As Image) As String
        Dim resImage As String = My.Computer.FileSystem.GetTempFileName
        Call res.Save(resImage, System.Drawing.Imaging.ImageFormat.Png)
        Dim bytes As Byte() = IO.File.ReadAllBytes(resImage)
        Return Convert.ToBase64String(Encoder.Encrypt(bytes))
    End Function

    Public Function DecodeImage(str As String) As Image
        Dim res = Encoder.Decrypt(System.Text.Encoding.Unicode.GetBytes(str))
        Dim resImage As String = My.Computer.FileSystem.GetTempFileName
        Call IO.File.WriteAllBytes(resImage, res)
        Return Image.FromFile(resImage)
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="path">File path of the <see cref="Drawing2D.DrawingScript"></see></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function LoadImageResource(path As String) As Dictionary(Of String, Image)
        Throw New NotImplementedException
    End Function
End Module

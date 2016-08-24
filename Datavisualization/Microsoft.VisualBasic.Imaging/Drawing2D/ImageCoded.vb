#Region "Microsoft.VisualBasic::721c45823a779b50ceba8f0f636f7060, ..\visualbasic_App\Datavisualization\Microsoft.VisualBasic.Imaging\Drawing2D\ImageCoded.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

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

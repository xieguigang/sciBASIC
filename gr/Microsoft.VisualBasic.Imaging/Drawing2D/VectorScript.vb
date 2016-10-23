#Region "Microsoft.VisualBasic::4011602bca0c183dc256f3d35d937a43, ..\visualbasic_App\gr\Microsoft.VisualBasic.Imaging\Drawing2D\VectorScript.vb"

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

Imports System.Text

Namespace Drawing2D

    ''' <summary>
    ''' 将绘图元素转换为Shoal脚本以方便进行保存
    ''' </summary>
    ''' <remarks>
    ''' 必须要有的元素：图形大小或者背景图片，这个元素是生成GDI设备锁必须的
    ''' </remarks>
    Public Class DrawingScript

        Dim _InternalVectogram As Vectogram

        Private Sub New()
        End Sub

        Sub New(Vectors As Vectogram)
            _InternalVectogram = Vectors
        End Sub

        Public Shared Function LoadDocument(Path As String) As DrawingScript
            Throw New NotImplementedException
        End Function

        Const [CALL] As String = "Call "

        Public Function ToScript() As String
            Dim sBuilder As StringBuilder = New StringBuilder(1024)

            Call sBuilder.AppendLine("# Vector Script Generator Information")
            Call sBuilder.AppendLine("#")
            Call sBuilder.AppendLine("# Library  [" & VectorAPI.APIModuleLibrary & "]")
            Call sBuilder.AppendLine("#")
            For Each attr In GetType(VectorAPI).Assembly.CustomAttributes
                Call sBuilder.AppendLine("# " & attr.ToString)
            Next
            Call sBuilder.AppendLine()

            Call sBuilder.AppendLine("Imports " & VectorAPI.VECTOR_SCRIPT_NAMESPACE)
            Call sBuilder.AppendLine()
            Call sBuilder.AppendLine(VectorAPI.DEVICE_WIDTH & "  <- " & Me._InternalVectogram.GDIDevice.Width)
            Call sBuilder.AppendLine(VectorAPI.DEVICE_HEIGHT & " <- " & Me._InternalVectogram.GDIDevice.Height)
            Call sBuilder.AppendLine()
            Call sBuilder.AppendLine([CALL] & VectorAPI.NEW_DEVICE)


            Return sBuilder.ToString
        End Function

        ''' <summary>
        ''' 将脚本转换为矢量图绘图设备
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ToVectogram() As Vectogram
            Throw New NotImplementedException
        End Function
    End Class
End Namespace

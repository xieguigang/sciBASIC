#Region "Microsoft.VisualBasic::cb5c3dd95e98fb947265188213234b0e, ..\visualbasic_App\Datavisualization\Microsoft.VisualBasic.Imaging\Drawing2D\VectorAPI.vb"

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

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports System.Drawing
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps

Namespace Drawing2D

    ''' <summary>
    ''' 矢量图描述脚本的API函数
    ''' </summary>
    ''' <remarks></remarks>
    ''' 
    <[PackageNamespace](VectorAPI.VECTOR_SCRIPT_NAMESPACE, Publisher:="xie.guigang@gmail.com", Url:="http://SourceForge.net/projects/shoal")>
    Public Module VectorAPI

        Public Const VECTOR_SCRIPT_NAMESPACE As String = "http://code.google.com/p/genome-in-code/tools/Microsoft.VisualBasic.VectorScript"

        Public ReadOnly Property APIModuleLibrary As String
            Get
                'Return IO.Path.GetFileNameWithoutExtension(GetType(VectorAPI).Assembly.Location)
                Return GetType(VectorAPI).Assembly.Location.ToFileURL
            End Get
        End Property

        Public Const NEW_TEXT_ELEMENT As String = "Text.Drawing.Element.New()"

        Friend Sub InitializeScript()

        End Sub

        Public Const NEW_DEVICE As String = "Vectogram.Device.New()"
        Public Const DEVICE_WIDTH As String = "Vectogram.Device.Width"
        Public Const DEVICE_HEIGHT As String = "Vectogram.Device.Height"

        <DataFrameColumn(DEVICE_WIDTH)> Dim Width As Integer = 1024
        <DataFrameColumn(DEVICE_HEIGHT)> Dim Height As Integer = 768

        <ExportAPI("Device.Open")>
        Public Function CreateDevice(Optional size As Size = Nothing) As Vectogram
            Throw New NotImplementedException
        End Function

        Public Const COLOR_CREATOR As String = "Color.From.Argb"

        <ExportAPI(VectorAPI.COLOR_CREATOR)>
        Public Function CreateColor(R As Integer, G As Integer, B As Integer, Optional Alpha As Integer = 255) As Color
            Return Color.FromArgb(Alpha, R, G, B)
        End Function
    End Module
End Namespace

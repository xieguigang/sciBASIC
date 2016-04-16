Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports System.Drawing
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

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
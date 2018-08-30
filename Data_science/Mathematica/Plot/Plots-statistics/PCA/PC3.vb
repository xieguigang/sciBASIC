Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Matrix
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports PCA_analysis = Microsoft.VisualBasic.Math.LinearAlgebra.PCA

Namespace PCA

    Partial Module PCAPlot

        ''' <summary>
        ''' 将目标数据通过PCA的方法降维到三维，然后绘制空间散点图
        ''' </summary>
        ''' <param name="input"></param>
        ''' <param name="sampleGroup%"></param>
        ''' <param name="labels$"></param>
        ''' <param name="size$"></param>
        ''' <param name="colorSchema$"></param>
        ''' <returns></returns>
        <Extension> Public Function PC3(input As GeneralMatrix,
                                        sampleGroup%,
                                        Optional labels$() = Nothing,
                                        Optional size$ = "2000,1800",
                                        Optional colorSchema$ = "Set1:c8") As GraphicsData

            Throw New NotImplementedException
        End Function
    End Module
End Namespace
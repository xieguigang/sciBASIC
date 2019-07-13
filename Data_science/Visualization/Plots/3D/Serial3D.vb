#Region "Microsoft.VisualBasic::01b6453c24d88ba62bb8afa00f92de01, Data_science\Visualization\Plots\3D\Serial3D.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class Serial3D
    ' 
    '         Properties: Color, Points, PointSize, Shape, Title
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Imaging.Drawing3D

Namespace Plot3D

    ''' <summary>
    ''' Scatter serial data in 3D
    ''' </summary>
    Public Class Serial3D

        Public Property Title As String
        Public Property Color As Color
        Public Property Shape As LegendStyles

        ''' <summary>
        ''' The point data which tagged a name label, if the label is empty, then will not display on the plot.
        ''' </summary>
        ''' <returns></returns>
        Public Property Points As NamedValue(Of Point3D)()
        Public Property PointSize As Single

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"[{Shape.ToString}, {Color.ToString}] {Title}"
        End Function
    End Class
End Namespace

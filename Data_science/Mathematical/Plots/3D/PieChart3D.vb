Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Imaging.Driver

Namespace Plot3D

    Public Module PieChart3D

        <Extension>
        Public Function Plot3D(data As IEnumerable(Of NamedValue(Of Integer))) As GraphicsData

        End Function

        <Extension>
        Public Function Plot3D(data As IEnumerable(Of Fractions)) As GraphicsData

        End Function
    End Module
End Namespace
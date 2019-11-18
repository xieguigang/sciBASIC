Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.csv.IO

Namespace Outlining

    Public Module OutliningDataLoader

        <Extension>
        Public Function LoadOutlining(Of T As Class)(filepath As String) As IEnumerable(Of T)
            Dim file As File = File.Load(filepath)
            ' 按照列空格进行文件的等级切割
        End Function
    End Module
End Namespace

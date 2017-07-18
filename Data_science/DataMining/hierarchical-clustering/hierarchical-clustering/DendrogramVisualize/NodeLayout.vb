Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering.Hierarchy
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace DendrogramVisualize

    ''' <summary>
    ''' 计算出layout位置信息的结果
    ''' </summary>
    Public Structure NodeLayout

        Dim childs As NodeLayout()
        Dim name$
        Dim distance As Distance
        Dim layout As Coordinate
        
    End Structure
End Namespace
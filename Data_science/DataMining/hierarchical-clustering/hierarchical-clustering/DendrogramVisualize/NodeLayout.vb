#Region "Microsoft.VisualBasic::7b907cebd9dc84aa41100aa5994ed710, Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\DendrogramVisualize\NodeLayout.vb"

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

    '     Structure NodeLayout
    ' 
    ' 
    ' 
    '     Enum Layouts
    ' 
    '         Circular, Horizon
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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

    ''' <summary>
    ''' 层次聚类树的绘制布局枚举
    ''' </summary>
    Public Enum Layouts As Byte
        ''' <summary>
        ''' 默认的竖直的布局
        ''' </summary>
        Vertical = 0
        ''' <summary>
        ''' 水平布局样式
        ''' </summary>
        Horizon
        Circular
    End Enum
End Namespace

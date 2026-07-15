Imports System
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Namespace NetworkEditor.Layout

    ''' <summary>
    ''' 统一布局接口：名称 / 参数对象(绑定 PropertyGrid) / 应用
    ''' </summary>
    Public Interface ILayoutRunner
        ReadOnly Property Name As String
        Function GetParameters() As Object
        Sub Apply(g As NetworkGraph, params As Object, Optional progress As Action(Of String) = Nothing)
    End Interface

End Namespace

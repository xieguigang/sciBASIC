#Region "Microsoft.VisualBasic::341a5a61b041e90b31f9831e07433468, gr\network-visualization\Visualizer\Styling\StyleMapper.vb"

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

    '     Structure StyleMapper
    ' 
    '         Function: (+2 Overloads) __createSelector, (+2 Overloads) FromJSON
    ' 
    '     Delegate Function
    ' 
    ' 
    '     Delegate Function
    ' 
    ' 
    '     Structure StyleCreator
    ' 
    '         Function: CompileSelector
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.Expressions
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Styling

    ''' <summary>
    ''' Network object visualize styling object model
    ''' </summary>
    Public Structure StyleMapper

        Dim nodeStyles As StyleCreator()
        Dim edgeStyles As StyleCreator()

        ''' <summary>
        ''' node label styling
        ''' </summary>
        Dim labelStyles As StyleCreator()

        Public Shared Function FromJSON(json$) As StyleMapper
            Dim styleJSON As StyleJSON = json.LoadJSON(Of StyleJSON)
            Return FromJSON(styleJSON)
        End Function

        Public Shared Function FromJSON(json As StyleJSON) As StyleMapper
            Return New StyleMapper With {
                .nodeStyles = StyleMapper.__createSelector(json.nodes)
            }
        End Function

        Private Shared Function __createSelector(styles As Dictionary(Of String, NodeStyle)) As StyleCreator()
            Return styles _
                .Select(Function(x) __createSelector(x.Key, x.Value)) _
                .ToArray
        End Function

        Private Shared Function __createSelector(selector$, style As NodeStyle) As StyleCreator
            Dim mapper As New StyleCreator With {
                .selector = selector,
                .fill = BrushExpression.Evaluate(style.fill),
                .stroke = Stroke.TryParse(style.stroke),
                .size = SizeExpression.Evaluate(style.size)
            }
            Return mapper
        End Function
    End Structure

    ''' <summary>
    ''' 这个函数描述了这样的一个过程：
    ''' 
    ''' 对一个节点集合进行成员的枚举，然后将每一个成员映射为一个大小数值，并返回这些映射集合
    ''' </summary>
    ''' <param name="node"></param>
    ''' <returns></returns>
    Public Delegate Function GetSize(node As IEnumerable(Of Node)) As IEnumerable(Of Map(Of Node, Single))
    Public Delegate Function GetBrush(node As IEnumerable(Of Node)) As IEnumerable(Of Map(Of Node, Brush))

    Public Structure StyleCreator

        ''' <summary>
        ''' 类似于CSS选择器的字符串表达式
        ''' </summary>
        Dim selector$
        ''' <summary>
        ''' 进行对象绘制的时候的边框样式
        ''' </summary>
        Dim stroke As Pen
        ''' <summary>
        ''' 对象标签的字体
        ''' </summary>
        Dim font As Font
        ''' <summary>
        ''' 对对象进行填充的样式画笔
        ''' </summary>
        Dim fill As GetBrush
        ''' <summary>
        ''' 主要是针对于节点对象的大小直径的获取函数
        ''' </summary>
        Dim size As GetSize

        ''' <summary>
        ''' 从对象之中得到标签字符串的方法函数指针
        ''' </summary>
        Dim label As IStringBuilder

        Public Function CompileSelector() As Func(Of IEnumerable(Of Node), IEnumerable(Of Node))
            Dim expression$ = selector
            Return Function(nodes)
                       Return nodes.[Select](expression, AddressOf SelectNodeValue)
                   End Function
        End Function
    End Structure
End Namespace

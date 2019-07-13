#Region "Microsoft.VisualBasic::3c4353d24abf909ca2603923128841d9, Microsoft.VisualBasic.Core\Language\Linq\Vectorization\Extensions.vb"

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

    '     Module Extensions
    ' 
    '         Function: Add, (+2 Overloads) ToVector
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Language.Vectorization

    <HideModuleName>
    Public Module Extensions

        ' 2019-05-30 因为在这里的向量对象创建函数的名称
        ' 原来是AsVector，会和math模块中的AsVector产生冲突
        ' 所以这里都修改为ToVector

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToVector(booleans As IEnumerable(Of Boolean)) As BooleanVector
            Return New BooleanVector(booleans)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToVector(Of T)(list As IEnumerable(Of T)) As Vector(Of T)
            Return New Vector(Of T)(list)
        End Function

        <Extension>
        Public Function Add(Of T)(x As Vector(Of T), obj As T) As Vector(Of T)
            Call x.Array.Add(obj)
            Return x
        End Function
    End Module
End Namespace

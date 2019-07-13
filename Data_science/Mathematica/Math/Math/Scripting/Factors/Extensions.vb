#Region "Microsoft.VisualBasic::a49dc545e5e58623d65056c7a7aafee9, Data_science\Mathematica\Math\Math\Scripting\Factors\Extensions.vb"

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

    '     Module FactorExtensions
    ' 
    '         Properties: names
    ' 
    '         Function: factors
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace Scripting

    Public Module FactorExtensions

        ''' <summary>
        ''' 这个函数和<see cref="SeqIterator"/>类似，但是这个函数之中添加了去重和排序
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="step">取默认值1是为了保持与Integer类型的index的兼容</param>
        ''' <returns></returns>
        <Extension>
        Public Function factors(Of T As IComparable(Of T))(source As IEnumerable(Of T), Optional step! = 1) As Factor(Of T)()
            Dim array = source.ToArray
            Dim unique As IEnumerable(Of T) = array _
                .Distinct _
                .OrderBy(Function(x) x)
            Dim factorValues As New Dictionary(Of T, Factor(Of T))
            Dim y#

            For Each x As T In unique
                factorValues.Add(x, New Factor(Of T)(x, y))
                y += [step]
            Next

            Dim out As Factor(Of T)() = array _
                .Select(Function(x)
                            Return New Factor(Of T)(x, factorValues(x).Value)
                        End Function) _
                .ToArray

            Return out
        End Function

        Public Property names(vector As IFactorVector) As String()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return vector.index.Keys.ToArray
            End Get
            Set(value As String())
                vector.index = value _
                    .SeqIterator _
                    .ToDictionary(Function(key) key.value,
                                  Function(index) index.i)
            End Set
        End Property
    End Module
End Namespace

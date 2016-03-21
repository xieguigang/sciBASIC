Namespace DFL_Driver

    ''' <summary>
    ''' A node in the fuzzy logic network.(模糊逻辑网络之中的一个节点)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class dflNode

        Protected __factorList As List(Of I_FactorElement)

        Public ReadOnly Property State As Double
            Get
                Dim Factors As Double() = (From item In __factorList Select item.FunctionalState).ToArray '原始数据
                Dim FS As Boolean() = (From n As Double In Factors Select n >= 0).ToArray         '状态数据
                Dim Effect As Boolean = MostAppears(FS)
                Factors = (From n As Double In Factors Where If(Effect = True, n >= 0, n <= 0) Select n).ToArray  '数据筛选

                Dim value As Double = Factors.Sum()  '返回作用值
                Return value
            End Get
        End Property

        ''' <summary>
        ''' 获取当前节点上面的调控因子的数量的总和
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Friend Function get_FactorsCollectionWeight() As Double
            Dim LQuery = (From item In __factorList Select item.Quantity).Sum
            Return LQuery
        End Function

        Sub New(data As Generic.IEnumerable(Of I_FactorElement))
            Me.__factorList = New List(Of I_FactorElement)

            For Each item In data
                Call Me.__factorList.Add(I_FactorElement.ShadowCopy(item, Me))
            Next
        End Sub

        Public Function ToArray() As I_FactorElement()
            Return __factorList.ToArray
        End Function

        Private Shared Function MostAppears(Status As Boolean()) As Boolean
            Dim TLQuery = (From n In Status Where n = True Select 1).ToArray.Length
            Dim FLQuery = Status.Length - TLQuery

            If TLQuery > FLQuery Then  '在细胞群之中激活的数目多余抑制的数目，则在转录组水平上整体呈现激活的状态
                Return True
            Else
                Return False
            End If
        End Function
    End Class
End Namespace
#Region "Microsoft.VisualBasic::5fe28d8fe19788ad638c9172a49963b1, Microsoft.VisualBasic.Core\ComponentModel\Ranges\Selector\NumericTagged.vb"

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

    '     Structure NumericTagged
    ' 
    '         Properties: IValueOf_Value
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+3 Overloads) CompareTo, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.Ranges

    ''' <summary>
    ''' Almost equals to <see cref="DoubleTagged(Of T)"/>, but this object is a structure type. 
    ''' (作用几乎等同于<see cref="DoubleTagged(Of T)"/>，只不过这个是Structure类型，开销会小一些)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Structure NumericTagged(Of T) : Implements IComparable(Of Double), IComparable, IComparable(Of NumericTagged(Of T))
        Implements Value(Of T).IValueOf

        Dim tag#
        Dim value As T

        Private Property IValueOf_Value As T Implements Value(Of T).IValueOf.Value

        Sub New(tag#, value As T)
            Me.tag = tag
            Me.value = value
        End Sub

        Public Overrides Function ToString() As String
            Return $"#{tag} {value.GetJson}"
        End Function

        ''' <summary>
        ''' 比较的是<see cref="Tag"/>属性
        ''' </summary>
        ''' <param name="other"></param>
        ''' <returns></returns>
        Public Function CompareTo(other As Double) As Integer Implements IComparable(Of Double).CompareTo
            Dim d = tag - other

            If d = 0R Then
                Return 0
            Else
                Return Math.Sign(d)
            End If
        End Function

        Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
            If obj Is Nothing Then
                Return 1
            Else
                Dim type = obj.GetType

                If type = GetType(Double) Then
                    Return CompareTo(CDbl(obj))
                ElseIf type = GetType(NumericTagged(Of T)) Then
                    Return CompareTo(DirectCast(obj, NumericTagged(Of T)).tag)
                Else
                    Return 1
                End If
            End If
        End Function

        Public Function CompareTo(other As NumericTagged(Of T)) As Integer Implements IComparable(Of NumericTagged(Of T)).CompareTo
            Return CompareTo(other.tag)
        End Function
    End Structure
End Namespace

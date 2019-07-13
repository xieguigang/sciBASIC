#Region "Microsoft.VisualBasic::c6f0f3dc3e76521301ea697b72859951, Data_science\SVM\SVM\model\LabeledPoint.vb"

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

    '     Class LabeledPoint
    ' 
    '         Properties: ColorClass, X, Y
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Clone, Equals, HasColorClass, ICloneable_Clone, ListEqual
    '                   ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization.JSON
Imports [Class] = Microsoft.VisualBasic.DataMining.ComponentModel.ColorClass

Namespace Model

    ''' <summary>
    ''' һ������������ʵ��
    ''' 
    ''' @author Ralf Wondratschek
    ''' </summary>
    Public Class LabeledPoint : Implements ICloneable

        Sub New(color As [Class])
        End Sub

        Sub New(x As Double, y As Double, clazz As [Class])
            Me.X = New Vector({x, y})
            Me.ColorClass = clazz
        End Sub

        Public ReadOnly Property ColorClass As [Class]

        ''' <summary>
        ''' x
        ''' </summary>
        ''' <returns></returns>
        Public Property X As Vector

        Public ReadOnly Property Y As Integer
            Get
                Return ColorClass.int
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"[{ColorClass}] ({X.ToArray.GetJson})"
        End Function

        Public Shared Function ListEqual(list1 As IList(Of LabeledPoint), list2 As IList(Of LabeledPoint)) As Boolean
            If list1.Count <> list2.Count Then Return False
            For Each p As LabeledPoint In list1
                If Not list2.Contains(p) Then Return False
            Next
            For Each p As LabeledPoint In list2
                If Not list1.Contains(p) Then Return False
            Next
            Return True
        End Function

        Public Shared Function HasColorClass(points As IList(Of LabeledPoint), clazz As ColorClass) As Boolean
            For Each p As LabeledPoint In points
                If p.ColorClass.Equals(clazz) Then
                    Return True
                End If
            Next
            Return False
        End Function

        Public Overrides Function Equals(o As Object) As Boolean
            If TypeOf o Is LabeledPoint Then
                With TryCast(o, LabeledPoint)
                    Return .ColorClass = ColorClass AndAlso (.X = X).All(Function(t) t)
                End With
            End If

            Return MyBase.Equals(o)
        End Function

        Public Function Clone() As LabeledPoint
            Return New LabeledPoint(ColorClass) With {
                .X = X
            }
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return Clone()
        End Function
    End Class
End Namespace

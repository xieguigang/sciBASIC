Imports System.Linq
Imports Microsoft.VisualBasic.DataMining.ComponentModel

Namespace Model

    ''' <summary>
    ''' @author Ralf Wondratschek
    ''' </summary>
    Public Class LabeledPoint : Implements ICloneable

        Sub New(x As Double, y As Double, clazz As ColorClass)
            X1 = x
            X2 = y
            ColorClass = clazz
        End Sub

        Public ReadOnly Property ColorClass As ColorClass
        ''' <summary>
        ''' x
        ''' </summary>
        ''' <returns></returns>
        Public Property X1 As Double
        ''' <summary>
        ''' y
        ''' </summary>
        ''' <returns></returns>
        Public Property X2 As Double
        Public ReadOnly Property Y As Integer
            Get
                Return ColorClass.int
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"[{ColorClass}] ({X1}, {1 - X2})"
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
                Dim point As LabeledPoint = CType(o, LabeledPoint)
                Return point.ColorClass = ColorClass AndAlso
                    point.X1 = X1 AndAlso
                    point.X2 = X2
            End If

            Return MyBase.Equals(o)
        End Function

        Public Function Clone() As LabeledPoint
            Return New LabeledPoint(X1, X2, ColorClass)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return Clone()
        End Function
    End Class
End Namespace
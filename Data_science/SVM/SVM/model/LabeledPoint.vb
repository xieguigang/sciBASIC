Imports System.Linq

Namespace Model

    ''' <summary>
    ''' @author Ralf Wondratschek
    ''' </summary>
    Public Class LabeledPoint : Implements ICloneable

        Private Shared sPool As IList(Of LabeledPoint) = New List(Of LabeledPoint)

        Public Shared Function getInstance(x As Double, y As Double, clazz As ColorClass) As LabeledPoint
            If sPool.Count = 0 Then
                Return New LabeledPoint(x, y, clazz)
            Else
                Dim p As LabeledPoint = sPool.First
                Call sPool.RemoveAt(0)
                p.X1 = x
                p.X2 = y
                p.mColorClass = clazz
                Return p
            End If
        End Function


        Private mColorClass As ColorClass

        Private Shared count As Integer = 0

        Private Sub New(x As Double, y As Double, clazz As ColorClass)
            X1 = x
            X2 = y
            mColorClass = clazz

            count += 1
            ' net.vrallev.android.base.util.L.debug("Points Count " & count)
        End Sub

        Public Property X1 As Double

        Public Property X2 As Double

        Public Overrides Function ToString() As String
            Return $"[{ColorClass}] ({X1}, {1 - X2})"
        End Function

        Public ReadOnly Property ColorClass As ColorClass
            Get
                Return mColorClass
            End Get
        End Property

        Public ReadOnly Property Y As Integer
            Get
                Return mColorClass
            End Get
        End Property

        Public Sub release()
            sPool.Add(Me)
        End Sub

        Public Shared Function listEqual(list1 As IList(Of LabeledPoint), list2 As IList(Of LabeledPoint)) As Boolean
            If list1.Count <> list2.Count Then Return False
            For Each p As LabeledPoint In list1
                If Not list2.Contains(p) Then Return False
            Next
            For Each p As LabeledPoint In list2
                If Not list1.Contains(p) Then Return False
            Next
            Return True
        End Function

        Public Shared Function hasColorClass(points As IList(Of LabeledPoint), clazz As ColorClass) As Boolean
            For Each p As LabeledPoint In points
                If p.ColorClass.Equals(clazz) Then Return True
            Next
            Return False
        End Function

        Public Overrides Function Equals(o As Object) As Boolean
            If TypeOf o Is LabeledPoint Then
                Dim point As LabeledPoint = CType(o, LabeledPoint)
                Return point.mColorClass.Equals(mColorClass) AndAlso point.X1 = X1 AndAlso point.X2 = X2
            End If

            Return MyBase.Equals(o)
        End Function

        Public Function clone() As LabeledPoint
            Return getInstance(X1, X2, mColorClass)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return clone()
        End Function
    End Class

End Namespace
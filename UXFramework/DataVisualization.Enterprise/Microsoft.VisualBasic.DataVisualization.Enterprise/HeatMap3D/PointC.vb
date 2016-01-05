Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports System.Drawing


Namespace Gradiant3D
	Friend Class PointC
		Public pointf As New PointF()
		Public C As Single = 0
		Public ARGBArray As Integer() = New Integer(3) {}

		Public Sub New()
		End Sub

		Public Sub New(ptf As PointF, c__1 As Single)
			pointf = ptf
			C = c__1
		End Sub

		Public Sub New(ptf As PointF, c__1 As Single, argbArray__2 As Integer())
			pointf = ptf
			C = c__1
			ARGBArray = argbArray__2
		End Sub
	End Class
End Namespace

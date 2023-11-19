Namespace BisectingKMeans

	''' <summary>
	''' Created by touhid on 12/21/15.
	''' @author touhid
	''' </summary>
	Public Class DataPoint

		Public Sub New(dx As Double, dy As Double)
			Me.Dx = dx
			Me.Dy = dy
		End Sub

		Public Sub New(dx As Double, dy As Double, clusterNo As Integer)
			Me.Dx = dx
			Me.Dy = dy
			Me.ClusterNo = clusterNo
		End Sub

		Public Overridable Property ClusterNo As Integer

		Public Overridable Property Dx As Double

		Public Overridable Property Dy As Double

		Public Overrides Function ToString() As String
			Return "DataPoint{" & "dx=" & Dx & ", dy=" & Dy & ", clusterNo=" & ClusterNo & "}"c
		End Function
	End Class

End Namespace
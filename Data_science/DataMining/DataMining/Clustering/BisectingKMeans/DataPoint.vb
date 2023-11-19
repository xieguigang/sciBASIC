Namespace BisectingKMeans

	''' <summary>
	''' Created by touhid on 12/21/15.
	''' @author touhid
	''' </summary>
	Public Class DataPoint

		Private dx_Conflict, dy_Conflict As Double
		Private clusterNo_Conflict As Integer

		Public Sub New(ByVal dx As Double, ByVal dy As Double)
			Me.dx_Conflict = dx
			Me.dy_Conflict = dy
		End Sub

		Public Sub New(ByVal dx As Double, ByVal dy As Double, ByVal clusterNo As Integer)
			Me.dx_Conflict = dx
			Me.dy_Conflict = dy
			Me.clusterNo_Conflict = clusterNo
		End Sub

		Public Overridable Property ClusterNo As Integer
			Get
				Return clusterNo_Conflict
			End Get
			Set(ByVal clusterNo As Integer)
				Me.clusterNo_Conflict = clusterNo
			End Set
		End Property


		Public Overridable Property Dx As Double
			Get
				Return dx_Conflict
			End Get
			Set(ByVal dx As Double)
				Me.dx_Conflict = dx
			End Set
		End Property


		Public Overridable Property Dy As Double
			Get
				Return dy_Conflict
			End Get
			Set(ByVal dy As Double)
				Me.dy_Conflict = dy
			End Set
		End Property


		Public Overrides Function ToString() As String
			Return "DataPoint{" & "dx=" & dx_Conflict & ", dy=" & dy_Conflict & ", clusterNo=" & clusterNo_Conflict & "}"c
		End Function
	End Class

End Namespace
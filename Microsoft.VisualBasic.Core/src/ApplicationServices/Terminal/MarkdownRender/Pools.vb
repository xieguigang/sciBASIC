#Region "License Header"
' This Source Code Form is subject to the terms of the Mozilla Public
' License, v. 2.0. If a copy of the MPL was not distributed with this
' file, You can obtain one at https://mozilla.org/MPL/2.0/.
#End Region

Imports System.Collections.Generic
Imports System.Text

Namespace ApplicationServices.Terminal

	Friend NotInheritable Class ListPool(Of T)
		Private ReadOnly pool As New Stack(Of List(Of T))()

		Public Shared ReadOnly [Shared] As New ListPool(Of T)()

		Public Function [Get](ByVal capacity As Integer) As List(Of T)
			Dim result As List(Of T) = Nothing
			SyncLock pool
				If pool.Count > 0 Then
					result = pool.Pop()
				End If
			End SyncLock
			If result Is Nothing Then
				result = New List(Of T)(capacity)
			Else
				If result.Capacity < capacity Then
					result.Capacity = capacity
				End If
			End If
			Return result
		End Function

		Public Sub Put(ByVal list As List(Of T))
			list.Clear()
			SyncLock pool
				pool.Push(list)
			End SyncLock
		End Sub
	End Class

	Friend NotInheritable Class StringBuilderPool
		Private ReadOnly pool As New Stack(Of StringBuilder)()

		Public Shared ReadOnly [Shared] As New StringBuilderPool()

		Public Function [Get](ByVal capacity As Integer) As StringBuilder
			Dim result As StringBuilder = Nothing
			SyncLock pool
				If pool.Count > 0 Then
					result = pool.Pop()
				End If
			End SyncLock
			If result Is Nothing Then
				result = New StringBuilder(capacity)
			Else
				If result.Capacity < capacity Then
					result.Capacity = capacity
				End If
			End If
			Return result
		End Function

		Public Sub Put(ByVal list As StringBuilder)
			list.Clear()
			SyncLock pool
				pool.Push(list)
			End SyncLock
		End Sub
	End Class
End Namespace
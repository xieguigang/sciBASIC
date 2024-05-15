#Region "Microsoft.VisualBasic::4079bb68fd98664bb3cb05db0d71b37f, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ShellProgressBar\Progress.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 37
    '    Code Lines: 31
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 1.31 KB


    ' 	Class Progress
    ' 
    ' 	    Constructor: (+1 Overloads) Sub New
    ' 	    Sub: Dispose, Report
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System

Namespace ApplicationServices.Terminal.ProgressBar.ShellProgressBar
	Friend Class Progress(Of T)
		Implements IProgress(Of T), IDisposable
		Private ReadOnly _progressBar As WeakReference(Of IProgressBar)
		Private ReadOnly _message As Func(Of T, String)
		Private ReadOnly _percentage As Func(Of T, Double?)

		Public Sub New(progressBar As IProgressBar, message As Func(Of T, String), percentage As Func(Of T, Double?))
			_progressBar = New WeakReference(Of IProgressBar)(progressBar)
			_message = message
			_percentage = If(percentage, Function(value) If(CType(CObj(value), Double?), CType(CObj(value), Single?)))
		End Sub

		Public Sub Report(value As T) Implements IProgress(Of T).Report
			Dim progressBar As IProgressBar = Nothing
			If Not _progressBar.TryGetTarget(progressBar) Then Return

			Dim message = _message?.Invoke(value)
			Dim percentage = _percentage(value)
			If percentage.HasValue Then
				progressBar.Tick(percentage * progressBar.MaxTicks, message)
			Else
				progressBar.Tick(message)
			End If
		End Sub

		Public Sub Dispose() Implements IDisposable.Dispose
			Dim progressBar As IProgressBar = Nothing

			If _progressBar.TryGetTarget(progressBar) Then
				progressBar.Dispose()
			End If
		End Sub
	End Class
End Namespace

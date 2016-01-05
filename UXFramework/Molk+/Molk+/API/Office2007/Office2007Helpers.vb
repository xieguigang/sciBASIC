'******************************************************************

'  Office 2007 Renderer Project                                    

'                                                                  

'  Use the Office2007Renderer class as a custom renderer by        

'  providing it to the ToolStripManager.Renderer property. Then    

'  all tool strips, menu strips, status strips etc will be drawn   

'  using the Office 2007 style renderer in your application.       

'                                                                  

'   Author: Phil Wright                                            

'  Website: www.componentfactory.com                               

'  Contact: phil.wright@componentfactory.com                       

'******************************************************************


Imports System.Drawing
Imports System.Drawing.Text
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms

''' <summary>
''' Set the SmoothingMode=AntiAlias until instance disposed.
''' </summary>
Public Class UseAntiAlias
	Implements IDisposable
	#Region "Instance Fields"
	Private _g As Graphics
	Private _old As SmoothingMode
	#End Region

	#Region "Identity"
	''' <summary>
	''' Initialize a new instance of the UseAntiAlias class.
	''' </summary>
	''' <param name="g">Graphics instance.</param>
	Public Sub New(g As Graphics)
		_g = g
		_old = _g.SmoothingMode
		_g.SmoothingMode = SmoothingMode.AntiAlias
	End Sub

	''' <summary>
	''' Revert the SmoothingMode back to original setting.
	''' </summary>
	Public Sub Dispose() Implements IDisposable.Dispose
		_g.SmoothingMode = _old
	End Sub
	#End Region
End Class

''' <summary>
''' Set the TextRenderingHint.ClearTypeGridFit until instance disposed.
''' </summary>
Public Class UseClearTypeGridFit
	Implements IDisposable
	#Region "Instance Fields"
	Private _g As Graphics
	Private _old As TextRenderingHint
	#End Region

	#Region "Identity"
	''' <summary>
	''' Initialize a new instance of the UseClearTypeGridFit class.
	''' </summary>
	''' <param name="g">Graphics instance.</param>
	Public Sub New(g As Graphics)
		_g = g
		_old = _g.TextRenderingHint

		_g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit
	End Sub

	''' <summary>
	''' Revert the TextRenderingHint back to original setting.
	''' </summary>
	Public Sub Dispose() Implements IDisposable.Dispose
		_g.TextRenderingHint = _old
	End Sub
	#End Region
End Class

''' <summary>
''' Set the clipping region until instance disposed.
''' </summary>
Public Class UseClipping
	Implements IDisposable
	#Region "Instance Fields"
	Private _g As Graphics
	Private _old As Region
	#End Region

	#Region "Identity"
	''' <summary>
	''' Initialize a new instance of the UseClipping class.
	''' </summary>
	''' <param name="g">Graphics instance.</param>
	''' <param name="path">Clipping path.</param>
	Public Sub New(g As Graphics, path As GraphicsPath)
		_g = g
		_old = g.Clip
		Dim clip As Region = _old.Clone()
		clip.Intersect(path)
		_g.Clip = clip
	End Sub

	''' <summary>
	''' Initialize a new instance of the UseClipping class.
	''' </summary>
	''' <param name="g">Graphics instance.</param>
	''' <param name="region">Clipping region.</param>
	Public Sub New(g As Graphics, region As Region)
		_g = g
		_old = g.Clip
		Dim clip As Region = _old.Clone()
		clip.Intersect(region)
		_g.Clip = clip
	End Sub

	''' <summary>
	''' Revert clipping back to origina setting.
	''' </summary>
	Public Sub Dispose() Implements IDisposable.Dispose
		_g.Clip = _old
	End Sub
	#End Region
End Class


Imports System.IO
Imports Microsoft.VisualBasic.Net.Http

#If NET48 Then
Imports System.Drawing
#Else
Imports Microsoft.VisualBasic.Imaging
#End If

Namespace Imaging.Driver

    ''' <summary>
    ''' 
    ''' </summary>
    Public Interface GdiRasterGraphics

        ReadOnly Property ImageResource As Image

    End Interface

    Public MustInherit Class IGraphicsData

        ''' <summary>
        ''' The graphics engine driver type indicator, 
        ''' 
        ''' + for <see cref="Drivers.GDI"/> -> <see cref="ImageData"/>(<see cref="Drawing.Image"/>, <see cref="Bitmap"/>)
        ''' + for <see cref="Drivers.SVG"/> -> <see cref="SVGData"/>(<see cref="SVGDocument"/>)
        ''' 
        ''' (驱动程序的类型)
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride ReadOnly Property Driver As Drivers

        ''' <summary>
        ''' http content type
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property content_type As String
            Get
                Select Case Driver
                    Case Drivers.GDI
                        Return "image/png"
                    Case Drivers.PS
                        Return "application/postscript"
                    Case Drivers.SVG
                        Return "image/svg+xml"
                    Case Drivers.WMF
                        Return "application/x-wmf"
                    Case Else
                        Return "application/octet-stream"
                End Select
            End Get
        End Property

        Public MustOverride ReadOnly Property Width As Integer
        Public MustOverride ReadOnly Property Height As Integer

        Public MustOverride Function GetDataURI() As DataURI

        ''' <summary>
        ''' Save the image graphics to file
        ''' </summary>
        ''' <param name="path$"></param>
        ''' <returns></returns>
        Public MustOverride Function Save(path$) As Boolean
        ''' <summary>
        ''' Save the image graphics to a specific output stream
        ''' </summary>
        ''' <param name="out"></param>
        ''' <returns></returns>
        Public MustOverride Function Save(out As Stream) As Boolean

    End Class
End Namespace
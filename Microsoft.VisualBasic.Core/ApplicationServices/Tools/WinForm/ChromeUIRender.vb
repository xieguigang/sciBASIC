#Region "Microsoft.VisualBasic::a52f224044e46fb7dcafc4ecf1739dbf, Microsoft.VisualBasic.Core\ApplicationServices\Tools\WinForm\ChromeUIRender.vb"

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

    '     Class ChromeUIRender
    ' 
    '         Sub: OnRenderMenuItemBackground, OnRenderSeparator
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing

Namespace Windows.Forms

    Public Class ChromeUIRender : Inherits ToolStripSystemRenderer

        ''' <summary>
        ''' Draw MenuItem Background of MBMenuStrip
        ''' </summary>
        Protected Overrides Sub OnRenderMenuItemBackground(e As ToolStripItemRenderEventArgs)
            Dim g As Graphics = e.Graphics
            Dim rect As New Rectangle With {
                .Location = New Point,
                .Size = e.Item.Size
            }

            If e.Item.Selected Then
                g.FillRectangle(__chromeMenuSelected, rect)
            Else
                g.FillRectangle(Brushes.White, rect)
            End If
        End Sub

        ReadOnly __chromeMenuSelected As New SolidBrush(Color.FromArgb(66, 129, 244))
        ReadOnly __chromeSeperator As New Pen(New SolidBrush(Color.FromArgb(233, 233, 233)), 1)

        Protected Overrides Sub OnRenderSeparator(e As ToolStripSeparatorRenderEventArgs)
            Dim h As Integer = e.Item.Height / 2
            Dim layout As New Rectangle With {
                .Location = New Point,
                .Size = New Size With {
                    .Width = e.ToolStrip.Width,
                    .Height = e.Item.Height
                }
            }
            Dim pa As New Point(0, h)
            Dim pb As New Point With {
                .X = e.ToolStrip.Width,
                .Y = h
            }

            e.Graphics.FillRectangle(Brushes.White, layout)
            e.Graphics.DrawLine(__chromeSeperator, pa, pb)
        End Sub
    End Class
End Namespace

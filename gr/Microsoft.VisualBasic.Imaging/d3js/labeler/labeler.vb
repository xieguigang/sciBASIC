Imports System.Drawing

Namespace d3js

    ''' <summary>
    ''' A D3 plug-in for automatic label placement using simulated annealing that 
    ''' easily incorporates into existing D3 code, with syntax mirroring other 
    ''' D3 layouts.
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/tinker10/D3-Labeler
    ''' </remarks>
    Public Class Labeler
        Dim lab As Label()
        Dim anc As Anchor()
        Dim w = 1, h = 1 ' box width/height
        Dim labeler

        Dim max_move As Double = 5
        Dim max_angle As Double = 0.5
        Dim acc As Double = 0
        Dim rej As Double = 0

        ' weights
        Dim w_len As Double = 0.2 ' leader line length 
        Dim w_inter As Double = 1.0 ' leader line intersection
        Dim w_lab2 As Double = 30.0 ' label-label overlap
        Dim w_lab_anc As Double = 30.0 ' label-anchor overlap
        Dim w_orient As Double = 3.0 ' orientation bias

        ' booleans for user defined functions
        Dim user_energy As Boolean = False
        Dim user_schedule As Boolean = False

        Dim user_defined_energy, user_defined_schedule
    End Class
End Namespace

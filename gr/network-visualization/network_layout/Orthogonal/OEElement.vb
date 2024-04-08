' 
'  To change this license header, choose License Headers in Project Properties.
'  To change this template file, choose Tools | Templates
'  and open the template in the editor.
' 
Namespace Orthogonal

    ''' 
    ''' <summary>
    ''' @author santi
    ''' </summary>
    Public Class OEElement

        ' 
        ' 		    // counter clock wise:
        ' 		    public static final int RIGHT = 0;
        ' 		    public static final int UP = 1;
        ' 		    public static final int LEFT = 2;
        ' 		    public static final int DOWN = 3;
        ' 		 
        ' clock wise (in mathematical coordinates, in which y is inverted wrt to screen coordinates):
        Public Shared ReadOnly directionNames As String() = New String() {"right", "down", "left", "up"}
        Public Const RIGHT As Integer = 0
        Public Const DOWN As Integer = 1
        Public Const LEFT As Integer = 2
        Public Const UP As Integer = 3

        Public v As Integer ' the origin node
        Public dest As Integer ' the target node
        Public angle As Integer ' 0 : right, 1: up, 2: left, 3: down
        Public bends As Integer ' number of left bends
        Public sym As OEElement ' the symmetric element in the target node

        Public bendsToAddToSymmetric As Integer

        Public Sub New(a_v As Integer, a_dest As Integer, a_angle As Integer, a_bends As Integer)
            v = a_v
            dest = a_dest
            angle = a_angle
            bends = a_bends
            sym = Nothing

            bendsToAddToSymmetric = 0
        End Sub

        Public Sub New(a_v As Integer, a_dest As Integer, a_angle As Integer, a_bends As Integer, a_sym As OEElement)
            v = a_v
            dest = a_dest
            angle = a_angle
            bends = a_bends
            sym = a_sym

            bendsToAddToSymmetric = 0
        End Sub

        Public Overrides Function ToString() As String
            Dim angles = New String() {"right", "up", "left", "down"}
            Return "(" & v.ToString() & " -> " & dest.ToString() & "," & angles(angle) & "," & bends.ToString() & ")"
        End Function
    End Class

End Namespace

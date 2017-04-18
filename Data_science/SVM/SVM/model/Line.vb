Namespace Model


    ''' <summary>
    ''' @author Ralf Wondratschek
    ''' </summary>
    Public Class Line
        Implements ICloneable

        Private mNormalVector As NormalVector
        Private mOffset As Double

        Public Sub New(startX As Double, startY As Double, endX As Double, endY As Double)
            Dim m As Double = (endY - startY) / (endX - startX)

            mNormalVector = New NormalVector(-1 * m, 1)
            mOffset = startY - m * startX
        End Sub

        Public Sub New(vector As NormalVector, offset As Double)
            mNormalVector = vector
            mOffset = offset
        End Sub

        Public ReadOnly Property Increase As Double
            Get
                Return mNormalVector.W1 * -1 / mNormalVector.W2
            End Get
        End Property

        Public ReadOnly Property Offset As Double
            Get
                Return mOffset
            End Get
        End Property

        Public ReadOnly Property NormalVector As NormalVector
            Get
                Return mNormalVector
            End Get
        End Property

        Public Function getY(x As Double) As Double
            Return Increase * x + Offset
        End Function

        Public Function getX(y As Double) As Double
            Return (y - Offset) / Increase
        End Function

        Public Overrides Function ToString() As String
            Return "y = " & Math.Round(Increase * 100) / 100.0R & " * x + " & Math.Round(Offset * 100) / 100.0R
        End Function

        Public Overrides Function Equals(o As Object) As Boolean
            If TypeOf o Is Line Then
                Dim line As Line = CType(o, Line)
                Dim offset1 As Integer = CInt(Fix(line.mOffset * 1000))
                Dim offset2 As Integer = CInt(Fix(mOffset * 1000))

                Return line.mNormalVector.Equals(mNormalVector) AndAlso offset1 = offset2
            End If

            Return MyBase.Equals(o)
        End Function

        Public Function clone() As Line
            Return New Line(New NormalVector(mNormalVector.W1, mNormalVector.W2), mOffset)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return clone()
        End Function

        Public Class Builder

            Public Sub New(startX As Single, startY As Single, endX As Single, endY As Single)
                startX = startX
                endX = endX
                startY = startY
                endY = endY
            End Sub

            Public Function updateEndPoint(x As Single, y As Single) As Builder
                EndX = x
                EndY = y
                Return Me
            End Function

            Public ReadOnly Property Length As Double
                Get
                    If StartX = EndX AndAlso StartY = EndY Then Return 0

                    Return Math.Sqrt(Math.Pow(StartX - EndX, 2) + Math.Pow(StartY - EndY, 2))
                End Get
            End Property

            Public Property StartX As Single
            Public Property EndX As Single
            Public Property StartY As Single
            Public Property EndY As Single

            Public Function build() As Line
                Return New Line(StartX, StartY, EndX, EndY)
            End Function

            Private Sub flipPoints()
                Dim val As Single = StartX
                StartX = EndX
                EndX = val

                val = StartY
                StartY = EndY
                EndY = val
            End Sub
        End Class
    End Class

End Namespace
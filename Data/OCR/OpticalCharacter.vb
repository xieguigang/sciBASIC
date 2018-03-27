
Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Class OpticalCharacter

    ''' <summary>
    ''' 使用-1分割，按照行进行排列
    ''' </summary>
    Public PixelsVector As Vector
    Public [char] As Char

    Public ReadOnly Property PixelLines As Vector()
        Get
            Return PixelsVector.Split(Function(x) x = -1.0R) _
                               .Select(Function(p) p.AsVector) _
                               .ToArray
        End Get
    End Property

    Sub New()
    End Sub

    Sub New(optical As Image, Optional [char] As Char = Nothing)
        Me.char = [char]
        Me.PixelsVector = optical.ToVector(fillDeli:=True).First
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="test">模板样本数据</param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Compare(test As OpticalCharacter) As Double
        Return SSM(PixelsVector, test.PixelsVector)
    End Function

    Public Overrides Function ToString() As String
        Dim sb As New StringBuilder

        For Each line As Double() In PixelsVector.Split(Function(x) x = -1.0R)
            For Each c As Double In line
                If c = 0R Then
                    Call sb.Append(" ")
                Else
                    Call sb.Append("$")
                End If
            Next

            Call sb.AppendLine()
        Next

        Return sb.ToString
    End Function
End Class
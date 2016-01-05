Imports Microsoft.VisualBasic.CompilerServices
Imports System

Namespace Microsoft.VisualBasic
    <StandardModule> _
    Public NotInheritable Class VBMath
        ' Methods
        Private Shared Function GetTimer() As Single
            Dim now As DateTime = DateTime.Now
            Return CSng((((((60 * now.Hour) + now.Minute) * 60) + now.Second) + (CDbl(now.Millisecond) / 1000)))
        End Function

        Public Shared Sub Randomize()
            Dim timer As Single = VBMath.GetTimer
            Dim projectData As ProjectData = ProjectData.GetProjectData
            Dim rndSeed As Integer = projectData.m_rndSeed
            Dim num3 As Integer = BitConverter.ToInt32(BitConverter.GetBytes(timer), 0)
            num3 = (((num3 And &HFFFF) Xor (num3 >> &H10)) << 8)
            rndSeed = ((rndSeed And -16776961) Or num3)
            projectData.m_rndSeed = rndSeed
        End Sub

        Public Shared Sub Randomize(Number As Double)
            Dim num2 As Integer
            Dim projectData As ProjectData = ProjectData.GetProjectData
            Dim rndSeed As Integer = projectData.m_rndSeed
            If BitConverter.IsLittleEndian Then
                num2 = BitConverter.ToInt32(BitConverter.GetBytes(Number), 4)
            Else
                num2 = BitConverter.ToInt32(BitConverter.GetBytes(Number), 0)
            End If
            num2 = (((num2 And &HFFFF) Xor (num2 >> &H10)) << 8)
            rndSeed = ((rndSeed And -16776961) Or num2)
            projectData.m_rndSeed = rndSeed
        End Sub

        Public Shared Function Rnd() As Single
            Return VBMath.Rnd(1.0!)
        End Function

        Public Shared Function Rnd(Number As Single) As Single
            Dim projectData As ProjectData = ProjectData.GetProjectData
            Dim rndSeed As Integer = projectData.m_rndSeed
            If (Number <> 0) Then
                If (Number < 0) Then
                    Dim num1 As UInt64 = (BitConverter.ToInt32(BitConverter.GetBytes(Number), 0) And &HFFFFFFFF)
                    rndSeed = CInt(((num1 + (num1 >> &H18)) And CULng(&HFFFFFF)))
                End If
                rndSeed = CInt((((rndSeed * &H43FD43FD) + &HC39EC3) And &HFFFFFF))
            End If
            projectData.m_rndSeed = rndSeed
            Return (CSng(rndSeed) / 1.677722E+7!)
        End Function

    End Class
End Namespace


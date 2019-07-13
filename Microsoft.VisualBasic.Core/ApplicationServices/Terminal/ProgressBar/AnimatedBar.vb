#Region "Microsoft.VisualBasic::add5e7662691a588b501cb9803aff443, Microsoft.VisualBasic.Core\ApplicationServices\Terminal\ProgressBar\AnimatedBar.vb"

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

    '     Class AnimatedBar
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: [Step]
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Terminal.ProgressBar

    Public Class AnimatedBar
        Inherits AbstractBar

        Dim animation As List(Of String)
        Dim counter As Integer

        Public Sub New()
            MyBase.New()

            animation = New List(Of String)() From {"/", "-", "\", "|"}
            counter = 0
        End Sub

        ''' <summary>
        ''' prints the character found in the animation according to the current index
        ''' </summary>
        Public Overrides Sub [Step]()
            Console.Write(vbCr)
            Console.Write(animation(counter) & vbBack)

            counter += 1

            If counter = animation.Count Then
                counter = 0
            End If
        End Sub
    End Class
End Namespace

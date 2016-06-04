Namespace Win32

    Public Module PriorityClass

        ''' <summary>
        ''' 当前进程句柄  
        ''' </summary>
        ''' <returns></returns>
        Public Declare Function GetCurrentProcess Lib "kernel32" () As Integer
        Public Declare Function SetPriorityClass Lib "kernel32" (hProcess As Integer, dwPriorityClass As Integer) As Integer

        ''' <summary>
        ''' 新进程应该有非常低的优先级——只有在系统空闲的时候才能运行。基本值是4  
        ''' </summary>
        Public Const IDLE_PRIORITY_CLASS = &H40
        ''' <summary>
        ''' 新进程有非常高的优先级，它优先于大多数应用程序。基本值是13。注意尽量避免采用这个优先级  
        ''' </summary>
        Public Const HIGH_PRIORITY_CLASS = &H80
        ''' <summary>
        ''' 标准优先级。如进程位于前台，则基本值是9；如在后台，则优先值是7  
        ''' </summary>
        Public Const NORMAL_PRIORITY_CLASS = &H20

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="priority"><see cref="IDLE_PRIORITY_CLASS"/>, <see cref="HIGH_PRIORITY_CLASS"/>, <see cref="NORMAL_PRIORITY_CLASS"/></param>
        ''' <returns></returns>
        Public Function PriorityClass(priority As Integer) As Boolean
            Dim CurrentProcesshWnd As Integer = GetCurrentProcess

            If (SetPriorityClass(CurrentProcesshWnd, priority) = 0) Then
                Return False
            Else
                Return True
            End If
        End Function
    End Module
End Namespace
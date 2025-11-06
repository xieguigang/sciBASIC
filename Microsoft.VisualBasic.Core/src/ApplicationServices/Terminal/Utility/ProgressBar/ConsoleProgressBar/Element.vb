' Description: ProgressBar for Console Applications, with advanced features.
' Project site: https://github.com/iluvadev/ConsoleProgressBar
' Issues: https://github.com/iluvadev/ConsoleProgressBar/issues
' License (MIT): https://github.com/iluvadev/ConsoleProgressBar/blob/main/LICENSE
'
' Copyright (c) 2021, iluvadev, and released under MIT License.
'

Namespace ApplicationServices.Terminal.ProgressBar.ConsoleProgressBar
    ''' <summary>
    ''' An element of a ProgressBar
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class Element(Of T)
        Private _ValueGetter As Func(Of ProgressBar, T)
        Private _ForegroundColorGetter As Func(Of ProgressBar, ConsoleColor)
        Private _BackgroundColorGetter As Func(Of ProgressBar, ConsoleColor)
        Private _VisibleGetter As Func(Of ProgressBar, Boolean)

        ''' <summary>
        ''' Ctor
        ''' </summary>
        Public Sub New()
        End Sub

        ''' <summary>
        ''' Ctor
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="foregroundColor"></param>
        ''' <param name="backgroundColor"></param>
        Public Sub New(value As T, foregroundColor As ConsoleColor, backgroundColor As ConsoleColor)
            SetValue(value)
            SetForegroundColor(foregroundColor)
            SetBackgroundColor(backgroundColor)
        End Sub

        ''' <summary>
        ''' Sets the ProgressBar element visible (or not)
        ''' </summary>
        ''' <param name="show"></param>
        ''' <returns></returns>
        Public Function SetVisible(show As Boolean) As Element(Of T)
            Return SetVisible(Function(pb) show)
        End Function

        ''' <summary>
        ''' Sets the ProgressBar element visible (or not)
        ''' </summary>
        ''' <param name="showGetter"></param>
        ''' <returns></returns>
        Public Function SetVisible(showGetter As Func(Of ProgressBar, Boolean)) As Element(Of T)
            _VisibleGetter = showGetter
            Return Me
        End Function
        ''' <summary>
        ''' Gets the ProgressBar element visibility
        ''' </summary>
        ''' <param name="progressBar"></param>
        ''' <returns></returns>
        Public Function GetVisible(progressBar As ProgressBar) As Boolean
            Return If(_VisibleGetter?.Invoke(progressBar), True)
        End Function

        ''' <summary>
        ''' Sets the Value of the ProgressBar element
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Public Function SetValue(value As T) As Element(Of T)
            Return SetValue(Function(pb) value)
        End Function

        ''' <summary>
        ''' Sets the Value of the ProgressBar element
        ''' </summary>
        ''' <param name="valueGetter"></param>
        ''' <returns></returns>
        Public Function SetValue(valueGetter As Func(Of ProgressBar, T)) As Element(Of T)
            _ValueGetter = valueGetter
            Return Me
        End Function

        ''' <summary>
        ''' Gets the Value of the ProgressBar element
        ''' </summary>
        ''' <param name="progressBar"></param>
        ''' <returns></returns>
        Public Overridable Function GetValue(progressBar As ProgressBar) As T
            Return If(GetVisible(progressBar) AndAlso _ValueGetter IsNot Nothing, _ValueGetter.Invoke(progressBar), Nothing)
        End Function

        ''' <summary>
        ''' Sets the ForegroundColor of the ProgressBar element
        ''' </summary>
        ''' <param name="foregroundColor"></param>
        ''' <returns></returns>
        Public Function SetForegroundColor(foregroundColor As ConsoleColor) As Element(Of T)
            Return SetForegroundColor(Function(pb) foregroundColor)
        End Function

        ''' <summary>
        ''' Sets the ForegroundColor of the ProgressBar element
        ''' </summary>
        ''' <param name="foregroundColorGetter"></param>
        ''' <returns></returns>
        Public Function SetForegroundColor(foregroundColorGetter As Func(Of ProgressBar, ConsoleColor)) As Element(Of T)
            _ForegroundColorGetter = foregroundColorGetter
            Return Me
        End Function

        ''' <summary>
        ''' Gets the ForegroundColor of the ProgressBar element
        ''' </summary>
        ''' <param name="progressBar"></param>
        ''' <returns></returns>
        Public Overridable Function GetForegroundColor(progressBar As ProgressBar) As ConsoleColor?
            Return If(GetVisible(progressBar) AndAlso _ForegroundColorGetter IsNot Nothing, _ForegroundColorGetter.Invoke(progressBar), CType(Nothing, ConsoleColor?))
        End Function

        ''' <summary>
        ''' Sets the BackgroundColor of the ProgressBar element
        ''' </summary>
        ''' <param name="backgroundColor"></param>
        ''' <returns></returns>
        Public Function SetBackgroundColor(backgroundColor As ConsoleColor) As Element(Of T)
            Return SetBackgroundColor(Function(pb) backgroundColor)
        End Function

        ''' <summary>
        ''' Sets the BackgroundColor of the ProgressBar element
        ''' </summary>
        ''' <param name="backgroundColorGetter"></param>
        ''' <returns></returns>
        Public Function SetBackgroundColor(backgroundColorGetter As Func(Of ProgressBar, ConsoleColor)) As Element(Of T)
            _BackgroundColorGetter = backgroundColorGetter
            Return Me
        End Function

        ''' <summary>
        ''' Gets the BackgroundColor of the ProgressBar element
        ''' </summary>
        ''' <param name="progressBar"></param>
        ''' <returns></returns>
        Public Overridable Function GetBackgroundColor(progressBar As ProgressBar) As ConsoleColor?
            Return If(GetVisible(progressBar) AndAlso _BackgroundColorGetter IsNot Nothing, _BackgroundColorGetter.Invoke(progressBar), CType(Nothing, ConsoleColor?))
        End Function
    End Class

End Namespace

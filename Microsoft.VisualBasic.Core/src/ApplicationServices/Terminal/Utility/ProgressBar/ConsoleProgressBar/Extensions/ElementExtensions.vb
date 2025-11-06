' Description: ProgressBar for Console Applications, with advanced features.
' Project site: https://github.com/iluvadev/ConsoleProgressBar
' Issues: https://github.com/iluvadev/ConsoleProgressBar/issues
' License (MIT): https://github.com/iluvadev/ConsoleProgressBar/blob/main/LICENSE
'
' Copyright (c) 2021, iluvadev, and released under MIT License.
'

Imports System
Imports System.Collections.Generic
Imports System.Runtime.CompilerServices

Namespace iluvadev.ConsoleProgressBar.Extensions
    ''' <summary>
    ''' Extensions for Element
    ''' </summary>
    Public Module ElementExtensions
        ''' <summary>
        ''' Returns a list of Actions to write the element in Console
        ''' </summary>
        ''' <param name="element"></param>
        ''' <param name="progressBar"></param>
        ''' <param name="valueTransformer">Function to Transform the value before write</param>
        ''' <returns></returns>
        <Extension()>
        Public Function GetRenderActions(element As Element(Of String), progressBar As ProgressBar, Optional valueTransformer As Func(Of String, String) = Nothing) As List(Of Action)
            Dim list = New List(Of Action)()

            If progressBar Is Nothing OrElse element Is Nothing OrElse Not element.GetVisible(progressBar) Then Return list

            Dim foregroundColor = element.GetForegroundColor(progressBar)
            If foregroundColor.HasValue Then list.Add(Sub() Console.ForegroundColor = foregroundColor.Value)

            Dim backgroundColor = element.GetBackgroundColor(progressBar)
            If backgroundColor.HasValue Then list.Add(Sub() Console.BackgroundColor = backgroundColor.Value)

            Dim value = element.GetValue(progressBar)
            If valueTransformer IsNot Nothing Then value = valueTransformer.Invoke(value)

            list.Add(Sub() Console.Write(value))
            list.Add(Sub() Console.ResetColor())

            Return list
        End Function

        ''' <summary>
        ''' Returns a list of Actions to write the element in Console
        ''' </summary>
        ''' <param name="element"></param>
        ''' <param name="progressBar"></param>
        ''' <param name="repetition"></param>
        ''' <returns></returns>
        <Extension()>
        Public Function GetRenderActions(element As Element(Of Char), progressBar As ProgressBar, Optional repetition As Integer = 1) As List(Of Action)
            Dim list = New List(Of Action)()

            If progressBar Is Nothing OrElse repetition < 1 OrElse element Is Nothing OrElse Not element.GetVisible(progressBar) Then Return list

            Dim foregroundColor = element.GetForegroundColor(progressBar)
            If foregroundColor.HasValue Then list.Add(Sub() Console.ForegroundColor = foregroundColor.Value)

            Dim backgroundColor = element.GetBackgroundColor(progressBar)
            If backgroundColor.HasValue Then list.Add(Sub() Console.BackgroundColor = backgroundColor.Value)

            Dim value = element.GetValue(progressBar)
            list.Add(Sub() Console.Write(New String(value, repetition)))
            list.Add(Sub() Console.ResetColor())

            Return list
        End Function
    End Module

End Namespace

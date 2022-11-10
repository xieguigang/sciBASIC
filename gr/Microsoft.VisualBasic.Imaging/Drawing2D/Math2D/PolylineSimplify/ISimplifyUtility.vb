' High-performance polyline simplification library
'
' This is a port of simplify-js by Vladimir Agafonkin, Copyright (c) 2012
' https://github.com/mourner/simplify-js
' 
' The code is ported from JavaScript to C#.
' The library is created as portable and 
' is targeting multiple Microsoft plattforms.
'
' This library was ported by imshz @ http://www.shz.no
' https://github.com/imshz/simplify-net
'
' This code is provided as is by the author. For complete license please
' read the original license at https://github.com/mourner/simplify-js

Namespace Drawing2D.Math2D.PolylineSimplify

    Public Interface ISimplifyUtility

        ''' <summary>
        ''' Simplifies a list of points to a shorter list of points.
        ''' </summary>
        ''' <param name="points">Points original list of points</param>
        ''' <param name="tolerance">Tolerance tolerance in the same measurement as the point coordinates</param>
        ''' <param name="highestQuality">Enable highest quality for using Douglas-Peucker, 
        ''' set false for Radial-Distance algorithm</param>
        ''' <returns>Simplified list of points</returns>
        Function Simplify(points As Point(),
                          Optional tolerance As Double = 0.3,
                          Optional highestQuality As Boolean = False) As List(Of Point)

    End Interface
End Namespace

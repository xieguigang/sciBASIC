# GDITransform
_namespace: [Microsoft.VisualBasic.Imaging](./index.md)_





### Methods

#### Centre
```csharp
Microsoft.VisualBasic.Imaging.GDITransform.Centre(System.Drawing.RectangleF)
```
Gets the center location of the region rectangle.

|Parameter Name|Remarks|
|--------------|-------|
|rect|-|


#### RotateImage
```csharp
Microsoft.VisualBasic.Imaging.GDITransform.RotateImage(System.Drawing.Image,System.Single)
```
Creates a new Image containing the same image only rotated

|Parameter Name|Remarks|
|--------------|-------|
|image|The @``T:System.Drawing.Image`` to rotate|
|angle|The amount to rotate the image, clockwise, in degrees|


_returns: A new @``T:System.Drawing.Bitmap`` that is just large enough
 to contain the rotated image without cutting any corners off._
> 
>  
>  Explaination of the calculations
> 
>  The trig involved in calculating the new width and height
>  is fairly simple; the hard part was remembering that when 
>  PI/2 <= theta <= PI and 3PI/2 <= theta < 2PI the width and 
>  height are switched.
>   
>  When you rotate a rectangle, r, the bounding box surrounding r
>  contains for right-triangles of empty space.  Each of the 
>  triangles hypotenuse's are a known length, either the width or
>  the height of r.  Because we know the length of the hypotenuse
>  and we have a known angle of rotation, we can use the trig
>  function identities to find the length of the other two sides.
>   
>  sine = opposite/hypotenuse
>  cosine = adjacent/hypotenuse
>   
>  solving for the unknown we get
>   
>  opposite = sine * hypotenuse
>  adjacent = cosine * hypotenuse
>   
>  Another interesting point about these triangles is that there
>  are only two different triangles. The proof for which is easy
>  to see, but its been too long since I've written a proof that
>  I can't explain it well enough to want to publish it.  
>   
>  Just trust me when I say the triangles formed by the lengths 
>  width are always the same (for a given theta) and the same 
>  goes for the height of r.
>   
>  Rather than associate the opposite/adjacent sides with the
>  width and height of the original bitmap, I'll associate them
>  based on their position.
>   
>  adjacent/oppositeTop will refer to the triangles making up the 
>  upper right and lower left corners
>   
>  adjacent/oppositeBottom will refer to the triangles making up 
>  the upper left and lower right corners
>   
>  The names are based on the right side corners, because thats 
>  where I did my work on paper (the right side).
>   
>  Now if you draw this out, you will see that the width of the 
>  bounding box is calculated by adding together adjacentTop and 
>  oppositeBottom while the height is calculate by adding 
>  together adjacentBottom and oppositeTop.
>  
>  



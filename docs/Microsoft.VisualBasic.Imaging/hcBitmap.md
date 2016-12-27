# hcBitmap
_namespace: [Microsoft.VisualBasic.Imaging](./index.md)_





### Methods

#### Binarization
```csharp
Microsoft.VisualBasic.Imaging.hcBitmap.Binarization(System.Drawing.Bitmap@,Microsoft.VisualBasic.Imaging.hcBitmap.BinarizationStyles)
```


|Parameter Name|Remarks|
|--------------|-------|
|curBitmap|-|

> 
>  http://www.codeproject.com/Articles/1094534/Image-Binarization-Using-Program-Languages
>  
>  The .net Bitmap object keeps a reference to HBITMAP handle, Not to the underlying bitmap itself.
>  So, single pixel access call to @``M:System.Drawing.Bitmap.SetPixel(System.Int32,System.Int32,System.Drawing.Color)``/@``M:System.Drawing.Bitmap.GetPixel(System.Int32,System.Int32)`` Or 
>  even retrieve Width/Height properties does something Like: 
>  lock handle In place-Get/Set value/unlock handle. It Is the most inefficient way To manipulate bitmaps In .NET. 
>  The author should read about @``M:System.Drawing.Bitmap.LockBits(System.Drawing.Rectangle,System.Drawing.Imaging.ImageLockMode,System.Drawing.Imaging.PixelFormat)`` first.
>  



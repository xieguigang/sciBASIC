# Delegates

Easy way to create delegates for obtaining/using all member types (fields, properties, indexers, methods, constructors and events) of all types (public and non public) and all visibilities (public, internal, protected and private)

> Original works from Github: https://github.com/npodbielski/Delegates

Code is written as an example for articles:

+ http://internetexception.com/post/2016/08/05/Faster-then-Reflection-Delegates.aspx
+ http://internetexception.com/post/2016/08/16/Faster-than-Reflection-Delegates-Part-2.aspx
+ http://internetexception.com/post/2016/09/02/Faster-than-Reflection-Delegates-Part-3.aspx

Articles are also available on CodeProject:

+ http://www.codeproject.com/Articles/1118828/Faster-than-Reflection-Delegates-Part - part 1
+ http://www.codeproject.com/Articles/1124966/Faster-than-Reflection-Delegates-Part - part 2
+ http://www.codeproject.com/Articles/1124863/Faster-than-Reflection-Delegates-Part - part 3

## Examples

Below few examples of use of DelegateFactory

### Constructors

```vbnet
Dim cd = DelegateFactory.DefaultContructor(Of TestClass)()
Dim c1 = DelegateFactory.Contructor(Of Func(Of TestClass))()
Dim c2 = DelegateFactory.Contructor(Of Func(Of int, TestClass))()
Dim c3 = Type.Contructor(GetType(Microsoft.VisualBasic.Language.int))
```

### Static Properties

#### Getters

```vbnet
Dim spg1 = DelegateFactory.StaticPropertyGet(Of TestClass, String)("StaticPublicProperty")
Dim spg2 = Type.StaticPropertyGet(Of String)("StaticPublicProperty")
Dim spg3 = Type.StaticPropertyGet("StaticPublicProperty")
```

#### Setters

```vbnet
Dim sps1 = DelegateFactory.StaticPropertySet(Of TestClass, String)("StaticPublicProperty")
Dim sps2 = Type.StaticPropertySet(Of String)("StaticPublicProperty")
Dim sps3 = Type.StaticPropertySet("StaticPublicProperty")
```

### Instance Properties

#### Getters

```vbnet
Dim pg1 = DelegateFactory.PropertyGet(Of TestClass, String)("PublicProperty")
Dim pg2 = Type.PropertyGet(Of String)("PublicProperty")
Dim pg3 = Type.PropertyGet("PublicProperty")
```

#### Setters

```vbnet
Dim ps1 = DelegateFactory.PropertySet(Of TestClass, String)("PublicProperty")
Dim ps2 = Type.PropertySet(Of String)("PublicProperty")
Dim ps3 = Type.PropertySet("PublicProperty")
```

### Indexers

#### Getters

```vbnet
Dim ig1 = DelegateFactory.IndexerGet(Of TestClass, int, int)()
Dim ig2 = Type.IndexerGet(Of int, int)()
Dim ig3 = Type.IndexerGet(GetType(int), GetType(int))
```

#### Setters

```vbnet
Dim is1 = DelegateFactory.IndexerSet(Of TestClass, int, int)()
Dim is2 = Type.IndexerSet(GetType(int), GetType(int))
Dim is3 = Type.IndexerSet(Of int, int)()
```

### Static Fields

#### Get value

```vbnet
Dim sfg1 = DelegateFactory.StaticFieldGet(Of TestClass, String)("StaticPublicField")
Dim sfg2 = Type.StaticFieldGet(Of String)("StaticPublicField")
Dim sfg3 = Type.StaticFieldGet("StaticPublicField")
```

#### Set value

```vbnet
Dim sfs1 = DelegateFactory.StaticFieldSet(Of TestClass, String)("StaticPublicField")
Dim sfs2 = Type.StaticFieldSet(Of String)("StaticPublicField")
Dim sfs3 = Type.StaticFieldSet("StaticPublicField")
```

### Instance Fields

#### Get value

```vbnet
Dim fg1 = DelegateFactory.FieldGet(Of TestClass, String)("PublicField")
Dim fg2 = Type.FieldGet(Of String)("PublicField")
Dim fg3 = Type.FieldGet("PublicField")
```

#### Set value

```vbnet
Dim fs1 = DelegateFactory.FieldSet(Of TestClass, String)("PublicField")
Dim fs2 = Type.FieldSet(Of String)("PublicField")
Dim fs3 = Type.FieldSet("PublicField")
```

### Static Methods

```vbnet
Dim sm1 = DelegateFactory.StaticMethod(Of TestClass, Func(Of String, String))("StaticPublicMethod")
Dim sm2 = Type.StaticMethod(Of Func(Of String, String))("StaticPublicMethod")
Dim sm3 = Type.StaticMethod("StaticPublicMethod", GetType(String))
Dim sm4 = Type.StaticMethodVoid("StaticPublicMethodVoid", GetType(String))
```

### Instance Methods

```vbnet
Dim m1 = DelegateFactory.InstanceMethod(Of Func(Of TestClass, String, String))("PublicMethod")
Dim m2 = DelegateFactory.InstanceMethod2(Of Func(Of TestClass, String, String))("PublicMethod")
Dim m3 = Type.InstanceMethod("PublicMethod", GetType(String))
Dim m4 = Type.InstanceMethodVoid("PublicMethodVoid", GetType(String))
```

### Generic Methods

#### Static

```vbnet
Dim sg1 = DelegateFactory.StaticMethod(Of TestClass, Func(Of TestClass, TestClass), TestClass)("StaticGenericMethod")
Dim sg2 = Type.StaticMethod(Of Func(Of TestClass, TestClass), TestClass)("StaticGenericMethod")
Dim sg3 = Type.StaticGenericMethod("StaticGenericMethod", { Type }, { Type })
Dim sg4 = Type.StaticGenericMethodVoid("StaticGenericMethodVoid", { Type }, { Type })
```

#### Instance

```vbnet
Dim ig1 = DelegateFactory.InstanceMethod(Of Func(Of TestClass, TestClass, TestClass), TestClass)("GenericMethod")
Dim ig2 = Type.InstanceMethod(Of Func(Of TestClass, TestClass, TestClass), TestClass)("GenericMethod")
Dim ig3 = Type.InstanceMethod(Of Func(Of object, TestClass, TestClass), TestClass)("GenericMethod")
Dim ig4 = Type.InstanceGenericMethod("GenericMethod", { Type }, { Type })
Dim ig5 = Type.InstanceGenericMethodVoid("GenericMethodVoid", { Type }, { Type })
```

### Events

#### Add Accessors

```vbnet
Dim ea1 = DelegateFactory.EventAdd(Of TestClass, TestClass.PublicEventArgs)("PublicEvent")
Dim ea2 = Type.EventAdd(Of TestClass.PublicEventArgs)("PublicEvent")
Dim ea3 = Type.EventAdd("PrivateEvent")
```

#### Remove Accessors

```vbnet
Dim er1 = DelegateFactory.EventRemove(Of TestClass, TestClass.PublicEventArgs)("PublicEvent")
Dim er4 = Type.EventRemove(Of TestClass.PublicEventArgs)("PublicEvent")
Dim er5 = Type.EventRemove("PrivateEvent")
```
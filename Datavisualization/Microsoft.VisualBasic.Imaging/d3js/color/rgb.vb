Imports Microsoft.VisualBasic.Imaging.d3.color.rgb

Namespace d3.color

    Public Module rgb

        Public Function d3_rgb_hex(v As Byte) As String
            Return If(v < &H10, "0" & Math.Max(CByte(0), v).ToString(16), Math.Min(CByte(255), v).ToString(16))
        End Function

        '        import "../arrays/map";
        'import "color";
        'import "hsl";
        'import "lab";
        'import "xyz";

        Function d3_rgb(r, g, b)
            '  Return this instanceof d3_rgb ? void (this.r = ~~r, this.g = ~~g, this.b = ~~b)
            '       arguments.length <2 ? (r instanceof d3_rgb ? new d3_rgb(r.r, r.g, r.b)
            '      : d3_rgb_parse("" + r, d3_rgb, d3_hsl_rgb))
            '      : new d3_rgb(r, g, b);
            Throw New NotImplementedException
        End Function

        Public Function d3_rgbNumber(value)
            '     Return New d3_rgb_names(value >> 16, value >> 8 & &HFF, value & &HFF)
            Throw New NotImplementedException
        End Function

        Function d3_rgbString(value)
            Return d3_rgbNumber(value) & ""
        End Function

        'var d3_rgbPrototype = d3_rgb.prototype = New d3_color;

        'd3_rgbPrototype.brighter = Function(k) {
        '  k = Math.Pow(0.7, arguments.length ? k : 1);
        '  var r = this.r,
        '      g = this.g,
        '      b = this.b,
        '      i = 30;
        '  If (!r &&!g &&!b) Then Return New d3_rgb(i, i, i) ;
        '  If (r && r < i) Then r = i;
        '  If (g && g < i) Then g = i;
        '  If (b && b < i) Then b = i;
        '  Return New d3_rgb(Math.Min(255, r / k), Math.Min(255, g / k), Math.Min(255, b / k));
        '};

        'd3_rgbPrototype.darker = Function(k) {
        '  k = Math.Pow(0.7, arguments.length ? k : 1);
        '  Return New d3_rgb(k * this.r, k * this.g, k * this.b);
        '};

        'd3_rgbPrototype.hsl = Function() {
        '  return d3_rgb_hsl(this.r, this.g, this.b);
        '};

        'd3_rgbPrototype.toString = Function() {
        '  return "#" + d3_rgb_hex(this.r) + d3_rgb_hex(this.g) + d3_rgb_hex(this.b);
        '};

        'Function d3_rgb_hex(v) {
        '  Return v < &H10
        '            ? "0" + Math.Max(0, v).ToString(16)
        '            Math.Min(255, v).ToString(16);
        '}

        'Function d3_rgb_parse(format, rgb, hsl) {
        '  var r = 0, // red channel; int In [0, 255]
        '      g = 0, // green channel; int In [0, 255]
        '      b = 0, // blue channel; int In [0, 255]
        '      m1, // CSS color specification match
        '      m2, // CSS color specification type (e.g., rgb)
        '      color;

        '  /* Handle hsl, rgb. */
        '  m1 = / ([a-z]+)\((.*)\)/.exec(format = format.toLowerCase());
        '  If (m1) Then {
        '    m2 = m1[2].split(",");
        '    Switch(m1[1]) {
        '      Case "hsl" :   {
        '        Return hsl(
        '          parseFloat(m2[0]), // degrees
        '          parseFloat(m2[1]) / 100, // percentage
        '          parseFloat(m2[2]) / 100 // percentage
        '        );
        '      }
        '      Case "rgb" :   {
        '        Return rgb(
        '          d3_rgb_parseNumber(m2[0]),
        '          d3_rgb_parseNumber(m2[1]),
        '          d3_rgb_parseNumber(m2[2])
        '        );
        '      }
        '    }
        '  }

        '  /* Named colors. */
        '  If (color = d3_rgb_names.get(format)) Then {
        '    Return rgb(color.r, color.g, color.b);
        '  }

        '  /* Hexadecimal colors #rgb And #rrggbb. */
        '  If (format! = Null && format.charAt(0) === "#" &&!isNaN(color = parseInt(format.slice(1), 16))) Then {
        '    If (format.length === 4) Then {
        '      r = (color & &HF00) >> 4; r = (r >> 4) | r;
        '      g = (color & &HF0); g = (g >> 4) | g;
        '      b = (color & &HF); b = (b << 4) | b;
        '    } else if (format.length === 7) {
        '      r = (color & &HFF0000) >> 16;
        '      g = (color & &HFF00) >> 8;
        '      b = (color & &HFF);
        '    }
        '  }

        '  Return rgb(r, g, b);
        '}

        'Public Function d3_rgb_hsl(r, g, b)
        '            Dim min = Math.Min(r /= 255, g /= 255, b /= 255),
        '      Max = Math.Max(r, g, b),
        '      d = Max - min,
        '      h,
        '      s,
        '      l = (Max() + min) / 2
        '            If (d) Then {
        '    s = l < 0.5 ? d / (max + min) : d / (2 - Max() - min);
        '    If (r == Max()) Then h = (g - b) / d + (g < b ? 6 : 0);
        '    ElseIf (g == Max()) Then h = (b - r) / d + 2;
        '    Else h = (r - g) / d + 4;
        '    h *= 60;
        '  } else {
        '    h = NaN;
        '    s = l > 0 && l < 1 ? 0 : h
        '            End If
        '            Return New d3_hsl(h, s, l)
        '        End Function

        '        Function d3_rgb_lab(r, g, b) {
        '  r = d3_rgb_xyz(r);
        '  g = d3_rgb_xyz(g);
        '  b = d3_rgb_xyz(b);
        '  var x = d3_xyz_lab((0.4124564 * r + 0.3575761 * g + 0.1804375 * b) / d3_lab_X),
        '      y = d3_xyz_lab((0.2126729 * r + 0.7151522 * g + 0.072175 * b) / d3_lab_Y),
        '      Z = d3_xyz_lab((0.0193339 * r + 0.119192 * g + 0.9503041 * b) / d3_lab_Z);
        '  Return d3_lab(116 * y - 16, 500 * (x - y), 200 * (y - Z));
        '}

        'Function d3_rgb_xyz(r) {
        '  Return (r /= 255) <= 0.04045 ? r / 12.92 : Math.Pow((r + 0.055) / 1.055, 2.4);
        '}

        'Function d3_rgb_parseNumber(c) { // either Integer Or percentage
        '  var f = parseFloat(c);
        '  Return c.charAt(c.length - 1) === "%" ? Math.round(f * 2.55) : f;
        '}

        Public Enum d3_rgb_names
            aliceblue = &HF0F8FF
            antiquewhite = &HFAEBD7
            aqua = &HFFFF
            aquamarine = &H7FFFD4
            azure = &HF0FFFF
            beige = &HF5F5DC
            bisque = &HFFE4C4
            black = &H0
            blanchedalmond = &HFFEBCD
            blue = &HFF
            blueviolet = &H8A2BE2
            brown = &HA52A2A
            burlywood = &HDEB887
            cadetblue = &H5F9EA0
            chartreuse = &H7FFF00
            chocolate = &HD2691E
            coral = &HFF7F50
            cornflowerblue = &H6495ED
            cornsilk = &HFFF8DC
            crimson = &HDC143C
            cyan = &HFFFF
            darkblue = &H8B
            darkcyan = &H8B8B
            darkgoldenrod = &HB8860B
            darkgray = &HA9A9A9
            darkgreen = &H6400
            darkgrey = &HA9A9A9
            darkkhaki = &HBDB76B
            darkmagenta = &H8B008B
            darkolivegreen = &H556B2F
            darkorange = &HFF8C00
            darkorchid = &H9932CC
            darkred = &H8B0000
            darksalmon = &HE9967A
            darkseagreen = &H8FBC8F
            darkslateblue = &H483D8B
            darkslategray = &H2F4F4F
            darkslategrey = &H2F4F4F
            darkturquoise = &HCED1
            darkviolet = &H9400D3
            deeppink = &HFF1493
            deepskyblue = &HBFFF
            dimgray = &H696969
            dimgrey = &H696969
            dodgerblue = &H1E90FF
            firebrick = &HB22222
            floralwhite = &HFFFAF0
            forestgreen = &H228B22
            fuchsia = &HFF00FF
            gainsboro = &HDCDCDC
            ghostwhite = &HF8F8FF
            gold = &HFFD700
            goldenrod = &HDAA520
            gray = &H808080
            green = &H8000
            greenyellow = &HADFF2F
            grey = &H808080
            honeydew = &HF0FFF0
            hotpink = &HFF69B4
            indianred = &HCD5C5C
            indigo = &H4B0082
            ivory = &HFFFFF0
            khaki = &HF0E68C
            lavender = &HE6E6FA
            lavenderblush = &HFFF0F5
            lawngreen = &H7CFC00
            lemonchiffon = &HFFFACD
            lightblue = &HADD8E6
            lightcoral = &HF08080
            lightcyan = &HE0FFFF
            lightgoldenrodyellow = &HFAFAD2
            lightgray = &HD3D3D3
            lightgreen = &H90EE90
            lightgrey = &HD3D3D3
            lightpink = &HFFB6C1
            lightsalmon = &HFFA07A
            lightseagreen = &H20B2AA
            lightskyblue = &H87CEFA
            lightslategray = &H778899
            lightslategrey = &H778899
            lightsteelblue = &HB0C4DE
            lightyellow = &HFFFFE0
            lime = &HFF00
            limegreen = &H32CD32
            linen = &HFAF0E6
            magenta = &HFF00FF
            maroon = &H800000
            mediumaquamarine = &H66CDAA
            mediumblue = &HCD
            mediumorchid = &HBA55D3
            mediumpurple = &H9370DB
            mediumseagreen = &H3CB371
            mediumslateblue = &H7B68EE
            mediumspringgreen = &HFA9A
            mediumturquoise = &H48D1CC
            mediumvioletred = &HC71585
            midnightblue = &H191970
            mintcream = &HF5FFFA
            mistyrose = &HFFE4E1
            moccasin = &HFFE4B5
            navajowhite = &HFFDEAD
            navy = &H80
            oldlace = &HFDF5E6
            olive = &H808000
            olivedrab = &H6B8E23
            orange = &HFFA500
            orangered = &HFF4500
            orchid = &HDA70D6
            palegoldenrod = &HEEE8AA
            palegreen = &H98FB98
            paleturquoise = &HAFEEEE
            palevioletred = &HDB7093
            papayawhip = &HFFEFD5
            peachpuff = &HFFDAB9
            peru = &HCD853F
            pink = &HFFC0CB
            plum = &HDDA0DD
            powderblue = &HB0E0E6
            purple = &H800080
            rebeccapurple = &H663399
            red = &HFF0000
            rosybrown = &HBC8F8F
            royalblue = &H4169E1
            saddlebrown = &H8B4513
            salmon = &HFA8072
            sandybrown = &HF4A460
            seagreen = &H2E8B57
            seashell = &HFFF5EE
            sienna = &HA0522D
            silver = &HC0C0C0
            skyblue = &H87CEEB
            slateblue = &H6A5ACD
            slategray = &H708090
            slategrey = &H708090
            snow = &HFFFAFA
            springgreen = &HFF7F
            steelblue = &H4682B4
            tan = &HD2B48C
            teal = &H8080
            thistle = &HD8BFD8
            tomato = &HFF6347
            turquoise = &H40E0D0
            violet = &HEE82EE
            wheat = &HF5DEB3
            white = &HFFFFFF
            whitesmoke = &HF5F5F5
            yellow = &HFFFF00
            yellowgreen = &H9ACD32
        End Enum

    End Module
End Namespace
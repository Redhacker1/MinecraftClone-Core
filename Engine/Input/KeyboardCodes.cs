   using System;

   /// <summary>
   /// This is mostly Copy+Paste from the SDL2 Keycodes, with some absent keycodes and nicer naming schemes,
   /// also seems to correspond (mostly) well with the ascii character table, 
   /// </summary>
   public enum Keycode
    {
      Unknown = 0,
      Backspace = 8,
      Tab = 9,
      Return = 13, // 0x0000000D
      Escape = 27, // 0x0000001B
      Space = 32, // 0x00000020
      ExclamationPoint = 33, // 0x00000021
      DoubleQuotes = 34, // 0x00000022
      Hash = 35, // 0x00000023
      Dollar = 36, // 0x00000024
      Percent = 37, // 0x00000025
      Ampersand = 38, // 0x00000026
      Quote = 39, // 0x00000027
      LeftParenthesis = 40, // 0x00000028
      RightParenthesis = 41, // 0x00000029
      Astrisk = 42, // 0x0000002A
      Plus = 43, // 0x0000002B
      Comma = 44, // 0x0000002C
      Minus = 45, // 0x0000002D
      Period = 46, // 0x0000002E
      ForwardSlash = 47, // 0x0000002F
      Zero = 48, // 0x00000030
      One = 49, // 0x00000031
      Two = 50, // 0x00000032
      Three = 51, // 0x00000033
      Four = 52, // 0x00000034
      Five = 53, // 0x00000035
      Six = 54, // 0x00000036
      Seven = 55, // 0x00000037
      Eight = 56, // 0x00000038
      Nine = 57, // 0x00000039
      Colon = 58, // 0x0000003A
      Semicolon = 59, // 0x0000003B
      LessThan = 60, // 0x0000003C
      EqualSign = 61, // 0x0000003D
      GreaterThan = 62, // 0x0000003E
      QuestionMark = 63, // 0x0000003F
      AtSymbol = 64, // 0x00000040
      LeftBracket = 91, // 0x0000005B
      BackSlash = 92, // 0x0000005C
      RightBracket = 93, // 0x0000005D
      Caret = 94, // 0x0000005E
      Underscore = 95, // 0x0000005F
      BackQuote = 96, // 0x00000060
      A = 97, // 0x00000061
      B = 98, // 0x00000062
      C = 99, // 0x00000063
      D = 100, // 0x00000064
      E = 101, // 0x00000065
      F = 102, // 0x00000066
      G = 103, // 0x00000067
      H = 104, // 0x00000068
      I = 105, // 0x00000069
      J = 106, // 0x0000006A
      K = 107, // 0x0000006B
      L = 108, // 0x0000006C
      M = 109, // 0x0000006D
      N = 110, // 0x0000006E
      O = 111, // 0x0000006F
      P = 112, // 0x00000070
      Q = 113, // 0x00000071
      R = 114, // 0x00000072
      S = 115, // 0x00000073
      T = 116, // 0x00000074
      U = 117, // 0x00000075
      V = 118, // 0x00000076
      W = 119, // 0x00000077
      X = 120, // 0x00000078
      Y = 121, // 0x00000079
      Z = 122, // 0x0000007A
      Delete = 127, // 0x0000007F
      F1 = 1073741882, // 0x4000003A
      F2 = 1073741883, // 0x4000003B
      F3 = 1073741884, // 0x4000003C
      F4 = 1073741885, // 0x4000003D
      F5 = 1073741886, // 0x4000003E
      F6 = 1073741887, // 0x4000003F
      F7 = 1073741888, // 0x40000040
      F8 = 1073741889, // 0x40000041
      F9 = 1073741890, // 0x40000042
      F10 = 1073741891, // 0x40000043
      F11 = 1073741892, // 0x40000044
      F12 = 1073741893, // 0x40000045
      RightCursorKey = 1073741903, // 0x4000004F
      LeftCursorKey = 1073741904, // 0x40000050
      DownCursorKey = 1073741905, // 0x40000051
      UpCursorKey = 1073741906, // 0x40000052
      Keypad1 = 1073741913, // 0x40000059
      Keypad2 = 1073741914, // 0x4000005A
      Keypad3 = 1073741915, // 0x4000005B
      Keypad4 = 1073741916, // 0x4000005C
      Keypad5 = 1073741917, // 0x4000005D
      Keypad6 = 1073741918, // 0x4000005E
      Keypad7 = 1073741919, // 0x4000005F
      Keypad8 = 1073741920, // 0x40000060
      Keypad9 = 1073741921, // 0x40000061
      Keypad0 = 1073741922, // 0x40000062
      LeftCtrl = 1073742048, // 0x400000E0
      LeftShift = 1073742049, // 0x400000E1
      LeftAlt = 1073742050, // 0x400000E2
      RightCtrl = 1073742052, // 0x400000E4
      RightShift = 1073742053, // 0x400000E5
      RightAlt = 1073742054, // 0x400000E6
    }

    [Flags]
    public enum KeyModifiers : ushort
    {
      None = 0,
      LeftShift = 1,
      RightShift = 2,
      LeftCtrl = 64, // 0x0040
      RightCtrl = 128, // 0x0080
      LeftAlt = 256, // 0x0100
      RightAlt = 512, // 0x0200
      NumberLock = 4096, // 0x1000
      Capslock = 8192, // 0x2000
      ScrollLock = 32768, // 0x8000
      Ctrl = RightCtrl | LeftCtrl, // 0x00C0
      Shift = RightShift | LeftShift, // 0x0003
      Alt = RightAlt | LeftAlt, // 0x0300
    }
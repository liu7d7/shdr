namespace shdr.Shared
{
   public sealed class __fmt
   {
      public static readonly Dictionary<char, __fmt> values = new();
      
      public static readonly __fmt black = new(0, '0');
      public static readonly __fmt darkblue = new(0x0000aa, '1');
      public static readonly __fmt darkgreen = new(0x00aa00, '2');
      public static readonly __fmt darkcyan = new(0x00aaaa, '3');
      public static readonly __fmt darkred = new(0xaa0000, '4');
      public static readonly __fmt darkpurple = new(0xaa00aa, '5');
      public static readonly __fmt gold = new(0xffaa00, '6');
      public static readonly __fmt gray = new(0xaaaaaa, '7');
      public static readonly __fmt darkgray = new(0x555555, '8');
      public static readonly __fmt blue = new(0x5555ff, '9');
      public static readonly __fmt green = new(0x55ff55, 'a');
      public static readonly __fmt cyan = new(0x55ffff, 'b');
      public static readonly __fmt red = new(0xff5555, 'c');
      public static readonly __fmt purple = new(0xff55ff, 'd');
      public static readonly __fmt yellow = new(0xffff55, 'e');
      public static readonly __fmt white = new(0xffffff, 'f');
      public static readonly __fmt reset = new(0, 'r');
      public static readonly __fmt keyword = new(0xff7084, 'g');
      public static readonly __fmt number = new(0xa0ffe0, 'h');
      public static readonly __fmt normal = new (0xffffff, 'i');
      public static readonly __fmt operator_ = new(0xabc8ff, 'j');
      public static readonly __fmt function = new(0x66e5ff, 'k');

      public static string key(string str)
      {
         return $"{__fmt.keyword}{str}{__fmt.normal}";
      }
      
      public static string num(string str)
      {
         return $"{__fmt.number}{str}{__fmt.normal}";
      }
      
      public static string norm(string str)
      {
         return $"{__fmt.normal}{str}{__fmt.normal}";
      }

      public static string op(string str)
      {
         return $"{__fmt.operator_}{str}{__fmt.normal}";
      }
      
      public static string func(string str)
      {
         return $"{__fmt.function}{str}{__fmt.normal}";
      }

      public override string ToString()
      {
         return "\u00a7" + _code;
      }

      private readonly char _code;

      public readonly uint color;

      private __fmt(uint color, char code)
      {
         this.color = color;
         _code = code;
         values[code] = this;
      }
   }
}
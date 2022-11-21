using System.Collections.Concurrent;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using shdr.Engine;
using TextCopy;

namespace shdr.Shared
{
   public static class __text_box
   {
      public struct __pos
      {
         public int col;
         public int lin;

         public static bool operator ==(__pos one, __pos two)
         {
            return one.col == two.col && one.lin == two.lin;
         }

         public static bool operator !=(__pos one, __pos two)
         {
            return !(one == two);
         }
      }

      public struct __data
      {
         public float lastUpdate = 0;
         public __pos prevStart = default;
         public __pos prevEnd = default;
         public __pos start = new();
         public __pos end = new();
         public float scroll = 0f;
         public List<string> text = new();
         public List<string> display = new();
         public bool typing = false;

         public __data()
         {
         }
      }

      public enum __dir
      {
         up,
         down,
         left,
         right,
         none
      }

      private static readonly Dictionary<string, Func<string, string>> _fmt;
      private static readonly HashSet<string> _fmtKeys;
      private static readonly HashSet<string> _delimiters;
      private static readonly HashSet<string> _operators;

      static __text_box()
      {
         List<string> keywords = new()
         {
            "^float$",
            "^int$",
            "^vec2$",
            "^vec3$",
            "^vec4$",
            "^mat2$",
            "^mat3$",
            "^mat4$",
            "^void$",
            "^layout$",
            "^in$",
            "^out$",
            "^inout$",
            "^if$",
            "^for$",
            "^while$",
            "^const$",
            "^uniform$",
            "^sampler2D$",
            "^sampler1D$",
            "^sampler3D$",
            "^return$",
            "^break$",
            "^continue$",
            "^#[a-zA-Z0-9_]+$",
         };
         _fmt = new Dictionary<string, Func<string, string>>
         {
            { "-?^\\d*e?\\.?\\d*", __fmt.num }
         };
         foreach (string str in keywords)
         {
            _fmt[str] = __fmt.key;
         }

         foreach (string str in File.ReadLines("Resource/Texture/glslfuncs.txt"))
         {
            _fmt[str] = __fmt.func;
         }

         _fmtKeys = new HashSet<string>(_fmt.Keys);
         _delimiters = new HashSet<string>
         {
            "(", ")", "{", "}", "[", "]", ";", ",", ".", "+", "-", "*", "/", "%", "=", "!", "<", ">", "&", "|", "^",
            "~", "?", ":", "\\", "\"", "'", "`", " ", "\n"
         };
         _operators = new HashSet<string>
         {
            "(", ")", "{", "}", "[", "]", "+", "-", "*", "/", "%", "=", "!", "<", ">", "&", "|", "^", "~", "?", ":", "\\", "\"", "'", "`", ";"
         };
      }

      public static __pos pos_at(__data dat, float mouseX, float mouseY, float x, float y)
      {
         float lineHeight = __font.get_height() * 1.6f;
         float textOffset = dat.scroll * lineHeight + 15f;
         __pos pos;
         int lin = (int)((mouseY - y - textOffset) / lineHeight);
         int col = 0;
         mouseX += safe(1) / 2f;
         for (int i = 0; i <= dat.text[lin].Length; i++)
         {
            if (safe(i) < mouseX - x - 50)
            {
               col = i;
            }
            else
            {
               break;
            }
         }
         pos.lin = lin;
         pos.col = col;
         return pos;
      }

      public static void click(ref __data dat, float mouseX, float mouseY, float x, float y, bool shiftDown)
      {
         __pos pos = pos_at(dat, mouseX, mouseY, x, y);
         if (shiftDown)
         {
            ref __pos notSelected = ref dat.start;
            ref __pos selected = ref dat.end;
            if (_dir == -1)
            {
               notSelected = ref dat.end;
               selected = ref dat.start;
            }
            _dir = pos.lin < selected.lin || (pos.lin == selected.lin && pos.col < selected.col) ? -1 : 1;
            if (_dir == -1)
            {
               selected = pos;
            }
            else
            {
               selected = pos;
            }
         }
         else
         {
            _dir = 0;
            dat.start = dat.end = pos;
         }
      }
      
      public static void update(ref __data dat, bool firstRun = false)
      {
         if (dat.display.Count < dat.text.Count)
         {
            dat.display.AddRange(new string[dat.text.Count - dat.display.Count]);
         }
         if (dat.display.Count > dat.text.Count)
         {
            dat.display.RemoveRange(dat.text.Count, dat.display.Count - dat.text.Count);
         }
         for (int i = firstRun ? 0 : Math.Min(dat.start.lin, dat.end.lin);
              i <=
              (firstRun
                 ? dat.text.Count - 1
                 : Math.Max(dat.start.lin, dat.end.lin));
              i++)
         {
            string str = dat.text[i];
            StringBuilder full = new();
            StringBuilder word = new();
            foreach (char c in str)
            {
               if (_delimiters.Contains(c.ToString()))
               {
                  string st = word.ToString();
                  string works = "";
                  foreach (string key in _fmtKeys)
                  {
                     Match mat = Regex.Match(st, key);
                     if (mat.Value == st)
                     {
                        works = key;
                        break;
                     }
                  }

                  full.Append(works != "" ? _fmt[works](st) : c == '(' ? __fmt.func(st) : __fmt.norm(st));
                  full.Append(_operators.Contains(c.ToString()) ? __fmt.op(c.ToString()) : c.ToString());
                  word.Clear();
                  continue;
               }

               word.Append(c);
            }

            {
               string st = word.ToString();
               string works = "";
               foreach (string key in _fmtKeys)
               {
                  Match mat = Regex.Match(st, key);
                  if (mat.Value == st)
                  {
                     works = key;
                     break;
                  }
               }

               full.Append(works != "" ? _fmt[works](st) : __fmt.norm(st));
               word.Clear();
            }

            dat.display[i] = full.ToString();
         }
      }

      private static float safe(int e)
      {
         return __font.get_width(" ") * e - (e - 1) * 0.4f;
      }

      private static int _scrollSource;

      public static void render(ref __data dat, float x, float y, float width, float height)
      {
         float d = MathF.Pow(MathHelper.Clamp((Environment.TickCount - dat.lastUpdate) / 100f, 0, 1), 2f);
         float sline = __util.lerp(dat.prevStart.lin, dat.start.lin, d);
         float scol = __util.lerp(safe(dat.prevStart.col),
            safe(dat.start.col), d);
         float eline = __util.lerp(dat.prevEnd.lin, dat.end.lin, d);
         float ecol = __util.lerp(safe(dat.prevEnd.col),
            safe(dat.end.col), d);
         float lineHeight = __font.get_height() * 1.6f;

         if (_scrollSource == 1)
         {
            if (sline < -dat.scroll)
            {
               dat.scroll = -sline;
            }

            if (eline > -dat.scroll + (int)(height - 20) / lineHeight - 1.5f)
            {
               dat.scroll = -eline + (int)(height - 20) / lineHeight - 1.5f;
            }
         }
         
         uint cyan = (uint)new Color4(0, 238, 255,
            (byte)((Math.Abs(Math.Sin(GLFW.GetTime() % 1.0 * Math.PI)) + 1) * 127)).ToArgb();

         uint hotPink = (uint)new Color4(0xFF, 0x00, 0x94,
            (byte)((Math.Abs(Math.Sin(GLFW.GetTime() % 1.0 * Math.PI)) + 1) * 127)).ToArgb();
         GL.Enable(EnableCap.ScissorTest);
         GL.Scissor((int)Math.Floor(x), __shdr.instance.Size.Y - (int)Math.Floor(y + height), (int)Math.Ceiling(width), (int)Math.Ceiling(height));
         float textOffset = dat.scroll * lineHeight + 15f;

         __util.draw_rect(x, y, width, height, 0xcc202531);
         if (dat.typing)
            __util.draw_rect(x, y + textOffset + lineHeight * eline, width, lineHeight, 0xee202531);

         __font.bind();
         for (int i = 0; i < dat.display.Count; i++)
         {
            __font.draw((i + 1).ToString(), x + 20, y + i * lineHeight + textOffset + 2.5f, 0xffcccccc, true);
            __font.draw(dat.display[i], x + 50, y + i * lineHeight + textOffset + 2.5f, 0xffffffff, true);
         }

         __font.render();

         if (dat.typing)
         {
            __util.draw_rect(x + 50 + scol - 1, y + textOffset + sline * lineHeight, 2, lineHeight, hotPink);
            __util.draw_rect(x + 50 + ecol - 1, y + textOffset + eline * lineHeight, 2, lineHeight, hotPink);
         }

         GL.Disable(EnableCap.ScissorTest);
      }

      private static void ensure_pos(ref __data dat, ref __pos pos, __dir dir)
      {
         pos.lin = MathHelper.Clamp(pos.lin, 0, dat.text.Count - 1);

         if (pos.col > dat.text[pos.lin].Length)
         {
            if (pos.lin == dat.text.Count - 1)
            {
               pos.col = dat.text[pos.lin].Length;
            }
            else if (dir is __dir.down or __dir.up)
            {
               pos.col = dat.text[pos.lin].Length;
            }
            else
            {
               pos.lin++;
               pos.col = 0;
            }
         }

         if (pos.col < 0)
         {
            if (pos.lin == 0)
            {
               pos.col = 0;
            }
            else
            {
               pos.lin--;
               pos.col = dat.text[pos.lin].Length;
            }
         }
      }

      private static void ensure_poses(ref __data dat, __dir dir)
      {
         if (dat.start.lin > dat.end.lin || (dat.start.lin == dat.end.lin && dat.start.col > dat.end.col))
         {
            (dat.start, dat.end) = (dat.end, dat.start);
         }

         ensure_pos(ref dat, ref dat.start, dir);
         ensure_pos(ref dat, ref dat.end, dir);
      }

      private static int spaces(string str)
      {
         return str.Length - str.TrimStart().Length;
      }

      public static void wheel(ref __data dat, float scroll)
      {
         _scrollSource = 2;
         dat.scroll += Math.Sign(scroll);
      }

      private static int _dir = 1;

      public static void key(ref __data dat, Keys key, bool shiftDown, bool controlDown)
      {
         if (!dat.typing) return;
         
         dat.lastUpdate = Environment.TickCount;
         dat.prevEnd = dat.end;
         dat.prevStart = dat.start;

         bool typingHappened = false;
         bool important = false;

         void cursor(ref __data dat, Vector2i dir)
         {
            _scrollSource = 1;
            if (!shiftDown)
               _dir = 0;
            ref __pos one = ref dat.end;
            if (dir.X < 0 || dir.Y < 0 || _dir == -1)
            {
               if (_dir == 0)
                  _dir = -1;
               one = ref dat.start;
            }

            ref __pos two = ref dat.end;
            if (dir.X > 0 || dir.Y > 0 || _dir == 1)
            {
               if (_dir == 0)
                  _dir = 1;
               two = ref dat.start;
            }

            if (shiftDown)
            {
               if (_dir == -1)
               {
                  dat.start.lin += dir.Y;
                  dat.start.col += dir.X;
               }
               else
               {
                  dat.end.lin += dir.Y;
                  dat.end.col += dir.X;
               }
            }
            else
            {
               _dir = 0;
               one.col += dir.X;
               one.lin += dir.Y;
               two = one;
            }

            if (dat.start == dat.end)
            {
               _dir = 0;
            }
         }

         __dir dir = __dir.none;

         switch (key)
         {
            case Keys.F5:
            case Keys.S:
               if (controlDown || key == Keys.F5)
               {
                  __shdr.reload();
               }
               break;
            case Keys.C:
               if (controlDown)
               {
                  important = true;
                  if (dat.start.lin == dat.end.lin)
                  {
                     string str = dat.text[dat.start.lin].Substring(dat.start.col, dat.end.col - dat.start.col);
                     ClipboardService.SetText(str);
                  }
                  else
                  {
                     string str = dat.text[dat.start.lin][dat.start.col..] + "\n";
                     if (dat.start.lin + 1 != dat.end.lin)
                     {
                        for (int i = dat.start.lin + 1; i < dat.end.lin; i++)
                        {
                           str += dat.text[i] + "\n";
                        }
                     }
                     str += dat.text[dat.end.lin][..dat.end.col];
                     ClipboardService.SetText(str);
                  }
               }
               break;
            case Keys.V:
               if (controlDown)
               {
                  typingHappened = true;
                  important = true;
                  string str = ClipboardService.GetText();
                  if (str != null)
                  {
                     // split by newlines
                     string[] lines = str.Split("\n");
                     if (lines.Length == 1)
                     {
                        // single line
                        string line = dat.text[dat.start.lin];
                        dat.text[dat.start.lin] = line[..dat.start.col] + lines[0] + line[dat.end.col..];
                        dat.end.col += lines[0].Length;
                        dat.start = dat.end;
                     }
                     else
                     {
                        // multiple lines
                        string overflow = dat.text[dat.end.lin][dat.end.col..];
                        dat.text[dat.start.lin] = dat.text[dat.start.lin][..dat.start.col] + lines[0];
                        dat.text.InsertRange(dat.start.lin + 1, lines[1..^1]);
                        dat.end.lin += lines.Length - 1;
                        dat.text.Insert(dat.end.lin, lines[^1] + overflow);
                        dat.end.col = lines[^1].Length;
                        dat.start = dat.end;
                     }
                  }
               }
               break;
            case Keys.Down:
               cursor(ref dat, (0, 1));
               dir = __dir.down;
               break;
            case Keys.Up:
               cursor(ref dat, (0, -1));
               dir = __dir.up;
               break;
            case Keys.Left:
               cursor(ref dat, (-1, 0));
               dir = __dir.left;
               break;
            case Keys.Right:
               cursor(ref dat, (1, 0));
               dir = __dir.right;
               break;
            case Keys.Home:
               if (controlDown)
               {
                  if (_dir == -1)
                  {
                     dat.start.lin = 0;
                     dat.start.col = 0;
                  }
                  else
                  {
                     dat.end.lin = 0;
                     dat.end.col = 0;
                  }
               }
               else
               {
                  if (_dir == -1)
                  {
                     dat.start.col = spaces(dat.text[dat.end.lin]);
                  }
                  else
                  {
                     dat.end.col = spaces(dat.text[dat.start.lin]);
                  }
               }

               if (!shiftDown)
               {
                  dat.start.col = dat.end.col;
                  dat.start.lin = dat.end.lin;
               }

               break;
            case Keys.End:
               if (controlDown)
               {
                  if (_dir == -1)
                     dat.start.lin = dat.text.Count - 1;
                  else
                     dat.end.lin = dat.text.Count - 1;
               }

               if (_dir == -1)
                  dat.start.col = dat.text[dat.end.lin].Length;
               else
                  dat.end.col = dat.text[dat.end.lin].Length;
               
               if (!shiftDown)
               {
                  dat.start.col = dat.end.col;
                  dat.start.lin = dat.end.lin;
               }

               break;
            case Keys.Enter:
            {
               important = true;
               typingHappened = true;
               string text = dat.text[dat.start.lin];
               int s = spaces(dat.text[dat.start.lin]);
               if (dat.start == dat.end)
               {
                  dat.text[dat.start.lin] = text[..dat.start.col];
                  dat.text.Insert(dat.end.lin + 1, " ".repeat(s) + text[dat.start.col..]);
               }

               dat.start.lin++;
               dat.start.col = s;
               dat.end.col = s;
               dat.end.lin = dat.start.lin;
               break;
            }
            case Keys.Backspace:
               typingHappened = true;
               string text1 = dat.text[dat.start.lin];
               if (dat.start == dat.end)
               {
                  if (dat.start.col > 0)
                  {
                     dat.text[dat.start.lin] = text1[..(dat.start.col - 1)] + text1[dat.start.col..];
                     dat.start.col--;
                     dat.end.col--;
                  }
                  else if (dat.start.lin > 0)
                  {
                     important = true;
                     dat.start.lin--;
                     dat.end.lin--;
                     dat.start.col = dat.end.col = dat.text[dat.start.lin].Length;
                     dat.text[dat.start.lin] += text1;
                     dat.text.RemoveAt(dat.start.lin + 1);
                  }
               }
               else
               {
                  if (dat.end.lin != dat.start.lin)
                  {
                     important = true;
                     dat.text[dat.start.lin] = text1[..dat.start.col] + dat.text[dat.end.lin][dat.end.col..];
                     dat.text[dat.end.lin] = dat.text[dat.end.lin][dat.end.col..];
                     for (int i = dat.start.lin + 1; i <= dat.end.lin; i++)
                     {
                        dat.text.RemoveAt(dat.start.lin + 1);
                     }
                  }
                  else
                  {
                     dat.text[dat.start.lin] = text1[..dat.start.col] + text1[dat.end.col..];
                  }

                  dat.end = dat.start;
               }

               break;
            case Keys.Delete:
               typingHappened = true;
               dat.end.col++;
               dat.start.col++;
               ensure_pos(ref dat, ref dat.end, __dir.right);
               ensure_pos(ref dat, ref dat.start, __dir.right);
               __text_box.key(ref dat, Keys.Backspace, shiftDown, controlDown);
               break;
            case Keys.Tab:
               typingHappened = true;
               if (dat.start == dat.end)
               {
                  string text2 = dat.text[dat.start.lin];
                  if (shiftDown)
                  {
                     int ec = dat.end.col;
                     int el = dat.end.lin;
                     if (ec >= 2)
                     {
                        dat.end.col -= 2;
                        dat.start.col -= 2;
                        if (dat.text[el][ec - 1] == dat.text[el][ec - 2] && dat.text[el][ec - 1] == ' ')
                        {
                           // remove chars @ ec - 1 and ec -2
                           dat.text[el] = text2[..(ec - 2)] + text2[ec..];
                        }
                     }
                  }
                  else
                  {
                     type(ref dat, "  ");
                  }
               }

               break;
         }

         if (typingHappened)
            update(ref dat, important);

         ensure_poses(ref dat, dir);
      }

      public static void type(ref __data dat, string str)
      {
         if (!dat.typing) return;
         
         dat.lastUpdate = Environment.TickCount;
         dat.prevEnd = dat.end;
         dat.prevStart = dat.start;

         if (dat.start == dat.end)
         {
            dat.text[dat.start.lin] = dat.text[dat.start.lin][..dat.start.col] + str +
                                      dat.text[dat.start.lin][dat.start.col..];
            dat.start.col += str.Length;
            dat.end.col = dat.start.col;
         }
         else
         {
            string text = dat.text[dat.start.lin];
            dat.text[dat.start.lin] = text[..dat.start.col] + str + text[dat.end.col..];
            dat.start.col += str.Length;
            dat.end.col = dat.start.col;
         }

         update(ref dat);

         ensure_poses(ref dat, __dir.none);
      }
   }
}
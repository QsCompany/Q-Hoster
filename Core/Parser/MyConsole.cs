using System;
using System.Collections.Generic;
using System.Threading;
class MyConsole
{
    class Pipe 
    {
        List<WriteLn> list = new List<WriteLn>();
        private Thread t;
        public void Add(object s)
        {
            exec(s?.ToString() ?? "", null);
        }

        internal void Add(string s, object[] args)
        {
            exec(s, args);
            //_Add(new WriteLn(s,args));
        }
        private void _exec(string s, object[] args)
        {

            switch (args?.Length)
            {
                case 0:
                case null:
                    global::System.Console.Write(s);
                    break;
                case 1:
                    global::System.Console.Write(s, args[0]);
                    break;

                case 2:
                    global::System.Console.Write(s, args[0], args[1]);
                    break;

                case 3:
                    global::System.Console.Write(s, args[0], args[1], args[2]);
                    break;

                case 4:
                    global::System.Console.Write(s, args[0], args[1], args[2], args[3]);
                    break;

                case 5:
                    global::System.Console.Write(s, args[0], args[0], args[1], args[2], args[3], args[4]);
                    break;
                case 6:
                    global::System.Console.Write(s, args[0], args[0], args[1], args[2], args[3], args[4], args[5]);
                    break;
            }
        }
        private void exec(string s, object[] args)
        {

            switch (args?.Length)
            {
                case 0:
                case null:
                    QServer.Log.Write(s);
                    break;
                case 1:
                    QServer.Log.Write(string.Format(s, args[0]));
                    break;

                case 2:
                    QServer.Log.Write(string.Format(s, args[0], args[1]));
                    break;

                case 3:
                    QServer.Log.Write(string.Format(s, args[0], args[1], args[2]));
                    break;

                case 4:
                    QServer.Log.Write(string.Format(s, args[0], args[1], args[2], args[3]));
                    break;

                case 5:
                    QServer.Log.Write(string.Format(s, args[0], args[0], args[1], args[2], args[3], args[4]));
                    break;
                case 6:
                    QServer.Log.Write(string.Format(s, args[0], args[0], args[1], args[2], args[3], args[4], args[5]));
                    break;
            }
        }
        private void _start()
        {
            db:
            try
            {
                WriteLn[] array;
                lock (this)
                {
                    array =list. ToArray();
                    list.Clear();
                }

                foreach (var c in array)
                {
                    if (c.args == null) Write(c.un?.ToString() ?? "");
                    else exec(c.s, c.args);
                }
            }
            catch (Exception)
            {
            }
            Thread.Sleep(500);
            goto db;
        }
        
    }
    private class WriteLn
    {
        public string s;
        public object[] args;
        public object un;

        public WriteLn(string s,object[] args)
        {
            this.s = s;
            this.args = args;
        }
        public WriteLn(object un)
        {
            this.un = un;
        }
        
    }
    private static Pipe pipe = new Pipe();

    public MyConsole()
    {
    }

    public static int BufferWidth => 160;

    public static void WriteLine(object s)
    {
        pipe.Add((s ?? "").ToString() + Environment.NewLine);
    }
    public static void WriteLine(string s, params object[] args)
    {
        pipe.Add((s ?? "").ToString() + Environment.NewLine, args);
    }
    public static void Write(string s)
    {
        pipe.Add(s ?? "");
    }
    public static string ReadLine()
    {
        Thread.Sleep(500);
        return global::System.Console.ReadLine();
    }

    internal static ConsoleKeyInfo ReadKey(bool v)
    {
        Thread.Sleep(500);
        return new ConsoleKeyInfo('0', ConsoleKey.D0, false, false, false);
        //return System.Console.ReadKey(v);
    }

    internal static void WriteSeparator(string v, char sp = '*')
    {
        var s = (BufferWidth - v.Length - 6) / 2;
        var vv = s <= 0 ? "" : new String(sp, s);
        WriteLine(vv + "   " + v + "   " + vv);
    }
    internal static void WriteSeparator(char sp = '*')
    {
        WriteLine(new String(sp, BufferWidth));
    }
    internal static void WriteSeparator(params object[] sp )
    {
        foreach (var p in sp)
        {
            if (p is char) WriteSeparator((char)p);
            else WriteSeparator(p?.ToString() ?? "");
        }
    }

    internal static void Write(Exception e)
    {
        WriteSeparator();
        var t = e;
        do
        {
            WriteLine(e.Message);
            t = t.InnerException;
        } while (t!=null);
        WriteSeparator();
    }
}

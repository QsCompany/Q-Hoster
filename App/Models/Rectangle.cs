using System;

public struct Rectangle
{
    public int X;
    public int Y;
    public int W;
    public int H;
    public Rectangle(int x,int y,int w,int h):this()
    {
        X = x;
        Y = y;
        W = w;
        H = h;
    }
    public Rectangle(string s):this()
    {
        var e = s.Split(new[] { ',', ')', '(' });
        if (e.Length < 4) throw new ArgumentOutOfRangeException();
        this.X = int.Parse(e[0]);
        this.Y = int.Parse(e[1]);
        this.W = int.Parse(e[2]);
        this.H = int.Parse(e[3]);

    }
}
using System.Collections.Generic;

public class App
{
    public static string dirPath = @"D:/fla2csd/";
    public static string cachePath = @"D:/fla2csd_out/";
    public static string ccsPath = @"C:\Users\yzqlwt\Documents\WorkSpace\cocos-ui\mangomath-ui\CocosProject.ccs";
    public static string Activity = "game1000";
    public static string Skin = "skin01";
}

public class SymbolConfig
{
    public List<ItemConfig> symbols;
    public string activity;
    public string skin;
}
public class ItemConfig
{
    public string name;
    public string image;
    public Position position;
    public Size size;

}

public class Position
{
    public float x;
    public float y;
}

public class Size
{
    public float width;
    public float height;
}
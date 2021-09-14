using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

public static class EnumExtentions
{
   //NOTE: Taken from  https://stackoverflow.com/questions/3916914/c-sharp-using-numbers-in-an-enum
    public static string GetDescription(this Enum value)
    {
        return ((DescriptionAttribute)Attribute.GetCustomAttribute(
            value.GetType().GetFields(BindingFlags.Public | BindingFlags.Static)
                .Single(x => x.GetValue(null).Equals(value)),
            typeof(DescriptionAttribute)))?.Description ?? value.ToString();
    }
}

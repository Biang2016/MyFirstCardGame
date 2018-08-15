﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

public class Utils
{
    public static List<Type> GetClassesByNameSpace(string nameSpace)
    {
        Assembly asm = Assembly.GetCallingAssembly();

        List<Type> res = new List<Type>();

        foreach (Type type in asm.GetTypes())
        {
            if (type.Namespace == nameSpace)
                res.Add(type);
        }

        return res;
    }

    public static List<Type> GetClassesByBaseClass(Type baseType)
    {
        Assembly asm = Assembly.GetExecutingAssembly();

        List<Type> res = new List<Type>();

        foreach (Type type in asm.GetTypes())
        {
            if(type.IsAbstract)continue;
            if (type.BaseType == baseType)
            {
                res.Add(type);
            }
        }

        return res;
    }
}
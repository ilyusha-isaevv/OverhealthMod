using System;
using System.Reflection;
using MonoMod.Cil;
using Terraria.ModLoader;

namespace OverhealthMod.Utils;

public static class QuickIL
{
    public static void EditMethod<T>(string methodName, ILContext.Manipulator manipulator) => EditMethod(typeof(T).GetMethod(methodName), manipulator);

    public static void EditMethod(Type type, string methodName, ILContext.Manipulator manipulator) => EditMethod(type.GetMethod(methodName), manipulator);

    public static void EditMethod(Assembly assembly, string typeFullName, string methodName, ILContext.Manipulator manipulator) => EditMethod(assembly.GetType(typeFullName).GetMethod(methodName), manipulator);

    public static void EditMethod(MethodInfo methodInfo, ILContext.Manipulator manipulator)
    {
        if (methodInfo == null)
            return;
        MonoModHooks.Modify(methodInfo, manipulator);
    }
}

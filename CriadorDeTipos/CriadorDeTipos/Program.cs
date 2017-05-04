using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CriadorDeTipos
{
    class Program
    {
        public static void Main(string[] args)
        {
            Type typeOfT = typeof(Person);
            var assemblyBuilder = GetAssembly($"{typeOfT.Name}");
            var moduleBuilder = assemblyBuilder.DefineDynamicModule($"Deney.{typeOfT.Name}");
            var interfaceType = typeof(IProxy);
            var typeBuilder = moduleBuilder.DefineType(
                $"{typeOfT.Name}_{Guid.NewGuid().ToString().Replace("-", "")}", TypeAttributes.Public | TypeAttributes.Class
                , typeOfT
                );
            typeBuilder.AddInterfaceImplementation(interfaceType);
            var setIsDirty = CreateIsDirtyProperty(typeBuilder);
            var info = typeBuilder.CreateTypeInfo();
        }

        static AssemblyBuilder GetAssembly(string name)
        {
            var builder = AssemblyBuilder.DefineDynamicAssembly(new System.Reflection.AssemblyName { Name = name }, AssemblyBuilderAccess.Run);

            return builder;
        }
        static MethodInfo CreateIsDirtyProperty(TypeBuilder typeBuilder)
        {
            var propType = typeof(bool);
            var field = typeBuilder.DefineField("_IsDirty", propType, FieldAttributes.Private);
            var property = typeBuilder.DefineProperty("IsDirty", PropertyAttributes.None, propType, new Type[] { propType });
            const MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.NewSlot | MethodAttributes.SpecialName |
                                                MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig;
            var currentGetPropertyMethodBuilder = typeBuilder.DefineMethod("get_IsDirty",
                                                   getSetAttr,
                                                   propType,
                                                   Type.EmptyTypes
                                                  );
            var currentGetIL = currentGetPropertyMethodBuilder.GetILGenerator();
            currentGetIL.Emit(OpCodes.Ldarg_0);
            currentGetIL.Emit(OpCodes.Ldfld, field);
            currentGetIL.Emit(OpCodes.Ret);

            var currentSetPropertyMethodBuilder = typeBuilder.DefineMethod("set_IsDirty",
                                                    getSetAttr,
                                                    null,
                                                    new Type[] { propType }
                                                  );
            var currentSetIL = currentSetPropertyMethodBuilder.GetILGenerator();
            currentSetIL.Emit(OpCodes.Ldarg_0);
            currentSetIL.Emit(OpCodes.Ldarg_1);
            currentSetIL.Emit(OpCodes.Stfld, field);
            currentSetIL.Emit(OpCodes.Ret);

            property.SetGetMethod(currentGetPropertyMethodBuilder);
            property.SetSetMethod(currentSetPropertyMethodBuilder);

            var getMethod = typeof(IProxy).GetMethod("get_" + "IsDirty");
            var setMethod = typeof(IProxy).GetMethod("set_" + "IsDirty");

            typeBuilder.DefineMethodOverride(currentGetPropertyMethodBuilder, getMethod);
            typeBuilder.DefineMethodOverride(currentSetPropertyMethodBuilder, setMethod);
            return currentSetPropertyMethodBuilder;
        }
    }

    public interface IProxy
    {
        bool IsDirty { get; set; }
    }

    public class Person
    {
        public string Name { get; set; }

    }
}

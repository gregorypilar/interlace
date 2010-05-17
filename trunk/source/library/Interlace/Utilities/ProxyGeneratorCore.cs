#region Using Directives and Copyright Notice

// Copyright (c) 2007-2010, Computer Consultancy Pty Ltd
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the Computer Consultancy Pty Ltd nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL COMPUTER CONSULTANCY PTY LTD BE LIABLE 
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER 
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY 
// OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;

#endregion

namespace Interlace.Utilities
{
    public class ProxyGeneratorCore
    {
        static readonly Dictionary<Type, OpCode> _loadFromPointerIntrinsicOpcodes;
        static readonly Dictionary<Type, OpCode> _storeToPointerIntrinsicOpcodes;

        AssemblyBuilder _assemblyBuilder;
        ModuleBuilder _module;

        static ProxyGeneratorCore()
        {
            _loadFromPointerIntrinsicOpcodes = new Dictionary<Type, OpCode>();
            _loadFromPointerIntrinsicOpcodes[typeof(char)] = OpCodes.Ldind_U2;
            _loadFromPointerIntrinsicOpcodes[typeof(bool)] = OpCodes.Ldind_I1;
            _loadFromPointerIntrinsicOpcodes[typeof(short)] = OpCodes.Ldind_I2;
            _loadFromPointerIntrinsicOpcodes[typeof(int)] = OpCodes.Ldind_I4;
            _loadFromPointerIntrinsicOpcodes[typeof(long)] = OpCodes.Ldind_I8;
            _loadFromPointerIntrinsicOpcodes[typeof(ushort)] = OpCodes.Ldind_U2;
            _loadFromPointerIntrinsicOpcodes[typeof(uint)] = OpCodes.Ldind_U4;
            _loadFromPointerIntrinsicOpcodes[typeof(ulong)] = OpCodes.Ldind_I8;
            _loadFromPointerIntrinsicOpcodes[typeof(float)] = OpCodes.Ldind_R4;
            _loadFromPointerIntrinsicOpcodes[typeof(double)] = OpCodes.Ldind_R8;

            _storeToPointerIntrinsicOpcodes = new Dictionary<Type, OpCode>();
            _storeToPointerIntrinsicOpcodes[typeof(char)] = OpCodes.Stind_I2;
            _storeToPointerIntrinsicOpcodes[typeof(bool)] = OpCodes.Stind_I1;
            _storeToPointerIntrinsicOpcodes[typeof(short)] = OpCodes.Stind_I2;
            _storeToPointerIntrinsicOpcodes[typeof(int)] = OpCodes.Stind_I4;
            _storeToPointerIntrinsicOpcodes[typeof(long)] = OpCodes.Stind_I8;
            _storeToPointerIntrinsicOpcodes[typeof(ushort)] = OpCodes.Stind_I2;
            _storeToPointerIntrinsicOpcodes[typeof(uint)] = OpCodes.Stind_I4;
            _storeToPointerIntrinsicOpcodes[typeof(ulong)] = OpCodes.Stind_I8;
            _storeToPointerIntrinsicOpcodes[typeof(float)] = OpCodes.Stind_R4;
            _storeToPointerIntrinsicOpcodes[typeof(double)] = OpCodes.Stind_R8;
        }

        public ProxyGeneratorCore()
        {
            AssemblyName assemblyName = new AssemblyName(); 
            assemblyName.Name = "ProxiesAssembly"; 

            _assemblyBuilder = 
                Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

            _module = _assemblyBuilder.DefineDynamicModule("ProxiesModule.dll", true);
        }

        public ProxyFactory<TInterface> GenerateProxy<TInterface>() where TInterface : class
        {
            return GenerateProxy(typeof(TInterface)) as ProxyFactory<TInterface>;
        }

        public ProxyFactoryBase GenerateProxy(Type interfaceType)
        {
            if (interfaceType == null) throw new ArgumentNullException("interfaceType");
            if (!interfaceType.IsInterface) throw new ArgumentException("An interface type is required to generate a proxy.");
            if (interfaceType.ContainsGenericParameters) throw new ArgumentException("An interface with generic parameters is not supported.");

            TypeBuilder typeBuilder = _module.DefineType(string.Format("ProxyFor{0}", interfaceType.Name),
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed);
            typeBuilder.AddInterfaceImplementation(interfaceType);

            FieldBuilder handlerField = typeBuilder.DefineField("_handler", 
                typeof(IProxyHandler), FieldAttributes.Private);

            FieldBuilder methodListField = typeBuilder.DefineField("_methodList", 
                typeof(MethodInfo[]), FieldAttributes.Private);

            MethodInfo[] methodList = interfaceType.GetMethods();

            BuildProxyConstructor(typeBuilder, methodListField, handlerField);

            for (int i = 0; i < methodList.Length; i++)
            {
                BuildProxyMethod(typeBuilder, methodList[i], i, handlerField, methodListField);
            }

            Type proxy = typeBuilder.CreateType();

            Type proxyFactory = typeof(ProxyFactory<>).MakeGenericType(interfaceType);
            
            ConstructorInfo proxyFactoryConstructor = proxyFactory.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                new Type[] { typeof(Type), typeof(MethodInfo[]) }, null);

            return proxyFactoryConstructor.Invoke(new object[] { proxy, methodList }) as ProxyFactoryBase;
        }

        static void BuildProxyConstructor(TypeBuilder typeBuilder, FieldBuilder methodListField,
            FieldBuilder handlerField)
        {
            ConstructorBuilder builder = typeBuilder.DefineConstructor(MethodAttributes.Public, 
                CallingConventions.Standard, new Type[] { typeof(MethodInfo[]), typeof(IProxyHandler) });

            ILGenerator generator = builder.GetILGenerator();

            // Store the first argument into this._methodList:
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Stfld, methodListField);

            // Store the second argument into this._handler:
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_2);
            generator.Emit(OpCodes.Stfld, handlerField);

            // Return:
            generator.Emit(OpCodes.Ret);
        }

        static void BuildProxyMethod(TypeBuilder typeBuilder, MethodInfo method, int methodIndex, 
            FieldBuilder handlerField, FieldBuilder methodListField)
        {
            // Get a parameter list and build a parameter type list:
            ParameterInfo[] parameters = method.GetParameters();

            Type[] parameterTypes = new Type[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                parameterTypes[i] = parameters[i].ParameterType;
            }

            // Check the parameter types:
            foreach (ParameterInfo parameter in parameters)
            {
                if (parameter.ParameterType.IsByRef &&
                   (parameter.Attributes & ParameterAttributes.Out) != ParameterAttributes.Out)
                {
                    throw new NotImplementedException("Proxies can not be created for interfaces " +
                        "containing methods with \"ref\" parameters.");
                }
            }

            // Methods with more than 255 arguments, apart from being insane, are 
            // not supported; because they are insane:
            if (parameters.Length > 255)
            {
                throw new InvalidOperationException(
                    "Interface methods with more than 255 parameters are not supported.");
            }

            // Create the method:
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(method.Name,
                MethodAttributes.Public | MethodAttributes.Virtual, CallingConventions.Standard,
                method.ReturnType, parameterTypes);

            ILGenerator generator = methodBuilder.GetILGenerator();

            LocalBuilder argumentsLocal = generator.DeclareLocal(typeof(object[]));

            MethodInfo handlerInvokeMethod = typeof(IProxyHandler).GetMethod(
                "Invoke", new Type[] { typeof(object), typeof(MethodInfo), typeof(object[]) });

            // Create the arguments array:
            generator.Emit(OpCodes.Ldc_I4, (int)parameters.Length);
            generator.Emit(OpCodes.Newarr, typeof(object));
            generator.Emit(OpCodes.Stloc, argumentsLocal);

            // Set each element of the array:
            for (int i = 0; i < parameters.Length; i++)
            {
                Type parameterType = parameterTypes[i];

                if (!parameterType.IsByRef)
                {
                    generator.Emit(OpCodes.Ldloc, argumentsLocal);
                    generator.Emit(OpCodes.Ldc_I4, (int)i);
                    generator.Emit(OpCodes.Ldarg_S, (byte)(i + 1));

                    if (parameterType.IsValueType)
                    {
                        generator.Emit(OpCodes.Box, parameterType);
                    }

                    generator.Emit(OpCodes.Stelem_Ref);
                }
            }

            // Push _handler, then "this", followed by the method information from the method list, 
            // followed by arguments array; then call the handler:
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, handlerField);

            generator.Emit(OpCodes.Ldarg_0);

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, methodListField);
            generator.Emit(OpCodes.Ldc_I4, (int)methodIndex);
            generator.Emit(OpCodes.Ldelem_Ref);

            generator.Emit(OpCodes.Ldloc, argumentsLocal);

            generator.Emit(OpCodes.Callvirt, handlerInvokeMethod);

            // Return the data in the "out" arguments:
            for (int i = 0; i < parameters.Length; i++)
            {
                Type parameterType = parameterTypes[i];

                if (parameterType.IsByRef)
                {
                    // Get the reference pointer:
                    generator.Emit(OpCodes.Ldarg_S, (byte)(i + 1));

                    // Get the boxed value from the arguments array:
                    generator.Emit(OpCodes.Ldloc, argumentsLocal);
                    generator.Emit(OpCodes.Ldc_I4, (int)i);
                    generator.Emit(OpCodes.Ldelem_Ref);

                    if (parameterType.GetElementType().IsValueType)
                    {
                        generator.Emit(OpCodes.Unbox_Any, parameterType.GetElementType());
                    }

                    // Store the value in the reference pointer:
                    EmitStoreToReference(generator, parameterType.GetElementType());
                }
            }

            // Return the return value:
            if (method.ReturnType.Equals(typeof(void)))
            {
                generator.Emit(OpCodes.Pop);
            }
            else if (method.ReturnType.IsValueType)
            {
                generator.Emit(OpCodes.Unbox_Any, method.ReturnType);
            }

            generator.Emit(OpCodes.Ret);
        }

        static void EmitStoreToReference(ILGenerator generator, Type type)
        {
            if (!type.IsValueType) 
            {
                generator.Emit(OpCodes.Stind_Ref);
            }
            else if (type.IsPrimitive)
            {
                generator.Emit(_storeToPointerIntrinsicOpcodes[type]);
            }
            else
            {
                if (type.IsEnum)
                {
                    generator.Emit(OpCodes.Stind_I4);
                }
                else
                {
                    generator.Emit(OpCodes.Stind_Ref);
                }
            }
        }

        static void EmitLoadFromPointer(ILGenerator generator, Type type)
        {
            if (!type.IsValueType) throw new InvalidOperationException();

            if (type.IsPrimitive)
            {
                generator.Emit(_loadFromPointerIntrinsicOpcodes[type]);
            }
            else
            {
                if (type.IsEnum)
                {
                    generator.Emit(OpCodes.Ldind_I4);
                }
                else
                {
                    generator.Emit(OpCodes.Ldobj);
                }
            }
        }
    }
}

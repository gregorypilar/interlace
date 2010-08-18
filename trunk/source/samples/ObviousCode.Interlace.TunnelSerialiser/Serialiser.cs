using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using ObviousCode.Interlace.TunnelSerialiser.Attributes;
using System.IO;
using Interlace.PropertyLists;

namespace ObviousCode.Interlace.TunnelSerialiser
{
    public class Serialiser
    {
        public static byte[] Tunnel(object objectToSerialise)
        {
            return Tunnel(objectToSerialise, false);
        }       

        public static byte[] Tunnel(object objectToSerialise, bool allowInheritance)
        {
            PropertyDictionary dictionary = PropertyDictionary.EmptyDictionary();

            Type type = objectToSerialise.GetType();

            AssertTypeIsTunnelable(type, allowInheritance);

            foreach (PropertyInfo pinfo in type.GetProperties())
            {
                if (!allowInheritance && pinfo.DeclaringType != objectToSerialise.GetType()) continue;

                AddPropertyToDictionaryIfTunnelable(dictionary, objectToSerialise, pinfo, allowInheritance);
            }
            
            return dictionary.PersistToByteArray();
        }        
        
        public static T Restore<T>(MemoryStream stream, bool allowInheritance)
        {
            PropertyDictionary dictionary = PropertyDictionary.FromStream(stream);

            AssertTypeIsTunnelable(typeof(T), allowInheritance);

            T newT = Activator.CreateInstance<T>();

            foreach (PropertyInfo pinfo in typeof(T).GetProperties())
            {
                if (!allowInheritance && pinfo.DeclaringType != typeof(T)) continue;
                
                AddPropertyToInstanceIfTunnelable(dictionary, newT, pinfo, allowInheritance);
            }

            return newT;
        }        

        public static T Restore<T>(MemoryStream stream)
        {
            return Restore<T>(stream, false);            
        }        

        public static T Restore<T>(byte[] bytes, bool allowInheritance)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                return Restore<T>(stream, allowInheritance);
            }
        }

        public static T Restore<T>(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                return Restore<T>(stream, false);
            }
        }

        private static void AssertTypeIsTunnelable(Type type, bool allowInheritance)
        {
            MemberInfo info = type;

            object[] classAttributes = info.GetCustomAttributes(typeof(TunnelAttribute), allowInheritance);

            if (classAttributes.Length == 0)
            {
                throw new InvalidOperationException("Only objects decorated with the Tunnel Attribute may be serialised.");
            }
        }

        private static void AddPropertyToDictionaryIfTunnelable(PropertyDictionary dictionary, object objectToSerialise, PropertyInfo pinfo, bool allowInheritance)
        {
            object[] attributes = pinfo.GetCustomAttributes(typeof(TunnelAttribute), allowInheritance);

            if (attributes.Length > 0)
            {
                TunnelAttribute attribute = attributes[0] as TunnelAttribute;

                string tunnelName =
                    string.IsNullOrEmpty(attribute.TunnelName) ?
                        pinfo.Name : attribute.TunnelName;

                object value = pinfo.GetValue(objectToSerialise, null);
                
                if (value is Enum)
                {
                    if (Enum.GetUnderlyingType(value.GetType()) == typeof(Int32))
                    {
                        value = (int)value;
                    }
                    else if (Enum.GetUnderlyingType(value.GetType()) == typeof(byte))
                    {
                        value = (byte)value;
                    }
                }

                if (value is DateTime)
                {
                    value = ((DateTime)value).Ticks;
                }                
                else if (!(value == null || value is byte[] || value is string || value is int || value is PropertyDictionary))
                {
                    value = value.ToString();
                }

                dictionary.SetValueFor(tunnelName, value);
            }
        }

        private static void AddPropertyToInstanceIfTunnelable(PropertyDictionary dictionary, object newT, PropertyInfo pinfo, bool allowInheritance)
        {
            object[] attributes = pinfo.GetCustomAttributes(typeof(TunnelAttribute), allowInheritance);

            if (attributes.Length > 0)
            {                
                TunnelAttribute attribute = attributes[0] as TunnelAttribute;

                string tunnelName =
                    string.IsNullOrEmpty(attribute.TunnelName) ?
                        pinfo.Name : attribute.TunnelName;

                object value = dictionary.ValueFor(tunnelName);
                if (value == null)
                {
                    return;
                }
                else if (pinfo.PropertyType == typeof(double))
                {
                    pinfo.SetValue(newT, double.Parse(value.ToString()), null);
                }
                else if (pinfo.PropertyType == typeof(float))
                {
                    pinfo.SetValue(newT, float.Parse(value.ToString()), null);
                }
                else if (pinfo.PropertyType == typeof(decimal))
                {
                    pinfo.SetValue(newT, decimal.Parse(value.ToString()), null);
                }
                else if (pinfo.PropertyType == typeof(bool))
                {
                    pinfo.SetValue(newT, bool.Parse(value.ToString()), null);
                }
                else if (pinfo.PropertyType == typeof(long))
                {
                    pinfo.SetValue(newT, long.Parse(value.ToString()), null);
                }
                else if (pinfo.PropertyType == typeof(DateTime))
                {
                    pinfo.SetValue(newT, new DateTime(long.Parse(value.ToString())), null);
                }
                else if (pinfo.PropertyType.BaseType == typeof(Enum))
                {
                    pinfo.SetValue(newT,
                        Enum.Parse(pinfo.PropertyType, value.ToString())
                        , null
                        );
                }
                //else if (pinfo.PropertyType == typeof(byte[]))
                //{
                //    pinfo.SetValue(newT, Encoding.Unicode.GetBytes(value.ToString()), null);
                //}
                else if (value is byte[] || value is string || value is int || value is PropertyDictionary)
                {
                    pinfo.SetValue(newT, value, null);
                }
                
                else throw new InvalidCastException();
                
            }
        }
    
    }
}

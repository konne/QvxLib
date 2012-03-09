/*
    This Library is to have an easy access to Qvx Files and the Qlikview
    Connector Interface.
  
    Copyright (C) 2011  Konrad Mattheis (mattheis@ukma.de)
 
    This Software is available under the GPL and a comercial licence.
    For further information to the comercial licence please contact
    Konrad Mattheis. 

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

namespace QvxLib
{
    #region Usings
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Microsoft.CSharp;
    using System.Collections;
    #endregion

    public static class ReflectionExtensions
    {
        public static bool IsCustomValueType(this Type type)
        {
            return type.IsValueType && !type.IsPrimitive && type.Namespace != null && !type.Namespace.StartsWith("System.");
        }
    }

    #region QvxData
    public class QvxData
    {
        #region Mapping C# Types -> Qvx Types
        private class NetType2QvxType
        {
            #region BinaryWriterTypes
            public static readonly List<Type> BinaryWriterTypes = new List<Type>() {
                                                        typeof(bool),
                                                        typeof(byte),                                                                                                                                                                                                                                                   
                                                        // only 4 or 8 byte REAL are supported by Qlikview
                                                        //typeof(decimal),
                                                        typeof(double),
                                                        typeof(float),
                                                        typeof(int),
                                                        typeof(long),
                                                        typeof(sbyte),
                                                        typeof(char),
                                                        typeof(short),                                                      
                                                        typeof(uint),
                                                        typeof(ulong),
                                                        typeof(ushort),
                                                        typeof(DateTime)};
            #endregion

            internal static QvxTableHeaderQvxFieldHeader MapFieldType(Type mapType)
            {
                if (mapType == typeof(SByte)) return new QvxTableHeaderQvxFieldHeader() { Type = QvxFieldType.QVX_SIGNED_INTEGER, ByteWidth = 1, ByteWidthSpecified = true, Extent = QvxFieldExtent.QVX_FIX, NullRepresentation = QvxNullRepresentation.QVX_NULL_NEVER, FieldFormat = new FieldAttributes() { Type = FieldAttrType.INTEGER, nDec = 0 } };
                if (mapType == typeof(Int16)) return new QvxTableHeaderQvxFieldHeader() { Type = QvxFieldType.QVX_SIGNED_INTEGER, ByteWidth = 2, ByteWidthSpecified = true, Extent = QvxFieldExtent.QVX_FIX, NullRepresentation = QvxNullRepresentation.QVX_NULL_NEVER, FieldFormat = new FieldAttributes() { Type = FieldAttrType.INTEGER, nDec = 0 } };
                if (mapType == typeof(Int32)) return new QvxTableHeaderQvxFieldHeader() { Type = QvxFieldType.QVX_SIGNED_INTEGER, ByteWidth = 4, ByteWidthSpecified = true, Extent = QvxFieldExtent.QVX_FIX, NullRepresentation = QvxNullRepresentation.QVX_NULL_NEVER, FieldFormat = new FieldAttributes() { Type = FieldAttrType.INTEGER, nDec = 0 } };
                if (mapType == typeof(Int64)) return new QvxTableHeaderQvxFieldHeader() { Type = QvxFieldType.QVX_SIGNED_INTEGER, ByteWidth = 8, ByteWidthSpecified = true, Extent = QvxFieldExtent.QVX_FIX, NullRepresentation = QvxNullRepresentation.QVX_NULL_NEVER, FieldFormat = new FieldAttributes() { Type = FieldAttrType.INTEGER, nDec = 0 } };
                if (mapType == typeof(Byte)) return new QvxTableHeaderQvxFieldHeader() { Type = QvxFieldType.QVX_UNSIGNED_INTEGER, ByteWidth = 1, ByteWidthSpecified = true, Extent = QvxFieldExtent.QVX_FIX, NullRepresentation = QvxNullRepresentation.QVX_NULL_NEVER, FieldFormat = new FieldAttributes() { Type = FieldAttrType.INTEGER, nDec = 0 } };
                if (mapType == typeof(UInt16)) return new QvxTableHeaderQvxFieldHeader() { Type = QvxFieldType.QVX_UNSIGNED_INTEGER, ByteWidth = 2, ByteWidthSpecified = true, Extent = QvxFieldExtent.QVX_FIX, NullRepresentation = QvxNullRepresentation.QVX_NULL_NEVER, FieldFormat = new FieldAttributes() { Type = FieldAttrType.INTEGER, nDec = 0 } };
                if (mapType == typeof(UInt32)) return new QvxTableHeaderQvxFieldHeader() { Type = QvxFieldType.QVX_UNSIGNED_INTEGER, ByteWidth = 4, ByteWidthSpecified = true, Extent = QvxFieldExtent.QVX_FIX, NullRepresentation = QvxNullRepresentation.QVX_NULL_NEVER, FieldFormat = new FieldAttributes() { Type = FieldAttrType.INTEGER, nDec = 0 } };
                if (mapType == typeof(UInt64)) return new QvxTableHeaderQvxFieldHeader() { Type = QvxFieldType.QVX_UNSIGNED_INTEGER, ByteWidth = 8, ByteWidthSpecified = true, Extent = QvxFieldExtent.QVX_FIX, NullRepresentation = QvxNullRepresentation.QVX_NULL_NEVER, FieldFormat = new FieldAttributes() { Type = FieldAttrType.INTEGER, nDec = 0 } };
                if (mapType == typeof(Single)) return new QvxTableHeaderQvxFieldHeader() { Type = QvxFieldType.QVX_IEEE_REAL, ByteWidth = 4, ByteWidthSpecified = true, Extent = QvxFieldExtent.QVX_FIX, NullRepresentation = QvxNullRepresentation.QVX_NULL_NEVER, FieldFormat = new FieldAttributes() { Type = FieldAttrType.REAL, nDec = 0 } };
                if (mapType == typeof(Double)) return new QvxTableHeaderQvxFieldHeader() { Type = QvxFieldType.QVX_IEEE_REAL, ByteWidth = 8, ByteWidthSpecified = true, Extent = QvxFieldExtent.QVX_FIX, NullRepresentation = QvxNullRepresentation.QVX_NULL_NEVER, FieldFormat = new FieldAttributes() { Type = FieldAttrType.REAL, nDec = 0 } };
                // only 4 or 8 byte REAL are supported by Qlikview
                //if (mapType == typeof(Decimal)) return new QvxTableHeaderQvxFieldHeader() { Type = QvxFieldType.QVX_IEEE_REAL, ByteWidth = 16, ByteWidthSpecified = true, Extent = QvxFieldExtent.QVX_FIX, NullRepresentation = QvxNullRepresentation.QVX_NULL_NEVER, FieldFormat = new FieldAttributes() { Type = FieldAttrType.REAL, nDec = 0 } };

                if (mapType == typeof(bool)) return new QvxTableHeaderQvxFieldHeader() { Type = QvxFieldType.QVX_UNSIGNED_INTEGER, ByteWidth = 2, ByteWidthSpecified = true, Extent = QvxFieldExtent.QVX_FIX, NullRepresentation = QvxNullRepresentation.QVX_NULL_NEVER, FieldFormat = new FieldAttributes() { Type = FieldAttrType.INTEGER, nDec = 0 } };

                if (mapType == typeof(Char)) return new QvxTableHeaderQvxFieldHeader() { Type = QvxFieldType.QVX_TEXT, ByteWidth = 4, ByteWidthSpecified = true, Extent = QvxFieldExtent.QVX_COUNTED, NullRepresentation = QvxNullRepresentation.QVX_NULL_NEVER, FieldFormat = new FieldAttributes() { Type = FieldAttrType.UNKNOWN } };
                if (mapType == typeof(Char[])) return new QvxTableHeaderQvxFieldHeader() { Type = QvxFieldType.QVX_TEXT, ByteWidth = 4, ByteWidthSpecified = true, Extent = QvxFieldExtent.QVX_COUNTED, NullRepresentation = QvxNullRepresentation.QVX_NULL_FLAG_WITH_UNDEFINED_DATA, FieldFormat = new FieldAttributes() { Type = FieldAttrType.UNKNOWN } };
                
                // BLOB not working
                // if (mapType == typeof(Byte[])) return new QvxTableHeaderQvxFieldHeader() { Type = QvxFieldType.QVX_BLOB, ByteWidth = 1, ByteWidthSpecified = true, Extent = QvxFieldExtent.QVX_COUNTED, NullRepresentation = QvxNullRepresentation.QVX_NULL_FLAG_WITH_UNDEFINED_DATA, FieldFormat = new FieldAttributes() { Type = FieldAttrType.UNKNOWN } };

                if (mapType == typeof(DateTime)) return new QvxTableHeaderQvxFieldHeader() { Type = QvxFieldType.QVX_IEEE_REAL, ByteWidth = 8, ByteWidthSpecified = true, Extent = QvxFieldExtent.QVX_FIX, NullRepresentation = QvxNullRepresentation.QVX_NULL_NEVER, FieldFormat = new FieldAttributes() { Type = FieldAttrType.TIMESTAMP, nDec = 0 } };

                return new QvxTableHeaderQvxFieldHeader() { Type = QvxFieldType.QVX_TEXT, ByteWidth = 4, ByteWidthSpecified = true, NullRepresentation = QvxNullRepresentation.QVX_NULL_ZERO_LENGTH, Extent = QvxFieldExtent.QVX_COUNTED, FieldFormat = new FieldAttributes() { Type = FieldAttrType.UNKNOWN } };
            }
        }
        #endregion

        private static void QvxFieldHeaderFromObject(ref List<QvxTableHeaderQvxFieldHeader> result, string prefix, ref string code1, ref string code2, Type t, ref int? blocksize, Dictionary<string, List<QvxBaseAttribute>> overrideAttributes)
        {
            string intprefix = prefix.Replace(".", "__");

            if (result == null)
                result = new List<QvxTableHeaderQvxFieldHeader>();

            if (result == null)
                throw new ArgumentNullException("result");

            var valueList = new List<Tuple<Type, string, List<Attribute>, bool>>();

            foreach (FieldInfo item in t.GetFields())
            {
                code1 += "            MethodInfo " + intprefix + item.Name + " = T_" + intprefix + ".GetField(\"" + item.Name + "\").GetGetMethod();\r\n";                

                valueList.Add(
                    new Tuple<Type, string, List<Attribute>, bool>(
                        item.FieldType,
                        item.Name,
                        (from c in item.GetCustomAttributes(true) select c as Attribute).ToList(),
                        false));
            }

            foreach (PropertyInfo item in t.GetProperties())
            {
                code1 += "            MethodInfo " + intprefix + item.Name + " = T_" + intprefix + ".GetProperty(\"" + item.Name + "\").GetGetMethod();\r\n";

                if (item.CanRead)
                {
                    valueList.Add(
                       new Tuple<Type, string, List<Attribute>, bool>(
                           item.PropertyType,
                           item.Name,
                           (from c in item.GetCustomAttributes(true) select c as Attribute).ToList(),
                           true));
                }
            }

            // Add designtime QvxAttributes
            if (overrideAttributes != null)
            {
                foreach (var item in valueList)
                {
                    var localname = intprefix.Replace("base__", "") + item.Item2;
                    if (overrideAttributes.ContainsKey(localname))
                    {
                        var extAttrList = overrideAttributes[localname];
                        if (extAttrList != null)
                            item.Item3.AddRange(extAttrList);
                    }
                }
            }

            foreach (var item in valueList)
            {
                if (item.Item3.Contains(QvxIgnoreAttribute.Yes)) continue;

                string getValue = intprefix + item.Item2 + ".Invoke(" + intprefix + "item," + (item.Item4 ? "null" : "new object[] {" + intprefix + "item}") + ")";

                if (!(item.Item1.Namespace != null && item.Item1.Namespace.StartsWith("System")) && !item.Item1.IsEnum && !item.Item3.Contains(QvxSubClassAsStringAttribute.Yes))
                {
                    code1 += "            Type T_" + intprefix + item.Item2 + "__ = " + intprefix + item.Item2 + ".FieldType;\r\n";
                    code2 = "                var " + intprefix + item.Item2 + "__item = " + getValue + ";\r\n" + code2;
                    QvxFieldHeaderFromObject(ref result, prefix + item.Item2 + ".", ref  code1, ref code2, item.Item1, ref blocksize, overrideAttributes);
                }
                else
                {
                    var subfieldAtt = (from c in item.Item3 where c.GetType().IsAssignableFrom(typeof(QvxSubfieldAttribute)) select c).FirstOrDefault() as QvxSubfieldAttribute;


                    var enumerable = (from c in item.Item1.GetInterfaces() where c.Name == typeof(IEnumerable<>).Name select c).FirstOrDefault();

                    QvxTableHeaderQvxFieldHeader fieldHeader = null;

                    if (subfieldAtt != null && enumerable != null)
                    {                                                                       
                        #region Subfield Types
                        fieldHeader = NetType2QvxType.MapFieldType(typeof(string));
                        string subfieldLen = subfieldAtt.SubFieldDevider.Length.ToString();

                        code2 += "                tmpValue = " + getValue + ";\r\n";
                        code2 += "                sb = new StringBuilder();\r\n";
                        code2 += "                foreach (var item in (IEnumerable<object>)tmpValue)\r\n";
                        code2 += "                    sb.Append(item.ToString()+\"" + subfieldAtt.SubFieldDevider + "\");\r\n";
                        code2 += "                if (sb.Length > 0) sb.Remove(sb.Length-" + subfieldLen + "," + subfieldLen + ");\r\n";
                        code2 += "                sbuf =Encoding.UTF8.GetBytes(sb.ToString());\r\n";
                        code2 += "                bw.Write((Int32)sbuf.Length);\r\n";
                        code2 += "                bw.Write(sbuf);\r\n";
                        #endregion
                    }
                    else
                    {
                        #region Normal Types
                        Type nullAbleType = Nullable.GetUnderlyingType(item.Item1);
                        Type type = nullAbleType ?? item.Item1;

                        string stype = "";
                        fieldHeader = NetType2QvxType.MapFieldType(type);
                        if (NetType2QvxType.BinaryWriterTypes.Contains(type))
                            stype = type.Name;

                        // TODO: subfield for Arraytypes

                        // TODO: second QVX for nested Tables                 

                        if ((nullAbleType != null) | stype == "")
                        {
                            fieldHeader.NullRepresentation = QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA;

                            code2 += "                tmpValue = " + getValue + ";\r\n";
                            code2 += "                if (tmpValue == null)\r\n";
                            code2 += "                  bw.Write((byte)1);\r\n";
                            code2 += "                else\r\n";
                            code2 += "                {\r\n";
                            code2 += "                  bw.Write((byte)0);\r\n";

                            if (stype != "")
                            {
                                code2 += "                  bw.Write((tmpValue as Nullable<" + stype + ">).Value);\r\n";
                            }
                            else
                            {
                                if (type == typeof(byte[]))
                                    code2 += "                  sbuf =(byte[])tmpValue;\r\n";
                                else
                                    if (type == typeof(Char[]))
                                        code2 += "                  sbuf =Encoding.UTF8.GetBytes((Char[])tmpValue);\r\n";
                                    else
                                        code2 += "                  sbuf =Encoding.UTF8.GetBytes(tmpValue.ToString());\r\n";
                                code2 += "                  bw.Write((Int32)sbuf.Length);\r\n";
                                code2 += "                  bw.Write(sbuf);\r\n";
                            }
                            code2 += "                }\r\n";
                        }
                        else
                        {
                            if (type != typeof(Char))
                            {
                                var functioncall = "";
                                if (type == typeof(DateTime))
                                    functioncall = ".ToOADate()";
                                code2 += "                bw.Write(((" + stype + ")" + getValue + ")" + functioncall + ");\r\n";
                            }
                            else
                            {
                                code2 += "                sbuf =Encoding.UTF8.GetBytes(" + getValue + "+\"\");\r\n";
                                code2 += "                bw.Write((Int32)sbuf.Length);\r\n";
                                code2 += "                bw.Write(sbuf);\r\n";
                            }
                        } 
                        #endregion
                    }
                       

                    // TODO: block write Qvx (check fixed size,...)                    

                    QvxMaxLengthAttribute MaxSizeAttribute = null;
                    foreach (var itemAttribute in item.Item3)
                        if (itemAttribute is QvxMaxLengthAttribute)
                            MaxSizeAttribute = itemAttribute as QvxMaxLengthAttribute;

                    if (blocksize != null)
                        if (fieldHeader.Extent == QvxFieldExtent.QVX_FIX | MaxSizeAttribute != null)
                        {
                            if (fieldHeader.NullRepresentation == QvxNullRepresentation.QVX_NULL_FLAG_WITH_UNDEFINED_DATA | fieldHeader.NullRepresentation == QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA)
                                blocksize += 1;
                            if (fieldHeader.Extent == QvxFieldExtent.QVX_FIX | fieldHeader.Extent == QvxFieldExtent.QVX_COUNTED)
                                blocksize += fieldHeader.ByteWidth;
                            if (fieldHeader.Extent != QvxFieldExtent.QVX_FIX)
                                blocksize += MaxSizeAttribute.MaxLength;
                        }
                        else blocksize = null;

                    fieldHeader.FieldFormat.ApplyCustomAttributes(item.Item3);

                    fieldHeader.FieldName = prefix + item.Item2;

                    result.Add(fieldHeader);
                }
            }
        }

        public static Tuple<List<QvxTableHeaderQvxFieldHeader>, string, string> FromObjectCode(Type t, Dictionary<string, List<QvxBaseAttribute>> overrideAttributes)
        {
            var result = new List<QvxTableHeaderQvxFieldHeader>();
            result = null;
            string code1 = "";
            string code2 = "";
            int? blocksize = 0;
            QvxFieldHeaderFromObject(ref result, "base.", ref code1, ref code2, t, ref blocksize, overrideAttributes);
            foreach (var item in result)
            {
                item.FieldName = item.FieldName.Substring(5);
            }
            if (blocksize.HasValue)
            {
                // TODO: blocksize
                //code1 = code1 + "              byte[] diffBuf = new Byte[" + blocksize.Value.ToString() + "];\r\n";
                //code2 = "                long startPos = bw.BaseStream.Position;\r\n" + code2;
                //code2 = code2 + "                ";
            }

            return new Tuple<List<QvxTableHeaderQvxFieldHeader>, string, string>(result, code1, code2);
        }
    }    
    #endregion

    #region QvxWriteCode
    internal class QvxWriteCode
    {
        private byte[] QvxHeader;
        private MethodInfo QvxWriterMethod;

        private QvxTableHeader qvxTableHeader = null;
        internal QvxTableHeader QvxTableHeader
        {
            get
            {
                if (qvxTableHeader != null)
                    // make a copy
                    return QvxTableHeader.Deserialize(qvxTableHeader.Serialize());
                else
                    return new QvxTableHeader(); ;
            }
        }

        public void WriteHeader(BinaryWriter bw)
        {
            bw.Write(QvxHeader);
            bw.Write((byte)0x00);
        }

        public void WriteData(object item, BinaryWriter bw)
        {
            WriteData(new List<object>() { item }, bw);
        }

        public void WriteData(IEnumerable items, BinaryWriter bw)
        {
            QvxWriterMethod.Invoke(null, new object[] { bw, items });
        }

        public QvxWriteCode(Type type, Dictionary<string, List<QvxBaseAttribute>> overrideAttributes)
        {
            var data = QvxData.FromObjectCode(type, overrideAttributes);
            qvxTableHeader = new QvxTableHeader();
            qvxTableHeader.TableName = type.Name;
            qvxTableHeader.Fields.AddRange(data.Item1);
            QvxHeader = Encoding.UTF8.GetBytes(qvxTableHeader.Serialize());

            

            #region Code Template
            string code = @"
namespace QvxLib { 
    using System; 
    using System.Collections.Generic;
    using System.Linq;
    using System.Text; 
    using System.Reflection;
    using System.Reflection.Emit;
    using System.IO; 


    public static class FieldInfoExtensions
    {
        public static DynamicMethod GetGetMethod(this FieldInfo fi)
        {       
            DynamicMethod dm = new DynamicMethod(""Get"" + fi.Name,
                fi.FieldType, new Type[] { fi.DeclaringType }, fi.DeclaringType);
            ILGenerator il = dm.GetILGenerator();          
            il.Emit(OpCodes.Ldarg_0);            
            il.Emit(OpCodes.Ldfld, fi);            
            il.Emit(OpCodes.Ret);
            return dm;
        }
    }

    public class QvxWriterClass 
    { 
        public static void QvxWriter(BinaryWriter bw, IEnumerable<object> list) 
        {           
            object tmpValue = null;    
            byte[] sbuf = null; 
            StringBuilder sb = null;    
                          
            Type T_base__ = list.GetType().GetInterface(typeof(IEnumerable<>).Name).GetGenericArguments()[0];
" + data.Item2 + @"                        
            foreach (var base__item in list) 
            {
" + data.Item3 + @"
            }
        }
    }
}";
            #endregion

            #region Compiler Paramater
            CompilerParameters compilerParams = new CompilerParameters()
            {
                CompilerOptions = "/target:library /optimize /define:RELEASE",
                GenerateExecutable = false,
                WarningLevel = 3,
                GenerateInMemory = true,
                IncludeDebugInformation = false,
                ReferencedAssemblies = { "mscorlib.dll", "System.dll", "System.Core.dll" }
            };
#if DEBUG
            compilerParams.CompilerOptions = "/target:library /define:DEBUG";
            compilerParams.GenerateInMemory = false;
            compilerParams.IncludeDebugInformation = true;
            compilerParams.TempFiles.KeepFiles = true;
            compilerParams.TempFiles = new TempFileCollection(Environment.GetEnvironmentVariable("TEMP"), true);
#endif
            #endregion

            var provOptions = new Dictionary<string, string>() { { "CompilerVersion", "v3.5" } };

            CodeDomProvider codeProvider = new CSharpCodeProvider(provOptions);
            CompilerResults results = codeProvider.CompileAssemblyFromSource(compilerParams, code);

            if (!results.Errors.HasErrors)
            {
                var QvxWriterClassType = results.CompiledAssembly.GetType("QvxLib.QvxWriterClass");
                QvxWriterMethod = QvxWriterClassType.GetMethod("QvxWriter", BindingFlags.Static | BindingFlags.Public);
            }
            else
                throw new Exception("Compilation Error" + results.Output.ToString());
        }
    } 
    #endregion

    #region QvxWriter
    public class QvxWriter
    {
        #region Variables
        BinaryWriter bw;
        QvxWriteCode qvxWriteCode;
        #endregion

        #region Construtors
        internal QvxWriter(QvxWriteCode qvxWriteCode, BinaryWriter bw)
        {
            if (bw == null | qvxWriteCode == null)
                throw new ArgumentNullException();

            this.qvxWriteCode = qvxWriteCode;
            this.bw = bw;

            bw.BaseStream.Seek(0, SeekOrigin.Begin);
            qvxWriteCode.WriteHeader(bw);
        }

        public QvxWriter(Type type, Dictionary<string, List<QvxBaseAttribute>> overrideAttributes, BinaryWriter bw) :
            this(new QvxWriteCode(type,overrideAttributes), bw)
        {
        }

        public QvxWriter(Type type, BinaryWriter bw) : this(type, null, bw) { }

        public QvxWriter(Type type, Dictionary<string, List<QvxBaseAttribute>> overrideAttributes, String FileName)
            : this(type, overrideAttributes, new BinaryWriter(File.OpenWrite(FileName)))
        {
        }

        public QvxWriter(Type type, String FileName) : this(type, null, FileName) { }
        #endregion

        #region Add Date
        public void Add(object item)
        {
            qvxWriteCode.WriteData(item, bw);
        }

        public void AddRange(IEnumerable items)
        {
            qvxWriteCode.WriteData(items, bw);
        }
        #endregion

        #region Close
        public void Close()
        {
            bw.Close();
        }
        #endregion
    } 
    #endregion

    #region QvxSerializer
    public class QvxSerializer
    {
        QvxWriteCode qvxWriteCode;

        public QvxTableHeader QvxTableHeader
        {
            get
            {
                if (qvxWriteCode != null)
                    return qvxWriteCode.QvxTableHeader;
                else
                    return new QvxTableHeader();
            }
        }

        public QvxSerializer(Type type)
            : this(type, null)
        {
        }

        public QvxSerializer(Type type, Dictionary<string, List<QvxBaseAttribute>> overrideAttributes)
        {
            qvxWriteCode = new QvxWriteCode(type, overrideAttributes);
        }

        public void Serialize(IEnumerable items, BinaryWriter bw)
        {
            if (bw == null | items == null)
                throw new ArgumentNullException();

            qvxWriteCode.WriteHeader(bw);
           
            qvxWriteCode.WriteData(items, bw);
            bw.Close();
        }

        public void Serialize(IEnumerable items, String FileName)
        {
            Serialize(items, new BinaryWriter(File.OpenWrite(FileName)));
        }

    } 
    #endregion

    #region QvxSerializer<T>
    public class QvxSerializer<T>
    {
        QvxSerializer qvxSerializer;

        public QvxTableHeader QvxTableHeader
        {
            get
            {
                if (qvxSerializer != null)
                    return qvxSerializer.QvxTableHeader;
                else
                    return new QvxTableHeader();
            }
        }

        public QvxSerializer() : this(null) { }
       
        public QvxSerializer(Dictionary<string,List<QvxBaseAttribute>> overrideAttributes)
        {
              qvxSerializer = new QvxSerializer(typeof(T),overrideAttributes);
        }

        public void Serialize(IEnumerable<T> items, BinaryWriter bw)
        {
            qvxSerializer.Serialize(items, bw);
        }

        public void Serialize(IEnumerable<T> items, String FileName)
        {
            qvxSerializer.Serialize(items, FileName);
        }
    } 
    #endregion
}
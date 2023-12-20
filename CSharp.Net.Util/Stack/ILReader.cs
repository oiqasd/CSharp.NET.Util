using System;
using System.Reflection;
using System.Reflection.Emit;

namespace CSharp.Net.Util.Stack
{
    /// <summary>
    /// from Ben.Demystifier
    /// </summary>
    internal class ILReader
    {
        private static OpCode[] singleByteOpCode;

        private static OpCode[] doubleByteOpCode;

        private readonly byte[] _cil;

        private int ptr;

        public OpCode OpCode { get; private set; }

        public int MetadataToken { get; private set; }

        public MemberInfo? Operand { get; private set; }

        public ILReader(byte[] cil)
        {
            _cil = cil;
        }

        public bool Read(MethodBase methodInfo)
        {
            if (ptr < _cil.Length)
            {
                OpCode = ReadOpCode();
                Operand = ReadOperand(OpCode, methodInfo);
                return true;
            }

            return false;
        }

        private OpCode ReadOpCode()
        {
            byte b = ReadByte();
            if (b < 254)
            {
                return singleByteOpCode[b];
            }

            return doubleByteOpCode[ReadByte()];
        }

        private MemberInfo? ReadOperand(OpCode code, MethodBase methodInfo)
        {
            MetadataToken = 0;
            int num;
            switch (code.OperandType)
            {
                case OperandType.InlineMethod:
                    {
                        MetadataToken = ReadInt();
                        Type[] genericMethodArguments = null;
                        if (methodInfo.GetType() != typeof(ConstructorInfo) && !methodInfo.GetType().IsSubclassOf(typeof(ConstructorInfo)))
                        {
                            genericMethodArguments = methodInfo.GetGenericArguments();
                        }

                        Type[] genericTypeArguments = null;
                        if (methodInfo.DeclaringType != null)
                        {
                            genericTypeArguments = methodInfo.DeclaringType!.GetGenericArguments();
                        }

                        try
                        {
                            return methodInfo.Module.ResolveMember(MetadataToken, genericTypeArguments, genericMethodArguments);
                        }
                        catch
                        {
                            return null;
                        }
                    }
                case OperandType.InlineNone:
                    num = 0;
                    break;
                case OperandType.ShortInlineBrTarget:
                case OperandType.ShortInlineI:
                case OperandType.ShortInlineVar:
                    num = 1;
                    break;
                case OperandType.InlineVar:
                    num = 2;
                    break;
                case OperandType.InlineBrTarget:
                case OperandType.InlineField:
                case OperandType.InlineI:
                case OperandType.InlineSig:
                case OperandType.InlineString:
                case OperandType.InlineSwitch:
                case OperandType.InlineTok:
                case OperandType.InlineType:
                case OperandType.ShortInlineR:
                    num = 4;
                    break;
                case OperandType.InlineI8:
                case OperandType.InlineR:
                    num = 8;
                    break;
                default:
                    return null;
            }

            for (int i = 0; i < num; i++)
            {
                ReadByte();
            }

            return null;
        }

        private byte ReadByte()
        {
            return _cil[ptr++];
        }

        private int ReadInt()
        {
            byte num = ReadByte();
            byte b = ReadByte();
            byte b2 = ReadByte();
            byte b3 = ReadByte();
            return num | (b << 8) | (b2 << 16) | (b3 << 24);
        }

        static ILReader()
        {
            singleByteOpCode = new OpCode[225];
            doubleByteOpCode = new OpCode[31];
            FieldInfo[] opCodeFields = GetOpCodeFields();
            for (int i = 0; i < opCodeFields.Length; i++)
            {
                OpCode opCode = (OpCode)opCodeFields[i].GetValue(null);
                if (opCode.OpCodeType != OpCodeType.Nternal)
                {
                    if (opCode.Size == 1)
                    {
                        singleByteOpCode[opCode.Value] = opCode;
                    }
                    else
                    {
                        doubleByteOpCode[opCode.Value & 0xFF] = opCode;
                    }
                }
            }
        }

        private static FieldInfo[] GetOpCodeFields()
        {
            return typeof(OpCodes)!.GetFields(BindingFlags.Static | BindingFlags.Public);
        }
    }
}

using System.Diagnostics;
using System.Reflection;

namespace CSharp.Net.Util.Stack
{

    /// <summary>
    /// from Ben.Demystifier
    /// </summary>
    public class EnhancedStackFrame : StackFrame
    {
        private readonly string _fileName;

        private readonly int _lineNumber;

        private readonly int _colNumber;

        public StackFrame StackFrame { get; }

        public bool IsRecursive
        {
            get
            {
                return MethodInfo.RecurseCount > 0;
            }
            internal set
            {
                MethodInfo.RecurseCount++;
            }
        }

        public ResolvedMethod MethodInfo { get; }

        internal EnhancedStackFrame(StackFrame stackFrame, ResolvedMethod methodInfo, string fileName, int lineNumber, int colNumber)
            : base(fileName, lineNumber, colNumber)
        {
            StackFrame = stackFrame;
            MethodInfo = methodInfo;
            _fileName = fileName;
            _lineNumber = lineNumber;
            _colNumber = colNumber;
        }

        internal bool IsEquivalent(ResolvedMethod methodInfo, string fileName, int lineNumber, int colNumber)
        {
            if (_lineNumber == lineNumber && _colNumber == colNumber && _fileName == fileName)
            {
                return MethodInfo.IsSequentialEquivalent(methodInfo);
            }

            return false;
        }

        public override int GetFileColumnNumber()
        {
            return _colNumber;
        }

        public override int GetFileLineNumber()
        {
            return _lineNumber;
        }

        public override string GetFileName()
        {
            return _fileName;
        }

        public override int GetILOffset()
        {
            return StackFrame.GetILOffset();
        }

        public override MethodBase GetMethod()
        {
            return StackFrame.GetMethod();
        }

        public override int GetNativeOffset()
        {
            return StackFrame.GetNativeOffset();
        }

        public override string ToString()
        {
            return MethodInfo.ToString();
        }
    }
}
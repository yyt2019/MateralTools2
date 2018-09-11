﻿using MateralTools.Base;
using System;
using MateralTools.Base.Model;

namespace MateralTools.MResult
{
    public class MResultException : MException
    {
        public MResultException() : base() { }
        public MResultException(string message) : base(message) { }
        public MResultException(string message, Exception innerException) : base(message, innerException) { }
    }
}

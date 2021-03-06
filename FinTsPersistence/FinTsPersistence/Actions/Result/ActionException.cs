﻿using System;

namespace FinTsPersistence.Actions.Result
{
    public class ActionException : Exception
    {
        public ActionException(Exception innerException, string trace)
            : base("An unexpected exceptio orrured while executing the action.", innerException)
        {
            FinTsTrace = trace;
        }

        public string FinTsTrace { get; private set; }
    }
}

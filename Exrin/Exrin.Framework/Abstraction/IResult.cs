﻿namespace Exrin.Abstraction
{
    public interface IResult
    {
        /// <summary>
        /// The resulting action of the execute
        /// </summary>
        ResultType ResultAction { get; set; }
        /// <summary>
        /// Arguments that may be required to pass to the next operation
        /// Or for the final handling of the final rezult
        /// </summary>
        IResultArgs Arguments { get; set; }
        
    }
}

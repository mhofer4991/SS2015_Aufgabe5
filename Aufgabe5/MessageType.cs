//-----------------------------------------------------------------------
// <copyright file="MessageType.cs" company="Markus Hofer">
//     Copyright (c) Markus Hofer
// </copyright>
// <summary>This enumeration contains different types of messages.</summary>
//-----------------------------------------------------------------------
namespace Aufgabe5
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// This enumeration contains different types of messages.
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// Indicates that the client wants to transfer records to the server.
        /// </summary>
        TransferRecords = 1,

        /// <summary>
        /// Indicates that the transferred record has been rejected by the server.
        /// </summary>
        RecordRejected = 2,

        /// <summary>
        /// Indicates that the transferred record has been accepted by the server.
        /// </summary>
        RecordTransferred = 3,

        /// <summary>
        /// Indicates that the message type is not known.
        /// </summary>
        Unknown
    }
}

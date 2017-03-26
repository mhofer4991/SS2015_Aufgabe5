//-----------------------------------------------------------------------
// <copyright file="Protocol.cs" company="Markus Hofer">
//     Copyright (c) Markus Hofer
// </copyright>
// <summary>This class sets rules for transferring records from client to server.</summary>
//-----------------------------------------------------------------------
namespace Aufgabe5
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// This class sets rules for transferring records from client to server.
    /// </summary>
    public static class Protocol
    {
        /// <summary>
        /// Gets the default port for the receiver server.
        /// </summary>
        public const int DefaultServerPort = 1234;

        /// <summary>
        /// This enumeration contains different reasons for a record rejection.
        /// </summary>
        public enum RejectionReason
        {
            /// <summary>
            /// Indicates that the record has been rejected due to duplicate IDs.
            /// </summary>
            RejectionDueToDuplicateID = 1,

            /// <summary>
            /// Indicates that the record has been rejected due to duplicate timestamps.
            /// </summary>
            RejectionDueToDuplicateTimestamp = 2
        }

        /// <summary>
        /// This method extracts the message type from a byte array.
        /// </summary>
        /// <param name="data">The byte array.</param>
        /// <returns>The extracted message type.</returns>
        public static MessageType GetMessageType(byte[] data)
        {
            if (data.Length != 1)
            {
                return MessageType.Unknown;
            }
            else
            {
                switch (data[0])
                {
                    case (byte)MessageType.TransferRecords:
                        return MessageType.TransferRecords;
                    case (byte)MessageType.RecordRejected:
                        return MessageType.RecordRejected;
                    case (byte)MessageType.RecordTransferred:
                        return MessageType.RecordTransferred;
                    default:
                        return MessageType.Unknown;
                }
            }
        }

        /// <summary>
        /// Gets the request message for transferring records as a byte array.
        /// </summary>
        /// <param name="amountRecords">The amount of records, which will be sent.</param>
        /// <returns>A message represented by a byte array.</returns>
        public static byte[] GetAnnouncementForTransfer(int amountRecords)
        {
            List<byte> message = new List<byte>();

            byte[] msgType = new byte[1] { (int)MessageType.TransferRecords };
            byte[] amount = BitConverter.GetBytes((ushort)amountRecords);

            message.AddRange(msgType);
            message.AddRange(amount);

            return message.ToArray();
        }

        /// <summary>
        /// Gets the response message for a rejected record and the given reason of its rejection.
        /// </summary>
        /// <param name="record">The rejected record.</param>
        /// <param name="reason">The given reason.</param>
        /// <returns>A response message represented by a byte array.</returns>
        public static byte[] GetResponseForRejectingRecord(Record record, RejectionReason reason)
        {
            List<byte> message = new List<byte>();

            byte[] msgType = new byte[1] { (int)MessageType.RecordRejected };
            byte[] recordID = BitConverter.GetBytes(record.ID);

            message.AddRange(msgType);
            message.AddRange(recordID);
            message.Add((byte)reason);

            return message.ToArray();
        }

        /// <summary>
        /// Gets the response message for a accepted record.
        /// </summary>
        /// <param name="record">The accepted record.</param>
        /// <returns>A response message represented by a byte array.</returns>
        public static byte[] GetResponseForAcceptingRecord(Record record)
        {
            List<byte> message = new List<byte>();

            byte[] msgType = new byte[1] { (int)MessageType.RecordTransferred };
            byte[] recordID = BitConverter.GetBytes(record.ID);

            message.AddRange(msgType);
            message.AddRange(recordID);

            return message.ToArray();
        }

        /// <summary>
        /// Converts a given record to a byte array, which then can be sent over a network.
        /// </summary>
        /// <param name="record">The given record.</param>
        /// <param name="cells">The amount of cells, which have been modified in this record.</param>
        /// <returns>A byte array of a record.</returns>
        public static byte[] RecordToByteArray(Record record, int cells)
        {
            List<byte> message = new List<byte>();

            message.AddRange(BitConverter.GetBytes(record.ID));

            message.AddRange(BitConverter.GetBytes((ushort)record.Timestamp.Year));

            message.Add((byte)record.Timestamp.Month);
            message.Add((byte)record.Timestamp.Day);

            message.Add((byte)record.Timestamp.Hour);
            message.Add((byte)record.Timestamp.Minute);
            message.Add((byte)record.Timestamp.Second);

            message.AddRange(BitConverter.GetBytes(record.Duration));

            message.AddRange(BitConverter.GetBytes((ushort)cells));

            return message.ToArray();
        }

        /// <summary>
        /// Converts a given cell to a byte array, which then can be sent over a network.
        /// </summary>
        /// <param name="cell">The given cell.</param>
        /// <returns>A byte array of a cell.</returns>
        public static byte[] CellToByteArray(Cell cell)
        {
            List<byte> message = new List<byte>();

            message.AddRange(BitConverter.GetBytes((ushort)cell.X));
            message.AddRange(BitConverter.GetBytes((ushort)cell.Y));
            message.AddRange(BitConverter.GetBytes(cell.Sign));
            message.Add((byte)cell.ForeColor);
            message.Add((byte)cell.BackColor);

            return message.ToArray();
        }

        /// <summary>
        /// Converts a given byte array to a record.
        /// </summary>
        /// <param name="arr">The given byte array.</param>
        /// <param name="nonEmptyCells">The amount of cells, which have been modified in this record.</param>
        /// <returns>A record from a byte array.</returns>
        public static Record RecordFromByteArray(byte[] arr, ref int nonEmptyCells)
        {
            int id = BitConverter.ToInt32(new byte[] { arr[0], arr[1], arr[2], arr[3] }, 0);
            DateTime dt = new DateTime(BitConverter.ToInt16(new byte[] { arr[4], arr[5] }, 0), arr[6], arr[7], arr[8], arr[9], arr[10]);
            Cell[,] data = new Cell[Record.DefaultColumns, Record.DefaultRows];

            Record.ClearPagedata(data);

            Record r = new Record(id, dt, BitConverter.ToInt32(new byte[] { arr[11], arr[12], arr[13], arr[14] }, 0), data);

            nonEmptyCells = BitConverter.ToUInt16(new byte[] { arr[15], arr[16] }, 0);

            return r;
        }

        /// <summary>
        /// Converts a given byte array to a cell.
        /// </summary>
        /// <param name="arr">The given byte array.</param>
        /// <returns>A cell from a byte array.</returns>
        public static Cell CellFromByteArray(byte[] arr)
        {
            int x = BitConverter.ToUInt16(new byte[] { arr[0], arr[1] }, 0);
            int y = BitConverter.ToUInt16(new byte[] { arr[2], arr[3] }, 0);
            char sign = BitConverter.ToChar(new byte[] { arr[4], arr[5] }, 0);

            return new Cell(x, y, sign, (ConsoleColor)arr[6], (ConsoleColor)arr[7]);
        }

        /// <summary>
        /// Reads from a network stream and returns the message type.
        /// </summary>
        /// <param name="stream">The network stream, which holds data.</param>
        /// <returns>The message type.</returns>
        public static MessageType GetMessageTypeFromStream(NetworkStream stream)
        {
            byte[] buff = new byte[1];

            if (stream.Read(buff, 0, buff.Length) == buff.Length)
            {
                return Protocol.GetMessageType(buff);
            }
            else
            {
                return MessageType.Unknown;
            }
        }

        /// <summary>
        /// Reads from a network stream and returns the amount of transferred records.
        /// </summary>
        /// <param name="stream">The network stream, which holds data.</param>
        /// <returns>The amount of transferred records.</returns>
        public static int GetAmountOfRecordsFromStream(NetworkStream stream)
        {
            byte[] buff = new byte[2];

            if (stream.Read(buff, 0, buff.Length) == buff.Length)
            {
                return BitConverter.ToUInt16(buff, 0);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Reads from a network stream and returns a record plus the amount of transferred, modified cells.
        /// </summary>
        /// <param name="stream">The network stream, which holds data.</param>
        /// <param name="amountOfCells">The amount of transferred, modified cells.</param>
        /// <returns>A new record.</returns>
        public static Record GetRecordFromStream(NetworkStream stream, ref int amountOfCells)
        {
            byte[] buff = new byte[17];

            if (stream.Read(buff, 0, buff.Length) == buff.Length)
            {
                return Protocol.RecordFromByteArray(buff, ref amountOfCells);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Reads from a network stream and returns a new cell.
        /// </summary>
        /// <param name="stream">The network stream, which holds data.</param>
        /// <returns>A new cell.</returns>
        public static Cell GetCellFromStream(NetworkStream stream)
        {
            byte[] buff = new byte[8];

            if (stream.Read(buff, 0, buff.Length) == buff.Length)
            {
                return Protocol.CellFromByteArray(buff);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Reads from a network stream and returns the reason for the rejection of a record.
        /// </summary>
        /// <param name="stream">The network stream, which holds data.</param>
        /// <returns>The reason for the rejection of a record.</returns>
        public static RejectionReason GetRejectionReasonFromStream(NetworkStream stream)
        {
            byte[] buff = new byte[1];

            stream.Read(buff, 0, buff.Length);

            return (Protocol.RejectionReason)buff[0];
        }
    }
}

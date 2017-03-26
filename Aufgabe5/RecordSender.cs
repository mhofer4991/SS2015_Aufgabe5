//-----------------------------------------------------------------------
// <copyright file="RecordSender.cs" company="Markus Hofer">
//     Copyright (c) Markus Hofer
// </copyright>
// <summary>This class connects to a server and transmits data pages.</summary>
//-----------------------------------------------------------------------
namespace Aufgabe5
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// This class connects to a server and transmits data pages.
    /// </summary>
    public class RecordSender
    {
        /// <summary>
        /// The IP address of the server.
        /// </summary>
        private IPAddress ip;

        /// <summary>
        /// The port of the server.
        /// </summary>
        private int port;

        /// <summary>
        /// An instance of the TCP client.
        /// </summary>
        private TcpClient client;

        /// <summary>
        /// The network stream of the TCP client.
        /// </summary>
        private NetworkStream stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecordSender"/> class.
        /// </summary>
        /// <param name="address">IP address of the server.</param>
        /// <param name="port">Port of the server.</param>
        public RecordSender(string address, string port)
        {
            this.ip = IPAddress.Parse(address);
            this.port = int.Parse(port);
        }

        /// <summary>
        /// Delegate for event OnRecordRejectedDueToDuplicateIDs.
        /// </summary>
        /// <param name="record">ID of the rejected record.</param>
        public delegate void RecordRejectedDueToDuplicateIDs(Record record);

        /// <summary>
        /// Delegate for event OnRecordRejectedDueToDuplicateTimestamps.
        /// </summary>
        /// <param name="record">Record with duplicate timestamp.</param>
        public delegate void RecordRejectedDueToDuplicateTimestamps(Record record);

        /// <summary>
        /// Delegate for event OnRecordAccepted.
        /// </summary>
        /// <param name="record">ID of the accepted record.</param>
        public delegate void RecordAccepted(Record record);

        /// <summary>
        /// Delegate for event OnTransferFinished.
        /// </summary>
        /// <param name="unconfirmedRecords">The amount of unconfirmed records.</param>
        /// <param name="acceptedRecords">The amount of accepted records.</param>
        /// <param name="rejectedRecords">The amount of rejected records.</param>
        public delegate void TransferFinished(int unconfirmedRecords, int acceptedRecords, int rejectedRecords);

        /// <summary>
        /// Gets called when a record has been rejected by the server due to duplicate IDs.
        /// </summary>
        public event RecordRejectedDueToDuplicateIDs OnRecordRejectedDueToDuplicateIDs;

        /// <summary>
        /// Gets called when a record has been rejected by the server due to duplicate timestamps.
        /// </summary>
        public event RecordRejectedDueToDuplicateTimestamps OnRecordRejectedDueToDuplicateTimestamps;

        /// <summary>
        /// Gets called when a record has been accepted by the server.
        /// </summary>
        public event RecordAccepted OnRecordAccepted;

        /// <summary>
        /// Gets called when a transfer has been finished.
        /// </summary>
        public event TransferFinished OnTransferFinished;

        /// <summary>
        /// Checks if the given string is valid IP address.
        /// </summary>
        /// <param name="s">The given string.</param>
        /// <returns>A boolean indicating whether the given string is valid IP address or not.</returns>
        public static bool IsValidIPAddress(string s)
        {
            IPAddress temp;

            return IPAddress.TryParse(s, out temp);
        }

        /// <summary>
        /// Checks if the given string is valid port.
        /// </summary>
        /// <param name="s">The given string.</param>
        /// <returns>A boolean indicating whether the given string is valid port or not.</returns>
        public static bool IsValidPort(string s)
        {
            int port;

            return int.TryParse(s, out port) && port >= 0 && port <= 65535;
        }

        /// <summary>
        /// Connects to the server.
        /// </summary>
        /// <returns>A boolean indicating whether the connection was successful or not.</returns>
        public bool Connect()
        {
            this.client = new TcpClient();

            IPEndPoint serverEndPoint = new IPEndPoint(this.ip, this.port);

            try
            {
                this.client.Connect(serverEndPoint);
                this.stream = this.client.GetStream();

                return true;
            }
            catch (SocketException)
            {
                return false;
            }
        }

        /// <summary>
        /// Sends the given list of records to the connected server.
        /// </summary>
        /// <param name="records">The given list of records.</param>
        public void SendRecords(List<Record> records)
        {
            // Send announcement before sending the records.
            byte[] request = Protocol.GetAnnouncementForTransfer(records.Count);

            this.stream.Write(request, 0, request.Length);
            this.stream.Flush();

            // Sending.
            for (int i = 0; i < records.Count; i++)
            {
                this.SendRecord(records[i]);
            }

            // Receive confirmations after sending.
            int amountAcknowledgements = 0;
            int accepted = 0;
            int rejected = 0;
            int attempts = 5; // Sender should not wait an endless time for acknowledgments from the server.

            // Waits for all acknowledgments from the server or stops if therer are no attempts left.
            while (amountAcknowledgements < records.Count && attempts > 0)
            {
                if (this.stream.DataAvailable)
                {
                    MessageType messageType = Protocol.GetMessageTypeFromStream(this.stream);

                    if (messageType == MessageType.RecordRejected ||
                        messageType == MessageType.RecordTransferred)
                    {
                        // Find related record.
                        byte[] buff = new byte[4];

                        this.stream.Read(buff, 0, buff.Length);

                        Record record = this.LookForID(records, BitConverter.ToInt32(buff, 0));

                        // Handle rejection and acception.
                        if (messageType == MessageType.RecordRejected)
                        {
                            rejected++;

                            // If rejected, a reason has also been sent.
                            this.HandleRejection(record, Protocol.GetRejectionReasonFromStream(this.stream));
                        }
                        else if (messageType == MessageType.RecordTransferred)
                        {
                            accepted++;
                            this.HandleAcception(record);
                        }

                        amountAcknowledgements++;

                        attempts = 5;
                    }
                }
                else
                {
                    attempts--;
                    Thread.Sleep(1000);
                }
            }

            if (this.OnTransferFinished != null)
            {
                this.OnTransferFinished(records.Count - amountAcknowledgements, accepted, rejected);
            }
        }

        /// <summary>
        /// Gets the record from the list which has the given ID.
        /// </summary>
        /// <param name="records">The list of records.</param>
        /// <param name="id">The given ID.</param>
        /// <returns>The record with the given ID.</returns>
        private Record LookForID(List<Record> records, int id)
        {
            for (int i = 0; i < records.Count; i++)
            {
                if (records[i].ID == id)
                {
                    return records[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Handles the rejected record by the given reason.
        /// </summary>
        /// <param name="record">The rejected record.</param>
        /// <param name="reason">The given reason.</param>
        private void HandleRejection(Record record, Protocol.RejectionReason reason)
        {
            if (reason == Protocol.RejectionReason.RejectionDueToDuplicateID)
            {
                if (this.OnRecordRejectedDueToDuplicateIDs != null)
                {
                    this.OnRecordRejectedDueToDuplicateIDs(record);
                }
            }
            else if (reason == Protocol.RejectionReason.RejectionDueToDuplicateTimestamp)
            {
                if (this.OnRecordRejectedDueToDuplicateTimestamps != null)
                {
                    this.OnRecordRejectedDueToDuplicateTimestamps(record);
                }
            }
        }

        /// <summary>
        /// Handles the accepted record.
        /// </summary>
        /// <param name="record">The accepted record.</param>
        private void HandleAcception(Record record)
        {
            if (this.OnRecordAccepted != null)
            {
                this.OnRecordAccepted(record);
            }
        }

        /// <summary>
        /// Sends the given record plus all its non empty cells of the page data.
        /// </summary>
        /// <param name="record">The given record.</param>
        private void SendRecord(Record record)
        {
            List<Cell> nonEmptyCells = new List<Cell>();

            for (int i = 0; i < record.Pagedata.GetUpperBound(0) + 1; i++)
            {
                for (int j = 0; j < record.Pagedata.GetUpperBound(1) + 1; j++)
                {
                    if (!record.Pagedata[i, j].IsEmpty)
                    {
                        nonEmptyCells.Add(record.Pagedata[i, j]);
                    }
                }
            }

            byte[] recordArray = Protocol.RecordToByteArray(record, nonEmptyCells.Count);

            this.stream.Write(recordArray, 0, recordArray.Length);

            this.stream.Flush();

            for (int i = 0; i < nonEmptyCells.Count; i++)
            {
                byte[] cellArray = Protocol.CellToByteArray(nonEmptyCells[i]);

                this.stream.Write(cellArray, 0, cellArray.Length);
            }

            this.stream.Flush();
        }
    }
}

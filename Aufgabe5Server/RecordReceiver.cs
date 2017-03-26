//-----------------------------------------------------------------------
// <copyright file="RecordReceiver.cs" company="Markus Hofer">
//     Copyright (c) Markus Hofer
// </copyright>
// <summary>This class receives data pages from clients and renders them on the console.</summary>
//-----------------------------------------------------------------------
namespace Aufgabe5Server
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Aufgabe5;

    /// <summary>
    /// This class receives data pages from clients and renders them on the console.
    /// </summary>
    public class RecordReceiver
    {
        /// <summary>
        /// Gets the default port of the server.
        /// </summary>
        public const int DefaultPort = Protocol.DefaultServerPort;

        /// <summary>
        /// The current port of the server.
        /// </summary>
        private int port;

        /// <summary>
        /// Gets a value indicating whether the server is listening or not.
        /// </summary>
        private bool listening;

        /// <summary>
        /// List of all records, which are currently saved by the program.
        /// </summary>
        private List<Record> allSavedRecords;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecordReceiver"/> class.
        /// </summary>
        /// <param name="currentRecords">List of all records, which are currently saved by the program.</param>
        public RecordReceiver(List<Record> currentRecords)
            : this(RecordReceiver.DefaultPort, currentRecords)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecordReceiver"/> class.
        /// </summary>
        /// <param name="port">Port of the server.</param>
        /// <param name="currentRecords">List of all records, which are currently saved by the program.</param>
        public RecordReceiver(int port, List<Record> currentRecords)
        {
            this.port = port;
            this.listening = false;
            this.allSavedRecords = currentRecords;
        }

        /// <summary>
        /// Delegate for event OnRecordsReceived.
        /// </summary>
        /// <param name="transferredRecords">All received records.</param>
        public delegate void RecordsReceived(List<Record> transferredRecords);

        /// <summary>
        /// Gets called when some records have been received.
        /// </summary>
        public event RecordsReceived OnRecordsReceived;

        /// <summary>
        /// Starts the server by listening on a specified port.
        /// </summary>
        public void Start()
        {
            this.listening = true;

            Thread thread = new Thread(new ThreadStart(this.Listen));
            thread.Start();
        }

        /// <summary>
        /// Listens on a specified port.
        /// </summary>
        private void Listen()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, this.port);
            listener.Start();

            while (this.listening)
            {
                TcpClient client = listener.AcceptTcpClient();

                Thread thread = new Thread(new ParameterizedThreadStart(this.HandleClient));
                thread.Start(client);
            }
        }

        /// <summary>
        /// Handles a client which has connected to the server.
        /// </summary>
        /// <param name="data">Data, which is the client.</param>
        private void HandleClient(object data)
        {
            TcpClient client = (TcpClient)data;
            NetworkStream stream = client.GetStream();

            while (true)
            {
                if (stream.DataAvailable)
                {
                    MessageType messageType = Protocol.GetMessageTypeFromStream(stream);
                    
                    if (messageType == MessageType.TransferRecords)
                    {
                        List<Record> records = this.ReceiveRecords(stream);

                        if (this.OnRecordsReceived != null)
                        {
                            this.OnRecordsReceived(records);
                        }
                    }
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
        }

        /// <summary>
        /// Receives all records over a given network stream.
        /// </summary>
        /// <param name="stream">The given network stream.</param>
        /// <returns>A list of all records.</returns>
        private List<Record> ReceiveRecords(NetworkStream stream)
        {
            int amount = Protocol.GetAmountOfRecordsFromStream(stream);

            List<Record> records = new List<Record>();

            for (int i = 0; i < amount; i++)
            {
                Record record = this.ReceiveRecord(stream);

                byte[] data;

                // Not sure, if all saved records or only the currently transferred records should be used for the check. The task wasn't clear enough about this.
                // Check if ID already exists on the server and if two received records have the same ID.
                if (records.Contains(record) || this.allSavedRecords.Contains(record))
                {
                    data = Protocol.GetResponseForRejectingRecord(record, Protocol.RejectionReason.RejectionDueToDuplicateID);
                }
                else if (record.DuplicateTimestampInList(records))
                {
                    // Two received records have the same timestamp.
                    data = Protocol.GetResponseForRejectingRecord(record, Protocol.RejectionReason.RejectionDueToDuplicateTimestamp);
                }
                else
                {
                    records.Add(record);
                    data = Protocol.GetResponseForAcceptingRecord(record);
                }

                stream.Write(data, 0, data.Length);
                stream.Flush();
            }

            return records;
        }

        /// <summary>
        /// Receives a record over a given network stream.
        /// </summary>
        /// <param name="stream">The given network stream.</param>
        /// <returns>A single record.</returns>
        private Record ReceiveRecord(NetworkStream stream)
        {
            int nonEmptyCells = 0;

            Record record = Protocol.GetRecordFromStream(stream, ref nonEmptyCells);

            for (int j = 0; j < nonEmptyCells; j++)
            {
                Cell c = Protocol.GetCellFromStream(stream);

                record.ModifyCell(c);
            }

            return record;
        }
    }
}
